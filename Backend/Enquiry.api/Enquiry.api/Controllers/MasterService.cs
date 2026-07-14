using Enquiry.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<List<Services>>> GetAllServices()
        {
            var services = await _context.Services.ToListAsync();
            return Ok(services);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddNewServices(Services obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            obj.serviceId = 0;
            obj.createdate = DateTime.Now;
            _context.Services.Add(obj);
            await _context.SaveChangesAsync();
            return Created($"service/{obj.serviceId}", obj);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{ServiceId}")]
        public async Task<IActionResult> UpdateServices(int ServiceId, Services obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var oldServices = await _context.Services.SingleOrDefaultAsync(x => x.serviceId == ServiceId);

            if (oldServices == null)
                return NotFound(new { message = $"Service with id {ServiceId} not found" });

            oldServices.serviceName = obj.serviceName;
            oldServices.rate = obj.rate;
            oldServices.IsActive = obj.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Service updated successfully", id = ServiceId });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{ServiceId}")]
        public async Task<IActionResult> ServiceDelbyId(int ServiceId)
        {
            var deloldservices = await _context.Services.SingleOrDefaultAsync(x => x.serviceId == ServiceId);
            if (deloldservices == null)
                return NotFound(new { message = $"Service with id {ServiceId} not found" });

            try
            {
                var relatedEnquiries = await _context.EnquiryMasters
                    .Where(e => e.serviceId == ServiceId)
                    .ToListAsync();

                foreach (var enquiry in relatedEnquiries)
                {
                    enquiry.serviceId = null;
                }

                _context.Services.Remove(deloldservices);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Service deleted successfully", id = ServiceId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error deleting service: {ex.Message}" });
            }
        }
    }
}
