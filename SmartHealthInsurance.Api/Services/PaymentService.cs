using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Enums;
using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.Repositories;

namespace SmartHealthInsurance.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPolicyRepository _policyRepository;

        private readonly INotificationService _notificationService;

        public PaymentService(IPaymentRepository paymentRepository, IPolicyRepository policyRepository, INotificationService notificationService)
        {
            _paymentRepository = paymentRepository;
            _policyRepository = policyRepository;
            _notificationService = notificationService;
        }

        public async Task<string> PayPremiumAsync(PremiumPaymentDto dto)
        {
            var policy = await _policyRepository.GetPolicyWithPlanAsync(dto.PolicyId);
            if (policy == null) throw new Exception("Policy not found");

            if (policy.PremiumPaid) throw new Exception("Premium already paid");
            decimal requiredAmount = policy.PremiumAmount > 0 ? policy.PremiumAmount : (policy.Plan?.PremiumAmount ?? 0);

            if (dto.Amount < requiredAmount) throw new Exception($"Insufficient payment amount. Required: {requiredAmount:C}");

            var payment = new Payment
            {
                PolicyId = policy.PolicyId,
                Amount = dto.Amount,
                PaymentType = PaymentType.Premium,
                Status = PaymentStatus.Completed, 
                PaymentMethod = dto.PaymentMethod,
                PaymentDate = DateTime.UtcNow,
                PaymentReference = $"PAY-{Guid.NewGuid().ToString("N")[..8].ToUpper()}"
            };

            await _paymentRepository.AddAsync(payment);


            policy.PremiumPaid = true;

            await _policyRepository.UpdateAsync(policy);

            await _notificationService.CreateNotificationAsync(policy.UserId, 
                $"Premium payment of {dto.Amount:C} received. Policy {policy.PolicyNumber} is now active.");

            if (policy.AgentId.HasValue)
            {
                await _notificationService.CreateNotificationAsync(policy.AgentId.Value, 
                    $"Premium paid for Policy {policy.PolicyNumber}. Policy is now Active.");
            }

            return "Premium payment successful. Policy is now active.";
        }

        public async Task<IEnumerable<Payment>> GetCustomerPaymentsAsync(int userId)
        {
            return await _paymentRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _paymentRepository.GetAllAsync();
        }
    }
}
