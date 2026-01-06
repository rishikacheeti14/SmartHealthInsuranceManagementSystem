using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.Repositories;
using System.Linq.Expressions;

namespace SmartHealthInsurance.Api.Services
{
    public class TreatmentService : ITreatmentService
    {
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IHospitalRepository _hospitalRepository;
        private readonly IPolicyRepository _policyRepository;

        public TreatmentService(
            ITreatmentRepository treatmentRepository,
            IHospitalRepository hospitalRepository,
            IPolicyRepository policyRepository)
        {
            _treatmentRepository = treatmentRepository;
            _hospitalRepository = hospitalRepository;
            _policyRepository = policyRepository;
        }

        public async Task<string> SubmitTreatmentAsync(TreatmentCreateDto dto, int providerUserId)
        {

            var hospitals = await _hospitalRepository.FindAsync(h => h.UserId == providerUserId);
            var hospital = hospitals.FirstOrDefault();
            if (hospital == null) throw new Exception("Hospital record not found for this user");


            var policies = await _policyRepository.FindAsync(p => p.PolicyNumber.ToLower() == dto.PolicyNumber.ToLower());
            var policy = policies.FirstOrDefault();
            
            if (policy == null) throw new Exception("Policy not found! Please check the Policy Number.");
            if (policy.Status != Api.Enums.PolicyStatus.Active) throw new Exception("Policy is not Active.");



            var treatment = new TreatmentRecord
            {
                HospitalId = hospital.HospitalId,
                PolicyId = policy.PolicyId,
                CustomerId = policy.UserId,
                
                TreatmentDate = dto.TreatmentDate,
                Diagnosis = dto.Diagnosis,
                TreatmentDetails = dto.TreatmentDetails,
                TreatmentCost = dto.TreatmentCost,
                
                SubmittedAt = DateTime.UtcNow
            };

            await _treatmentRepository.AddAsync(treatment);
            return "Treatment details submitted successfully";
        }

        public async Task<IEnumerable<TreatmentRecord>> GetHospitalTreatmentsAsync(int hospitalId)
        {
            return await _treatmentRepository.GetByHospitalIdAsync(hospitalId);
        }

        public async Task<IEnumerable<TreatmentRecord>> GetTreatmentsByProviderUserIdAsync(int userId)
        {

            var hospitals = await _hospitalRepository.FindAsync(h => h.UserId == userId);
            var hospital = hospitals.FirstOrDefault();
            
            if (hospital == null) 
            {

                throw new Exception("Hospital record not found for this user");
            }
            

            var treatments = await _treatmentRepository.GetByHospitalIdAsync(hospital.HospitalId);
            return treatments;
        }

        public async Task<IEnumerable<TreatmentRecord>> GetCustomerTreatmentsAsync(int customerId)
        {
            return await _treatmentRepository.FindAsync(t => t.CustomerId == customerId);
        }

        public async Task<IEnumerable<TreatmentRecord>> GetAllTreatmentsAsync()
        {
            return await _treatmentRepository.GetAllAsync();
        }

        public async Task<string> UpdateTreatmentAsync(int id, TreatmentCreateDto dto)
        {
            var treatment = await _treatmentRepository.GetByIdAsync(id);
            if (treatment == null) throw new Exception("Treatment record not found");

            treatment.Diagnosis = dto.Diagnosis;
            treatment.TreatmentDetails = dto.TreatmentDetails;
            treatment.TreatmentCost = dto.TreatmentCost;
            treatment.TreatmentDate = dto.TreatmentDate;


            await _treatmentRepository.UpdateAsync(treatment);
            return "Treatment updated successfully";
        }

        public async Task<string> DeleteTreatmentAsync(int id)
        {
            await _treatmentRepository.DeleteAsync(id);
            return "Treatment deleted successfully";
        }

        public async Task<PagedResultDto<TreatmentRecord>> GetPagedTreatmentsAsync(int page, int pageSize, string sortColumn, bool isAscending, int? hospitalId = null, int? customerId = null, int? providerUserId = null, string? searchTerm = null)
        {
            Expression<Func<TreatmentRecord, bool>>? filter = null;
            

            if (providerUserId.HasValue)
            {
                var hospitals = await _hospitalRepository.FindAsync(h => h.UserId == providerUserId.Value);
                var hospital = hospitals.FirstOrDefault();
                if (hospital != null) hospitalId = hospital.HospitalId;
            }

            if (hospitalId.HasValue)
            {
                filter = t => t.HospitalId == hospitalId.Value;
            }
            else if (customerId.HasValue)
            {
                filter = t => t.CustomerId == customerId.Value;
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                
                Expression<Func<TreatmentRecord, bool>> searchExpression = t => 
                    (t.Diagnosis.ToLower().Contains(term) || 
                     (t.TreatmentDetails != null && t.TreatmentDetails.ToLower().Contains(term)) ||
                     (t.Policy != null && t.Policy.PolicyNumber.ToLower().Contains(term))); 
                
                if (filter != null)
                {

                   if (hospitalId.HasValue)
                   {
                       filter = t => t.HospitalId == hospitalId.Value && 
                                     (t.Diagnosis.ToLower().Contains(term) || 
                                      (t.TreatmentDetails != null && t.TreatmentDetails.ToLower().Contains(term)));
                   }
                   else if (customerId.HasValue)
                   {
                       filter = t => t.CustomerId == customerId.Value && 
                                     (t.Diagnosis.ToLower().Contains(term) || 
                                      (t.TreatmentDetails != null && t.TreatmentDetails.ToLower().Contains(term)));
                   }
                }
                else
                {
                   filter = t => t.Diagnosis.ToLower().Contains(term) || 
                                 (t.TreatmentDetails != null && t.TreatmentDetails.ToLower().Contains(term));
                }
            }

            var result = await _treatmentRepository.GetPagedAsync(page, pageSize, sortColumn, isAscending, filter);
            return new PagedResultDto<TreatmentRecord>
            {
                Items = result.Items,
                TotalCount = result.TotalCount
            };
        }
    }
}
