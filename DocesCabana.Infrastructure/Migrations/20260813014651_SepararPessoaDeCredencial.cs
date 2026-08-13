using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocesCabana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SepararPessoaDeCredencial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_Usuario_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_Usuario_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_Usuario_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_Usuario_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "Usuario");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Usuario");

            migrationBuilder.RenameColumn(
                name: "NomeCompleto",
                table: "Usuario",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Usuario",
                newName: "UsuarioId");

            migrationBuilder.AddColumn<string>(
                name: "Celular",
                table: "Usuario",
                type: "TEXT",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ContaDeAcesso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContaDeAcesso", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "ContaDeAcesso",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "ContaDeAcesso",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_ContaDeAcesso_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "ContaDeAcesso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_ContaDeAcesso_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "ContaDeAcesso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_ContaDeAcesso_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "ContaDeAcesso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_ContaDeAcesso_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "ContaDeAcesso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_ContaDeAcesso_UsuarioId",
                table: "Usuario",
                column: "UsuarioId",
                principalTable: "ContaDeAcesso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_ContaDeAcesso_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_ContaDeAcesso_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_ContaDeAcesso_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_ContaDeAcesso_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_ContaDeAcesso_UsuarioId",
                table: "Usuario");

            migrationBuilder.DropTable(
                name: "ContaDeAcesso");

            migrationBuilder.DropColumn(
                name: "Celular",
                table: "Usuario");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Usuario",
                newName: "NomeCompleto");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Usuario",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "Usuario",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Usuario",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Usuario",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Usuario",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "Usuario",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "Usuario",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Usuario",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                table: "Usuario",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Usuario",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Usuario",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "Usuario",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Usuario",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Usuario",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Usuario",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Usuario",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Usuario",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_Usuario_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_Usuario_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_Usuario_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_Usuario_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
