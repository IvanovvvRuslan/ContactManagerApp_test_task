using ContactManagerApp.DTO;
using ContactManagerApp.Models;
using ContactManagerApp.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ContactManagerApp.Controllers;

public class ContactController: Controller
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

   public async Task<IActionResult> Index(PaginationRequest request)
    {
        _logger.LogInformation("Retrieving paged announcements: Page {PageNumber}, Size {PageSize}", 
            request.PageNumber, request.PageSize);
        
        var contacts = await _contactService.GetPagedAsync(request);
        
        _logger.LogInformation("Successfully retrieved {TotalCount} contacts", contacts.TotalCount);
        
        return View(contacts);
    }

    public IActionResult Create()
    {
        return View();
    }
   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContactDto contactDto)
    {
        var result = await _validator.ValidateAsync(contactDto);

        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(contactDto);
        }
        
        _logger.LogInformation("Creating a new contact with name {Name}",  contactDto.Name);
        
        await _contactService.CreateAsync(contactDto);
        
        _logger.LogInformation("Successfully created a new contact with name {Name}",  contactDto.Name);
        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var contact = await _contactService.GetByIdTrackedAsync(id);
        return View(contact);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContactDto contactDto)
    {
        var result = await _validator.ValidateAsync(contactDto);

        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(contactDto);
        }
        
        _logger.LogInformation("Updating a contact with name {Name}",  contactDto.Name);
        
        await _contactService.UpdateAsync(id, contactDto);
        
        _logger.LogInformation("Successfully updated a contact with name {Name}", contactDto.Name);
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting a contact with id {Id}", id);
        
        await _contactService.DeleteAsync(id);
        
        _logger.LogInformation("Successfully deleted a contact with id {Id}", id);
        
        return RedirectToAction(nameof(Index));
    }
}