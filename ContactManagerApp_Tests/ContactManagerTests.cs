using ContactManagerApp.DTO;
using ContactManagerApp.Exceptions;
using ContactManagerApp.Models;
using ContactManagerApp.Repository;
using ContactManagerApp.Services;
using ContactManagerApp.Validators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ContactManager_Tests;

public class ContactManagerTests
{
    private readonly IContactRepository _repository;
    private readonly IContactService  _service;
    private readonly IValidator<ContactDto> _validator;

    public ContactManagerTests()
    {
        _repository = Substitute.For<IContactRepository>();
        var logger = Substitute.For<ILogger<ContactService>>();
        _validator = new ContactValidator();
        _service = new ContactService (_repository, logger, _validator);
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedResult()
    {
        //Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Name = "Name 1" },
            new Contact { Id = 2, Name = "Name 2" },
            new Contact { Id = 3, Name = "Name 3" }
        };
        
        _repository.GetAllAsync().Returns(contacts);
        
        //Act
        var result = await _service.GetAllAsync();
        
        //Assert
        var resultList = result.ToList();
        
        Assert.Equal(contacts.Count, resultList.Count);

        for (int i = 0; i < contacts.Count; i++)
        {
            Assert.Equal(contacts[i].Id, resultList[i].Id);
            Assert.Equal(contacts[i].Name, resultList[i].Name);
        }
    }
    
    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResult()
    {
        //Arrange
        var request = new PaginationRequest
        {
            PageNumber = 1,
            PageSize = 10,
        };
        
        var repositoryResult = new  PagedResult<Contact>
        {
            Items = new List<Contact>
            {
              new Contact {Id = 1, Name = "Name 1"},
              new Contact {Id = 2, Name = "Name 2"}  
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };
        
        _repository.GetPagedAsync(request).Returns(repositoryResult);
        
        //Act
        var result = await _service.GetPagedAsync(request);
        
        //Assert
        Assert.Equal(repositoryResult.TotalCount, result.TotalCount);
        Assert.Equal(repositoryResult.Items.Count, result.Items.Count);
        Assert.Equal(repositoryResult.PageNumber, result.PageNumber);
        Assert.Equal(repositoryResult.PageSize, result.PageSize);
    }
    
    [Fact]
    public async Task GetPagedAsync_ShouldCallRepositoryWithCorrectRequest()
    {
        // Arrange
        var request = new PaginationRequest
        {
            PageNumber = 2, 
            PageSize = 5
        };
        
        var repositoryResult = new PagedResult<Contact>
        {
            Items = new List<Contact>()
        };
        
        _repository.GetPagedAsync(request).Returns(repositoryResult);
    
        // Act
        await _service.GetPagedAsync(request);
    
        // Assert
        await _repository.Received(1).GetPagedAsync(request);
    }
    
    [Fact]
    public async Task GetPagedAsync_WhenNoItems_ShouldReturnEmptyResult()
    {
        // Arrange
        var request = new PaginationRequest
        {
            PageNumber = 1, 
            PageSize = 10
        };
        var repositoryResult = new PagedResult<Contact>
        {
            Items = new List<Contact>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };
    
        _repository.GetPagedAsync(request).Returns(repositoryResult);
    
        // Act
        var result = await _service.GetPagedAsync(request);
    
        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetByIdTrackedAsync_ShouldReturnSuccess()
    {
        //Arrange
        var contact = new Contact { Id = 1, Name = "Name 1" };
        
        _repository.GetByIdTrackedAsync(contact.Id).Returns(contact);
        
        //Act
        var result = await _service.GetByIdTrackedAsync(contact.Id);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal(contact.Id, result.Id);
        Assert.Equal(contact.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdTrackedAsync_ShouldThrowIfNotfound()
    {
        //Arrange
        int contactId = 1;
        
        _repository.GetByIdTrackedAsync(contactId).Returns((Contact)null);
        
        //Act & Assert
        await Assert.ThrowsAnyAsync<NotFoundException>(async () => await _service.GetByIdTrackedAsync(contactId));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateContact()
    {
        //Arrange
        var contactDto = new ContactDto
        {
            Name = "Name 1",
            BirthDate = new DateTime(2025, 1, 1),
            IsMarried = true,
            PhoneNumber = "+380677777777",
            Salary = 777.77m
        };
        
        //Act
        await _service.CreateAsync(contactDto);
        
        //Assert
        await _repository.Received(1).CreateAsync(Arg.Is<Contact>(c => 
            c.Name == contactDto.Name &&
            c.BirthDate == contactDto.BirthDate &&
            c.IsMarried == contactDto.IsMarried &&
            c.PhoneNumber == contactDto.PhoneNumber &&
            c.Salary == contactDto.Salary));
        
        await _repository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateContact()
    {
        //Arrange
        var oldContact = new Contact
        {
            Id = 1,
            Name = "Old  Name",
            BirthDate = new DateTime(2025, 1, 1),
            IsMarried = true,
            PhoneNumber = "+380677777777",
            Salary = 777.77m
        };
        
        _repository.GetByIdTrackedAsync(oldContact.Id).Returns(oldContact);

        var newContact = new ContactDto
        {
            Name = "New Name",
            BirthDate = new DateTime(2025, 2, 2),
            IsMarried = false,
            PhoneNumber = "+380505555555",
            Salary = 999.99m
        };
        
        //Act
        await _service.UpdateAsync(oldContact.Id, newContact);
        
        //Assert
        Assert.Equal(newContact.Name, oldContact.Name);
        Assert.Equal(newContact.BirthDate, oldContact.BirthDate);
        Assert.Equal(newContact.IsMarried, oldContact.IsMarried);
        Assert.Equal(newContact.PhoneNumber, oldContact.PhoneNumber);
        Assert.Equal(newContact.Salary, oldContact.Salary);
        
        await _repository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldThrowIfNotfound()
    {
        //Arrange
        int contactId = 1;
        var contactDto = new ContactDto();
        
        _repository.GetByIdAsync(contactId).Returns((Contact)null);
        
        //Act & Assert
        await Assert.ThrowsAnyAsync<NotFoundException>(async () => await _service.UpdateAsync(contactId, contactDto));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteSuccessfully()
    {
        // Arrange
        var existingContact = new Contact { Id = 1, Name = "Name 1" };
        
        _repository.GetByIdAsync(existingContact.Id).Returns(existingContact);
        
        // Act
        await _service.DeleteAsync(existingContact.Id);
        
        // Assert
        _repository.Received(1).Delete(existingContact);
        await _repository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowIfNotfound()
    {
        //Arrange
        int contactId = 1;
        
        _repository.GetByIdAsync(contactId).Returns((Contact)null);
        
        //Act & Assert
        await Assert.ThrowsAnyAsync<NotFoundException>(async () => await _service.DeleteAsync(contactId));
    }

    [Fact]
    public async Task UpdateFieldAsync_NameShouldUpdateSuccessfully()
    {
        //Arrange
        var contact = new Contact { Id = 1, Name = "Old Name" };
        _repository.GetByIdTrackedAsync(contact.Id).Returns(contact);
        var field = nameof(Contact.Name);
        var newValue = "New Name";
        
        /*var validator = Substitute.For<IValidator<ContactDto>>();
        
        validator
    .ValidateAsync(Arg.Any<ContactDto>(), Arg.Any<CancellationToken>())
    .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));*/
        
        /*_validator
            .ValidateAsync(Arg.Any<ContactDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));*/
                    
        //Act
        await _service.UpdateFieldAsync(contact.Id, field,  newValue);
        
        //Assert
        Assert.Equal(newValue, contact.Name);
        await _repository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task UpdateFieldAsync_BirthDateShouldUpdateSuccessfully()
    {
        //Arrange
        var contact = new Contact { Id = 1, BirthDate = new DateTime(2025, 1, 1) };
        _repository.GetByIdTrackedAsync(contact.Id).Returns(contact);
        var field = nameof(Contact.BirthDate);
        var newValue = new DateTime(2020, 2,2).ToString();
        
        //Act
        await _service.UpdateFieldAsync(contact.Id, field,  newValue);
        
        //Assert
        Assert.Equal(newValue, contact.BirthDate.ToString());
        await _repository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task UpdateFieldAsync_IsMarriedShouldUpdateSuccessfully()
    {
        //Arrange
        var contact = new Contact { Id = 1, IsMarried = false};
        _repository.GetByIdTrackedAsync(contact.Id).Returns(contact);
        var field = nameof(Contact.IsMarried);
        var newValue = true.ToString();
        
        //Act
        await _service.UpdateFieldAsync(contact.Id, field,  newValue);
        
        //Assert
        Assert.Equal(newValue, contact.IsMarried.ToString());
        await _repository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task UpdateFieldAsync_PhoneNumberShouldUpdateSuccessfully()
    {
        //Arrange
        var contact = new Contact { Id = 1, PhoneNumber = "+380677777777" };
        _repository.GetByIdTrackedAsync(contact.Id).Returns(contact);
        var field = nameof(Contact.PhoneNumber);
        var newValue = "+380505555555";
        
        //Act
        await _service.UpdateFieldAsync(contact.Id, field,  newValue);
        
        //Assert
        Assert.Equal(newValue, contact.PhoneNumber);
        await _repository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task UpdateFieldAsync_SalaryNumberShouldUpdateSuccessfully()
    {
        //Arrange
        var contact = new Contact { Id = 1, Salary = 7777.77m };
        _repository.GetByIdTrackedAsync(contact.Id).Returns(contact);
        var field = nameof(Contact.Salary);
        var newValue = "5555.55";
        
        //Act
        await _service.UpdateFieldAsync(contact.Id, field,  newValue);
        
        //Assert
        Assert.Equal(5555.55m, contact.Salary);
        await _repository.Received(1).SaveChangesAsync();
    }
}