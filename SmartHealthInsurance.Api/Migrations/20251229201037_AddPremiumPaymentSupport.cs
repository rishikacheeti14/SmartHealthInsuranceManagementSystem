using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHealthInsurance.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPremiumPaymentSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PremiumPaid",
                table: "Policies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PremiumPaid",
                table: "Policies");
        }
    }
}
