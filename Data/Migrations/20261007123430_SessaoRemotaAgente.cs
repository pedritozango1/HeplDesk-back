using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Novati.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class SessaoRemotaAgente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RustDeskId",
                table: "SessoesRemotas");

            migrationBuilder.DropColumn(
                name: "RustDeskSenhaCifrada",
                table: "SessoesRemotas");

            migrationBuilder.AddColumn<string>(
                name: "AgentePc",
                table: "SessoesRemotas",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentePc",
                table: "SessoesRemotas");

            migrationBuilder.AddColumn<string>(
                name: "RustDeskId",
                table: "SessoesRemotas",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RustDeskSenhaCifrada",
                table: "SessoesRemotas",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }
    }
}
