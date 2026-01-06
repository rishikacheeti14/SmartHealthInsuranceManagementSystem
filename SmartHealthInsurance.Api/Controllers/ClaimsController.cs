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
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimsController(IClaimService claimService)
        {
            _claimService = claimService;
        }
        [HttpPost("initiate")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> InitiateClaim(ClaimInitiateDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _claimService.InitiateClaimAsync(dto, userId);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/hospital-update")]
        [Authorize(Roles = "HospitalManager")]
        public async Task<IActionResult> UpdateClaimByHospital(int id, ClaimHospitalUpdateDto dto)
        {
            try
            {
                var hospitalUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _claimService.UpdateClaimByHospitalAsync(id, dto, hospitalUserId);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/finalize")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> FinalizeClaim(int id, ClaimFinalizeDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _claimService.FinalizeClaimAsync(id, dto, userId);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> SubmitClaim(ClaimCreateDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _claimService.SubmitClaimAsync(dto, userId);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [AllowAnonymous] 
        public async Task<IActionResult> DeleteClaim(int id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return Unauthorized();

            try
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                           ?? User.FindFirst("nameid") 
                           ?? User.FindFirst("sub");
                           
                if (idClaim == null) return Unauthorized(new { Message = "User ID missing in token" });
                
                var userId = int.Parse(idClaim.Value);

                var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
                var userRole = roleClaim?.Value ?? "Customer";

                await _claimService.DeleteClaimAsync(id, userId, userRole);
                return Ok(new { Message = "Claim deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateClaim(int id, [FromBody] UpdateClaimDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                await _claimService.UpdateClaimDescriptionAsync(id, dto.TreatmentDescription, userId);
                return Ok(new { Message = "Claim updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/review")]
        [Authorize(Roles = "ClaimsOfficer")]
        public async Task<IActionResult> ReviewClaim(int id, ClaimReviewDto dto)
        {
            try
            {
                var officerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _claimService.ReviewClaimAsync(id, dto, officerId);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("my-claims")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyClaims()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var claims = await _claimService.GetUserClaimsAsync(userId);
            return Ok(claims);
        }

        [HttpGet]
        [Authorize(Roles = "ClaimsOfficer,Admin")]
        public async Task<IActionResult> GetClaims([FromQuery] string? status)
        {
            var claims = await _claimService.GetClaimsByStatusAsync(status);
            return Ok(claims);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedClaims(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string sortColumn = "ClaimId", 
            [FromQuery] bool isAscending = false,
            [FromQuery] string? status = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool isArchived = false)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
                var role = roleClaim?.Value ?? "Customer"; 

                int? filterUserId = null;
                int? filterProviderUserId = null;

                if (role == "Customer") filterUserId = userId;
                else if (role == "HospitalManager") filterProviderUserId = userId;

                var result = await _claimService.GetPagedClaimsAsync(page, pageSize, sortColumn, isAscending, filterUserId, status, filterProviderUserId, searchTerm, isArchived);
                
                return Ok(new PagedResultDto<object>
                {
                    Items = result.Items.Select(c => new
                    {
                        c.ClaimId,
                        c.ClaimNumber,
                        c.ClaimAmount,
                        c.ApprovedAmount,
                        c.Status,
                        c.SubmittedAt,
                        c.ReviewedAt,
                        c.ProcessedAt,
                        c.RejectionReason,
                        c.TreatmentDetails,
                        c.TreatmentDescription,
                        Diagnosis = c.TreatmentRecord != null ? c.TreatmentRecord.Diagnosis : null,
                        TreatmentRecord = c.TreatmentRecord == null ? null : new 
                        {
                            c.TreatmentRecord.TreatmentId,
                            c.TreatmentRecord.TreatmentCost,
                            c.TreatmentRecord.Diagnosis,
                            c.TreatmentRecord.TreatmentDetails,
                            c.TreatmentRecord.TreatmentDate,
                            c.TreatmentRecord.HospitalId
                        }, 
                        c.PolicyId,
                        PolicyNumber = c.Policy?.PolicyNumber ?? "Unknown Policy",
                        HospitalName = c.Hospital?.HospitalName ?? "Unknown Hospital",
                        CustomerName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown Customer"
                    }),
                    TotalCount = result.TotalCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
