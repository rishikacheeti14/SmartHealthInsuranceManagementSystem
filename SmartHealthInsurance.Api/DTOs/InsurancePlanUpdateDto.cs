namespace SmartHealthInsurance.Api.DTOs
{
    public class InsurancePlanUpdateDto
    {
        public string PlanName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CoverageLimit { get; set; }
        public int DurationInMonths { get; set; }
        public bool IsActive { get; set; }
    }
}
