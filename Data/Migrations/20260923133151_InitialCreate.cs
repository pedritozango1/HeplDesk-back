using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Novati.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Localizacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PaiId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localizacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Localizacoes_Localizacoes_PaiId",
                        column: x => x.PaiId,
                        principalTable: "Localizacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModelosComponente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Capacidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StockMinimo = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelosComponente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModelosDispositivo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Fabricante = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelosDispositivo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Assinatura = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItensStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModeloComponenteId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensStock_ModelosComponente_ModeloComponenteId",
                        column: x => x.ModeloComponenteId,
                        principalTable: "ModelosComponente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Compatibilidades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModeloDispositivoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModeloComponenteId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compatibilidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compatibilidades_ModelosComponente_ModeloComponenteId",
                        column: x => x.ModeloComponenteId,
                        principalTable: "ModelosComponente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Compatibilidades_ModelosDispositivo_ModeloDispositivoId",
                        column: x => x.ModeloDispositivoId,
                        principalTable: "ModelosDispositivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Artigos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Conteudo = table.Column<string>(type: "text", nullable: false),
                    Categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    AutorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artigos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Artigos_Users_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispositivosFisicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Patrimonio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NumeroSerie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    DataAquisicao = table.Column<DateOnly>(type: "date", nullable: true),
                    GarantiaMeses = table.Column<int>(type: "integer", nullable: true),
                    ModeloDispositivoId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalizacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsavelId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispositivosFisicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispositivosFisicos_Localizacoes_LocalizacaoId",
                        column: x => x.LocalizacaoId,
                        principalTable: "Localizacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispositivosFisicos_ModelosDispositivo_ModeloDispositivoId",
                        column: x => x.ModeloDispositivoId,
                        principalTable: "ModelosDispositivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispositivosFisicos_Users_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Lida = table.Column<bool>(type: "boolean", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimentosStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ItemStockId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentosStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentosStock_ItensStock_ItemStockId",
                        column: x => x.ItemStockId,
                        principalTable: "ItensStock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InstanciasComponentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    DispositivoFisicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModeloComponenteId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstanciasComponentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstanciasComponentes_DispositivosFisicos_DispositivoFisico~",
                        column: x => x.DispositivoFisicoId,
                        principalTable: "DispositivosFisicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstanciasComponentes_ModelosComponente_ModeloComponenteId",
                        column: x => x.ModeloComponenteId,
                        principalTable: "ModelosComponente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Solicitacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    Prioridade = table.Column<string>(type: "text", nullable: false),
                    Categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataCriacao = table.Column<DateOnly>(type: "date", nullable: false),
                    ResolvidaViaBase = table.Column<bool>(type: "boolean", nullable: false),
                    SolicitanteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispositivoFisicoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Avaliacao_Estrelas = table.Column<int>(type: "integer", nullable: true),
                    Avaliacao_Comentario = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Avaliacao_Data = table.Column<DateOnly>(type: "date", nullable: true),
                    Anexos = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitacoes_DispositivosFisicos_DispositivoFisicoId",
                        column: x => x.DispositivoFisicoId,
                        principalTable: "DispositivosFisicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitacoes_Users_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Texto = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Hora = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Lida = table.Column<bool>(type: "boolean", nullable: false),
                    SolicitacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensagens_Solicitacoes_SolicitacaoId",
                        column: x => x.SolicitacaoId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mensagens_Users_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdensReparo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Diagnostico = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Solucao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SolucaoSugerida = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    TempoGastoMin = table.Column<int>(type: "integer", nullable: true),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: true),
                    SolicitacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TecnicoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensReparo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdensReparo_Solicitacoes_SolicitacaoId",
                        column: x => x.SolicitacaoId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdensReparo_Users_TecnicoId",
                        column: x => x.TecnicoId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdemHistoricos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    OrdemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemHistoricos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemHistoricos_OrdensReparo_OrdemId",
                        column: x => x.OrdemId,
                        principalTable: "OrdensReparo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdemHistoricos_Users_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrdemRejeicoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    OrdemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemRejeicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemRejeicoes_OrdensReparo_OrdemId",
                        column: x => x.OrdemId,
                        principalTable: "OrdensReparo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelatoriosTecnicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateOnly>(type: "date", nullable: false),
                    AtualizadoEm = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Sumario = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Diagnostico = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SolucaoAplicada = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    TempoGastoMin = table.Column<int>(type: "integer", nullable: true),
                    Procedimentos = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AssinaturaTecnico = table.Column<string>(type: "text", nullable: false),
                    AssinaturaResponsavel = table.Column<string>(type: "text", nullable: false),
                    ComentariosInternos = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    OrdemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PecasUsadas = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatoriosTecnicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelatoriosTecnicos_OrdensReparo_OrdemId",
                        column: x => x.OrdemId,
                        principalTable: "OrdensReparo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelatoriosTecnicos_Users_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequisicoesCompra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    Justificativa = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    ItemStockId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModeloComponenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    SolicitanteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisicoesCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequisicoesCompra_ItensStock_ItemStockId",
                        column: x => x.ItemStockId,
                        principalTable: "ItensStock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicoesCompra_ModelosComponente_ModeloComponenteId",
                        column: x => x.ModeloComponenteId,
                        principalTable: "ModelosComponente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicoesCompra_OrdensReparo_OrdemId",
                        column: x => x.OrdemId,
                        principalTable: "OrdensReparo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RequisicoesCompra_Users_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    ItemStockId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservadaParaOrdemId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnidadesStock_ItensStock_ItemStockId",
                        column: x => x.ItemStockId,
                        principalTable: "ItensStock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnidadesStock_OrdensReparo_ReservadaParaOrdemId",
                        column: x => x.ReservadaParaOrdemId,
                        principalTable: "OrdensReparo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RelatorioHistoricos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Acao = table.Column<string>(type: "text", nullable: false),
                    Campo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValorAntigo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ValorNovo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RelatorioId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatorioHistoricos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelatorioHistoricos_RelatoriosTecnicos_RelatorioId",
                        column: x => x.RelatorioId,
                        principalTable: "RelatoriosTecnicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelatorioHistoricos_Users_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdemPecasUsadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnidadeStockId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModeloComponenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstaladoInstanciaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemPecasUsadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemPecasUsadas_InstanciasComponentes_InstaladoInstanciaId",
                        column: x => x.InstaladoInstanciaId,
                        principalTable: "InstanciasComponentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrdemPecasUsadas_ModelosComponente_ModeloComponenteId",
                        column: x => x.ModeloComponenteId,
                        principalTable: "ModelosComponente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdemPecasUsadas_OrdensReparo_OrdemId",
                        column: x => x.OrdemId,
                        principalTable: "OrdensReparo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdemPecasUsadas_UnidadesStock_UnidadeStockId",
                        column: x => x.UnidadeStockId,
                        principalTable: "UnidadesStock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artigos_AutorId",
                table: "Artigos",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Compatibilidades_ModeloComponenteId_ModeloDispositivoId",
                table: "Compatibilidades",
                columns: new[] { "ModeloComponenteId", "ModeloDispositivoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Compatibilidades_ModeloDispositivoId",
                table: "Compatibilidades",
                column: "ModeloDispositivoId");

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosFisicos_LocalizacaoId",
                table: "DispositivosFisicos",
                column: "LocalizacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosFisicos_ModeloDispositivoId",
                table: "DispositivosFisicos",
                column: "ModeloDispositivoId");

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosFisicos_Patrimonio",
                table: "DispositivosFisicos",
                column: "Patrimonio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosFisicos_ResponsavelId",
                table: "DispositivosFisicos",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_InstanciasComponentes_DispositivoFisicoId",
                table: "InstanciasComponentes",
                column: "DispositivoFisicoId");

            migrationBuilder.CreateIndex(
                name: "IX_InstanciasComponentes_ModeloComponenteId",
                table: "InstanciasComponentes",
                column: "ModeloComponenteId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensStock_ModeloComponenteId",
                table: "ItensStock",
                column: "ModeloComponenteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Localizacoes_PaiId",
                table: "Localizacoes",
                column: "PaiId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_AutorId",
                table: "Mensagens",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_SolicitacaoId",
                table: "Mensagens",
                column: "SolicitacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentosStock_ItemStockId",
                table: "MovimentosStock",
                column: "ItemStockId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_UserId",
                table: "Notificacoes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemHistoricos_AutorId",
                table: "OrdemHistoricos",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemHistoricos_OrdemId",
                table: "OrdemHistoricos",
                column: "OrdemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemPecasUsadas_InstaladoInstanciaId",
                table: "OrdemPecasUsadas",
                column: "InstaladoInstanciaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemPecasUsadas_ModeloComponenteId",
                table: "OrdemPecasUsadas",
                column: "ModeloComponenteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemPecasUsadas_OrdemId",
                table: "OrdemPecasUsadas",
                column: "OrdemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemPecasUsadas_UnidadeStockId",
                table: "OrdemPecasUsadas",
                column: "UnidadeStockId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemRejeicoes_OrdemId",
                table: "OrdemRejeicoes",
                column: "OrdemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensReparo_SolicitacaoId",
                table: "OrdensReparo",
                column: "SolicitacaoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdensReparo_TecnicoId",
                table: "OrdensReparo",
                column: "TecnicoId");

            migrationBuilder.CreateIndex(
                name: "IX_RelatorioHistoricos_AutorId",
                table: "RelatorioHistoricos",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_RelatorioHistoricos_RelatorioId",
                table: "RelatorioHistoricos",
                column: "RelatorioId");

            migrationBuilder.CreateIndex(
                name: "IX_RelatoriosTecnicos_AutorId",
                table: "RelatoriosTecnicos",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_RelatoriosTecnicos_OrdemId",
                table: "RelatoriosTecnicos",
                column: "OrdemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequisicoesCompra_ItemStockId",
                table: "RequisicoesCompra",
                column: "ItemStockId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicoesCompra_ModeloComponenteId",
                table: "RequisicoesCompra",
                column: "ModeloComponenteId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicoesCompra_OrdemId",
                table: "RequisicoesCompra",
                column: "OrdemId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicoesCompra_SolicitanteId",
                table: "RequisicoesCompra",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_DispositivoFisicoId",
                table: "Solicitacoes",
                column: "DispositivoFisicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_SolicitanteId",
                table: "Solicitacoes",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesStock_Codigo",
                table: "UnidadesStock",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesStock_ItemStockId",
                table: "UnidadesStock",
                column: "ItemStockId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesStock_ReservadaParaOrdemId",
                table: "UnidadesStock",
                column: "ReservadaParaOrdemId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Artigos");

            migrationBuilder.DropTable(
                name: "Compatibilidades");

            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropTable(
                name: "MovimentosStock");

            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "OrdemHistoricos");

            migrationBuilder.DropTable(
                name: "OrdemPecasUsadas");

            migrationBuilder.DropTable(
                name: "OrdemRejeicoes");

            migrationBuilder.DropTable(
                name: "RelatorioHistoricos");

            migrationBuilder.DropTable(
                name: "RequisicoesCompra");

            migrationBuilder.DropTable(
                name: "InstanciasComponentes");

            migrationBuilder.DropTable(
                name: "UnidadesStock");

            migrationBuilder.DropTable(
                name: "RelatoriosTecnicos");

            migrationBuilder.DropTable(
                name: "ItensStock");

            migrationBuilder.DropTable(
                name: "OrdensReparo");

            migrationBuilder.DropTable(
                name: "ModelosComponente");

            migrationBuilder.DropTable(
                name: "Solicitacoes");

            migrationBuilder.DropTable(
                name: "DispositivosFisicos");

            migrationBuilder.DropTable(
                name: "Localizacoes");

            migrationBuilder.DropTable(
                name: "ModelosDispositivo");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
