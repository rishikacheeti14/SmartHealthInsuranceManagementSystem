using System.ComponentModel.DataAnnotations;

namespace SmartHealthInsurance.Api.DTOs
{
    public class PolicyCreateDto
    {
        
        [Required]
        public string FirstName { get; set; } = null!;
        
        [Required]
        public string LastName { get; set; } = null!;
        
        [Required, EmailAddress]
        public string Email { get; set; } = null!;
        
        [Required]
        public DateTime DateOfBirth { get; set; }

        public int PlanId { get; set; }
        public DateTime StartDate { get; set; }
    }
}
