using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Novati.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class CronometroOrdem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConcluidaEm",
                table: "OrdensReparo",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EsperaValidacaoSeg",
                table: "OrdensReparo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "IniciadaEm",
                table: "OrdensReparo",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PropostaEm",
                table: "OrdensReparo",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TempoTrabalhoMin",
                table: "OrdensReparo",
                type: "integer",
                nullable: true);

            // Ordens que já existiam: não há hora guardada, usa-se o início do dia em que foram
            // assumidas. ConcluidaEm fica null nas já resolvidas — o tempo delas foi declarado
            // pelo técnico, não cronometrado.
            migrationBuilder.Sql(
                """UPDATE "OrdensReparo" SET "IniciadaEm" = "DataInicio"::timestamp AT TIME ZONE 'UTC';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcluidaEm",
                table: "OrdensReparo");

            migrationBuilder.DropColumn(
                name: "EsperaValidacaoSeg",
                table: "OrdensReparo");

            migrationBuilder.DropColumn(
                name: "IniciadaEm",
                table: "OrdensReparo");

            migrationBuilder.DropColumn(
                name: "PropostaEm",
                table: "OrdensReparo");

            migrationBuilder.DropColumn(
                name: "TempoTrabalhoMin",
                table: "OrdensReparo");
        }
    }
}
