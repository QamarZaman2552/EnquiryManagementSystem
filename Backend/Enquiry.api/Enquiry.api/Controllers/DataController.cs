using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Enquiry.api.Models;

namespace Enquiry.api.Controllers
{
    [Route("api/data")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly EnquiryDbContext _context;

        public DataController(EnquiryDbContext context)
        {
            _context = context;
        }

        [HttpPost("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportData([FromBody] DataRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email is required." });

            var enquiries = await _context.EnquiryMasters
                .Where(e => e.Email == dto.Email)
                .OrderByDescending(e => e.EnquiryDate)
                .Select(e => new
                {
                    e.EnquiryId,
                    e.CustomerName,
                    e.Email,
                    e.MobilenNo,
                    e.Subject,
                    e.Message,
                    e.Status,
                    e.EnquiryDate
                })
                .ToListAsync();

            var contactMessages = await _context.ContactMessages
                .Where(c => c.Email == dto.Email)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                email = dto.Email,
                enquiries,
                contactMessages,
                exportedAt = DateTime.UtcNow
            });
        }

        [HttpPost("delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteData([FromBody] DataRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email is required." });

            var enquiries = await _context.EnquiryMasters
                .Where(e => e.Email == dto.Email)
                .ToListAsync();
            _context.EnquiryMasters.RemoveRange(enquiries);

            var contactMessages = await _context.ContactMessages
                .Where(c => c.Email == dto.Email)
                .ToListAsync();
            _context.ContactMessages.RemoveRange(contactMessages);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "All personal data associated with this email has been deleted.",
                deletedEnquiries = enquiries.Count,
                deletedMessages = contactMessages.Count
            });
        }
    }

    public class DataRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
