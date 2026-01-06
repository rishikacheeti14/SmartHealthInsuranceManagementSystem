namespace SmartHealthInsurance.Api.DTOs
{
    public class PolicyLookupDto
    {
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        
        public string PlanName { get; set; } = string.Empty;
        
        public string PatientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
