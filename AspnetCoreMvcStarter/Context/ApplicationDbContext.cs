using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcStarter.Models;

namespace AspnetCoreMvcStarter.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets không thay đổi
        public DbSet<User> Users { get; set; }
        public DbSet<KpiTask> KpiTasks { get; set; }
        public DbSet<TaskUpdate> TaskUpdates { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<MonthlyKpiReport> MonthlyKpiReports { get; set; }
        public DbSet<JobType> JobTypes { get; set; }
		public DbSet<TaskDefinition> TaskDefinitions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<TaskDefinition>(entity =>
            {
                entity.HasOne(d => d.JobType)
                      .WithMany(p => p.TaskDefinitions)
                      .HasForeignKey(d => d.JobTypeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình JobType - không thay đổi
            modelBuilder.Entity<JobType>()
                .HasIndex(j => j.Code)
                .IsUnique()
                .HasFilter("[Code] IS NOT NULL");
            // ... (các cấu hình khác của JobType giữ nguyên)

            modelBuilder.Entity<TaskDefinition>(entity =>
            {
              entity.HasOne(d => d.JobType)
                .WithMany(p => p.TaskDefinitions)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Def thì xóa cả Task liên quan
            });

             modelBuilder.Entity<KpiTask>(entity =>
            {
                entity.HasOne(d => d.TaskDefinition)
                      .WithMany(p => p.KpiTasks)
                      .HasForeignKey(d => d.TaskDefinitionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(k => k.AssignedUser)
                      .WithMany(u => u.AssignedTasks)
                      .HasForeignKey(k => k.AssignedUserId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);

                entity.HasOne(k => k.Approver)
                      .WithMany(u => u.ApprovedTasks)
                      .HasForeignKey(k => k.ApproverId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                entity.HasOne(k => k.MainDepartment)
                      .WithMany()
                      .HasForeignKey(k => k.MainDepartmentId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });
            // Cấu hình TaskUpdate - không thay đổi
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


            // Cấu hình User và Department - không thay đổi
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);


            // Cấu hình MonthlyKpiReport - không thay đổi
            modelBuilder.Entity<MonthlyKpiReport>()
                .HasOne(r => r.User)
                .WithMany(u => u.MonthlyReports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MonthlyKpiReport>()
                .HasIndex(r => new { r.UserId, r.Month, r.Year })
                .IsUnique();

            modelBuilder.Entity<MonthlyKpiReport>()
                .Property(r => r.CompletionRate)
                .HasColumnType("decimal(5,2)");



            // Các index cho KpiTask
            modelBuilder.Entity<KpiTask>()
                .HasIndex(k => k.Status); // Vẫn còn

            modelBuilder.Entity<KpiTask>()
                .HasIndex(k => new { k.AssignedUserId, k.Status });

            // Các index cho JobType - không thay đổi
            modelBuilder.Entity<JobType>()
                .HasIndex(j => j.IsActive);

            modelBuilder.Entity<JobType>()
                .HasIndex(j => j.DisplayOrder);
        }
    }
}
