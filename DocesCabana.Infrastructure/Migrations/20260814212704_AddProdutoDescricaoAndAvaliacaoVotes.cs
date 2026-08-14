using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProdutoDescricaoAndAvaliacaoVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpVote",
                table: "Avaliacao");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Produto",
                type: "TEXT",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Avaliacao",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "VotoUtil",
                columns: table => new
                {
                    AvaliacaoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotoUtil", x => new { x.AvaliacaoId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_VotoUtil_Avaliacao_AvaliacaoId",
                        column: x => x.AvaliacaoId,
                        principalTable: "Avaliacao",
                        principalColumn: "AvaliacaoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VotoUtil_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VotoUtil_UsuarioId",
                table: "VotoUtil",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VotoUtil");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Avaliacao");

            migrationBuilder.AddColumn<bool>(
                name: "UpVote",
                table: "Avaliacao",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
