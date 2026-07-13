using Enquiry.api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Enquiry.api.Controllers
{
    [Route("api/service")]
    [ApiController]
    public class MasterService : ControllerBase
    {
        private readonly EnquiryDbContext _context;
        public MasterService(EnquiryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public List<Services> GetAllServices()
        {
            var services = _context.Services.ToList();
            return services;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddNewServices(Services obj)
        {
            // Reset the ID to 0 so EF Core knows it's a new entity and doesn't try to insert an explicit Identity value.
            obj.serviceId = 0;
            // Automatically set the creation date to right now, so you don't have to enter it manually
            obj.createdate = DateTime.Now;
            _context.Services.Add(obj);
            _context.SaveChanges();
            return Created("service create suceesfully", obj);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{ServiceId}")]
        public IActionResult UpdateServices(int ServiceId, Services obj)
        {
            var oldServices = _context.Services.SingleOrDefault(x => x.serviceId == ServiceId);

            if (oldServices != null)
            {
                oldServices.serviceName = obj.serviceName;
                oldServices.rate = obj.rate;
                oldServices.IsActive = obj.IsActive;
                _context.SaveChanges();
                return Ok(new { message = "service updated with id " + ServiceId });
            }
            else
            {
                // Return a 404 Not Found if the service doesn't exist
                return NotFound(new { message = $"Service with id {ServiceId} not found" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{ServiceId}")]
        public IActionResult ServiceDelbyId(int ServiceId)
        {
            var deloldservices = _context.Services.SingleOrDefault(x => x.serviceId == ServiceId);
            if (deloldservices == null)
            {
                return NotFound(new { message = $"Service with id {ServiceId} not found" });
            }

            try
            {
                // Detach related enquiries first to avoid FK constraint violation
                var relatedEnquiries = _context.EnquiryMasters
                    .Where(e => e.serviceId == ServiceId)
                    .ToList();

                foreach (var enquiry in relatedEnquiries)
                {
                    enquiry.serviceId = null;
                }

                _context.Services.Remove(deloldservices);
                _context.SaveChanges();
                return Ok(new { message = "service deleted with id " + ServiceId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error deleting service: {ex.Message}" });
            }
        }
    }
}
