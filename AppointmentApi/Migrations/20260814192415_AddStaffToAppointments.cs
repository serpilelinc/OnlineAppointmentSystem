using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StaffId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StaffId",
                table: "Appointments",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Staffs_StaffId",
                table: "Appointments",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Staffs_StaffId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_StaffId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Appointments");
        }
    }
}
