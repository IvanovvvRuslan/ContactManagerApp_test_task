using ContactManagerApp.DTO;
using FluentValidation;

namespace ContactManagerApp.Validators;

public class ContactValidator : AbstractValidator<ContactDto>
{
    public ContactValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name cannot exceed 50 characters");
        
        RuleFor(x => x.BirthDate).NotEmpty().WithMessage("BirthDate is required")
            .LessThan(DateTime.Today).WithMessage("BirthDate must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-110)).WithMessage("BirthDate is not valid");
        
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[0-9]{7,15}$")
            .WithMessage("Phone number must contain only digits and optional leading + (7–15 chars)");
        
        RuleFor(x => x.Salary).NotEmpty().WithMessage("Salary is required")
            .GreaterThan(0).WithMessage("Salary must be greater than 0");
    }
}