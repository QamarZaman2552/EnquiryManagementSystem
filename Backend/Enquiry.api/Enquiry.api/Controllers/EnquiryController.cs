using Microsoft.AspNetCore.Mvc;
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

    // GET: api/EnquiryMaster
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
                createdate = e.EnquiryDate
            })
            .ToListAsync();

        return Ok(enquiries);
    }

    // GET: api/EnquiryMaster/5
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

    // PUT: api/EnquiryMaster/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

    // POST: api/EnquiryMaster
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<EnquiryMaster>> PostEnquiryMaster(EnquiryDto dto)
    {
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

        _context.EnquiryMasters.Add(enquirymaster);
        await _context.SaveChangesAsync();

        return Ok(enquirymaster);
    }

    // DELETE: api/EnquiryMaster/5
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
