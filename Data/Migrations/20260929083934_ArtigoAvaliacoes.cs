using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Novati.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ArtigoAvaliacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArtigoAvaliacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Util = table.Column<bool>(type: "boolean", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    ArtigoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtigoAvaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtigoAvaliacoes_Artigos_ArtigoId",
                        column: x => x.ArtigoId,
                        principalTable: "Artigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtigoAvaliacoes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtigoAvaliacoes_ArtigoId_UserId",
                table: "ArtigoAvaliacoes",
                columns: new[] { "ArtigoId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtigoAvaliacoes_UserId",
                table: "ArtigoAvaliacoes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtigoAvaliacoes");
        }
    }
}
