using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHealthInsurance.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPremiumAmountToPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PremiumAmount",
                table: "Policies",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PremiumAmount",
                table: "Policies");
        }
    }
}
