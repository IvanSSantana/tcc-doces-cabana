using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPedidoDadosDeEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrazoMaximoEmDias",
                table: "Pedido",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PrazoMinimoEmDias",
                table: "Pedido",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Servico",
                table: "Pedido",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Transportadora",
                table: "Pedido",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ValorDoFrete",
                table: "Pedido",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrazoMaximoEmDias",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "PrazoMinimoEmDias",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Servico",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Transportadora",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "ValorDoFrete",
                table: "Pedido");
        }
    }
}
