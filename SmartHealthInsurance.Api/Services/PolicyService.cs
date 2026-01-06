using Microsoft.EntityFrameworkCore;
using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Enums;
using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.Repositories;
using System.Linq.Expressions;

namespace SmartHealthInsurance.Api.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IRepository<InsurancePlan> _planRepository; 
        private readonly IUserRepository _userRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly INotificationService _notificationService;

        public PolicyService(
            IPolicyRepository policyRepository, 
            IRepository<InsurancePlan> planRepository,
            IUserRepository userRepository,
            IClaimRepository claimRepository,
            IPaymentRepository paymentRepository,
            ITreatmentRepository treatmentRepository,
            INotificationService notificationService)
        {
            _policyRepository = policyRepository;
            _planRepository = planRepository;
            _userRepository = userRepository;
            _claimRepository = claimRepository;
            _paymentRepository = paymentRepository;
            _treatmentRepository = treatmentRepository;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<InsurancePlan>> GetAllPlansAsync()
        {
            return await _planRepository.GetAllAsync();
        }

        public async Task<string> EnrollPolicyAsync(PolicyCreateDto dto, int agentId)
        {
            var plan = await _planRepository.GetByIdAsync(dto.PlanId);
            if (plan == null) throw new Exception("Insurance Plan not found");

            var allUsers = await _userRepository.GetAllAsync();

            var user = await _userRepository.GetByEmailAsync(dto.Email);
            

            
            if (user == null)
            {
               throw new Exception($"Customer with email '{dto.Email}' is not registered. Please ask the customer to register first.");
            }
            else
            {

                if (user.Role == UserRole.InsuranceAgent || user.Role == UserRole.Admin || user.Role == UserRole.ClaimsOfficer || user.Role == UserRole.HospitalManager)
                {
                    throw new Exception($"The email '{dto.Email}' belongs to a Staff Member ({user.Role}). Policies can only be enrolled for Customers.");
                }
            }


            var existingPolicy = await _policyRepository.GetActivePolicyByUserIdAsync(user.UserId);
             if (existingPolicy != null && existingPolicy.PlanId == dto.PlanId)
                 throw new Exception("This customer already has an active policy for this plan.");

            var policy = new Policy
            {
                PolicyNumber = $"POL-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                UserId = user.UserId,
                AgentId = agentId,
                PlanId = dto.PlanId,
                StartDate = dto.StartDate,
                EndDate = dto.StartDate.AddMonths(plan.DurationInMonths),
                Status = PolicyStatus.Active, 
                PremiumPaid = false,
                PremiumAmount = plan.PremiumAmount 
            };
            
            await _policyRepository.AddAsync(policy);

 
            await _notificationService.CreateNotificationAsync(user.UserId, 
                $"Policy {policy.PolicyNumber} has been successfully enrolled for you. Please pay the premium to activate it.");
            
            
            await _notificationService.CreateNotificationAsync(agentId,
                $"You successfully enrolled Policy {policy.PolicyNumber} for {dto.FirstName} {dto.LastName}.");

            return "Policy enrolled successfully for " + dto.FirstName;
        }

        public async Task<string> RenewPolicyAsync(int id)
        {
            var policy = await _policyRepository.GetByIdAsync(id);
            if (policy == null) throw new Exception("Policy not found");

            if (policy.Plan == null)
            {
                policy.Plan = await _planRepository.GetByIdAsync(policy.PlanId);
            }
            if (policy.Plan == null) throw new Exception("Associated Plan not found");

            if (policy.Plan == null) throw new Exception("Associated Plan not found");

            decimal baseAmount = policy.PremiumAmount > 0 ? policy.PremiumAmount : policy.Plan.PremiumAmount;
            
            policy.PremiumAmount = Math.Round(baseAmount * 1.08m, 2);
            if (policy.EndDate < DateTime.UtcNow)
            {
               var newStart = policy.EndDate > DateTime.UtcNow ? policy.EndDate : DateTime.UtcNow;
               policy.EndDate = newStart.AddMonths(policy.Plan.DurationInMonths);
            }
            else
            {
                policy.EndDate = policy.EndDate.AddMonths(policy.Plan.DurationInMonths);
            }

            policy.PremiumPaid = false; 
            policy.Status = PolicyStatus.Active; 

            await _policyRepository.UpdateAsync(policy);

             if (policy.AgentId.HasValue)
             {
                 await _notificationService.CreateNotificationAsync(policy.AgentId.Value,
                     $"Policy {policy.PolicyNumber} renewed successfully. New End Date: {policy.EndDate:d}");
             }

             await _notificationService.CreateNotificationAsync(policy.UserId,
                 $"Your Policy {policy.PolicyNumber} has been renewed. Please pay the renewal premium of {policy.PremiumAmount:C}.");

            return $"Policy renewed. New Premium: {policy.PremiumAmount:C}. Valid until: {policy.EndDate:d}";
        }

        public async Task<IEnumerable<Policy>> GetCustomerPoliciesAsync(int userId)
        {

            var policies = await _policyRepository.GetByUserIdAsync(userId);

            if (!policies.Any())
            {

                 var allPolicies = await _policyRepository.GetAllAsync();
                 var fallbackPolicies = allPolicies.Where(p => p.UserId == userId).ToList();

                 
                 if (fallbackPolicies.Any()) return fallbackPolicies;
            }

            return policies;
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByAgentAsync(int agentId)
        {
            return await _policyRepository.GetByAgentIdAsync(agentId);
        }

        public async Task<IEnumerable<Policy>> GetAllPoliciesAsync()
        {
            return await _policyRepository.GetAllAsync();
        }

        public async Task<Policy> GetPolicyByIdAsync(int id)
        {
            return await _policyRepository.GetByIdAsync(id);
        }

        public async Task<string> UpdatePolicyAsync(int id, PolicyCreateDto dto)
        {
            var policy = await _policyRepository.GetByIdAsync(id);
            if (policy == null) throw new Exception("Policy not found");

            if (policy.PlanId != dto.PlanId)
            {
                var newPlan = await _planRepository.GetByIdAsync(dto.PlanId);
                if (newPlan == null) throw new Exception("New Insurance Plan not found");

                policy.PlanId = dto.PlanId;
                policy.EndDate = policy.StartDate.AddMonths(newPlan.DurationInMonths);
            }

            if (policy.StartDate != dto.StartDate)
            {
                policy.StartDate = dto.StartDate;

                var plan = await _planRepository.GetByIdAsync(policy.PlanId);
                if(plan != null)
                   policy.EndDate = policy.StartDate.AddMonths(plan.DurationInMonths);
            }
            


            await _policyRepository.UpdateAsync(policy);

             if (policy.AgentId.HasValue)
             {
                  await _notificationService.CreateNotificationAsync(policy.AgentId.Value,
                     $"Policy {policy.PolicyNumber} details were updated.");
             }

             await _notificationService.CreateNotificationAsync(policy.UserId,
                 $"Updates have been made to your Policy {policy.PolicyNumber}.");

            return "Policy updated successfully";
        }

        public async Task<string> DeletePolicyAsync(int id)
        {
            var policy = await _policyRepository.GetByIdAsync(id);
            if (policy == null) throw new Exception("Policy not found");

            var hasPayments = await _paymentRepository.ExistsAsync(p => p.PolicyId == id);
            if (hasPayments)
                throw new Exception("Cannot delete policy with associated payments. Please cancel or deactivate it instead.");

            var hasClaims = await _claimRepository.ExistsAsync(c => c.PolicyId == id);
            if (hasClaims)
                throw new Exception("Cannot delete policy with associated claims.");

            var hasTreatments = await _treatmentRepository.ExistsAsync(tr => tr.PolicyId == id);
            if (hasTreatments)
                throw new Exception("Cannot delete policy with associated treatment records.");

            await _policyRepository.DeleteAsync(id);
            return "Policy deleted successfully";
        }

        public async Task<string> TogglePolicyStatusAsync(int id)
        {
            var policy = await _policyRepository.GetByIdAsync(id);
            if (policy == null) throw new Exception("Policy not found");

            if (policy.Status == PolicyStatus.Expired)
                throw new Exception("Cannot change status of an Expired policy.");

            if (policy.Status == PolicyStatus.Active)
            {
                policy.Status = PolicyStatus.Suspended;
                await _policyRepository.UpdateAsync(policy);
                return "Policy deactivated (suspended) successfully.";
            }
            else if (policy.Status == PolicyStatus.Suspended)
            {
                policy.Status = PolicyStatus.Active;
                await _policyRepository.UpdateAsync(policy);
                return "Policy activated successfully.";
            }

            return "Status unchanged.";
        }

        public async Task<Policy> GetPolicyByNumberAsync(string policyNumber)
        {
            var policies = await _policyRepository.FindAsync(p => p.PolicyNumber == policyNumber);
            var policy = policies.FirstOrDefault();
            
            if (policy != null)
            {
                if (policy.User == null)
                    policy.User = await _userRepository.GetByIdAsync(policy.UserId);
                
                if (policy.Plan == null)
                    policy.Plan = await _planRepository.GetByIdAsync(policy.PlanId); 
            }
            return policy;
        }

        public async Task<PagedResultDto<Policy>> GetPagedPoliciesAsync(int page, int pageSize, string sortColumn, bool isAscending, int? userId = null, int? agentId = null, string? searchTerm = null)
        {
            Expression<Func<Policy, bool>>? filter = null;
            if (userId.HasValue)
            {
                filter = p => p.UserId == userId.Value;
            }
            else if (agentId.HasValue)
            {
                filter = p => p.AgentId == agentId.Value;
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                Expression<Func<Policy, bool>> searchFilter = p => 
                    (p.PolicyNumber.ToLower().Contains(term) ||
                    (p.User != null && (p.User.FirstName.ToLower().Contains(term) || p.User.LastName.ToLower().Contains(term))) ||
                    (p.Plan != null && p.Plan.PlanName.ToLower().Contains(term)));

                if (filter != null)
                {
                    if (userId.HasValue)
                    {
                         filter = p => p.UserId == userId.Value && 
                            (p.PolicyNumber.ToLower().Contains(term) ||
                            (p.User != null && (p.User.FirstName.ToLower().Contains(term) || p.User.LastName.ToLower().Contains(term))) ||
                            (p.Plan != null && p.Plan.PlanName.ToLower().Contains(term)));
                    }
                    else if (agentId.HasValue)
                    {
                         filter = p => p.AgentId == agentId.Value && 
                            (p.PolicyNumber.ToLower().Contains(term) ||
                            (p.User != null && (p.User.FirstName.ToLower().Contains(term) || p.User.LastName.ToLower().Contains(term))) ||
                            (p.Plan != null && p.Plan.PlanName.ToLower().Contains(term)));
                    }
                }
                else
                {
                    filter = searchFilter;
                }
            }

            var result = await _policyRepository.GetPagedAsync(page, pageSize, sortColumn, isAscending, filter);
            return new PagedResultDto<Policy>
            {
                Items = result.Items,
                TotalCount = result.TotalCount
            };
        }
    }
}
