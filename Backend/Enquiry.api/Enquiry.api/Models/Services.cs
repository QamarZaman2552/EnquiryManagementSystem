using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enquiry.api.Models
{
    public class Services
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int serviceId { get; set; }

        public string serviceName { get; set; }

        public DateTime createdate { get; set; }

        public bool IsActive { get; set; }

        public double rate { get; set; }


    }
}
