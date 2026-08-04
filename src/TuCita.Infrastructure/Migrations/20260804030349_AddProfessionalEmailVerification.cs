using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuCita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionalEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email_verification_token",
                table: "professionals",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "email_verification_token_expires_at",
                table: "professionals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "email_verified",
                table: "professionals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Los profesionales que ya existían antes de esta migración se
            // registraron sin flujo de verificación; no deben quedar
            // bloqueados del login retroactivamente.
            migrationBuilder.Sql("UPDATE professionals SET email_verified = true;");

            migrationBuilder.CreateIndex(
                name: "IX_professionals_email_verification_token",
                table: "professionals",
                column: "email_verification_token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_professionals_email_verification_token",
                table: "professionals");

            migrationBuilder.DropColumn(
                name: "email_verification_token",
                table: "professionals");

            migrationBuilder.DropColumn(
                name: "email_verification_token_expires_at",
                table: "professionals");

            migrationBuilder.DropColumn(
                name: "email_verified",
                table: "professionals");
        }
    }
}
