using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Novati.API.Common.Extensions;
using Novati.API.Common.Middleware;
using Novati.API.Common.Settings;
using Novati.API.Data;
using Novati.API.Data.Seed;
using Novati.API.Realtime;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(o =>
    {
        o.InvalidModelStateResponseFactory = ctx =>
        {
        var message = string.Join(" ",
            ctx.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
        );

            return new BadRequestObjectResult(new
            {
                statusCode = 400,
                message
            });
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Novati API",
        Version = "v1",
        Description = "API do helpdesk de TI."
    });

    // XML
    string? xml = Path.Combine(
        AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"
    );

    if (File.Exists(xml))
        o.IncludeXmlComments(xml);

    // JWT
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = []
    });
});

// 5173 = vite dev · 4173 = vite preview · 5174 = front dos testes E2E (playwright.config.js)
string[] origensLocais = ["http://localhost:5173", "http://localhost:4173", "http://localhost:5174"];
// Produção: o endereço do front (Vercel) vem de Cors:Origins, separado por vírgulas se forem vários.
var origensExtra = (builder.Configuration["Cors:Origins"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(o => o.TrimEnd('/'));

builder.Services.AddCors(o => o.AddPolicy("front", p => p
.WithOrigins([.. origensLocais, .. origensExtra]).
AllowAnyHeader().
AllowAnyMethod().
AllowCredentials()));   // o cliente SignalR envia credenciais na negociação

// Tempo real (chat, presença, dados alterados) — ver Realtime/TempoRealHub.cs
builder.Services.AddSignalR();
builder.Services.AddSingleton<PresencaTracker>();


builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetLigacaoBd()));

builder.Services.AddApplicationServices(builder.Configuration);

// ─── JWT ────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwt = jwtSection.Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;                       // senão "role" não vira ClaimTypes.Role
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            NameClaimType = "sub",
            RoleClaimType = "role",                       // o token tem de ser criado com estes nomes
            ClockSkew = TimeSpan.Zero,
        };
        // O WebSocket do SignalR não envia cabeçalhos: o token vem na query string (?access_token=).
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
var app = builder.Build();

// Aplicar migrations pendentes e semear dados (em Development, ou com Seed:Demo=true)
using (var scope = app.Services.CreateScope())

{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    if (app.Configuration.ModoDemo(app.Environment))
    {
        await DbSeeder.SeedAsync(db);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();                                   // serve /swagger/v1/swagger.json
    app.UseSwaggerUI(o =>                               // UI em http://localhost:3333/swagger
    {
        o.EnablePersistAuthorization();                 // não perdes o token ao recarregar (F5)
        o.DocumentTitle = "Novati API";
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("front");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TempoRealHub>("/hubs/tempo-real");
// Verificação de saúde para o alojamento (sem token — a política por omissão exige login).
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.Run();
