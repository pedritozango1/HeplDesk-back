using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Novati.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class HistoricoPosicao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Posicao",
                table: "RelatorioHistoricos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Posicao",
                table: "OrdemHistoricos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Linhas já existentes: posição por data e, no mesmo dia, pela ordem lógica do fluxo.
            migrationBuilder.Sql("""
                UPDATE "OrdemHistoricos" h SET "Posicao" = x.pos
                FROM (SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "OrdemId" ORDER BY "Data",
                        CASE "Tipo" WHEN 'ASSUMIDA' THEN 0 WHEN 'REATRIBUIDA' THEN 1 WHEN 'DIAGNOSTICO' THEN 2
                                    WHEN 'COMENTARIO' THEN 3 WHEN 'SOLUCAO' THEN 4 WHEN 'REJEITADA' THEN 5
                                    WHEN 'ACEITE' THEN 6 ELSE 7 END) - 1 AS pos
                      FROM "OrdemHistoricos") x
                WHERE h."Id" = x."Id";
                """);
            migrationBuilder.Sql("""
                UPDATE "RelatorioHistoricos" h SET "Posicao" = x.pos
                FROM (SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "RelatorioId" ORDER BY "Data",
                        CASE "Acao" WHEN 'CRIADO' THEN 0 WHEN 'EDITADO' THEN 1 WHEN 'REABERTO' THEN 2
                                    WHEN 'FINALIZADO' THEN 3 WHEN 'APROVADO' THEN 4 ELSE 5 END) - 1 AS pos
                      FROM "RelatorioHistoricos") x
                WHERE h."Id" = x."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Posicao",
                table: "RelatorioHistoricos");

            migrationBuilder.DropColumn(
                name: "Posicao",
                table: "OrdemHistoricos");
        }
    }
}
