using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrimeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveThreeColumnFromContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DealLink",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "DealPrice",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "ReviewScore",
                table: "Contents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DealLink",
                table: "Contents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DealPrice",
                table: "Contents",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewScore",
                table: "Contents",
                type: "int",
                nullable: true);
        }
    }
}
