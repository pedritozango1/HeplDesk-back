# PLANO MESTRE — Construção Modular do Back-end Novati

> ASP.NET Core Web API (.NET 10) + Entity Framework Core + PostgreSQL, tendo o front-end React existente (`novati/`) como fonte de verdade para requisitos, entidades, relações, regras de negócio e contrato de API.

## Como usar este documento

Este plano funciona como **instrutor**: abre-lo, segue as fases pela ordem indicada (0 → 14) e constrói-se o projeto progressivamente. Para cada parte do sistema, o documento responde sempre às mesmas dez perguntas:

**O QUÊ** vamos criar? → **ONDE** vamos criar? → **PORQUÊ** vamos criar? → **COMO** será implementado? → **COMO** se relaciona com o resto do sistema? → **DE QUÊ** depende? → **O QUÊ** depende dele? → **COMO** testar? → **COMO** validar? → **QUANDO** podemos avançar?

**Divisão de trabalho:** o utilizador escreve o back-end (Claude orienta como sénior e revê). Claude trata da **ligação ao front-end** (Módulo/Fase 13) — para isso, cada endpoint tem o contrato exato que o front espera.

### Os três níveis de detalhe do documento

| Nível | Conteúdo | Quando usar |
|---|---|---|
| **NÍVEL 1 — Roadmap geral** | Todas as fases (0–14), uma linha de objetivo cada, diagrama de dependências entre módulos | Visão do caminho completo, antes de começar |
| **NÍVEL 2 — Plano do módulo** | Por fase: 4.1 a 4.21 (objetivo, funcionalidades, dependências, entidades, DTOs, repository, service, controller, testes, integração front-end, critério de conclusão) | Antes de começar uma fase, para saber o que vais construir e porquê |
| **NÍVEL 3 — Plano da tarefa** | Lista numerada de tarefas dentro do módulo, cada uma com Arquivo / Responsabilidade / Depende de / É utilizado por / Resultado esperado | Durante a implementação, tarefa a tarefa |

### Legenda de estados

Cada módulo e cada tarefa têm um estado, que o utilizador vai atualizando à mão conforme avança:

| Estado | Significado |
|---|---|
| `[PLANEADO]` | Descrito neste documento, código ainda não escrito |
| `[EM IMPLEMENTAÇÃO]` | Código a ser escrito |
| `[IMPLEMENTADO]` | Código escrito, compila |
| `[TESTADO]` | Testado manualmente (Swagger/curl) ou com testes automáticos |
| `[VALIDADO]` | Critério "✅ Feito quando" cumprido |
| `[CONCLUÍDO]` | Módulo fechado, pode avançar-se para o seguinte |

> **Estado atual (2026-09-22): tudo `[PLANEADO]`** — nada foi implementado ainda. Os únicos progressos reais são as ferramentas instaladas na máquina (ver Módulo 0).

### Prioridades deste documento

**PRECISÃO > COMPLETUDE > RASTREABILIDADE > ORGANIZAÇÃO > APRENDIZAGEM > BREVIDADE.** Nada foi cortado do plano anterior para poupar espaço; o conteúdo foi reorganizado e expandido. Onde uma informação não pôde ser determinada a partir do plano anterior, fica marcada explicitamente como `PONTO A DECIDIR`.

---

# NÍVEL 1 — ROADMAP GERAL

## Sequência das 15 fases/módulos

| # | Módulo | Objetivo numa frase | Tempo estim. | Estado |
|:-:|---|---|:-:|---|
| 0 | Ambiente | Ter .NET SDK, dotnet-ef e PostgreSQL prontos na máquina | 0,5 h | `[PLANEADO]` |
| 1 | Projeto + esqueleto + Swagger | Criar o projeto Web API, a estrutura de pastas, CORS, e a documentação viva (Swagger) que serve de contrato oficial | 1,75 h | `[PLANEADO]` |
| 2 | Enums + Entidades | Modelar em C# os 12 enums e as 21 entidades que representam o domínio do helpdesk | 2 h | `[PLANEADO]` |
| 3 | DbContext + Migrations | Ligar as entidades ao PostgreSQL: `AppDbContext`, Fluent API, primeira migration | 2 h | `[PLANEADO]` |
| 4 | Base transversal (erros, Repository, UoW) | Infraestrutura reutilizada por todos os módulos de domínio: exceções, middleware de erro, `Repository<T>`, `UnitOfWork`, DI | 2 h | `[PLANEADO]` |
| 5 | Seed | Popular a BD com os dados de demonstração (`seed.js`), sem os quais não há login nem testes E2E | 2 h | `[PLANEADO]` |
| 6 | Autenticação (JWT) + Utilizadores | Login, emissão/validação de JWT, CRUD de utilizadores — módulo do qual **todos** os módulos seguintes dependem para autorização | 3 h | `[PLANEADO]` |
| 7 | Catálogo | Modelos de dispositivo/componente e compatibilidades — base de dados de referência usada por Dispositivos e Stock | 2 h | `[PLANEADO]` |
| 8 | Localizações, Dispositivos e Instâncias | Inventário físico: onde estão os dispositivos, que componentes têm instalados | 3 h | `[PLANEADO]` |
| 9 | Stock | Peças de substituição: itens, unidades físicas, movimentos de entrada/saída | 2–3 h | `[PLANEADO]` |
| 10 | Solicitações + Notificações + Base de Conhecimento + Chat | Pedido de suporte do funcionário, sistema de avisos, artigos de autoajuda, conversa por solicitação | 4–5 h | `[PLANEADO]` |
| 11 | Atendimentos + Compras | Ciclo de vida da ordem de reparação (a máquina de estados mais complexa do sistema) e o fluxo de requisição de compra quando falta stock | 5–6 h | `[PLANEADO]` |
| 12 | Relatórios técnicos | Documento formal com histórico de auditoria por ordem resolvida | 3 h | `[PLANEADO]` |
| 13 | Ligação ao Front-end | Verificar o contrato endpoint a endpoint contra `AppContext.jsx`/`api.js`, corrigir bugs do front, correr os testes E2E | 2–3 h | `[PLANEADO]` |
| 14 | Extras (opcionais) | SLA, testes unitários, concorrência otimista, logs, secrets, paginação, docker-compose, refresh tokens, rate limiting | — | `[PLANEADO]` |

**Marcos:**
- Fim do Módulo 1 → `/swagger` abre; toda a documentação/teste manual passa a fazer-se ali.
- Fim do Módulo 6 → login real, testável com o botão **Authorize** do Swagger.
- Fim do Módulo 12 → os 17 GET de hidratação existem → já se pode ligar o front.
- Fim do Módulo 13 → projeto integrado e testado de ponta a ponta.

## Diagrama de dependências entre módulos

Uma seta `A → B` lê-se "B depende de A" (B não pode ser validado sem A estar concluído). O motivo da dependência está classificado como **FK** (chave estrangeira obrigatória), **Lógica** (regra de negócio usa dados/serviço do outro módulo), **Autorização** (precisa de `[Authorize]`/roles) ou **Dados** (precisa de registos existentes na BD).

```
Módulo 0 (Ambiente)
   │ Infra: SDK/dotnet-ef/Postgres têm de existir antes de criar o projeto
   ▼
Módulo 1 (Projeto + esqueleto + Swagger)
   │ Infra: as entidades entram num projeto Web API já criado
   ▼
Módulo 2 (Enums + Entidades)
   │ Lógica: o DbContext expõe DbSet<T> para entidades que têm de existir primeiro
   ▼
Módulo 3 (DbContext + Migrations)
   │ FK: o Repository<T> genérico opera sobre o AppDbContext já configurado
   ▼
Módulo 4 (Base transversal: erros/Repository/UoW)
   │ Dados: o seed usa o DbContext e lança as mesmas exceções de domínio
   ▼
Módulo 5 (Seed)
   │ Dados: sem utilizadores semeados não há login possível
   ▼
Módulo 6 (Auth + Utilizadores)  ──────────────────────────────────────────┐
   │ Autorização: TODOS os módulos 7–12 exigem [Authorize(Roles=...)]     │
   │ FK: User é referenciado por quase todas as entidades de domínio     │
   │  (Solicitacao.SolicitanteId, OrdemReparo.TecnicoId, Artigo.AutorId,  │
   │   Mensagem.AutorId, RelatorioTecnico.AutorId, Notificacao.UserId,    │
   │   DispositivoFisico.ResponsavelId, RequisicaoCompra.SolicitanteId)   │
   ▼                                                                       │
Módulo 7 (Catálogo)                                                       │
   │ FK: DispositivoFisico.ModeloDispositivoId, InstanciaComponente       │
   │      .ModeloComponenteId, ItemStock.ModeloComponenteId               │
   │ Lógica: instalar peça valida Compatibilidade(modelo disp., modelo    │
   │         componente)                                                  │
   ├───────────────────────────────┬─────────────────────────────────────┤
   ▼                                ▼                                     │
Módulo 8 (Localizações,        Módulo 9 (Stock)                           │
          Dispositivos,             │ FK: ItemStock por ModeloComponente  │
          Instâncias)               │ Lógica: geração de código de       │
   │ FK: Solicitacao               │        unidade partilhada com Compras│
   │     .DispositivoFisicoId       │                                     │
   │     (opcional)                 │                                     │
   └──────────────┬─────────────────┴──────────────┬──────────────────────┘
                  ▼                                 ▼
         Módulo 10 (Solicitações + Notificações + Base + Chat)
                  │ FK: OrdemReparo.SolicitacaoId (1–1); Mensagem.SolicitacaoId
                  │ Lógica: "assumir" só parte de Solicitacao ABERTA
                  ▼
         Módulo 11 (Atendimentos + Compras)  ← depende também do Módulo 9
                  │  (reservar/instalar peça usa UnidadeStock/ItemStock/
                  │   MovimentoStock; falta de stock cria RequisicaoCompra)
                  │ FK: RelatorioTecnico.OrdemId (1–1)
                  │ Lógica: só se cria relatório de uma ordem RESOLVIDO
                  ▼
         Módulo 12 (Relatórios técnicos)
                  │ Dados: os 17 GET de hidratação têm de estar todos a
                  │        responder 200 antes de ligar o front
                  ▼
         Módulo 13 (Ligação ao Front-end)
                  │ Lógica: extras só fazem sentido com o sistema já
                  │         a funcionar ponta a ponta
                  ▼
         Módulo 14 (Extras)
```

**Regra de ordem:** não se implementa um módulo antes de a parte necessária das suas dependências estar `[CONCLUÍDO]`. Exemplo: não se começa o Módulo 11 (Atendimentos) sem o Módulo 9 (Stock) ter `POST /api/stock/entrada` e as entidades `ItemStock`/`UnidadeStock`/`MovimentoStock` validadas — Atendimentos **reutiliza** essa lógica (algoritmo de reserva de peça), não a reimplementa.

---

# FUNDAMENTOS TRANSVERSAIS

> Esta secção aplica-se a **todos** os módulos abaixo. É o que dá coerência ao sistema inteiro: se um módulo parecer contradizer isto, a secção transversal vence.

## F.1 — O que foi descoberto no front-end (fonte dos requisitos)

- Front: React 19 + Vite, pasta `novati/`. **Já está preparado para uma API REST**: `src/lib/api.js` (cliente fetch com JWT) e `src/context/AppContext.jsx` (todas as chamadas).
- O `.env` já aponta para `VITE_API_URL=http://localhost:3333/api` → **o back-end deve correr na porta 3333 com prefixo `/api`**. Assim não se mexe no front.
- Existiu antes um back-end NestJS (`backend.log`) que já não está na pasta. Está a ser **reescrito em .NET** mantendo o mesmo contrato.
- Existem testes E2E (`novati/e2e/integracao.spec.js`) que assumem: login `POST /auth/login`, password de todos os utilizadores demo = `novati123`, dados de seed iguais ao `seed.js`. **Esses testes são o critério de "está pronto"** (ver Módulo 13).
- 4 perfis: `ADMIN`, `GESTOR`, `TECNICO`, `FUNCIONARIO`.
- Ferramentas confirmadas na máquina: **.NET SDK 10.0.102 ✅**, **dotnet-ef 10.0.12 ✅**, **PostgreSQL ❌** (instala-se no Módulo 0).

## F.2 — As 6 regras de ouro do contrato

Evitam 90% dos erros de ligação entre back-end e front-end. Válidas em **todos** os módulos:

| # | Regra | Porquê |
|---|-------|--------|
| 1 | Prefixo `/api`, porta **3333**, JSON em **camelCase** | `.env` do front + ASP.NET já faz camelCase por defeito |
| 2 | **Nunca devolver 200 com corpo vazio.** Ou devolve-se JSON, ou `204 NoContent` | `api.js` faz `res.json()` em tudo o que não é 204 → corpo vazio = erro "Unexpected end of JSON" mesmo com sucesso |
| 3 | Erros sempre no formato `{ "statusCode": 400, "message": "texto" }` (ou `message` como array de textos) | `api.js` lê `data.message`. O `ProblemDetails` por defeito do .NET usa `title/detail` → o utilizador veria "Erro 400" |
| 4 | **Os 17 GET da hidratação nunca devolvem 401/403/404/500 para nenhum perfil** — filtram (devolvem menos dados ou `[]`) em vez de bloquear | Se um só falhar, o `catch` do `AppContext` faz **logout** e o utilizador volta ao login sem explicação |
| 5 | **Coleções nunca `null`**: `pecasUsadas`, `rejeicoes`, `historico`, `anexos`, `tags` são sempre `[]` quando vazias | O front faz `ordem.historico.map(...)` e `ordem.pecasUsadas.length` |
| 6 | **Enums = texto exatamente como o front escreve** (`EM_DIAGNOSTICO`, `MANUTENCAO`, `rascunho`…) e **datas = `"AAAA-MM-DD"`** | O front compara strings (`estado === 'ABERTA'`) e faz `new Date(dataCriacao)` |

## F.3 — Arquitetura em camadas

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
| **Mapper** | Converte `Entity ⇄ DTO` (métodos de extensão escritos à mão — sem AutoMapper, para se aprender o mapeamento) | Sem acesso a dados |
| **Entity** | Classe que representa uma tabela | Nunca sai da API diretamente — sempre via DTO |
| **DTO** | Formato do JSON que entra/sai (o contrato com o front) | — |

**Injeção de dependência (DI):** cada classe recebe o que precisa **pelo construtor** (interfaces). Regista-se tudo em `Program.cs`. Tempo de vida: `AddScoped` (1 instância por pedido HTTP) para repositórios, services e `DbContext`.

**Unit of Work:** todos os repositórios partilham o mesmo `AppDbContext` (scoped). O Service chama `_uow.SaveChangesAsync()` uma vez no fim → tudo o que foi alterado (unidade de stock + ordem + requisição + notificação) grava **numa só transação**, ou nada grava.

## F.4 — Estrutura de pastas real do projeto

> Esta é a árvore **real** usada em todos os módulos — não existe pasta `Modules/`; a organização é por camada técnica (`Controllers/`, `Services/`, `Repositories/`, `Models/Entities`, `Models/Enums`, `Dtos/`, `Mappers/`, `Data/Configurations`, `Data/Migrations`, `Common/`), e dentro de cada uma organiza-se por módulo funcional (ex.: `Dtos/Catalogo/`, `Dtos/Atendimentos/`).

```
d:\Nova pasta\
├─ novati\                      ← front-end (já existe)
└─ backend\                     ← back-end .NET (criado no Módulo 1)
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
   ├─ swagger.json                contrato exportado (Módulo 1, tarefa S.6)
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
   └─ BackgroundServices\         SlaEscalationService (Módulo 14, extra)
```

## F.5 — Modelo de dados completo

### F.5.1 — Do estado do front (`useState`) para tabelas

O `AppContext` tem **17 coleções**. Algumas guardam listas *dentro* de outras (ex.: `ordem.historico`) — no relacional isso vira **tabelas filhas**. Total: **21 DbSets**.

| # | Estado no front | DbSet (`AppDbContext`) | Entidade | Endpoint que hidrata | Módulo dono |
|---|-----------------|------------------------|----------|----------------------|---|
| 1 | `users` | `Users` | `User` | `GET /users/directory` | 6 |
| 2 | `modelosDispositivo` | `ModelosDispositivo` | `ModeloDispositivo` | `GET /catalogo/modelos-dispositivo` | 7 |
| 3 | `modelosComponente` | `ModelosComponente` | `ModeloComponente` | `GET /catalogo/modelos-componente` | 7 |
| 4 | `compatibilidades` | `Compatibilidades` | `Compatibilidade` | `GET /catalogo/compatibilidades` | 7 |
| 5 | `localizacoes` | `Localizacoes` | `Localizacao` | `GET /dispositivos/localizacoes` | 8 |
| 6 | `dispositivosFisicos` | `DispositivosFisicos` | `DispositivoFisico` | `GET /dispositivos` | 8 |
| 7 | `instanciasComponentes` | `InstanciasComponentes` | `InstanciaComponente` | `GET /dispositivos/instancias` | 8 |
| 8 | `itensStock` | `ItensStock` | `ItemStock` | `GET /stock/itens` | 9 |
| 9 | `unidadesStock` | `UnidadesStock` | `UnidadeStock` | `GET /stock/unidades` | 9 |
| 10 | `movimentosStock` | `MovimentosStock` | `MovimentoStock` | `GET /stock/movimentos` | 9 |
| 11 | `solicitacoes` | `Solicitacoes` | `Solicitacao` | `GET /solicitacoes` | 10 |
| 12 | `ordensReparo` | `OrdensReparo` | `OrdemReparo` | `GET /atendimentos/ordens` | 11 |
| 12a | `ordem.pecasUsadas[]` | `OrdemPecasUsadas` | `OrdemPecaUsada` | (aninhado na ordem) | 11 |
| 12b | `ordem.rejeicoes[]` | `OrdemRejeicoes` | `OrdemRejeicao` | (aninhado na ordem) | 11 |
| 12c | `ordem.historico[]` | `OrdemHistoricos` | `OrdemHistorico` | (aninhado na ordem) | 11 |
| 13 | `requisicoesCompra` | `RequisicoesCompra` | `RequisicaoCompra` | `GET /compras` | 11 |
| 14 | `artigos` | `Artigos` | `Artigo` | `GET /base-conhecimento` | 10 |
| 15 | `notificacoes` | `Notificacoes` | `Notificacao` | `GET /notificacoes` | 10 |
| 16 | `mensagens` | `Mensagens` | `Mensagem` | `GET /chat/mensagens-todas` | 10 |
| 17 | `relatoriosTecnicos` | `RelatoriosTecnicos` | `RelatorioTecnico` | `GET /relatorios-tecnicos` | 12 |
| 17a | `relatorio.historico[]` | `RelatorioHistoricos` | `RelatorioHistorico` | (aninhado no relatório) | 12 |

> Mais 2 dados que **não são tabelas**: `solicitacao.avaliacao` (owned type na própria tabela) e `solicitacao.anexos[]` / `relatorio.pecasUsadas[]` (coluna JSONB).

### F.5.2 — Enums (12) — os nomes dos membros são **exatamente** o texto do front

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

> Em C# é convenção PascalCase, mas aqui **quebra-se a convenção de propósito** para que `JsonStringEnumConverter` escreva/leia o texto certo sem código extra. Explica-se num comentário no código (Módulo 2).

### F.5.3 — Entidades (21) — propriedades, tipos e restrições

Todas herdam de `BaseEntity { public Guid Id { get; set; } = Guid.NewGuid(); }`. O front trata ids como texto opaco → `Guid` serializa como string. ✅
Datas: usa-se **`DateOnly`** (coluna `date`, JSON `"2026-08-30"`). **Não usar `DateTime`** neste projeto (evita a armadilha do Npgsql exigir `DateTimeKind.Utc`).

| Entidade | Propriedades | Notas / índices | Módulo dono |
|----------|--------------|-----------------|---|
| **User** | `Nome`, `Email`, `PasswordHash`, `Role`, `Assinatura?` (text — dataURL base64) | **Email único**. `PasswordHash` nunca sai em DTO | 6 |
| **ModeloDispositivo** | `Nome`, `Fabricante`, `Tipo` | | 7 |
| **ModeloComponente** | `Nome`, `Tipo`, `Capacidade`, `StockMinimo` (int, default 1) | | 7 |
| **Compatibilidade** | `ModeloDispositivoId`, `ModeloComponenteId` | **Índice único (par)**. Tem `Id` próprio (o front apaga por id) | 7 |
| **Localizacao** | `Nome`, `PaiId?` | Auto-relação (árvore). DTO expõe `pai` | 8 |
| **DispositivoFisico** | `Patrimonio`, `ModeloDispositivoId`, `NumeroSerie`, `LocalizacaoId`, `ResponsavelId?`, `Estado`, `DataAquisicao?` (DateOnly), `GarantiaMeses?` | **Patrimonio único** | 8 |
| **InstanciaComponente** | `DispositivoFisicoId`, `ModeloComponenteId`, `Codigo`, `Estado` | | 8 |
| **ItemStock** | `ModeloComponenteId` | **Único** (1 item por modelo) | 9 |
| **UnidadeStock** | `ItemStockId`, `Codigo`, `Estado`, `ReservadaParaOrdemId?` | **Codigo único** | 9 |
| **MovimentoStock** | `ItemStockId`, `Tipo`, `Quantidade`, `Data` (DateOnly), `Observacao` | | 9 |
| **Solicitacao** | `Titulo`, `Descricao`, `Estado`, `DispositivoFisicoId?`, `SolicitanteId`, `DataCriacao`, `Prioridade`, `Categoria`, `ResolvidaViaBase` (bool), `Anexos` (JSONB `List<Anexo>`), `Avaliacao?` (owned: `Estrelas`, `Comentario`, `Data`) | | 10 |
| **OrdemReparo** | `SolicitacaoId`, `TecnicoId`, `Diagnostico`, `Solucao?`, `SolucaoSugerida?`, `Estado`, `TempoGastoMin?`, `DataInicio`, `DataFim?` | `SolicitacaoId` **único** (1–1) | 11 |
| **OrdemPecaUsada** | `OrdemId`, `UnidadeStockId`, `ModeloComponenteId`, `InstaladoInstanciaId?` | | 11 |
| **OrdemRejeicao** | `OrdemId`, `Motivo`, `Data` | | 11 |
| **OrdemHistorico** | `OrdemId`, `Tipo`, `Texto`, `AutorId?`, `Data` | DTO expõe `autor` = **nome** do autor | 11 |
| **RequisicaoCompra** | `ItemStockId`, `ModeloComponenteId`, `Quantidade`, `Justificativa`, `SolicitanteId`, `OrdemId?`, `Data`, `Estado` | | 11 |
| **Artigo** | `Titulo`, `Conteudo`, `Tags` (`List<string>` → `text[]` nativo no Postgres), `Categoria`, `AutorId` | | 10 |
| **Notificacao** | `UserId`, `Message`, `Link?`, `Lida`, `Data` | | 10 |
| **Mensagem** | `SolicitacaoId`, `AutorId`, `Texto`, `Data` (DateOnly), `Hora` (TimeOnly), `Lida` | DTO expõe `hora` como `"HH:mm"` | 10 |
| **RelatorioTecnico** | `OrdemId`, `AutorId`, `CriadoEm`, `AtualizadoEm`, `Status`, `Sumario`, `Diagnostico`, `SolucaoAplicada`, `PecasUsadas` (JSONB `List<PecaRelatorio{Nome,Quantidade,Codigo}>`), `TempoGastoMin`, `Procedimentos`, `Observacoes`, `AssinaturaTecnico`, `AssinaturaResponsavel`, `ComentariosInternos` | `OrdemId` **único** (1 relatório por ordem) | 12 |
| **RelatorioHistorico** | `RelatorioId`, `Data`, `AutorId`, `Acao`, `Campo`, `ValorAntigo`, `ValorNovo` | strings vazias, nunca `null` | 12 |

### F.5.4 — Relações e comportamento ao apagar

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

**Regra de ouro:** por defeito usa-se `Restrict` (a BD recusa apagar quem tem dependentes). Só se usa `Cascade` em "filhos que não existem sem o pai". Quando a BD recusar, o Service apanha `DbUpdateException` e devolve **409** com mensagem clara (ex.: *"Não é possível remover: o utilizador tem registos associados."*) — o front mostra essa mensagem.

## F.6 — Matriz de permissões (resumo transversal)

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

> Os **GET nunca devolvem 403** (regra de ouro nº 4): filtram. Só as **escritas** devolvem 403.

## F.7 — Erros comuns e como os evitar

| Sintoma | Causa provável | Solução |
|---|---|---|
| Front volta sempre ao login depois de entrar | Um dos 17 GET falhou (401/403/404/500) | Abre o DevTools → separador *Network* → vê qual ficou vermelho |
| "Failed to fetch" / erro CORS na consola | `UseCors` em falta ou depois de `MapControllers`; origem ≠ `http://localhost:5173`; `UseHttpsRedirection` ativo | Ordem dos middlewares (Módulo 1) |
| Mensagem "Erro 400" em vez do texto | Erro de validação com `ProblemDetails` | `InvalidModelStateResponseFactory` (Módulo 4) |
| `Unexpected end of JSON input` após ação bem-sucedida | Devolveu-se `Ok()` sem corpo | `NoContent()` (204) ou devolver o objeto |
| `403` em tudo com `[Authorize(Roles=...)]` | `MapInboundClaims`/`RoleClaimType` mal configurados | Módulo 6: `role` + `RoleClaimType = "role"` |
| `Cannot write DateTime with Kind=Unspecified` | Usou-se `DateTime` | Usar `DateOnly` / `DateTime.UtcNow` |
| `relation "X" does not exist` | Esqueceu-se `dotnet ef database update` | Correr o comando |
| Enum chega ao front como número (`0`, `1`) | Falta `JsonStringEnumConverter` | Módulo 4 |
| `A possible object cycle was detected` | Serializou-se uma **entidade** (com navegações) em vez de DTO | Devolver sempre DTO via Mapper |
| `ordem.historico is undefined` / `.map` de `null` no front | Lista `null` no DTO | Regra 5: `= []` |
| Migration falha com "column already exists" | Aplicou-se à mão SQL ou misturaram-se migrations | `dotnet ef database drop --force` e recomeçar (é dev) |
| Swagger não compila (`OpenApiReference`, `OpenApiSecurityScheme.Reference`) | Sintaxe antiga do Microsoft.OpenApi 1.x | Usar `OpenApiSecuritySchemeReference("Bearer", doc)` (Módulo 1, S.2) |
| `/swagger` dá 404 | `UseSwagger()`/`UseSwaggerUI()` fora do `if (IsDevelopment())` ou `ASPNETCORE_ENVIRONMENT` ≠ Development | Confirmar `launchSettings.json` (perfil `http` com `"ASPNETCORE_ENVIRONMENT": "Development"`) |
| Swagger mostra a action sem schema de resposta | Devolve-se `IActionResult`/objeto anónimo | `ActionResult<TDto>` + `[ProducesResponseType]` |
| Swagger: "Failed to load API definition" | Exceção ao gerar o JSON (ex.: dois controllers com a mesma rota+método, ou tipos com o mesmo nome em namespaces diferentes) | Abrir `/swagger/v1/swagger.json` diretamente — mostra o erro real |
| No Swagger UI o cadeado não envia o token | Colou-se `Bearer <token>` (duplica o prefixo) ou falta o `AddSecurityRequirement` | Colar **só** o token |
| Login funciona mas `GET /users/me` dá 401 | Header sem `Bearer ` ou `Jwt:Key`/`Issuer`/`Audience` diferentes entre gerar e validar | Comparar `appsettings` |
| Datas com `T00:00:00` | Usou-se `DateTime` no DTO | DTO com `DateOnly` |

---

# NÍVEL 2 + NÍVEL 3 — MÓDULOS

> Cada módulo abaixo segue o mesmo esqueleto de 21 secções (4.1–4.21), a lista de tarefas na ordem exacta de execução, e termina com um gate de conclusão. Quando uma secção não se aplica a um módulo (ex.: um módulo de infraestrutura pura não tem "Authorization" de domínio), isso fica dito explicitamente — não se omite a secção.

## MÓDULO 0 — Ambiente `[PLANEADO]`

### 4.1 Objetivo do módulo
Garantir que a máquina de desenvolvimento tem tudo o que os módulos seguintes vão precisar: .NET SDK, `dotnet-ef` e um servidor PostgreSQL acessível. Sem isto, nenhum comando `dotnet` ou `dotnet ef` dos módulos seguintes funciona.

### 4.2 Funcionalidades
Não há funcionalidades de API neste módulo — é puramente preparação de ambiente.

### 4.3 Dependências
Nenhuma (é o módulo inicial).

### 4.4 Dependentes
Todos os módulos seguintes (1–14) dependem deste: precisam do SDK para compilar e do PostgreSQL para ligar o `AppDbContext`.

### 4.5 Entities
Não aplicável.

### 4.6 Enums
Não aplicável.

### 4.7 Relacionamentos
Não aplicável.

### 4.8 Database
Cria-se a instância/container do PostgreSQL e a base de dados vazia `novati` (utilizador `novati`, password `novati123`). As tabelas só existem a partir do Módulo 3 (migrations).

### 4.9 DTOs
Não aplicável.

### 4.10 Validation
Não aplicável.

### 4.11 Mappers
Não aplicável.

### 4.12 Repository
Não aplicável.

### 4.13 Service
Não aplicável.

### 4.14 Dependency Injection
Não aplicável.

### 4.15 Controller
Não aplicável.

### 4.16 Authorization
Não aplicável.

### 4.17 Swagger
Não aplicável (o projeto ainda não existe).

### 4.18 Migration
Não aplicável (ainda não há projeto nem `DbContext`).

### 4.19 Tests
Teste manual de ligação: `psql` ou pgAdmin a `localhost:5432` com o utilizador `novati` e a BD `novati`.

### 4.20 Integração Front-end
Não aplicável — este módulo não expõe nada ao front.

### 4.21 Critério de conclusão
Consegue ligar-se ao Postgres (`localhost:5432`, user `novati`, BD `novati`) com uma ferramenta gráfica ou `psql`.

### NÍVEL 3 — Tarefas (ordem exacta)

1. **Verificar o SDK instalado.**
   ```
   Ação: Confirmar dotnet --version e dotnet ef --version
   Responsabilidade: garantir que o ambiente compila o projeto Web API que será criado no Módulo 1
   Depende de: nada
   É utilizado por: Módulo 1 (dotnet new webapi), todos os módulos seguintes (dotnet build)
   Resultado esperado: dotnet --version → 10.0.102; dotnet ef --version → 10.0.12
   ```
   Se o `dotnet-ef` estiver desatualizado: `dotnet tool update --global dotnet-ef`.

2. **Instalar/arrancar o PostgreSQL.** Escolher **uma** opção:
   - **Docker** (recomendado, isolado):
     ```powershell
     docker run --name novati-pg -e POSTGRES_USER=novati -e POSTGRES_PASSWORD=novati123 -e POSTGRES_DB=novati -p 5432:5432 -d postgres:17
     ```
     Depois de reiniciar o PC: `docker start novati-pg`.
   - **Instalador Windows** (postgresql.org/download) → criar a BD `novati` no pgAdmin.
   ```
   Arquivo: n/a (infraestrutura, não código)
   Responsabilidade: disponibilizar um servidor PostgreSQL em localhost:5432 com a BD novati
   Depende de: Docker instalado (opção Docker) ou instalador do postgresql.org (opção nativa)
   É utilizado por: Módulo 3 (connection string), e todos os módulos que gravam dados
   Resultado esperado: servidor Postgres a aceitar ligações em localhost:5432
   ```

3. **Instalar um cliente gráfico** (pgAdmin, ou a extensão *PostgreSQL* do VS Code) para inspecionar tabelas ao longo dos módulos seguintes.

### GATE DE CONCLUSÃO — Módulo 0

```
MÓDULO 0 — Ambiente
  ↓
[VALIDAÇÃO]
  - dotnet --version devolve 10.0.102 ou superior?
  - dotnet ef --version devolve 10.0.12 ou superior?
  - Postgres aceita ligação em localhost:5432 com user novati / BD novati?
  - Existe uma ferramenta gráfica para inspecionar tabelas?
  ↓
SIM → MÓDULO 0 CONCLUÍDO → avançar para o Módulo 1
NÃO → corrigir instalação → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 1 — Projeto + Esqueleto + Swagger `[PLANEADO]`

### 4.1 Objetivo do módulo
Criar o projeto ASP.NET Core Web API (`Novati.Api`), a estrutura de pastas definitiva (F.4), a configuração base de `Program.cs` (controllers, CORS, porta 3333), um endpoint de saúde para validar tudo, e a documentação viva (Swagger/OpenAPI) que vai servir de **contrato oficial** entre back-end e front-end a partir daqui.

### 4.2 Funcionalidades
- `GET /api/health` — endpoint anónimo de verificação.
- Swagger UI em `/swagger` com botão **Authorize** (preparado desde já, mesmo sem JWT configurado ainda — a definição de segurança fica pronta para o Módulo 6).
- Exportação do contrato (`swagger.json`) para uso na integração do Módulo 13.

### 4.3 Dependências
Módulo 0 (SDK e Postgres instalados).

### 4.4 Dependentes
Todos os módulos 2–14 — é o esqueleto onde todo o código deles vai viver. Em particular, a checklist Swagger (S.4) definida aqui é reaplicada em **todos** os controllers dos módulos 6–12.

### 4.5 Entities
Nenhuma entidade de domínio ainda (chega no Módulo 2). Não há classes de modelo neste módulo.

### 4.6 Enums
Nenhum ainda.

### 4.7 Relacionamentos
Não aplicável.

### 4.8 Database
Nenhuma tabela ainda — só a ligação (connection string) será configurada no Módulo 3.

### 4.9 DTOs
`Dtos/Common/ErrorResponse.cs` — o formato de erro usado por **toda** a API (regra de ouro nº 3):
```csharp
/// <summary>Formato de erro devolvido por toda a API.</summary>
public record ErrorResponse(int StatusCode, string Message);
```

### 4.10 Validation
Não aplicável neste módulo (chega com os DTOs de domínio a partir do Módulo 6). A infraestrutura de validação (`InvalidModelStateResponseFactory`) fica para o Módulo 4.

### 4.11 Mappers
Não aplicável.

### 4.12 Repository
Não aplicável (chega no Módulo 4, `IRepository<T>` genérico).

### 4.13 Service
Não aplicável.

### 4.14 Dependency Injection
Registo mínimo em `Program.cs`: `AddControllers()`, `AddSwaggerGen()`, `AddCors()`, `AddEndpointsApiExplorer()`. Os registos de repositórios/services vêm no Módulo 4 (`AddApplicationServices()`).

### 4.15 Controller
`Controllers/HealthController.cs` — único controller deste módulo, `[AllowAnonymous]`.

### 4.16 Authorization
Ainda não há JWT (Módulo 6). `HealthController` é anónimo. A definição de segurança "Bearer" fica registada no Swagger desde já (S.2) para não ser preciso voltar atrás.

### 4.17 Swagger
Secção central deste módulo — ver tarefas S.1 a S.6 abaixo. É aqui que se ativa XML doc, `AddSwaggerGen`, o botão Authorize, e a checklist de documentação (S.4) que **todos** os controllers dos módulos seguintes têm de seguir:
- [ ] `/// <summary>` numa frase (o que faz)
- [ ] `[ProducesResponseType]` para o **sucesso** (200/201/204) e para cada erro da tabela de contrato da fase (400, 401, 403, 404, 409)
- [ ] `[Authorize(Roles = "...")]` igual à coluna "Quem" da tabela de contrato
- [ ] `[Tags("Nome do módulo")]` no controller
- [ ] Ações que devolvem **204** → `[ProducesResponseType(StatusCodes.Status204NoContent)]`
- [ ] Retornos tipados (`ActionResult<T>`), nunca `IActionResult` a devolver objetos anónimos (o Swagger não consegue descrever o formato)

### 4.18 Migration
Nenhuma ainda (chega no Módulo 3).

### 4.19 Tests
Teste manual: `curl.exe http://localhost:3333/api/health` → `{"status":"ok"}`; abrir `http://localhost:3333/swagger` e confirmar que lista `GET /api/health`.

### 4.20 Integração Front-end
Ainda não há ligação real, mas a **porta 3333** e o **prefixo `/api`** são definidos exatamente aqui porque é o que `novati/.env` (`VITE_API_URL=http://localhost:3333/api`) espera — sem isto o front nunca vai encontrar a API, independentemente do resto do trabalho.

### 4.21 Critério de conclusão
`curl.exe http://localhost:3333/api/health` devolve `{"status":"ok"}` e `http://localhost:3333/swagger` abre, mostrando "Novati API v1" e listando `GET /api/health`.

### NÍVEL 3 — Tarefas (ordem exacta)

1. **Criar o projeto Web API.**
   ```powershell
   cd "d:\Nova pasta"
   dotnet new webapi -n Novati.Api -o backend --use-controllers
   cd backend
   dotnet new gitignore
   ```
   ```
   Arquivo: backend/Novati.Api.csproj, backend/Program.cs
   Responsabilidade: esqueleto do projeto ASP.NET Core com suporte a Controllers (não Minimal APIs)
   Depende de: Módulo 0 (SDK instalado)
   É utilizado por: todo o resto do backend
   Resultado esperado: projeto criado, dotnet build sem erros
   ```
   > `--use-controllers` é essencial: o template novo por defeito usa *Minimal APIs*; o projeto usa **Controllers**.

2. **Instalar os pacotes NuGet.**
   ```powershell
   # Bibliotecas de dados
   dotnet add package Microsoft.EntityFrameworkCore
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   dotnet add package Microsoft.EntityFrameworkCore.Design

   # Segurança
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   dotnet add package BCrypt.Net-Next

   # Documentação/teste interativo
   dotnet add package Swashbuckle.AspNetCore
   ```
   ```
   Arquivo: backend/Novati.Api.csproj
   Responsabilidade: trazer EF Core + Npgsql (Módulo 2–3), JWT + BCrypt (Módulo 6), Swashbuckle (este módulo)
   Depende de: tarefa 1
   É utilizado por: Módulos 2, 3, 4, 6 em diante
   Resultado esperado: dotnet build continua sem erros; sem --version, o dotnet add package escolhe a versão compatível com net10.0
   ```

3. **Limpar o template.** Apagar `WeatherForecast.cs` e `Controllers/WeatherForecastController.cs`. Remover o `AddOpenApi()`/`MapOpenApi()` gerado pelo template (o Swashbuckle substitui-os); pode remover-se o pacote `Microsoft.AspNetCore.OpenApi` do `.csproj`.
   ```
   CRIAR: nada
   ALTERAR: Program.cs (remover AddOpenApi/MapOpenApi)
   REMOVER: WeatherForecast.cs, Controllers/WeatherForecastController.cs
   ```

4. **Criar a estrutura de pastas** definida em F.4 (`Controllers/`, `Services/Interfaces/`, `Repositories/Interfaces/`, `Models/Entities/`, `Models/Enums/`, `Dtos/`, `Mappers/`, `Data/Configurations/`, `Data/Migrations/`, `Data/Seed/`, `Common/Exceptions/`, `Common/Middleware/`, `Common/Extensions/`, `Common/Settings/`, `BackgroundServices/`).

5. **Configurar a porta 3333.** Em `Properties/launchSettings.json`, no perfil `http`: `"applicationUrl": "http://localhost:3333"`.
   ```
   Arquivo: Properties/launchSettings.json
   Responsabilidade: fazer a API correr na porta que novati/.env espera (VITE_API_URL=http://localhost:3333/api)
   Depende de: tarefa 1
   É utilizado por: F.1 (contrato de porta), Módulo 13 (ligação ao front)
   Resultado esperado: dotnet run arranca em http://localhost:3333
   ```

6. **Criar `Controllers/HealthController.cs`.**
   ```csharp
   [ApiController]
   [Route("api/health")]
   [AllowAnonymous]
   public class HealthController : ControllerBase
   {
       [HttpGet] public IActionResult Get() => Ok(new { status = "ok" });
   }
   ```
   ```
   Arquivo: Controllers/HealthController.cs
   Responsabilidade: endpoint de verificação simples, sem autenticação
   Depende de: tarefa 1
   É utilizado por: teste manual deste módulo; scripts de verificação do Módulo 13
   Resultado esperado: GET /api/health → 200 {"status":"ok"}
   ```

7. **Configurar `Program.cs` base.**
   ```csharp
   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddControllers();
   builder.Services.AddSwaggerGen();         // configuração completa nas tarefas S.1–S.6 abaixo

   builder.Services.AddCors(o => o.AddPolicy("front", p => p
       .WithOrigins("http://localhost:5173", "http://localhost:4173")
       .AllowAnyHeader().AllowAnyMethod()));

   var app = builder.Build();

   if (app.Environment.IsDevelopment())
   {
       app.UseSwagger();
       app.UseSwaggerUI();
   }

   app.UseCors("front");
   app.MapControllers();
   app.Run();
   ```
   ```
   Arquivo: Program.cs
   Responsabilidade: pipeline mínimo de middlewares — CORS antes de MapControllers (Authentication/Authorization entram no Módulo 6)
   Depende de: tarefas 1, 6
   É utilizado por: todo o pipeline HTTP da aplicação
   Resultado esperado: dotnet run serve GET /api/health e /swagger
   ```
   ⚠️ Ordem dos middlewares importa: `UseCors` → (`UseAuthentication` → `UseAuthorization`, a partir do Módulo 6) → `MapControllers`.
   ⚠️ **Remover** `app.UseHttpsRedirection()` se o template o incluir — o front chama `http://`, e o redirect para https parte o CORS.

8. **Ativar o XML de comentários** (`Novati.Api.csproj`, dentro de `<PropertyGroup>`):
   ```xml
   <GenerateDocumentationFile>true</GenerateDocumentationFile>
   <NoWarn>$(NoWarn);1591</NoWarn>   <!-- 1591 = "falta comentário XML" -->
   ```
   ```
   Arquivo: Novati.Api.csproj
   Responsabilidade: permitir que os comentários /// dos controllers apareçam no Swagger
   Depende de: tarefa 1
   É utilizado por: tarefa 9 (AddSwaggerGen → IncludeXmlComments)
   Resultado esperado: gerado Novati.Api.xml em bin/ no build
   ```

9. **Configurar o Swagger completo no `Program.cs`** — substitui o `AddSwaggerGen()` simples da tarefa 7:
   ```csharp
   using System.Reflection;
   using Microsoft.OpenApi;

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

       var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
       if (File.Exists(xml)) o.IncludeXmlComments(xml);

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

   // depois do builder.Build(), dentro do if (app.Environment.IsDevelopment()):
   app.UseSwagger();
   app.UseSwaggerUI(o =>
   {
       o.EnablePersistAuthorization();
       o.DocumentTitle = "Novati API";
   });
   ```
   ```
   Arquivo: Program.cs
   Responsabilidade: gerar o /swagger/v1/swagger.json e servir a UI em /swagger, com botão Authorize pronto para JWT
   Depende de: tarefa 8
   É utilizado por: todos os controllers dos módulos 6–12 (documentação); Módulo 13 (exportação do contrato)
   Resultado esperado: /swagger abre, mostra "Novati API v1"
   ```
   > Versão validada: `Swashbuckle.AspNetCore 10.2.3`, usa `Microsoft.OpenApi 2.x` — a sintaxe acima é a atual; tutoriais antigos com `OpenApiReference` **não compilam**.
   > ⚠️ O Swagger só deve estar ativo em **Development** (já está garantido pelo `if`). Em produção expõe a estrutura da API.
   > ⚠️ Ao chegar ao Módulo 6 (fallback policy de autenticação), confirmar que `/swagger` continua a abrir **sem token** — a UI é servida por middleware, não por controller, por isso normalmente não é afetada.

10. **Confirmar enums em texto no Swagger** (`JsonStringEnumConverter`, configurado só no Módulo 4) — verificação a repetir nessa altura: se aparecerem números (`0,1,2`) no schema em vez de texto, falta o conversor.

11. **Aplicar a checklist de documentação (S.4)** ao `HealthController` como exemplo mínimo, e reutilizar o exemplo completo (`AuthController`, Módulo 6) como referência para todos os módulos seguintes:
    ```csharp
    /// <summary>Autenticação.</summary>
    [ApiController]
    [Route("api/auth")]
    [Tags("Autenticação")]
    [Produces("application/json")]
    public class AuthController(IAuthService auth) : ControllerBase
    {
        /// <summary>Inicia sessão e devolve o JWT.</summary>
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
    E nos DTOs, documentar campos e restrições com `///` e `<example>` (preenche automaticamente o botão *Try it out*).

12. **Criar `Dtos/Common/ErrorResponse.cs`** (secção 4.9 acima) — é o formato do middleware de erro que será escrito no Módulo 4.

13. **Testar com o Swagger UI:**
    1. `dotnet run` → abrir **http://localhost:3333/swagger**.
    2. Confirmar que `GET /api/health` aparece listado e responde `{"status":"ok"}` via *Try it out*.
    3. (A partir do Módulo 6) fazer login, copiar `accessToken`, usar o botão **Authorize**.

14. **Exportar o contrato** (repetir sempre que a API mudar; usar-se-á definitivamente no Módulo 13):
    ```powershell
    curl.exe http://localhost:3333/swagger/v1/swagger.json -o "d:\Nova pasta\backend\swagger.json"
    ```

### GATE DE CONCLUSÃO — Módulo 1

```
MÓDULO 1 — Projeto + Esqueleto + Swagger
  ↓
[VALIDAÇÃO]
  - dotnet build sem erros?
  - dotnet run arranca em http://localhost:3333?
  - GET /api/health devolve 200 {"status":"ok"}?
  - /swagger abre e lista GET /api/health?
  - Estrutura de pastas de F.4 criada?
  - CORS permite http://localhost:5173?
  ↓
SIM → MÓDULO 1 CONCLUÍDO → avançar para o Módulo 2
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 2 — Enums + Entidades `[PLANEADO]`

### 4.1 Objetivo do módulo
Modelar em C# o domínio completo do helpdesk: os 12 enums e as 21 entidades (F.5.2, F.5.3), com as duas pontas de cada relação declaradas, para que o Módulo 3 consiga gerar o `AppDbContext` e as migrations a partir delas.

### 4.2 Funcionalidades
Não há endpoints neste módulo — é puramente modelação de classes.

### 4.3 Dependências
Módulo 1 (projeto criado com a pasta `Models/Entities/` e `Models/Enums/`).

### 4.4 Dependentes
Módulo 3 (DbContext precisa destas classes para os `DbSet<T>`), e todos os módulos de domínio (6–12), cujos Services/Repositories/Mappers trabalham sobre estas entidades.

### 4.5 Entities
As 21 entidades completas de F.5.3: `User`, `ModeloDispositivo`, `ModeloComponente`, `Compatibilidade`, `Localizacao`, `DispositivoFisico`, `InstanciaComponente`, `ItemStock`, `UnidadeStock`, `MovimentoStock`, `Solicitacao`, `OrdemReparo`, `OrdemPecaUsada`, `OrdemRejeicao`, `OrdemHistorico`, `RequisicaoCompra`, `Artigo`, `Notificacao`, `Mensagem`, `RelatorioTecnico`, `RelatorioHistorico`. Mais `BaseEntity` (classe base) e as classes auxiliares não-tabela: `Anexo`, `Avaliacao`, `PecaRelatorio`.

### 4.6 Enums
Os 12 enums de F.5.2, criados em `Models/Enums/`.

### 4.7 Relacionamentos
Todos os relacionamentos de F.5.4, declarados **nas duas pontas** (propriedade FK + propriedade de navegação em cada classe envolvida). Exemplo:
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
A configuração Fluent API (índices, `OnDelete`) só é escrita no Módulo 3 — aqui só se declaram as propriedades C#.

### 4.8 Database
Ainda nenhuma tabela real (isso é o Módulo 3). Este módulo só produz as classes que **vão** virar tabelas.

### 4.9 DTOs
Não aplicável — DTOs entram por módulo de domínio (6 em diante).

### 4.10 Validation
Não aplicável a entidades (a validação de entrada é feita nos DTOs, com Data Annotations, a partir do Módulo 6).

### 4.11 Mappers
Não aplicável ainda.

### 4.12 Repository
Não aplicável ainda (Módulo 4).

### 4.13 Service
Não aplicável ainda.

### 4.14 Dependency Injection
Não aplicável.

### 4.15 Controller
Não aplicável.

### 4.16 Authorization
Não aplicável.

### 4.17 Swagger
Não aplicável (nenhum endpoint expõe estas classes diretamente — nunca seriam expostas como entidade de qualquer forma, regra da camada Entity em F.3).

### 4.18 Migration
Preparação para o Módulo 3: é a partir destas classes que a primeira migration (`CreateInitial`) será gerada.

### 4.19 Tests
`dotnet build` sem erros é o único teste possível neste módulo (não há comportamento a testar, só estrutura de dados).

### 4.20 Integração Front-end
Indireta: os nomes dos enums (`ABERTA`, `EM_DIAGNOSTICO`, `rascunho`...) têm de bater certo, caractere a caractere, com o que o front compara em `estado === 'ABERTA'` — ver regra de ouro nº 6 (F.2).

### 4.21 Critério de conclusão
`dotnet build` sem erros, com os 12 enums e as 21 entidades + 3 classes auxiliares criadas, todas as listas inicializadas a `[]` e strings a `""`.

### NÍVEL 3 — Tarefas (ordem exacta)

1. **Criar os 12 enums** em `Models/Enums/` (Role, Prioridade, EstadoSolicitacao, EstadoOrdem, EstadoDispositivo, EstadoInstancia, EstadoUnidade, TipoMovimento, EstadoRequisicao, TipoHistoricoOrdem, StatusRelatorio, AcaoRelatorio) — conteúdo exato em F.5.2.
   ```
   Arquivo: Models/Enums/Enums.cs (ou 1 ficheiro por enum, à escolha)
   Responsabilidade: representar os estados de domínio com o texto exato que o front usa
   Depende de: Módulo 1
   É utilizado por: todas as entidades que têm uma propriedade de enum; JsonStringEnumConverter (Módulo 4)
   Resultado esperado: dotnet build sem erros
   ```
   > Em C# é convenção PascalCase, mas aqui quebra-se a convenção de propósito para que `JsonStringEnumConverter` escreva/leia o texto certo sem código extra — comentar isto no código.

2. **Criar `BaseEntity`** em `Models/Entities/BaseEntity.cs`:
   ```csharp
   public abstract class BaseEntity { public Guid Id { get; set; } = Guid.NewGuid(); }
   ```
   ```
   Arquivo: Models/Entities/BaseEntity.cs
   Responsabilidade: chave primária comum a todas as tabelas (Guid, serializa como string — o front trata ids como texto opaco)
   Depende de: tarefa 1 (nenhuma, na verdade — é independente dos enums)
   É utilizado por: as 21 entidades
   Resultado esperado: dotnet build sem erros
   ```

3. **Criar as classes auxiliares não-tabela**: `Anexo { Nome, Tipo, DataUrl }`, `Avaliacao { Estrelas, Comentario, Data }`, `PecaRelatorio { Nome, Quantidade, Codigo }` — usadas como owned types/JSONB dentro de `Solicitacao` e `RelatorioTecnico`.

4. **Criar as 21 entidades** em `Models/Entities/`, uma classe por entidade, cada uma herdando de `BaseEntity`, com **as duas pontas** de cada relação (FK + navegação), listas inicializadas a `= []`, strings a `= ""`, e propriedades de data como `DateOnly`/`TimeOnly` (nunca `DateTime`, para evitar a exigência do Npgsql de `DateTimeKind.Utc`).
   ```
   Arquivo: Models/Entities/User.cs, ModeloDispositivo.cs, ModeloComponente.cs, Compatibilidade.cs,
            Localizacao.cs, DispositivoFisico.cs, InstanciaComponente.cs, ItemStock.cs, UnidadeStock.cs,
            MovimentoStock.cs, Solicitacao.cs, OrdemReparo.cs, OrdemPecaUsada.cs, OrdemRejeicao.cs,
            OrdemHistorico.cs, RequisicaoCompra.cs, Artigo.cs, Notificacao.cs, Mensagem.cs,
            RelatorioTecnico.cs, RelatorioHistorico.cs
   Responsabilidade: representar as 21 tabelas do domínio (ver F.5.3 para propriedades exatas de cada uma)
   Depende de: tarefas 1, 2, 3
   É utilizado por: Módulo 3 (AppDbContext/DbSet<T>), Repositories, Services, Mappers de todos os módulos de domínio
   Resultado esperado: dotnet build sem erros; nenhuma lista nula; nenhuma string nula
   ```

5. **Validar com `dotnet build`** que todas as referências cruzadas (FK ⇄ navegação) compilam.

### GATE DE CONCLUSÃO — Módulo 2

```
MÓDULO 2 — Enums + Entidades
  ↓
[VALIDAÇÃO]
  - dotnet build sem erros?
  - Os 12 enums têm exatamente os nomes de membro do front?
  - As 21 entidades existem em Models/Entities/?
  - Cada relação tem FK + navegação nas duas pontas?
  - Todas as listas = [] e strings = "" por omissão?
  ↓
SIM → MÓDULO 2 CONCLUÍDO → avançar para o Módulo 3
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 3 — DbContext + Migrations `[PLANEADO]`

### 4.1 Objetivo do módulo
Ligar as 21 entidades ao PostgreSQL: criar o `AppDbContext` com os 21 `DbSet<T>`, escrever a configuração Fluent API de cada entidade (índices únicos, `OnDelete`, owned types/JSONB), configurar a connection string, e gerar + aplicar a primeira migration para que as tabelas existam fisicamente na BD.

### 4.2 Funcionalidades
Nenhum endpoint HTTP — este módulo entrega a camada de persistência que os módulos seguintes (Repository, Service) vão consumir.

### 4.3 Dependências
Módulo 2 (entidades e enums já existem). Módulo 0 (Postgres a correr).

### 4.4 Dependentes
Módulo 4 (`Repository<T>` opera sobre `AppDbContext`), Módulo 5 (seed grava através do `AppDbContext`), e todos os módulos de domínio (6–12).

### 4.5 Entities
As mesmas 21 do Módulo 2 — aqui ganham a sua representação em tabela via `DbSet<T>` e `IEntityTypeConfiguration<T>`.

### 4.6 Enums
Configuração de conversão: todos os 12 enums são guardados como **texto** na BD (`HaveConversion<string>()`), não como inteiro — legível no pgAdmin e estável se a ordem dos membros mudar.

### 4.7 Relacionamentos
Todos os relacionamentos de F.5.4 ganham aqui a sua configuração Fluent API exata: `HasOne(...).WithMany(...).HasForeignKey(...).OnDelete(...)`.

### 4.8 Database
Este módulo **cria** as 21 tabelas físicas + `__EFMigrationsHistory` na BD `novati`, através da primeira migration.

### 4.9 DTOs
Não aplicável.

### 4.10 Validation
Não aplicável a este módulo (validação de índices únicos é feita pela BD via `HasIndex(...).IsUnique()`, não por Data Annotations).

### 4.11 Mappers
Não aplicável.

### 4.12 Repository
Não aplicável ainda (Módulo 4) — mas o `AppDbContext` criado aqui é a dependência direta do `Repository<T>` genérico do Módulo 4.

### 4.13 Service
Não aplicável.

### 4.14 Dependency Injection
`builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString))` em `Program.cs`.

### 4.15 Controller
Não aplicável.

### 4.16 Authorization
Não aplicável.

### 4.17 Swagger
Não aplicável (o DbContext não é exposto por nenhum endpoint).

### 4.18 Migration
Secção central deste módulo — ver tarefas 6–7 abaixo. É aqui que se aprendem os comandos `dotnet ef migrations add` / `dotnet ef database update` que serão reutilizados sempre que uma entidade mudar, nos módulos seguintes.

### 4.19 Tests
Verificação no pgAdmin: as 21 tabelas + `__EFMigrationsHistory` existem; `Solicitacoes` tem coluna `Anexos` (jsonb) e colunas `Avaliacao_*`; `Artigos.Tags` é `text[]`.

### 4.20 Integração Front-end
Indireta: os tipos de coluna escolhidos aqui (`DateOnly` → `date`, enums → texto, JSONB para listas aninhadas) são o que garante que os DTOs dos módulos seguintes conseguem produzir exatamente o JSON que o front espera (regras de ouro 5 e 6).

### 4.21 Critério de conclusão
No pgAdmin aparecem as 21 tabelas + `__EFMigrationsHistory`; `Solicitacoes.Anexos` é `jsonb`; `Solicitacoes` tem colunas `Avaliacao_*`; `Artigos.Tags` é `text[]`.

### NÍVEL 3 — Tarefas (ordem exacta)

1. **Criar `Data/AppDbContext.cs`** com os 21 `DbSet<T>` e `OnModelCreating` a aplicar todas as configurações via `ApplyConfigurationsFromAssembly`:
   ```csharp
   public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
   {
       public DbSet<User> Users => Set<User>();
       // ... os restantes 20 DbSet<T> (ver F.5.1 para a lista completa)

       protected override void OnModelCreating(ModelBuilder mb)
           => mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

       protected override void ConfigureConventions(ModelConfigurationBuilder cb)
       {
           cb.Properties<Role>().HaveConversion<string>();
           cb.Properties<Prioridade>().HaveConversion<string>();
           cb.Properties<EstadoSolicitacao>().HaveConversion<string>();
           // ... uma linha por enum (os 12 de F.5.2)
       }
   }
   ```
   ```
   Arquivo: Data/AppDbContext.cs
   Responsabilidade: ponto de entrada do EF Core — expõe as 21 tabelas e garante que todos os enums gravam como texto
   Depende de: Módulo 2 (as 21 entidades + 12 enums)
   É utilizado por: Módulo 4 (Repository<T>), Módulo 5 (Seed), todos os Services dos módulos 6–12
   Resultado esperado: dotnet build sem erros
   ```

2. **Criar `Data/Configurations/*.cs`** — 1 classe `IEntityTypeConfiguration<T>` por entidade, com `HasKey`, `HasMaxLength`, `HasIndex(...).IsUnique()`, `HasOne(...).WithMany(...).HasForeignKey(...).OnDelete(...)` (tabela F.5.4). Casos-chave a não esquecer:
   - `UserConfiguration`: `HasIndex(u => u.Email).IsUnique()`
   - `SolicitacaoConfiguration`: `OwnsOne(s => s.Avaliacao)` e `OwnsMany(s => s.Anexos, b => b.ToJson())`
   - `RelatorioTecnicoConfiguration`: `OwnsMany(r => r.PecasUsadas, b => b.ToJson())`
   - `LocalizacaoConfiguration`: `HasOne(l => l.Pai).WithMany(l => l.Filhos).HasForeignKey(l => l.PaiId).OnDelete(DeleteBehavior.Restrict)`
   - `OrdemReparoConfiguration`: `HasOne(o => o.Solicitacao).WithOne(s => s.Ordem).HasForeignKey<OrdemReparo>(o => o.SolicitacaoId)` + índice único
   ```
   Arquivo: Data/Configurations/UserConfiguration.cs, ModeloDispositivoConfiguration.cs, ... (21 ficheiros, 1 por entidade)
   Responsabilidade: centralizar as regras de persistência fora das entidades (índices únicos, OnDelete, owned types/JSONB)
   Depende de: tarefa 1
   É utilizado por: AppDbContext.OnModelCreating (via ApplyConfigurationsFromAssembly)
   Resultado esperado: dotnet build sem erros; migration gerada reflete os índices/relações da tabela F.5.4
   ```

3. **Configurar `appsettings.Development.json`:**
   ```json
   {
     "ConnectionStrings": {
       "Default": "Host=localhost;Port=5432;Database=novati;Username=novati;Password=novati123"
     },
     "Jwt": { "Key": "troca-isto-por-uma-chave-com-pelo-menos-32-caracteres!", "Issuer": "novati", "Audience": "novati-front", "ExpiresMinutes": 480 }
   }
   ```
   > A secção `Jwt` já fica pronta aqui para o Módulo 6 não precisar de voltar a este ficheiro.

4. **Registar o `AppDbContext` em `Program.cs`:**
   ```csharp
   builder.Services.AddDbContext<AppDbContext>(o =>
       o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
   ```

5. **Gerar a migration inicial e aplicar à BD:**
   ```powershell
   dotnet ef migrations add CreateInitial -o Data/Migrations
   dotnet ef database update
   ```
   ```
   Arquivo: Data/Migrations/<timestamp>_CreateInitial.cs (+ .Designer.cs, AppDbContextModelSnapshot.cs)
   Responsabilidade: traduzir o modelo C# (entidades + configurações) em SQL que cria as 21 tabelas no Postgres
   Depende de: tarefas 1, 2, 3, 4
   É utilizado por: dotnet ef database update (aplica); Módulo 5 (grava dados nas tabelas criadas)
   Resultado esperado: 21 tabelas + __EFMigrationsHistory visíveis no pgAdmin
   ```
   Ler sempre a migration gerada antes de aplicar: é assim que se aprende o SQL por trás.

6. **Memorizar os comandos de manutenção de migrations** (vão ser usados em quase todos os módulos seguintes, sempre que uma entidade mudar):
   ```powershell
   dotnet ef migrations add NomeDaAlteracao -o Data/Migrations   # nova migration depois de mudar uma entidade
   dotnet ef migrations list                                     # ver quais estão aplicadas
   dotnet ef migrations remove                                   # desfazer a ÚLTIMA migration (só se ainda não aplicada)
   dotnet ef database update NomeDaMigrationAnterior             # voltar atrás na BD
   dotnet ef database drop --force                               # apagar a BD toda (dev: recomeçar do zero)
   dotnet ef migrations script -o script.sql                     # gerar o SQL (para ver o que o EF faz)
   dotnet ef dbcontext info                                      # confirmar provider e connection string
   ```
   Fluxo de trabalho: **mudar entidade/configuração → `migrations add` → ler o ficheiro gerado → `database update`**.
   Se algo correr mal numa migration **já aplicada**: `database update <anterior>` → `migrations remove` → corrigir → `migrations add` de novo.

7. **Verificar no pgAdmin** que as 21 tabelas + `__EFMigrationsHistory` existem, e que os tipos de coluna especiais (jsonb, text[], date) estão corretos.

### GATE DE CONCLUSÃO — Módulo 3

```
MÓDULO 3 — DbContext + Migrations
  ↓
[VALIDAÇÃO]
  - dotnet build sem erros?
  - dotnet ef migrations add CreateInitial gera migration sem avisos estranhos?
  - dotnet ef database update aplica sem erros?
  - As 21 tabelas + __EFMigrationsHistory existem no pgAdmin?
  - Solicitacoes.Anexos é jsonb? Artigos.Tags é text[]? Datas são date (não timestamp)?
  - Índices únicos (Email, Patrimonio, Codigo, SolicitacaoId, OrdemId) existem?
  ↓
SIM → MÓDULO 3 CONCLUÍDO → avançar para o Módulo 4
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 4 — Base Transversal: Erros, JSON, Repository, Unit of Work `[PLANEADO]`

### 4.1 Objetivo do módulo
Construir a infraestrutura que **todos** os módulos de domínio (6–12) vão reutilizar: exceções de domínio, o middleware que as transforma no formato de erro do contrato (regra de ouro nº 3), o conversor de enums para texto no JSON (regra nº 6), o repositório genérico e o Unit of Work.

### 4.2 Funcionalidades
Nenhum endpoint de domínio — só infraestrutura. Um endpoint de teste temporário (`throw new NotFoundException(...)`) é criado e depois apagado, só para validar o middleware.

### 4.3 Dependências
Módulo 3 (`AppDbContext` pronto — o `Repository<T>` e o `UnitOfWork` operam sobre ele).

### 4.4 Dependentes
Todos os módulos de domínio (6–12): cada `Service` lança as exceções daqui, cada `Repository` específico herda/usa o `Repository<T>` genérico, e cada `Service` chama `_uow.SaveChangesAsync()`.

### 4.5 Entities
Não aplicável (não são entidades de domínio).

### 4.6 Enums
Não aplicável — mas é aqui que se regista o `JsonStringEnumConverter` que faz os 12 enums da F.5.2 serializarem como texto no JSON (e não como número).

### 4.7 Relacionamentos
Não aplicável.

### 4.8 Database
Não aplicável (nenhuma tabela nova).

### 4.9 DTOs
`ErrorResponse` já foi criado no Módulo 1 (`Dtos/Common/ErrorResponse.cs`) — é reutilizado aqui pelo middleware.

### 4.10 Validation
Configuração global de erros de validação de DTOs (`[Required]`, `[EmailAddress]`, `[Range]`) via `ConfigureApiBehaviorOptions`, para que o formato bata com o contrato (regra nº 3) em vez do `ProblemDetails` por defeito do .NET.

### 4.11 Mappers
Não aplicável (mappers são por entidade, a partir do Módulo 6).

### 4.12 Repository
Secção central deste módulo: `Repositories/Interfaces/IRepository<T>` (`GetByIdAsync`, `GetAllAsync`, `AddAsync`, `Update`, `Remove`, `ExistsAsync`) e a implementação genérica `Repository<T>(AppDbContext ctx)` sobre `ctx.Set<T>()`. Leituras usam `.AsNoTracking()` quando não vão ser alteradas.

### 4.13 Service
Não aplicável ainda (services concretos a partir do Módulo 6) — mas a convenção "`SaveChanges` uma vez por caso de uso, através do `IUnitOfWork`" é definida aqui.

### 4.14 Dependency Injection
`Common/Extensions/ServiceCollectionExtensions.cs` com `AddApplicationServices()`, onde cada par `AddScoped<IX, X>()` de repositório/service será registado (uma linha por par, à medida que os módulos seguintes os criam). Em `Program.cs`: `builder.Services.AddApplicationServices();`.

### 4.15 Controller
Não aplicável (só o endpoint de teste temporário da tarefa 6, apagado no final).

### 4.16 Authorization
Não aplicável ainda (chega no Módulo 6).

### 4.17 Swagger
Não aplicável diretamente — mas o `ErrorResponse` documentado aqui é o tipo usado em todos os `[ProducesResponseType(typeof(ErrorResponse), ...)]` dos módulos seguintes.

### 4.18 Migration
Nenhuma (nenhuma entidade nova).

### 4.19 Tests
Endpoint de teste temporário que faz `throw new NotFoundException("teste")` → confirma `404 {"statusCode":404,"message":"teste"}`. Remover depois de validar.

### 4.20 Integração Front-end
Direta: é este módulo que garante as regras de ouro nº 3 (formato de erro), nº 5 (via mappers/DTOs dos módulos seguintes, mas o `JsonStringEnumConverter` regista-se aqui) e nº 6 (enums como texto).

### 4.21 Critério de conclusão
Um endpoint de teste que faça `throw new NotFoundException("teste")` devolve `404 {"statusCode":404,"message":"teste"}`. (Apagar o endpoint de teste depois.)

### NÍVEL 3 — Tarefas (ordem exacta)

1. **Criar as exceções de domínio** em `Common/Exceptions/`: `NotFoundException` (404), `ForbiddenException` (403), `ConflictException` (409), `BusinessRuleException` (400), `UnauthorizedException` (401) — todas com propriedade `Message`.
   ```
   Arquivo: Common/Exceptions/NotFoundException.cs, ForbiddenException.cs, ConflictException.cs,
            BusinessRuleException.cs, UnauthorizedException.cs
   Responsabilidade: permitir que os Services expressem erros de negócio sem conhecer HTTP
   Depende de: nada (classes puras)
   É utilizado por: todos os Services dos módulos 6–12; ExceptionHandlingMiddleware (tarefa 2)
   Resultado esperado: dotnet build sem erros
   ```

2. **Criar `Common/Middleware/ExceptionHandlingMiddleware.cs`**: `try { await next(ctx); } catch (...)` → mapeia cada exceção da tarefa 1 para o status HTTP correspondente e escreve `{ "statusCode": N, "message": "..." }`. Exceções desconhecidas → `500` com mensagem genérica *"Erro interno do servidor."* (e log do erro real — nunca expor o stack trace ao cliente).
   ```
   Arquivo: Common/Middleware/ExceptionHandlingMiddleware.cs
   Responsabilidade: converter qualquer exceção lançada por um Service no formato de erro do contrato (regra de ouro nº 3)
   Depende de: tarefa 1
   É utilizado por: registado em Program.cs, antes de MapControllers
   Resultado esperado: qualquer exceção não tratada devolve JSON { statusCode, message }, nunca um stack trace
   ```
   Registar em `Program.cs`: `app.UseMiddleware<ExceptionHandlingMiddleware>();` **antes** de `MapControllers`.

3. **Configurar erros de validação de modelo** em `Program.cs`:
   ```csharp
   builder.Services.AddControllers()
     .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
     .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = ctx =>
     {
         var msgs = ctx.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
         return new BadRequestObjectResult(new { statusCode = 400, message = msgs });
     });
   ```
   ```
   Arquivo: Program.cs
   Responsabilidade: (a) fazer os enums serializarem como texto (regra nº 6); (b) formatar erros de [Required]/[EmailAddress]/[Range] no formato do contrato
   Depende de: tarefa 2
   É utilizado por: todos os DTOs de request dos módulos 6–12
   Resultado esperado: um DTO inválido devolve 400 { statusCode: 400, message: ["..."] }; enums em texto no JSON
   ```

4. **Criar `Repositories/Interfaces/IRepository<T>`** e a implementação genérica `Repository<T>`:
   ```
   Arquivo: Repositories/Interfaces/IRepository.cs, Repositories/Repository.cs
   Responsabilidade: operações CRUD genéricas reutilizáveis por todos os repositórios específicos (Módulos 6–12)
   Depende de: Módulo 3 (AppDbContext)
   É utilizado por: IUserRepository, ICatalogoRepository, ... (todos herdam ou compõem Repository<T>)
   Resultado esperado: GetByIdAsync/GetAllAsync/AddAsync/Update/Remove/ExistsAsync funcionam sobre qualquer entidade
   ```

5. **Criar `IUnitOfWork` e `UnitOfWork(AppDbContext ctx)`** com `Task<int> SaveChangesAsync(CancellationToken ct = default)`.
   ```
   Arquivo: Data/UnitOfWork.cs, Repositories/Interfaces/IUnitOfWork.cs
   Responsabilidade: garantir que várias alterações de um caso de uso (ex.: reservar peça) gravam numa só transação
   Depende de: Módulo 3 (AppDbContext)
   É utilizado por: todos os Services dos módulos 6–12, sempre no fim do método, uma única vez
   Resultado esperado: um Service que altera 3 entidades diferentes e chama SaveChangesAsync() uma vez grava tudo ou nada
   ```

6. **Criar `Common/Extensions/ServiceCollectionExtensions.cs`** com `AddApplicationServices()`; registar `IRepository<T>`/`Repository<T>` e `IUnitOfWork`/`UnitOfWork` como `AddScoped`. Chamar `builder.Services.AddApplicationServices();` em `Program.cs`.

7. **Testar com um endpoint temporário**: criar uma action que faça `throw new NotFoundException("teste");`, confirmar `404 {"statusCode":404,"message":"teste"}`, depois **apagar** o endpoint de teste.

### GATE DE CONCLUSÃO — Módulo 4

```
MÓDULO 4 — Base Transversal
  ↓
[VALIDAÇÃO]
  - As 5 exceções de domínio existem e compilam?
  - ExceptionHandlingMiddleware devolve {statusCode,message} para uma NotFoundException de teste?
  - Um DTO inválido devolve 400 {statusCode:400, message:[...]}?
  - Enums aparecem como texto no JSON de resposta (não número)?
  - IRepository<T>/Repository<T> compilam e funcionam sobre uma entidade de teste?
  - IUnitOfWork.SaveChangesAsync() grava alterações em várias entidades numa só transação?
  - Endpoint de teste temporário foi removido?
  ↓
SIM → MÓDULO 4 CONCLUÍDO → avançar para o Módulo 5
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 5 — Seed (dados de demonstração) `[PLANEADO]`

### 4.1 Objetivo do módulo
Popular a BD com os mesmos dados de demonstração que o front-end já assume (`novati/src/data/seed.js` + `seedRelatorios.js`). Sem seed não há utilizadores → não há login → o front não arranca, e os testes E2E (`integracao.spec.js`) dependem exatamente destes dados.

### 4.2 Funcionalidades
Nenhum endpoint — é um processo de arranque (`DbSeeder.SeedAsync`) que corre uma vez, condicionalmente, quando a app inicia em Development.

### 4.3 Dependências
Módulo 3 (tabelas criadas), Módulo 4 (nenhuma dependência direta forte, mas segue a mesma convenção de `SaveChangesAsync`).

### 4.4 Dependentes
Módulo 6 (login só funciona com utilizadores semeados), e todos os módulos de domínio (7–12), cujos "✅ Feito quando" comparam contagens/ids com os dados do seed. Módulo 13 (os testes E2E assumem estes dados).

### 4.5 Entities
Todas as 21, por esta ordem de inserção (respeita as FKs): `Users` → `ModelosDispositivo` → `ModelosComponente` → `Compatibilidades` → `Localizacoes` (pais primeiro: Financeiro, depois Sala 2) → `DispositivosFisicos` → `InstanciasComponentes` → `ItensStock` → `Solicitacoes` → `OrdensReparo` (+`PecasUsadas`, `Rejeicoes`, `Historico`) → `UnidadesStock` (a `us4` fica reservada para `or1`, por isso vem depois) → `MovimentosStock` → `Artigos` → `Mensagens` (converter `hora: '09:15'` → `TimeOnly`) → `RelatoriosTecnicos` (+`Historico`).

### 4.6 Enums
Todos usados nos valores semeados (ex.: `Role.ADMIN`, `EstadoSolicitacao.ABERTA`, `StatusRelatorio.rascunho`).

### 4.7 Relacionamentos
Os ids do front (`u1`, `md1`, `s1`...) **não são Guid** → usa-se um `Dictionary<string, Guid>` (`ids["u1"] = Guid.NewGuid()`) para ligar as relações durante o seed.

### 4.8 Database
Grava linhas reais nas 21 tabelas criadas no Módulo 3. Idempotente: só corre `if (!await db.Users.AnyAsync())`.

### 4.9 DTOs
Não aplicável (o seed grava entidades diretamente via `AppDbContext`, não passa por Controllers/DTOs).

### 4.10 Validation
Não aplicável (dados de confiança, escritos no código, não input de utilizador).

### 4.11 Mappers
Não aplicável.

### 4.12 Repository
Não aplicável — o seed usa o `AppDbContext` diretamente (é um processo de arranque, não um caso de uso HTTP).

### 4.13 Service
`Data/Seed/DbSeeder.cs` — `static async Task SeedAsync(AppDbContext db)`.

### 4.14 Dependency Injection
Não é registado no contentor DI; é invocado manualmente em `Program.cs` a partir de um `scope` criado depois de `Build()`.

### 4.15 Controller
Não aplicável.

### 4.16 Authorization
Não aplicável.

### 4.17 Swagger
Não aplicável.

### 4.18 Migration
Nenhuma nova (usa as tabelas já migradas no Módulo 3). `Program.cs` chama `await db.Database.MigrateAsync();` antes do seed, para garantir que migrations pendentes são aplicadas mesmo que se tenha esquecido o `dotnet ef database update` manual.

### 4.19 Tests
Arrancar `dotnet run` e confirmar no pgAdmin as contagens da secção 4.21. Arrancar 2× seguidas **não deve duplicar** (idempotência).

### 4.20 Integração Front-end
Direta e crítica: os 6 utilizadores demo (mesma password `novati123`), os ids lógicos e os dados semeados são exatamente o que os testes E2E (`novati/e2e/integracao.spec.js`) e o front em geral esperam encontrar.

### 4.21 Critério de conclusão
Ao arrancar `dotnet run`, no pgAdmin: `Users` tem 6 linhas, `Solicitacoes` 10, `Mensagens` 30, `RelatoriosTecnicos` 8. Arrancar 2× seguidas **não duplica**.

### NÍVEL 3 — Tarefas (ordem exacta)

1. **Criar `Data/Seed/DbSeeder.cs`** com `static async Task SeedAsync(AppDbContext db)`, guarda de idempotência `if (!await db.Users.AnyAsync()) { ... }`.
   ```
   Arquivo: Data/Seed/DbSeeder.cs
   Responsabilidade: popular a BD vazia com os dados de demonstração, uma única vez
   Depende de: Módulo 3 (AppDbContext e tabelas)
   É utilizado por: Program.cs (chamado no arranque, só em Development)
   Resultado esperado: método estático reentrante e seguro para correr em todos os arranques em dev
   ```

2. **Converter `novati/src/data/seed.js` + `seedRelatorios.js`** para as 21 entidades, usando o dicionário `Dictionary<string, Guid>` para mapear ids lógicos (`u1`, `s1`...) para `Guid` reais.

3. **Criar os 6 utilizadores demo** (password de todos: **`novati123`**, hash com `BCrypt.Net.BCrypt.HashPassword("novati123")`):

   | Nome | Email | Role |
   |------|-------|------|
   | Rita Almeida | rita.admin@empresa.com | ADMIN |
   | Carlos Gestor | carlos.gestor@empresa.com | GESTOR |
   | João Técnico | joao.tecnico@empresa.com | TECNICO |
   | Marta Técnica | marta.tecnica@empresa.com | TECNICO |
   | Pedro Funcionário | pedro.funcionario@empresa.com | FUNCIONARIO |
   | Ana Funcionária | ana.funcionaria@empresa.com | FUNCIONARIO |

4. **Inserir as restantes entidades pela ordem de 4.5**, respeitando as FKs. Usar `SaveChangesAsync` por blocos, se preferido.

5. **Criar histórico mínimo para cada ordem `RESOLVIDO`**: `ASSUMIDA`, `DIAGNOSTICO`, `SOLUCAO`, `ACEITE` (o front mostra a timeline em `OrdemReparoDetail.jsx`).

6. **Registar o arranque com migration + seed em `Program.cs`**, depois de `Build()`:
   ```csharp
   using (var scope = app.Services.CreateScope())
   {
       var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
       await db.Database.MigrateAsync();
       if (app.Environment.IsDevelopment()) await DbSeeder.SeedAsync(db);
   }
   ```
   ```
   Arquivo: Program.cs
   Responsabilidade: aplicar migrations pendentes e semear dados de demonstração automaticamente ao arrancar em dev
   Depende de: tarefas 1–5
   É utilizado por: qualquer dotnet run em Development
   Resultado esperado: primeira execução povoa a BD; execuções seguintes não duplicam nada
   ```

### GATE DE CONCLUSÃO — Módulo 5

```
MÓDULO 5 — Seed
  ↓
[VALIDAÇÃO]
  - dotnet run arranca sem erros e aplica migrations pendentes?
  - Users tem 6 linhas com os emails/roles corretos?
  - Solicitacoes tem 10, Mensagens 30, RelatoriosTecnicos 8?
  - Todas as passwords fazem login com "novati123" (hash BCrypt correto)?
  - Correr dotnet run uma segunda vez NÃO duplica os dados?
  - A ordem RESOLVIDO do seed tem histórico ASSUMIDA/DIAGNOSTICO/SOLUCAO/ACEITE?
  ↓
SIM → MÓDULO 5 CONCLUÍDO → avançar para o Módulo 6
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 6 — Autenticação (JWT) + Utilizadores `[PLANEADO]`

### 4.1 Objetivo do módulo
Implementar login com JWT, validação de token, claims/roles, e o CRUD de utilizadores — é o primeiro módulo de domínio completo (Controller → Service → Repository → Mapper → DTO) e o padrão que todos os módulos seguintes vão replicar. É também o módulo do qual **todos** os módulos 7–12 dependem para autorização.

### 4.2 Funcionalidades
- Login (`POST /auth/login`) — devolve JWT + dados do utilizador.
- Consulta do utilizador atual (`GET /users/me`) e diretório completo (`GET /users/directory`, todos os perfis).
- CRUD de utilizadores restrito a `ADMIN`.
- Atualização da assinatura digital do próprio utilizador (usada nos relatórios técnicos, Módulo 12).

### 4.3 Dependências
Módulo 5 (utilizadores semeados — sem eles não há com quem testar login), Módulo 4 (exceções, middleware, `Repository<T>`, `UnitOfWork`), Módulo 1 (Swagger, para o botão Authorize).

### 4.4 Dependentes
Todos os módulos 7–12: usam `[Authorize(Roles = "...")]` sobre o esquema JWT configurado aqui, e referenciam `User` como FK (solicitante, técnico, autor, responsável). Módulo 13 (login é o primeiro passo de qualquer sessão no front).

### 4.5 Entities
`User` (já criada no Módulo 2).

### 4.6 Enums
`Role` (`ADMIN`, `GESTOR`, `TECNICO`, `FUNCIONARIO`).

### 4.7 Relacionamentos
`User` é o lado "1" de várias relações 1:N com outras entidades (Módulos 8–12), mas neste módulo só a entidade em si é manipulada.

### 4.8 Database
Tabela `Users` já existe (Módulo 3). Nenhuma migration nova é necessária neste módulo, salvo se a modelação da assinatura precisar de ajuste (`Assinatura` já está prevista como `string?`).

### 4.9 DTOs
```csharp
public class LoginRequest { [Required, EmailAddress] public string Email { get; set; } = ""; [Required] public string Password { get; set; } = ""; }
public record LoginResponse(string AccessToken, UserDto User);
public class UserDto { public Guid Id; public string Nome; public string Email; public string Role; public string? Assinatura; }
public class CreateUserRequest { [Required] public string Nome; [Required, EmailAddress] public string Email; [Required] public string Role; }
public class UpdateUserRequest { [Required] public string Nome; [Required, EmailAddress] public string Email; [Required] public string Role; }
public class SignatureRequest { public string? DataUrl; }
```
`UserDto` é o que o front guarda em `currentUser`:
```json
{ "id": "guid", "nome": "Rita Almeida", "email": "rita.admin@empresa.com", "role": "ADMIN", "assinatura": null }
```
`assinatura` = dataURL (string) ou `null`. **Nunca** incluir `passwordHash` em nenhum DTO de saída.

### 4.10 Validation
`[Required]`, `[EmailAddress]` nos DTOs de entrada. Regras de negócio adicionais (não expressáveis em Data Annotations) ficam no Service: email duplicado → 409; utilizador não pode apagar-se a si próprio → 400; FK em uso ao apagar → 409.

### 4.11 Mappers
`UserMapper.ToDto(User u)` — nunca expõe `PasswordHash`.

### 4.12 Repository
`IUserRepository` (estende/compõe `IRepository<User>`) com `GetByEmailAsync(string email)` e `EmailExistsAsync(string email, Guid? excludeId = null)`.

### 4.13 Service
`IAuthService`/`AuthService`: `LoginAsync(LoginRequest)` — verifica password com BCrypt, gera JWT.
`IUserService`/`UserService`: `GetMeAsync`, `GetDirectoryAsync`, `GetAllAsync` (ADMIN), `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `UpdateSignatureAsync`.

**Geração do token**: claims `new Claim("sub", user.Id.ToString())`, `new Claim("role", user.Role.ToString())`, `new Claim("email", user.Email)`. Extensão `ClaimsPrincipalExtensions`: `GetUserId()` (lê `sub`) e `GetRole()` (lê `role`).

### 4.14 Dependency Injection
Registar em `AddApplicationServices()` (Módulo 4): `AddScoped<IUserRepository, UserRepository>()`, `AddScoped<IAuthService, AuthService>()`, `AddScoped<IUserService, UserService>()`.

**Configuração JWT em `Program.cs`** (armadilha dos nomes de claims):
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
          NameClaimType = "sub", RoleClaimType = "role",   // ⚠️ o token tem de ser criado com estes nomes
          ClockSkew = TimeSpan.Zero
      };
  });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
```
E `app.UseAuthentication(); app.UseAuthorization();` entre `UseCors` e `MapControllers`. Tudo exige login, exceto o que for marcado `[AllowAnonymous]`.

### 4.15 Controller
`AuthController` (rota `api/auth`), `UsersController` (rota `api/users`).

### 4.16 Authorization

| Método + rota | Quem | Body (request) | Resposta | Regras / erros |
|---|---|---|---|---|
| `POST /api/auth/login` | Anónimo | `{ email, password }` | `200 { accessToken, user: UserDto }` | Email/password errados → **`401 { message: "Credenciais inválidas." }`** (mesma mensagem nos dois casos — não revelar qual falhou) |
| `GET /api/users/me` | Autenticado | — | `UserDto` | |
| `GET /api/users/directory` | Autenticado (todos os perfis) | — | `UserDto[]` | ⚠️ tem de funcionar para **todos** (hidratação) |
| `GET /api/users` | ADMIN | — | `UserDto[]` | |
| `POST /api/users` | ADMIN | `{ nome, email, role }` | `201 UserDto` | Sem campo password no form → define **password inicial `novati123`**. Email duplicado → `409 "Já existe um utilizador com este email."` |
| `PATCH /api/users/{id}` | ADMIN | `{ nome, email, role }` | `UserDto` | 404 se não existe; 409 se email de outro |
| `DELETE /api/users/{id}` | ADMIN | — | `204` | Não pode apagar-se a si próprio (400). FK em uso → 409 |
| `PUT /api/users/me/signature` | Autenticado | `{ dataUrl: string \| null }` | `UserDto` **do utilizador atual já atualizado** | `null` remove a assinatura |

### 4.17 Swagger
Aplicar a checklist S.4 (Módulo 1) em `AuthController` e `UsersController` — este é o módulo onde o exemplo completo do login é implementado literalmente (ver Módulo 1, tarefa 11). Depois de fazer login no Swagger UI e usar o botão **Authorize**, testar que com o token do Pedro (FUNCIONARIO) `POST /api/users` dá **403** e `GET /api/users/directory` dá **200**.

### 4.18 Migration
Nenhuma nova, salvo ajustes de última hora ao índice único de `Email` (já previsto no Módulo 3).

### 4.19 Tests
```powershell
curl.exe -X POST http://localhost:3333/api/auth/login -H "Content-Type: application/json" -d "{\"email\":\"rita.admin@empresa.com\",\"password\":\"novati123\"}"
# copiar o accessToken:
curl.exe http://localhost:3333/api/users/me -H "Authorization: Bearer <TOKEN>"
curl.exe http://localhost:3333/api/users        # sem token → 401
```
+ password errada → `401` com `message`. Testes E2E "Autenticação" (2) passam: `npx playwright test -g "Autenticação"` na pasta `novati`.

### 4.20 Integração Front-end
`UserDto` é exatamente o que `AppContext.jsx` guarda em `currentUser`. O token vai no header `Authorization: Bearer <token>` em todas as chamadas seguintes de `api.js`.

### 4.21 Critério de conclusão
O fluxo de teste manual (4.19) passa para todos os 6 utilizadores demo; `POST /api/users` dá 403 para não-ADMIN; testes E2E "Autenticação" passam.

### NÍVEL 3 — Tarefas (ordem exacta)

1. **Configurar JWT + fallback policy em `Program.cs`** (ver bloco de código em 4.14).
   ```
   Arquivo: Program.cs
   Responsabilidade: validar tokens JWT em todos os pedidos, exceto os marcados [AllowAnonymous]
   Depende de: Módulo 3 (appsettings.Development.json já tem a secção Jwt)
   É utilizado por: [Authorize] em todos os controllers dos módulos 6–12
   Resultado esperado: pedido sem token a um endpoint autenticado devolve 401
   ```

2. **Criar `Common/Settings/JwtSettings.cs`** e ler via `builder.Configuration.GetSection("Jwt")`.

3. **Criar `Common/Extensions/ClaimsPrincipalExtensions.cs`** (`GetUserId()`, `GetRole()`).

4. **Criar `IUserRepository`/`UserRepository`** com `GetByEmailAsync`, `EmailExistsAsync`.
   ```
   Arquivo: Repositories/Interfaces/IUserRepository.cs, Repositories/UserRepository.cs
   Responsabilidade: consultas específicas de User (por email) além do CRUD genérico
   Depende de: Módulo 4 (Repository<T>), Módulo 3 (AppDbContext)
   É utilizado por: IAuthService, IUserService
   Resultado esperado: GetByEmailAsync devolve o User ou null
   ```

5. **Criar `UserMapper.ToDto(User)`** (nunca inclui `PasswordHash`).

6. **Criar os DTOs** (4.9): `LoginRequest`, `LoginResponse`, `UserDto`, `CreateUserRequest`, `UpdateUserRequest`, `SignatureRequest`.

7. **Criar `IAuthService`/`AuthService`** — `LoginAsync`: procura por email, verifica password com `BCrypt.Net.BCrypt.Verify`, gera JWT com os claims de 4.13. Falha (email não existe OU password errada) → `UnauthorizedException("Credenciais inválidas.")` (mesma mensagem nos dois casos).
   ```
   Arquivo: Services/Interfaces/IAuthService.cs, Services/AuthService.cs
   Responsabilidade: autenticar e emitir o JWT
   Depende de: tarefas 2, 3, 4, 6
   É utilizado por: AuthController
   Resultado esperado: LoginAsync devolve LoginResponse válido para credenciais corretas; lança UnauthorizedException senão
   ```

8. **Criar `IUserService`/`UserService`** com os 6 métodos de 4.13, incluindo as regras de negócio de 4.10 (email duplicado, autoexclusão, FK em uso).
   ```
   Arquivo: Services/Interfaces/IUserService.cs, Services/UserService.cs
   Responsabilidade: regras de negócio de utilizadores (CRUD restrito a ADMIN, assinatura do próprio)
   Depende de: tarefas 4, 5, 6
   É utilizado por: UsersController
   Resultado esperado: CreateAsync com email duplicado lança ConflictException; DeleteAsync sobre si próprio lança BusinessRuleException
   ```

9. **Registar tudo em `AddApplicationServices()`** (Módulo 4).

10. **Criar `AuthController`** (`POST /api/auth/login`, `[AllowAnonymous]`) e **`UsersController`** com as 7 rotas de 4.16, aplicando `[Authorize(Roles = "...")]` conforme a coluna "Quem".

11. **Aplicar a checklist Swagger S.4** a ambos os controllers (comentários `///`, `[ProducesResponseType]`, `[Tags]`).

12. **Testar manualmente** conforme 4.19, e correr os testes E2E de Autenticação.

### GATE DE CONCLUSÃO — Módulo 6

```
MÓDULO 6 — Auth + Utilizadores
  ↓
[VALIDAÇÃO]
  - Entity funciona? (User já validada no Módulo 2/3)
  - Database funciona? (tabela Users com índice único em Email)
  - Repository funciona? (GetByEmailAsync, EmailExistsAsync)
  - Service funciona? (login emite token válido; regras de negócio de User)
  - Endpoints funcionam? (8 rotas da tabela 4.16)
  - Swagger funciona? (botão Authorize aceita o token e desbloqueia rotas protegidas)
  - Authorization funciona? (403 para roles erradas, testado com o Pedro)
  - Testes passam? (E2E "Autenticação")
  - Front-end consegue consumir? (UserDto bate certo com currentUser)
  ↓
SIM → MÓDULO 6 CONCLUÍDO → avançar para o Módulo 7
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 7 — Catálogo `[PLANEADO]`

### 4.1 Objetivo do módulo
CRUD dos dados de referência do sistema — modelos de dispositivo, modelos de componente e as compatibilidades entre eles. É o primeiro módulo de domínio "simples" (sem máquina de estados), e a base de dados de referência que os Módulos 8 e 9 vão consumir via FK.

### 4.2 Funcionalidades
CRUD completo de `ModeloDispositivo`, `ModeloComponente`, `Compatibilidade`. Leitura por qualquer perfil autenticado; escrita só `ADMIN`.

### 4.3 Dependências
Módulo 6 (autenticação/autorização), Módulo 4 (Repository genérico, exceções), Módulo 3 (tabelas `ModelosDispositivo`, `ModelosComponente`, `Compatibilidades`).

### 4.4 Dependentes
Módulo 8 (`DispositivoFisico.ModeloDispositivoId`, `InstanciaComponente.ModeloComponenteId` são FK para aqui), Módulo 9 (`ItemStock.ModeloComponenteId`), Módulo 11 (algoritmo de instalar peça valida `Compatibilidade`).

### 4.5 Entities
`ModeloDispositivo`, `ModeloComponente`, `Compatibilidade`.

### 4.6 Enums
Nenhum específico deste módulo (`Tipo` é uma string livre, não enum, tanto em `ModeloDispositivo` como `ModeloComponente`).

### 4.7 Relacionamentos
`ModeloDispositivo` N—N `ModeloComponente` via `Compatibilidade` (Cascade ao apagar); `ModeloDispositivo` 1—N `DispositivoFisico` (Restrict); `ModeloComponente` 1—N `InstanciaComponente` (Restrict); `ModeloComponente` 1—1 `ItemStock` (Restrict) — estas duas últimas só se tornam relevantes a partir dos Módulos 8/9, mas condicionam o que este módulo pode apagar.

### 4.8 Database
Tabelas já existem (Módulo 3): `ModelosDispositivo`, `ModelosComponente`, `Compatibilidades` (índice único no par `ModeloDispositivoId`+`ModeloComponenteId`).

### 4.9 DTOs
```csharp
public record ModeloDispositivoDto(Guid Id, string Nome, string Fabricante, string Tipo);
public class CreateModeloDispositivoRequest { [Required] public string Nome; [Required] public string Fabricante; [Required] public string Tipo; }
public record ModeloComponenteDto(Guid Id, string Nome, string Tipo, string Capacidade, int StockMinimo);
public class CreateModeloComponenteRequest { [Required] public string Nome; [Required] public string Tipo; [Required] public string Capacidade; public int? StockMinimo; }
public record CompatibilidadeDto(Guid Id, Guid ModeloDispositivoId, Guid ModeloComponenteId);
public class CreateCompatibilidadeRequest { [Required] public Guid ModeloDispositivoId; [Required] public Guid ModeloComponenteId; }
```

### 4.10 Validation
`[Required]` nos campos de texto. Regras de negócio no Service: `stockMinimo` default = 1 quando omitido (o form do front não envia); par de compatibilidade repetido → 409; ids inexistentes na criação de compatibilidade → 404; apagar modelo com dispositivos associados → 409.

### 4.11 Mappers
`CatalogoMapper` com `ToDto` para cada uma das 3 entidades.

### 4.12 Repository
`IModeloDispositivoRepository`, `IModeloComponenteRepository`, `ICompatibilidadeRepository` (cada um sobre `IRepository<T>` genérico, mais métodos específicos como `ExistsPairAsync`).

### 4.13 Service
`ICatalogoService`/`CatalogoService` — orquestra os 3 repositórios; apanha `DbUpdateException` ao apagar e traduz para `ConflictException("Existem dispositivos deste modelo.")` quando aplicável.

### 4.14 Dependency Injection
Registar `ICatalogoService`, `IModeloDispositivoRepository`, `IModeloComponenteRepository`, `ICompatibilidadeRepository` em `AddApplicationServices()`.

### 4.15 Controller
`CatalogoController`, rota base `api/catalogo`.

### 4.16 Authorization

| Método + rota | Body | Resposta |
|---|---|---|
| `GET /api/catalogo/modelos-dispositivo` | — | `[{ id, nome, fabricante, tipo }]` |
| `POST /api/catalogo/modelos-dispositivo` | `{ nome, fabricante, tipo }` | `201` o objeto criado |
| `PATCH /api/catalogo/modelos-dispositivo/{id}` | `{ nome, fabricante, tipo }` | objeto atualizado |
| `DELETE /api/catalogo/modelos-dispositivo/{id}` | — | `204` (compatibilidades apagam em cascata; se houver dispositivos → 409 *"Existem dispositivos deste modelo."*) |
| `GET /api/catalogo/modelos-componente` | — | `[{ id, nome, tipo, capacidade, stockMinimo }]` |
| `POST /api/catalogo/modelos-componente` | `{ nome, tipo, capacidade }` (+ `stockMinimo?`) | `201` objeto. **`stockMinimo` default = 1** se não vier |
| `PATCH /api/catalogo/modelos-componente/{id}` | `{ nome, tipo, capacidade, stockMinimo? }` | objeto |
| `DELETE /api/catalogo/modelos-componente/{id}` | — | `204` |
| `GET /api/catalogo/compatibilidades` | — | `[{ id, modeloDispositivoId, modeloComponenteId }]` |
| `POST /api/catalogo/compatibilidades` | `{ modeloDispositivoId, modeloComponenteId }` | `201` objeto. Par repetido → 409; ids inexistentes → 404 |
| `DELETE /api/catalogo/compatibilidades/{id}` | — | `204` |

**Leitura:** qualquer perfil autenticado (o funcionário vê nomes de dispositivos). **Escrita:** só `ADMIN` — todas as rotas `POST`/`PATCH`/`DELETE` levam `[Authorize(Roles = "ADMIN")]`.

> ⚠️ Front, `removeModeloDispositivo` etc.: depois do POST/PATCH o front **re-faz o GET da lista** — por isso a resposta do POST/PATCH pode ser qualquer JSON válido, mas devolver o objeto criado é boa prática.

### 4.17 Swagger
Checklist S.4 aplicada ao `CatalogoController`, com `[Tags("Catálogo")]`.

### 4.18 Migration
Nenhuma nova (tabelas já criadas no Módulo 3).

### 4.19 Tests
CRUD completo testado no Swagger UI; `GET` devolve os 3 modelos de dispositivo, 4 de componente e 6 compatibilidades do seed (Módulo 5). `PONTO A DECIDIR`: testes automatizados (xUnit) específicos para os casos de erro (par repetido, apagar com dependentes) — cobertura razoável sugerida no Módulo 14.

### 4.20 Integração Front-end
`ModeloDispositivoDto`/`ModeloComponenteDto`/`CompatibilidadeDto` alimentam `modelosDispositivo`, `modelosComponente`, `compatibilidades` em `AppContext.jsx`.

### 4.21 Critério de conclusão
CRUD completo testado no Swagger UI; o `GET` devolve os 3 modelos de dispositivo, 4 de componente e 6 compatibilidades do seed; o endpoint aparece em `/swagger` com os schemas e códigos de resposta da tabela 4.16.

### NÍVEL 3 — Tarefas (ordem exacta)

1. Criar `Dtos/Catalogo/*.cs` (4.9).
2. Criar `IModeloDispositivoRepository`/`ModeloDispositivoRepository`, `IModeloComponenteRepository`/`ModeloComponenteRepository`, `ICompatibilidadeRepository`/`CompatibilidadeRepository`.
   ```
   Arquivo: Repositories/Interfaces/IModeloDispositivoRepository.cs, ModeloDispositivoRepository.cs (+ equivalentes para os outros 2)
   Responsabilidade: consultas/persistência específicas de cada entidade de catálogo
   Depende de: Módulo 4 (Repository<T>)
   É utilizado por: CatalogoService
   Resultado esperado: CRUD funcional sobre as 3 tabelas
   ```
3. Criar `CatalogoMapper`.
4. Criar `ICatalogoService`/`CatalogoService` com as regras de 4.10/4.13.
5. Registar em `AddApplicationServices()`.
6. Criar `CatalogoController` com as 11 rotas de 4.16, `[Authorize(Roles = "ADMIN")]` nas escritas.
7. Aplicar checklist Swagger S.4.
8. Testar manualmente contra os dados do seed.

### GATE DE CONCLUSÃO — Módulo 7

```
MÓDULO 7 — Catálogo
  ↓
[VALIDAÇÃO]
  - Entity/Database já validadas (Módulos 2–3)?
  - Repository funciona para as 3 entidades?
  - Service aplica as regras de negócio (par repetido, stockMinimo default, FK em uso)?
  - Os 11 endpoints funcionam e devolvem os status corretos?
  - Swagger documenta as 3 subseções (modelos-dispositivo, modelos-componente, compatibilidades)?
  - Authorization: leitura para todos, escrita só ADMIN?
  - GET devolve os dados exatos do seed (3+4+6)?
  - Front-end consegue consumir (schemas batem com AppContext.jsx)?
  ↓
SIM → MÓDULO 7 CONCLUÍDO → avançar para o Módulo 8
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 8 — Localizações, Dispositivos e Instâncias `[PLANEADO]`

### 4.1 Objetivo do módulo
Modelar o inventário físico: onde estão os dispositivos (árvore de localizações), que dispositivos existem (com patrimônio único, estado, responsável), e que componentes têm instalados (instâncias).

### 4.2 Funcionalidades
CRUD de localizações (árvore), CRUD/listagem de dispositivos físicos com mudança de estado, listagem e criação de instâncias de componente (com validação de compatibilidade).

### 4.3 Dependências
Módulo 6 (autenticação), Módulo 7 (`ModeloDispositivo`, `ModeloComponente`, `Compatibilidade` como FK/validação).

### 4.4 Dependentes
Módulo 10 (`Solicitacao.DispositivoFisicoId` opcional), Módulo 11 (algoritmo de instalar peça usa `DispositivoFisico`/`InstanciaComponente` diretamente).

### 4.5 Entities
`Localizacao`, `DispositivoFisico`, `InstanciaComponente`.

### 4.6 Enums
`EstadoDispositivo` (`ATIVO`, `MANUTENCAO`, `INATIVO`), `EstadoInstancia` (`INSTALADO`).

### 4.7 Relacionamentos
`Localizacao` 1—N `Localizacao` (auto-relação, Restrict); `Localizacao` 1—N `DispositivoFisico` (Restrict); `User` 1—N `DispositivoFisico` (responsável, SetNull); `ModeloDispositivo` 1—N `DispositivoFisico` (Restrict); `DispositivoFisico` 1—N `InstanciaComponente` (Cascade); `ModeloComponente` 1—N `InstanciaComponente` (Restrict).

### 4.8 Database
Tabelas já existem (Módulo 3): `Localizacoes` (auto-FK `PaiId`), `DispositivosFisicos` (`Patrimonio` único), `InstanciasComponentes`.

### 4.9 DTOs
```csharp
public record LocalizacaoDto(Guid Id, string Nome, Guid? Pai);
public class CreateLocalizacaoRequest { [Required] public string Nome; public Guid? Pai; }
public class DispositivoDto { public Guid Id; public string Patrimonio; public Guid ModeloDispositivoId; public string NumeroSerie; public Guid LocalizacaoId; public Guid? ResponsavelId; public string Estado; public DateOnly? DataAquisicao; public int? GarantiaMeses; }
public class CreateDispositivoRequest { [Required] public string Patrimonio; [Required] public Guid ModeloDispositivoId; [Required] public string NumeroSerie; [Required] public Guid LocalizacaoId; public Guid? ResponsavelId; public DateOnly? DataAquisicao; public int? GarantiaMeses; }
public class UpdateEstadoDispositivoRequest { [Required] public string Estado; }
public record InstanciaDto(Guid Id, Guid DispositivoFisicoId, Guid ModeloComponenteId, string Codigo, string Estado);
public class CreateInstanciaRequest { [Required] public Guid ModeloComponenteId; [Required] public string Codigo; }
```
`DispositivoDto`:
```json
{ "id":"guid","patrimonio":"NB-001","modeloDispositivoId":"guid","numeroSerie":"ABC123",
  "localizacaoId":"guid","responsavelId":"guid|null","estado":"ATIVO",
  "dataAquisicao":"2023-05-10","garantiaMeses":24 }
```

### 4.10 Validation
`pai` inexistente na criação de localização → 404. Património repetido → `409 "Já existe um dispositivo com este nº de património."`. Estado inválido (fora do enum) → 400. Instância: componente incompatível com o modelo do dispositivo → `400 "Componente incompatível com este modelo."` (usa `Compatibilidade` do Módulo 7).

### 4.11 Mappers
`DispositivoMapper` (`Localizacao`, `DispositivoFisico`, `InstanciaComponente` → DTOs respetivos).

### 4.12 Repository
`ILocalizacaoRepository`, `IDispositivoRepository`, `IInstanciaRepository`.

### 4.13 Service
`IDispositivoService`/`DispositivoService` — orquestra os 3 repositórios + `ICompatibilidadeRepository` (Módulo 7) para validar instâncias.

### 4.14 Dependency Injection
Registar `IDispositivoService`, `ILocalizacaoRepository`, `IDispositivoRepository`, `IInstanciaRepository` em `AddApplicationServices()`.

### 4.15 Controller
`DispositivosController`, rota base `api/dispositivos`.

⚠️ **Conflito de rotas:** `GET /dispositivos/localizacoes` e `GET /dispositivos/instancias` podem ser confundidos com `GET /dispositivos/{id}`. Usar **constraint**: `[HttpGet("{id:guid}")]` — só casa com Guid.

### 4.16 Authorization
**Leitura:** todos os perfis (senão a hidratação falha). **Escrita:** `ADMIN`, `TECNICO`.

| Método + rota | Body | Resposta |
|---|---|---|
| `GET /api/dispositivos/localizacoes` | — | `[{ id, nome, pai }]` — `pai` = `id` da localização-pai ou `null` |
| `POST /api/dispositivos/localizacoes` | `{ nome, pai }` (`pai` pode ser `null`) | `201 { id, nome, pai }`. `pai` inexistente → 404 |
| `GET /api/dispositivos` | — | `[DispositivoDto]` |
| `POST /api/dispositivos` | `{ patrimonio, modeloDispositivoId, numeroSerie, localizacaoId, responsavelId (null ok), dataAquisicao ("AAAA-MM-DD" ou null), garantiaMeses (int ou null) }` | `201 DispositivoDto`. Património repetido → `409` |
| `PATCH /api/dispositivos/{id}/estado` | `{ estado: "ATIVO"\|"MANUTENCAO"\|"INATIVO" }` | `DispositivoDto` atualizado (o front **substitui** o item pela resposta) |
| `GET /api/dispositivos/instancias` | — | `[{ id, dispositivoFisicoId, modeloComponenteId, codigo, estado }]` (`estado`: `"INSTALADO"`) |
| `POST /api/dispositivos/{id}/instancias` | `{ modeloComponenteId, codigo }` | `201` instância criada. Validar compatibilidade (senão 400) |

### 4.17 Swagger
Checklist S.4, `[Tags("Dispositivos")]`.

### 4.18 Migration
Nenhuma nova (tabelas já criadas no Módulo 3).

### 4.19 Tests
Manual via Swagger UI: os 3 dispositivos + 4 instâncias + 4 localizações do seed aparecem; mudar estado funciona; criar dispositivo com património repetido dá 409. `PONTO A DECIDIR`: testes automatizados da árvore de localizações (ciclos, profundidade).

### 4.20 Integração Front-end
`LocalizacaoDto`, `DispositivoDto`, `InstanciaDto` alimentam `localizacoes`, `dispositivosFisicos`, `instanciasComponentes` em `AppContext.jsx`.

### 4.21 Critério de conclusão
Os 3 dispositivos + 4 instâncias + 4 localizações do seed aparecem; mudar estado funciona; criar dispositivo com património repetido dá 409 com mensagem.

### NÍVEL 3 — Tarefas (ordem exacta)

1. Criar `Dtos/Dispositivos/*.cs` (4.9).
2. Criar `ILocalizacaoRepository`/`LocalizacaoRepository`, `IDispositivoRepository`/`DispositivoRepository`, `IInstanciaRepository`/`InstanciaRepository`.
3. Criar `DispositivoMapper`.
4. Criar `IDispositivoService`/`DispositivoService`, incluindo a validação de compatibilidade (usa `ICompatibilidadeRepository` do Módulo 7 — **dependência externa declarada**, não se reimplementa o Módulo 7).
5. Registar em `AddApplicationServices()`.
6. Criar `DispositivosController` com as 7 rotas de 4.16, aplicando `[HttpGet("{id:guid}")]` para evitar conflito de rotas.
7. Aplicar checklist Swagger S.4.
8. Testar manualmente contra os dados do seed.

### GATE DE CONCLUSÃO — Módulo 8

```
MÓDULO 8 — Localizações, Dispositivos, Instâncias
  ↓
[VALIDAÇÃO]
  - Repository funciona para as 3 entidades (incluindo árvore de localizações)?
  - Service valida património único e compatibilidade de componente?
  - Os 7 endpoints funcionam, sem conflito de rotas entre /localizacoes, /instancias e /{id}?
  - Swagger documenta o módulo?
  - Authorization: leitura para todos, escrita ADMIN/TECNICO?
  - GET devolve os dados exatos do seed (4 localizações, 3 dispositivos, 4 instâncias)?
  - Front-end consegue consumir?
  ↓
SIM → MÓDULO 8 CONCLUÍDO → avançar para o Módulo 9
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 9 — Stock `[PLANEADO]`

### 4.1 Objetivo do módulo
Gerir peças de substituição: o "tipo de item" (`ItemStock`, 1 por modelo de componente), as unidades físicas individuais (`UnidadeStock`, com código único e estado), e o histórico de movimentos (`MovimentoStock`). A lógica de geração de código de unidade e criação em lote construída aqui será **reutilizada** (não duplicada) pelo Módulo 11 (Compras).

### 4.2 Funcionalidades
Listagem de itens/unidades/movimentos, entrada manual de stock (cria N unidades + 1 movimento).

### 4.3 Dependências
Módulo 6 (autenticação), Módulo 7 (`ModeloComponente` como FK de `ItemStock`).

### 4.4 Dependentes
Módulo 11 (Atendimentos usa `UnidadeStock`/`ItemStock`/`MovimentoStock` no algoritmo de reservar/instalar peça; Compras reutiliza `IStockService.CriarUnidadesAsync`).

### 4.5 Entities
`ItemStock`, `UnidadeStock`, `MovimentoStock`.

### 4.6 Enums
`EstadoUnidade` (`DISPONIVEL`, `RESERVADA`, `AVARIADA`, `INSTALADA`), `TipoMovimento` (`ENTRADA`, `SAIDA`, `TRANSFERENCIA`, `RESERVA`, `INSTALACAO`).

### 4.7 Relacionamentos
`ModeloComponente` 1—1 `ItemStock` (Restrict); `ItemStock` 1—N `UnidadeStock`/`MovimentoStock`/`RequisicaoCompra` (Restrict); `OrdemReparo` 1—N `UnidadeStock` via `ReservadaParaOrdemId` (opcional, SetNull — usado a partir do Módulo 11).

### 4.8 Database
Tabelas já existem (Módulo 3): `ItensStock` (único por `ModeloComponenteId`), `UnidadesStock` (`Codigo` único), `MovimentosStock`.

### 4.9 DTOs
```csharp
public record ItemStockDto(Guid Id, Guid ModeloComponenteId);
public record UnidadeDto(Guid Id, Guid ItemStockId, string Codigo, string Estado, Guid? ReservadaParaOrdemId);
public record MovimentoDto(Guid Id, Guid ItemStockId, string Tipo, int Quantidade, DateOnly Data, string Observacao);
public class EntradaStockRequest { [Required] public Guid ModeloComponenteId; [Required, Range(1, int.MaxValue)] public int Quantidade; public string? Observacao; }
```

### 4.10 Validation
`quantidade` ≥ 1 (`[Range(1, int.MaxValue)]` + verificação no Service). Modelo de componente inexistente → 404.

### 4.11 Mappers
`StockMapper`.

### 4.12 Repository
`IItemStockRepository` (com `GetOrCreateByModeloAsync`), `IUnidadeStockRepository` (com `ExistsCodigoAsync`, `GetDisponivelAsync`), `IMovimentoStockRepository`.

### 4.13 Service
`IStockService`/`StockService`, com o método público `CriarUnidadesAsync(Guid modeloComponenteId, int quantidade, string observacao, TipoMovimento tipo)` — **este é o método que o Módulo 11 (Compras) reutiliza**, não copia.

**Lógica de `entrada`:**
1. `quantidade` ≥ 1 (senão 400).
2. Modelo existe? (senão 404).
3. **Get-or-create** do `ItemStock` desse modelo.
4. Criar `quantidade` `UnidadeStock` com `Estado = DISPONIVEL`, código único `PREFIXO-NNN`: prefixo pelo tipo (`SSD`→`SSD`, `RAM`→`RAM`, `Fonte`→`FNT`, `Bateria`→`BAT`, outro → 3 primeiras letras em maiúsculas); `NNN` = próximo número livre (verificar se o código já existe; o seed tem `SSD-099`... não colidir).
5. 1 `MovimentoStock` `ENTRADA` com `Quantidade = quantidade` e `Observacao` (default *"Entrada manual"*).
6. Tudo num único `SaveChangesAsync`.

### 4.14 Dependency Injection
Registar `IStockService`, `IItemStockRepository`, `IUnidadeStockRepository`, `IMovimentoStockRepository` em `AddApplicationServices()`.

### 4.15 Controller
`StockController`, rota base `api/stock`.

### 4.16 Authorization
**Leitura:** todos os perfis (hidratação!). **Escrita (`entrada`):** `ADMIN`, `TECNICO`.

| Método + rota | Body | Resposta |
|---|---|---|
| `GET /api/stock/itens` | — | `[{ id, modeloComponenteId }]` |
| `GET /api/stock/unidades` | — | `[{ id, itemStockId, codigo, estado, reservadaParaOrdemId }]` (`reservadaParaOrdemId` = `null` quando não reservada) |
| `GET /api/stock/movimentos` | — | `[{ id, itemStockId, tipo, quantidade, data, observacao }]` — **mais recentes primeiro** |
| `POST /api/stock/entrada` | `{ modeloComponenteId, quantidade, observacao }` | `201` **array** `UnidadeDto[]` das unidades criadas (⚠️ é um **array**: o front faz `criadas.map(...)`) |

### 4.17 Swagger
Checklist S.4, `[Tags("Stock")]`.

### 4.18 Migration
Nenhuma nova.

### 4.19 Tests
4 itens, 7 unidades, 7 movimentos do seed; uma `POST /stock/entrada` cria N unidades, 1 movimento, e `GET /stock/unidades` reflete.

### 4.20 Integração Front-end
`ItemStockDto`, `UnidadeDto`, `MovimentoDto` alimentam `itensStock`, `unidadesStock`, `movimentosStock`.

### 4.21 Critério de conclusão
4 itens, 7 unidades, 7 movimentos do seed; uma `POST /stock/entrada` cria N unidades, 1 movimento e o `GET /stock/unidades` reflete.

### NÍVEL 3 — Tarefas (ordem exacta)

1. Criar `Dtos/Stock/*.cs` (4.9).
2. Criar `IItemStockRepository`/`ItemStockRepository` com `GetOrCreateByModeloAsync`.
3. Criar `IUnidadeStockRepository`/`UnidadeStockRepository` com `ExistsCodigoAsync`, `GetDisponivelAsync`.
4. Criar `IMovimentoStockRepository`/`MovimentoStockRepository`.
5. Criar `StockMapper`.
6. Criar `IStockService`/`StockService` com `CriarUnidadesAsync` (algoritmo de 4.13) — expor publicamente para reutilização no Módulo 11.
7. Registar em `AddApplicationServices()`.
8. Criar `StockController` com as 4 rotas de 4.16.
9. Aplicar checklist Swagger S.4.
10. Testar manualmente contra os dados do seed.

### GATE DE CONCLUSÃO — Módulo 9

```
MÓDULO 9 — Stock
  ↓
[VALIDAÇÃO]
  - IStockService.CriarUnidadesAsync gera códigos únicos sem colidir com o seed?
  - Get-or-create de ItemStock funciona (não duplica item para o mesmo modelo)?
  - Os 4 endpoints funcionam, POST /stock/entrada devolve um ARRAY?
  - Swagger documenta o módulo?
  - Authorization: leitura para todos, escrita ADMIN/TECNICO?
  - GET devolve os dados exatos do seed (4 itens, 7 unidades, 7 movimentos)?
  - Front-end consegue consumir?
  ↓
SIM → MÓDULO 9 CONCLUÍDO → avançar para o Módulo 10
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 10 — Solicitações + Notificações + Base de Conhecimento + Chat `[PLANEADO]`

### 4.1 Objetivo do módulo
Implementar o pedido de suporte do funcionário (Solicitação), o sistema de avisos partilhado por todos os módulos (Notificação), os artigos de autoajuda com pesquisa (Base de Conhecimento) e a conversa por solicitação (Chat). É o módulo mais transversal em termos de visibilidade por perfil.

### 4.2 Funcionalidades
- Notificações: listar as do utilizador atual, marcar como lida (uma ou todas).
- Solicitações: criar, listar (filtrado por perfil), fechar, avaliar, resolver via base de conhecimento.
- Base de conhecimento: listar, criar, pesquisar por texto.
- Chat: listar mensagens (filtrado por perfil de acesso à conversa), enviar mensagem, marcar conversa como lida.

### 4.3 Dependências
Módulo 6 (autenticação — `SolicitanteId`/`AutorId` vêm do token), Módulo 8 (`Solicitacao.DispositivoFisicoId` opcional).

### 4.4 Dependentes
Módulo 11 (`OrdemReparo.SolicitacaoId` é FK 1–1 para `Solicitacao`; "assumir" só parte de uma `Solicitacao` `ABERTA`; o serviço de notificações criado aqui é usado por Atendimentos e Compras).

### 4.5 Entities
`Solicitacao`, `Notificacao`, `Artigo`, `Mensagem`. Classes auxiliares: `Anexo`, `Avaliacao` (owned em `Solicitacao`).

### 4.6 Enums
`EstadoSolicitacao` (`ABERTA`, `EM_ATENDIMENTO`, `AGUARDA_VALIDACAO`, `RESOLVIDA`, `FECHADA`), `Prioridade` (`BAIXA`, `MEDIA`, `ALTA`, `URGENTE`).

### 4.7 Relacionamentos
`User` 1—N `Solicitacao` (solicitante, Restrict); `DispositivoFisico` 1—N `Solicitacao` (opcional, Restrict); `Solicitacao` 1—0..1 `OrdemReparo` (Restrict, usado a partir do Módulo 11); `Solicitacao` 1—N `Mensagem` (Cascade); `User` 1—N `Mensagem`/`Artigo` (Restrict); `User` 1—N `Notificacao` (Cascade).

### 4.8 Database
Tabelas já existem (Módulo 3): `Solicitacoes` (com `Anexos` jsonb e colunas `Avaliacao_*` owned), `Notificacoes`, `Artigos` (`Tags` como `text[]`), `Mensagens`.

### 4.9 DTOs
```csharp
public class SolicitacaoDto { public Guid Id; public string Titulo; public string Descricao; public string Estado; public Guid? DispositivoFisicoId; public Guid SolicitanteId; public DateOnly DataCriacao; public string Prioridade; public string Categoria; public List<AnexoDto> Anexos = []; public AvaliacaoDto? Avaliacao; public bool ResolvidaViaBase; }
public class CreateSolicitacaoRequest { [Required] public string Titulo; [Required] public string Descricao; public Guid? DispositivoFisicoId; [Required] public string Prioridade; [Required] public string Categoria; public List<AnexoDto> Anexos = []; }
public class AvaliarSolicitacaoRequest { [Range(1,5)] public int Estrelas; public string Comentario = ""; }
public record NotificacaoDto(Guid Id, Guid UserId, string Message, string? Link, bool Lida, DateOnly Data);
public record ArtigoDto(Guid Id, string Titulo, string Conteudo, List<string> Tags, string Categoria, Guid AutorId);
public class CreateArtigoRequest { [Required] public string Titulo; [Required] public string Conteudo; public List<string> Tags = []; [Required] public string Categoria; }
public class MensagemDto { public Guid Id; public Guid SolicitacaoId; public Guid AutorId; public string Texto; public DateOnly Data; public string Hora; public bool Lida; }
public class CreateMensagemRequest { [Required] public Guid SolicitacaoId; [Required] public string Texto; }
```
`SolicitacaoDto`:
```json
{ "id":"guid","titulo":"...","descricao":"...","estado":"ABERTA","dispositivoFisicoId":"guid|null",
  "solicitanteId":"guid","dataCriacao":"2026-08-30","prioridade":"ALTA","categoria":"Hardware",
  "anexos":[{"nome":"","tipo":"","dataUrl":""}],
  "avaliacao": null,
  "resolvidaViaBase": false }
```
> `avaliacao` = `null` quando não avaliada (o front testa `!s.avaliacao`).

### 4.10 Validation
`titulo`/`descricao`/`prioridade`/`categoria` obrigatórios. `estrelas` entre 1 e 5. Texto de mensagem vazio → 400. Regras de estado (só fechar `RESOLVIDA`, só avaliar sem avaliação prévia, etc.) — ver 4.13.

### 4.11 Mappers
`SolicitacaoMapper`, `NotificacaoMapper`, `ArtigoMapper`, `MensagemMapper` (`hora` formatada como `"HH:mm"`).

### 4.12 Repository
`ISolicitacaoRepository`, `INotificacaoRepository`, `IArtigoRepository`, `IMensagemRepository`.

### 4.13 Service

**10.1 `NotificacaoService` (interno, usado por todos os outros módulos a partir daqui):**
`Task CriarAsync(Guid userId, string message, string? link)` — **só adiciona** ao contexto (o `SaveChanges` é do Service chamador → mesma transação). Método auxiliar: `NotificarPerfisAsync(Role[] roles, ...)`, útil para "avisar todos os técnicos".

| Método + rota | Quem | Resposta |
|---|---|---|
| `GET /api/notificacoes` | Autenticado | **só as do utilizador do token**: `[{ id, userId, message, link, lida, data }]` (`link` = `null` ou rota do front) — **ordem cronológica crescente** (o front inverte) |
| `PATCH /api/notificacoes/{id}/lida` | dono | `204` (é dele? senão 403) |
| `PATCH /api/notificacoes/lidas` | Autenticado | `204` — marca **todas** as do utilizador |

⚠️ `PATCH /notificacoes/lidas` vs `/{id}/lida`: rotas diferentes, mas usar `{id:guid}`.

**10.2 Solicitações** — `ISolicitacaoService`. **Visibilidade no `GET`:** `FUNCIONARIO` → só as suas (`SolicitanteId == userId`); `TECNICO`, `GESTOR`, `ADMIN` → todas.

| Método + rota | Quem | Body | Resposta |
|---|---|---|---|
| `GET /api/solicitacoes` | todos (filtrado) | — | `[SolicitacaoDto]` |
| `POST /api/solicitacoes` | Autenticado | `{ titulo, descricao, dispositivoFisicoId (ou null), prioridade, categoria, anexos: [{nome,tipo,dataUrl}] }` | `201 SolicitacaoDto`. **`solicitanteId` vem do token**. Estado inicial `ABERTA`, `dataCriacao` = hoje. **Notifica todos os TECNICO** (`"Nova solicitação: {titulo}"`, link `/atendimentos`) |
| `POST /api/solicitacoes/{id}/fechar` | solicitante, GESTOR, ADMIN | — | `SolicitacaoDto` (só se `RESOLVIDA`, senão 400 *"Só solicitações resolvidas podem ser fechadas."*) → `FECHADA` |
| `POST /api/solicitacoes/{id}/avaliar` | **só o solicitante** | `{ estrelas (1–5), comentario }` | `SolicitacaoDto`. Só em `RESOLVIDA`/`FECHADA` e se ainda sem avaliação |
| `POST /api/solicitacoes/{id}/resolver-base` | só o solicitante | — | `SolicitacaoDto` → estado `RESOLVIDA`, `resolvidaViaBase = true` (só a partir de `ABERTA`) |

**10.3 Base de conhecimento** — `BaseConhecimentoService`. **Leitura:** todos. **Escrita:** `ADMIN`, `TECNICO`, `GESTOR`.

| Método + rota | Body | Resposta |
|---|---|---|
| `GET /api/base-conhecimento` | — | `[{ id, titulo, conteudo, tags: [], categoria, autorId }]` |
| `POST /api/base-conhecimento` | `{ titulo, conteudo, tags: [], categoria }` | `201 ArtigoDto` (`autorId` = utilizador do token) |
| `GET /api/base-conhecimento/buscar?q=texto` | — | `ArtigoDto[]` — um array simples de artigos |

**Algoritmo do `buscar`** (imita `knowledgeEngine.js`): normalizar (minúsculas + remover acentos) → tokens com > 2 letras → pontuação: token no título ×3, nas tags ×2, no conteúdo ×1 → devolver os **3** com maior pontuação > 0, por ordem decrescente. Trabalha em memória (poucos artigos). `q` com menos de 3 caracteres → `[]` (nunca erro).

**10.4 Chat** — `ChatService`. Cada conversa = 1 solicitação. **Visibilidade** (igual a `utils/chat.js`):
- `FUNCIONARIO` → conversas das **suas** solicitações
- `TECNICO` → solicitações para as quais **tem ordem** (`OrdemReparo.TecnicoId == userId`) — **dependência externa** para o Módulo 11 (usa `IOrdemRepository` só para leitura da relação técnico↔ordem; não se reimplementa Atendimentos aqui)
- `ADMIN`, `GESTOR` → todas

| Método + rota | Body | Resposta |
|---|---|---|
| `GET /api/chat/mensagens-todas` | — | `[{ id, solicitacaoId, autorId, texto, data, hora, lida }]` (`hora` = `"09:15"`) **ordenado por data+hora** |
| `POST /api/chat/mensagens` | `{ solicitacaoId, texto }` | `201 MensagemDto`. Autor = token; sem acesso à conversa → 403; texto vazio → 400. **Notifica a outra parte** (link `/chat`) |
| `PATCH /api/chat/conversas/{solicitacaoId}/lidas` | — | `204` — marca como lidas as mensagens **de outros** nessa conversa |

### 4.14 Dependency Injection
Registar `INotificacaoService`, `ISolicitacaoService`, `IBaseConhecimentoService`, `IChatService` + respetivos repositórios em `AddApplicationServices()`.

### 4.15 Controller
`NotificacoesController`, `SolicitacoesController`, `BaseConhecimentoController`, `ChatController`.

### 4.16 Authorization
Ver tabelas por subseção em 4.13. Regra geral: nenhum GET destas 4 subseções pode devolver 403 (regra de ouro nº 4) — filtram por visibilidade em vez de bloquear.

### 4.17 Swagger
Checklist S.4 em cada um dos 4 controllers, com `[Tags]` "Notificações", "Solicitações", "Base de Conhecimento", "Chat".

### 4.18 Migration
Nenhuma nova (tabelas já criadas no Módulo 3).

### 4.19 Tests
Login como Pedro (u5) → `GET /solicitacoes` só devolve as dele (`s1,s3,s6,s8,s9`); como João → todas; `mensagens-todas` do Pedro só traz conversas dele; `buscar?q=wifi` devolve o artigo do Wi-Fi.

### 4.20 Integração Front-end
Alimenta `notificacoes`, `solicitacoes`, `artigos`, `mensagens` em `AppContext.jsx`. `NotificacaoService.CriarAsync` será chamado a partir dos Módulos 11/12 também.

### 4.21 Critério de conclusão
Login como Pedro (u5) → `GET /solicitacoes` só devolve as dele; como João → todas; `mensagens-todas` do Pedro só traz conversas dele; `buscar?q=wifi` devolve o artigo do Wi-Fi.

### NÍVEL 3 — Tarefas (ordem exacta)

1. Criar `Dtos/Solicitacoes/*.cs`, `Dtos/Notificacoes/*.cs`, `Dtos/BaseConhecimento/*.cs`, `Dtos/Chat/*.cs` (4.9).
2. Criar `INotificacaoRepository`/`NotificacaoRepository` e `INotificacaoService`/`NotificacaoService` (`CriarAsync`, `NotificarPerfisAsync`) — **primeiro**, porque os outros 3 subserviços deste módulo (e, mais tarde, os Módulos 11/12) dependem dele.
3. Criar `ISolicitacaoRepository`/`SolicitacaoRepository` (com filtro por `SolicitanteId`).
4. Criar `SolicitacaoMapper`.
5. Criar `ISolicitacaoService`/`SolicitacaoService` com as 4 regras de 4.13 (10.2), incluindo a chamada a `NotificacaoService`.
6. Criar `IArtigoRepository`/`ArtigoRepository`, `ArtigoMapper`, `IBaseConhecimentoService`/`BaseConhecimentoService` (incluindo o algoritmo de `buscar`).
7. Criar `IMensagemRepository`/`MensagemRepository`, `MensagemMapper`, `IChatService`/`ChatService` (com a lógica de visibilidade que consulta `IOrdemRepository` só para leitura — **dependência externa** do Módulo 11, declarada, não implementada aqui).
8. Registar tudo em `AddApplicationServices()`.
9. Criar `NotificacoesController`, `SolicitacoesController`, `BaseConhecimentoController`, `ChatController` com as rotas de 4.13.
10. Aplicar checklist Swagger S.4 aos 4 controllers.
11. Testar manualmente conforme 4.19.

### GATE DE CONCLUSÃO — Módulo 10

```
MÓDULO 10 — Solicitações + Notificações + Base + Chat
  ↓
[VALIDAÇÃO]
  - NotificacaoService.CriarAsync adiciona ao contexto sem SaveChanges próprio?
  - Visibilidade de Solicitações filtra corretamente por perfil (nunca 403)?
  - Algoritmo de busca da Base de Conhecimento devolve no máximo 3 resultados corretos?
  - Visibilidade do Chat filtra por perfil (funcionário/técnico/admin-gestor)?
  - Os endpoints das 4 subseções funcionam com os status/formatos corretos?
  - Swagger documenta os 4 controllers?
  - GET devolve os dados exatos do seed, filtrados por perfil de teste (Pedro vs João)?
  - Front-end consegue consumir?
  ↓
SIM → MÓDULO 10 CONCLUÍDO → avançar para o Módulo 11
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 11 — Atendimentos + Compras `[PLANEADO]` ⭐ o módulo mais difícil

### 4.1 Objetivo do módulo
Implementar o ciclo de vida completo da ordem de reparação — a máquina de estados mais complexa do sistema, com regras de negócio reais, transações multi-tabela e resposta polimórfica — e o fluxo de requisição de compra que se dispara quando falta stock para uma reparação.

### 4.2 Funcionalidades
Assumir solicitação (cria ordem), registar diagnóstico, concluir diagnóstico, comentar, reatribuir, reservar peça (com fallback para requisição de compra), instalar peça, propor solução, responder à validação (aceitar/recusar), guardar solução sugerida (assistente); e, em Compras: listar, aprovar, dar entrada, recusar requisições.

### 4.3 Dependências
Módulo 6 (autenticação), Módulo 9 (Stock — reutiliza `IStockService.CriarUnidadesAsync` e as entidades `ItemStock`/`UnidadeStock`/`MovimentoStock`), Módulo 10 (`Solicitacao` como FK 1–1 de `OrdemReparo`; `NotificacaoService` para todos os avisos deste módulo), Módulo 8 (`DispositivoFisico`/`InstanciaComponente` usados no algoritmo de instalar peça).

### 4.4 Dependentes
Módulo 12 (`RelatorioTecnico.OrdemId` é FK 1–1; só se cria relatório de uma ordem `RESOLVIDO`).

### 4.5 Entities
`OrdemReparo`, `OrdemPecaUsada`, `OrdemRejeicao`, `OrdemHistorico`, `RequisicaoCompra`.

### 4.6 Enums
`EstadoOrdem` (`EM_DIAGNOSTICO`, `EM_REPARACAO`, `AGUARDA_VALIDACAO`, `RESOLVIDO`), `TipoHistoricoOrdem` (`ASSUMIDA`, `DIAGNOSTICO`, `SOLUCAO`, `ACEITE`, `REJEITADA`, `COMENTARIO`, `REATRIBUIDA`), `EstadoRequisicao` (`PENDENTE`, `APROVADA`, `RECUSADA`, `ENTREGUE`); reutiliza `EstadoSolicitacao` (Módulo 10) e `EstadoUnidade`/`TipoMovimento` (Módulo 9).

### 4.7 Relacionamentos
`Solicitacao` 1—0..1 `OrdemReparo` (Restrict); `User` 1—N `OrdemReparo` (técnico, Restrict); `OrdemReparo` 1—N `OrdemPecaUsada`/`OrdemRejeicao`/`OrdemHistorico` (**Cascade**); `OrdemReparo` 1—N `RequisicaoCompra` (opcional, SetNull); `OrdemReparo` 1—N `UnidadeStock` via `ReservadaParaOrdemId` (opcional, SetNull); `ItemStock` 1—N `RequisicaoCompra` (Restrict); `User` 1—N `RequisicaoCompra` (Restrict).

### 4.8 Database
Tabelas já existem (Módulo 3): `OrdensReparo` (`SolicitacaoId` único), `OrdemPecasUsadas`, `OrdemRejeicoes`, `OrdemHistoricos`, `RequisicoesCompra`.

### 4.9 DTOs
```csharp
public class OrdemDto {
  public Guid Id; public Guid SolicitacaoId; public Guid TecnicoId;
  public string Diagnostico = ""; public string? Solucao; public string? SolucaoSugerida;
  public string Estado;
  public List<PecaUsadaDto> PecasUsadas = [];
  public int? TempoGastoMin; public DateOnly DataInicio; public DateOnly? DataFim;
  public List<RejeicaoDto> Rejeicoes = [];
  public List<HistoricoDto> Historico = [];
}
public record PecaUsadaDto(Guid UnidadeStockId, Guid ModeloComponenteId, Guid? InstaladoInstanciaId);
public record RejeicaoDto(string Motivo, DateOnly Data);
public record HistoricoDto(Guid Id, string Tipo, string Texto, string Autor, DateOnly Data);   // Autor = NOME

public class DiagnosticoRequest { [Required] public string Diagnostico; }
public class ComentarioRequest { [Required] public string Texto; }
public class ReatribuirRequest { [Required] public Guid NovoTecnicoId; }
public class ReservarPecaRequest { [Required] public Guid ModeloComponenteId; }
public class InstalarPecaRequest { [Required] public Guid UnidadeStockId; [Required] public Guid DispositivoFisicoId; public Guid? InstanciaAntigaId; }
public class PropostaSolucaoRequest { [Required] public string Solucao; [Required, Range(1,int.MaxValue)] public int TempoGastoMin; }
public class ResponderValidacaoRequest { public bool Aceite; public string Motivo = ""; }
public class SolucaoSugeridaRequest { [Required] public string Solucao; }
public record ReservaOkResponse(bool Ok, OrdemDto Ordem, Guid UnidadeStockId, string Codigo);
public record ReservaFalhaResponse(bool Ok, OrdemDto Ordem, RequisicaoDto Requisicao, Guid RequisicaoId);
public record RequisicaoDto(Guid Id, Guid ItemStockId, Guid ModeloComponenteId, int Quantidade, string Justificativa, Guid SolicitanteId, Guid? OrdemId, DateOnly Data, string Estado);
```
`OrdemDto` — o front assume TODOS estes campos:
```json
{ "id":"guid","solicitacaoId":"guid","tecnicoId":"guid",
  "diagnostico":"","solucao":null,"solucaoSugerida":null,
  "estado":"EM_DIAGNOSTICO",
  "pecasUsadas":[{"unidadeStockId":"guid","modeloComponenteId":"guid","instaladoInstanciaId":"guid|null"}],
  "tempoGastoMin":null,"dataInicio":"2026-08-10","dataFim":null,
  "rejeicoes":[{"motivo":"...","data":"2026-08-16"}],
  "historico":[{"id":"guid","tipo":"ASSUMIDA","texto":"...","autor":"João Técnico","data":"2026-08-10"}] }
```
`pecasUsadas`, `rejeicoes`, `historico` **sempre arrays**. `historico[].autor` = **nome** (string, junta-se no mapper via `User.Nome`). `diagnostico` = `""` (não `null`) quando vazio.

`RequisicaoDto`:
```json
{ "id":"guid","itemStockId":"guid","modeloComponenteId":"guid","quantidade":1,
  "justificativa":"...","solicitanteId":"guid","ordemId":"guid|null","data":"2026-09-10","estado":"PENDENTE" }
```

### 4.10 Validation
Ver máquina de estados e algoritmos A/B/C em 4.13 — a maioria das regras deste módulo são de negócio (dependentes de estado), não de forma de DTO. `tempoGastoMin` > 0; `motivo` obrigatório quando `aceite = false`.

### 4.11 Mappers
`OrdemMapper` (`Historico[].Autor` = nome, resolvido via `User`), `RequisicaoMapper`.

### 4.12 Repository
`IOrdemRepository` — sempre com `Include` de `PecasUsadas`, `Rejeicoes`, `Historico`→`Autor`. `ICompraRepository`.

### 4.13 Service — `IAtendimentoService`/`AtendimentoService`, `IComprasService`/`ComprasService`

#### Máquina de estados

```
Solicitacao:  ABERTA ──assumir──► EM_ATENDIMENTO ──propor-solucao──► AGUARDA_VALIDACAO ──aceite──► RESOLVIDA ──fechar──► FECHADA
                                        ▲                                     │
                                        └─────────────── recusa ──────────────┘

OrdemReparo:  EM_DIAGNOSTICO ──concluir-diagnostico──► EM_REPARACAO ──propor-solucao──► AGUARDA_VALIDACAO ──aceite──► RESOLVIDO
                    ▲                                                                       │
                    └───────────────────────────── recusa (guarda rejeição) ────────────────┘
```

#### Endpoints — Atendimentos

`GET /api/atendimentos/ordens` — visibilidade: `ADMIN`/`GESTOR`/`TECNICO` → todas; `FUNCIONARIO` → só as ordens das **suas** solicitações (⚠️ não pode dar 403: hidratação). Devolve `[OrdemDto]`.

| Método + rota | Quem | Body | O que faz | Resposta |
|---|---|---|---|---|
| `POST /api/atendimentos/solicitacoes/{solicitacaoId}/assumir` | `TECNICO`, `ADMIN` | — | Solicitação tem de estar `ABERTA` (senão 409 *"Esta solicitação já foi assumida."*). Cria ordem (`EM_DIAGNOSTICO`, `tecnicoId` = token, `dataInicio` = hoje, `diagnostico = ""`). Solicitação → `EM_ATENDIMENTO`. Histórico `ASSUMIDA` (*"Solicitação assumida por {nome}."*). Notifica o solicitante | `201 OrdemDto` |
| `PATCH /api/atendimentos/ordens/{id}/diagnostico` | técnico da ordem, `ADMIN` | `{ diagnostico }` | Guarda o texto. Só em `EM_DIAGNOSTICO` | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/concluir-diagnostico` | técnico da ordem, `ADMIN` | — | `EM_DIAGNOSTICO` → `EM_REPARACAO`. Histórico `DIAGNOSTICO` com o texto. **Se `diagnostico` vazio → 400 *"Preencha o diagnóstico antes de avançar."*** | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/comentarios` | técnico da ordem, `ADMIN`, `GESTOR` | `{ texto }` | Histórico `COMENTARIO` com `autor` = token | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/reatribuir` | `ADMIN`, `GESTOR` | `{ novoTecnicoId }` | Novo id tem de ser `TECNICO` (senão 400). Muda `tecnicoId`. Histórico `REATRIBUIDA`. Notifica o novo técnico | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/reservar-peca` | técnico da ordem, `ADMIN` | `{ modeloComponenteId }` | Ver **algoritmo A** | Ver abaixo (**200 nos dois casos**) |
| `POST /api/atendimentos/ordens/{id}/instalar-peca` | técnico da ordem, `ADMIN` | `{ unidadeStockId, dispositivoFisicoId, instanciaAntigaId (ou null) }` | Ver **algoritmo B** | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/propor-solucao` | técnico da ordem, `ADMIN` | `{ solucao, tempoGastoMin }` | Ordem → `AGUARDA_VALIDACAO`; guarda `solucao` e `tempoGastoMin` (> 0); Solicitação → `AGUARDA_VALIDACAO`; histórico `SOLUCAO`; notifica o solicitante (link `/solicitacoes`) | `OrdemDto` (com `solicitacaoId`) |
| `POST /api/atendimentos/ordens/{id}/responder-validacao` | **só o solicitante** da solicitação | `{ aceite: bool, motivo }` | Ver **algoritmo C** | `OrdemDto` |
| `POST /api/atendimentos/ordens/{id}/solucao-sugerida` | técnico da ordem, `ADMIN` | `{ solucao }` | Guarda `solucaoSugerida` (vinda do assistente) | `OrdemDto` |

**Algoritmo A — reservar peça** (transação única):
1. Ordem em `EM_REPARACAO` (senão 400).
2. Get-or-create `ItemStock` do `modeloComponenteId` — **reutiliza** `IStockService` (Módulo 9), não reimplementa.
3. Procurar 1 `UnidadeStock` `DISPONIVEL` desse item.
4. **Se existe:** `Estado = RESERVADA`, `ReservadaParaOrdemId = ordem.Id`; nova `OrdemPecaUsada` (`instaladoInstanciaId = null`); `MovimentoStock` `RESERVA` qty 1 (*"Reservado para ordem …"*). Resposta:
   ```json
   { "ok": true, "ordem": OrdemDto, "unidadeStockId": "guid", "codigo": "RAM-050" }
   ```
5. **Se não existe:** criar `RequisicaoCompra` (`PENDENTE`, qty 1, `justificativa` = *"Sem stock para a ordem …"*, `solicitanteId` = token, `ordemId`); notificar `GESTOR`/`ADMIN` (link `/compras`, via `NotificacaoService` do Módulo 10). Resposta:
   ```json
   { "ok": false, "ordem": OrdemDto, "requisicao": RequisicaoDto, "requisicaoId": "guid" }
   ```
   O front usa `resultado.requisicao.itemStockId` → o `RequisicaoDto` **tem de** incluir `itemStockId`.

**Algoritmo B — instalar peça:**
1. A unidade está `RESERVADA` **para esta ordem** e existe em `pecasUsadas` (senão 400).
2. (Se possível) validar que o dispositivo é compatível com o modelo do componente (reutiliza `Compatibilidade`, Módulo 7).
3. Criar `InstanciaComponente` (`codigo` = código da unidade, `dispositivoFisicoId`, `modeloComponenteId`, `INSTALADO`).
4. Unidade → `INSTALADA`. Preencher `OrdemPecaUsada.InstaladoInstanciaId`.
5. Se `instanciaAntigaId` ≠ null: essa instância tem de ser do mesmo dispositivo → **remover** a instância antiga. *(Desafio opcional: criar uma `UnidadeStock` `AVARIADA` com o código antigo.)*
6. `MovimentoStock` `INSTALACAO` (*"Instalado no dispositivo NB-001"*).

**Algoritmo C — responder validação:**
- Ordem tem de estar `AGUARDA_VALIDACAO`; quem chama é o solicitante.
- `aceite = true`: ordem → `RESOLVIDO`, `dataFim` = hoje; solicitação → `RESOLVIDA`; histórico `ACEITE`; notifica o técnico.
- `aceite = false`: `motivo` obrigatório (400 se vazio); nova `OrdemRejeicao`; ordem → `EM_DIAGNOSTICO`; solicitação → `EM_ATENDIMENTO`; histórico `REJEITADA` (texto = motivo); notifica o técnico.
- O front, a seguir, faz `GET /solicitacoes` → tem de refletir o novo estado.

#### Endpoints — Compras

`GET /api/compras` — `ADMIN`/`GESTOR` → todas (ordem de criação **crescente**; o front inverte); **qualquer outro perfil → `[]` (200, não 403)**.

| Método + rota | Quem | O que faz | Resposta |
|---|---|---|---|
| `POST /api/compras/{id}/aprovar` | `GESTOR`, `ADMIN` | `PENDENTE` → `APROVADA` (senão 400). Notifica quem a pediu | `RequisicaoDto` |
| `POST /api/compras/{id}/entrada` | `GESTOR`, `ADMIN` | Só `APROVADA`. Cria `quantidade` unidades `DISPONIVEL` no `ItemStock` (**reutiliza** `IStockService.CriarUnidadesAsync`, Módulo 9) + `MovimentoStock` `ENTRADA` (*"Entrada da requisição …"*). Requisição → `ENTREGUE` | `RequisicaoDto` |
| *(extra)* `POST /api/compras/{id}/recusar` | `GESTOR`, `ADMIN` | `PENDENTE` → `RECUSADA` | `RequisicaoDto` |

> 💡 **Reutilizar** a lógica de "criar unidades com código único" do Módulo 9 num método partilhado (`IStockService.CriarUnidadesAsync`) — não copiar código.

### 4.14 Dependency Injection
Registar `IAtendimentoService`, `IOrdemRepository`, `IComprasService`, `ICompraRepository` em `AddApplicationServices()`. Injetar `IStockService` (Módulo 9) e `INotificacaoService` (Módulo 10) por construtor.

### 4.15 Controller
`AtendimentosController` (rota base `api/atendimentos`), `ComprasController` (rota base `api/compras`).

### 4.16 Authorization
Ver tabelas em 4.13 — coluna "Quem". Regra especial: `responder-validacao` só o solicitante da própria solicitação (não basta ser `FUNCIONARIO` genérico).

### 4.17 Swagger
Checklist S.4 em ambos os controllers. Atenção especial ao endpoint `reservar-peca`, que tem **duas formas de resposta possível** (200 com `ok:true` ou `ok:false`) — documentar ambos os schemas via `[ProducesResponseType]` com um tipo de união explícito ou union DTO (`PONTO A DECIDIR`: Swashbuckle não modela `oneOf` nativamente; documentar como comentário `<remarks>` com os dois exemplos de JSON é a alternativa pragmática).

### 4.18 Migration
Nenhuma nova (tabelas já criadas no Módulo 3).

### 4.19 Tests
Fluxo completo à mão no Swagger UI: João assume `s1` → diagnóstico → concluir → reservar `mc3` (Fonte, **sem stock** → `ok:false` + requisição) → Carlos aprova e dá entrada → João reserva de novo (`ok:true`) → instala → propõe solução → Pedro aceita → `s1` fica `RESOLVIDA`. `PONTO A DECIDIR`: suite xUnit dedicada aos algoritmos A/B/C, sugerida como prioridade alta no Módulo 14.

### 4.20 Integração Front-end
`OrdemDto` e `RequisicaoDto` alimentam `ordensReparo` e `requisicoesCompra` em `AppContext.jsx`. O front usa `ordem.solicitacaoId` depois de `propor-solucao`, e `resultado.requisicao.itemStockId` depois de `reservar-peca` — campos que têm de estar presentes exatamente com esses nomes.

### 4.21 Critério de conclusão
Fluxo completo à mão no Swagger UI (4.19) funciona de ponta a ponta sem erros, incluindo o caminho sem stock (requisição de compra) e o caminho de recusa da validação.

### NÍVEL 3 — Tarefas (ordem exacta)

1. Criar `Dtos/Atendimentos/*.cs` e `Dtos/Compras/*.cs` (4.9).
2. Criar `IOrdemRepository`/`OrdemRepository` com `Include` de `PecasUsadas`, `Rejeicoes`, `Historico`→`Autor`.
3. Criar `ICompraRepository`/`CompraRepository`.
4. Criar `OrdemMapper` (resolve `Historico[].Autor` para nome) e `RequisicaoMapper`.
5. Criar `IAtendimentoService`/`AtendimentoService` — implementar primeiro `assumir`, `diagnóstico`, `concluir-diagnóstico`, `comentários`, `reatribuir` (fluxo linear sem stock).
6. Implementar o **algoritmo A** (reservar-peça) em `AtendimentoService`, injetando `IStockService` (Módulo 9) e `INotificacaoService` (Módulo 10) — **dependência externa**, não reimplementar.
7. Implementar o **algoritmo B** (instalar-peça), injetando validação de `Compatibilidade` (Módulo 7) — **dependência externa**.
8. Implementar `propor-solucao` e o **algoritmo C** (responder-validação), incluindo a sincronização de estado com `Solicitacao` (Módulo 10).
9. Implementar `solucao-sugerida`.
10. Criar `IComprasService`/`ComprasService` (`aprovar`, `entrada` reutilizando `IStockService.CriarUnidadesAsync`, `recusar`).
11. Registar tudo em `AddApplicationServices()`.
12. Criar `AtendimentosController` com as 10 rotas de 4.13, `ComprasController` com as 3 rotas.
13. Aplicar checklist Swagger S.4 a ambos.
14. Testar o fluxo completo (4.19) manualmente.

### GATE DE CONCLUSÃO — Módulo 11

```
MÓDULO 11 — Atendimentos + Compras
  ↓
[VALIDAÇÃO]
  - Máquina de estados de Solicitacao e OrdemReparo respeitada em todas as transições?
  - Algoritmo A (reservar-peça) cobre os dois caminhos (com stock / sem stock)?
  - Algoritmo B (instalar-peça) cria InstanciaComponente e atualiza UnidadeStock corretamente?
  - Algoritmo C (responder-validação) cobre aceite e recusa, com histórico correto?
  - IStockService.CriarUnidadesAsync é REUTILIZADO em Compras (não duplicado)?
  - Visibilidade de GET /atendimentos/ordens e GET /compras nunca dá 403?
  - Os 13 endpoints funcionam com os status/formatos corretos, incluindo a resposta polimórfica de reservar-peca?
  - Swagger documenta ambos os controllers?
  - Fluxo completo do seed (assumir → diagnosticar → reservar sem stock → aprovar/entrada → reservar com stock → instalar → propor → validar) funciona de ponta a ponta?
  - Front-end consegue consumir (campos solicitacaoId, itemStockId presentes)?
  ↓
SIM → MÓDULO 11 CONCLUÍDO → avançar para o Módulo 12
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 12 — Relatórios Técnicos `[PLANEADO]`

### 4.1 Objetivo do módulo
Implementar o documento formal que fecha uma ordem de reparação resolvida, com uma máquina de estados própria (`rascunho` → `finalizado` → `aprovado`) e auditoria completa de edições (diff campo a campo).

### 4.2 Funcionalidades
Listar (filtrado por perfil), consultar um, criar (a partir de uma ordem `RESOLVIDO`), editar (com histórico automático), finalizar, reabrir, aprovar.

### 4.3 Dependências
Módulo 11 (`RelatorioTecnico.OrdemId` é FK 1–1 para `OrdemReparo`; só se cria relatório de ordem `RESOLVIDO`), Módulo 6 (autenticação; `AssinaturaResponsavel` pode usar `User.Assinatura`), Módulo 10 (`NotificacaoService`).

### 4.4 Dependentes
Nenhum módulo de domínio depende deste — é o último módulo de domínio antes da integração (Módulo 13).

### 4.5 Entities
`RelatorioTecnico`, `RelatorioHistorico`. Classe auxiliar: `PecaRelatorio` (owned/JSONB).

### 4.6 Enums
`StatusRelatorio` (`rascunho`, `finalizado`, `aprovado` — minúsculas, como o front), `AcaoRelatorio` (`CRIADO`, `EDITADO`, `FINALIZADO`, `REABERTO`, `APROVADO`).

### 4.7 Relacionamentos
`OrdemReparo` 1—0..1 `RelatorioTecnico` (Restrict); `User` 1—N `RelatorioTecnico` (autor, Restrict); `RelatorioTecnico` 1—N `RelatorioHistorico` (**Cascade**).

### 4.8 Database
Tabelas já existem (Módulo 3): `RelatoriosTecnicos` (`OrdemId` único, `PecasUsadas` jsonb), `RelatorioHistoricos`.

### 4.9 DTOs
```csharp
public class RelatorioDto {
  public Guid Id; public Guid OrdemId; public Guid AutorId;
  public DateOnly CriadoEm; public DateOnly AtualizadoEm; public string Status;
  public string Sumario = ""; public string Diagnostico = ""; public string SolucaoAplicada = "";
  public List<PecaRelatorioDto> PecasUsadas = [];
  public int? TempoGastoMin; public string Procedimentos = ""; public string Observacoes = "";
  public string AssinaturaTecnico = ""; public string AssinaturaResponsavel = ""; public string ComentariosInternos = "";
  public List<RelatorioHistoricoDto> Historico = [];
}
public record PecaRelatorioDto(string Nome, int Quantidade, string Codigo);
public record RelatorioHistoricoDto(DateOnly Data, Guid AutorId, string Acao, string Campo, string ValorAntigo, string ValorNovo);
public class CreateRelatorioRequest { [Required] public Guid OrdemId; public string Sumario=""; public string Diagnostico=""; public string SolucaoAplicada=""; public List<PecaRelatorioDto> PecasUsadas=[]; public int? TempoGastoMin; public string Procedimentos=""; public string Observacoes=""; public string AssinaturaTecnico=""; public string AssinaturaResponsavel=""; public string ComentariosInternos=""; }
public class UpdateRelatorioRequest { /* mesmos campos de CreateRelatorioRequest, todos opcionais (subconjunto) */ }
```
`RelatorioDto`:
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

### 4.10 Validation
Ordem tem de estar `RESOLVIDO` para criar relatório (senão 400); já existe relatório para a ordem → 409; não editável se `aprovado` (400).
> ⚠️ **Não inventar regras que o front não conhece** (ex.: exigir `sumario` para finalizar) — o front não tem UI para mostrar esse erro. Só as regras aqui descritas.

### 4.11 Mappers
`RelatorioMapper`.

### 4.12 Repository
`IRelatorioRepository`.

### 4.13 Service — `IRelatorioService`/`RelatorioService`

**Visibilidade no `GET`/`GET /{id}`** — nunca 403 na lista:
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

**Algoritmo de diff (usado em `PATCH`):** para cada propriedade do request que difira do valor atual da entidade, criar uma `RelatorioHistorico` com `Acao = EDITADO`, `Campo` = nome da propriedade, `ValorAntigo`/`ValorNovo` = representação em string (para `PecasUsadas`, serializar a lista completa em JSON como valor). Todas as linhas de histórico da mesma chamada partilham a mesma `Data`/`AutorId`.

### 4.14 Dependency Injection
Registar `IRelatorioService`, `IRelatorioRepository` em `AddApplicationServices()`. Injetar `IOrdemRepository` (Módulo 11, leitura) e `INotificacaoService` (Módulo 10) por construtor.

### 4.15 Controller
`RelatoriosTecnicosController`, rota base `api/relatorios-tecnicos`.

### 4.16 Authorization
Ver tabela em 4.13 — coluna "Quem". Visibilidade de leitura nunca dá 403 (filtra); escrita segue as regras de autor/`ADMIN`/`GESTOR`.

### 4.17 Swagger
Checklist S.4, `[Tags("Relatórios Técnicos")]`.

### 4.18 Migration
Nenhuma nova (tabelas já criadas no Módulo 3).

### 4.19 Tests
Os 8 relatórios do seed aparecem (Carlos/GESTOR vê 5: `rt1,rt2,rt4,rt6,rt7`); um técnico cria → edita (vê `EDITADO` no histórico) → finaliza → gestor aprova; segunda criação para a mesma ordem dá 409.

### 4.20 Integração Front-end
`RelatorioDto` alimenta `relatoriosTecnicos` em `AppContext.jsx`. É também o DTO envolvido no bug do front documentado no Módulo 13 (`gerarRelatorio()` sem `await`).

### 4.21 Critério de conclusão
Os 8 relatórios do seed aparecem, filtrados corretamente por perfil; o fluxo criar → editar → finalizar → aprovar funciona; segunda criação para a mesma ordem dá 409.

### NÍVEL 3 — Tarefas (ordem exacta)

1. Criar `Dtos/RelatoriosTecnicos/*.cs` (4.9).
2. Criar `IRelatorioRepository`/`RelatorioRepository`.
3. Criar `RelatorioMapper`.
4. Criar `IRelatorioService`/`RelatorioService` — implementar primeiro `GetAllAsync`/`GetByIdAsync` com a visibilidade por perfil de 4.13.
5. Implementar `CreateAsync` (valida ordem `RESOLVIDO`, unicidade 1-1, histórico `CRIADO`).
6. Implementar `UpdateAsync` com o **algoritmo de diff** (histórico `EDITADO` por campo alterado).
7. Implementar `FinalizarAsync`, `ReabrirAsync`, `AprovarAsync` (incluindo preencher `AssinaturaResponsavel` com o nome de quem aprova, se vazia).
8. Registar em `AddApplicationServices()`.
9. Criar `RelatoriosTecnicosController` com as 6 rotas de 4.13.
10. Aplicar checklist Swagger S.4.
11. Testar manualmente conforme 4.19.

### GATE DE CONCLUSÃO — Módulo 12

```
MÓDULO 12 — Relatórios Técnicos
  ↓
[VALIDAÇÃO]
  - Visibilidade por perfil (ADMIN/TECNICO todos, GESTOR só finalizado+aprovado, FUNCIONARIO só aprovado das suas) correta?
  - CreateAsync valida ordem RESOLVIDO e unicidade 1-1 (409 na segunda criação)?
  - UpdateAsync gera uma linha de histórico EDITADO por campo alterado?
  - Máquina de estados rascunho→finalizado→aprovado (e reabrir) respeitada?
  - AssinaturaResponsavel é preenchida automaticamente ao aprovar, se vazia?
  - Os 6 endpoints funcionam com os status/formatos corretos?
  - Swagger documenta o módulo?
  - Os 8 relatórios do seed aparecem corretamente filtrados?
  - Front-end consegue consumir?
  ↓
SIM → MÓDULO 12 CONCLUÍDO → avançar para o Módulo 13
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 13 — Ligação ao Front-end `[PLANEADO]`

> Divisão de trabalho: **este módulo é feito por Claude** (o utilizador acompanha). É o único módulo do documento onde a implementação de código não é do utilizador — mas o critério de conclusão e o contrato continuam a ser exatamente os definidos nos módulos anteriores.

### 4.1 Objetivo do módulo
Verificar o contrato da API endpoint a endpoint contra o que `AppContext.jsx`/`api.js` esperam, corrigir os bugs conhecidos do front, e validar o sistema de ponta a ponta com os testes E2E existentes.

### 4.2 Funcionalidades
Nenhuma funcionalidade nova de API — este módulo é validação + pequenos ajustes no front.

### 4.3 Dependências
Módulos 0–12 completos: em particular, os **17 GET de hidratação** têm de responder 200 para os 4 perfis.

### 4.4 Dependentes
Módulo 14 (extras só fazem sentido com o sistema já a funcionar ponta a ponta).

### 4.5 Entities
Não aplicável (nenhuma entidade nova).

### 4.6 Enums
Não aplicável.

### 4.7 Relacionamentos
Não aplicável.

### 4.8 Database
Não aplicável — usa a BD já semeada (Módulo 5).

### 4.9 DTOs
Não aplicável (usa os DTOs já definidos nos Módulos 6–12) — a tarefa aqui é **comparar** esses DTOs com o que o front lê, não criar novos.

### 4.10 Validation
Não aplicável.

### 4.11 Mappers
Não aplicável.

### 4.12 Repository
Não aplicável.

### 4.13 Service
Não aplicável.

### 4.14 Dependency Injection
Não aplicável.

### 4.15 Controller
Não aplicável (nenhum controller novo).

### 4.16 Authorization
Não aplicável.

### 4.17 Swagger
Reutiliza a exportação (`swagger.json`, Módulo 1 tarefa 14) como fonte de verdade para a comparação com o front.

### 4.18 Migration
Não aplicável.

### 4.19 Tests
Testes E2E existentes (`novati/e2e/integracao.spec.js`, 9 testes) + percurso manual completo com os 4 perfis.

### 4.20 Integração Front-end
Secção central deste módulo — ver tarefas abaixo.

### 4.21 Critério de conclusão
Os 9 testes de `integracao.spec.js` passam **e** o fluxo completo com os 4 perfis funciona manualmente (funcionário abre pedido → técnico assume → reserva → instala → propõe → funcionário valida e avalia → técnico gera relatório → gestor aprova).

### NÍVEL 3 — Tarefas (ordem exacta)

1. **Confirmar que os 17 GET respondem 200 para os 4 perfis.** Script (PowerShell): para cada email de utilizador demo → login → chamar os 17 GETs → nenhum pode falhar:
   ```
   /users/me  /users/directory  /catalogo/modelos-dispositivo  /catalogo/modelos-componente
   /catalogo/compatibilidades  /dispositivos/localizacoes  /dispositivos  /dispositivos/instancias
   /stock/itens  /stock/unidades  /stock/movimentos  /solicitacoes  /atendimentos/ordens
   /compras  /base-conhecimento  /notificacoes  /chat/mensagens-todas  /relatorios-tecnicos
   ```
   ```
   Arquivo: script.ps1 (temporário, escrito por Claude quando este módulo começa)
   Responsabilidade: validar a regra de ouro nº 4 (nenhum GET de hidratação dá 401/403/404/500) antes de ligar o front
   Depende de: Módulos 0–12 concluídos
   É utilizado por: este módulo
   Resultado esperado: 17 × 4 = 68 chamadas, todas 200
   ```

2. **Exportar o contrato** (`swagger.json`, Módulo 1 tarefa 14) e comparar, endpoint a endpoint, com as chamadas de `AppContext.jsx` (rota, método, body, campos da resposta, códigos de erro). Devolver uma lista de discrepâncias **antes** de ligar o front.

3. **Corrigir bugs conhecidos do front** (pequenas alterações, já identificadas na análise inicial — Módulo 0/F.1):
   - `novati/.env`: corrigir o comentário (já não é NestJS); `src/lib/api.js`: fallback da URL de `3000` → `3333`.
   - **Bug real:** `OrdemReparoDetail.jsx` → `gerarRelatorio()` chama `criarRelatorioTecnico(...)` **sem `await`** — a função é assíncrona, logo navegaria para `/relatorios-tecnicos/[object Promise]/editar`. Passar a `async/await`.
   - **Corrida:** `atualizarDiagnostico(...)` e `concluirDiagnostico(...)` são chamados em sequência **sem await** (linha ~119 do mesmo ficheiro) — o segundo pode chegar ao servidor antes do primeiro e falhar a regra "diagnóstico vazio". Passar a `await` em sequência.
   - Robustez: trocar `Promise.all` por `Promise.allSettled` na hidratação, para 1 endpoint com falha não deslogar o utilizador (e mostrar `notify` do que falhou).
   - Mostrar erros de `criarSolicitacao`, `assumirSolicitacao`, etc. ao utilizador (hoje as promessas rejeitadas ficam sem `catch` em vários handlers → erros silenciosos).
   ```
   ALTERAR: novati/.env, novati/src/lib/api.js, novati/src/pages/OrdemReparoDetail.jsx, novati/src/context/AppContext.jsx
   ```

4. **Correr o sistema completo:**
   ```powershell
   # terminal 1
   cd "d:\Nova pasta\backend"; dotnet run
   # terminal 2
   cd "d:\Nova pasta\novati"; npm run dev
   # terminal 3
   cd "d:\Nova pasta\novati"; npx playwright test
   ```

5. **Validar manualmente o fluxo completo** com os 4 perfis: funcionário abre pedido → técnico assume → reserva → instala → propõe → funcionário valida e avalia → técnico gera relatório → gestor aprova.

### GATE DE CONCLUSÃO — Módulo 13

```
MÓDULO 13 — Ligação ao Front-end
  ↓
[VALIDAÇÃO]
  - Os 17 GET respondem 200 para os 4 perfis (68 chamadas)?
  - O swagger.json bate certo com AppContext.jsx/api.js, sem discrepâncias por resolver?
  - Os bugs conhecidos do front foram corrigidos (gerarRelatorio await, corrida de diagnóstico, .env, allSettled, catch nos handlers)?
  - Os 9 testes de integracao.spec.js passam?
  - O fluxo manual completo com os 4 perfis funciona sem erros?
  ↓
SIM → MÓDULO 13 CONCLUÍDO → avançar para o Módulo 14 (opcional)
NÃO → corrigir módulo atual → validar novamente
```

Estado: `[PLANEADO]`

---

## MÓDULO 14 — Extras (opcionais) `[PLANEADO]`

### 4.1 Objetivo do módulo
Elevar o projeto de "funcional" para "profissional", só depois de o sistema completo (Módulos 0–13) estar a funcionar ponta a ponta. Nenhum destes itens é pré-requisito de outro módulo.

### 4.2 Funcionalidades
Lista de extras independentes entre si — ver tabela abaixo. Cada um pode ser feito isoladamente, em qualquer ordem, exceto quando explicitamente indicado.

### 4.3 Dependências
Módulo 13 concluído (sistema integrado e testado ponta a ponta).

### 4.4 Dependentes
Nenhum — é o último módulo do plano.

### 4.5–4.18
Variam por extra — ver tabela. Nenhum extra introduz entidades novas obrigatórias, exceto o `SlaEscalationService` (não cria tabela nova, só lê `Solicitacao`/`Notificacao` já existentes).

### 4.19 Tests
O extra "Testes unitários" **é** o próprio conteúdo de teste do módulo — xUnit + `Microsoft.EntityFrameworkCore.InMemory` ou Testcontainers, focado nos Services mais complexos (`reservar-peca`, `responder-validacao`, Módulo 11).

### 4.20 Integração Front-end
Nenhum destes extras muda o contrato existente (regras de ouro F.2 mantêm-se); paginação, se implementada, teria de ser combinada com o front antes de mudar o formato de resposta das listas — `PONTO A DECIDIR` se/quando isso avançar.

### 4.21 Critério de conclusão
Cada extra tem o seu próprio critério — ver "O que aprendes" na tabela; não há um critério único porque os extras são independentes e opcionais.

| Extra | O que aprendes |
|---|---|
| **`SlaEscalationService`** (`BackgroundService`, corre a cada X min, notifica GESTORes de solicitações fora do prazo: BAIXA 72h · MEDIA 48h · ALTA 24h · URGENTE 4h — como `utils/sla.js`; o NestJS antigo tinha um) | Serviços em background, `IServiceScopeFactory` |
| **Testes unitários** (xUnit + `Microsoft.EntityFrameworkCore.InMemory` ou Testcontainers) dos Services (`reservar-peca`, `responder-validacao`) | Testabilidade que a arquitetura em camadas dá |
| **Concorrência otimista**: 2 técnicos a reservarem a última peça em simultâneo → `xmin` do Postgres como *concurrency token* | Concorrência, `DbUpdateConcurrencyException` → 409 |
| **Logs estruturados** (`ILogger`, Serilog) + `GET /api/health` com verificação da BD (`AddHealthChecks().AddNpgSql`) | Observabilidade |
| **Segredos fora do código**: `dotnet user-secrets` para `Jwt:Key` e connection string | Segurança básica |
| **Paginação** (`?page=&pageSize=`) nas listas grandes (movimentos, notificações) | Performance |
| **`docker-compose.yml`** (API + Postgres) | Deploy |
| **Refresh tokens** | Autenticação a sério |
| **Rate limiting** no `/auth/login` (`AddRateLimiter`) | Proteção contra força bruta |

### NÍVEL 3 — Tarefas
Não há uma ordem exata obrigatória — cada extra é independente. Ordem sugerida por valor/risco: testes unitários → logs estruturados → segredos fora do código → concorrência otimista → SLA → paginação → docker-compose → refresh tokens → rate limiting. `PONTO A DECIDIR`: prioridade real a definir pelo utilizador quando chegar a este módulo.

### GATE DE CONCLUSÃO — Módulo 14

```
MÓDULO 14 — Extras
  ↓
[VALIDAÇÃO] — por extra escolhido, individualmente
  ↓
SIM (extra escolhido funciona) → extra CONCLUÍDO → escolher o próximo, ou terminar o projeto aqui
NÃO → corrigir extra → validar novamente
```

Estado: `[PLANEADO]`

---

# COMO TRABALHAMOS DAQUI PARA A FRENTE

Regra de implementação, válida para todos os módulos 0–14:

```
FASE → MÓDULO → TAREFA → IMPLEMENTAÇÃO → EXPLICAÇÃO → TESTE → VALIDAÇÃO → PRÓXIMA TAREFA
```

E só depois de terminar todas as tarefas de um módulo e o seu gate de conclusão dar `SIM`: **PRÓXIMO MÓDULO**.

Passo a passo do ciclo de trabalho:

1. Dizer **"vou começar o Módulo N"**. Claude resume o que vai ser criado e porquê (secções 4.1–4.4 do módulo).
2. O utilizador escreve o código, tarefa a tarefa, seguindo a ordem exata da secção NÍVEL 3 do módulo.
3. Ao terminar cada tarefa, atualizar o seu estado (`[PLANEADO]` → `[EM IMPLEMENTAÇÃO]` → `[IMPLEMENTADO]`).
4. Quando terminar o módulo (ou quiser rever uma tarefa), dizer "revê o Módulo N" (ou "revê a tarefa X do Módulo N") — Claude faz **code review**: convenções, bugs, segurança, o que ficou fora do contrato.
5. Só se avança para o módulo seguinte quando o **gate de conclusão** do módulo atual der `SIM` em todos os pontos — atualizar o estado do módulo para `[VALIDADO]`/`[CONCLUÍDO]`.
6. Se ficar preso, colar o erro completo — Claude explica **o que significa** antes de corrigir.
7. No Módulo 13, Claude liga o front e corre os testes E2E em conjunto com o utilizador.
8. Não se salta módulos, não se implementam funcionalidades de um módulo posterior dentro de um módulo anterior só porque existe uma dependência — se for necessária uma dependência externa ainda não implementada, isso fica marcado como **DEPENDÊNCIA EXTERNA** dentro da tarefa (ver exemplos nos Módulos 8, 10 e 11), sem implementar o módulo inteiro de que depende.
9. Nada é inventado além do que está neste documento; onde a informação não estava determinada no plano original, ficou marcada como `PONTO A DECIDIR` (ver Módulos 7, 8, 11, 13, 14) — essas decisões ficam para quando se chegar a essa tarefa.

