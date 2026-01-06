namespace SmartHealthInsurance.Api.DTOs
{
    public class PremiumPaymentDto
    {
        public int PolicyId { get; set; }
        public decimal Amount { get; set; } 
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
