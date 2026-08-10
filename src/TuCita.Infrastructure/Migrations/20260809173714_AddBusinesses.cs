using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuCita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinesses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "business_id",
                table: "professionals",
                type: "character varying(26)",
                maxLength: 26,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "business_invites",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    business_id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_business_invites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "businesses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    email_verification_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    email_verification_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_businesses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_professionals_business_id",
                table: "professionals",
                column: "business_id");

            migrationBuilder.CreateIndex(
                name: "IX_business_invites_business_id",
                table: "business_invites",
                column: "business_id");

            migrationBuilder.CreateIndex(
                name: "IX_business_invites_token",
                table: "business_invites",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_businesses_email",
                table: "businesses",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_businesses_email_verification_token",
                table: "businesses",
                column: "email_verification_token");

            migrationBuilder.CreateIndex(
                name: "IX_businesses_slug",
                table: "businesses",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "business_invites");

            migrationBuilder.DropTable(
                name: "businesses");

            migrationBuilder.DropIndex(
                name: "IX_professionals_business_id",
                table: "professionals");

            migrationBuilder.DropColumn(
                name: "business_id",
                table: "professionals");
        }
    }
}
