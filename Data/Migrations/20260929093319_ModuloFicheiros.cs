using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Novati.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModuloFicheiros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A coluna antiga guardava a assinatura como texto (dataURL base64) — no seed era
            // só o nome da pessoa, o que fazia o front achar que já havia assinatura e nunca
            // a pedir. Descarta-se: cada utilizador volta a definir a assinatura no Perfil.
            migrationBuilder.DropColumn(
                name: "Assinatura",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "AssinaturaFicheiroId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssinaturaTecnicoFicheiroId",
                table: "RelatoriosTecnicos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Ficheiros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeOriginal = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    CaminhoRelativo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ficheiros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ficheiros_Users_CriadoPorId",
                        column: x => x.CriadoPorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_AssinaturaFicheiroId",
                table: "Users",
                column: "AssinaturaFicheiroId");

            migrationBuilder.CreateIndex(
                name: "IX_RelatoriosTecnicos_AssinaturaTecnicoFicheiroId",
                table: "RelatoriosTecnicos",
                column: "AssinaturaTecnicoFicheiroId");

            migrationBuilder.CreateIndex(
                name: "IX_Ficheiros_CriadoPorId",
                table: "Ficheiros",
                column: "CriadoPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelatoriosTecnicos_Ficheiros_AssinaturaTecnicoFicheiroId",
                table: "RelatoriosTecnicos",
                column: "AssinaturaTecnicoFicheiroId",
                principalTable: "Ficheiros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Ficheiros_AssinaturaFicheiroId",
                table: "Users",
                column: "AssinaturaFicheiroId",
                principalTable: "Ficheiros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelatoriosTecnicos_Ficheiros_AssinaturaTecnicoFicheiroId",
                table: "RelatoriosTecnicos");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Ficheiros_AssinaturaFicheiroId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Ficheiros");

            migrationBuilder.DropIndex(
                name: "IX_Users_AssinaturaFicheiroId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_RelatoriosTecnicos_AssinaturaTecnicoFicheiroId",
                table: "RelatoriosTecnicos");

            migrationBuilder.DropColumn(
                name: "AssinaturaFicheiroId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AssinaturaTecnicoFicheiroId",
                table: "RelatoriosTecnicos");

            migrationBuilder.AddColumn<string>(
                name: "Assinatura",
                table: "Users",
                type: "text",
                nullable: true);
        }
    }
}
