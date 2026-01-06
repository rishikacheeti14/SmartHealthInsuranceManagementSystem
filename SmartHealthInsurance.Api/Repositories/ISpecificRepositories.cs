using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.DTOs;

namespace SmartHealthInsurance.Api.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
    }

    public interface IPolicyRepository : IRepository<Policy>
    {
        Task<IEnumerable<Policy>> GetByUserIdAsync(int userId);
        Task<Policy> GetActivePolicyByUserIdAsync(int userId);
        Task<IEnumerable<Policy>> GetByAgentIdAsync(int agentId);
        Task<Policy> GetPolicyWithPlanAsync(int id);
        Task<IEnumerable<PolicyDistributionDto>> GetPolicyDistributionReportAsync();
    }

    public interface IClaimRepository : IRepository<Claim>
    {
        Task<IEnumerable<Claim>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Claim>> GetByStatusAsync(string status);

        Task<IEnumerable<ClaimAnalyticsDto>> GetClaimsAnalyticsReportAsync();
        Task<IEnumerable<HighValueClaimDto>> GetHighValueClaimsAsync(decimal? threshold = null);
        Task<decimal> GetTotalPayoutsAsync(int? userId = null);
    }
    

    
    public interface IHospitalRepository : IRepository<Hospital>
    {
      
    }

    public interface IPaymentRepository : IRepository<Payment> 
    {
        Task<IEnumerable<Payment>> GetByUserIdAsync(int userId);
        Task<decimal> GetTotalPremiumsAsync(int? userId = null);
    }

    public interface ITreatmentRepository : IRepository<TreatmentRecord>
    {
        Task<IEnumerable<TreatmentRecord>> GetByHospitalIdAsync(int hospitalId);
    }

    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId);
        Task<IEnumerable<Notification>> GetAllByUserIdAsync(int userId);
    }
}
