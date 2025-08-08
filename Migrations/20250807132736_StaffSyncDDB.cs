using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class StaffSyncDDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$1X7gZ8x9y2z3w4v5u6t7r8s9t0u1v2w3x4y5z6A7B8C9D0E1F2G3" });

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0U1V2W3X4Y5Z6" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2025, 8, 7, 21, 4, 25, 993, DateTimeKind.Local).AddTicks(1620), "$2a$11$vEcaO.PHNXTomveshwAUe.HMmUt5qBhrT44Au9LdqPTf1cZnkR2eC" });

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2025, 8, 7, 21, 4, 25, 993, DateTimeKind.Local).AddTicks(1935), "$2a$11$j5jB2WyzHd4jSvQlEQuv/ufeJrsKxuHMHV8fbIQq7Bivfa1xTk3/G" });
        }
    }
}
