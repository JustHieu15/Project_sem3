using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcStarter.Models;
using AspnetCoreMvcStarter.Models;

namespace AspnetCoreMvcStarter.Areas.Admin.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Existing DbSets
        public DbSet<User> Users { get; set; }

        // New DbSets for KPI System
        public DbSet<KpiTask> KpiTasks { get; set; }
        public DbSet<TaskUpdate> TaskUpdates { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<MonthlyKpiReport> MonthlyKpiReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships for KpiTask
            modelBuilder.Entity<KpiTask>()
                .HasOne(k => k.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(k => k.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KpiTask>()
                .HasOne(k => k.ApprovedByUser)
                .WithMany(u => u.ApprovedTasks)
                .HasForeignKey(k => k.ApprovedByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Configure relationships for TaskUpdate
            modelBuilder.Entity<TaskUpdate>()
                .HasOne(tu => tu.Task)
                .WithMany(k => k.TaskUpdates)
                .HasForeignKey(tu => tu.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskUpdate>()
                .HasOne(tu => tu.UpdatedByUser)
                .WithMany(u => u.TaskUpdates)
                .HasForeignKey(tu => tu.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships for User and Department
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure relationships for MonthlyKpiReport
            modelBuilder.Entity<MonthlyKpiReport>()
                .HasOne(r => r.User)
                .WithMany(u => u.MonthlyReports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create unique index for monthly reports (one report per user per month/year)
            modelBuilder.Entity<MonthlyKpiReport>()
                .HasIndex(r => new { r.UserId, r.Month, r.Year })
                .IsUnique();

            // Configure decimal precision
            modelBuilder.Entity<MonthlyKpiReport>()
                .Property(r => r.CompletionRate)
                .HasColumnType("decimal(5,2)");
        }
    }
}
