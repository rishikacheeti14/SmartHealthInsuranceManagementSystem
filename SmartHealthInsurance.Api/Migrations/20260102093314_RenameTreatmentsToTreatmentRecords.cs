using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHealthInsurance.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameTreatmentsToTreatmentRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Treatments_TreatmentId",
                table: "Claims");

            migrationBuilder.RenameTable(
                name: "Treatments",
                newName: "TreatmentRecords");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_TreatmentRecords_TreatmentId",
                table: "Claims",
                column: "TreatmentId",
                principalTable: "TreatmentRecords",
                principalColumn: "TreatmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_TreatmentRecords_TreatmentId",
                table: "Claims");

            migrationBuilder.RenameTable(
                name: "TreatmentRecords",
                newName: "Treatments");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Treatments_TreatmentId",
                table: "Claims",
                column: "TreatmentId",
                principalTable: "Treatments",
                principalColumn: "TreatmentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
