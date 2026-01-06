namespace SmartHealthInsurance.Api.DTOs
{
    public class TreatmentResponseDto
    {
        public int TreatmentId { get; set; }
        public int PolicyId { get; set; }
        public int PolicyHolderId { get; set; }
        public string HospitalName { get; set; } = null!;
        public string Diagnosis { get; set; } = null!;
        public decimal TreatmentCost { get; set; }
        public DateTime TreatmentDate { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
