using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Produto",
                columns: table => new
                {
                    ProdutoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubcategoriaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PromocaoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ImagemUrl = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produto", x => x.ProdutoId);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NomeCompleto = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Senha = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Celular = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "date", nullable: false),
                    CPF = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.UsuarioId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_CPF",
                table: "Usuario",
                column: "CPF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Email",
                table: "Usuario",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Produto");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
