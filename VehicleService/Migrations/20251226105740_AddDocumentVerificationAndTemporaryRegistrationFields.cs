using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleService.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentVerificationAndTemporaryRegistrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CustomsApprovalDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomsApprovalNumber",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomsOffice",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HasCustomsDutiesProof",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasInsuranceProof",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasOwnerIdentityProof",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasOwnershipProof",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPaymentConfirmation",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasTechnicalValidityCertificate",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasTemporaryImportApproval",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRegisteredInNationalRegistry",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NationalRegistryDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnershipProofType",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TechnicalValidityCertificateDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemporaryUseReason",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomsApprovalDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CustomsApprovalNumber",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CustomsOffice",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasCustomsDutiesProof",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasInsuranceProof",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasOwnerIdentityProof",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasOwnershipProof",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasPaymentConfirmation",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasTechnicalValidityCertificate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasTemporaryImportApproval",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "IsRegisteredInNationalRegistry",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NationalRegistryDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "OwnershipProofType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TechnicalValidityCertificateDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TemporaryUseReason",
                table: "Vehicles");
        }
    }
}
