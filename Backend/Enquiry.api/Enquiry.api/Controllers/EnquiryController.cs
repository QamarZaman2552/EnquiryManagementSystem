using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Enquiry.api.Models;
using Enquiry.api.EmailService;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Route("api/[controller]")]
[ApiController]
public class EnquiryController : ControllerBase
{
    private readonly EnquiryDbContext _context;
    private readonly EmailService _email;
    public EnquiryController(EnquiryDbContext context, EmailService email)
    {
        _context = context;
        _email = email;
    }

    // GET: api/EnquiryMaster  (Admin only - requires auth)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<object>> GetEnquiryMaster(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.EnquiryMasters
            .Include(e => e.Service)
            .OrderByDescending(e => e.EnquiryDate);

        var total = await query.CountAsync();
        var enquiries = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                id = e.EnquiryId,
                fullName = e.CustomerName,
                email = !string.IsNullOrEmpty(e.Email) ? e.Email : "N/A",
                mobile = e.MobilenNo,
                subject = !string.IsNullOrEmpty(e.Subject) ? e.Subject : "N/A",
                message = !string.IsNullOrEmpty(e.Message) ? e.Message : "N/A",
                serviceId = e.serviceId,
                serviceName = e.Service != null ? e.Service.serviceName : "N/A",
                status = e.Status,
                createdate = e.EnquiryDate
            })
            .ToListAsync();

        return Ok(new
        {
            data = enquiries,
            total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    // GET: api/EnquiryMaster/5  (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpGet("{enquiryid}")]
    public async Task<ActionResult<EnquiryMaster>> GetEnquiryMaster(int enquiryid)
    {
        var enquirymaster = await _context.EnquiryMasters.FindAsync(enquiryid);

        if (enquirymaster == null)
        {
            return NotFound();
        }

        return enquirymaster;
    }

    // PUT: api/EnquiryMaster/5  (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpPut("{enquiryid}")]
    public async Task<IActionResult> PutEnquiryMaster(int? enquiryid, EnquiryMaster enquirymaster)
    {
        if (enquiryid != enquirymaster.EnquiryId)
        {
            return BadRequest();
        }

        _context.Entry(enquirymaster).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EnquiryMasterExists(enquiryid))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/EnquiryMaster  (Public - users submit enquiries without login)
    [HttpPost]
    public async Task<ActionResult<EnquiryMaster>> PostEnquiryMaster(EnquiryDto dto)
    {
        // Validate serviceId: must be > 0 and must exist in Services table
        if (dto.serviceId <= 0)
            return BadRequest(new { message = "Please select a valid service." });

        var serviceExists = await _context.Services.AnyAsync(s => s.serviceId == dto.serviceId);
        if (!serviceExists)
            return BadRequest(new { message = $"Service with ID {dto.serviceId} does not exist." });

        var enquirymaster = new EnquiryMaster
        {
            CustomerName = dto.fullName,
            MobilenNo = dto.mobile,
            city = "Default",
            EnquiryDate = DateTime.Now,
            Status = "Pending",
            serviceId = dto.serviceId,
            Email = dto.email,
            Subject = dto.subject,
            Message = dto.message
        };

        try
        {
            _context.EnquiryMasters.Add(enquirymaster);
            await _context.SaveChangesAsync();

            try
            {
                await _email.SendAsync(
                    toEmail: dto.email,
                    subject: "Enquiry Received – EnquiryPro",
                    body: $@"
                        <h2>Thank You, {dto.fullName}!</h2>
                        <p>We have received your enquiry and will get back to you shortly.</p>
                        <table style='border-collapse:collapse;width:100%;max-width:500px;'>
                            <tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold;'>Subject</td><td style='padding:8px;border:1px solid #ddd;'>{dto.subject}</td></tr>
                            <tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold;'>Message</td><td style='padding:8px;border:1px solid #ddd;'>{dto.message}</td></tr>
                            <tr><td style='padding:8px;border:1px solid #ddd;font-weight:bold;'>Status</td><td style='padding:8px;border:1px solid #ddd;'>Pending</td></tr>
                        </table>
                        <p style='margin-top:20px;font-size:0.85em;color:#666;'>This is an automated confirmation. Please do not reply to this email.</p>"
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send confirmation email to {Email}", dto.email);
            }

            return Ok(enquirymaster);
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { message = "Failed to save enquiry.", detail = ex.InnerException?.Message });
        }
    }

    // PATCH: api/EnquiryMaster/status/5  (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpPatch("status/{enquiryid}")]
    public async Task<IActionResult> UpdateStatus(int enquiryid, [FromBody] StatusUpdateDto dto)
    {
        var enquiry = await _context.EnquiryMasters.FindAsync(enquiryid);
        if (enquiry == null) return NotFound(new { message = "Enquiry not found." });

        var allowed = new[] { "Pending", "In Progress", "Resolved", "Closed" };
        if (!allowed.Contains(dto.Status))
            return BadRequest(new { message = "Invalid status value." });

        enquiry.Status = dto.Status;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Status updated.", status = enquiry.Status });
    }

    // DELETE: api/EnquiryMaster/5  (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpDelete("{enquiryid}")]
    public async Task<IActionResult> DeleteEnquiryMaster(int? enquiryid)
    {
        var enquirymaster = await _context.EnquiryMasters.FindAsync(enquiryid);
        if (enquirymaster == null)
        {
            return NotFound();
        }

        _context.EnquiryMasters.Remove(enquirymaster);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool EnquiryMasterExists(int? enquiryid)
    {
        return _context.EnquiryMasters.Any(e => e.EnquiryId == enquiryid);
    }
}

public class EnquiryDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100)]
    public string fullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(200)]
    public string email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required")]
    [StringLength(20)]
    public string mobile { get; set; } = string.Empty;

    [StringLength(200)]
    public string subject { get; set; } = string.Empty;

    [StringLength(2000)]
    public string message { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid service")]
    public int serviceId { get; set; }
}

public class StatusUpdateDto
{
    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;
}
