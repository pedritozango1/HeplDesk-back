# ── Fase 1: compilar (imagem com o SDK completo) ─────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Só o .csproj primeiro: o restore dos pacotes fica em cache enquanto as dependências não mudarem.
COPY Novati.API.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# ── Fase 2: correr (imagem só com o runtime, muito mais pequena) ─────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production

# O alojamento (Render, Railway…) indica a porta na variável PORT; sem ela usa 8080.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet Novati.API.dll"]
