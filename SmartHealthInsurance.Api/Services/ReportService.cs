using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Repositories;

namespace SmartHealthInsurance.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;

        public ReportService(
            IPolicyRepository policyRepository,
            IClaimRepository claimRepository,
            IPaymentRepository paymentRepository,
            IUserRepository userRepository)
        {
            _policyRepository = policyRepository;
            _claimRepository = claimRepository;
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var userCount = await _userRepository.CountAsync(); 
            var policyCount = await _policyRepository.CountAsync();
            var totalRevenue = await _paymentRepository.GetTotalPremiumsAsync(null);
            var activeClaims = await _claimRepository.CountAsync(c => c.Status != Api.Enums.ClaimStatus.Paid && c.Status != Api.Enums.ClaimStatus.Rejected);

            return new DashboardStatsDto
            {
                TotalUsers = userCount,
                TotalPolicies = policyCount,
                TotalRevenue = totalRevenue,
                ActiveClaims = activeClaims
            };
        }

        public async Task<IEnumerable<RevenueTrendDto>> GetRevenueTrendAsync()
        { 
            var payments = await _paymentRepository.GetAllAsync();
            
            var trend = payments
                .Where(p => p.PaymentType == Api.Enums.PaymentType.Premium && p.Status == Api.Enums.PaymentStatus.Completed)
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new RevenueTrendDto
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Revenue = g.Sum(p => p.Amount)
                })
                .OrderBy(x => DateTime.Parse(x.Month))
                .TakeLast(6) 
                .ToList();

            return trend;
        }

        public async Task<IEnumerable<PolicyDistributionDto>> GetPolicyDistributionAsync()
        {
            return await _policyRepository.GetPolicyDistributionReportAsync();
        }

        public async Task<IEnumerable<ClaimAnalyticsDto>> GetClaimsAnalyticsAsync()
        {
            return await _claimRepository.GetClaimsAnalyticsReportAsync();
        }

        public async Task<PremiumVsPayoutDto> GetPremiumVsPayoutAsync(int? userId)
        {
            var premiums = await _paymentRepository.GetTotalPremiumsAsync(userId);
            var payouts = await _claimRepository.GetTotalPayoutsAsync(userId);
            
            decimal totalCoverage = 0;
            if (userId.HasValue)
            {
                var policies = await _policyRepository.GetByUserIdAsync(userId.Value);
                totalCoverage = policies
                    .Where(p => p.Status == Api.Enums.PolicyStatus.Active && p.Plan != null)
                    .Sum(p => p.Plan!.CoverageLimit);
            }
            else
            {
                var allPolicies = await _policyRepository.GetAllAsync();
                totalCoverage = allPolicies
                    .Where(p => p.Status == Api.Enums.PolicyStatus.Active && p.Plan != null)
                    .Sum(p => p.Plan!.CoverageLimit);
            }


            var allPayments = userId.HasValue ? await _paymentRepository.GetByUserIdAsync(userId.Value) : await _paymentRepository.GetAllAsync();
            var allClaims = userId.HasValue ? await _claimRepository.GetByUserIdAsync(userId.Value) : await _claimRepository.GetAllAsync();

            var paymentGroups = allPayments
                .Where(p => p.PaymentType == Api.Enums.PaymentType.Premium && p.Status == Api.Enums.PaymentStatus.Completed)
                .GroupBy(p => p.Policy?.Plan?.PlanName ?? "Unknown Plan")
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            var payoutGroups = allClaims
                .Where(c => c.Status == Api.Enums.ClaimStatus.Paid && c.ApprovedAmount > 0)
                .GroupBy(c => c.Policy?.Plan?.PlanName ?? "Unknown Plan")
                .ToDictionary(g => g.Key, g => g.Sum(c => c.ApprovedAmount));

            var allPlans = paymentGroups.Keys.Union(payoutGroups.Keys).ToList();
            var breakdown = new List<PlanFinancialDto>();

            foreach (var plan in allPlans)
            {
                breakdown.Add(new PlanFinancialDto
                {
                    PlanName = plan,
                    TotalPremium = paymentGroups.ContainsKey(plan) ? paymentGroups[plan] : 0,
                    TotalPayout = payoutGroups.ContainsKey(plan) ? payoutGroups[plan] : 0
                });
            }

            return new PremiumVsPayoutDto
            {
                TotalPremiumsCollected = premiums,
                TotalClaimsPaid = payouts,
                TotalCoverageAmount = totalCoverage,
                PlanBreakdown = breakdown
            };
        }



        public async Task<IEnumerable<HighValueClaimDto>> GetHighValueClaimsAsync(decimal? threshold = null)
        {
            return await _claimRepository.GetHighValueClaimsAsync(threshold);
        }
    }
}
