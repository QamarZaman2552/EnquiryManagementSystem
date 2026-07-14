using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enquiry.api.Models
{
    public class EnquiryMaster
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnquiryId { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [StringLength(20, ErrorMessage = "Mobile number cannot exceed 20 characters")]
        public string MobilenNo { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string city { get; set; } = string.Empty;

        public DateTime EnquiryDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; } = string.Empty;

        public int? serviceId { get; set; }

        [ForeignKey("serviceId")]
        public Services? Service { get; set; }
    }
}
