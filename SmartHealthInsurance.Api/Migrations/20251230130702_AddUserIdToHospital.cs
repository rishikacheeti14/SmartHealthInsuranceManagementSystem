using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHealthInsurance.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToHospital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ Add column as NULLABLE first
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Hospitals",
                nullable: true);

            // 2️⃣ Create index
            migrationBuilder.CreateIndex(
                name: "IX_Hospitals_UserId",
                table: "Hospitals",
                column: "UserId");

            // 3️⃣ Add FK WITHOUT cascade
            migrationBuilder.AddForeignKey(
                name: "FK_Hospitals_Users_UserId",
                table: "Hospitals",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_Hospitals_Users_UserId", "Hospitals");
            migrationBuilder.DropIndex("IX_Hospitals_UserId", "Hospitals");
            migrationBuilder.DropColumn("UserId", "Hospitals");
        }
    }
    }
