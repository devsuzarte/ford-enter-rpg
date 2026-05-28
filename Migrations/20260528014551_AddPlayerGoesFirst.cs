using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FordEnterRPG.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerGoesFirst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PlayerGoesFirst",
                table: "Battles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerGoesFirst",
                table: "Battles");
        }
    }
}
