using Microsoft.EntityFrameworkCore;
using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Enums;
using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.Repositories;

namespace SmartHealthInsurance.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IHospitalRepository _hospitalRepository;
        private readonly INotificationRepository _notificationRepository;

        public UserService(
            IUserRepository userRepository,
            IPolicyRepository policyRepository,
            IClaimRepository claimRepository,
            IHospitalRepository hospitalRepository,
            INotificationRepository notificationRepository)
        {
            _userRepository = userRepository;
            _policyRepository = policyRepository;
            _claimRepository = claimRepository;
            _hospitalRepository = hospitalRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
             if (!Enum.TryParse<UserRole>(role, true, out var parsedRole))
                throw new ArgumentException("Invalid role");

            return await _userRepository.FindAsync(u => u.Role == parsedRole);
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<string> UpdateUserRoleAsync(int id, UpdateUserRoleDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var newRole))
                 throw new ArgumentException("Invalid role");

            user.Role = newRole;
            
            await _userRepository.UpdateAsync(user);
            return "User role updated successfully";
        }

        public async Task<string> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
            
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {

                var existing = await _userRepository.GetByEmailAsync(dto.Email);
                if (existing != null) throw new Exception("Email already in use");
                user.Email = dto.Email;
            }

            await _userRepository.UpdateAsync(user);
            return "User updated successfully";
        }

        public async Task<string> ToggleUserStatusAsync(int id, bool isActive)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            user.IsActive = isActive;
            await _userRepository.UpdateAsync(user);
            return isActive ? "User activated" : "User deactivated";
        }

        public async Task<string> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            if (user.Email == "admin@health.com" || user.Role == UserRole.Admin)
                throw new Exception("System administrator cannot be deleted.");

            var hasPolicies = await _policyRepository.ExistsAsync(p => p.UserId == id || p.AgentId == id);
            if (hasPolicies)
                 throw new Exception("Cannot delete user because they have associated policies. Please delete the policies first.");

            var hasClaims = await _claimRepository.ExistsAsync(c => c.UserId == id || c.ReviewedBy == id);
            if (hasClaims)
                throw new Exception("Cannot delete user with associated claims.");

            if (user.Role == UserRole.HospitalManager)
            {
                var isLinkedToHospital = await _hospitalRepository.ExistsAsync(h => h.UserId == id);
                if (isLinkedToHospital)
                     throw new Exception("Cannot delete a Hospital Manager who is assigned to a hospital.");
            }

            var notifications = await _notificationRepository.FindAsync(n => n.UserId == id);
            foreach (var note in notifications)
            {
                await _notificationRepository.DeleteAsync(note.NotificationId);
            }

            await _userRepository.DeleteAsync(id);
            return "User deleted successfully";
        }

        public async Task<PagedResultDto<User>> GetPagedUsersAsync(int page, int pageSize, string sortColumn, bool isAscending, string? searchTerm = null)
        {
            System.Linq.Expressions.Expression<Func<User, bool>> filter = null;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                filter = u => u.Email.ToLower().Contains(term) || 
                              (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                              (u.LastName != null && u.LastName.ToLower().Contains(term));
            }

            var result = await _userRepository.GetPagedAsync(page, pageSize, sortColumn, isAscending, filter);
            return new PagedResultDto<User>
            {
                Items = result.Items,
                TotalCount = result.TotalCount
            };
        }
    }
}
