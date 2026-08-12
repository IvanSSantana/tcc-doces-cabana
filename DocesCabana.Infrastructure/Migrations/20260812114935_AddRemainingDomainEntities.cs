using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingDomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Avaliacao",
                columns: table => new
                {
                    AvaliacaoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Nota = table.Column<byte>(type: "INTEGER", nullable: false),
                    UpVote = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacao", x => x.AvaliacaoId);
                    table.ForeignKey(
                        name: "FK_Avaliacao_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "ProdutoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Avaliacao_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    CategoriaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.CategoriaId);
                });

            migrationBuilder.CreateTable(
                name: "Endereco",
                columns: table => new
                {
                    EnderecoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CEP = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Rua = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    Complemento = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Endereco", x => x.EnderecoId);
                    table.ForeignKey(
                        name: "FK_Endereco_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Estoque",
                columns: table => new
                {
                    ProdutoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estoque", x => x.ProdutoId);
                    table.ForeignKey(
                        name: "FK_Estoque_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "ProdutoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Favorito",
                columns: table => new
                {
                    ProdutoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorito", x => new { x.ProdutoId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_Favorito_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "ProdutoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Favorito_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Promocao",
                columns: table => new
                {
                    PromocaoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Tipo = table.Column<byte>(type: "INTEGER", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promocao", x => x.PromocaoId);
                });

            migrationBuilder.CreateTable(
                name: "Subcategoria",
                columns: table => new
                {
                    SubcategoriaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subcategoria", x => x.SubcategoriaId);
                    table.ForeignKey(
                        name: "FK_Subcategoria_Categoria_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categoria",
                        principalColumn: "CategoriaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pedido",
                columns: table => new
                {
                    PedidoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EnderecoEntregaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PagamentoAprovado = table.Column<bool>(type: "INTEGER", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<byte>(type: "INTEGER", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedido", x => x.PedidoId);
                    table.ForeignKey(
                        name: "FK_Pedido_Endereco_EnderecoEntregaId",
                        column: x => x.EnderecoEntregaId,
                        principalTable: "Endereco",
                        principalColumn: "EnderecoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedido_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemPedido",
                columns: table => new
                {
                    ItemPedidoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PedidoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<short>(type: "INTEGER", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPedido", x => x.ItemPedidoId);
                    table.ForeignKey(
                        name: "FK_ItemPedido_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedido",
                        principalColumn: "PedidoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemPedido_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "ProdutoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagamento",
                columns: table => new
                {
                    PagamentoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PedidoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Metodo = table.Column<byte>(type: "INTEGER", nullable: false),
                    Status = table.Column<byte>(type: "INTEGER", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamento", x => x.PagamentoId);
                    table.ForeignKey(
                        name: "FK_Pagamento_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedido",
                        principalColumn: "PedidoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Produto_PromocaoId",
                table: "Produto",
                column: "PromocaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_SubcategoriaId",
                table: "Produto",
                column: "SubcategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacao_ProdutoId",
                table: "Avaliacao",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacao_UsuarioId",
                table: "Avaliacao",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Endereco_UsuarioId",
                table: "Endereco",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorito_UsuarioId",
                table: "Favorito",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPedido_PedidoId",
                table: "ItemPedido",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPedido_ProdutoId",
                table: "ItemPedido",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_PedidoId",
                table: "Pagamento",
                column: "PedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_EnderecoEntregaId",
                table: "Pedido",
                column: "EnderecoEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_UsuarioId",
                table: "Pedido",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Subcategoria_CategoriaId",
                table: "Subcategoria",
                column: "CategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_Promocao_PromocaoId",
                table: "Produto",
                column: "PromocaoId",
                principalTable: "Promocao",
                principalColumn: "PromocaoId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_Subcategoria_SubcategoriaId",
                table: "Produto",
                column: "SubcategoriaId",
                principalTable: "Subcategoria",
                principalColumn: "SubcategoriaId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_Promocao_PromocaoId",
                table: "Produto");

            migrationBuilder.DropForeignKey(
                name: "FK_Produto_Subcategoria_SubcategoriaId",
                table: "Produto");

            migrationBuilder.DropTable(
                name: "Avaliacao");

            migrationBuilder.DropTable(
                name: "Estoque");

            migrationBuilder.DropTable(
                name: "Favorito");

            migrationBuilder.DropTable(
                name: "ItemPedido");

            migrationBuilder.DropTable(
                name: "Pagamento");

            migrationBuilder.DropTable(
                name: "Promocao");

            migrationBuilder.DropTable(
                name: "Subcategoria");

            migrationBuilder.DropTable(
                name: "Pedido");

            migrationBuilder.DropTable(
                name: "Categoria");

            migrationBuilder.DropTable(
                name: "Endereco");

            migrationBuilder.DropIndex(
                name: "IX_Produto_PromocaoId",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Produto_SubcategoriaId",
                table: "Produto");
        }
    }
}
