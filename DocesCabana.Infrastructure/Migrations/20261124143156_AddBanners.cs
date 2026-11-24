using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBanners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banner",
                columns: table => new
                {
                    BannerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImagemUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TextoAlternativo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubcategoriaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProdutoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banner", x => x.BannerId);
                    table.CheckConstraint("CK_Banner_UmDestino", "((\"CategoriaId\" IS NOT NULL)::int + (\"SubcategoriaId\" IS NOT NULL)::int + (\"ProdutoId\" IS NOT NULL)::int) = 1");
                    table.ForeignKey(
                        name: "FK_Banner_Categoria_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categoria",
                        principalColumn: "CategoriaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Banner_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "ProdutoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Banner_Subcategoria_SubcategoriaId",
                        column: x => x.SubcategoriaId,
                        principalTable: "Subcategoria",
                        principalColumn: "SubcategoriaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Banner_Ativo_DataInicio",
                table: "Banner",
                columns: new[] { "Ativo", "DataInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Banner_CategoriaId",
                table: "Banner",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Banner_ProdutoId",
                table: "Banner",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Banner_SubcategoriaId",
                table: "Banner",
                column: "SubcategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Banner");
        }
    }
}
