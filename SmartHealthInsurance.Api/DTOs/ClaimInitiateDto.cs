namespace SmartHealthInsurance.Api.DTOs
{
    public class ClaimInitiateDto
    {
        public int PolicyId { get; set; }
        public int HospitalId { get; set; }
        public string TreatmentDescription { get; set; } = null!;
    }
}
