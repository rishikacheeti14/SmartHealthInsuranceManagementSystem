using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Enums;
using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.Repositories;
using System.Linq.Expressions;

namespace SmartHealthInsurance.Api.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IHospitalRepository _hospitalRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;

        public ClaimService(
            IClaimRepository claimRepository,
            IPolicyRepository policyRepository,
            ITreatmentRepository treatmentRepository,
            IPaymentRepository paymentRepository,
            IHospitalRepository hospitalRepository,
            INotificationService notificationService,
            IUserRepository userRepository)
        {
            _claimRepository = claimRepository;
            _policyRepository = policyRepository;
            _treatmentRepository = treatmentRepository;
            _paymentRepository = paymentRepository;
            _hospitalRepository = hospitalRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
        }

        public async Task<string> SubmitClaimAsync(ClaimCreateDto dto, int userId)
        {

            var policy = await _policyRepository.GetPolicyWithPlanAsync(dto.PolicyId);
            if (policy == null || policy.UserId != userId)
                throw new Exception("Invalid policy");

            var payments = await _paymentRepository.FindAsync(p => p.PolicyId == dto.PolicyId 
                                                               && p.PaymentType == PaymentType.Premium 
                                                               && p.Status == PaymentStatus.Completed);
            if (!payments.Any())
                throw new Exception("Cannot submit claim. Premium has not been paid for this policy yet.");

            if (policy.Plan != null && dto.ClaimAmount > policy.Plan.CoverageLimit)
            {
                throw new Exception($"Claim amount ({dto.ClaimAmount:C}) exceeds the policy coverage limit of {policy.Plan.CoverageLimit:C}.");
            }
            
            var treatment = await _treatmentRepository.GetByIdAsync(dto.TreatmentId);
            if (treatment == null) throw new Exception("Invalid treatment details");
            
            var existingClaims = await _claimRepository.FindAsync(c => c.TreatmentId == dto.TreatmentId);
            if (existingClaims.Any()) throw new Exception("Claim already submitted for this treatment");

            var claim = new Claim
            {
                ClaimNumber = $"CLM-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                PolicyId = policy.PolicyId,
                UserId = userId,
                TreatmentId = treatment.TreatmentId,
                HospitalId = treatment.HospitalId,
                ClaimAmount = dto.ClaimAmount,
                ApprovedAmount = 0,
                TreatmentDate = treatment.TreatmentDate,
                TreatmentDetails = treatment.TreatmentDetails,
                Status = ClaimStatus.Submitted,
                SubmittedAt = DateTime.UtcNow
            };

            await _claimRepository.AddAsync(claim);
            return "Claim submitted successfully";
        }

        public async Task<string> InitiateClaimAsync(ClaimInitiateDto dto, int userId)
        {
            var policy = await _policyRepository.GetByIdAsync(dto.PolicyId);
            if (policy == null || policy.UserId != userId)
                throw new Exception("Invalid policy");

            var payments = await _paymentRepository.FindAsync(p => p.PolicyId == dto.PolicyId 
                                                               && p.PaymentType == PaymentType.Premium 
                                                               && p.Status == PaymentStatus.Completed);
            if (!payments.Any())
                throw new Exception("Cannot initiate claim. Premium has not been paid for this policy yet.");
                
            var claim = new Claim
            {
                ClaimNumber = $"CLM-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                UserId = userId,
                PolicyId = dto.PolicyId,
                HospitalId = dto.HospitalId,
                TreatmentDescription = dto.TreatmentDescription,
                Status = ClaimStatus.Initiated,
                SubmittedAt = DateTime.UtcNow
            };

            await _claimRepository.AddAsync(claim);

            await _notificationService.CreateNotificationAsync(userId, "Claim initiated. Please wait for hospital to update treatment details.");

            var hospital = await _hospitalRepository.GetByIdAsync(dto.HospitalId);
            if (hospital != null && hospital.UserId != 0)
            {
                await _notificationService.CreateNotificationAsync(hospital.UserId, $"New claim initiated for patient. Claim #{claim.ClaimNumber}");
            }

            return "Claim initiated. Please wait for hospital to update treatment details.";
        }

        public async Task<string> UpdateClaimByHospitalAsync(int claimId, ClaimHospitalUpdateDto dto, int hospitalUserId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new Exception("Claim not found");

            var hospitals = await _hospitalRepository.FindAsync(h => h.UserId == hospitalUserId);
            var hospital = hospitals.FirstOrDefault();
            if (hospital == null || claim.HospitalId != hospital.HospitalId)
                throw new Exception("Unauthorized: You are not the manager of the hospital for this claim.");

            if (claim.Status != ClaimStatus.Initiated)
                throw new Exception("Claim is not in 'Initiated' state.");

            var treatment = new TreatmentRecord
            {
                HospitalId = hospital.HospitalId,
                CustomerId = claim.UserId,
                TreatmentDate = dto.TreatmentDate,
                Diagnosis = dto.Diagnosis,
                TreatmentDetails = dto.TreatmentDetails,
                TreatmentCost = dto.TreatmentCost,
                SubmittedAt = DateTime.UtcNow

            };
            await _treatmentRepository.AddAsync(treatment);

            claim.TreatmentId = treatment.TreatmentId;
            claim.ClaimAmount = dto.TreatmentCost;
            claim.TreatmentDate = dto.TreatmentDate;
            claim.TreatmentDetails = dto.TreatmentDetails;
            claim.Status = ClaimStatus.AwaitingPolicy;

            await _claimRepository.UpdateAsync(claim);


            await _notificationService.CreateNotificationAsync(claim.UserId, 
                $"Hospital has updated treatment details for Claim {claim.ClaimNumber}. Please finalize the claim.");

            return "Treatment details added successfully. Customer can now finalize the claim.";
        }

        public async Task<string> FinalizeClaimAsync(int claimId, ClaimFinalizeDto dto, int userId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null || claim.UserId != userId)
                throw new Exception("Claim not found or unauthorized");

            if (claim.Status != ClaimStatus.AwaitingPolicy)
                throw new Exception("Claim is not ready for finalization (Awaiting hospital update).");

            var policy = await _policyRepository.GetPolicyWithPlanAsync(dto.PolicyId);
            if (policy == null || policy.UserId != userId)
                throw new Exception("Invalid or unauthorized policy selection.");
            
            if (policy.Status != Api.Enums.PolicyStatus.Active)
                throw new Exception("Selected policy is not active.");

            if (policy.Plan != null && claim.ClaimAmount > policy.Plan.CoverageLimit)
            {
               throw new Exception($"Claim amount ({claim.ClaimAmount:C}) exceeds the policy coverage limit of {policy.Plan.CoverageLimit:C}.");
            }

            claim.PolicyId = dto.PolicyId;
            claim.Status = ClaimStatus.Submitted;

            if (claim.TreatmentId.HasValue)
            {
                var treatment = await _treatmentRepository.GetByIdAsync(claim.TreatmentId.Value);
                if (treatment != null)
                {
                    treatment.PolicyId = policy.PolicyId;
                    await _treatmentRepository.UpdateAsync(treatment);
                }
            }

            await _claimRepository.UpdateAsync(claim);

            await _notificationService.CreateNotificationAsync(claim.UserId, $"Claim {claim.ClaimNumber} submitted for review.");

            var officers = await _userRepository.FindAsync(u => u.Role == UserRole.ClaimsOfficer && u.IsActive);
            var officer = officers.FirstOrDefault();
            if (officer != null)
            {
                await _notificationService.CreateNotificationAsync(officer.UserId, $"New claim {claim.ClaimNumber} requires review.");
            }

            return "Claim finalized and submitted successfully.";


        }

        public async Task<string> ReviewClaimAsync(int claimId, ClaimReviewDto dto, int officerId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new Exception("Claim not found");
            
            if (claim.Status == ClaimStatus.Paid) throw new Exception("Claim already paid");

            claim.ReviewedBy = officerId;
            claim.ReviewedAt = DateTime.UtcNow;

            if (!dto.IsApproved)
            {
                claim.Status = ClaimStatus.Rejected;
                claim.RejectionReason = dto.RejectionReason;
                await _claimRepository.UpdateAsync(claim);
                
                await _notificationService.CreateNotificationAsync(claim.UserId, "Your claim has been rejected. Reason: " + dto.RejectionReason);

                await _notificationService.CreateNotificationAsync(officerId, $"You rejected Claim {claim.ClaimNumber}.");
                
                return "Claim rejected";

            }

            claim.Status = ClaimStatus.Approved;
            claim.ApprovedAmount = dto.ApprovedAmount;
            claim.ProcessedAt = DateTime.UtcNow;

            var payout = new Payment
            {
                PolicyId = claim.PolicyId ?? 0,
                ClaimId = claim.ClaimId,
                Amount = dto.ApprovedAmount,
                PaymentType = PaymentType.Payout,
                Status = PaymentStatus.Completed,
                PaymentMethod = "Bank Transfer (Auto)",
                PaymentDate = DateTime.UtcNow,
                PaymentReference = $"PAY-{Guid.NewGuid().ToString("N")[..8].ToUpper()}"
            };

            await _paymentRepository.AddAsync(payout);
            
            claim.Status = ClaimStatus.Paid; 
            await _claimRepository.UpdateAsync(claim);

            await _notificationService.CreateNotificationAsync(claim.UserId, $"Claim approved and payout of {dto.ApprovedAmount:C} initiated.");

            await _notificationService.CreateNotificationAsync(officerId, $"You approved Claim {claim.ClaimNumber}. Payout initiated.");

            return "Claim approved and payout completed";
        }

        public async Task<IEnumerable<Claim>> GetUserClaimsAsync(int userId)
        {
            return await _claimRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Claim>> GetClaimsByStatusAsync(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return await _claimRepository.GetAllAsync();
                
            return await _claimRepository.GetByStatusAsync(status);
        }

        public async Task<Claim> GetClaimByIdAsync(int id)
        {
            return await _claimRepository.GetByIdAsync(id);
        }

        public async Task<PagedResultDto<Claim>> GetPagedClaimsAsync(int page, int pageSize, string sortColumn, bool isAscending, int? userId = null, string? status = null, int? providerUserId = null, string? searchTerm = null, bool isArchived = false)
        {
            int? targetHospitalId = null;
            if (providerUserId.HasValue)
            {
                var hospitals = await _hospitalRepository.FindAsync(h => h.UserId == providerUserId.Value);
                var hospital = hospitals.FirstOrDefault();
                if (hospital == null)
                {
                    return new PagedResultDto<Claim> { Items = new List<Claim>(), TotalCount = 0 };
                }
                targetHospitalId = hospital.HospitalId;
            }

            ClaimStatus? targetStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ClaimStatus>(status, true, out var parsedStatus))
            {
                targetStatus = parsedStatus;
            }
            string? normalizedTerm = !string.IsNullOrEmpty(searchTerm) ? searchTerm.ToLower() : null;

            Expression<Func<Claim, bool>> filter = c =>

                (!userId.HasValue || c.UserId == userId.Value) &&
                (!targetHospitalId.HasValue || c.HospitalId == targetHospitalId.Value) &&

                (!targetStatus.HasValue || c.Status == targetStatus.Value) &&

                (!isArchived || (c.Status == ClaimStatus.Paid || c.Status == ClaimStatus.Rejected)) &&

                (normalizedTerm == null || (
                    (c.ClaimNumber != null && c.ClaimNumber.ToLower().Contains(normalizedTerm)) ||
                    (c.TreatmentDescription != null && c.TreatmentDescription.ToLower().Contains(normalizedTerm)) ||
                    (c.TreatmentRecord != null && c.TreatmentRecord.Diagnosis != null && c.TreatmentRecord.Diagnosis.ToLower().Contains(normalizedTerm))
                ));


            var result = await _claimRepository.GetPagedAsync(page, pageSize, sortColumn, isAscending, filter);
            return new PagedResultDto<Claim>
            {
                Items = result.Items,
                TotalCount = result.TotalCount
            };
        }

        public async Task DeleteClaimAsync(int claimId, int userId, string userRole)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new Exception("Claim not found");


            bool isPrivileged = userRole == "Admin" || userRole == "ClaimsOfficer";

            if (!isPrivileged)
            {
                if (claim.UserId != userId) throw new Exception("Unauthorized");

                if (claim.Status != ClaimStatus.Initiated)
                    throw new Exception("Cannot delete claim after treatment details have been submitted.");
            }

            var payments = await _paymentRepository.FindAsync(p => p.ClaimId == claimId);
            foreach (var payment in payments)
            {
                await _paymentRepository.DeleteAsync(payment.PaymentId);
            }

            await _claimRepository.DeleteAsync(claimId);
        }

        public async Task UpdateClaimDescriptionAsync(int claimId, string description, int userId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new Exception("Claim not found");
            if (claim.UserId != userId) throw new Exception("Unauthorized");

            if (claim.Status != ClaimStatus.Initiated)
                throw new Exception("Cannot edit claim after treatment details have been submitted.");

            claim.TreatmentDescription = description;
            await _claimRepository.UpdateAsync(claim);
        }
    }
}
