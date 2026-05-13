using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FordEnterRPG.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterIsDead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDead",
                table: "Characters",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDead",
                table: "Characters");
        }
    }
}
