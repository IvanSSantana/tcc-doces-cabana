using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCupons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoDoCupom",
                table: "Pedido",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CupomId",
                table: "Pedido",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DescontoDoCupom",
                table: "Pedido",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DescontoNoFrete",
                table: "Pedido",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Cupom",
                columns: table => new
                {
                    CupomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Tipo = table.Column<byte>(type: "smallint", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    QuantidadeLeve = table.Column<short>(type: "smallint", nullable: true),
                    QuantidadePague = table.Column<short>(type: "smallint", nullable: true),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: false),
                    ValorMinimo = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    UmUsoPorCliente = table.Column<bool>(type: "boolean", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubcategoriaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cupom", x => x.CupomId);
                    table.ForeignKey(
                        name: "FK_Cupom_Categoria_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categoria",
                        principalColumn: "CategoriaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cupom_Subcategoria_SubcategoriaId",
                        column: x => x.SubcategoriaId,
                        principalTable: "Subcategoria",
                        principalColumn: "SubcategoriaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_CupomId",
                table: "Pedido",
                column: "CupomId");

            migrationBuilder.CreateIndex(
                name: "IX_Cupom_CategoriaId",
                table: "Cupom",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cupom_Codigo",
                table: "Cupom",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cupom_SubcategoriaId",
                table: "Cupom",
                column: "SubcategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Cupom_CupomId",
                table: "Pedido",
                column: "CupomId",
                principalTable: "Cupom",
                principalColumn: "CupomId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Cupom_CupomId",
                table: "Pedido");

            migrationBuilder.DropTable(
                name: "Cupom");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_CupomId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "CodigoDoCupom",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "CupomId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "DescontoDoCupom",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "DescontoNoFrete",
                table: "Pedido");
        }
    }
}
