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

            // Seed default admin user (password: admin123, hashed with BCrypt)
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Username = "admin",
                PasswordHash = "$2a$11$bTOrJpAibsvXQDUtE.CGv.2FxLB.1PejP01dlb23kn3yDPcRVX8NO",
                Role = "Admin"
            });
        }
    }
}
