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
                    subject: "We've Received Your Enquiry – EnquiryPro",
                    body: $@"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'></head>
<body style='margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0'>
    <tr><td align='center' style='padding:40px 16px;'>
      <table width='560' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.06);'>
        <!-- Header -->
        <tr>
          <td style='background:linear-gradient(135deg,#2563eb,#06b6d4);padding:36px 32px;text-align:center;'>
            <div style='font-size:48px;margin-bottom:8px;'>&#128222;</div>
            <h1 style='color:#fff;margin:0;font-size:24px;font-weight:700;'>Thank You, {dto.fullName}!</h1>
            <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:15px;'>Your enquiry has been received successfully.</p>
          </td>
        </tr>
        <!-- Body -->
        <tr><td style='padding:32px;'>
          <p style='color:#334155;font-size:15px;line-height:1.6;margin:0 0 20px;'>
            We've received your request and our team will review it shortly. Here's a summary of what you submitted:
          </p>
          <table width='100%' cellpadding='0' cellspacing='0' style='background:#f8fafc;border-radius:12px;overflow:hidden;'>
            <tr><td style='padding:16px 20px;border-bottom:1px solid #e2e8f0;'>
              <span style='color:#64748b;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;'>Subject</span>
              <div style='color:#0f172a;font-size:15px;font-weight:600;margin-top:2px;'>{dto.subject}</div>
            </td></tr>
            <tr><td style='padding:16px 20px;border-bottom:1px solid #e2e8f0;'>
              <span style='color:#64748b;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;'>Message</span>
              <div style='color:#334155;font-size:14px;margin-top:2px;line-height:1.5;'>{dto.message}</div>
            </td></tr>
            <tr><td style='padding:16px 20px;'>
              <span style='color:#64748b;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;'>Status</span>
              <div><span style='display:inline-block;padding:4px 14px;border-radius:20px;font-size:13px;font-weight:600;background:#fef3c7;color:#92400e;margin-top:2px;'>Pending Review</span></div>
            </td></tr>
          </table>
          <p style='color:#64748b;font-size:14px;line-height:1.6;margin:24px 0 0;padding:16px;background:#eff6ff;border-radius:10px;'>
            <strong style='color:#2563eb;'>&#128161; What happens next?</strong><br/>
            Our team will review your enquiry and contact you at <strong>{dto.email}</strong> within <strong>24 hours</strong>.
          </p>
        </td></tr>
        <!-- Footer -->
        <tr>
          <td style='padding:24px 32px;background:#f8fafc;text-align:center;border-top:1px solid #e2e8f0;'>
            <p style='color:#94a3b8;font-size:12px;margin:0;'>This is an automated message from <strong>EnquiryPro</strong>. Please do not reply to this email.</p>
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>"
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
