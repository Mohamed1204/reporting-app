using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportingApi1.Migrations
{
    /// <inheritdoc />
    public partial class AddReplacedByTokenId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReplacedByTokenId",
                table: "RefreshTokens",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReplacedByTokenId",
                table: "RefreshTokens");
        }
    }
}
