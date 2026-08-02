using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuCita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentProfessionalStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_appointments_professional_id_Status_Date_start_time",
                table: "appointments",
                columns: new[] { "professional_id", "Status", "Date", "start_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_appointments_professional_id_Status_Date_start_time",
                table: "appointments");
        }
    }
}
