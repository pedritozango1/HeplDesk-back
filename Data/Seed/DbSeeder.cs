using Microsoft.EntityFrameworkCore;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Data.Seed;

/// <summary>
/// Popula a base de dados com os dados de demonstração do front-end
/// (<c>novati/src/data/seed.js</c> + <c>novati/src/data/seedRelatorios.js</c>).
/// Só corre se ainda não existirem utilizadores (idempotente).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Guarda de idempotência
        if (await db.Users.AnyAsync()) return;

        // Dicionário que mapeia ids lógicos do front ("u1", "s1") para Guid reais
        var ids = new Dictionary<string, Guid>();

        // ─── BLOCO 1: Utilizadores ───────────────────────────
        const string password = "novati123";
        string hash = BCrypt.Net.BCrypt.HashPassword(password);

        db.Users.AddRange(
            new User { Id = ids["u1"] = Guid.NewGuid(), Nome = "Rita Almeida",      Email = "rita.admin@empresa.com",        Role = Role.ADMIN,       PasswordHash = hash },
            new User { Id = ids["u2"] = Guid.NewGuid(), Nome = "Carlos Gestor",     Email = "carlos.gestor@empresa.com",     Role = Role.GESTOR,      PasswordHash = hash },
            new User { Id = ids["u3"] = Guid.NewGuid(), Nome = "João Técnico",      Email = "joao.tecnico@empresa.com",      Role = Role.TECNICO,     PasswordHash = hash },
            new User { Id = ids["u4"] = Guid.NewGuid(), Nome = "Marta Técnica",     Email = "marta.tecnica@empresa.com",     Role = Role.TECNICO,     PasswordHash = hash },
            new User { Id = ids["u5"] = Guid.NewGuid(), Nome = "Pedro Funcionário", Email = "pedro.funcionario@empresa.com", Role = Role.FUNCIONARIO, PasswordHash = hash },
            new User { Id = ids["u6"] = Guid.NewGuid(), Nome = "Ana Funcionária",   Email = "ana.funcionaria@empresa.com",   Role = Role.FUNCIONARIO, PasswordHash = hash });

        // ─── BLOCO 2: Catálogo ────────────────────────────────
        db.ModelosDispositivo.AddRange(
            new ModeloDispositivo { Id = ids["md1"] = Guid.NewGuid(), Nome = "Dell Latitude 5420",     Fabricante = "Dell",   Tipo = "Notebook" },
            new ModeloDispositivo { Id = ids["md2"] = Guid.NewGuid(), Nome = "HP LaserJet Pro M404",   Fabricante = "HP",     Tipo = "Impressora" },
            new ModeloDispositivo { Id = ids["md3"] = Guid.NewGuid(), Nome = "Lenovo ThinkCentre M720", Fabricante = "Lenovo", Tipo = "Desktop" });

        db.ModelosComponente.AddRange(
            new ModeloComponente { Id = ids["mc1"] = Guid.NewGuid(), Nome = "SSD Samsung 512GB", Tipo = "SSD",     Capacidade = "512GB", StockMinimo = 1 },
            new ModeloComponente { Id = ids["mc2"] = Guid.NewGuid(), Nome = "RAM 8GB DDR4",      Tipo = "RAM",     Capacidade = "8GB",   StockMinimo = 2 },
            new ModeloComponente { Id = ids["mc3"] = Guid.NewGuid(), Nome = "Fonte 65W",         Tipo = "Fonte",   Capacidade = "65W",   StockMinimo = 1 },
            new ModeloComponente { Id = ids["mc4"] = Guid.NewGuid(), Nome = "Bateria 4 células",  Tipo = "Bateria", Capacidade = "42Wh",  StockMinimo = 1 });

        db.Compatibilidades.AddRange(
            new Compatibilidade { Id = Guid.NewGuid(), ModeloDispositivoId = ids["md1"], ModeloComponenteId = ids["mc1"] },
            new Compatibilidade { Id = Guid.NewGuid(), ModeloDispositivoId = ids["md1"], ModeloComponenteId = ids["mc2"] },
            new Compatibilidade { Id = Guid.NewGuid(), ModeloDispositivoId = ids["md1"], ModeloComponenteId = ids["mc3"] },
            new Compatibilidade { Id = Guid.NewGuid(), ModeloDispositivoId = ids["md1"], ModeloComponenteId = ids["mc4"] },
            new Compatibilidade { Id = Guid.NewGuid(), ModeloDispositivoId = ids["md3"], ModeloComponenteId = ids["mc1"] },
            new Compatibilidade { Id = Guid.NewGuid(), ModeloDispositivoId = ids["md3"], ModeloComponenteId = ids["mc2"] });

        // ─── BLOCO 3: Localizações, dispositivos e instâncias ─
        //Os pais são inseridos antes dos filhos (Sala 2 depende de Financeiro).
        db.Localizacoes.AddRange(
            new Localizacao { Id = ids["l1"] = Guid.NewGuid(), Nome = "Financeiro", PaiId = null },
            new Localizacao { Id = ids["l2"] = Guid.NewGuid(), Nome = "Sala 2",     PaiId = ids["l1"] },
            new Localizacao { Id = ids["l3"] = Guid.NewGuid(), Nome = "Receção",    PaiId = null },
            new Localizacao { Id = ids["l4"] = Guid.NewGuid(), Nome = "TI",         PaiId = null });

        db.DispositivosFisicos.AddRange(
            new DispositivoFisico
            {
                Id = ids["d1"] = Guid.NewGuid(), Patrimonio = "NB-001", NumeroSerie = "ABC123",
                LocalizacaoId = ids["l2"], ResponsavelId = ids["u5"], Estado = EstadoDispositivo.ATIVO,
                DataAquisicao = new DateOnly(2023, 5, 10), GarantiaMeses = 24, ModeloDispositivoId = ids["md1"],
            },
            new DispositivoFisico
            {
                Id = ids["d2"] = Guid.NewGuid(), Patrimonio = "NB-002", NumeroSerie = "ABC456",
                LocalizacaoId = ids["l3"], ResponsavelId = ids["u6"], Estado = EstadoDispositivo.ATIVO,
                DataAquisicao = new DateOnly(2025, 2, 15), GarantiaMeses = 24, ModeloDispositivoId = ids["md1"],
            },
            new DispositivoFisico
            {
                Id = ids["d3"] = Guid.NewGuid(), Patrimonio = "PR-001", NumeroSerie = "HPX789",
                LocalizacaoId = ids["l4"], ResponsavelId = null, Estado = EstadoDispositivo.ATIVO,
                DataAquisicao = new DateOnly(2022, 1, 20), GarantiaMeses = 12, ModeloDispositivoId = ids["md2"],
            });

        db.InstanciasComponentes.AddRange(
            new InstanciaComponente { Id = ids["ic1"] = Guid.NewGuid(), Codigo = "SSD-001", Estado = EstadoInstancia.INSTALADO, DispositivoFisicoId = ids["d1"], ModeloComponenteId = ids["mc1"] },
            new InstanciaComponente { Id = ids["ic2"] = Guid.NewGuid(), Codigo = "RAM-001", Estado = EstadoInstancia.INSTALADO, DispositivoFisicoId = ids["d1"], ModeloComponenteId = ids["mc2"] },
            new InstanciaComponente { Id = ids["ic3"] = Guid.NewGuid(), Codigo = "FNT-001", Estado = EstadoInstancia.INSTALADO, DispositivoFisicoId = ids["d1"], ModeloComponenteId = ids["mc3"] },
            new InstanciaComponente { Id = ids["ic4"] = Guid.NewGuid(), Codigo = "SSD-002", Estado = EstadoInstancia.INSTALADO, DispositivoFisicoId = ids["d2"], ModeloComponenteId = ids["mc1"] });

        // ─── BLOCO 4: Stock ───────────────────────────────────
        db.ItensStock.AddRange(
            new ItemStock { Id = ids["is1"] = Guid.NewGuid(), ModeloComponenteId = ids["mc1"] },
            new ItemStock { Id = ids["is2"] = Guid.NewGuid(), ModeloComponenteId = ids["mc2"] },
            new ItemStock { Id = ids["is3"] = Guid.NewGuid(), ModeloComponenteId = ids["mc3"] },
            new ItemStock { Id = ids["is4"] = Guid.NewGuid(), ModeloComponenteId = ids["mc4"] });

        // ─── BLOCO 5: Solicitações ───────────────────────────
        var solicitacoes = new[]
        {
            new Solicitacao
            {
                Id = ids["s1"] = Guid.NewGuid(), Titulo = "Computador não liga",
                Descricao = "Carreguei no botão e não acontece nada, nem a luz acende.",
                Estado = EstadoSolicitacao.ABERTA, Prioridade = Prioridade.ALTA, Categoria = "Hardware",
                DataCriacao = new DateOnly(2026, 8, 30), SolicitanteId = ids["u5"], DispositivoFisicoId = ids["d1"],
            },
            new Solicitacao
            {
                Id = ids["s2"] = Guid.NewGuid(), Titulo = "Impressora a fazer barulho estranho",
                Descricao = "Faz um ruído mecânico quando imprime.",
                Estado = EstadoSolicitacao.ABERTA, Prioridade = Prioridade.BAIXA, Categoria = "Hardware",
                DataCriacao = new DateOnly(2026, 8, 31), SolicitanteId = ids["u6"], DispositivoFisicoId = ids["d3"],
            },
            new Solicitacao
            {
                Id = ids["s3"] = Guid.NewGuid(), Titulo = "Ecrã azul constante",
                Descricao = "O computador reinicia sozinho com erro azul.",
                Estado = EstadoSolicitacao.RESOLVIDA, Prioridade = Prioridade.ALTA, Categoria = "Hardware",
                DataCriacao = new DateOnly(2026, 8, 10), SolicitanteId = ids["u5"], DispositivoFisicoId = ids["d1"],
                Avaliacao = new Avaliacao { Estrelas = 5, Comentario = "Excelente, resolveu rapidamente!", Data = new DateOnly(2026, 8, 12) },
            },
            new Solicitacao
            {
                Id = ids["s4"] = Guid.NewGuid(), Titulo = "Não consigo imprimir",
                Descricao = "A impressora não responde, fica em erro.",
                Estado = EstadoSolicitacao.RESOLVIDA, Prioridade = Prioridade.MEDIA, Categoria = "Hardware",
                DataCriacao = new DateOnly(2026, 8, 12), SolicitanteId = ids["u6"], DispositivoFisicoId = ids["d3"],
                Avaliacao = new Avaliacao { Estrelas = 4, Comentario = "Demorou um pouco mas ficou bom.", Data = new DateOnly(2026, 8, 14) },
            },
            new Solicitacao
            {
                Id = ids["s5"] = Guid.NewGuid(), Titulo = "Wi-Fi não conecta",
                Descricao = "O portátil não deteta nenhuma rede wireless.",
                Estado = EstadoSolicitacao.RESOLVIDA, Prioridade = Prioridade.URGENTE, Categoria = "Rede",
                DataCriacao = new DateOnly(2026, 8, 14), SolicitanteId = ids["u6"], DispositivoFisicoId = ids["d2"],
                Avaliacao = new Avaliacao { Estrelas = 5, Comentario = "Super rápido, ficou a funcionar em minutos.", Data = new DateOnly(2026, 8, 14) },
            },
            new Solicitacao
            {
                Id = ids["s6"] = Guid.NewGuid(), Titulo = "Laptop muito lento",
                Descricao = "Demora mais de 5 minutos a arrancar.",
                Estado = EstadoSolicitacao.RESOLVIDA, Prioridade = Prioridade.MEDIA, Categoria = "Hardware",
                DataCriacao = new DateOnly(2026, 8, 15), SolicitanteId = ids["u5"], DispositivoFisicoId = ids["d1"],
                Avaliacao = new Avaliacao { Estrelas = 3, Comentario = "Melhorou mas ainda não ficou como antes.", Data = new DateOnly(2026, 8, 18) },
            },
            new Solicitacao
            {
                Id = ids["s7"] = Guid.NewGuid(), Titulo = "Email não sincroniza",
                Descricao = "O Outlook está sempre a dar erro de ligação.",
                Estado = EstadoSolicitacao.RESOLVIDA, Prioridade = Prioridade.ALTA, Categoria = "Software",
                DataCriacao = new DateOnly(2026, 8, 18), SolicitanteId = ids["u6"], DispositivoFisicoId = ids["d2"],
                Avaliacao = new Avaliacao { Estrelas = 4, Comentario = "Ficou resolvido, obrigada.", Data = new DateOnly(2026, 8, 19) },
            },
            new Solicitacao
            {
                Id = ids["s8"] = Guid.NewGuid(), Titulo = "Teclado com teclas presas",
                Descricao = "As teclas A e S estão a falhar.",
                Estado = EstadoSolicitacao.FECHADA, Prioridade = Prioridade.BAIXA, Categoria = "Hardware",
                DataCriacao = new DateOnly(2026, 8, 20), SolicitanteId = ids["u5"], DispositivoFisicoId = ids["d1"],
                Avaliacao = new Avaliacao { Estrelas = 2, Comentario = "Demorou demasiado tempo, esperava mais.", Data = new DateOnly(2026, 8, 25) },
            },
            new Solicitacao
            {
                Id = ids["s9"] = Guid.NewGuid(), Titulo = "Projector não liga",
                Descricao = "Não faz sinal e o HDMI não funciona.",
                Estado = EstadoSolicitacao.RESOLVIDA, Prioridade = Prioridade.MEDIA, Categoria = "Hardware",
                DataCriacao = new DateOnly(2026, 8, 22), SolicitanteId = ids["u5"], DispositivoFisicoId = ids["d3"],
                Avaliacao = new Avaliacao { Estrelas = 5, Comentario = "Muito profissional e rápido.", Data = new DateOnly(2026, 8, 23) },
            },
            new Solicitacao
            {
                Id = ids["s10"] = Guid.NewGuid(), Titulo = "Acesso bloqueado ao ERP",
                Descricao = "Não consigo entrar no sistema de gestão.",
                Estado = EstadoSolicitacao.RESOLVIDA, Prioridade = Prioridade.URGENTE, Categoria = "Contas e acessos",
                DataCriacao = new DateOnly(2026, 8, 25), SolicitanteId = ids["u6"], DispositivoFisicoId = ids["d2"],
            },
        };
        db.Solicitacoes.AddRange(solicitacoes);

        // ─── BLOCO 6: Ordens de reparação ─────────────────────
        // Uma ordem por solicitação resolvida/fechada (s3..s10); s1 e s2 ficam na fila (ABERTA).
        var ordens = new[]
        {
            new OrdemReparo
            {
                Id = ids["or1"] = Guid.NewGuid(), SolicitacaoId = ids["s3"], TecnicoId = ids["u3"],
                Estado = EstadoOrdem.RESOLVIDO, Diagnostico = "RAM com defeito, a causar crashes azuis.",
                Solucao = "Substituída a módulo RAM defeituoso.", TempoGastoMin = 35,
                DataInicio = new DateOnly(2026, 8, 10), DataFim = new DateOnly(2026, 8, 12),
            },
            new OrdemReparo
            {
                Id = ids["or2"] = Guid.NewGuid(), SolicitacaoId = ids["s4"], TecnicoId = ids["u4"],
                Estado = EstadoOrdem.RESOLVIDO, Diagnostico = "Roda de alimentação de papel desalinhada.",
                Solucao = "Realinhei a roda e limpei os guias.", TempoGastoMin = 50,
                DataInicio = new DateOnly(2026, 8, 12), DataFim = new DateOnly(2026, 8, 14),
            },
            new OrdemReparo
            {
                Id = ids["or3"] = Guid.NewGuid(), SolicitacaoId = ids["s5"], TecnicoId = ids["u3"],
                Estado = EstadoOrdem.RESOLVIDO, Diagnostico = "Driver de rede desatualizado.",
                Solucao = "Atualizei o driver e reiniciei o serviço Wi-Fi.", TempoGastoMin = 15,
                DataInicio = new DateOnly(2026, 8, 14), DataFim = new DateOnly(2026, 8, 14),
            },
            new OrdemReparo
            {
                Id = ids["or4"] = Guid.NewGuid(), SolicitacaoId = ids["s6"], TecnicoId = ids["u3"],
                Estado = EstadoOrdem.RESOLVIDO, Diagnostico = "Disco SSD quase cheio e muitos programas em arranque.",
                Solucao = "Limpei o disco e desativei programas desnecessários.", TempoGastoMin = 90,
                DataInicio = new DateOnly(2026, 8, 15), DataFim = new DateOnly(2026, 8, 18),
            },
            new OrdemReparo
            {
                Id = ids["or5"] = Guid.NewGuid(), SolicitacaoId = ids["s7"], TecnicoId = ids["u4"],
                Estado = EstadoOrdem.RESOLVIDO, Diagnostico = "Perfil corrompido no Outlook.",
                Solucao = "Recriei o perfil e reconfigurei as contas.", TempoGastoMin = 40,
                DataInicio = new DateOnly(2026, 8, 18), DataFim = new DateOnly(2026, 8, 19),
            },
            new OrdemReparo
            {
                Id = ids["or6"] = Guid.NewGuid(), SolicitacaoId = ids["s8"], TecnicoId = ids["u4"],
                Estado = EstadoOrdem.RESOLVIDO, Diagnostico = "Desgaste físico das teclas.",
                Solucao = "Substituí o teclado interno.", TempoGastoMin = 120,
                DataInicio = new DateOnly(2026, 8, 20), DataFim = new DateOnly(2026, 8, 25),
            },
            new OrdemReparo
            {
                Id = ids["or7"] = Guid.NewGuid(), SolicitacaoId = ids["s9"], TecnicoId = ids["u3"],
                Estado = EstadoOrdem.RESOLVIDO, Diagnostico = "Cabo HDMI com contactos oxidados.",
                Solucao = "Substituí o cabo e testei com outro projector.", TempoGastoMin = 20,
                DataInicio = new DateOnly(2026, 8, 22), DataFim = new DateOnly(2026, 8, 23),
            },
            new OrdemReparo
            {
                Id = ids["or8"] = Guid.NewGuid(), SolicitacaoId = ids["s10"], TecnicoId = ids["u3"],
                Estado = EstadoOrdem.RESOLVIDO, Diagnostico = "Conta bloqueada por excesso de tentativas.",
                Solucao = "Desbloqueei a conta e reiniciei a senha.", TempoGastoMin = 10,
                DataInicio = new DateOnly(2026, 8, 25), DataFim = new DateOnly(2026, 8, 25),
            },
        };

        // Horas do cronómetro: assumida às 9h do dia de início e aprovada no dia de fim.
        // O valor escrito acima passa a ser o tempo de trabalho; o total (assumir → aprovar)
        // sai da diferença entre as duas horas, como faz o AtendimentoService.
        foreach (var o in ordens)
        {
            var trabalho = o.TempoGastoMin ?? 0;
            o.IniciadaEm = o.DataInicio.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
            o.ConcluidaEm = o.DataFim!.Value.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc).AddMinutes(trabalho);
            o.TempoTrabalhoMin = trabalho;
            o.TempoGastoMin = (int)(o.ConcluidaEm.Value - o.IniciadaEm).TotalMinutes;
        }
        db.OrdensReparo.AddRange(ordens);

        // Rejeições de solução (mostram a timeline de tentativas falhadas)
        db.OrdemRejeicoes.AddRange(
            new OrdemRejeicao { Id = ids["orj1"] = Guid.NewGuid(), OrdemId = ids["or4"], Motivo = "O computador continua lento após a limpeza.", Data = new DateOnly(2026, 8, 16) },
            new OrdemRejeicao { Id = ids["orj2"] = Guid.NewGuid(), OrdemId = ids["or6"], Motivo = "Troca de teclado não resolveu, problema era na placa-mãe.", Data = new DateOnly(2026, 8, 22) },
            new OrdemRejeicao { Id = ids["orj3"] = Guid.NewGuid(), OrdemId = ids["or6"], Motivo = "Ainda com intermitência nas teclas.", Data = new DateOnly(2026, 8, 23) });

        // ─── BLOCO 7: Unidades e movimentos de stock ──────────
        // A us4 fica reservada para a or1, por isso esta secção vem depois das ordens.
        db.UnidadesStock.AddRange(
            new UnidadeStock { Id = ids["us1"] = Guid.NewGuid(), Codigo = "SSD-099",  Estado = EstadoUnidade.DISPONIVEL, ItemStockId = ids["is1"] },
            new UnidadeStock { Id = ids["us2"] = Guid.NewGuid(), Codigo = "RAM-050",  Estado = EstadoUnidade.DISPONIVEL, ItemStockId = ids["is2"] },
            new UnidadeStock { Id = ids["us3"] = Guid.NewGuid(), Codigo = "RAM-051",  Estado = EstadoUnidade.DISPONIVEL, ItemStockId = ids["is2"] },
            new UnidadeStock { Id = ids["us4"] = Guid.NewGuid(), Codigo = "SSD-100",  Estado = EstadoUnidade.RESERVADA,  ItemStockId = ids["is1"], ReservadaParaOrdemId = ids["or1"] },
            new UnidadeStock { Id = ids["us5"] = Guid.NewGuid(), Codigo = "SSD-101",  Estado = EstadoUnidade.DISPONIVEL, ItemStockId = ids["is1"] },
            new UnidadeStock { Id = ids["us6"] = Guid.NewGuid(), Codigo = "RAM-052",  Estado = EstadoUnidade.AVARIADA,   ItemStockId = ids["is2"] },
            // Sem fontes em stock de propósito, para o MVP mostrar o fluxo de Compras.
            new UnidadeStock { Id = ids["us7"] = Guid.NewGuid(), Codigo = "BAT-001",  Estado = EstadoUnidade.DISPONIVEL, ItemStockId = ids["is4"] });

        // Peça instalada na or1 (troca da RAM) — depois das unidades, por causa da FK
        db.OrdemPecasUsadas.Add(new OrdemPecaUsada
        {
            Id = ids["opu1"] = Guid.NewGuid(),
            OrdemId = ids["or1"],
            UnidadeStockId = ids["us2"],
            ModeloComponenteId = ids["mc2"],
            InstaladoInstanciaId = ids["ic2"],
        });

        db.MovimentosStock.AddRange(
            new MovimentoStock { Id = ids["ms1"] = Guid.NewGuid(), ItemStockId = ids["is1"], Tipo = TipoMovimento.ENTRADA,       Quantidade = 1, Data = new DateOnly(2026, 8, 20), Observacao = "Compra inicial" },
            new MovimentoStock { Id = ids["ms2"] = Guid.NewGuid(), ItemStockId = ids["is2"], Tipo = TipoMovimento.ENTRADA,       Quantidade = 2, Data = new DateOnly(2026, 8, 20), Observacao = "Compra inicial" },
            new MovimentoStock { Id = ids["ms3"] = Guid.NewGuid(), ItemStockId = ids["is1"], Tipo = TipoMovimento.INSTALACAO,    Quantidade = 1, Data = new DateOnly(2026, 8, 28), Observacao = "Instalado no dispositivo NB-001" },
            new MovimentoStock { Id = ids["ms4"] = Guid.NewGuid(), ItemStockId = ids["is2"], Tipo = TipoMovimento.RESERVA,       Quantidade = 1, Data = new DateOnly(2026, 8, 30), Observacao = "Reservado para ordem or1" },
            new MovimentoStock { Id = ids["ms5"] = Guid.NewGuid(), ItemStockId = ids["is4"], Tipo = TipoMovimento.ENTRADA,       Quantidade = 1, Data = new DateOnly(2026, 9, 1),  Observacao = "Compra para reposição" },
            new MovimentoStock { Id = ids["ms6"] = Guid.NewGuid(), ItemStockId = ids["is2"], Tipo = TipoMovimento.SAIDA,         Quantidade = 1, Data = new DateOnly(2026, 9, 3),  Observacao = "Consumido em manutenção" },
            new MovimentoStock { Id = ids["ms7"] = Guid.NewGuid(), ItemStockId = ids["is1"], Tipo = TipoMovimento.TRANSFERENCIA, Quantidade = 1, Data = new DateOnly(2026, 9, 5),  Observacao = "Transferência entre armazéns" });

        // ─── BLOCO 8: Histórico das ordens ────────────────────
        var historicos = new List<OrdemHistorico>();

        void AddHistoricoResolvida(string ordem, string tecnico, DateOnly inicio, DateOnly fim, string diag, string sol)
        {
            historicos.Add(new OrdemHistorico { Id = Guid.NewGuid(), OrdemId = ids[ordem], Tipo = TipoHistoricoOrdem.ASSUMIDA,    Texto = "Solicitação assumida.",           AutorId = ids[tecnico], Data = inicio });
            historicos.Add(new OrdemHistorico { Id = Guid.NewGuid(), OrdemId = ids[ordem], Tipo = TipoHistoricoOrdem.DIAGNOSTICO, Texto = diag,                               AutorId = ids[tecnico], Data = inicio < fim ? inicio.AddDays(1) : fim });
            historicos.Add(new OrdemHistorico { Id = Guid.NewGuid(), OrdemId = ids[ordem], Tipo = TipoHistoricoOrdem.SOLUCAO,     Texto = sol,                                AutorId = ids[tecnico], Data = fim });
            historicos.Add(new OrdemHistorico { Id = Guid.NewGuid(), OrdemId = ids[ordem], Tipo = TipoHistoricoOrdem.ACEITE,      Texto = "Solução aceite pelo solicitante.", AutorId = null,           Data = fim });
        }

        AddHistoricoResolvida("or1", "u3", new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 12), "RAM com defeito, a causar crashes azuis.", "Substituída a módulo RAM defeituoso.");
        AddHistoricoResolvida("or2", "u4", new DateOnly(2026, 8, 12), new DateOnly(2026, 8, 14), "Roda de alimentação de papel desalinhada.", "Realinhei a roda e limpei os guias.");
        AddHistoricoResolvida("or3", "u3", new DateOnly(2026, 8, 14), new DateOnly(2026, 8, 14), "Driver de rede desatualizado.", "Atualizei o driver e reiniciei o serviço Wi-Fi.");
        AddHistoricoResolvida("or4", "u3", new DateOnly(2026, 8, 15), new DateOnly(2026, 8, 18), "Disco SSD quase cheio e muitos programas em arranque.", "Limpei o disco e desativei programas desnecessários.");
        AddHistoricoResolvida("or5", "u4", new DateOnly(2026, 8, 18), new DateOnly(2026, 8, 19), "Perfil corrompido no Outlook.", "Recriei o perfil e reconfigurei as contas.");
        AddHistoricoResolvida("or6", "u4", new DateOnly(2026, 8, 20), new DateOnly(2026, 8, 25), "Desgaste físico das teclas.", "Substituí o teclado interno.");
        AddHistoricoResolvida("or7", "u3", new DateOnly(2026, 8, 22), new DateOnly(2026, 8, 23), "Cabo HDMI com contactos oxidados.", "Substituí o cabo e testei com outro projector.");
        AddHistoricoResolvida("or8", "u3", new DateOnly(2026, 8, 25), new DateOnly(2026, 8, 25), "Conta bloqueada por excesso de tentativas.", "Desbloqueei a conta e reiniciei a senha.");

        // As rejeições também ficam registadas na timeline, antes do aceite final.
        historicos.Add(new OrdemHistorico { Id = Guid.NewGuid(), OrdemId = ids["or4"], Tipo = TipoHistoricoOrdem.REJEITADA, Texto = "O computador continua lento após a limpeza.", AutorId = null, Data = new DateOnly(2026, 8, 16) });
        historicos.Add(new OrdemHistorico { Id = Guid.NewGuid(), OrdemId = ids["or6"], Tipo = TipoHistoricoOrdem.REJEITADA, Texto = "Troca de teclado não resolveu, problema era na placa-mãe.", AutorId = null, Data = new DateOnly(2026, 8, 22) });
        historicos.Add(new OrdemHistorico { Id = Guid.NewGuid(), OrdemId = ids["or6"], Tipo = TipoHistoricoOrdem.REJEITADA, Texto = "Ainda com intermitência nas teclas.", AutorId = null, Data = new DateOnly(2026, 8, 23) });

        // Posição na timeline de cada ordem: por data, mantendo a ordem de inserção no mesmo dia.
        foreach (var grupo in historicos.GroupBy(h => h.OrdemId))
        {
            var posicao = 0;
            foreach (var h in grupo.OrderBy(h => h.Data)) h.Posicao = posicao++;
        }
        db.OrdemHistoricos.AddRange(historicos);

        // ─── BLOCO 9: Base de conhecimento ────────────────────
        db.Artigos.AddRange(
            new Artigo
            {
                Id = ids["a1"] = Guid.NewGuid(), Titulo = "Não consigo ligar ao Wi-Fi",
                Conteudo = "Passo 1: Clique no ícone de rede na barra de tarefas. Passo 2: Selecione a rede \"Empresa-WiFi\". Passo 3: Insira a palavra-passe fornecida pelo TI. Se persistir, reinicie o computador.",
                Tags = ["wifi", "rede", "internet", "ligação"], Categoria = "Rede", AutorId = ids["u3"],
            },
            new Artigo
            {
                Id = ids["a2"] = Guid.NewGuid(), Titulo = "Computador não liga",
                Conteudo = "Passo 1: Verifique se o cabo de alimentação está bem ligado à tomada. Passo 2: Experimente outra tomada. Passo 3: Segure o botão de ligar por 10 segundos e tente novamente. Se não resolver, abra uma solicitação.",
                Tags = ["ligar", "energia", "boot", "não liga"], Categoria = "Hardware", AutorId = ids["u3"],
            },
            new Artigo
            {
                Id = ids["a3"] = Guid.NewGuid(), Titulo = "Impressora não imprime",
                Conteudo = "Passo 1: Verifique se há papel e tinta/toner. Passo 2: Confirme que a impressora está ligada à rede. Passo 3: Reinicie a fila de impressão no seu computador.",
                Tags = ["impressora", "imprimir", "papel", "toner"], Categoria = "Hardware", AutorId = ids["u4"],
            },
            new Artigo
            {
                Id = ids["a4"] = Guid.NewGuid(), Titulo = "Ecrã azul e reinícios constantes (memória RAM)",
                Conteudo = "Passo 1: Anote o código do erro apresentado no ecrã azul. Passo 2: Reinicie e teste com um só módulo de memória. Passo 3: Se o erro persistir, a memória avariada deve ser substituída pelo técnico.",
                Tags = ["ecrã azul", "reinicia", "crash", "ram", "memória", "bsod"], Categoria = "Hardware", AutorId = ids["u3"],
            },
            new Artigo
            {
                Id = ids["a5"] = Guid.NewGuid(), Titulo = "Computador sem energia (fonte de alimentação)",
                Conteudo = "Passo 1: Confirme o cabo na tomada e no computador. Passo 2: Teste noutra tomada com outro cabo se possível. Passo 3: Se nem a luz do carregador acende, a fonte poderá estar avariada — o técnico deve substituí-la.",
                Tags = ["não liga", "energia", "fonte", "carregador", "luz"], Categoria = "Hardware", AutorId = ids["u3"],
            },
            new Artigo
            {
                Id = ids["a6"] = Guid.NewGuid(), Titulo = "Projetor sem sinal (HDMI)",
                Conteudo = "Passo 1: Verifique se o cabo HDMI está bem encaixado nos dois lados. Passo 2: Teste com outra entrada ou outro cabo. Passo 3: Confirme a fonte selecionada no projetor (HDMI 1/2).",
                Tags = ["projetor", "hdmi", "sem sinal", "imagem"], Categoria = "Hardware", AutorId = ids["u3"],
            },
            new Artigo
            {
                Id = ids["a7"] = Guid.NewGuid(), Titulo = "Email não sincroniza (Outlook)",
                Conteudo = "Passo 1: Feche e reabra o Outlook. Passo 2: Verifique a ligação à internet. Passo 3: Se persistir, o perfil da conta pode estar corrompido — o técnico deve recriar o perfil.",
                Tags = ["email", "outlook", "sincroniza", "correio"], Categoria = "Software", AutorId = ids["u4"],
            },
            new Artigo
            {
                Id = ids["a8"] = Guid.NewGuid(), Titulo = "Conta bloqueada no sistema (ERP)",
                Conteudo = "Passo 1: Confirme que escreve a palavra-passe sem bloquear maiúsculas. Passo 2: Aguarde 5 minutos e tente novamente. Passo 3: Se continuar bloqueada, peça ao técnico para desbloquear e redefinir.",
                Tags = ["conta", "bloqueada", "erp", "password", "senha", "acesso"], Categoria = "Contas e acessos", AutorId = ids["u3"],
            },
            new Artigo
            {
                Id = ids["a9"] = Guid.NewGuid(), Titulo = "Teclado com teclas a falhar",
                Conteudo = "Passo 1: Reinicie o computador e teste de novo. Passo 2: Limpe as teclas afetadas com ar comprimido. Passo 3: Se as falhas continuarem, o teclado deve ser substituído pelo técnico.",
                Tags = ["teclado", "teclas", "falha", "input"], Categoria = "Hardware", AutorId = ids["u4"],
            });

        // ─── BLOCO 10: Conversas de chat ──────────────────────
        var mensagens = new List<Mensagem>();

        void AddMensagem(string id, string solicitacao, string autor, string texto, DateOnly data, TimeOnly hora, bool lida = true)
        {
            mensagens.Add(new Mensagem
            {
                Id = ids[id] = Guid.NewGuid(),
                SolicitacaoId = ids[solicitacao],
                AutorId = ids[autor],
                Texto = texto,
                Data = data,
                Hora = hora,
                Lida = lida,
            });
        }

        // s1 (Computador não liga) — aberta, a pedir ajuda
        AddMensagem("msg1", "s1", "u5", "Olá, o meu computador não liga. Já tentei várias tomadas e nada.", new DateOnly(2026, 8, 30), new TimeOnly(9, 15));
        AddMensagem("msg2", "s1", "u3", "Olá Pedro, vou ver isto. Pode-me dizer se ouve algum som ao ligar?", new DateOnly(2026, 8, 30), new TimeOnly(9, 32));
        AddMensagem("msg3", "s1", "u5", "Não, nem a luz acende sequer.", new DateOnly(2026, 8, 30), new TimeOnly(9, 40));
        AddMensagem("msg4", "s1", "u3", "Percebido. Suspeito de problema na fonte de alimentação. Vou verificar.", new DateOnly(2026, 8, 30), new TimeOnly(9, 48));
        AddMensagem("msg5", "s1", "u3", "Já reservei uma fonte nova no stock. Assim que chegar troco-lhe.", new DateOnly(2026, 9, 2), new TimeOnly(11, 12), lida: false);

        // s2 (Impressora a fazer barulho) — aberta, sem assumir
        AddMensagem("msg6", "s2", "u6", "A impressora da receção está a fazer um barulho estranho ao imprimir.", new DateOnly(2026, 8, 31), new TimeOnly(11, 5));
        AddMensagem("msg7", "s2", "u3", "Obrigado Ana. Já está na fila, vou ver assim que conseguir passar pela receção.", new DateOnly(2026, 8, 31), new TimeOnly(11, 20));

        // s3 (Ecrã azul) — resolvida com troca de RAM
        AddMensagem("msg8", "s3", "u5", "O computador está a reiniciar sozinho com ecrã azul, já aconteceu 3 vezes hoje.", new DateOnly(2026, 8, 10), new TimeOnly(8, 50));
        AddMensagem("msg9", "s3", "u3", "Felizmente identifiquei o problema — a RAM está com defeito. Vou substituí-la.", new DateOnly(2026, 8, 10), new TimeOnly(10, 20));
        AddMensagem("msg10", "s3", "u5", "Muito obrigado, está a funcionar perfeitamente agora!", new DateOnly(2026, 8, 12), new TimeOnly(14, 10));

        // s4 (Não consigo imprimir) — resolvida pela Marta
        AddMensagem("msg11", "s4", "u6", "A impressora não responde, fica em estado de erro.", new DateOnly(2026, 8, 12), new TimeOnly(10, 30));
        AddMensagem("msg12", "s4", "u4", "Já identifiquei — a roda de alimentação ficou desalinhada. Vou corrigir.", new DateOnly(2026, 8, 12), new TimeOnly(15, 20));
        AddMensagem("msg13", "s4", "u6", "Demorou um pouco mas ficou bom. Obrigada!", new DateOnly(2026, 8, 14), new TimeOnly(9, 10));

        // s5 (Wi-Fi) — resolvida rápido
        AddMensagem("msg14", "s5", "u6", "Não consigo ligar-me ao Wi-Fi da empresa em lado nenhum.", new DateOnly(2026, 8, 14), new TimeOnly(9, 0));
        AddMensagem("msg15", "s5", "u3", "Era o driver de rede desatualizado. Já atualizei, está resolvido.", new DateOnly(2026, 8, 14), new TimeOnly(9, 25));
        AddMensagem("msg16", "s5", "u6", "Perfeito, voltou a ligar, obrigada!", new DateOnly(2026, 8, 14), new TimeOnly(9, 30));

        // s6 (Laptop lento) — teve rejeição
        AddMensagem("msg17", "s6", "u5", "O laptop continua muito lento depois da última limpeza.", new DateOnly(2026, 8, 16), new TimeOnly(10, 10));
        AddMensagem("msg18", "s6", "u3", "Peço desculpa, vou verificar novamente se há algum processo a consumir recursos.", new DateOnly(2026, 8, 16), new TimeOnly(10, 30));

        // s7 (Email) — resolvida pela Marta
        AddMensagem("msg19", "s7", "u6", "O Outlook continua sem sincronizar depois de reiniciar o computador.", new DateOnly(2026, 8, 18), new TimeOnly(9, 40));
        AddMensagem("msg20", "s7", "u4", "Era um perfil corrompido. Recriei-o e está tudo operacional.", new DateOnly(2026, 8, 19), new TimeOnly(15, 5));
        AddMensagem("msg21", "s7", "u6", "Muito obrigada pela ajuda!", new DateOnly(2026, 8, 19), new TimeOnly(15, 12));

        // s8 (Teclado) — com rejeições
        AddMensagem("msg22", "s8", "u5", "As teclas A e S do teclado estão a falhar sem razão aparente.", new DateOnly(2026, 8, 20), new TimeOnly(11, 30));
        AddMensagem("msg23", "s8", "u4", "Substituí o teclado, mas o problema pode estar na placa-mãe. Vou acompanhar.", new DateOnly(2026, 8, 22), new TimeOnly(16, 20));
        AddMensagem("msg24", "s8", "u5", "Ainda falha de vez em quando...", new DateOnly(2026, 8, 23), new TimeOnly(9, 50));
        AddMensagem("msg30", "s8", "u4", "Entendido. Vou precisar de ver o equipamento novamente para confirmar.", new DateOnly(2026, 8, 23), new TimeOnly(10, 5));

        // s9 (Projector) — resolvida
        AddMensagem("msg25", "s9", "u5", "O projector da sala de reuniões não liga em nenhum caso.", new DateOnly(2026, 8, 22), new TimeOnly(14, 0));
        AddMensagem("msg26", "s9", "u3", "O cabo HDMI estava danificado. Já o substituí, testei e está a funcionar.", new DateOnly(2026, 8, 23), new TimeOnly(10, 15));

        // s10 (Acesso ERP) — resolvida, sem avaliação
        AddMensagem("msg27", "s10", "u6", "A minha conta no ERP foi bloqueada e não consigo entrar.", new DateOnly(2026, 8, 25), new TimeOnly(8, 30));
        AddMensagem("msg28", "s10", "u3", "A conta foi bloqueada por excesso de tentativas. Já desbloqueei e reiniciei a senha.", new DateOnly(2026, 8, 25), new TimeOnly(8, 55));
        AddMensagem("msg29", "s10", "u6", "Já consigo entrar, obrigada!", new DateOnly(2026, 8, 25), new TimeOnly(9, 5));

        db.Mensagens.AddRange(mensagens);

        // ─── BLOCO 11: Relatórios técnicos ────────────────────
        // Um relatório por ordem resolvida (FK 1-1), em estados variados para
        // demonstrar o fluxo rascunho -> finalizado -> aprovado.
        db.RelatoriosTecnicos.AddRange(
            new RelatorioTecnico
            {
                Id = ids["rt1"] = Guid.NewGuid(), OrdemId = ids["or1"], AutorId = ids["u3"],
                CriadoEm = new DateOnly(2026, 8, 12), AtualizadoEm = new DateOnly(2026, 8, 13),
                Status = StatusRelatorio.aprovado,
                Sumario = "Substituição do módulo RAM avariado resolveu os crashes de ecrã azul.",
                Diagnostico = "RAM com defeito, a causar crashes azuis.",
                SolucaoAplicada = "Substituída a módulo RAM defeituoso.",
                PecasUsadas = [new PecaRelatorio { Nome = "RAM 8GB DDR4", Quantidade = 1, Codigo = "RAM-050" }],
                TempoGastoMin = 35,
                Procedimentos = "1) Abertos testes de memória, que confirmaram erro no módulo instalado.\n2) Substituído o módulo por unidade nova do stock.\n3) Reiniciado o equipamento e repetidos os testes com sucesso.",
                Observacoes = "A peça antiga foi devolvida como avariada para análise do fabricante.",
                AssinaturaTecnico = "João Técnico", AssinaturaResponsavel = "Carlos Gestor", ComentariosInternos = "",
            },
            new RelatorioTecnico
            {
                Id = ids["rt2"] = Guid.NewGuid(), OrdemId = ids["or2"], AutorId = ids["u4"],
                CriadoEm = new DateOnly(2026, 8, 14), AtualizadoEm = new DateOnly(2026, 8, 14),
                Status = StatusRelatorio.finalizado,
                Sumario = "Corrigido alinhamento da roda de alimentação da impressora.",
                Diagnostico = "Roda de alimentação de papel desalinhada.",
                SolucaoAplicada = "Realinhei a roda e limpei os guias.",
                PecasUsadas = [],
                TempoGastoMin = 50,
                Procedimentos = "1) Retirado o conjunto de alimentação frontal.\n2) Realinhada a roda de tração.\n3) Limpeza dos guias de papel.\n4) Teste de impressão de 20 folhas sem falhas.",
                Observacoes = "",
                AssinaturaTecnico = "Marta Técnica", AssinaturaResponsavel = "",
                ComentariosInternos = "Recomendo plano de manutenção preventiva trimestral.",
            },
            new RelatorioTecnico
            {
                Id = ids["rt3"] = Guid.NewGuid(), OrdemId = ids["or3"], AutorId = ids["u3"],
                CriadoEm = new DateOnly(2026, 8, 14), AtualizadoEm = new DateOnly(2026, 8, 14),
                Status = StatusRelatorio.rascunho,
                Sumario = "",
                Diagnostico = "Driver de rede desatualizado.",
                SolucaoAplicada = "Atualizei o driver e reiniciei o serviço Wi-Fi.",
                PecasUsadas = [],
                TempoGastoMin = 15,
                Procedimentos = "", Observacoes = "",
                AssinaturaTecnico = "", AssinaturaResponsavel = "",
                ComentariosInternos = "Falta detalhar os passos executados.",
            },
            new RelatorioTecnico
            {
                Id = ids["rt4"] = Guid.NewGuid(), OrdemId = ids["or4"], AutorId = ids["u3"],
                CriadoEm = new DateOnly(2026, 8, 18), AtualizadoEm = new DateOnly(2026, 8, 18),
                Status = StatusRelatorio.finalizado,
                Sumario = "Limpeza de disco e desativação de programas desnecessários.",
                Diagnostico = "Disco SSD quase cheio e muitos programas em arranque.",
                SolucaoAplicada = "Limpei o disco e desativei programas desnecessários.",
                PecasUsadas = [],
                TempoGastoMin = 90,
                Procedimentos = "1) Identificação de ficheiros temporários e caches.\n2) Remoção de dados desnecessários (cerca de 60GB).\n3) Desativação de programas de arranque.\n4) Reboot e teste de arranque em tempo aceitável.",
                Observacoes = "Rejeição inicial da solução foi respondida com nova limpeza de processos em segundo plano.",
                AssinaturaTecnico = "João Técnico", AssinaturaResponsavel = "", ComentariosInternos = "",
            },
            new RelatorioTecnico
            {
                Id = ids["rt5"] = Guid.NewGuid(), OrdemId = ids["or5"], AutorId = ids["u4"],
                CriadoEm = new DateOnly(2026, 8, 19), AtualizadoEm = new DateOnly(2026, 8, 19),
                Status = StatusRelatorio.rascunho,
                Sumario = "",
                Diagnostico = "Perfil corrompido no Outlook.",
                SolucaoAplicada = "Recriei o perfil e reconfigurei as contas.",
                PecasUsadas = [],
                TempoGastoMin = 40,
                Procedimentos = "", Observacoes = "",
                AssinaturaTecnico = "", AssinaturaResponsavel = "", ComentariosInternos = "",
            },
            new RelatorioTecnico
            {
                Id = ids["rt6"] = Guid.NewGuid(), OrdemId = ids["or6"], AutorId = ids["u4"],
                CriadoEm = new DateOnly(2026, 8, 25), AtualizadoEm = new DateOnly(2026, 8, 25),
                Status = StatusRelatorio.finalizado,
                Sumario = "Substituição do teclado interno.",
                Diagnostico = "Desgaste físico das teclas.",
                SolucaoAplicada = "Substituí o teclado interno.",
                PecasUsadas = [],
                TempoGastoMin = 120,
                Procedimentos = "1) Abertura do equipamento e remoção do teclado.\n2) Instalação do novo teclado interno.\n3) Teste completo dos caracteres.\n4) Rejeições anteriores investigadas — confirmou-se intermitência residual na placa-mãe, acompanhamento em curso.",
                Observacoes = "A troca de teclado não resolveu por completo o sintoma inicial; o problema secundário na placa-mãe fica sob monitorização.",
                AssinaturaTecnico = "Marta Técnica", AssinaturaResponsavel = "", ComentariosInternos = "",
            },
            new RelatorioTecnico
            {
                Id = ids["rt7"] = Guid.NewGuid(), OrdemId = ids["or7"], AutorId = ids["u3"],
                CriadoEm = new DateOnly(2026, 8, 23), AtualizadoEm = new DateOnly(2026, 8, 23),
                Status = StatusRelatorio.aprovado,
                Sumario = "Substituição do cabo HDMI e verificação do projector.",
                Diagnostico = "Cabo HDMI com contactos oxidados.",
                SolucaoAplicada = "Substituí o cabo e testei com outro projector.",
                PecasUsadas = [],
                TempoGastoMin = 20,
                Procedimentos = "1) Inspeção do cabo HDMI da sala de reuniões.\n2) Substituição por cabo novo.\n3) Teste de ligação e imagem.",
                Observacoes = "",
                AssinaturaTecnico = "João Técnico", AssinaturaResponsavel = "Carlos Gestor", ComentariosInternos = "",
            },
            new RelatorioTecnico
            {
                Id = ids["rt8"] = Guid.NewGuid(), OrdemId = ids["or8"], AutorId = ids["u3"],
                CriadoEm = new DateOnly(2026, 8, 25), AtualizadoEm = new DateOnly(2026, 8, 26),
                Status = StatusRelatorio.rascunho,
                Sumario = "",
                Diagnostico = "Conta bloqueada por excesso de tentativas.",
                SolucaoAplicada = "Desbloqueei a conta e reiniciei a senha.",
                PecasUsadas = [],
                TempoGastoMin = 10,
                Procedimentos = "",
                Observacoes = "Sugerir ativação de autenticação em dois fatores.",
                AssinaturaTecnico = "", AssinaturaResponsavel = "", ComentariosInternos = "",
            });

        // O tempo do relatório é sempre o cronometrado na ordem (ver RelatorioService.CreateAsync).
        foreach (var r in db.RelatoriosTecnicos.Local)
            r.TempoGastoMin = ordens.First(o => o.Id == r.OrdemId).TempoGastoMin;

        // Histórico dos 8 relatórios (CRIADO -> [EDITADO] -> FINALIZADO -> APROVADO)
        var posicoesRelatorio = new Dictionary<string, int>();
        void AddHistRelatorio(string relatorio, string autor, DateOnly data, AcaoRelatorio acao, string campo = "", string valorAntigo = "", string valorNovo = "")
        {
            var posicao = posicoesRelatorio.GetValueOrDefault(relatorio);
            posicoesRelatorio[relatorio] = posicao + 1;
            db.RelatorioHistoricos.Add(new RelatorioHistorico
            {
                Id = Guid.NewGuid(),
                RelatorioId = ids[relatorio],
                AutorId = ids[autor],
                Data = data,
                Posicao = posicao,
                Acao = acao,
                Campo = campo,
                ValorAntigo = valorAntigo,
                ValorNovo = valorNovo,
            });
        }

        AddHistRelatorio("rt1", "u3", new DateOnly(2026, 8, 12), AcaoRelatorio.CRIADO);
        AddHistRelatorio("rt1", "u3", new DateOnly(2026, 8, 12), AcaoRelatorio.FINALIZADO);
        AddHistRelatorio("rt1", "u2", new DateOnly(2026, 8, 13), AcaoRelatorio.APROVADO);

        AddHistRelatorio("rt2", "u4", new DateOnly(2026, 8, 14), AcaoRelatorio.CRIADO);
        AddHistRelatorio("rt2", "u4", new DateOnly(2026, 8, 14), AcaoRelatorio.FINALIZADO);

        AddHistRelatorio("rt3", "u3", new DateOnly(2026, 8, 14), AcaoRelatorio.CRIADO);

        AddHistRelatorio("rt4", "u3", new DateOnly(2026, 8, 18), AcaoRelatorio.CRIADO);
        AddHistRelatorio("rt4", "u3", new DateOnly(2026, 8, 18), AcaoRelatorio.FINALIZADO);

        AddHistRelatorio("rt5", "u4", new DateOnly(2026, 8, 19), AcaoRelatorio.CRIADO);

        AddHistRelatorio("rt6", "u4", new DateOnly(2026, 8, 25), AcaoRelatorio.CRIADO);
        AddHistRelatorio("rt6", "u4", new DateOnly(2026, 8, 25), AcaoRelatorio.FINALIZADO);

        AddHistRelatorio("rt7", "u3", new DateOnly(2026, 8, 23), AcaoRelatorio.CRIADO);
        AddHistRelatorio("rt7", "u3", new DateOnly(2026, 8, 23), AcaoRelatorio.FINALIZADO);
        AddHistRelatorio("rt7", "u2", new DateOnly(2026, 8, 23), AcaoRelatorio.APROVADO);

        AddHistRelatorio("rt8", "u3", new DateOnly(2026, 8, 25), AcaoRelatorio.CRIADO);
        AddHistRelatorio("rt8", "u3", new DateOnly(2026, 8, 26), AcaoRelatorio.EDITADO, "observacoes", "", "Sugerir ativação de autenticação em dois fatores.");

        await db.SaveChangesAsync();
    }
}
