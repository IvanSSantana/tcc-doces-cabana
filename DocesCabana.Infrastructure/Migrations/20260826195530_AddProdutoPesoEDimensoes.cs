using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProdutoPesoEDimensoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Altura",
                table: "Produto",
                type: "decimal(10,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Comprimento",
                table: "Produto",
                type: "decimal(10,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Largura",
                table: "Produto",
                type: "decimal(10,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Peso",
                table: "Produto",
                type: "decimal(10,3)",
                nullable: false,
                defaultValue: 0m);

            // Os cem produtos já existentes nasceram com 0 (o padrão
            // temporário acima, exigência de NOT NULL sobre tabela povoada).
            // Um valor por categoria, não único para todos (spec 020, plano
            // §5) — Adega pesada e compacta, Souvenir leve e volumosa é o
            // par que faz o peso cubado da transportadora divergir do peso
            // real; um valor único faria os dois sempre coincidirem.
            migrationBuilder.Sql(@"
                UPDATE Produto SET Peso = 1.200, Altura = 32, Largura = 8, Comprimento = 8
                WHERE SubcategoriaId IN (
                    SELECT SubcategoriaId FROM Subcategoria WHERE CategoriaId IN (
                        SELECT CategoriaId FROM Categoria WHERE Nome = 'Adega'));");

            migrationBuilder.Sql(@"
                UPDATE Produto SET Peso = 0.400, Altura = 12, Largura = 15, Comprimento = 15
                WHERE SubcategoriaId IN (
                    SELECT SubcategoriaId FROM Subcategoria WHERE CategoriaId IN (
                        SELECT CategoriaId FROM Categoria WHERE Nome = 'Doces'));");

            migrationBuilder.Sql(@"
                UPDATE Produto SET Peso = 0.500, Altura = 14, Largura = 10, Comprimento = 10
                WHERE SubcategoriaId IN (
                    SELECT SubcategoriaId FROM Subcategoria WHERE CategoriaId IN (
                        SELECT CategoriaId FROM Categoria WHERE Nome = 'Empório'));");

            migrationBuilder.Sql(@"
                UPDATE Produto SET Peso = 0.300, Altura = 20, Largura = 25, Comprimento = 30
                WHERE SubcategoriaId IN (
                    SELECT SubcategoriaId FROM Subcategoria WHERE CategoriaId IN (
                        SELECT CategoriaId FROM Categoria WHERE Nome = 'Souvenir'));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Altura",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "Comprimento",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "Largura",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "Peso",
                table: "Produto");
        }
    }
}
