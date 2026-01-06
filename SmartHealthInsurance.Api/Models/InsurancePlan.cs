using System.ComponentModel.DataAnnotations;

namespace SmartHealthInsurance.Api.Models
{
    public class InsurancePlan
    {
        [Key]
        public int PlanId { get; set; }

        public string PlanName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal PremiumAmount { get; set; }
        public decimal CoverageLimit { get; set; }
        public int DurationInMonths { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Policy>? Policies { get; set; }
    }
}
