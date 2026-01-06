using SmartHealthInsurance.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartHealthInsurance.Api.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public UserRole Role { get; set; }  
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Policy>? Policies { get; set; }  
        public ICollection<Claim>? Claims { get; set; }   
        public ICollection<Notification>? Notifications { get; set; }
    }
}
