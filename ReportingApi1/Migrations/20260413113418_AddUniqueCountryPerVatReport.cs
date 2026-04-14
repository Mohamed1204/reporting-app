using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportingApi1.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueCountryPerVatReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesEntries_VatReportId",
                table: "SalesEntries");

            migrationBuilder.CreateIndex(
                name: "IX_SalesEntries_VatReportId_Country",
                table: "SalesEntries",
                columns: new[] { "VatReportId", "Country" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesEntries_VatReportId_Country",
                table: "SalesEntries");

            migrationBuilder.CreateIndex(
                name: "IX_SalesEntries_VatReportId",
                table: "SalesEntries",
                column: "VatReportId");
        }
    }
}
