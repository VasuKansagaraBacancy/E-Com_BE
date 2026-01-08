using E_Commerce.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).HasMaxLength(500);
                entity.Property(e => e.GoogleId).HasMaxLength(200);
                entity.HasIndex(e => e.GoogleId).IsUnique().HasFilter("[GoogleId] IS NOT NULL");
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            });

            // PasswordResetOtp configuration
            modelBuilder.Entity<PasswordResetOtp>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Otp);
                entity.Property(e => e.Otp).IsRequired().HasMaxLength(6);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.PasswordResetOtps)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}


