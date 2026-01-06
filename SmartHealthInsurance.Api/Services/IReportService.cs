using SmartHealthInsurance.Api.DTOs;

namespace SmartHealthInsurance.Api.Services
{
    public interface IReportService
    {
        Task<IEnumerable<PolicyDistributionDto>> GetPolicyDistributionAsync();
        Task<IEnumerable<ClaimAnalyticsDto>> GetClaimsAnalyticsAsync();
        Task<PremiumVsPayoutDto> GetPremiumVsPayoutAsync(int? userId);
        Task<IEnumerable<HighValueClaimDto>> GetHighValueClaimsAsync(decimal? threshold = null);
        
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<IEnumerable<RevenueTrendDto>> GetRevenueTrendAsync();
    }
}
