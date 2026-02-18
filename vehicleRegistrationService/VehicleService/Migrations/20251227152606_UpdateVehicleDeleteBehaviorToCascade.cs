using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehicleDeleteBehaviorToCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationRequests_Vehicles_VehicleId",
                table: "RegistrationRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationRequests_Vehicles_VehicleId",
                table: "RegistrationRequests",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationRequests_Vehicles_VehicleId",
                table: "RegistrationRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationRequests_Vehicles_VehicleId",
                table: "RegistrationRequests",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
