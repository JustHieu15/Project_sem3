
using AspnetCoreMvcStarter.Models;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcStarter.Areas.Admin.Context
{
  public class ApplicationDbContext : DbContext
  {
      public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
      {

      }
      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
        base.OnModelCreating(modelBuilder);

      //Congifure table User
      modelBuilder.Entity<User>().HasKey(x => x.Id);
      modelBuilder.Entity<User>().Property(x => x.Phone).IsRequired();
      modelBuilder.Entity<User>().Property(x => x.Email).IsRequired();
      modelBuilder.Entity<User>().Property(x => x.Password).IsRequired();

      //Congifure table Sms_logs
      modelBuilder.Entity<SmsLogs>().HasKey(s => s.Id);
      modelBuilder.Entity<SmsLogs>().HasOne(i => i.User).WithMany(u => u.SmsLogs).HasForeignKey(i => i.UserId);
      modelBuilder.Entity<SmsLogs>().HasOne(i => i.Invoices).WithMany(u => u.Smslogs).HasForeignKey(i => i.InvoicesId).OnDelete(DeleteBehavior.Restrict);
      //Congifure table Services
      modelBuilder.Entity<Services>().HasKey(s => s.Id);

      //Congifure table Invoices
      modelBuilder.Entity<Invoices>().HasKey(i => i.Id);
      modelBuilder.Entity<Invoices>().HasOne(i => i.User).WithMany(u => u.Invoices).HasForeignKey(i => i.UserId);
      modelBuilder.Entity<Invoices>().HasOne<Services>(i => i.Services).WithMany(s => s.Invoices).HasForeignKey(ii => ii.ServicesId);

      //Congifure table InvoicesItem
      modelBuilder.Entity<InvoicesItem>().HasKey(ii => ii.Id);
      modelBuilder.Entity<InvoicesItem>().HasOne(ii => ii.Invoice).WithMany(i => i.InvoicesItems).HasForeignKey(ii => ii.InvoiceId);

    }
      public DbSet<User> Users { get; set; }
      public DbSet<Invoices> Invoices { get; set; }
      public DbSet<InvoicesItem> InvoicesItems { get; set; }
      public DbSet<Services> Services { get; set; }
      public DbSet<SmsLogs> SmsLogs { get; set; }
  }
}
