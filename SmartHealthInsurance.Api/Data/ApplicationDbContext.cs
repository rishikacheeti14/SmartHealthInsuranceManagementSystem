using Microsoft.EntityFrameworkCore;
using SmartHealthInsurance.Api.Models;

namespace SmartHealthInsurance.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<InsurancePlan> InsurancePlans => Set<InsurancePlan>();
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<Hospital> Hospitals => Set<Hospital>();
        public DbSet<Claim> Claims => Set<Claim>();
        public DbSet<TreatmentRecord> TreatmentRecords { get; set; }

        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Claim>()
                .HasOne(c => c.TreatmentRecord)
                .WithMany()
                .HasForeignKey(c => c.TreatmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Hospital>()
        .HasOne(h => h.User)
        .WithMany()
        .HasForeignKey(h => h.UserId)
        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Policy>()
                .HasOne(p => p.User)
                .WithMany(u => u.Policies)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Policy>()
                .HasOne(p => p.Plan)
                .WithMany(ip => ip.Policies)
                .HasForeignKey(p => p.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Policy>()
     .Property(p => p.Status)
     .HasConversion<int>();

            modelBuilder.Entity<Claim>()
                .HasOne(c => c.User)
                .WithMany(u => u.Claims)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Hospital)
                .WithMany(h => h.Claims)
                .HasForeignKey(c => c.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Policy)
                .WithMany(pol => pol.Payments)
                .HasForeignKey(p => p.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Claim)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.ClaimId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<InsurancePlan>(entity =>
            {
                entity.Property(p => p.PremiumAmount)
                      .HasPrecision(18, 2);

                entity.Property(p => p.CoverageLimit)
                      .HasPrecision(18, 2);

              
            });

            modelBuilder.Entity<Claim>(entity =>
            {
                entity.Property(c => c.ClaimAmount)
                      .HasPrecision(18, 2);

                entity.Property(c => c.ApprovedAmount)
                      .HasPrecision(18, 2);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount)
                      .HasPrecision(18, 2);
            });

        }
    }
}
