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
    public class HospitalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHospitalRepository _hospitalRepository;
        private readonly Services.INotificationService _notificationService;

        public HospitalsController(
            ApplicationDbContext context, 
            IHospitalRepository hospitalRepository,
            Services.INotificationService notificationService)
        {
            _context = context;
            _hospitalRepository = hospitalRepository;
            _notificationService = notificationService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateHospital(HospitalCreateDto dto)
        {
            if (dto.UserId > 0 && await _context.Hospitals.AnyAsync(h => h.UserId == dto.UserId))
            {
                return BadRequest(new { message = "This manager is already assigned to another hospital." });
            }

            var hospital = new Hospital
            {
                HospitalName = dto.HospitalName,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode,
                IsNetworkProvider = dto.IsNetworkProvider,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hospital created successfully" });
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetHospitals()
        {
            var hospitals = await _context.Hospitals
                .Select(h => new
                {
                    h.HospitalId,
                    h.HospitalName,
                    h.City,
                    h.State,
                    h.PhoneNumber,
                    h.Email,
                    h.IsNetworkProvider
                })
                .ToListAsync();

            return Ok(hospitals);
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetHospitalById(int id)
        {
            var hospital = await _context.Hospitals
                .Include(h => h.Claims)
                .FirstOrDefaultAsync(h => h.HospitalId == id);

            if (hospital == null)
                return NotFound("Hospital not found");

            return Ok(hospital);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHospital(int id, HospitalUpdateDto dto)
        {
            var hospital = await _context.Hospitals.FindAsync(id);

            if (hospital == null)
                return NotFound("Hospital not found");

            if (dto.UserId > 0 && await _context.Hospitals.AnyAsync(h => h.UserId == dto.UserId && h.HospitalId != id))
            {
                return BadRequest(new { message = "This manager is already assigned to another hospital." });
            }

            hospital.HospitalName = dto.HospitalName;
            hospital.Address = dto.Address;
            hospital.PhoneNumber = dto.PhoneNumber;
            hospital.Email = dto.Email;
            hospital.City = dto.City;
            hospital.State = dto.State;
            hospital.ZipCode = dto.ZipCode;
            hospital.IsNetworkProvider = dto.IsNetworkProvider;
            hospital.UserId = dto.UserId; 
            hospital.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Hospital updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHospital(int id)
        {
            var hospital = await _context.Hospitals.FindAsync(id);

            if (hospital == null)
                return NotFound("Hospital not found");

            try
            {
                if (hospital.UserId != 0)
                {
                    await _notificationService.CreateNotificationAsync(hospital.UserId, $"Hospital '{hospital.HospitalName}' has been deleted from the system.");
                }

                _context.Hospitals.Remove(hospital);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Hospital deleted successfully" });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Cannot delete this hospital because it has associated claims or treatments." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("paged")]
        [Authorize]
        public async Task<IActionResult> GetPagedHospitals(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string sortColumn = "HospitalId", 
            [FromQuery] bool isAscending = false,
            [FromQuery] string? searchTerm = null)
        {
            System.Linq.Expressions.Expression<Func<Hospital, bool>> filter = null;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                filter = h => h.HospitalName.ToLower().Contains(term) || 
                              h.City.ToLower().Contains(term) || 
                              h.State.ToLower().Contains(term);
            }

            var result = await _hospitalRepository.GetPagedAsync(page, pageSize, sortColumn, isAscending, filter);
            return Ok(new PagedResultDto<object>
            {
                Items = result.Items.Select(h => new
                {
                    h.HospitalId,
                    h.HospitalName,
                    h.City,
                    h.State,
                    h.PhoneNumber,
                    h.Email,
                    h.IsNetworkProvider
                }),
                TotalCount = result.TotalCount
            });
        }
    }
}
