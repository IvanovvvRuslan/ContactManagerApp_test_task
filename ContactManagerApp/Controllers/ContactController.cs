using ContactManagerApp.DTO;
using ContactManagerApp.Models;
using ContactManagerApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContactManagerApp.Controllers;

[ApiController]
[Route("contacts")]
public class ContactController: ControllerBase
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IContactService contactService, ILogger<ContactController> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ContactDto>>> GetAllContactsPagedAsync(
        [FromQuery] PaginationRequest request)
    {
        _logger.LogInformation("Retrieving paged announcements: Page {PageNumber}, Size {PageSize}", 
            request.PageNumber, request.PageSize);
        
        var contacts = await _contactService.GetContactsPagedAsync(request);
        
        _logger.LogInformation("Successfully retrieved {TotalCount} contacts", contacts.TotalCount);
        
        return Ok(contacts);
    }

    [HttpPost]
    public async Task<IActionResult> CreateContactAsync([FromBody]ContactDto contactDto)
    {
        _logger.LogInformation("Creating a new contact with name {Name}",  contactDto.Name);
        
        await _contactService.CreateContactAsync(contactDto);
        
        _logger.LogInformation("Successfully created a new contact with name {Name}",  contactDto.Name);
        
        return Ok("Contact created");
    }
}