using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class StaffSyncCDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeId1",
                table: "Payslips",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimePay",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPay",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "CreatedDate", "Email", "IsActive", "Password", "Role", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 7, 21, 4, 25, 993, DateTimeKind.Local).AddTicks(1620), "superadmin@example.com", true, "$2a$11$vEcaO.PHNXTomveshwAUe.HMmUt5qBhrT44Au9LdqPTf1cZnkR2eC", "Super Admin", "superadmin" },
                    { 2, new DateTime(2025, 8, 7, 21, 4, 25, 993, DateTimeKind.Local).AddTicks(1935), "admin@example.com", true, "$2a$11$j5jB2WyzHd4jSvQlEQuv/ufeJrsKxuHMHV8fbIQq7Bivfa1xTk3/G", "Admin", "admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_EmployeeId1",
                table: "Payslips",
                column: "EmployeeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Payslips_Employees_EmployeeId1",
                table: "Payslips",
                column: "EmployeeId1",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payslips_Employees_EmployeeId1",
                table: "Payslips");

            migrationBuilder.DropIndex(
                name: "IX_Payslips_EmployeeId1",
                table: "Payslips");

            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "OvertimePay",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "TotalPay",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Employees");
        }
    }
}
