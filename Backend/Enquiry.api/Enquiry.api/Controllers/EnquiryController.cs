using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Enquiry.api.Models;

[Route("api/[controller]")]
[ApiController]
public class EnquiryController : ControllerBase
{
    private readonly EnquiryDbContext _context;
    public EnquiryController(EnquiryDbContext context)
    {
        _context = context;
     }

    // GET: api/EnquiryMaster  (public - users don't need auth)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetEnquiryMaster()
    {
        var enquiries = await _context.EnquiryMasters
            .Include(e => e.Service)
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

        return Ok(enquiries);
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
    public string fullName { get; set; }
    public string email { get; set; }
    public string mobile { get; set; }
    public string subject { get; set; }
    public string message { get; set; }
    public int serviceId { get; set; }
}

public class StatusUpdateDto
{
    public string Status { get; set; } = string.Empty;
}
