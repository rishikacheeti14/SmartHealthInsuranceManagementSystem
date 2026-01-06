using SmartHealthInsurance.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartHealthInsurance.Api.Models
{
    public class Policy
    {
        [Key]
        public int PolicyId { get; set; }

        public string PolicyNumber { get; set; } = null!;

        public int UserId { get; set; }       
        public int? AgentId { get; set; }    
        public int PlanId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public PolicyStatus Status { get; set; }

        public bool PremiumPaid { get; set; } = false;

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal PremiumAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public User? User { get; set; }
        public InsurancePlan? Plan { get; set; }
        public ICollection<Claim>? Claims { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}
