using Microsoft.EntityFrameworkCore;
using SmartHealthInsurance.Api.Data;
using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.Enums;
using SmartHealthInsurance.Api.DTOs;
using System.Linq;
using System.Linq.Expressions;

namespace SmartHealthInsurance.Api.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<User> GetByEmailAsync(string email)
        {
            var cleanEmail = email.Trim().ToLower();

            return await _context.Users.FirstOrDefaultAsync(u => u.Email.Trim().ToLower() == cleanEmail);
        }
    }

    public class PolicyRepository : Repository<Policy>, IPolicyRepository
    {
        public PolicyRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Policy>> GetByUserIdAsync(int userId)
        {
            return await _context.Policies
                .Include(p => p.Plan)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<Policy> GetActivePolicyByUserIdAsync(int userId)
        {
             return await _context.Policies
                .Include(p => p.Plan)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Status == PolicyStatus.Active);
        }

        public async Task<IEnumerable<Policy>> GetByAgentIdAsync(int agentId)
        {
            return await _context.Policies
                .Include(p => p.Plan)
                .Include(p => p.User)
                .Where(p => p.AgentId == agentId)
                .ToListAsync();
        }

        public async Task<Policy> GetPolicyWithPlanAsync(int id)
        {
             return await _context.Policies
                .Include(p => p.Plan)
                .FirstOrDefaultAsync(p => p.PolicyId == id);
        }

        public async Task<IEnumerable<PolicyDistributionDto>> GetPolicyDistributionReportAsync()
        {
             return await _context.Policies
                 .Include(p => p.Plan)
                 .GroupBy(p => new { p.Plan!.PlanName, p.Status })
                 .Select(g => new PolicyDistributionDto
                 {
                     PolicyType = g.Key.PlanName,
                     Status = g.Key.Status,
                     Count = g.Count()
                 })
                 .ToListAsync();
        }

        public override async Task<IEnumerable<Policy>> GetAllAsync()
        {
             return await _context.Policies
                 .Include(p => p.Plan)
                 .Include(p => p.User)
                 .ToListAsync();
        }

        public override async Task<(IEnumerable<Policy> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string sortColumn, bool isAscending, Expression<Func<Policy, bool>>? filter = null)
        {
            var query = _context.Policies
                .Include(p => p.Plan)
                .Include(p => p.User)
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(sortColumn))
            {

                var parameter = System.Linq.Expressions.Expression.Parameter(typeof(Policy), "x");
                var property = System.Linq.Expressions.Expression.Property(parameter, sortColumn);
                var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);

                var methodName = isAscending ? "OrderBy" : "OrderByDescending";
                var resultExpression = System.Linq.Expressions.Expression.Call(
                    typeof(Queryable), 
                    methodName, 
                    new Type[] { typeof(Policy), property.Type }, 
                    query.Expression, 
                    System.Linq.Expressions.Expression.Quote(lambda));

                query = query.Provider.CreateQuery<Policy>(resultExpression);
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }

    public class ClaimRepository : Repository<Claim>, IClaimRepository
    {
        public ClaimRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Claim>> GetByUserIdAsync(int userId)
        {
            return await _context.Claims
                .Include(c => c.Policy)
                    .ThenInclude(p => p.Plan)
                .Include(c => c.Hospital)
                .Include(c => c.TreatmentRecord)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Claim>> GetByStatusAsync(string status)
        {

             if (Enum.TryParse<ClaimStatus>(status, true, out var claimStatus))
             {
                 return await _context.Claims
                    .Include(c => c.Policy)
                    .Include(c => c.User)
                    .Where(c => c.Status == claimStatus)
                    .ToListAsync();
             }
             return new List<Claim>();
        }
        
        public override async Task<IEnumerable<Claim>> GetAllAsync()
        {
             return await _context.Claims
                .Include(c => c.Policy)
                    .ThenInclude(p => p.Plan)
                .Include(c => c.User)
                .Include(c => c.Hospital)
                .Include(c => c.TreatmentRecord)
                .ToListAsync();
        }
        
        public override async Task<Claim> GetByIdAsync(int id)
        {
             return await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.User)
                .Include(c => c.Hospital)
                .Include(c => c.TreatmentRecord)
                .FirstOrDefaultAsync(c => c.ClaimId == id);
        }

        public override async Task<(IEnumerable<Claim> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string sortColumn, bool isAscending, Expression<Func<Claim, bool>>? filter = null)
        {
            var query = _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.User)
                .Include(c => c.Hospital)
                .Include(c => c.TreatmentRecord)
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(typeof(Claim), "x");
                var property = System.Linq.Expressions.Expression.Property(parameter, sortColumn);
                var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);

                var methodName = isAscending ? "OrderBy" : "OrderByDescending";
                var resultExpression = System.Linq.Expressions.Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(Claim), property.Type },
                    query.Expression,
                    System.Linq.Expressions.Expression.Quote(lambda));

                query = query.Provider.CreateQuery<Claim>(resultExpression);
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<ClaimAnalyticsDto>> GetClaimsAnalyticsReportAsync()
        {
             return await _context.Claims
                 .Include(c => c.Hospital)
                 .GroupBy(c => new { c.Status, c.Hospital!.HospitalName })
                 .Select(g => new ClaimAnalyticsDto
                 {
                     Status = g.Key.Status,
                     HospitalName = g.Key.HospitalName,
                     TotalAmount = g.Sum(x => x.ClaimAmount ?? 0m),
                     TotalApprovedAmount = g.Sum(x => x.ApprovedAmount),
                     Count = g.Count()
                 })
                 .ToListAsync();
        }

        public async Task<IEnumerable<HighValueClaimDto>> GetHighValueClaimsAsync(decimal? threshold = null)
        {
            
            return await _context.Claims
                .Include(c => c.Hospital)
                .Include(c => c.User)
                .Include(c => c.Policy).ThenInclude(p => p.Plan)
                .Where(c => c.Policy != null && c.Policy.Plan != null && 
                           ((c.ApprovedAmount > 0 ? c.ApprovedAmount : (c.ClaimAmount ?? 0m)) > (c.Policy.Plan.CoverageLimit * 0.7m)))
                .Select(c => new HighValueClaimDto
                {
                    ClaimId = c.ClaimId,
                    ClaimNumber = c.ClaimNumber,
                    ClaimAmount = (c.ApprovedAmount > 0) ? c.ApprovedAmount : (c.ClaimAmount ?? 0m),
                    HospitalName = c.Hospital!.HospitalName,
                    Diagnosis = c.TreatmentDetails ?? "", 
                    SubmittedAt = c.SubmittedAt,
                    CustomerName = c.User != null ? c.User.FirstName + " " + c.User.LastName : "Unknown",
                    CoverageAmount = c.Policy != null && c.Policy.Plan != null ? c.Policy.Plan.CoverageLimit : 0m,
                    ValueType = c.ApprovedAmount > 0 ? "Approved" : "Requested"
                })
                .OrderByDescending(c => c.ClaimAmount)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPayoutsAsync(int? userId = null)
        {
             var query = _context.Claims.Where(c => c.Status == ClaimStatus.Paid);
             if (userId.HasValue)
             {
                 query = query.Where(c => c.UserId == userId.Value);
             }
             return await query.SumAsync(c => c.ApprovedAmount);
        }
    }

    public class HospitalRepository : Repository<Hospital>, IHospitalRepository
    {
        public HospitalRepository(ApplicationDbContext context) : base(context) { }
    }

    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetByUserIdAsync(int userId)
        {
            return await _context.Payments
                .Include(p => p.Policy)
                    .ThenInclude(p => p.Plan)
                .Where(p => p.Policy.UserId == userId)
                .ToListAsync();
        }


        public async Task<decimal> GetTotalPremiumsAsync(int? userId = null)
        {
            if (userId.HasValue)
            {
                 var policyIds = await _context.Policies
                     .Where(p => p.UserId == userId.Value)
                     .Select(p => p.PolicyId)
                     .ToListAsync();
                     
                 if (!policyIds.Any()) return 0;
                 
                 return await _context.Payments
                     .Where(p => policyIds.Contains(p.PolicyId) && p.PaymentType == PaymentType.Premium)
                     .SumAsync(p => p.Amount);
            }
            
            return await _context.Payments
                .Where(p => p.PaymentType == PaymentType.Premium)
                .SumAsync(p => p.Amount);
        }

        public override async Task<IEnumerable<Payment>> GetAllAsync()
        {
             return await _context.Payments
                 .Include(p => p.Policy)
                    .ThenInclude(p => p.Plan)
                 .ToListAsync();
        }
    }

    public class TreatmentRepository : Repository<TreatmentRecord>, ITreatmentRepository
    {
        public TreatmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<TreatmentRecord>> GetByHospitalIdAsync(int hospitalId)
        {
            return await _context.TreatmentRecords
                .Include(t => t.Policy)
                .Include(t => t.Customer)
                .Where(t => t.HospitalId == hospitalId)
                .ToListAsync();
        }
        
        public override async Task<TreatmentRecord> GetByIdAsync(int id)
        {
             return await _context.TreatmentRecords
                .Include(t => t.Hospital)
                .Include(t => t.Policy)
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(t => t.TreatmentId == id);
        }

        public override async Task<(IEnumerable<TreatmentRecord> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string sortColumn, bool isAscending, Expression<Func<TreatmentRecord, bool>>? filter = null)
        {
            var query = _context.TreatmentRecords
                .Include(t => t.Policy)
                .Include(t => t.Customer)
                .Include(t => t.Hospital)
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TreatmentRecord), "x");
                var property = System.Linq.Expressions.Expression.Property(parameter, sortColumn);
                var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);

                var methodName = isAscending ? "OrderBy" : "OrderByDescending";
                var resultExpression = System.Linq.Expressions.Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(TreatmentRecord), property.Type },
                    query.Expression,
                    System.Linq.Expressions.Expression.Quote(lambda));

                query = query.Provider.CreateQuery<TreatmentRecord>(resultExpression);
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
