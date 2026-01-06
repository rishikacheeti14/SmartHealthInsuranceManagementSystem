using SmartHealthInsurance.Api.Enums;

namespace SmartHealthInsurance.Api.DTOs
{
    public class PolicyDistributionDto
    {
        public string PolicyType { get; set; } = string.Empty; 
        public PolicyStatus Status { get; set; }
        public int Count { get; set; }
    }

    public class ClaimAnalyticsDto
    {
        public ClaimStatus Status { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalApprovedAmount { get; set; }
        public int Count { get; set; }
    }

    public class PremiumVsPayoutDto
    {
        public decimal TotalPremiumsCollected { get; set; }
        public decimal TotalClaimsPaid { get; set; }
        public decimal TotalCoverageAmount { get; set; }
        public decimal RemainingCoverage => TotalCoverageAmount - TotalClaimsPaid;
        public List<PlanFinancialDto> PlanBreakdown { get; set; } = new List<PlanFinancialDto>();
    }

    public class PlanFinancialDto
    {
        public string PlanName { get; set; } = string.Empty;
        public decimal TotalPremium { get; set; }
        public decimal TotalPayout { get; set; }
    }

    public class HighValueClaimDto
    {
        public int ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal CoverageAmount { get; set; }
        public string ValueType { get; set; } = "Requested"; 
    }
    public class PagedResultDto<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
    }
}
