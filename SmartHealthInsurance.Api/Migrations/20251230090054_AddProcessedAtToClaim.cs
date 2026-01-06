using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHealthInsurance.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedAtToClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Policies_PolicyId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Treatments_Users_PolicyHolderId",
                table: "Treatments");

            migrationBuilder.DropIndex(
                name: "IX_Claims_PolicyId",
                table: "Claims");

            migrationBuilder.RenameColumn(
                name: "PolicyHolderId",
                table: "Treatments",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Treatments_PolicyHolderId",
                table: "Treatments",
                newName: "IX_Treatments_CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PolicyId",
                table: "Claims",
                column: "PolicyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Policies_PolicyId",
                table: "Claims",
                column: "PolicyId",
                principalTable: "Policies",
                principalColumn: "PolicyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Treatments_Users_CustomerId",
                table: "Treatments",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Policies_PolicyId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Treatments_Users_CustomerId",
                table: "Treatments");

            migrationBuilder.DropIndex(
                name: "IX_Claims_PolicyId",
                table: "Claims");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Treatments",
                newName: "PolicyHolderId");

            migrationBuilder.RenameIndex(
                name: "IX_Treatments_CustomerId",
                table: "Treatments",
                newName: "IX_Treatments_PolicyHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PolicyId",
                table: "Claims",
                column: "PolicyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Policies_PolicyId",
                table: "Claims",
                column: "PolicyId",
                principalTable: "Policies",
                principalColumn: "PolicyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Treatments_Users_PolicyHolderId",
                table: "Treatments",
                column: "PolicyHolderId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
