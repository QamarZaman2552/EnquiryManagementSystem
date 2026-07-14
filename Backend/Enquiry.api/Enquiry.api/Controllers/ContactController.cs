using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Enquiry.api.Models;
using System.ComponentModel.DataAnnotations;

namespace Enquiry.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly EnquiryDbContext _context;

        public ContactController(EnquiryDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ContactMessageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var contact = new ContactMessage
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone ?? string.Empty,
                Subject = dto.Subject,
                Message = dto.Message,
                CreatedAt = DateTime.Now
            };

            _context.ContactMessages.Add(contact);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message sent successfully" });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactMessage>>> GetAll()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return Ok(messages);
        }
    }

    public class ContactMessageDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}
