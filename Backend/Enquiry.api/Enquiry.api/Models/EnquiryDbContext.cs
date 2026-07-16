using Microsoft.EntityFrameworkCore;

namespace Enquiry.api.Models
{
    public class EnquiryDbContext:DbContext
    {
        public EnquiryDbContext(DbContextOptions<EnquiryDbContext> options) : base(options)
        {

        }
        public DbSet<EnquiryMaster> EnquiryMasters { get; set; }
        public DbSet<Services> Services { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Username = "admin",
                PasswordHash = "$2a$11$LV0m9ncOZMUq77oyqNi8C.BbUAOskrSoLZ06wDEyI4hpLYdP1fUPW",
                Role = "Admin",
                FailedLoginAttempts = 0,
                LockoutEnd = null
            });
        }
    }
}