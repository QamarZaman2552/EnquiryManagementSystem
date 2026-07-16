using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enquiry.api.Models
{
    public class AuditLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [StringLength(500)]
        public string Details { get; set; } = string.Empty;

        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
