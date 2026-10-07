using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Novati.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class SessoesRemotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessoesRemotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    Modo = table.Column<string>(type: "text", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TokenExpiraEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TokenUsadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TentativasFalhadas = table.Column<int>(type: "integer", nullable: false),
                    PedidaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondidaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IniciadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TerminadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TerminadaPorId = table.Column<Guid>(type: "uuid", nullable: true),
                    MotivoFim = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AutorizadaIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    SolicitacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TecnicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    SolicitanteId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessoesRemotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessoesRemotas_Solicitacoes_SolicitacaoId",
                        column: x => x.SolicitacaoId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessoesRemotas_Users_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessoesRemotas_Users_TecnicoId",
                        column: x => x.TecnicoId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessoesRemotas_SolicitacaoId",
                table: "SessoesRemotas",
                column: "SolicitacaoId",
                unique: true,
                filter: "\"Estado\" IN ('PEDIDA', 'AUTORIZADA', 'ATIVA')");

            migrationBuilder.CreateIndex(
                name: "IX_SessoesRemotas_SolicitanteId",
                table: "SessoesRemotas",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_SessoesRemotas_TecnicoId",
                table: "SessoesRemotas",
                column: "TecnicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessoesRemotas");
        }
    }
}
