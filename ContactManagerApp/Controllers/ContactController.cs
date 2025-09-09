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
    private readonly ICsvImportService _csvImportService;

    public ContactController(IContactService contactService, ILogger<ContactController> logger, IValidator<ContactDto> validator, 
        ICsvImportService csvImportService)
    {
        _contactService = contactService;
        _logger = logger;
        _validator = validator;
        _csvImportService = csvImportService;
    }

   public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Retrieving all contacts");
        
        var contacts = await _contactService.GetAllAsync();
        
        _logger.LogInformation("Successfully retrieved {TotalCount} contacts", contacts.Count());
        
        return View(contacts);
    }
   
    public async Task<IActionResult> Paginated(PaginationRequest request)
    {
        _logger.LogInformation("Retrieving paged contacts: Page {PageNumber}, Size {PageSize}", 
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

    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "No file selected.";
            return View();
        }
        
        var importResult = await _csvImportService.ImportContactsFromCsvAsync(file);

        if (!importResult.Success)
        {
            ViewBag.Error = "Some row(s) failed validation.";
            ViewBag.ValidationErrors = string.Join("<br/>", importResult.Errors);
            return View();
        }

        ViewBag.Success = "Contacts imported successfully! " +
                          $"<a href='{Url.Action("Index", "Contact")}'>Back to Home Page</a>";

        return View();
    }
    
    public IActionResult Privacy()
    {
        return View();
    }
}