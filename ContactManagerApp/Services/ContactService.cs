using System.Globalization;
using ContactManagerApp.DTO;
using ContactManagerApp.Exceptions;
using ContactManagerApp.Models;
using ContactManagerApp.Repository;
using FluentValidation;
using Mapster;

namespace ContactManagerApp.Services;

public interface IContactService
{
    Task<IEnumerable<ContactDto>> GetAllAsync();
    Task<PagedResult<ContactDto>> GetPagedAsync(PaginationRequest request);
    Task<ContactDto> GetByIdTrackedAsync(int id);
    Task CreateAsync(ContactDto contactDto);
    Task UpdateAsync(int id, ContactDto contactDto);
    Task UpdateFieldAsync(int id, string fieldName, string fieldValue);
    Task DeleteAsync(int id);
}

public class ContactService:  IContactService
{
    private readonly IContactRepository _repository;
    private readonly ILogger<ContactService> _logger;
    private readonly IValidator<ContactDto> _validator;

    public ContactService(IContactRepository repository, ILogger<ContactService> logger, IValidator<ContactDto> validator)
    {
        _repository = repository;
        _logger = logger;
        _validator = validator;
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

    public async Task UpdateFieldAsync(int id, string fieldName, string fieldValue)
    {
        _logger.LogDebug("Starting UpdateFieldAsync for Id {Id}, field {FieldName}", id, fieldName);
        
        var contact = await _repository.GetByIdTrackedAsync(id);
        
        if (contact == null)
            throw new NotFoundException("Contact not found");

        var dto = new ContactDto();
        string[] propertiesToValidate = new string[] { };
        
        switch (fieldName)
        {
            case "Name":
                dto.Name = fieldValue;
                propertiesToValidate = new[] { nameof(dto.Name) };
                break;
            
            case "BirthDate":
                if (!DateTime.TryParse(fieldValue, out var birthDate))
                    throw new ArgumentException("Invalid date format");

                dto.BirthDate = birthDate;
                propertiesToValidate = new[] { nameof(dto.BirthDate) };
                break;

            case "IsMarried":
                contact.IsMarried = bool.Parse(fieldValue);
                break;

            case "PhoneNumber":
                dto.PhoneNumber = fieldValue;
                propertiesToValidate = new[] { nameof(dto.PhoneNumber) };
                break;
            
            case "Salary":
                if (!decimal.TryParse(fieldValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var salary))
                    throw new ArgumentException("Invalid decimal format");
                dto.Salary = salary;
                propertiesToValidate = new[] { nameof(dto.Salary) };
                break;
            
            default:
                throw new ArgumentException($"Unknown field: {fieldName}");
        }

        if (propertiesToValidate.Length > 0)
        {
            var validationResult = await _validator.ValidateAsync(dto, opt => opt.IncludeProperties(propertiesToValidate));
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }

        switch (fieldName)
        {
            case "Name": contact.Name = fieldValue; break;
            case "BirthDate": contact.BirthDate = dto.BirthDate; break;
            case "PhoneNumber": contact.PhoneNumber = dto.PhoneNumber; break;
            case "Salary": contact.Salary = dto.Salary; break;
        }
        
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Successfully updated field {FieldName} for contact {Id}", fieldName, id);
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