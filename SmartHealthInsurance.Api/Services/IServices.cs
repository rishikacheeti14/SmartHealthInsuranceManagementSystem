using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Models;

namespace SmartHealthInsurance.Api.Services
{
    public interface IAuthService
    {
        Task<string> RegisterCustomerAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<string> CreateStaffAsync(CreateUserDto dto); 
    }

    public interface IPolicyService
    {
        Task<string> EnrollPolicyAsync(PolicyCreateDto dto, int agentId);
        Task<IEnumerable<Policy>> GetCustomerPoliciesAsync(int userId);
        Task<IEnumerable<Policy>> GetPoliciesByAgentAsync(int agentId);
        Task<string> RenewPolicyAsync(int id);
        Task<IEnumerable<InsurancePlan>> GetAllPlansAsync();
        Task<IEnumerable<Policy>> GetAllPoliciesAsync(); 
        Task<Policy> GetPolicyByIdAsync(int id);
        Task<string> UpdatePolicyAsync(int id, PolicyCreateDto dto);
        Task<string> DeletePolicyAsync(int id);
        Task<string> TogglePolicyStatusAsync(int id);
        Task<Policy> GetPolicyByNumberAsync(string policyNumber);
        Task<PagedResultDto<Policy>> GetPagedPoliciesAsync(int page, int pageSize, string sortColumn, bool isAscending, int? userId = null, int? agentId = null, string? searchTerm = null);
    }

    public interface ITreatmentService
    {
        Task<string> SubmitTreatmentAsync(TreatmentCreateDto dto, int providerUserId);
        Task<IEnumerable<TreatmentRecord>> GetHospitalTreatmentsAsync(int hospitalId);
        Task<IEnumerable<TreatmentRecord>> GetTreatmentsByProviderUserIdAsync(int userId); 
        Task<IEnumerable<TreatmentRecord>> GetCustomerTreatmentsAsync(int customerId);
        Task<IEnumerable<TreatmentRecord>> GetAllTreatmentsAsync();
        Task<string> UpdateTreatmentAsync(int id, TreatmentCreateDto dto); 
        Task<string> DeleteTreatmentAsync(int id);
        Task<PagedResultDto<TreatmentRecord>> GetPagedTreatmentsAsync(int page, int pageSize, string sortColumn, bool isAscending, int? hospitalId = null, int? customerId = null, int? providerUserId = null, string? searchTerm = null);
    }

    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<User> GetUserByIdAsync(int id);
        Task<string> UpdateUserRoleAsync(int id, UpdateUserRoleDto dto);
        Task<string> UpdateUserAsync(int id, UpdateUserDto dto);
        Task<string> ToggleUserStatusAsync(int id, bool isActive);
        Task<string> DeleteUserAsync(int id);
        Task<PagedResultDto<User>> GetPagedUsersAsync(int page, int pageSize, string sortColumn, bool isAscending, string? searchTerm = null);
    }

    public interface IClaimService
    {
        Task<string> SubmitClaimAsync(ClaimCreateDto dto, int userId);
        Task<string> InitiateClaimAsync(ClaimInitiateDto dto, int userId);
        Task<string> UpdateClaimByHospitalAsync(int claimId, ClaimHospitalUpdateDto dto, int hospitalUserId);
        Task<string> FinalizeClaimAsync(int claimId, ClaimFinalizeDto dto, int userId);
        Task<string> ReviewClaimAsync(int claimId, ClaimReviewDto dto, int officerId);
        Task<IEnumerable<Claim>> GetUserClaimsAsync(int userId);
        Task<IEnumerable<Claim>> GetClaimsByStatusAsync(string? status);
        Task<Claim> GetClaimByIdAsync(int id);
        Task<PagedResultDto<Claim>> GetPagedClaimsAsync(int page, int pageSize, string sortColumn, bool isAscending, int? userId = null, string? status = null, int? providerUserId = null, string? searchTerm = null, bool isArchived = false);
        Task DeleteClaimAsync(int claimId, int userId, string userRole);
        Task UpdateClaimDescriptionAsync(int claimId, string description, int userId);
    }
    
    public interface IPaymentService 
    {
         Task<string> PayPremiumAsync(PremiumPaymentDto dto);
         Task<IEnumerable<Payment>> GetCustomerPaymentsAsync(int userId);
         Task<IEnumerable<Payment>> GetAllPaymentsAsync();
    }

    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string message);
        Task<IEnumerable<Notification>> GetMyNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
    }
   
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Role { get; set; }
    }
}
