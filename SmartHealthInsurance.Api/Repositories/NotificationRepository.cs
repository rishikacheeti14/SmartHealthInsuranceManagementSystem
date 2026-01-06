using Microsoft.EntityFrameworkCore;
using SmartHealthInsurance.Api.Data;
using SmartHealthInsurance.Api.Models;

namespace SmartHealthInsurance.Api.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetAllByUserIdAsync(int userId)
        {
             return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50) 
                .ToListAsync();
        }
    }
}
