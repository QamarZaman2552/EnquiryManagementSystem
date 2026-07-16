using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enquiry.api.Models
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password hash is required")]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(20)]
        public string Role { get; set; } = "Admin";

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? LockoutEnd { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
