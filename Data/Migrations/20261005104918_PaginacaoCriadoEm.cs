using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Novati.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class PaginacaoCriadoEm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CriadoEm",
                table: "Solicitacoes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CriadoEm",
                table: "RequisicoesCompra",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CriadoEm",
                table: "MovimentosStock",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "Solicitacoes");

            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "RequisicoesCompra");

            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "MovimentosStock");
        }
    }
}
