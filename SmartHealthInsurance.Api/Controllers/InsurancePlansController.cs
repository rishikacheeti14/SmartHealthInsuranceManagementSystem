using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHealthInsurance.Api.Data;
using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.Repositories;

namespace SmartHealthInsurance.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsurancePlansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<InsurancePlan> _planRepository;

        public InsurancePlansController(ApplicationDbContext context, IRepository<InsurancePlan> planRepository)
        {
            _context = context;
            _planRepository = planRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPlans()
        {
            var plans = await _context.InsurancePlans
                .Select(p => new
                {
                    p.PlanId,
                    p.PlanName,
                    p.Description,
                    p.PremiumAmount,
                    p.CoverageLimit,
                    p.DurationInMonths,
                    p.IsActive
                })
                .ToListAsync();

            return Ok(plans);
        }
        [HttpGet("active")]
        public async Task<IActionResult> GetActivePlans()
        {
            var plans = await _context.InsurancePlans
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.PlanId,
                    p.PlanName,
                    p.Description,
                    p.PremiumAmount,
                    p.CoverageLimit,
                    p.DurationInMonths
                })
                .ToListAsync();

            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlanById(int id)
        {
            var plan = await _context.InsurancePlans
                .Where(p => p.PlanId == id)
                .Select(p => new
                {
                    p.PlanId,
                    p.PlanName,
                    p.Description,
                    p.PremiumAmount,
                    p.CoverageLimit,
                    p.DurationInMonths,
                    p.IsActive
                })
                .FirstOrDefaultAsync();

            if (plan == null)
                return NotFound("Plan not found");

            return Ok(plan);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePlan(InsurancePlanCreateDto dto)
        {
            var plan = new InsurancePlan
            {
                PlanName = dto.PlanName,
                Description = dto.Description ?? string.Empty,
                PremiumAmount = dto.PremiumAmount,
                CoverageLimit = dto.CoverageLimit,
                DurationInMonths = dto.DurationInMonths,
                IsActive = true
            };

            _context.InsurancePlans.Add(plan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Insurance plan created successfully" });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePlan(int id, InsurancePlanUpdateDto dto)
        {
            var plan = await _context.InsurancePlans.FindAsync(id);
            if (plan == null)
                return NotFound("Plan not found");

            plan.PlanName = dto.PlanName;
            plan.Description = dto.Description ?? string.Empty;
            plan.PremiumAmount = dto.PremiumAmount;
            plan.CoverageLimit = dto.CoverageLimit;
            plan.DurationInMonths = dto.DurationInMonths;
            plan.IsActive = dto.IsActive;
            plan.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Insurance plan updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var plan = await _context.InsurancePlans.FindAsync(id);
            if (plan == null)
                return NotFound("Plan not found");

            try 
            {
                _context.InsurancePlans.Remove(plan);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Insurance plan deleted" });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Cannot delete this plan because it is currently assigned to one or more policies." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedPlans(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string sortColumn = "PlanId", 
            [FromQuery] bool isAscending = false,
            [FromQuery] string? searchTerm = null)
        {
            System.Linq.Expressions.Expression<Func<InsurancePlan, bool>> filter = null;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                filter = p => p.PlanName.ToLower().Contains(term) || 
                              p.Description.ToLower().Contains(term);
            }

            var result = await _planRepository.GetPagedAsync(page, pageSize, sortColumn, isAscending, filter);
            return Ok(new PagedResultDto<object>
            {
                Items = result.Items.Select(p => new
                {
                    p.PlanId,
                    p.PlanName,
                    p.Description,
                    p.PremiumAmount,
                    p.CoverageLimit,
                    p.DurationInMonths,
                    p.IsActive
                }),
                TotalCount = result.TotalCount
            });
        }
    }
}
