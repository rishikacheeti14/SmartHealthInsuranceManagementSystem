namespace SmartHealthInsurance.Api.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalPolicies { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveClaims { get; set; }
    }

    public class RevenueTrendDto
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
    }
}
