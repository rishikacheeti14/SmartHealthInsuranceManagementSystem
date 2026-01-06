using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Services;
using SmartHealthInsurance.Api.Repositories;
using System.Security.Claims;

namespace SmartHealthInsurance.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TreatmentsController : ControllerBase
    {
        private readonly ITreatmentService _treatmentService;

        public TreatmentsController(ITreatmentService treatmentService)
        {
            _treatmentService = treatmentService;
        }

        // ==================================================
        // HOSPITAL MANAGER → SUBMIT TREATMENT
        // ==================================================
        [HttpPost]
        [Authorize(Roles = "HospitalManager")]
        public async Task<IActionResult> SubmitTreatment(TreatmentCreateDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _treatmentService.SubmitTreatmentAsync(dto, userId);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==================================================
        // CUSTOMER → VIEW MY TREATMENTS
        // ==================================================
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyTreatments()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var treatments = await _treatmentService.GetCustomerTreatmentsAsync(userId);
            return Ok(treatments);
        }

        // ==================================================
        // HOSPITAL MANAGER → VIEW MY TREATMENTS
        // ==================================================
        [HttpGet("hospital-v2")]
        [Authorize(Roles = "HospitalManager")]
        public async Task<IActionResult> HospitalTreatments(
            [FromServices] IHospitalRepository hospRepo,
            [FromServices] ITreatmentRepository treatRepo)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var hospitals = await hospRepo.FindAsync(h => h.UserId == userId);
                var hospital = hospitals.FirstOrDefault();

                if (hospital == null)
                {
                    return NotFound("Hospital record not found for the current user.");
                }

                var treatments = await treatRepo.GetByHospitalIdAsync(hospital.HospitalId);
                
                if (treatments == null || !treatments.Any())
                {
                    return Ok(new List<object>()); 
                }

                return Ok(treatments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin,ClaimsOfficer")]
        public async Task<IActionResult> GetAll()
        {
            var treatments = await _treatmentService.GetAllTreatmentsAsync();
            return Ok(treatments);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "HospitalManager")]
        public async Task<IActionResult> UpdateTreatment(int id, TreatmentCreateDto dto)
        {
            try
            {
                var result = await _treatmentService.UpdateTreatmentAsync(id, dto);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "HospitalManager")]
        public async Task<IActionResult> DeleteTreatment(int id)
        {
            try
            {
                var result = await _treatmentService.DeleteTreatmentAsync(id);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedTreatments(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string sortColumn = "TreatmentId", 
            [FromQuery] bool isAscending = false,
            [FromQuery] string? searchTerm = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            int? filterCustomerId = null;
            int? filterProviderUserId = null;

            if (role == "Customer") filterCustomerId = userId;
            else if (role == "HospitalManager") filterProviderUserId = userId;

            var result = await _treatmentService.GetPagedTreatmentsAsync(page, pageSize, sortColumn, isAscending, null, filterCustomerId, filterProviderUserId, searchTerm);
            
            return Ok(new PagedResultDto<object>
            {
                Items = result.Items.Select(t => new
                {
                    t.TreatmentId,
                    t.TreatmentDate,
                    t.Diagnosis,
                    t.TreatmentDetails,
                    t.TreatmentCost,
                    t.PolicyId, 
                    PolicyNumber = t.Policy?.PolicyNumber ?? "Unknown Policy",
                    HospitalName = t.Hospital?.HospitalName ?? "Unknown Hospital",
                    CustomerName = t.Customer != null ? $"{t.Customer.FirstName} {t.Customer.LastName}" : "Unknown Customer"
                }),
                TotalCount = result.TotalCount
            });
        }
    }
}
