public class TreatmentCreateDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public DateTime TreatmentDate { get; set; }
    public string TreatmentDetails { get; set; } = string.Empty;
    public decimal TreatmentCost { get; set; }
}
