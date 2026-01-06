using SmartHealthInsurance.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartHealthInsurance.Api.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        public string ClaimNumber { get; set; } = null!;
        public string? TreatmentDescription { get; set; } 

        public int? PolicyId { get; set; }
        public int UserId { get; set; }           
        public int? HospitalId { get; set; }
        public int? TreatmentId { get; set; }

        public decimal? ClaimAmount { get; set; }  
        public decimal ApprovedAmount { get; set; } 

        public DateTime? TreatmentDate { get; set; }
        public string? TreatmentDetails { get; set; }

        public ClaimStatus Status { get; set; }

        public int? ReviewedBy { get; set; }  
        public string? RejectionReason { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public Policy? Policy { get; set; }
        public User? User { get; set; }
        public Hospital? Hospital { get; set; }
        public TreatmentRecord? TreatmentRecord { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}
