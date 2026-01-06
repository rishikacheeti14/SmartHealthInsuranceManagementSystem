using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Services;
using System.Security.Claims;

namespace SmartHealthInsurance.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PoliciesController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        [HttpPost("enroll")]
        [Authorize(Roles = "InsuranceAgent")]
        public async Task<IActionResult> CreatePolicy(PolicyCreateDto dto)
        {
            try
            {
                var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _policyService.EnrollPolicyAsync(dto, agentId);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin,ClaimsOfficer,Hospital")]
        public async Task<IActionResult> GetAllPolicies()
        {
            var policies = await _policyService.GetAllPoliciesAsync();
            return Ok(policies);
        }
        [HttpGet("agent-policies")]
        [Authorize(Roles = "InsuranceAgent")]
        public async Task<IActionResult> GetAgentPolicies()
        {
            var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var policies = await _policyService.GetPoliciesByAgentAsync(agentId);
            return Ok(policies);
        }
        [HttpGet("my-policies")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyPolicies()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("User ID not found in token");

            int userId = int.Parse(userIdStr);
            var myPolicies = await _policyService.GetCustomerPoliciesAsync(userId);
            
            if (!myPolicies.Any())
            {
                return Ok(new List<object>()); 
            }

            return Ok(myPolicies);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPolicyById(int id)
        {
            var policy = await _policyService.GetPolicyByIdAsync(id);
            if (policy == null) return NotFound("Policy not found");
            return Ok(policy);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "InsuranceAgent")]
        public async Task<IActionResult> UpdatePolicy(int id, PolicyCreateDto dto)
        {
            try
            {
                var result = await _policyService.UpdatePolicyAsync(id, dto);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "InsuranceAgent")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            try
            {
                var result = await _policyService.DeletePolicyAsync(id);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("lookup/{policyNumber}")]
        [Authorize(Roles = "HospitalManager,Admin,ClaimsOfficer")]
        public async Task<IActionResult> LookupPolicy(string policyNumber)
        {
            try 
            {
                var policy = await _policyService.GetPolicyByNumberAsync(policyNumber);
                
                if (policy == null) return NotFound(new { message = "Policy not found" });

                return Ok(new PolicyLookupDto
                {
                    PolicyId = policy.PolicyId,
                    PolicyNumber = policy.PolicyNumber,
                    Status = policy.Status.ToString(),
                    PlanName = policy.Plan?.PlanName ?? "Unknown Plan",
                    PatientName = policy.User != null ? $"{policy.User.FirstName} {policy.User.LastName}" : "Unknown Patient",
                    StartDate = policy.StartDate,
                    EndDate = policy.EndDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/renew")]
        [Authorize(Roles = "InsuranceAgent")]
        public async Task<IActionResult> RenewPolicy(int id)
        {
            try
            {
                var result = await _policyService.RenewPolicyAsync(id);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/toggle-status")]
        [Authorize(Roles = "InsuranceAgent")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var result = await _policyService.TogglePolicyStatusAsync(id);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedPolicies(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string sortColumn = "PolicyId", 
            [FromQuery] bool isAscending = false,
            [FromQuery] string? searchTerm = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            int? filterUserId = null;
            int? filterAgentId = null;

            if (role == "Customer") filterUserId = userId;
            else if (role == "InsuranceAgent") filterAgentId = userId;

            var result = await _policyService.GetPagedPoliciesAsync(page, pageSize, sortColumn, isAscending, filterUserId, filterAgentId, searchTerm);
            
            return Ok(new PagedResultDto<object>
            {
                Items = result.Items.Select(p => new
                {
                    p.PolicyId,
                    p.PolicyNumber,
                    p.Status,
                    p.StartDate,
                    p.EndDate,
                    p.PremiumPaid,
                    PlanName = p.Plan?.PlanName ?? "Unknown Plan",
                    PremiumAmount = p.Plan?.PremiumAmount ?? 0m,
                    CustomerName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}" : "Unknown Customer"
                }),
                TotalCount = result.TotalCount
            });
        }
    }
}
