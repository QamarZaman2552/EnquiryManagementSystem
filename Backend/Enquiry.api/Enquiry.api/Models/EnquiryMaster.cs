using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enquiry.api.Models
{
    public class EnquiryMaster
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnquiryId { get; set; }

        public string CustomerName { get; set; }

        public string MobilenNo { get; set; }

        public string city { get; set; }

        public DateTime EnquiryDate { get; set; }

        public string Status { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public int? serviceId { get; set; }
        [ForeignKey("serviceId")]
        public Services? Service { get; set; }
    }
}
