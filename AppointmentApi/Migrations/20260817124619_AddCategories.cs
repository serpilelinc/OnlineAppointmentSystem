using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "ServiceTypes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypes_CategoryId",
                table: "ServiceTypes",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTypes_Category_CategoryId",
                table: "ServiceTypes",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTypes_Category_CategoryId",
                table: "ServiceTypes");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTypes_CategoryId",
                table: "ServiceTypes");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "ServiceTypes");
        }
    }
}
