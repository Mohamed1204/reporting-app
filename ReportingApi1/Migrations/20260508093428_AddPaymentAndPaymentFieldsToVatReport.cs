using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportingApi1.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAndPaymentFieldsToVatReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountDue",
                table: "VatReports",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "VatReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "VatReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SettlementCurrency",
                table: "VatReports",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VatReportId = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExternalReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_VatReports_VatReportId",
                        column: x => x.VatReportId,
                        principalTable: "VatReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_VatReportId_ExternalReference",
                table: "Payments",
                columns: new[] { "VatReportId", "ExternalReference" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropColumn(
                name: "AmountDue",
                table: "VatReports");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "VatReports");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "VatReports");

            migrationBuilder.DropColumn(
                name: "SettlementCurrency",
                table: "VatReports");
        }
    }
}
