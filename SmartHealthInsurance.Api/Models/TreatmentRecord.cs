using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHealthInsurance.Api.Models
{
    public class TreatmentRecord
    {
        [Key]
        public int TreatmentId { get; set; }

        [Required]
        public int HospitalId { get; set; }

        public int? PolicyId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public DateTime TreatmentDate { get; set; }

        [Required]
        public string Diagnosis { get; set; } = null!;

        [Required]
        public string TreatmentDetails { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TreatmentCost { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public Hospital Hospital { get; set; } = null!;
        public Policy? Policy { get; set; }
        public User Customer { get; set; } = null!;
    }
}
