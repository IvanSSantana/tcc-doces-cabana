using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPickupScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Transportadora",
                table: "Pedido",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Servico",
                table: "Pedido",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "PrazoMinimoEmDias",
                table: "Pedido",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "PrazoMaximoEmDias",
                table: "Pedido",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "EnderecoEntregaId",
                table: "Pedido",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataDaRetirada",
                table: "Pedido",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "FimDaRetirada",
                table: "Pedido",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "InicioDaRetirada",
                table: "Pedido",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Modalidade",
                table: "Pedido",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "AgendaDeRetirada",
                columns: table => new
                {
                    AgendaDeRetiradaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DuracaoDaFaixaEmMinutos = table.Column<int>(type: "integer", nullable: false),
                    PreparoEmMinutos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaDeRetirada", x => x.AgendaDeRetiradaId);
                });

            migrationBuilder.CreateTable(
                name: "DiaSemRetirada",
                columns: table => new
                {
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    AgendaDeRetiradaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaSemRetirada", x => new { x.AgendaDeRetiradaId, x.Data });
                    table.ForeignKey(
                        name: "FK_DiaSemRetirada_AgendaDeRetirada_AgendaDeRetiradaId",
                        column: x => x.AgendaDeRetiradaId,
                        principalTable: "AgendaDeRetirada",
                        principalColumn: "AgendaDeRetiradaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorarioDeRetirada",
                columns: table => new
                {
                    DiaDaSemana = table.Column<int>(type: "integer", nullable: false),
                    AgendaDeRetiradaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Abertura = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Fechamento = table.Column<TimeOnly>(type: "time without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorarioDeRetirada", x => new { x.AgendaDeRetiradaId, x.DiaDaSemana });
                    table.ForeignKey(
                        name: "FK_HorarioDeRetirada_AgendaDeRetirada_AgendaDeRetiradaId",
                        column: x => x.AgendaDeRetiradaId,
                        principalTable: "AgendaDeRetirada",
                        principalColumn: "AgendaDeRetiradaId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiaSemRetirada");

            migrationBuilder.DropTable(
                name: "HorarioDeRetirada");

            migrationBuilder.DropTable(
                name: "AgendaDeRetirada");

            migrationBuilder.DropColumn(
                name: "DataDaRetirada",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "FimDaRetirada",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "InicioDaRetirada",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Modalidade",
                table: "Pedido");

            migrationBuilder.AlterColumn<string>(
                name: "Transportadora",
                table: "Pedido",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Servico",
                table: "Pedido",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PrazoMinimoEmDias",
                table: "Pedido",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PrazoMaximoEmDias",
                table: "Pedido",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EnderecoEntregaId",
                table: "Pedido",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
