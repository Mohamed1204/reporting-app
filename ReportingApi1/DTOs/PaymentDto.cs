using System.Text.Json.Serialization;
using ReportingApi1.Entities;

namespace ReportingApi1.DTOs
{
    public class PaymentDto
    {
    }
    public class CreatePaymentResponseDto
    {
        public int PaymentId { get; set; }
        public decimal AmountDue { get; set; }
        public decimal TotalPaid { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatus PaymentStatus { get; set; }
    }
    public class CreatePaymentDto
    {
        public decimal Amount { get; set; }
        public string ExternalReferenceNumber { get; set; } = string.Empty;
    }
}
