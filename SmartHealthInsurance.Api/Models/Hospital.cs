using System.ComponentModel.DataAnnotations;

namespace SmartHealthInsurance.Api.Models
{
    public class Hospital
    {
        [Key]
        public int HospitalId { get; set; }

        public string HospitalName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string ZipCode { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public bool IsNetworkProvider { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<TreatmentRecord>? TreatmentRecords { get; set; }
        public ICollection<Claim>? Claims { get; set; }
    }
}
