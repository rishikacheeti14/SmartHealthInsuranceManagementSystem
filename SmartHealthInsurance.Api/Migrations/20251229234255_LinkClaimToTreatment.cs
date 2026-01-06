using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHealthInsurance.Api.Migrations
{
    /// <inheritdoc />
    public partial class LinkClaimToTreatment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TreatmentId",
                table: "Claims",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_TreatmentId",
                table: "Claims",
                column: "TreatmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Treatments_TreatmentId",
                table: "Claims",
                column: "TreatmentId",
                principalTable: "Treatments",
                principalColumn: "TreatmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Treatments_TreatmentId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_TreatmentId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "TreatmentId",
                table: "Claims");
        }
    }
}
