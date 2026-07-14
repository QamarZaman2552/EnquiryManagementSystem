using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enquiry.api.Models
{
    public class Services
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int serviceId { get; set; }

        [Required(ErrorMessage = "Service name is required")]
        [StringLength(200, ErrorMessage = "Service name cannot exceed 200 characters")]
        public string serviceName { get; set; } = string.Empty;

        public DateTime createdate { get; set; }

        public bool IsActive { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Rate must be a positive number")]
        public double rate { get; set; }
    }
}
