using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FordEnterRPG.Migrations
{
    /// <inheritdoc />
    public partial class AddRunSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RunSequence",
                table: "Battles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RunSequence",
                table: "Battles");
        }
    }
}
