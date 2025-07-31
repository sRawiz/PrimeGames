using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrimeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddViewCountToContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Contents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Contents");
        }
    }
}
