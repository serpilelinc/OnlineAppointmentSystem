using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentApi.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceTypeToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceTypeId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceTypeId",
                table: "Appointments",
                column: "ServiceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ServiceTypes_ServiceTypeId",
                table: "Appointments",
                column: "ServiceTypeId",
                principalTable: "ServiceTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_ServiceTypes_ServiceTypeId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ServiceTypeId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ServiceTypeId",
                table: "Appointments");
        }
    }
}
