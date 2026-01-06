using SmartHealthInsurance.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartHealthInsurance.Api.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public string PaymentReference { get; set; } = null!;

        public int PolicyId { get; set; }
        public int? ClaimId { get; set; }

        public decimal Amount { get; set; }

        public PaymentType PaymentType { get; set; }  
        public PaymentStatus Status { get; set; }

        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Policy? Policy { get; set; }
        public Claim? Claim { get; set; }
    }
}
