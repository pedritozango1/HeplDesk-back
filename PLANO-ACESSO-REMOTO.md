# Módulo 15 — Acesso remoto ao PC do solicitante (back-end)

> Estado a 2026-10-07: back-end `[IMPLEMENTADO]` (fases 15.1–15.9) · front-end `[IMPLEMENTADO]` · modo CONTROLAR com agente próprio `[IMPLEMENTADO]` (ver secção 12; o RustDesk foi retirado) · testes E2E em `novati/e2e/acesso-remoto.spec.js` · Depende de: Módulos 10 (Solicitações/Chat), 11 (Atendimentos) e do SignalR já existente.
>
> Custo: zero. Nenhum pacote novo; STUN público gratuito; o TURN é opcional e está desligado (`Turn:Urls` vazio).
>
> Diferenças face ao plano original: o token é uma classe estática (`Services/TokenAcesso.cs`) em vez de um serviço com interface; o alfabeto tem 31 símbolos; o hash inclui o Id da sessão; `Sinal(tipo, dados)` não recebe o Id da sessão (fica guardado na ligação pelo `Entrar`); o evento `terminada` é emitido pelo `UnitOfWork`.

## 1. O que se pretende

1. O técnico que **assumiu** a solicitação pede acesso remoto ao PC do funcionário.
2. O funcionário vê o pedido e **autoriza** (ou recusa). Ao autorizar, o servidor gera um **token de uso único**.
3. O token chega ao técnico (entrega dirigida pelo sistema; o funcionário também o vê, para o poder ditar).
4. O técnico **resgata** o token. Se for válido, a sessão fica ativa e os dois PCs ligam-se.
5. Qualquer das partes pode terminar a sessão a qualquer momento. Fica tudo registado.

## 2. A limitação técnica que decide a arquitetura

Um browser **consegue partilhar o ecrã** (`getDisplayMedia`) mas **não consegue receber rato/teclado de outra máquina** — é uma barreira de segurança do próprio browser, sem contorno. Por isso:

| | Etapa 1 — Ver o ecrã | Etapa 2 — Controlar o PC |
|---|---|---|
| O que o técnico faz | Vê o ecrã em direto e orienta o funcionário pelo chat | Mexe no rato e teclado |
| Instalação no PC do funcionário | Nenhuma (só o browser) | Um programa nativo (agente) |
| Tecnologia de transporte | WebRTC browser ↔ browser | Agente nativo (ver secção 12) |
| Esforço | Baixo, todo dentro do Novati | Agente próprio — feito, ver secção 12 |

**O back-end deste plano é o mesmo para as duas etapas.** Ele trata de consentimento, token, autorização, ciclo de vida, sinalização e auditoria. O que muda na etapa 2 é só o que o resgate devolve (as "credenciais de transporte"). Por isso começamos aqui sem medo de deitar trabalho fora.

## 3. Arquitetura

```
 Browser do TÉCNICO                    Novati.API                    Browser do FUNCIONÁRIO
 ──────────────────                    ──────────                    ──────────────────────
 POST pedir acesso  ───────────────►  SessaoRemota = PEDIDA
                                       notificação + SignalR  ─────►  "O técnico pede acesso"
                                       SessaoRemota = AUTORIZADA ◄──  POST autorizar
        token (só para este user) ◄──  gera token, guarda só o hash   resposta: token (1 vez)
 POST resgatar {token} ────────────►  valida · consome · ATIVA
        ◄── servidores ICE (STUN/TURN com credenciais temporárias)

 ── /hubs/acesso-remoto (sinalização: oferta, resposta, candidatos ICE) ──────────────────►
 ◄═══════════════ vídeo do ecrã, direto entre os dois PCs (WebRTC, cifrado) ═══════════════
                         (se a rede bloquear o caminho direto, passa pelo TURN)
```

Três canais, cada um com a sua função:

- **REST** (`/api/...`) — todas as decisões de negócio e mudanças de estado. É onde vivem as regras.
- **SignalR** — avisos em tempo real e sinalização WebRTC. Não decide nada; só transporta.
- **WebRTC** — o vídeo. Não passa pela API (a API não aguentaria nem deve ver o ecrã das pessoas).

## 4. Tecnologia

| Necessidade | Escolha | Porquê |
|---|---|---|
| Regras e estado | Controller → Service → Repository, EF Core, PostgreSQL | O padrão de todos os módulos |
| Gerar o token | `System.Security.Cryptography.RandomNumberGenerator` | Aleatoriedade criptográfica; `Random` é previsível |
| Guardar o token | Só o hash SHA-256 (`SHA256.HashData`) | Uma fuga da BD não revela tokens. Não é BCrypt: o token vive 5 minutos, não precisa de hash lento |
| Comparar o token | `CryptographicOperations.FixedTimeEquals` | Evita ataques por medição de tempo |
| Uso único sob concorrência | Concorrência otimista com `xmin` do PostgreSQL (`IsRowVersion()`) | Dois resgates em simultâneo: só um grava, o outro recebe 409 |
| Limitar tentativas | Contador por sessão + `Microsoft.AspNetCore.RateLimiting` | O segundo já vem no ASP.NET Core, sem pacote novo |
| Avisos e sinalização | SignalR (já existe) + hub novo `AcessoRemotoHub` | Reutiliza o JWT e o `OnMessageReceived` de `/hubs` |
| Expirar pedidos e sessões | `BackgroundService` com `PeriodicTimer` | Limpeza sem depender de alguém fazer um pedido |
| Atravessar NAT/firewall | STUN + TURN (`coturn`, alojado por nós) | Sem TURN, redes diferentes muitas vezes não ligam |
| Credenciais do TURN | Temporárias, HMAC-SHA1 com segredo partilhado (`use-auth-secret` do coturn) | O segredo nunca sai do servidor; cada sessão recebe credenciais que caducam |

**Pacotes NuGet novos: nenhum.** Tudo isto está no .NET 10 e no ASP.NET Core.

**Infraestrutura nova: nenhuma obrigatória.** Com o STUN público (gratuito) a ligação funciona na mesma rede local e na maioria das redes domésticas. Falha quando um dos lados está atrás de uma firewall empresarial restritiva ou de NAT simétrico (comum em dados móveis): aí só um servidor TURN resolve. O código já o suporta — basta preencher `Turn:Urls` e `Turn:Segredo`. O coturn é gratuito, mas precisa de uma máquina com portas UDP abertas (não corre em Render/Railway); a opção sem custo é uma VM do escalão gratuito permanente de um fornecedor de cloud.

## 5. Modelo de dados

### Entidade `SessaoRemota` (tabela `SessoesRemotas`)

| Campo | Tipo | Notas |
|---|---|---|
| `Id` | `Guid` | de `BaseEntity` |
| `SolicitacaoId` | `Guid` | FK → `Solicitacao`, Cascade |
| `TecnicoId` | `Guid` | FK → `User`, Restrict. Quem pediu; só ele pode resgatar |
| `SolicitanteId` | `Guid` | FK → `User`, Restrict. Quem autoriza. Copiado da solicitação |
| `Estado` | `EstadoSessaoRemota` | guardado como texto |
| `Modo` | `ModoAcessoRemoto` | `VER` (etapa 1). `CONTROLAR` fica reservado para a etapa 2 |
| `TokenHash` | `string?` (64) | SHA-256 em hexadecimal. Nunca o token em claro |
| `TokenExpiraEm` | `DateTime?` | UTC |
| `TokenUsadoEm` | `DateTime?` | preenchido no resgate; é o que faz o "uso único" |
| `TentativasFalhadas` | `int` | resgates com código errado |
| `PedidaEm` | `DateTime` | UTC, relógio do servidor |
| `RespondidaEm` | `DateTime?` | autorização ou recusa |
| `IniciadaEm` | `DateTime?` | momento do resgate |
| `TerminadaEm` | `DateTime?` | |
| `TerminadaPorId` | `Guid?` | null quando foi o sistema (expiração) |
| `MotivoFim` | `string?` (200) | "Terminada pelo solicitante", "Tempo máximo atingido"… |
| `AutorizadaIp` | `string?` (45) | prova do consentimento |
| `Versao` | `uint` | mapeado para `xmin` com `IsRowVersion()` |

Índices:

- `SolicitacaoId` (listagens).
- **Único parcial** em `SolicitacaoId` com filtro `"Estado" IN ('PEDIDA','AUTORIZADA','ATIVA')` — a BD garante que só há uma sessão em curso por solicitação, mesmo com dois pedidos em simultâneo.

### Enums novos

```
EstadoSessaoRemota: PEDIDA, AUTORIZADA, ATIVA, TERMINADA, RECUSADA, EXPIRADA, CANCELADA
ModoAcessoRemoto:   VER, CONTROLAR
TipoHistoricoOrdem: + ACESSO_REMOTO   (valor novo num enum que já existe)
```

## 6. Máquina de estados

```
PEDIDA ──autorizar──► AUTORIZADA ──resgatar──► ATIVA ──terminar──► TERMINADA
  │                      │                       │
  ├─recusar─► RECUSADA   ├─TTL do token─► EXPIRADA└─duração máxima─► TERMINADA
  ├─cancelar─► CANCELADA ├─5 erros─────► EXPIRADA
  └─TTL─► EXPIRADA       └─cancelar────► CANCELADA
```

Finais: `TERMINADA`, `RECUSADA`, `EXPIRADA`, `CANCELADA`. De um estado final não se sai — para tentar outra vez, cria-se um pedido novo.

Regras que atravessam os estados:

- Só se pede acesso com a solicitação em `EM_ATENDIMENTO` e sendo o técnico da ordem.
- No resgate volta-se a confirmar que o técnico **ainda é** o da ordem (pode ter havido reatribuição).
- Reatribuir a ordem ou a solicitação sair de `EM_ATENDIMENTO` encerra a sessão em curso.

## 7. O token

- **Formato:** 8 caracteres de um alfabeto de 31 símbolos sem caracteres confundíveis (sem `0/O`, `1/I/L`), mostrado como `XXXX-XXXX`. São cerca de 40 bits.
- **Porque chega tão curto:** dura 5 minutos, admite 5 tentativas, e só o técnico daquela sessão (JWT) o pode usar. Tem de ser curto para o funcionário o conseguir ditar ao telefone.
- **Geração:** `RandomNumberGenerator.GetString(alfabeto, 8)`.
- **Normalização antes de comparar:** maiúsculas, sem hífen nem espaços.
- **Armazenamento:** só `TokenHash`. O valor em claro existe em dois sítios, uma vez: na resposta do `autorizar` e no evento SignalR dirigido ao técnico.
- **Onde nunca aparece:** logs, texto de notificações, chat, evento `alterado`, `SessaoRemotaDto`.
- **Uso único:** o resgate grava `TokenUsadoEm` e muda o estado para `ATIVA` na mesma gravação. O `xmin` impede dois resgates simultâneos de passarem ambos.
- **Tentativa falhada:** incrementa o contador e **grava antes de lançar a exceção** (senão o contador perde-se).

## 8. Erros

O formato continua `{ statusCode, message }`, com um campo novo **opcional** `codigo` para o front reagir sem comparar frases.

| Situação | HTTP | `codigo` | Mensagem |
|---|---|---|---|
| Solicitação/sessão não existe | 404 | `NAO_ENCONTRADA` | Sessão de acesso remoto não encontrada. |
| Não é o técnico da ordem | 403 | `SEM_PERMISSAO` | Só o técnico que assumiu a solicitação pode pedir acesso. |
| Não é o solicitante (autorizar/recusar) | 403 | `SEM_PERMISSAO` | Só o solicitante pode responder a este pedido. |
| Solicitação fora de `EM_ATENDIMENTO` | 400 | `ESTADO_INVALIDO` | Só se pode pedir acesso a uma solicitação em atendimento. |
| Já há sessão em curso | 409 | `SESSAO_EM_CURSO` | Já existe um pedido de acesso em curso para esta solicitação. |
| Ação fora do estado certo | 409 | `ESTADO_INVALIDO` | Este pedido já foi respondido. / Esta sessão já terminou. |
| Token com formato errado | 400 | `TOKEN_FORMATO` | O código tem 8 caracteres. |
| Token errado | 400 | `TOKEN_INVALIDO` | Código incorreto. Restam N tentativas. |
| Token já usado | 409 | `TOKEN_USADO` | Este código já foi utilizado. |
| Token fora de prazo | 410 | `TOKEN_EXPIRADO` | O código expirou. Peça um novo acesso. |
| Tentativas esgotadas | 429 | `TENTATIVAS_ESGOTADAS` | Demasiadas tentativas. Peça um novo acesso. |
| Resgates a mais por minuto | 429 | `LIMITE_PEDIDOS` | Demasiados pedidos. Aguarde um momento. |

Exceções novas: `ExpiradoException` (410) e `DemasiadasTentativasException` (429), com os respetivos `catch` no `ExceptionHandlingMiddleware`.

## 9. API REST

| Método e rota | Quem | Efeito |
|---|---|---|
| `GET /api/acessos-remotos` | todos | Lista filtrada por perfil (FUNCIONARIO: as suas; TECNICO: as que pediu; GESTOR/ADMIN: todas). Nunca 403 |
| `POST /api/solicitacoes/{id}/acesso-remoto` | técnico da ordem | Cria sessão `PEDIDA`. Corpo: `{ modo }`. 201 |
| `POST /api/acessos-remotos/{id}/autorizar` | solicitante | `AUTORIZADA`. Devolve `{ sessao, token, expiraEm }` — única vez que o token sai por REST |
| `POST /api/acessos-remotos/{id}/recusar` | solicitante | `RECUSADA`. Corpo opcional: `{ motivo }` |
| `POST /api/acessos-remotos/{id}/resgatar` | técnico da sessão | Corpo: `{ token }`. `ATIVA`. Devolve `{ sessao, iceServers, expiraEm }` |
| `POST /api/acessos-remotos/{id}/terminar` | qualquer participante, GESTOR, ADMIN | `TERMINADA` (se `ATIVA`) ou `CANCELADA` (se ainda não começou) |
| `GET /api/acessos-remotos/{id}/ice` | participantes de sessão `ATIVA` | Renova credenciais TURN (o funcionário também precisa delas) |

`SessaoRemotaDto`: `id, solicitacaoId, tecnicoId, solicitanteId, estado, modo, pedidaEm, tokenExpiraEm, iniciadaEm, terminadaEm, motivoFim`. Sem token e sem hash.

## 10. Tempo real

### No `TempoRealHub` que já existe (só envio, a partir do service)

- `alterado` com `"acessosRemotos"` — automático, basta acrescentar `SessaoRemota` ao `RecursosAlterados`.
- `acessoRemotoToken` `{ sessaoId, token, expiraEm }` — enviado **apenas** ao grupo `user:{tecnicoId}`.

### Hub novo `AcessoRemotoHub` em `/hubs/acesso-remoto`

Separado do `TempoRealHub` para não misturar com a presença, e porque só está ligado durante uma sessão.

| Método (cliente → servidor) | O que faz |
|---|---|
| `Entrar(sessaoId)` | Confirma na BD que o utilizador é participante e a sessão está `ATIVA`; junta a ligação ao grupo `sessao:{id}`; avisa o outro com `parEntrou` |
| `Sinal(tipo, dados)` | Reencaminha `oferta` / `resposta` / `candidato` para o outro participante. `dados` é texto (JSON serializado pelo front). Não interpreta o conteúdo |

| Evento (servidor → cliente) | Quando |
|---|---|
| `parEntrou`, `parSaiu` | O outro lado ligou-se / perdeu a ligação |
| `sinal` | Mensagem de sinalização do outro lado |
| `terminada` `{ motivo }` | A sessão acabou — os dois browsers fecham a ligação WebRTC |

Segurança do hub: `Sinal` só aceita ligações que passaram por `Entrar` (guardado em `Context.Items`), e limita o tamanho de `dados`.

## 11. Configuração

```json
"AcessoRemoto": {
  "PedidoTtlSegundos": 600,
  "TokenTtlSegundos": 300,
  "MaxTentativas": 5,
  "DuracaoMaxMinutos": 60,
  "Stun": [ "stun:stun.l.google.com:19302" ],
  "Turn": { "Urls": [], "Segredo": "", "TtlSegundos": 3600 }
}
```

`Turn:Segredo` vai para `dotnet user-secrets` em desenvolvimento e para variável de ambiente em produção — nunca para o `appsettings.json`.

Credenciais TURN temporárias: `username = "{expiraUnix}:{userId}"`, `credential = Base64(HMACSHA1(segredo, username))`.

## 12. Modo CONTROLAR — Agente Novati

O RustDesk foi retirado: funcionava sozinho (bastava ditar o ID e a palavra-passe por telefone), o que deixava contornar o pedido, o token e o registo. O **Agente Novati** é o nosso programa (`agente/`, .NET, Windows) e só obedece a este servidor.

### Como funciona

```
 Agente (PC do funcionário)         Novati.API                    Browser do técnico
 ──────────────────────────         ──────────                    ──────────────────
 liga-se a /hubs/agente (sem login)
 Registar(nomePc) ───────────────►  dá-lhe um código de 9 dígitos
 mostra o código no ecrã
                                    funcionário autoriza com o código  → agente RESERVADO para a sessão
                                    técnico resgata o token            → sessão ATIVA
        ◄──────────── "iniciar" ──  (só agora)
 Quadro(jpeg) ───────────────────►  reencaminha "quadro" ───────────►  mostra a imagem
        ◄──────────── "visto" ────  ◄─────────────── QuadroVisto()     (controlo de ritmo)
        ◄──────────── "entrada" ──  ◄─────────────── Entrada(json)     rato e teclado
        ◄──────────── "parar" ────  sessão fechada (qualquer via)
```

- O agente **não tem conta, ID nem palavra-passe**. O código que mostra muda a cada ligação e só serve dentro de um pedido de acesso do Novati.
- Escrever o código prova que quem autoriza (com sessão iniciada) está à frente daquele PC.
- Imagem e entrada passam pelo servidor — não há ligação direta entre os PCs, por isso este modo não precisa de STUN/TURN.
- O agente só captura e só aplica rato/teclado entre o `iniciar` e o `parar`. Se perder a ligação ao servidor, pára sozinho.

### Som

- **Modo CONTROLAR:** o agente captura o que o PC está a tocar (a saída de som, não o microfone) com a biblioteca NAudio (gratuita) e envia blocos de 100 ms, mono, 16 kHz, 16 bits, por `AgenteHub.Som`. O browser do técnico toca-os com a Web Audio API. Em silêncio não é enviado nada. Qualidade de telefone: chega para avisos, erros e vozes, não para música.
- **Modo VER:** o browser pede o áudio junto com o ecrã. Só há som se o funcionário escolher o **ecrã inteiro** ou um **separador** e marcar a opção de partilhar o áudio — uma janela isolada não tem som (limite do browser). O painel dos dois lados diz se o som foi incluído.
- O técnico tem um botão "Som ligado / Som desligado" nos dois modos.
- Não há conversa por voz (microfone) — para isso continua a usar-se o chat ou o telefone.

### Três formas de cortar

1. **Terminar acesso** no Novati (qualquer das partes, gestor, admin, tempo máximo, reatribuição) → o `UnitOfWork` liberta o agente e envia-lhe `parar`.
2. **Terminar acesso** na janela do agente → `AgenteHub.Terminar` → sessão `TERMINADA`.
3. **Fechar o agente** (ou a rede cair) → `AgenteHub.OnDisconnectedAsync` → sessão `TERMINADA`.

Durante a sessão a janela do agente fica vermelha, por cima de todas as outras, com o nome do técnico.

### Distribuição ("o agente está na plataforma")

- `agente/publicar.ps1` compila um único `.exe` (não precisa de instalação nem do .NET no PC) e coloca-o em `backend/Agente/NovatiAgente.exe`. Correr sempre que o código do agente mudar.
- `POST /api/agente/links` (TECNICO, GESTOR, ADMIN) gera um link com validade (`Agente:LinkTtlHoras`, 72 h). Com `funcionarioId`, o link segue por notificação.
- O link é uma página do front (`/agente?t=…`) com o botão de download e as instruções.
- `GET /api/agente/download?t=…` não exige login (quem autoriza é o token, assinado com HMAC) e acrescenta ao fim do `.exe` o endereço do servidor — o agente lê-o do próprio ficheiro, por isso o funcionário não configura nada.
- No front: botão "Enviar agente ao funcionário" na ordem, e página **Agente Remoto** no menu da equipa.

### Limites desta primeira versão

- **Só Windows**, e só o ecrã principal.
- **Imagem a ~9 quadros por segundo no máximo**, em JPEG. Chega para suporte; não é fluida como um produto dedicado.
- **Janelas de administrador (UAC) e ecrã de bloqueio** não são capturáveis nem controláveis: o Windows mostra-os num ambiente protegido. Resolve-se com o agente a correr como serviço do sistema — fica para depois.
- **O Windows avisa ao abrir o `.exe`** ("O Windows protegeu o seu PC"), porque não tem assinatura digital. Uma assinatura reconhecida é paga; a página de download explica como continuar.
- **Tráfego pelo servidor:** cada imagem tem 100–300 KB. Num alojamento gratuito com limite de largura de banda, uma sessão longa pesa.
- **Um só servidor:** os agentes ligados ficam em memória (`AgentesLigados`), como a presença. Com várias instâncias da API seria preciso um backplane.
- **As teclas seguem o teclado do PC remoto:** o técnico envia a tecla física; o carácter que sai depende do esquema de teclado configurado no PC do funcionário.
- **O hub dos agentes é anónimo** (limitado a 2000 ligações). Um agente sem sessão não consegue nada, mas o endereço fica exposto a quem o quiser sobrecarregar.

### Ficheiros

| Ficheiro | Papel |
|---|---|
| `agente/LigacaoServidor.cs` | Ligação ao hub, ciclo de captura, obedece a `iniciar` / `parar` |
| `agente/CapturaEcra.cs` | Ecrã principal → JPEG (reduz acima de 1600 px de largura) |
| `agente/EntradaRemota.cs` | `SendInput`: rato e teclado; solta tudo o que ficou premido ao parar |
| `agente/JanelaAgente.cs` | Janela com o código e o aviso vermelho com "Terminar acesso" |
| `agente/Configuracao.cs` | Lê o servidor da marca no fim do próprio `.exe` |
| `backend/Realtime/AgentesLigados.cs` | Agentes ligados, reserva e libertação por sessão |
| `backend/Realtime/AgenteHub.cs` | `/hubs/agente`: registo, imagens, terminar, corte ao desligar |
| `backend/Realtime/AcessoRemotoHub.cs` | `QuadroVisto` e `Entrada` (só o técnico da sessão) |
| `backend/Services/AgenteService.cs`, `Controllers/AgenteController.cs` | Links assinados e download |
| `novati/src/hooks/useControloRemoto.js`, `modules/acesso-remoto/VisorControlo.jsx` | Visor com rato e teclado |
| `novati/src/modules/acesso-remoto/AgentePage.jsx` | Página de download e de envio de links |

### Testes

- `novati/e2e/acesso-remoto.spec.js` — o teste de controlo usa um agente simulado (um cliente SignalR), porque o verdadeiro mexeria no rato e no teclado da máquina dos testes.
- O agente verdadeiro foi testado à parte, de ponta a ponta: download pelo link, ligação ao servidor da marca, imagem real do ecrã, duas teclas inofensivas (F24) aplicadas, e os cortes por "terminar" e por fecho do agente.
- **Não testado automaticamente:** rato e teclas "a sério" no PC remoto, e o uso entre dois PCs diferentes pela rede.

## 12b. Front-end

| Ficheiro | Papel |
|---|---|
| `src/context/AppContext.jsx` | Recurso `acessosRemotos`, 6 ações, código de acesso guardado só em memória |
| `src/lib/acessoRemoto.js` | Ligação ao hub da sessão, URL de download do agente, utilitários |
| `src/hooks/useLigacaoRemota.js` | WebRTC: quem partilha envia a oferta e reenvia-a sempre que o outro lado entra |
| `src/modules/acesso-remoto/AcessoRemotoCentro.jsx` | Painéis flutuantes, montados fora das rotas (mudar de página não corta a partilha) |
| `src/modules/acesso-remoto/SessaoSolicitante.jsx` | Autorizar/recusar, ver o código, partilhar o ecrã, terminar |
| `src/modules/acesso-remoto/SessaoTecnico.jsx` | Esperar, introduzir o código, ver o vídeo (VER) ou controlar o PC (CONTROLAR) |
| `src/modules/acesso-remoto/PedirAcessoRemoto.jsx` | Botões de pedido dentro da ordem de reparo |

## 13. Fases de implementação

Cada fase compila e testa-se sozinha no Swagger.

### 15.1 — Modelo e migration
- `Models/Enums/EstadoSessaoRemota.cs`, `ModoAcessoRemoto.cs` (novos)
- `Models/Enums/TipoHistoricoOrdem.cs` → acrescentar `ACESSO_REMOTO`
- `Models/Entities/SessaoRemota.cs` (novo)
- `Data/Configurations/SessaoRemotaConfiguration.cs` (novo): FKs, tamanhos, índice único parcial, `IsRowVersion()`
- `Data/AppDbContext.cs`: `DbSet<SessaoRemota>` + conversão dos dois enums para texto
- `dotnet ef migrations add SessoesRemotas`
- **Verificar:** a tabela e o índice parcial existem no pgAdmin; a migration não cria coluna `Versao` (é o `xmin`).

### 15.2 — Configuração e token
- `Common/Settings/AcessoRemotoSettings.cs` (novo) + secção no `appsettings.json`
- `Services/Interfaces/ITokenAcessoService.cs`, `Services/TokenAcessoService.cs` (novos): `Gerar()` → `(claro, hash)`, `Normalizar()`, `Verificar(claro, hash)`
- **Verificar:** classe pura, sem BD — boa candidata ao primeiro teste xUnit do projeto.

### 15.3 — Repositório
- `Repositories/Interfaces/ISessaoRemotaRepository.cs`, `Repositories/SessaoRemotaRepository.cs` (novos): `GetEmCursoPorSolicitacaoAsync`, `GetVisiveisAsync(userId, role)`, `GetExpiradasAsync(agora)`

### 15.4 — Erros
- `Common/Exceptions/ExpiradoException.cs`, `DemasiadasTentativasException.cs` (novos)
- `Common/Middleware/ExceptionHandlingMiddleware.cs`: dois `catch` novos + campo `codigo` opcional
- **Verificar:** os erros antigos continuam com o mesmo JSON.

### 15.5 — Pedir, autorizar, recusar, terminar
- `Dtos/AcessoRemoto/*` (novos): `SessaoRemotaDto`, `PedirAcessoRequest`, `AutorizacaoResponse`, `RecusarRequest`
- `Mappers/SessaoRemotaMapper.cs` (novo)
- `Services/Interfaces/IAcessoRemotoService.cs`, `Services/AcessoRemotoService.cs` (novos)
- `Controllers/AcessosRemotosController.cs` (novo) + rota `POST` em `SolicitacoesController`
- `Common/Extensions/ServiceCollectionExtensions.cs`: bloco "Módulo 15"
- `Realtime/RecursosAlterados.cs`: `[typeof(SessaoRemota)] = "acessosRemotos"`
- Notificação ao solicitante no pedido; ao técnico na recusa
- Violação do índice único (`DbUpdateException`, código Postgres `23505`) → `ConflictException`
- **Verificar:** técnico pede → funcionário autoriza → resposta traz token; segundo pedido dá 409; outro técnico dá 403.

### 15.6 — Resgatar
- `AcessoRemotoService.ResgatarAsync`: ordem das verificações = permissão → estado → tentativas → prazo → hash
- Contador de tentativas gravado antes da exceção
- `DbUpdateConcurrencyException` → `ConflictException` (`TOKEN_USADO`)
- `Services/CredenciaisTurnService.cs` (novo) + `GET .../ice`
- `Program.cs`: `AddRateLimiter` (janela fixa por utilizador) + `UseRateLimiter` + `[EnableRateLimiting]` no resgate
- Entrega dirigida do token: `IHubContext<TempoRealHub>` → `Clients.Group(GrupoUser(tecnicoId))`
- Entrada no histórico da ordem: "Acesso remoto iniciado"
- **Verificar:** token certo → `ATIVA`; o mesmo token outra vez → 409; errado 5 vezes → 429 e sessão `EXPIRADA`; esperar o TTL → 410.

### 15.7 — Hub de sinalização
- `Realtime/AcessoRemotoHub.cs` (novo)
- `Program.cs`: `app.MapHub<AcessoRemotoHub>("/hubs/acesso-remoto")`
- **Verificar:** dois browsers em sessão `ATIVA` trocam `sinal`; um terceiro utilizador é rejeitado em `Entrar`.

### 15.8 — Expiração e ligação aos atendimentos
- `BackgroundServices/ExpiracaoSessoesRemotasService.cs` (novo): `PeriodicTimer` de 30 s, `IServiceScopeFactory` (o `DbContext` é scoped, o serviço é singleton)
- `Program.cs`: `AddHostedService<...>()`
- `AtendimentoService`: reatribuir e aceitar solução encerram a sessão em curso
- O estado é também validado na leitura — a correção não depende do temporizador
- **Verificar:** pedido sem resposta passa a `EXPIRADA`; sessão `ATIVA` além do máximo termina e os browsers recebem `terminada`.

### 15.9 — Fecho
- `DominioService`: expor os enums novos em `GET /api/dominio`
- Comentários XML e `ProducesResponseType` em todas as rotas
- Fluxo completo em ambiente isolado (API `:3334`, BD `Novati_E2E`)

## 14. Segurança e privacidade — lista de controlo

- [ ] O consentimento é sempre uma ação explícita do solicitante; não existe acesso sem ele.
- [ ] O funcionário pode terminar a qualquer momento, e a sessão tem duração máxima.
- [ ] O token nunca é escrito em logs, notificações, chat ou eventos difundidos a todos.
- [ ] Só o técnico da ordem pede e resgata; revalidado no resgate.
- [ ] Uso único garantido pela BD, não por um `if`.
- [ ] O segredo do TURN não está no repositório.
- [ ] Nada é gravado: nem vídeo, nem capturas. Fica só quem, quando, quanto tempo e porquê.
- [ ] Em produção, API e hubs só por HTTPS/WSS.
