using ContactManagerApp.DTO;
using ContactManagerApp.Exceptions;
using ContactManagerApp.Models;
using ContactManagerApp.Repository;
using Mapster;

namespace ContactManagerApp.Services;

public interface IContactService
{
    Task<IEnumerable<ContactDto>> GetAllAsync();
    Task<PagedResult<ContactDto>> GetPagedAsync(PaginationRequest request);
    Task<ContactDto> GetByIdTrackedAsync(int id);
    Task CreateAsync(ContactDto contactDto);
    Task UpdateAsync(int id, ContactDto contactDto);
    Task DeleteAsync(int id);
}

public class ContactService:  IContactService
{
    private readonly IContactRepository _repository;
    private readonly ILogger<ContactService> _logger;

    public ContactService(IContactRepository repository, ILogger<ContactService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<ContactDto>> GetAllAsync()
    {
        _logger.LogInformation("Getting all contacts");
        
        var contacts = await _repository.GetAllAsync();
        
        _logger.LogDebug("Repository returned {ItemCount} contacts out of {TotalCount} total", 
            contacts.Count());
        
        var contactsDto = contacts.Adapt<List<ContactDto>>();
        
        return contactsDto;
    }

    public async Task<PagedResult<ContactDto>> GetPagedAsync(PaginationRequest request)
    {
        _logger.LogDebug("Starting GetPagedAsync: Page {PageNumber}, Size {PageSize}", 
            request.PageNumber, request.PageSize);
        
        var pagedResult = await _repository.GetPagedAsync(request);
        
        _logger.LogDebug("Repository returned {ItemCount} contacts out of {TotalCount} total", 
            pagedResult.Items.Count(), pagedResult.TotalCount);

        var result = new PagedResult<ContactDto>
        {
            Items = pagedResult.Items.Adapt<List<ContactDto>>(),
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
        
        _logger.LogInformation("Successfully retrieved page {PageNumber} with {ItemCount} contacts", 
            result.PageNumber, result.Items.Count);
        
        return result;
    }

    public async Task<ContactDto> GetByIdTrackedAsync(int id)
    {
        _logger.LogDebug("Starting GetByAsync: id {Id}", id);
        
        var contact = await _repository.GetByIdTrackedAsync(id);

        if (contact == null)
            throw new NotFoundException("Contact not found");
        
        _logger.LogInformation("Successfully retrieved contact with id {Id}", id);
        
        var contactDto = contact.Adapt<ContactDto>();
        
        return contactDto;
    }

    public async Task CreateAsync(ContactDto contactDto)
    {
        _logger.LogDebug("Starting CreateAsync for contact with name: '{Name}'", 
            contactDto.Name);

        var newContact = new Contact
        {
            Name = contactDto.Name,
            BirthDate = contactDto.BirthDate,
            IsMarried = contactDto.IsMarried,
            PhoneNumber = contactDto.PhoneNumber,
            Salary = contactDto.Salary
        };
        
        await _repository.CreateAsync(newContact);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Successfully created contact with Id {Id} and Name {Name}", 
            newContact.Id, newContact.Name);
    }

    public async Task UpdateAsync(int id, ContactDto contactDto)
    {
        _logger.LogDebug("Starting UpdateAsync for contact with Id {Id}", id);

        var oldContact = await _repository.GetByIdTrackedAsync(id);

        if (oldContact == null)
            throw new NotFoundException("Contact not found");
        
        _logger.LogDebug("Updating contact with Id {Id}", id);
        
        oldContact.Name = contactDto.Name;
        oldContact.BirthDate = contactDto.BirthDate;
        oldContact.IsMarried = contactDto.IsMarried;
        oldContact.PhoneNumber = contactDto.PhoneNumber;
        oldContact.Salary = contactDto.Salary;
        
        _logger.LogDebug("Successfully updated contact with Id {Id}", id);
        
        await _repository.SaveChangesAsync();
        
        _logger.LogInformation("Successfully updated contact with Id {Id}", id);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogDebug("Starting DeleteAsync for contact with Id {Id}", id);
        
        var contact = await _repository.GetByIdAsync(id);
        
        if (contact == null)
            throw new NotFoundException("Contact not found");
        
        _logger.LogDebug("Found contact with Id {Id}", id);
        
        _repository.Delete(contact);
        await _repository.SaveChangesAsync();
        
        _logger.LogInformation("Successfully deleted contact with Id {Id}", id);
    }
}