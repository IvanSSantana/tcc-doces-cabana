using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromocoesDeProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_Promocao_PromocaoId",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Produto_PromocaoId",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "PromocaoId",
                table: "Produto");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DataInicio",
                table: "Promocao",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DataFim",
                table: "Promocao",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaId",
                table: "Promocao",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "QuantidadeLeve",
                table: "Promocao",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "QuantidadePague",
                table: "Promocao",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubcategoriaId",
                table: "Promocao",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DescontoDaPromocao",
                table: "ItemPedido",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RotuloDaPromocao",
                table: "ItemPedido",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PromocaoProduto",
                columns: table => new
                {
                    ProdutosProdutoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromocaoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromocaoProduto", x => new { x.ProdutosProdutoId, x.PromocaoId });
                    table.ForeignKey(
                        name: "FK_PromocaoProduto_Produto_ProdutosProdutoId",
                        column: x => x.ProdutosProdutoId,
                        principalTable: "Produto",
                        principalColumn: "ProdutoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromocaoProduto_Promocao_PromocaoId",
                        column: x => x.PromocaoId,
                        principalTable: "Promocao",
                        principalColumn: "PromocaoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Promocao_CategoriaId",
                table: "Promocao",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Promocao_SubcategoriaId",
                table: "Promocao",
                column: "SubcategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_PromocaoProduto_PromocaoId",
                table: "PromocaoProduto",
                column: "PromocaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Promocao_Categoria_CategoriaId",
                table: "Promocao",
                column: "CategoriaId",
                principalTable: "Categoria",
                principalColumn: "CategoriaId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Promocao_Subcategoria_SubcategoriaId",
                table: "Promocao",
                column: "SubcategoriaId",
                principalTable: "Subcategoria",
                principalColumn: "SubcategoriaId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Promocao_Categoria_CategoriaId",
                table: "Promocao");

            migrationBuilder.DropForeignKey(
                name: "FK_Promocao_Subcategoria_SubcategoriaId",
                table: "Promocao");

            migrationBuilder.DropTable(
                name: "PromocaoProduto");

            migrationBuilder.DropIndex(
                name: "IX_Promocao_CategoriaId",
                table: "Promocao");

            migrationBuilder.DropIndex(
                name: "IX_Promocao_SubcategoriaId",
                table: "Promocao");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Promocao");

            migrationBuilder.DropColumn(
                name: "QuantidadeLeve",
                table: "Promocao");

            migrationBuilder.DropColumn(
                name: "QuantidadePague",
                table: "Promocao");

            migrationBuilder.DropColumn(
                name: "SubcategoriaId",
                table: "Promocao");

            migrationBuilder.DropColumn(
                name: "DescontoDaPromocao",
                table: "ItemPedido");

            migrationBuilder.DropColumn(
                name: "RotuloDaPromocao",
                table: "ItemPedido");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataInicio",
                table: "Promocao",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataFim",
                table: "Promocao",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<Guid>(
                name: "PromocaoId",
                table: "Produto",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produto_PromocaoId",
                table: "Produto",
                column: "PromocaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_Promocao_PromocaoId",
                table: "Produto",
                column: "PromocaoId",
                principalTable: "Promocao",
                principalColumn: "PromocaoId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
