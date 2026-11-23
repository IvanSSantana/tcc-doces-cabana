using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnificarPreparoEAlmocoNaRetirada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuracaoDaFaixaEmMinutos",
                table: "AgendaDeRetirada");

            migrationBuilder.RenameColumn(
                name: "PreparoEmMinutos",
                table: "AgendaDeRetirada",
                newName: "TempoDePreparoEmMinutos");

            // O preparo agora também é a duração da faixa: precisa ficar entre 15 e 240 minutos.
            migrationBuilder.Sql(
                "UPDATE \"AgendaDeRetirada\" SET \"TempoDePreparoEmMinutos\" = LEAST(GREATEST(\"TempoDePreparoEmMinutos\", 15), 240);");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "FimDoAlmoco",
                table: "AgendaDeRetirada",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "InicioDoAlmoco",
                table: "AgendaDeRetirada",
                type: "time without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FimDoAlmoco",
                table: "AgendaDeRetirada");

            migrationBuilder.DropColumn(
                name: "InicioDoAlmoco",
                table: "AgendaDeRetirada");

            migrationBuilder.RenameColumn(
                name: "TempoDePreparoEmMinutos",
                table: "AgendaDeRetirada",
                newName: "PreparoEmMinutos");

            migrationBuilder.AddColumn<int>(
                name: "DuracaoDaFaixaEmMinutos",
                table: "AgendaDeRetirada",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE \"AgendaDeRetirada\" SET \"DuracaoDaFaixaEmMinutos\" = \"PreparoEmMinutos\";");
        }
    }
}
