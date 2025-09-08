using ContactManagerApp.DTO;
using ContactManagerApp.Models;
using ContactManagerApp.Repository;
using Mapster;

namespace ContactManagerApp.Services;

public interface IContactService
{
    Task<PagedResult<ContactDto>> GetContactsPagedAsync(PaginationRequest request);
    Task CreateContactAsync(ContactDto contactDto);
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

    public async Task<PagedResult<ContactDto>> GetContactsPagedAsync(PaginationRequest request)
    {
        _logger.LogDebug("Starting GetPagedAsync: Page {PageNumber}, Size {PageSize}", 
            request.PageNumber, request.PageSize);
        
        var pagedResult = await _repository.GetContactsPagedAsync(request);
        
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

    public async Task CreateContactAsync(ContactDto contactDto)
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
        
        await _repository.CreateContactAsync(newContact);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Successfully created contact with Id {Id} and Name {Name}", 
            newContact.Id, newContact.Name);
    }
}