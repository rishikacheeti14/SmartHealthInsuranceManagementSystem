namespace SmartHealthInsurance.Api.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
        public int TotalClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalPremiumCollected { get; set; }
        public decimal TotalPayoutAmount { get; set; }
    }
}
