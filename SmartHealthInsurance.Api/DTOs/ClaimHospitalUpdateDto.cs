namespace SmartHealthInsurance.Api.DTOs
{
    public class ClaimHospitalUpdateDto
    {
        public string Diagnosis { get; set; } = null!;
        public decimal TreatmentCost { get; set; }
        public string TreatmentDetails { get; set; } = null!;
        public DateTime TreatmentDate { get; set; }
    }
}
