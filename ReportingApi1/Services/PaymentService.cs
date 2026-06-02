using Microsoft.EntityFrameworkCore;
using ReportingApi1.Data;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;
using ReportingApi1.Exceptions;

namespace ReportingApi1.Services
{
    public interface IPaymentService
    {
        public Task<CreatePaymentResponseDto> CreatePaymentAsync(int reportId, CreatePaymentDto payment);
    }
    public class PaymentService : IPaymentService
    {
        private readonly VatReportingContext _context;

        public PaymentService(VatReportingContext context)
        {
            _context = context;
        }
        public async Task<CreatePaymentResponseDto> CreatePaymentAsync(int reportId, CreatePaymentDto dto)
        {
            var report = await _context.VatReports
                .Include(r => r.SalesEntries)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == reportId)
                ?? throw new NotFoundException($"VAT report {reportId} not found.");

            if (report.Payments.Any(p => p.ExternalReference == dto.ExternalReferenceNumber))
                throw new ConflictException(
                    $"A payment with reference '{dto.ExternalReferenceNumber}' already exists for this report.");

            // Capture the prior total before Add() so EF relationship fix-up can't double-count.
            var totalPaid = report.Payments.Sum(p => p.Amount) + dto.Amount;

            var payment = new Payment
            {
                Amount = dto.Amount,
                ExternalReference = dto.ExternalReferenceNumber,
                VatReportId = reportId,
                Currency = report.SettlementCurrency ?? "EUR"
            };
            _context.Payments.Add(payment);

            // Reconcile the report's payment state. AmountDue falls back to the report's total VAT
            // owed when it hasn't been set explicitly.
            var amountDue = report.AmountDue
                ?? report.SalesEntries.Sum(se => se.Breakdown != null ? se.Breakdown.VatAmount : 0m);
            report.AmountDue = amountDue;
            report.PaymentStatus = totalPaid <= 0m ? PaymentStatus.Unpaid
                : totalPaid < amountDue ? PaymentStatus.PartiallyPaid
                : totalPaid == amountDue ? PaymentStatus.Paid
                : PaymentStatus.Overpaid;

            await _context.SaveChangesAsync();

            return new CreatePaymentResponseDto
            {
                PaymentId = payment.Id,
                AmountDue = amountDue,
                TotalPaid = totalPaid,
                PaymentStatus = report.PaymentStatus
            };
        }
    }
}
