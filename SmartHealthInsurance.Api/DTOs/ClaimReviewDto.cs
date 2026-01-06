namespace SmartHealthInsurance.Api.DTOs
{
    public class ClaimReviewDto
    {
        public bool IsApproved { get; set; }
        public decimal ApprovedAmount { get; set; }
        public string? RejectionReason { get; set; }
    }
}
