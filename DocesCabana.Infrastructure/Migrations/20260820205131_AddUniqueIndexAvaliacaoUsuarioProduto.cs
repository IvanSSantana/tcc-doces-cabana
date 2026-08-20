using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexAvaliacaoUsuarioProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Avaliacao_UsuarioId",
                table: "Avaliacao");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacao_UsuarioId_ProdutoId",
                table: "Avaliacao",
                columns: new[] { "UsuarioId", "ProdutoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Avaliacao_UsuarioId_ProdutoId",
                table: "Avaliacao");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacao_UsuarioId",
                table: "Avaliacao",
                column: "UsuarioId");
        }
    }
}
