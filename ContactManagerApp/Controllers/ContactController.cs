using ContactManagerApp.DTO;
using ContactManagerApp.Models;
using ContactManagerApp.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ContactManagerApp.Controllers;

[ApiController]
[Route("contacts")]
public class ContactController: ControllerBase
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactController> _logger;
    private readonly IValidator<ContactDto> _validator;

    public ContactController(IContactService contactService, ILogger<ContactController> logger, IValidator<ContactDto> validator)
    {
        _contactService = contactService;
        _logger = logger;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ContactDto>>> GetAllPagedAsync(
        [FromQuery] PaginationRequest request)
    {
        _logger.LogInformation("Retrieving paged announcements: Page {PageNumber}, Size {PageSize}", 
            request.PageNumber, request.PageSize);
        
        var contacts = await _contactService.GetPagedAsync(request);
        
        _logger.LogInformation("Successfully retrieved {TotalCount} contacts", contacts.TotalCount);
        
        return Ok(contacts);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody]ContactDto contactDto)
    {
        var result = await _validator.ValidateAsync(contactDto);

        if (!result.IsValid)
        {
            return BadRequest(result.Errors.Select(x => new
                {
                    field = x.ErrorMessage,
                    message = x.ErrorMessage
                })
            );
        }
        
        _logger.LogInformation("Creating a new contact with name {Name}",  contactDto.Name);
        
        await _contactService.CreateAsync(contactDto);
        
        _logger.LogInformation("Successfully created a new contact with name {Name}",  contactDto.Name);
        
        return Ok("Contact created");
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] ContactDto contactDto)
    {
        var result = await _validator.ValidateAsync(contactDto);

        if (!result.IsValid)
        {
            return BadRequest(result.Errors.Select(x => new
                {
                    field = x.ErrorMessage,
                    message = x.ErrorMessage
                })
            );
        }
        
        _logger.LogInformation("Updating a contact with name {Name}",  contactDto.Name);
        
        await _contactService.UpdateAsync(id, contactDto);
        
        _logger.LogInformation("Successfully updated a contact with name {Name}", contactDto.Name);
        
        return Ok("Contact updated");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
        _logger.LogInformation("Deleting a contact with id {Id}", id);
        
        await _contactService.DeleteAsync(id);
        
        _logger.LogInformation("Successfully deleted a contact with id {Id}", id);
        
        return Ok("Contact deleted");
    }
}