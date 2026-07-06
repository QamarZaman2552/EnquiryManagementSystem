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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed default admin user (password: admin123, hashed with SHA256)
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Username = "admin",
                PasswordHash = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9", // SHA256 of admin123
                Role = "Admin"
            });
        }
    }
}
