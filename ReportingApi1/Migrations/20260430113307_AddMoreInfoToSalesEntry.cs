using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportingApi1.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreInfoToSalesEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Country",
                table: "SalesEntries",
                newName: "BuyerCountry");

            migrationBuilder.RenameIndex(
                name: "IX_SalesEntries_VatReportId_Country",
                table: "SalesEntries",
                newName: "IX_SalesEntries_VatReportId_BuyerCountry");

            migrationBuilder.AddColumn<bool>(
                name: "BuyerHasValidVatNumber",
                table: "SalesEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BuyerType",
                table: "SalesEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Currency",
                table: "SalesEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SaleDate",
                table: "SalesEntries",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "SellerCountry",
                table: "SalesEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "productCategory",
                table: "SalesEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerHasValidVatNumber",
                table: "SalesEntries");

            migrationBuilder.DropColumn(
                name: "BuyerType",
                table: "SalesEntries");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "SalesEntries");

            migrationBuilder.DropColumn(
                name: "SaleDate",
                table: "SalesEntries");

            migrationBuilder.DropColumn(
                name: "SellerCountry",
                table: "SalesEntries");

            migrationBuilder.DropColumn(
                name: "productCategory",
                table: "SalesEntries");

            migrationBuilder.RenameColumn(
                name: "BuyerCountry",
                table: "SalesEntries",
                newName: "Country");

            migrationBuilder.RenameIndex(
                name: "IX_SalesEntries_VatReportId_BuyerCountry",
                table: "SalesEntries",
                newName: "IX_SalesEntries_VatReportId_Country");
        }
    }
}
