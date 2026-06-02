using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportingApi1.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxBreakdownToSalesEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Breakdown_VatRate",
                table: "SalesEntries",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Breakdown_Scheme",
                table: "SalesEntries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Breakdown_VatAmount",
                table: "SalesEntries",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            // Backfill the new owned TaxBreakdown columns from the existing per-entry VatRate
            // before dropping it, so historical reports keep their stored VAT.
            migrationBuilder.Sql(@"
                UPDATE [SalesEntries]
                SET [Breakdown_VatRate] = [VatRate],
                    [Breakdown_VatAmount] = ROUND([Amount] * [VatRate] / 100, 2),
                    [Breakdown_Scheme] = 'Oss';");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "SalesEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Breakdown_Scheme",
                table: "SalesEntries");

            migrationBuilder.DropColumn(
                name: "Breakdown_VatAmount",
                table: "SalesEntries");

            migrationBuilder.DropColumn(
                name: "Breakdown_VatRate",
                table: "SalesEntries");

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "SalesEntries",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
