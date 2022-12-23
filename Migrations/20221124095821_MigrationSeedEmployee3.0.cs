using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagementApp.Migrations
{
    public partial class MigrationSeedEmployee30 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Department", "Email", "Name" },
                values: new object[] { 4, 0, "henry@micro.com", "Henry" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
