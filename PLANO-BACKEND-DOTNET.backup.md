# Plano de implementação — Back-end Novati (.NET 10 Web API + EF Core + PostgreSQL)

> **Como usar este ficheiro:** segue as fases por ordem. Cada fase tem: objetivo, o que aprendes, comandos, o que criar, e **como testar antes de passar à seguinte**. Não saltes fases — cada uma assenta na anterior.
> **Divisão de trabalho:** tu escreves o back-end (eu oriento como sénior e reviso). Eu trato da **ligação ao front-end** (Fase 13). Para isso, cada endpoint abaixo tem o **contrato exato** que o front espera — se o cumprires à letra, a ligação não dá erros.

---

## 0. O que descobri no teu front-end (resumo da análise)

- Front: React 19 + Vite, pasta `novati/`. **Já está preparado para uma API REST**: `src/lib/api.js` (cliente fetch com JWT) e `src/context/AppContext.jsx` (todas as chamadas).
- O `.env` já aponta para `VITE_API_URL=http://localhost:3333/api` → **o teu back-end deve correr na porta 3333 com prefixo `/api`**. Assim não mexemos no front.
- Existiu antes um back-end NestJS (`backend.log`) que já não está na pasta. Estamos a **reescrevê-lo em .NET** mantendo o mesmo contrato.
- Existem testes E2E (`novati/e2e/integracao.spec.js`) que assumem: login `POST /auth/login`, password de todos os utilizadores demo = `novati123`, dados de seed iguais ao `seed.js`. **Vamos usar esses testes como critério de "está pronto".**
- 4 perfis: `ADMIN`, `GESTOR`, `TECNICO`, `FUNCIONARIO`.
- Ferramentas na tua máquina: **.NET SDK 10.0.102 ✅**, **dotnet-ef 10.0.12 ✅**, **PostgreSQL ❌ (falta — Fase 0)**.

### As 6 regras de ouro do contrato (guarda isto — evita 90% dos erros de ligação)

| # | Regra | Porquê |
|---|-------|--------|
| 1 | Prefixo `/api`, porta **3333**, JSON em **camelCase** | `.env` do front + ASP.NET já faz camelCase por defeito |
| 2 | **Nunca devolver 200 com corpo vazio.** Ou devolves JSON, ou `204 NoContent` | `api.js` faz `res.json()` em tudo o que não é 204 → corpo vazio = erro "Unexpected end of JSON" mesmo com sucesso |
| 3 | Erros sempre no formato `{ "statusCode": 400, "message": "texto" }` (ou `message` como array de textos) | `api.js` lê `data.message`. O `ProblemDetails` por defeito do .NET usa `title/detail` → o utilizador veria "Erro 400" |
| 4 | **Os 17 GET da hidratação nunca devolvem 401/403/404/500 para nenhum perfil** — filtram (devolvem menos dados ou `[]`) em vez de bloquear | Se um só falhar, o `catch` do `AppContext` faz **logout** e o utilizador volta ao login sem explicação |
| 5 | **Coleções nunca `null`**: `pecasUsadas`, `rejeicoes`, `historico`, `anexos`, `tags` são sempre `[]` quando vazias | O front faz `ordem.historico.map(...)` e `ordem.pecasUsadas.length` |
| 6 | **Enums = texto exatamente como o front escreve** (`EM_DIAGNOSTICO`, `MANUTENCAO`, `rascunho`…) e **datas = `"AAAA-MM-DD"`** | O front compara strings (`estado === 'ABERTA'`) e faz `new Date(dataCriacao)` |

---

## 1. Arquitetura

```
Cliente (React)
   │  HTTP + JSON
   ▼
Controller  ──►  Service  ──►  Repository  ──►  AppDbContext (EF Core)  ──►  PostgreSQL
 (HTTP)         (regras)       (queries)
   ▲               │
   └── DTO ◄── Mapper ◄── Entity
```

| Camada | Responsabilidade | O que **NÃO** faz |
|--------|------------------|-------------------|
| **Controller** | Recebe pedido, valida DTO, chama o Service, devolve `ActionResult` com o status certo | Sem regras de negócio, sem `DbContext` |
| **Service** | Regras de negócio (ex.: "só o técnico dono da ordem pode reservar peças"), orquestra vários repositórios, faz `SaveChanges` **uma vez** por caso de uso | Não conhece HTTP (`HttpContext`, `IActionResult`) |
| **Repository** | Acesso a dados: queries, `Include`, filtros | Sem regras de negócio, sem `SaveChanges` |
| **Mapper** | Converte `Entity ⇄ DTO` (métodos de extensão escritos à mão — para aprenderes; sem AutoMapper) | Sem acesso a dados |
| **Entity** | Classe que representa uma tabela | Nunca sai da API diretamente — sempre via DTO |
| **DTO** | Formato do JSON que entra/sai (o contrato com o front) | — |

**Injeção de dependência (DI):** cada classe recebe o que precisa **pelo construtor** (interfaces). Registas tudo em `Program.cs`. Tempo de vida: `AddScoped` (1 instância por pedido HTTP) para repositórios, services e `DbContext`.

**Unit of Work:** todos os repositórios partilham o mesmo `AppDbContext` (scoped). O Service chama `_uow.SaveChangesAsync()` uma vez no fim → tudo o que alteraste (unidade de stock + ordem + requisição + notificação) grava **numa só transação**, ou nada grava.

### Estrutura de pastas final

```
d:\Nova pasta\
├─ novati\                      ← front-end (já existe)
└─ backend\                     ← NOVO
   ├─ Novati.Api.csproj
   ├─ Program.cs
   ├─ appsettings.json / appsettings.Development.json
   ├─ Properties\launchSettings.json      (porta 3333)
   ├─ Controllers\                Auth, Users, Catalogo, Dispositivos, Stock,
   │                              Solicitacoes, Atendimentos, Compras,
   │                              BaseConhecimento, Notificacoes, Chat,
   │                              RelatoriosTecnicos, Health
   ├─ Services\
   │   ├─ Interfaces\             IAuthService, IUserService, ...
   │   └─ (implementações)
   ├─ Repositories\
   │   ├─ Interfaces\             IRepository<T>, IUserRepository, ...
   │   └─ (implementações)
   ├─ Models\
   │   ├─ Entities\               BaseEntity, User, Solicitacao, ...
   │   └─ Enums\                  Role, EstadoSolicitacao, ...
   ├─ Dtos\                       por módulo (Requests + Responses) + Common\ErrorResponse
   ├─ swagger.json                contrato exportado (secção SWAGGER, passo S.6)
   ├─ Mappers\                    UserMapper, OrdemMapper, ...
   ├─ Data\
   │   ├─ AppDbContext.cs
   │   ├─ UnitOfWork.cs
   │   ├─ Configurations\         1 ficheiro por entidade (Fluent API)
   │   ├─ Migrations\             gerado pelo `dotnet ef`
   │   └─ Seed\DbSeeder.cs
   ├─ Common\
   │   ├─ Exceptions\             NotFoundException, ConflictException, ...
   │   ├─ Middleware\             ExceptionHandlingMiddleware
   │   ├─ Extensions\             ClaimsPrincipalExtensions, ServiceCollectionExtensions
   │   └─ Settings\               JwtSettings
   └─ BackgroundServices\         SlaEscalationService (extra)
```

---

## 2. Modelo de dados — DbSets, entidades e relações

### 2.1 Do estado do front (`useState`) para tabelas

O `AppContext` tem **17 coleções**. Algumas guardam listas *dentro* de outras (ex.: `ordem.historico`) — no relacional isso vira **tabelas filhas**. Total: **21 DbSets**.

| # | Estado no front | DbSet (`AppDbContext`) | Entidade | Endpoint que hidrata |
|---|-----------------|------------------------|----------|----------------------|
| 1 | `users` | `Users` | `User` | `GET /users/directory` |
| 2 | `modelosDispositivo` | `ModelosDispositivo` | `ModeloDispositivo` | `GET /catalogo/modelos-dispositivo` |
| 3 | `modelosComponente` | `ModelosComponente` | `ModeloComponente` | `GET /catalogo/modelos-componente` |
| 4 | `compatibilidades` | `Compatibilidades` | `Compatibilidade` | `GET /catalogo/compatibilidades` |
| 5 | `localizacoes` | `Localizacoes` | `Localizacao` | `GET /dispositivos/localizacoes` |
| 6 | `dispositivosFisicos` | `DispositivosFisicos` | `DispositivoFisico` | `GET /dispositivos` |
| 7 | `instanciasComponentes` | `InstanciasComponentes` | `InstanciaComponente` | `GET /dispositivos/instancias` |
| 8 | `itensStock` | `ItensStock` | `ItemStock` | `GET /stock/itens` |
| 9 | `unidadesStock` | `UnidadesStock` | `UnidadeStock` | `GET /stock/unidades` |
| 10 | `movimentosStock` | `MovimentosStock` | `MovimentoStock` | `GET /stock/movimentos` |
| 11 | `solicitacoes` | `Solicitacoes` | `Solicitacao` | `GET /solicitacoes` |
| 12 | `ordensReparo` | `OrdensReparo` | `OrdemReparo` | `GET /atendimentos/ordens` |
| 12a | `ordem.pecasUsadas[]` | `OrdemPecasUsadas` | `OrdemPecaUsada` | (aninhado na ordem) |
| 12b | `ordem.rejeicoes[]` | `OrdemRejeicoes` | `OrdemRejeicao` | (aninhado na ordem) |
| 12c | `ordem.historico[]` | `OrdemHistoricos` | `OrdemHistorico` | (aninhado na ordem) |
| 13 | `requisicoesCompra` | `RequisicoesCompra` | `RequisicaoCompra` | `GET /compras` |
| 14 | `artigos` | `Artigos` | `Artigo` | `GET /base-conhecimento` |
| 15 | `notificacoes` | `Notificacoes` | `Notificacao` | `GET /notificacoes` |
| 16 | `mensagens` | `Mensagens` | `Mensagem` | `GET /chat/mensagens-todas` |
| 17 | `relatoriosTecnicos` | `RelatoriosTecnicos` | `RelatorioTecnico` | `GET /relatorios-tecnicos` |
| 17a | `relatorio.historico[]` | `RelatorioHistoricos` | `RelatorioHistorico` | (aninhado no relatório) |

> Mais 2 dados que **não são tabelas**: `solicitacao.avaliacao` (owned type na própria tabela) e `solicitacao.anexos[]` / `relatorio.pecasUsadas[]` (coluna JSONB).

### 2.2 Enums (os nomes dos membros são **exatamente** o texto do front)

```csharp
public enum Role { ADMIN, GESTOR, TECNICO, FUNCIONARIO }
public enum Prioridade { BAIXA, MEDIA, ALTA, URGENTE }
public enum EstadoSolicitacao { ABERTA, EM_ATENDIMENTO, AGUARDA_VALIDACAO, RESOLVIDA, FECHADA }
public enum EstadoOrdem { EM_DIAGNOSTICO, EM_REPARACAO, AGUARDA_VALIDACAO, RESOLVIDO }
public enum EstadoDispositivo { ATIVO, MANUTENCAO, INATIVO }
public enum EstadoInstancia { INSTALADO }
public enum EstadoUnidade { DISPONIVEL, RESERVADA, AVARIADA, INSTALADA }
public enum TipoMovimento { ENTRADA, SAIDA, TRANSFERENCIA, RESERVA, INSTALACAO }
public enum EstadoRequisicao { PENDENTE, APROVADA, RECUSADA, ENTREGUE }
public enum TipoHistoricoOrdem { ASSUMIDA, DIAGNOSTICO, SOLUCAO, ACEITE, REJEITADA, COMENTARIO, REATRIBUIDA }
public enum StatusRelatorio { rascunho, finalizado, aprovado }          // minúsculas, como o front
public enum AcaoRelatorio { CRIADO, EDITADO, FINALIZADO, REABERTO, APROVADO }
```

> Em C# é convenção PascalCase, mas aqui **quebramos a convenção de propósito** para que `JsonStringEnumConverter` escreva/leia o texto certo sem código extra. Explica-se num comentário no código.

### 2.3 Entidades (propriedades, tipos e restrições)

Todas herdam de `BaseEntity { public Guid Id { get; set; } = Guid.NewGuid(); }`. O front trata ids como texto opaco → `Guid` serializa como string. ✅
Datas: usa **`DateOnly`** (coluna `date`, JSON `"2026-08-30"`). **Não uses `DateTime`** neste projeto (evita a armadilha do Npgsql exigir `DateTimeKind.Utc`).

| Entidade | Propriedades | Notas / índices |
|----------|--------------|-----------------|
| **User** | `Nome`, `Email`, `PasswordHash`, `Role`, `Assinatura?` (text — dataURL base64) | **Email único**. `PasswordHash` nunca sai em DTO |
| **ModeloDispositivo** | `Nome`, `Fabricante`, `Tipo` | |
| **ModeloComponente** | `Nome`, `Tipo`, `Capacidade`, `StockMinimo` (int, default 1) | |
| **Compatibilidade** | `ModeloDispositivoId`, `ModeloComponenteId` | **Índice único (par)**. Tem `Id` próprio (o front apaga por id) |
| **Localizacao** | `Nome`, `PaiId?` | Auto-relação (árvore). DTO expõe `pai` |
| **DispositivoFisico** | `Patrimonio`, `ModeloDispositivoId`, `NumeroSerie`, `LocalizacaoId`, `ResponsavelId?`, `Estado`, `DataAquisicao?` (DateOnly), `GarantiaMeses?` | **Patrimonio único** |
| **InstanciaComponente** | `DispositivoFisicoId`, `ModeloComponenteId`, `Codigo`, `Estado` | |
| **ItemStock** | `ModeloComponenteId` | **Único** (1 item por modelo) |
| **UnidadeStock** | `ItemStockId`, `Codigo`, `Estado`, `ReservadaParaOrdemId?` | **Codigo único** |
| **MovimentoStock** | `ItemStockId`, `Tipo`, `Quantidade`, `Data` (DateOnly), `Observacao` | |
| **Solicitacao** | `Titulo`, `Descricao`, `Estado`, `DispositivoFisicoId?`, `SolicitanteId`, `DataCriacao`, `Prioridade`, `Categoria`, `ResolvidaViaBase` (bool), `Anexos` (JSONB `List<Anexo>`), `Avaliacao?` (owned: `Estrelas`, `Comentario`, `Data`) | |
| **OrdemReparo** | `SolicitacaoId`, `TecnicoId`, `Diagnostico`, `Solucao?`, `SolucaoSugerida?`, `Estado`, `TempoGastoMin?`, `DataInicio`, `DataFim?` | `SolicitacaoId` **único** (1–1) |
| **OrdemPecaUsada** | `OrdemId`, `UnidadeStockId`, `ModeloComponenteId`, `InstaladoInstanciaId?` | |
| **OrdemRejeicao** | `OrdemId`, `Motivo`, `Data` | |
| **OrdemHistorico** | `OrdemId`, `Tipo`, `Texto`, `AutorId?`, `Data` | DTO expõe `autor` = **nome** do autor |
| **RequisicaoCompra** | `ItemStockId`, `ModeloComponenteId`, `Quantidade`, `Justificativa`, `SolicitanteId`, `OrdemId?`, `Data`, `Estado` | |
| **Artigo** | `Titulo`, `Conteudo`, `Tags` (`List<string>` → `text[]` nativo no Postgres), `Categoria`, `AutorId` | |
| **Notificacao** | `UserId`, `Message`, `Link?`, `Lida`, `Data` | |
| **Mensagem** | `SolicitacaoId`, `AutorId`, `Texto`, `Data` (DateOnly), `Hora` (TimeOnly), `Lida` | DTO expõe `hora` como `"HH:mm"` |
| **RelatorioTecnico** | `OrdemId`, `AutorId`, `CriadoEm`, `AtualizadoEm`, `Status`, `Sumario`, `Diagnostico`, `SolucaoAplicada`, `PecasUsadas` (JSONB `List<PecaRelatorio{Nome,Quantidade,Codigo}>`), `TempoGastoMin`, `Procedimentos`, `Observacoes`, `AssinaturaTecnico`, `AssinaturaResponsavel`, `ComentariosInternos` | `OrdemId` **único** (1 relatório por ordem) |
| **RelatorioHistorico** | `RelatorioId`, `Data`, `AutorId`, `Acao`, `Campo`, `ValorAntigo`, `ValorNovo` | strings vazias, nunca `null` |

### 2.4 Relações (o que aponta para quê) e comportamento ao apagar

| Relação | Tipo | `OnDelete` |
|---------|------|-----------|
| `ModeloDispositivo` 1—N `DispositivoFisico` | 1:N | Restrict |
| `ModeloDispositivo` N—N `ModeloComponente` (via `Compatibilidade`) | N:N com entidade de junção | **Cascade** (o front remove compatibilidades ao apagar modelo) |
| `Localizacao` 1—N `Localizacao` (pai/filhos) | auto-relação | Restrict |
| `Localizacao` 1—N `DispositivoFisico` | 1:N | Restrict |
| `User` 1—N `DispositivoFisico` (responsável) | 1:N opcional | **SetNull** |
| `DispositivoFisico` 1—N `InstanciaComponente` | 1:N | Cascade |
| `ModeloComponente` 1—N `InstanciaComponente` | 1:N | Restrict |
| `ModeloComponente` 1—1 `ItemStock` | 1:1 | Restrict |
| `ItemStock` 1—N `UnidadeStock` / `MovimentoStock` / `RequisicaoCompra` | 1:N | Restrict |
| `OrdemReparo` 1—N `UnidadeStock` (`ReservadaParaOrdemId`) | 1:N opcional | SetNull |
| `User` 1—N `Solicitacao` (solicitante) | 1:N | Restrict |
| `DispositivoFisico` 1—N `Solicitacao` | 1:N opcional | Restrict |
| `Solicitacao` 1—0..1 `OrdemReparo` | 1:1 | Restrict |
| `User` 1—N `OrdemReparo` (técnico) | 1:N | Restrict |
| `OrdemReparo` 1—N `OrdemPecaUsada` / `OrdemRejeicao` / `OrdemHistorico` | 1:N | **Cascade** |
| `OrdemReparo` 1—N `RequisicaoCompra` | 1:N opcional | SetNull |
| `Solicitacao` 1—N `Mensagem` | 1:N | Cascade |
| `User` 1—N `Mensagem` / `Artigo` / `RequisicaoCompra` / `RelatorioTecnico` | 1:N | Restrict |
| `User` 1—N `Notificacao` | 1:N | Cascade |
| `OrdemReparo` 1—0..1 `RelatorioTecnico` | 1:1 | Restrict |
| `RelatorioTecnico` 1—N `RelatorioHistorico` | 1:N | Cascade |

**Regra de ouro:** por defeito usa `Restrict` (a BD recusa apagar quem tem dependentes). Só usa `Cascade` em "filhos que não existem sem o pai". Quando a BD recusar, o Service apanha `DbUpdateException` e devolve **409** com mensagem clara (ex.: *"Não é possível remover: o utilizador tem registos associados."*) — o front mostra essa mensagem.

---

## 3. Fases de implementação

> 📖 **Regra transversal — Swagger em todas as fases (7 a 12):** cada controller que criares tem de ficar documentado no Swagger com a checklist **S.4** (`///`, `[Tags]`, `[Authorize(Roles)]`, `[ProducesResponseType]` para sucesso e erros, `ActionResult<T>`). O critério "✅ Feito quando" de cada fase inclui agora: *"o endpoint aparece em `/swagger` com os schemas e códigos de resposta da tabela de contrato"*. As tabelas de contrato que se seguem são a fonte: cada linha "Resposta"/"Regras/erros" vira `[ProducesResponseType]`.

### FASE 0 — Ambiente (30 min)

**Aprendes:** o que precisas instalado e como verificar.

1. Verificar (já tens): `dotnet --version` → `10.0.102`; `dotnet ef --version` → `10.0.12`.
   Se o `dotnet-ef` estiver desatualizado: `dotnet tool update --global dotnet-ef`.
2. **PostgreSQL** (falta). Escolhe **uma**:
   - **Docker** (recomendado, isolado):
     ```powershell
     docker run --name novati-pg -e POSTGRES_USER=novati -e POSTGRES_PASSWORD=novati123 -e POSTGRES_DB=novati -p 5432:5432 -d postgres:17
     ```
     Depois de reiniciares o PC: `docker start novati-pg`.
   - **Instalador Windows** (postgresql.org/download) → cria a BD `novati` no pgAdmin.
3. Cliente gráfico para veres as tabelas: **pgAdmin** ou a extensão *PostgreSQL* do VS Code.

**✅ Feito quando:** consegues ligar ao Postgres (`localhost:5432`, user `novati`, BD `novati`).

---

### FASE 1 — Criar o projeto e o "esqueleto" (1 h)

**Aprendes:** estrutura de um projeto Web API, `Program.cs`, DI, CORS, launchSettings.

```powershell
cd "d:\Nova pasta"
dotnet new webapi -n Novati.Api -o backend --use-controllers
cd backend

# Bibliotecas de dados (as 3 que pediste)
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design

# Segurança
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next

# Documentação/teste interativo — Swagger (ver secção "SWAGGER" logo a seguir à Fase 1)
dotnet add package Swashbuckle.AspNetCore

dotnet new gitignore
dotnet build
```

> `--use-controllers` é essencial: o template novo por defeito usa *Minimal APIs*; nós queremos **Controllers**.
> Sem `--version`, o `dotnet add package` escolhe a versão compatível com `net10.0`.

**O que fazer:**
1. Apagar `WeatherForecast.cs` e `Controllers/WeatherForecastController.cs`. Apagar também o `AddOpenApi()` / `MapOpenApi()` que o template gera (o Swashbuckle substitui-os); podes remover o pacote `Microsoft.AspNetCore.OpenApi` do `.csproj`.
2. Criar as pastas da estrutura (secção 1).
3. `Properties/launchSettings.json` → no perfil `http`, `"applicationUrl": "http://localhost:3333"`.
4. Criar `Controllers/HealthController.cs`:
   ```csharp
   [ApiController]
   [Route("api/health")]
   [AllowAnonymous]
   public class HealthController : ControllerBase
   {
       [HttpGet] public IActionResult Get() => Ok(new { status = "ok" });
   }
   ```
5. `Program.cs` base (vamos crescer nas fases seguintes):
   ```csharp
   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddControllers();
   builder.Services.AddSwaggerGen();         // configuração completa na secção SWAGGER (S.2)

   builder.Services.AddCors(o => o.AddPolicy("front", p => p
       .WithOrigins("http://localhost:5173", "http://localhost:4173")
       .AllowAnyHeader().AllowAnyMethod()));

   var app = builder.Build();

   if (app.Environment.IsDevelopment())
   {
       app.UseSwagger();
       app.UseSwaggerUI();                   // UI em http://localhost:3333/swagger
   }

   app.UseCors("front");
   app.MapControllers();
   app.Run();
   ```
   ⚠️ Ordem dos middlewares importa: `UseCors` → (`UseAuthentication` → `UseAuthorization` mais tarde) → `MapControllers`.
   ⚠️ **Remove** `app.UseHttpsRedirection()` se o template o incluir — o front chama `http://`, e o redirect para https parte o CORS.

**Correr:** `dotnet run` (ou `dotnet watch` para recarregar sozinho).

**✅ Feito quando:** `curl.exe http://localhost:3333/api/health` → `{"status":"ok"}` e abres `http://localhost:3333/swagger` (Swagger UI com o endpoint `GET /api/health` listado). Depois faz a secção **SWAGGER** abaixo.
> No PowerShell usa sempre **`curl.exe`** (o `curl` sozinho é um alias de outro comando).

---

### 📖 SWAGGER — documentação viva da API (faz logo a seguir à Fase 1, 45 min)

**Aprendes:** o que é OpenAPI/Swagger, como a documentação é **gerada a partir do teu código**, como testar endpoints autenticados sem escrever `curl`, e como o Swagger passa a ser o **contrato oficial** que eu uso para ligar o front-end.

| Termo | Significado |
|---|---|
| **OpenAPI** | A especificação: um JSON (`/swagger/v1/swagger.json`) que descreve todas as rotas, bodies, respostas e erros |
| **Swagger UI** | A página web (`/swagger`) que lê esse JSON e deixa-te **experimentar** cada endpoint |
| **Swashbuckle** | A biblioteca .NET que gera o JSON a partir dos teus controllers e serve a UI |

> Versão validada por mim (compila e serve a UI em `net10.0`): `Swashbuckle.AspNetCore 10.2.3`. Usa `Microsoft.OpenApi 2.x`, por isso a sintaxe abaixo é a atual — tutoriais antigos com `OpenApiReference` **não compilam**.

#### S.1 Ativar o XML de comentários (`Novati.Api.csproj`)

Os comentários `///` que escreves nos controllers passam a aparecer no Swagger. Dentro de `<PropertyGroup>`:
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);1591</NoWarn>   <!-- 1591 = "falta comentário XML": evita avisos em cada método -->
```

#### S.2 Configurar o Swagger no `Program.cs`

Não há classe de extensão: fica tudo no `Program.cs`. **Substitui** o `AddSwaggerGen()` simples da Fase 1 por esta versão completa, e mantém o `UseSwagger()`/`UseSwaggerUI()` dentro do `if (IsDevelopment())`:

```csharp
using System.Reflection;
using Microsoft.OpenApi;

// ... antes do builder.Build():
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Novati API",
        Version = "v1",
        Description = "API do helpdesk de TI. Todas as rotas (exceto /auth/login e /health) exigem JWT. " +
                      "Contas demo (password novati123): rita.admin, carlos.gestor, joao.tecnico, " +
                      "marta.tecnica, pedro.funcionario, ana.funcionaria (@empresa.com)."
    });

    // Comentários /// dos controllers e DTOs
    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml)) o.IncludeXmlComments(xml);

    // Botão "Authorize" (cadeado) para colar o JWT
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Cola apenas o accessToken devolvido por POST /api/auth/login (o Swagger acrescenta 'Bearer ')."
    });
    o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = []
    });
});

// ... depois do builder.Build(), dentro do if (app.Environment.IsDevelopment()):
app.UseSwagger();                                   // serve /swagger/v1/swagger.json
app.UseSwaggerUI(o =>                               // UI em http://localhost:3333/swagger
{
    o.EnablePersistAuthorization();                 // não perdes o token ao recarregar (F5)
    o.DocumentTitle = "Novati API";
});
```

> ⚠️ O Swagger só deve estar ativo em **Development** (já está: `if (app.Environment.IsDevelopment())`). Em produção expõe a estrutura da tua API.
> ⚠️ Ao chegares à Fase 6 (fallback policy de autenticação), confirma que `/swagger` continua a abrir **sem token** — a UI é servida por middleware, não por controller, por isso normalmente não é afetada. Se ficar bloqueada, diz-me.

#### S.3 Enums em texto no Swagger

Como configuraste o `JsonStringEnumConverter` (Fase 4), o Swagger mostra os enums como **texto com a lista de valores** (`ABERTA`, `EM_ATENDIMENTO`, …) — exatamente o que o front envia. Se vires números (`0,1,2`) no schema, falta o conversor.

#### S.4 Convenção de documentação — em **todos** os controllers (regra do projeto)

Cada action leva **3 coisas**: comentário `///`, um `[ProducesResponseType]` para cada resposta possível, e a role exigida. Assim o Swagger mostra o contrato completo (incluindo erros) — que é o que o front precisa.

Cria primeiro o DTO de erro em `Dtos/Common/ErrorResponse.cs` (é o formato do middleware da Fase 4):
```csharp
/// <summary>Formato de erro devolvido por toda a API.</summary>
public record ErrorResponse(int StatusCode, string Message);
```
> Nos erros de validação (400) o `message` real é um array de textos; documenta-o na mesma como `ErrorResponse` — o Swagger é uma aproximação, o comportamento exato está nas "6 regras de ouro".

Exemplo completo (Fase 6 — `AuthController`):
```csharp
/// <summary>Autenticação.</summary>
[ApiController]
[Route("api/auth")]
[Tags("Autenticação")]                                  // nome do grupo no Swagger
[Produces("application/json")]
public class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>Inicia sessão e devolve o JWT.</summary>
    /// <remarks>
    /// Exemplo de pedido:
    ///
    ///     POST /api/auth/login
    ///     { "email": "rita.admin@empresa.com", "password": "novati123" }
    ///
    /// </remarks>
    /// <response code="200">Credenciais válidas: token + utilizador.</response>
    /// <response code="400">Body inválido (email/password em falta).</response>
    /// <response code="401">Credenciais inválidas.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        => Ok(await auth.LoginAsync(request));
}
```
E nos DTOs, documenta os campos e restrições:
```csharp
/// <summary>Pedido de login.</summary>
public class LoginRequest
{
    /// <summary>Email do utilizador.</summary>
    /// <example>rita.admin@empresa.com</example>
    [Required, EmailAddress] public string Email { get; set; } = "";

    /// <summary>Palavra-passe em texto.</summary>
    /// <example>novati123</example>
    [Required] public string Password { get; set; } = "";
}
```
> `<example>` preenche automaticamente o exemplo no botão *Try it out* — poupa-te escrever JSON à mão.

**Checklist por action** (usa em todas as fases):
- [ ] `/// <summary>` numa frase (o que faz)
- [ ] `[ProducesResponseType]` para o **sucesso** (200/201/204) e para cada erro da tabela de contrato da fase (400, 401, 403, 404, 409)
- [ ] `[Authorize(Roles = "...")]` igual à coluna "Quem" da tabela de contrato
- [ ] `[Tags("Nome do módulo")]` no controller
- [ ] Ações que devolvem **204** → `[ProducesResponseType(StatusCodes.Status204NoContent)]`
- [ ] Retornos tipados (`ActionResult<T>`), nunca `IActionResult` a devolver objetos anónimos (o Swagger não consegue descrever o formato)

> 💡 **O teu detetor de erros de contrato:** se o schema que o Swagger mostra para uma resposta não bater com o contrato deste plano (campo em falta, nome diferente, lista que pode ser `null`), **um dos dois está errado** — corrige antes de ligarmos o front.

#### S.5 Como testar com o Swagger UI

1. `dotnet run` → abre **http://localhost:3333/swagger**.
2. Expande **Autenticação → POST /api/auth/login** → *Try it out* → *Execute* com `rita.admin@empresa.com` / `novati123`.
3. Copia o valor de `accessToken` (sem aspas).
4. Botão **Authorize** 🔓 (topo direito) → cola o token → *Authorize* → *Close*. O cadeado fecha 🔒 e **todos os pedidos seguintes já levam o header**.
5. Testa qualquer endpoint: vês o **`curl` equivalente**, o URL, o status e o corpo da resposta.
6. Para testar outro perfil: *Authorize* → *Logout* → faz login com outro email e volta a autorizar (útil para verificar a matriz de permissões da secção 4 — ex.: com o Pedro, `POST /api/users` tem de dar **403**).

#### S.6 Exportar o contrato (para eu ligar o front)

`http://localhost:3333/swagger/v1/swagger.json` é a **especificação completa** da tua API. Na Fase 13 eu leio-o e comparo com o que o `AppContext.jsx` espera — qualquer diferença vira uma correção **antes** de haver erro em runtime. Para o guardares:
```powershell
curl.exe http://localhost:3333/swagger/v1/swagger.json -o "d:\Nova pasta\backend\swagger.json"
```

**✅ Feito quando:** `http://localhost:3333/swagger` abre, mostra "Novati API v1" e lista `GET /api/health`. (Depois da Fase 6: o botão **Authorize** funciona e `GET /api/users/me` devolve os teus dados com o token colado.)

---

### FASE 2 — Enums e Entidades (2 h)

**Aprendes:** classes = tabelas, propriedades de navegação, chaves estrangeiras por convenção, nullable (`?`).

1. Criar os 12 enums da secção 2.2 em `Models/Enums/` (1 ficheiro por enum, ou um só `Enums.cs`).
2. Criar `BaseEntity` e as 21 entidades da secção 2.3 em `Models/Entities/`.
3. Para cada relação, declara **as duas pontas**:
   ```csharp
   public class Solicitacao : BaseEntity
   {
       public string Titulo { get; set; } = "";
       public EstadoSolicitacao Estado { get; set; } = EstadoSolicitacao.ABERTA;
       public Guid SolicitanteId { get; set; }          // FK
       public User Solicitante { get; set; } = null!;   // navegação
       public Guid? DispositivoFisicoId { get; set; }   // FK opcional
       public DispositivoFisico? DispositivoFisico { get; set; }
       public List<Anexo> Anexos { get; set; } = [];     // JSONB
       public Avaliacao? Avaliacao { get; set; }         // owned
       public OrdemReparo? Ordem { get; set; }           // 1–1 inverso
   }
   ```
4. Inicializa **todas as listas** (`= []`) e strings (`= ""`) — regra de ouro nº 5.
5. Classes auxiliares (não são tabelas): `Anexo { Nome, Tipo, DataUrl }`, `Avaliacao { Estrelas, Comentario, Data }`, `PecaRelatorio { Nome, Quantidade, Codigo }`.

**✅ Feito quando:** `dotnet build` sem erros.

---

### FASE 3 — `AppDbContext`, configurações e MIGRATIONS (2 h)

**Aprendes:** o que é o `DbContext`, Fluent API, migrations, como o EF cria as tabelas.

1. `Data/AppDbContext.cs`:
   ```csharp
   public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
   {
       public DbSet<User> Users => Set<User>();
       public DbSet<ModeloDispositivo> ModelosDispositivo => Set<ModeloDispositivo>();
       public DbSet<ModeloComponente> ModelosComponente => Set<ModeloComponente>();
       public DbSet<Compatibilidade> Compatibilidades => Set<Compatibilidade>();
       public DbSet<Localizacao> Localizacoes => Set<Localizacao>();
       public DbSet<DispositivoFisico> DispositivosFisicos => Set<DispositivoFisico>();
       public DbSet<InstanciaComponente> InstanciasComponentes => Set<InstanciaComponente>();
       public DbSet<ItemStock> ItensStock => Set<ItemStock>();
       public DbSet<UnidadeStock> UnidadesStock => Set<UnidadeStock>();
       public DbSet<MovimentoStock> MovimentosStock => Set<MovimentoStock>();
       public DbSet<Solicitacao> Solicitacoes => Set<Solicitacao>();
       public DbSet<OrdemReparo> OrdensReparo => Set<OrdemReparo>();
       public DbSet<OrdemPecaUsada> OrdemPecasUsadas => Set<OrdemPecaUsada>();
       public DbSet<OrdemRejeicao> OrdemRejeicoes => Set<OrdemRejeicao>();
       public DbSet<OrdemHistorico> OrdemHistoricos => Set<OrdemHistorico>();
       public DbSet<RequisicaoCompra> RequisicoesCompra => Set<RequisicaoCompra>();
       public DbSet<Artigo> Artigos => Set<Artigo>();
       public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
       public DbSet<Mensagem> Mensagens => Set<Mensagem>();
       public DbSet<RelatorioTecnico> RelatoriosTecnicos => Set<RelatorioTecnico>();
       public DbSet<RelatorioHistorico> RelatorioHistoricos => Set<RelatorioHistorico>();

       protected override void OnModelCreating(ModelBuilder mb)
           => mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

       // Guardar TODOS os enums como texto na BD (legível no pgAdmin e estável se reordenares)
       protected override void ConfigureConventions(ModelConfigurationBuilder cb)
       {
           cb.Properties<Role>().HaveConversion<string>();
           cb.Properties<Prioridade>().HaveConversion<string>();
           cb.Properties<EstadoSolicitacao>().HaveConversion<string>();
           // ... uma linha por enum (os 12)
       }
   }
   ```
2. `Data/Configurations/*.cs` — 1 classe `IEntityTypeConfiguration<T>` por entidade com: `HasKey`, `HasMaxLength`, `HasIndex(...).IsUnique()`, `HasOne(...).WithMany(...).HasForeignKey(...).OnDelete(...)` (tabela 2.4). Exemplos-chave:
   - `User`: `HasIndex(u => u.Email).IsUnique()`
   - `Solicitacao`: `OwnsOne(s => s.Avaliacao)` e `OwnsMany(s => s.Anexos, b => b.ToJson())`
   - `RelatorioTecnico`: `OwnsMany(r => r.PecasUsadas, b => b.ToJson())`
   - `Localizacao`: `HasOne(l => l.Pai).WithMany(l => l.Filhos).HasForeignKey(l => l.PaiId).OnDelete(DeleteBehavior.Restrict)`
   - `OrdemReparo`: `HasOne(o => o.Solicitacao).WithOne(s => s.Ordem).HasForeignKey<OrdemReparo>(o => o.SolicitacaoId)` + índice único
3. `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Host=localhost;Port=5432;Database=novati;Username=novati;Password=novati123"
     },
     "Jwt": { "Key": "troca-isto-por-uma-chave-com-pelo-menos-32-caracteres!", "Issuer": "novati", "Audience": "novati-front", "ExpiresMinutes": 480 }
   }
   ```
4. `Program.cs`: registar o contexto:
   ```csharp
   builder.Services.AddDbContext<AppDbContext>(o =>
       o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
   ```

### 🔧 Comandos de migrations (a partir da pasta `backend\`)

```powershell
# 1) Criar a migration inicial (gera as classes em Data/Migrations)
dotnet ef migrations add CreateInitial -o Data/Migrations

# 2) Aplicar à base de dados (cria as tabelas no Postgres)
dotnet ef database update
```

Outros comandos que vais usar ao longo do projeto:

```powershell
dotnet ef migrations add NomeDaAlteracao -o Data/Migrations   # nova migration depois de mudares uma entidade
dotnet ef migrations list                                     # ver quais estão aplicadas
dotnet ef migrations remove                                   # desfaz a ÚLTIMA migration (só se ainda não aplicada)
dotnet ef database update NomeDaMigrationAnterior             # voltar atrás na BD
dotnet ef database drop --force                               # apagar a BD toda (dev: recomeçar do zero)
dotnet ef migrations script -o script.sql                     # gerar o SQL (para veres o que o EF faz)
dotnet ef dbcontext info                                      # confirmar provider e connection string
```

> Fluxo de trabalho: **mudas entidade/configuração → `migrations add` → lês o ficheiro gerado → `database update`**. Lê sempre a migration gerada antes de aplicar: é assim que aprendes o SQL por trás.
> Se te enganares numa migration **já aplicada**: `database update <anterior>` → `migrations remove` → corrige → `migrations add` de novo.

**✅ Feito quando:** no pgAdmin vês as 21 tabelas + `__EFMigrationsHistory`; `Solicitacoes` tem a coluna `Anexos` (jsonb) e colunas `Avaliacao_*`; `Artigos.Tags` é `text[]`.

---

### FASE 4 — Base transversal: erros, JSON, Repository, Unit of Work (2 h)

**Aprendes:** middleware, exceções de domínio, repositório genérico, DI, a regra nº 3 do contrato.

1. **Exceções** em `Common/Exceptions/`: `NotFoundException` (404), `ForbiddenException` (403), `ConflictException` (409), `BusinessRuleException` (400), `UnauthorizedException` (401) — todas com `Message`.
2. **`ExceptionHandlingMiddleware`**: `try { await next(ctx); } catch (...)` → mapeia exceção → status e escreve `{ "statusCode": N, "message": "..." }`. Exceções desconhecidas → `500` com mensagem genérica *"Erro interno do servidor."* (e log do erro real — nunca exponhas o stack trace).
3. **Erros de validação** (`[Required]`, `[EmailAddress]`, `[Range]` nos DTOs) — em `Program.cs`:
   ```csharp
   builder.Services.AddControllers()
     .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
     .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = ctx =>
     {
         var msgs = ctx.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
         return new BadRequestObjectResult(new { statusCode = 400, message = msgs });
     });
   ```
   E `app.UseMiddleware<ExceptionHandlingMiddleware>();` **antes** de `MapControllers`.
4. **Repositório genérico** `Repositories/Interfaces/IRepository<T>`: `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `Update`, `Remove`, `ExistsAsync`. Implementação `Repository<T>(AppDbContext ctx)` sobre `ctx.Set<T>()`. Leituras: `.AsNoTracking()` quando não vais alterar.
5. **`IUnitOfWork`** com `Task<int> SaveChangesAsync(CancellationToken ct = default)` → implementação `UnitOfWork(AppDbContext ctx)`.
6. `Common/Extensions/ServiceCollectionExtensions.cs` com `AddApplicationServices()` onde vais registar `AddScoped<IX, X>()` de cada repo/service (uma linha por par). No `Program.cs`: `builder.Services.AddApplicationServices();`.

**✅ Feito quando:** um endpoint de teste que faça `throw new NotFoundException("teste")` devolve `404 {"statusCode":404,"message":"teste"}`. (Apaga-o depois.)

---

### FASE 5 — Seed (dados de demonstração) (2 h)

**Aprendes:** popular a BD, hash de passwords, `MigrateAsync` no arranque.

Sem seed **não há utilizadores → não há login → o front não arranca**, e os testes E2E dependem destes dados.

1. `Data/Seed/DbSeeder.cs` — `static async Task SeedAsync(AppDbContext db)`; só corre `if (!await db.Users.AnyAsync())`.
2. Converte **`novati/src/data/seed.js` + `seedRelatorios.js`** para entidades. Os ids do front (`u1`, `md1`…) **não são Guid** → usa um `Dictionary<string, Guid>` (`ids["u1"] = Guid.NewGuid()`) para ligar as relações.
3. Utilizadores (password de todos: **`novati123`**, com `BCrypt.Net.BCrypt.HashPassword("novati123")`):

   | Nome | Email | Role |
   |------|-------|------|
   | Rita Almeida | rita.admin@empresa.com | ADMIN |
   | Carlos Gestor | carlos.gestor@empresa.com | GESTOR |
   | João Técnico | joao.tecnico@empresa.com | TECNICO |
   | Marta Técnica | marta.tecnica@empresa.com | TECNICO |
   | Pedro Funcionário | pedro.funcionario@empresa.com | FUNCIONARIO |
   | Ana Funcionária | ana.funcionaria@empresa.com | FUNCIONARIO |

4. Ordem de inserção (respeita as FKs): Users → ModelosDispositivo → ModelosComponente → Compatibilidades → Localizacoes (pais primeiro: Financeiro, depois Sala 2) → DispositivosFisicos → InstanciasComponentes → ItensStock → Solicitacoes → OrdensReparo (+PecasUsadas, Rejeicoes, Historico) → UnidadesStock (a `us4` fica reservada para `or1`, por isso vem depois) → MovimentosStock → Artigos → Mensagens (converter `hora: '09:15'` → `TimeOnly`) → RelatoriosTecnicos (+Historico).
   Usa `SaveChangesAsync` por blocos se preferires.
5. Para cada ordem `RESOLVIDO` cria histórico mínimo: `ASSUMIDA`, `DIAGNOSTICO`, `SOLUCAO`, `ACEITE` (o front mostra a timeline).
6. `Program.cs`, depois de `Build()`:
   ```csharp
   using (var scope = app.Services.CreateScope())
   {
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
       await db.Database.MigrateAsync();          // aplica migrations pendentes
       if (app.Environment.IsDevelopment()) await DbSeeder.SeedAsync(db);
   }
   ```

**✅ Feito quando:** arrancas o `dotnet run` e no pgAdmin `Users` tem 6 linhas, `Solicitacoes` 10, `Mensagens` 30, `RelatoriosTecnicos` 8. Arrancar 2× seguidas **não duplica**.

---

### FASE 6 — Autenticação (JWT) + Utilizadores (3 h)

**Aprendes:** JWT, BCrypt, claims, `[Authorize]`, roles, DTOs, mappers — o padrão completo pela 1.ª vez.

**Configuração JWT (`Program.cs`)** — atenção à armadilha dos nomes de claims:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(o =>
  {
      o.MapInboundClaims = false;                       // ⚠️ senão "role" não vira ClaimTypes.Role
      o.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true, ValidIssuer = jwt.Issuer,
          ValidateAudience = true, ValidAudience = jwt.Audience,
          ValidateLifetime = true, ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
          NameClaimType = "sub", RoleClaimType = "role",   // ⚠️ tens de criar o token com estes nomes
          ClockSkew = TimeSpan.Zero
      };
  });

// Tudo exige login, exceto o que marcares [AllowAnonymous]
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
```
E `app.UseAuthentication(); app.UseAuthorization();` entre `UseCors` e `MapControllers`.

**Ao gerar o token** (`IAuthService`/`TokenService`): claims `new Claim("sub", user.Id.ToString())`, `new Claim("role", user.Role.ToString())`, `new Claim("email", user.Email)`. Extensão `ClaimsPrincipalExtensions`: `GetUserId()` (lê `sub`) e `GetRole()` (lê `role`).

**Swagger nesta fase:** aplica a checklist S.4 em `AuthController` e `UsersController` (o exemplo completo do login está na secção SWAGGER). Depois de fazer login no Swagger UI e usar o botão **Authorize**, testa que com o token do Pedro (FUNCIONARIO) `POST /api/users` dá **403** e `GET /api/users/directory` dá **200**.

**Ficheiros:** `IUserRepository` (+`GetByEmailAsync`, `EmailExistsAsync`), `IUserService`, `IAuthService`, `UserMapper` (`User.ToDto()`), DTOs `LoginRequest`, `LoginResponse`, `UserDto`, `CreateUserRequest`, `UpdateUserRequest`, `SignatureRequest`, `AuthController`, `UsersController`.

**Contrato:**

| Método + rota | Quem | Body (request) | Resposta | Regras / erros |
|---|---|---|---|---|
| `POST /api/auth/login` | Anónimo | `{ email, password }` | `200 { accessToken, user: UserDto }` | Email/password errados → **`401 { message: "Credenciais inválidas." }`** (mesma mensagem nos dois casos — não reveles qual falhou) |
| `GET /api/users/me` | Autenticado | — | `UserDto` | |
| `GET /api/users/directory` | Autenticado (todos os perfis) | — | `UserDto[]` | ⚠️ tem de funcionar para **todos** (hidratação) |
| `GET /api/users` | ADMIN | — | `UserDto[]` | |
| `POST /api/users` | ADMIN | `{ nome, email, role }` | `201 UserDto` | Sem campo password no form → define **password inicial `novati123`**. Email duplicado → `409 "Já existe um utilizador com este email."` |
| `PATCH /api/users/{id}` | ADMIN | `{ nome, email, role }` | `UserDto` | 404 se não existe; 409 se email de outro |
| `DELETE /api/users/{id}` | ADMIN | — | `204` | Não pode apagar-se a si próprio (400). FK em uso → 409 |
| `PUT /api/users/me/signature` | Autenticado | `{ dataUrl: string \| null }` | `UserDto` **do utilizador atual já atualizado** | `null` remove a assinatura |

**`UserDto`** (é o que o front guarda em `currentUser`): 
```json
{ "id": "guid", "nome": "Rita Almeida", "email": "rita.admin@empresa.com", "role": "ADMIN", "assinatura": null }
```
`assinatura` = dataURL (string) ou `null`. **Nunca** incluas `passwordHash`.

**✅ Feito quando (Swagger UI em `/swagger` ou curl):**
```powershell
curl.exe -X POST http://localhost:3333/api/auth/login -H "Content-Type: application/json" -d "{\"email\":\"rita.admin@empresa.com\",\"password\":\"novati123\"}"
# copia o accessToken:
curl.exe http://localhost:3333/api/users/me -H "Authorization: Bearer <TOKEN>"
curl.exe http://localhost:3333/api/users        # sem token → 401
```
+ password errada → `401` com `message`.
Testes E2E "Autenticação" (2) passam: `npx playwright test -g "Autenticação"` na pasta `novati`.

---

### FASE 7 — Catálogo (modelos e compatibilidades) (2 h)

**Aprendes:** CRUD completo, repositórios simples, tratamento de conflitos e FKs.

Ficheiros: `CatalogoController`, `ICatalogoService/CatalogoService`, `IModeloDispositivoRepository`, `IModeloComponenteRepository`, `ICompatibilidadeRepository`, `CatalogoMapper`, DTOs.
**Leitura:** qualquer perfil autenticado (o funcionário vê nomes de dispositivos). **Escrita:** só `ADMIN`.

| Método + rota | Body | Resposta |
|---|---|---|
| `GET /api/catalogo/modelos-dispositivo` | — | `[{ id, nome, fabricante, tipo }]` |
| `POST /api/catalogo/modelos-dispositivo` | `{ nome, fabricante, tipo }` | `201` o objeto criado |
| `PATCH /api/catalogo/modelos-dispositivo/{id}` | `{ nome, fabricante, tipo }` | objeto atualizado |
| `DELETE /api/catalogo/modelos-dispositivo/{id}` | — | `204` (compatibilidades apagam em cascata; se houver dispositivos → 409 *"Existem dispositivos deste modelo."*) |
| `GET /api/catalogo/modelos-componente` | — | `[{ id, nome, tipo, capacidade, stockMinimo }]` |
| `POST /api/catalogo/modelos-componente` | `{ nome, tipo, capacidade }` (+ `stockMinimo?`) | `201` objeto. **`stockMinimo` default = 1** se não vier (o form do front não envia) |
| `PATCH /api/catalogo/modelos-componente/{id}` | `{ nome, tipo, capacidade, stockMinimo? }` | objeto |
| `DELETE /api/catalogo/modelos-componente/{id}` | — | `204` |
| `GET /api/catalogo/compatibilidades` | — | `[{ id, modeloDispositivoId, modeloComponenteId }]` |
| `POST /api/catalogo/compatibilidades` | `{ modeloDispositivoId, modeloComponenteId }` | `201` objeto. Par repetido → 409; ids inexistentes → 404 |
| `DELETE /api/catalogo/compatibilidades/{id}` | — | `204` |

> ⚠️ Front, `removeModeloDispositivo` etc.: depois do POST/PATCH o front **re-faz o GET da lista** — por isso a resposta do POST/PATCH pode ser qualquer JSON válido, mas devolve o objeto criado (boa prática).

**✅ Feito quando:** CRUD completo testado no Swagger UI (`/swagger`); o `GET` devolve os 3 modelos de dispositivo, 4 de componente e 6 compatibilidades do seed.

---

### FASE 8 — Localizações, Dispositivos e Instâncias (3 h)

**Aprendes:** auto-relação (árvore), `Include`/`ThenInclude`, rotas com literais vs `{id}`, DateOnly.

Ficheiros: `DispositivosController` (rota base `api/dispositivos`), `IDispositivoService`, repos `IDispositivoRepository`, `ILocalizacaoRepository`, `IInstanciaRepository`, `DispositivoMapper`.
**Leitura:** todos os perfis (senão a hidratação falha). **Escrita:** `ADMIN`, `TECNICO`.

⚠️ **Conflito de rotas:** `GET /dispositivos/localizacoes` e `GET /dispositivos/instancias` podem ser confundidos com `GET /dispositivos/{id}`. Usa **constraint**: `[HttpGet("{id:guid}")]` — só casa com Guid.

| Método + rota | Body | Resposta |
|---|---|---|
| `GET /api/dispositivos/localizacoes` | — | `[{ id, nome, pai }]` — `pai` = `id` da localização-pai ou `null` |
| `POST /api/dispositivos/localizacoes` | `{ nome, pai }` (`pai` pode ser `null`) | `201 { id, nome, pai }`. `pai` inexistente → 404 |
| `GET /api/dispositivos` | — | `[DispositivoDto]` |
| `POST /api/dispositivos` | `{ patrimonio, modeloDispositivoId, numeroSerie, localizacaoId, responsavelId (null ok), dataAquisicao ("AAAA-MM-DD" ou null), garantiaMeses (int ou null) }` | `201 DispositivoDto` — o front usa `criado.id` e `criado.patrimonio`. Património repetido → `409 "Já existe um dispositivo com este nº de património."` |
| `PATCH /api/dispositivos/{id}/estado` | `{ estado: "ATIVO"\|"MANUTENCAO"\|"INATIVO" }` | `DispositivoDto` atualizado (o front **substitui** o item pela resposta) |
| `GET /api/dispositivos/instancias` | — | `[{ id, dispositivoFisicoId, modeloComponenteId, codigo, estado }]` (`estado`: `"INSTALADO"`) |
| `POST /api/dispositivos/{id}/instancias` | `{ modeloComponenteId, codigo }` | `201` instância criada. Validar: o componente é **compatível** com o modelo do dispositivo (senão 400 *"Componente incompatível com este modelo."*) |

**`DispositivoDto`:**
```json
{ "id":"guid","patrimonio":"NB-001","modeloDispositivoId":"guid","numeroSerie":"ABC123",
  "localizacaoId":"guid","responsavelId":"guid|null","estado":"ATIVO",
  "dataAquisicao":"2023-05-10","garantiaMeses":24 }
```

**✅ Feito quando:** os 3 dispositivos + 4 instâncias + 4 localizações do seed aparecem; mudar estado funciona; criar dispositivo com património repetido dá 409 com mensagem.

---

### FASE 9 — Stock (2–3 h)

**Aprendes:** operações que criam várias linhas, geração de códigos, get-or-create, movimentos.

Ficheiros: `StockController`, `IStockService`, repos `IItemStockRepository`, `IUnidadeStockRepository`, `IMovimentoStockRepository`, `StockMapper`.
**Leitura:** todos os perfis (hidratação!). **Escrita (`entrada`):** `ADMIN`, `TECNICO`.

| Método + rota | Body | Resposta |
|---|---|---|
| `GET /api/stock/itens` | — | `[{ id, modeloComponenteId }]` |
| `GET /api/stock/unidades` | — | `[{ id, itemStockId, codigo, estado, reservadaParaOrdemId }]` (`reservadaParaOrdemId` = `null` quando não reservada) |
| `GET /api/stock/movimentos` | — | `[{ id, itemStockId, tipo, quantidade, data, observacao }]` — **mais recentes primeiro** (o front adiciona novos ao início) |
| `POST /api/stock/entrada` | `{ modeloComponenteId, quantidade, observacao }` | `201` **array** `UnidadeDto[]` das unidades criadas (⚠️ é um **array**: o front faz `criadas.map(...)`) |

**Lógica de `entrada`:**
1. `quantidade` ≥ 1 (senão 400).
2. Modelo existe? (senão 404).
3. **Get-or-create** do `ItemStock` desse modelo.
4. Criar `quantidade` `UnidadeStock` com `Estado = DISPONIVEL`, código único `PREFIXO-NNN`: prefixo pelo tipo (`SSD`→`SSD`, `RAM`→`RAM`, `Fonte`→`FNT`, `Bateria`→`BAT`, outro → 3 primeiras letras em maiúsculas); `NNN` = próximo número livre (verifica se o código já existe; o seed tem `SSD-099`… não colidas).
5. 1 `MovimentoStock` `ENTRADA` com `Quantidade = quantidade` e `Observacao` (default *"Entrada manual"*).
6. Tudo num único `SaveChangesAsync`.

**✅ Feito quando:** 4 itens, 7 unidades, 7 movimentos do seed; uma `POST /stock/entrada` cria N unidades, 1 movimento e o `GET /stock/unidades` reflete.

---

### FASE 10 — Solicitações, Notificações, Base de Conhecimento e Chat (4–5 h)

**Aprendes:** filtros por perfil (visibilidade), `Include` múltiplo, owned types/JSONB, serviço partilhado (notificações), pesquisa de texto.

#### 10.1 `NotificacaoService` (interno, usado por todos os outros)
`Task CriarAsync(Guid userId, string message, string? link)` — **só adiciona** ao contexto (o `SaveChanges` é do Service chamador → mesma transação). Métodos auxiliares: `NotificarPerfisAsync(Role[] roles, ...)`, útil para "avisar todos os técnicos".

| Método + rota | Quem | Resposta |
|---|---|---|
| `GET /api/notificacoes` | Autenticado | **só as do utilizador do token**: `[{ id, userId, message, link, lida, data }]` (`link` = `null` ou rota do front, ex. `"/atendimentos"`) — **ordem cronológica crescente** (o front inverte) |
| `PATCH /api/notificacoes/{id}/lida` | dono | `204` (é dele? senão 403) |
| `PATCH /api/notificacoes/lidas` | Autenticado | `204` — marca **todas** as do utilizador |

⚠️ `PATCH /notificacoes/lidas` vs `/{id}/lida`: rotas diferentes, mas usa `{id:guid}`.

#### 10.2 Solicitações

`SolicitacoesController`, `ISolicitacaoService`. **Visibilidade no `GET`:** `FUNCIONARIO` → só as suas (`SolicitanteId == userId`); `TECNICO`, `GESTOR`, `ADMIN` → todas.

| Método + rota | Quem | Body | Resposta |
|---|---|---|---|
| `GET /api/solicitacoes` | todos (filtrado) | — | `[SolicitacaoDto]` |
| `POST /api/solicitacoes` | Autenticado | `{ titulo, descricao, dispositivoFisicoId (ou null), prioridade, categoria, anexos: [{nome,tipo,dataUrl}] }` | `201 SolicitacaoDto`. **`solicitanteId` vem do token** (o front até o remove do body). Estado inicial `ABERTA`, `dataCriacao` = hoje. **Notifica todos os TECNICO** (`"Nova solicitação: {titulo}"`, link `/atendimentos`) |
| `POST /api/solicitacoes/{id}/fechar` | solicitante, GESTOR, ADMIN | — | `SolicitacaoDto` (só se `RESOLVIDA`, senão 400 *"Só solicitações resolvidas podem ser fechadas."*) → `FECHADA` |
| `POST /api/solicitacoes/{id}/avaliar` | **só o solicitante** | `{ estrelas (1–5), comentario }` | `SolicitacaoDto`. Só em `RESOLVIDA`/`FECHADA` e se ainda sem avaliação |
| `POST /api/solicitacoes/{id}/resolver-base` | só o solicitante | — | `SolicitacaoDto` → estado `RESOLVIDA`, `resolvidaViaBase = true` (só a partir de `ABERTA`) |

**`SolicitacaoDto`:**
```json
{ "id":"guid","titulo":"...","descricao":"...","estado":"ABERTA","dispositivoFisicoId":"guid|null",
  "solicitanteId":"guid","dataCriacao":"2026-08-30","prioridade":"ALTA","categoria":"Hardware",
  "anexos":[{"nome":"","tipo":"","dataUrl":""}],
  "avaliacao": null,                                   // ou { "estrelas":5,"comentario":"...","data":"2026-08-12" }
  "resolvidaViaBase": false }
```
> `avaliacao` = `null` quando não avaliada (o front testa `!s.avaliacao`).

#### 10.3 Base de conhecimento

`BaseConhecimentoController`. **Leitura:** todos. **Escrita:** `ADMIN`, `TECNICO`, `GESTOR`.

| Método + rota | Body | Resposta |
|---|---|---|
| `GET /api/base-conhecimento` | — | `[{ id, titulo, conteudo, tags: [], categoria, autorId }]` |
| `POST /api/base-conhecimento` | `{ titulo, conteudo, tags: [], categoria }` | `201 ArtigoDto` (`autorId` = utilizador do token) |
| `GET /api/base-conhecimento/buscar?q=texto` | — | `ArtigoDto[]` — **um array simples de artigos** (o front usa `a.id`, `a.titulo`, `a.conteudo`) |

**Algoritmo do `buscar`** (imita `knowledgeEngine.js`): normalizar (minúsculas + remover acentos) → tokens com > 2 letras → pontuação: token no título ×3, nas tags ×2, no conteúdo ×1 → devolver os **3** com maior pontuação > 0, por ordem decrescente. Trabalha em memória (poucos artigos) — não complica com SQL. `q` com menos de 3 caracteres → `[]` (nunca erro).

#### 10.4 Chat

`ChatController`. Cada conversa = 1 solicitação. **Visibilidade** (igual a `utils/chat.js`):
- `FUNCIONARIO` → conversas das **suas** solicitações
- `TECNICO` → solicitações para as quais **tem ordem** (`OrdemReparo.TecnicoId == userId`)
- `ADMIN`, `GESTOR` → todas

| Método + rota | Body | Resposta |
|---|---|---|
| `GET /api/chat/mensagens-todas` | — | `[{ id, solicitacaoId, autorId, texto, data, hora, lida }]` (`hora` = `"09:15"`) **ordenado por data+hora** |
| `POST /api/chat/mensagens` | `{ solicitacaoId, texto }` | `201 MensagemDto`. Autor = token; sem acesso à conversa → 403; texto vazio → 400. **Notifica a outra parte** (link `/chat`) |
| `PATCH /api/chat/conversas/{solicitacaoId}/lidas` | — | `204` — marca como lidas as mensagens **de outros** nessa conversa |

**✅ Feito quando:** login como Pedro (u5) → `GET /solicitacoes` só devolve as dele (`s1,s3,s6,s8,s9`); como João → todas; `mensagens-todas` do Pedro só traz conversas dele; o `buscar?q=wifi` devolve o artigo do Wi-Fi.

---

### FASE 11 — Atendimentos (ordens de reparação) + Compras (5–6 h) ⭐ a fase mais difícil

**Aprendes:** máquina de estados, regras de negócio reais, transações multi-tabela, resposta polimórfica.

`AtendimentosController` (rota base `api/atendimentos`), `IAtendimentoService`, `IOrdemRepository` (sempre com `Include` de `PecasUsadas`, `Rejeicoes`, `Historico`→`Autor`), `OrdemMapper`. `ComprasController`, `IComprasService`, `ICompraRepository`.

#### Máquina de estados

```
Solicitacao:  ABERTA ──assumir──► EM_ATENDIMENTO ──propor-solucao──► AGUARDA_VALIDACAO ──aceite──► RESOLVIDA ──fechar──► FECHADA
                                        ▲                                     │
                                        └─────────────── recusa ──────────────┘

OrdemReparo:  EM_DIAGNOSTICO ──concluir-diagnostico──► EM_REPARACAO ──propor-solucao──► AGUARDA_VALIDACAO ──aceite──► RESOLVIDO
                    ▲                                                                       │
                    └───────────────────────────── recusa (guarda rejeição) ────────────────┘
```

**`OrdemDto`** — o front assume TODOS estes campos:
```json
{ "id":"guid","solicitacaoId":"guid","tecnicoId":"guid",
  "diagnostico":"","solucao":null,"solucaoSugerida":null,
  "estado":"EM_DIAGNOSTICO",
  "pecasUsadas":[{"unidadeStockId":"guid","modeloComponenteId":"guid","instaladoInstanciaId":"guid|null"}],
  "tempoGastoMin":null,"dataInicio":"2026-08-10","dataFim":null,
  "rejeicoes":[{"motivo":"...","data":"2026-08-16"}],
  "historico":[{"id":"guid","tipo":"ASSUMIDA","texto":"...","autor":"João Técnico","data":"2026-08-10"}] }
```
`pecasUsadas`, `rejeicoes`, `historico` **sempre arrays**. `historico[].autor` = **nome** (string). `diagnostico` = `""` (não `null`) quando vazio.

#### Endpoints

`GET /api/atendimentos/ordens` — visibilidade: `ADMIN`/`GESTOR`/`TECNICO` → todas; `FUNCIONARIO` → só as ordens das **suas** solicitações (⚠️ não pode dar 403: hidratação). Devolve `[OrdemDto]`.

| Método + rota | Quem | Body | O que faz | Resposta |
|---|---|---|---|---|
| `POST /api/atendimentos/solicitacoes/{solicitacaoId}/assumir` | `TECNICO`, `ADMIN` | — | Solicitação tem de estar `ABERTA` (senão 409 *"Esta solicitação já foi assumida."*). Cria ordem (`EM_DIAGNOSTICO`, `tecnicoId` = token, `dataInicio` = hoje, `diagnostico = ""`). Solicitação → `EM_ATENDIMENTO`. Histórico `ASSUMIDA` (*"Solicitação assumida por {nome}."*). Notifica o solicitante | `201 OrdemDto` |
| `PATCH /api/atendimentos/ordens/{id}/diagnostico` | técnico da ordem, `ADMIN` | `{ diagnostico }` | Guarda o texto. Só em `EM_DIAGNOSTICO` | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/concluir-diagnostico` | técnico da ordem, `ADMIN` | — | `EM_DIAGNOSTICO` → `EM_REPARACAO`. Histórico `DIAGNOSTICO` com o texto. **Se `diagnostico` vazio → 400 *"Preencha o diagnóstico antes de avançar."*** | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/comentarios` | técnico da ordem, `ADMIN`, `GESTOR` | `{ texto }` | Histórico `COMENTARIO` com `autor` = token | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/reatribuir` | `ADMIN`, `GESTOR` | `{ novoTecnicoId }` | Novo id tem de ser `TECNICO` (senão 400). Muda `tecnicoId`. Histórico `REATRIBUIDA`. Notifica o novo técnico | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/reservar-peca` | técnico da ordem, `ADMIN` | `{ modeloComponenteId }` | Ver **algoritmo A** abaixo | Ver abaixo (**200 nos dois casos**) |
| `POST /api/atendimentos/ordens/{id}/instalar-peca` | técnico da ordem, `ADMIN` | `{ unidadeStockId, dispositivoFisicoId, instanciaAntigaId (ou null) }` | Ver **algoritmo B** | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/propor-solucao` | técnico da ordem, `ADMIN` | `{ solucao, tempoGastoMin }` | Ordem → `AGUARDA_VALIDACAO`; guarda `solucao` e `tempoGastoMin` (> 0); Solicitação → `AGUARDA_VALIDACAO`; histórico `SOLUCAO`; notifica o solicitante (link `/solicitacoes`) | `OrdemDto` (com `solicitacaoId`: o front lê `ordem.solicitacaoId`) |
| `POST /api/atendimentos/ordens/{id}/responder-validacao` | **só o solicitante** da solicitação | `{ aceite: bool, motivo }` | Ver **algoritmo C** | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/solucao-sugerida` | técnico da ordem, `ADMIN` | `{ solucao }` | Guarda `solucaoSugerida` (vinda do assistente) | `OrdemDto` |

**Algoritmo A — reservar peça** (transação única):
1. Ordem em `EM_REPARACAO` (senão 400).
2. Get-or-create `ItemStock` do `modeloComponenteId`.
3. Procurar 1 `UnidadeStock` `DISPONIVEL` desse item.
4. **Se existe:** `Estado = RESERVADA`, `ReservadaParaOrdemId = ordem.Id`; nova `OrdemPecaUsada` (`instaladoInstanciaId = null`); `MovimentoStock` `RESERVA` qty 1 (*"Reservado para ordem …"*). Resposta:
   ```json
   { "ok": true, "ordem": OrdemDto, "unidadeStockId": "guid", "codigo": "RAM-050" }
   ```
5. **Se não existe:** criar `RequisicaoCompra` (`PENDENTE`, qty 1, `justificativa` = *"Sem stock para a ordem …"*, `solicitanteId` = token, `ordemId`); notificar `GESTOR`/`ADMIN` (link `/compras`). Resposta:
   ```json
   { "ok": false, "ordem": OrdemDto, "requisicao": RequisicaoDto, "requisicaoId": "guid" }
   ```
   O front usa `resultado.requisicao.itemStockId` → inclui `itemStockId` no `RequisicaoDto`.

**Algoritmo B — instalar peça:**
1. A unidade está `RESERVADA` **para esta ordem** e existe em `pecasUsadas` (senão 400).
2. (Se possível) validar que o dispositivo é compatível com o modelo do componente.
3. Criar `InstanciaComponente` (`codigo` = código da unidade, `dispositivoFisicoId`, `modeloComponenteId`, `INSTALADO`).
4. Unidade → `INSTALADA`. Preencher `OrdemPecaUsada.InstaladoInstanciaId`.
5. Se `instanciaAntigaId` ≠ null: essa instância tem de ser do mesmo dispositivo → **remover** a instância antiga. *(Desafio opcional: criar uma `UnidadeStock` `AVARIADA` com o código antigo — é o que o relatório do seed descreve.)*
6. `MovimentoStock` `INSTALACAO` (*"Instalado no dispositivo NB-001"*).

**Algoritmo C — responder validação:**
- Ordem tem de estar `AGUARDA_VALIDACAO`; quem chama é o solicitante.
- `aceite = true`: ordem → `RESOLVIDO`, `dataFim` = hoje; solicitação → `RESOLVIDA`; histórico `ACEITE`; notifica o técnico.
- `aceite = false`: `motivo` obrigatório (400 se vazio); nova `OrdemRejeicao`; ordem → `EM_DIAGNOSTICO`; solicitação → `EM_ATENDIMENTO`; histórico `REJEITADA` (texto = motivo); notifica o técnico.
- O front, a seguir, faz `GET /solicitacoes` → tem de refletir o novo estado.

#### Compras

`GET /api/compras` — `ADMIN`/`GESTOR` → todas (ordem de criação **crescente**; o front inverte); **qualquer outro perfil → `[]` (200, não 403)**.

```json
RequisicaoDto = { "id":"guid","itemStockId":"guid","modeloComponenteId":"guid","quantidade":1,
  "justificativa":"...","solicitanteId":"guid","ordemId":"guid|null","data":"2026-09-10","estado":"PENDENTE" }
```

| Método + rota | Quem | O que faz | Resposta |
|---|---|---|---|
| `POST /api/compras/{id}/aprovar` | `GESTOR`, `ADMIN` | `PENDENTE` → `APROVADA` (senão 400). Notifica quem a pediu | `RequisicaoDto` |
| `POST /api/compras/{id}/entrada` | `GESTOR`, `ADMIN` | Só `APROVADA`. Cria `quantidade` unidades `DISPONIVEL` no `ItemStock` (mesma geração de códigos da Fase 9) + `MovimentoStock` `ENTRADA` (*"Entrada da requisição …"*). Requisição → `ENTREGUE` | `RequisicaoDto` |
| *(extra)* `POST /api/compras/{id}/recusar` | `GESTOR`, `ADMIN` | `PENDENTE` → `RECUSADA` | `RequisicaoDto` |

> 💡 **Reutiliza** a lógica de "criar unidades com código único" da Fase 9 num método partilhado (`IStockService.CriarUnidadesAsync`) — não copies código.

**✅ Feito quando:** fluxo completo à mão no Swagger UI: João assume `s1` → diagnóstico → concluir → reservar `mc3` (Fonte, **sem stock** → `ok:false` + requisição) → Carlos aprova e dá entrada → João reserva de novo (`ok:true`) → instala → propõe solução → Pedro aceita → `s1` fica `RESOLVIDA`.

---

### FASE 12 — Relatórios técnicos (3 h)

**Aprendes:** máquina de estados com auditoria (diff de campos), JSONB, permissões por perfil.

`RelatoriosTecnicosController`, `IRelatorioService`, `IRelatorioRepository`, `RelatorioMapper`.

**`RelatorioDto`:**
```json
{ "id":"guid","ordemId":"guid","autorId":"guid","criadoEm":"2026-08-12","atualizadoEm":"2026-08-13",
  "status":"rascunho|finalizado|aprovado",
  "sumario":"","diagnostico":"","solucaoAplicada":"",
  "pecasUsadas":[{"nome":"RAM 8GB DDR4","quantidade":1,"codigo":"RAM-050"}],
  "tempoGastoMin":35,"procedimentos":"","observacoes":"",
  "assinaturaTecnico":"","assinaturaResponsavel":"","comentariosInternos":"",
  "historico":[{"data":"2026-08-12","autorId":"guid","acao":"CRIADO","campo":"","valorAntigo":"","valorNovo":""}] }
```
Strings de texto = `""` quando vazias (nunca `null`).

**Visibilidade no `GET` (e no `GET /{id}`)** — nunca 403 na lista:
- `ADMIN`, `TECNICO` → todos
- `GESTOR` → só `finalizado` e `aprovado` (o teste E2E "gestor vê relatórios finalizados/aprovados" depende disto)
- `FUNCIONARIO` → só `aprovado` **das suas solicitações**

| Método + rota | Quem | Body | Regras | Resposta |
|---|---|---|---|---|
| `GET /api/relatorios-tecnicos` | todos (filtrado) | — | | `[RelatorioDto]` |
| `GET /api/relatorios-tecnicos/{id}` | quem pode ver | — | 404 se não existe/sem acesso | `RelatorioDto` |
| `POST /api/relatorios-tecnicos` | técnico da ordem, `ADMIN` | `{ ordemId, sumario, diagnostico, solucaoAplicada, pecasUsadas: [{nome,quantidade,codigo}], tempoGastoMin, procedimentos, observacoes, assinaturaTecnico, assinaturaResponsavel, comentariosInternos }` | Ordem tem de estar `RESOLVIDO` (400); já existe relatório para a ordem → **409**. Estado `rascunho`, `criadoEm = atualizadoEm = hoje`, histórico `CRIADO` | `201 RelatorioDto` |
| `PATCH /api/relatorios-tecnicos/{id}` | autor, `ADMIN` | qualquer subconjunto dos campos acima | Não editável se `aprovado` (400). Para **cada campo que mudou** → linha de histórico `EDITADO` (`campo`, `valorAntigo`, `valorNovo`; para `pecasUsadas` serializa em JSON). Atualiza `atualizadoEm` | `RelatorioDto` |
| `POST /api/relatorios-tecnicos/{id}/finalizar` | autor, `ADMIN` | — | `rascunho` → `finalizado`; histórico `FINALIZADO`; **notifica GESTOR** (link `/relatorios-tecnicos/{id}`) | `RelatorioDto` |
| `POST /api/relatorios-tecnicos/{id}/reabrir` | autor, `ADMIN` | — | `finalizado` → `rascunho`; histórico `REABERTO` | `RelatorioDto` |
| `POST /api/relatorios-tecnicos/{id}/aprovar` | `GESTOR`, `ADMIN` | — | `finalizado` → `aprovado`; histórico `APROVADO`; se `assinaturaResponsavel` vazia, preenche com o **nome** de quem aprova (como no seed); notifica o autor | `RelatorioDto` |

> ⚠️ **Não inventes regras que o front não conhece** (ex.: exigir `sumario` para finalizar) — o front não tem UI para mostrar esse erro. Só as regras acima.

**✅ Feito quando:** os 8 relatórios do seed aparecem (Carlos vê 5: `rt1,rt2,rt4,rt6,rt7`); um técnico cria → edita (vê `EDITADO` no histórico) → finaliza → gestor aprova; segunda criação para a mesma ordem dá 409.

---

### FASE 13 — Ligação ao Front-end (eu faço — tu acompanhas) (2–3 h)

**Antes de ligar, confirma:** `dotnet run` na porta 3333 e **os 17 GETs respondem 200 para os 4 perfis**. Script rápido (PowerShell) — para cada email: login → chamar os 17 GETs → nenhum pode falhar:

```
/users/me  /users/directory  /catalogo/modelos-dispositivo  /catalogo/modelos-componente
/catalogo/compatibilidades  /dispositivos/localizacoes  /dispositivos  /dispositivos/instancias
/stock/itens  /stock/unidades  /stock/movimentos  /solicitacoes  /atendimentos/ordens
/compras  /base-conhecimento  /notificacoes  /chat/mensagens-todas  /relatorios-tecnicos
```
(Eu escrevo este script quando chegares aqui.)

**Contrato via Swagger:** exporta `swagger.json` (passo S.6) e dá-mo. Eu comparo, endpoint a endpoint, com as chamadas do `AppContext.jsx` (rota, método, body, campos da resposta, códigos de erro) e devolvo-te uma lista de discrepâncias **antes** de ligar. Só depois arranco o front.

**Alterações que eu farei no front (pequenas):**
1. `novati/.env`: corrigir o comentário (já não é NestJS); `src/lib/api.js`: fallback da URL de `3000` → `3333`.
2. **Bug real a corrigir:** `OrdemReparoDetail.jsx` → `gerarRelatorio()` chama `criarRelatorioTecnico(...)` **sem `await`** — a função agora é assíncrona, logo navegaria para `/relatorios-tecnicos/[object Promise]/editar`. Passa a `async/await`.
3. **Corrida:** `atualizarDiagnostico(...)` e `concluirDiagnostico(...)` são chamados em sequência **sem await** (linha ~119 do mesmo ficheiro) — o segundo pode chegar ao servidor antes do primeiro e falhar a regra "diagnóstico vazio". Passo a `await` em sequência.
4. Robustez: trocar `Promise.all` por `Promise.allSettled` na hidratação, para 1 endpoint com falha não deslogar o utilizador (e mostrar `notify` do que falhou).
5. Mostrar erros de `criarSolicitacao`, `assumirSolicitacao`, etc. ao utilizador (hoje as promessas rejeitadas ficam sem `catch` em vários handlers → erros silenciosos).

**Como verificamos no fim:**
```powershell
# terminal 1
cd "d:\Nova pasta\backend"; dotnet run
# terminal 2
cd "d:\Nova pasta\novati"; npm run dev
# terminal 3
cd "d:\Nova pasta\novati"; npx playwright test        # os testes E2E existentes
```
**✅ Feito quando:** os 9 testes de `integracao.spec.js` passam **e** percorres à mão o fluxo completo com os 4 perfis (funcionário abre pedido → técnico assume → reserva → instala → propõe → funcionário valida e avalia → técnico gera relatório → gestor aprova).

---

### FASE 14 — Extras para ser "profissional" (opcionais, depois de tudo a funcionar)

| Extra | O que aprendes |
|---|---|
| **`SlaEscalationService`** (`BackgroundService`, corre a cada X min, notifica GESTORes de solicitações fora do prazo: BAIXA 72h · MEDIA 48h · ALTA 24h · URGENTE 4h — como `utils/sla.js`; o NestJS antigo tinha um) | Serviços em background, `IServiceScopeFactory` |
| **Testes unitários** (xUnit + `Microsoft.EntityFrameworkCore.InMemory` ou Testcontainers) dos Services (`reservar-peca`, `responder-validacao`) | Testabilidade que a arquitetura em camadas te dá |
| **Concorrência otimista**: 2 técnicos a reservarem a última peça em simultâneo → `xmin` do Postgres como *concurrency token* | Concorrência, `DbUpdateConcurrencyException` → 409 |
| **Logs estruturados** (`ILogger`, Serilog) + `GET /api/health` com verificação da BD (`AddHealthChecks().AddNpgSql`) | Observabilidade |
| **Segredos fora do código**: `dotnet user-secrets` para `Jwt:Key` e connection string | Segurança básica |
| **Paginação** (`?page=&pageSize=`) nas listas grandes (movimentos, notificações) | Performance |
| **`docker-compose.yml`** (API + Postgres) | Deploy |
| **Refresh tokens** | Autenticação a sério |
| **Rate limiting** no `/auth/login` (`AddRateLimiter`) | Proteção contra força bruta |

---

## 4. Matriz de permissões (resumo)

| Recurso | ADMIN | GESTOR | TECNICO | FUNCIONARIO |
|---|:-:|:-:|:-:|:-:|
| Utilizadores — escrever | ✅ | ❌ | ❌ | ❌ |
| Catálogo — escrever | ✅ | ❌ | ❌ | ❌ |
| Dispositivos/Localizações/Instâncias — escrever | ✅ | ❌ | ✅ | ❌ |
| Stock — entrada | ✅ | ❌ | ✅ | ❌ |
| Solicitações — criar | ✅ | ✅ | ✅ | ✅ |
| Solicitações — ver | todas | todas | todas | **só as suas** |
| Atendimentos — assumir / reservar / instalar / propor | ✅ | ❌ | ✅ (dono) | ❌ |
| Atendimentos — reatribuir | ✅ | ✅ | ❌ | ❌ |
| Atendimentos — validar solução | — | — | — | ✅ (**só o solicitante**) |
| Compras — ver/aprovar/entrada | ✅ | ✅ | ❌ (`[]`) | ❌ (`[]`) |
| Base de conhecimento — escrever | ✅ | ✅ | ✅ | ❌ |
| Relatórios técnicos — criar/editar | ✅ | ❌ | ✅ (autor) | ❌ |
| Relatórios técnicos — aprovar | ✅ | ✅ | ❌ | ❌ |

> Os **GET nunca devolvem 403** (regra 4): filtram. Só as **escritas** devolvem 403.

---

## 5. Erros comuns e como os evitar (cola isto ao lado do ecrã)

| Sintoma | Causa provável | Solução |
|---|---|---|
| Front volta sempre ao login depois de entrar | Um dos 17 GET falhou (401/403/404/500) | Abre o DevTools → separador *Network* → vê qual ficou vermelho |
| "Failed to fetch" / erro CORS na consola | `UseCors` em falta ou depois de `MapControllers`; origem ≠ `http://localhost:5173`; `UseHttpsRedirection` ativo | Ordem dos middlewares (Fase 1) |
| Mensagem "Erro 400" em vez do texto | Erro de validação com `ProblemDetails` | `InvalidModelStateResponseFactory` (Fase 4) |
| `Unexpected end of JSON input` após ação bem-sucedida | Devolveste `Ok()` sem corpo | `NoContent()` (204) ou devolve o objeto |
| `403` em tudo com `[Authorize(Roles=...)]` | `MapInboundClaims`/`RoleClaimType` mal configurados | Fase 6: `role` + `RoleClaimType = "role"` |
| `Cannot write DateTime with Kind=Unspecified` | Usaste `DateTime` | Usa `DateOnly` / `DateTime.UtcNow` |
| `relation "X" does not exist` | Esqueceste `dotnet ef database update` | Correr o comando |
| Enum chega ao front como número (`0`, `1`) | Falta `JsonStringEnumConverter` | Fase 4 |
| `A possible object cycle was detected` | Serializaste uma **entidade** (com navegações) em vez de DTO | Devolve sempre DTO via Mapper |
| `ordem.historico is undefined` / `.map` de `null` no front | Lista `null` no DTO | Regra 5: `= []` |
| Migration falha com "column already exists" | Aplicaste à mão SQL ou misturaste migrations | `dotnet ef database drop --force` e recomeça (é dev) |
| Swagger não compila (`OpenApiReference`, `OpenApiSecurityScheme.Reference`) | Sintaxe antiga do Microsoft.OpenApi 1.x | Usa `OpenApiSecuritySchemeReference("Bearer", doc)` (secção SWAGGER, S.2) |
| `/swagger` dá 404 | `UseSwagger()`/`UseSwaggerUI()` fora do `if (IsDevelopment())` ou `ASPNETCORE_ENVIRONMENT` ≠ Development | Confirma `launchSettings.json` (perfil `http` com `"ASPNETCORE_ENVIRONMENT": "Development"`) |
| Swagger mostra a action sem schema de resposta | Devolves `IActionResult`/objeto anónimo | `ActionResult<TDto>` + `[ProducesResponseType]` |
| Swagger: "Failed to load API definition" | Exceção ao gerar o JSON (ex.: dois controllers com a mesma rota+método, ou tipos com o mesmo nome em namespaces diferentes) | Abre `/swagger/v1/swagger.json` diretamente — mostra o erro real |
| No Swagger UI o cadeado não envia o token | Colaste `Bearer <token>` (duplica o prefixo) ou falta o `AddSecurityRequirement` | Cola **só** o token |
| Login funciona mas `GET /users/me` dá 401 | Header sem `Bearer ` ou `Jwt:Key`/`Issuer`/`Audience` diferentes entre gerar e validar | Comparar `appsettings` |
| Datas com `T00:00:00` | Usaste `DateTime` no DTO | DTO com `DateOnly` |

---

## 6. Ordem e estimativa

| Fase | Tema | Tempo | Dificuldade |
|:-:|---|:-:|:-:|
| 0 | Ambiente | 0,5 h | ⭐ |
| 1 | Projeto + esqueleto | 1 h | ⭐ |
| — | **Swagger** (documentação + teste) | 0,75 h | ⭐ |
| 2 | Enums + Entidades | 2 h | ⭐⭐ |
| 3 | DbContext + **Migrations** | 2 h | ⭐⭐ |
| 4 | Base (erros, Repository, UoW) | 2 h | ⭐⭐⭐ |
| 5 | Seed | 2 h | ⭐⭐ |
| 6 | Auth JWT + Utilizadores | 3 h | ⭐⭐⭐ |
| 7 | Catálogo | 2 h | ⭐⭐ |
| 8 | Dispositivos | 3 h | ⭐⭐ |
| 9 | Stock | 2–3 h | ⭐⭐⭐ |
| 10 | Solicitações + Notificações + Base + Chat | 4–5 h | ⭐⭐⭐ |
| 11 | Atendimentos + Compras | 5–6 h | ⭐⭐⭐⭐ |
| 12 | Relatórios técnicos | 3 h | ⭐⭐⭐ |
| 13 | **Ligação ao front (eu)** | 2–3 h | ⭐⭐ |
| 14 | Extras | — | — |

**Marcos para não te perderes:**
- Fim da secção **Swagger** → `/swagger` abre e passas a testar tudo sem `curl`.
- Fim da **Fase 6** → tens login real (testável no Swagger UI com o botão **Authorize**).
- Fim da **Fase 12** → todos os 17 GET existem → **já se pode ligar o front**.
- Fim da **Fase 13** → projeto integrado e testado de ponta a ponta.

---

## 7. Como trabalhamos daqui para a frente

1. Diz-me **"vou começar a Fase N"**. Eu resumo o que vais criar e o porquê.
2. Escreves o código. Quando terminares, mostra-me (ou diz "revê a Fase N") e eu faço **code review**: convenções, bugs, segurança, o que ficou fora do contrato.
3. Só passamos à seguinte quando o critério **✅ Feito quando** estiver cumprido.
4. Se ficares preso, cola o erro completo — eu explico **o que significa** antes de corrigir.
5. Na Fase 13 ligo o front e corremos os testes E2E juntos.
