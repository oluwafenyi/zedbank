using FluentValidation;
using zedbank.Database;
using zedbank.Models;

namespace zedbank.Validators;

public class UserRegistrationValidator: AbstractValidator<UserRegistrationDto>
{
    private readonly Context _context;
    
    public UserRegistrationValidator(Context context)
    {
        _context = context;
        RuleSet("Names", () => 
        {
            RuleFor(user => user.FirstName).NotNull().NotEmpty().MaximumLength(100);
            RuleFor(user => user.LastName).NotNull().NotEmpty().MaximumLength(100);
        });

        RuleFor(user => user.Email).EmailAddress().Must(UniqueEmail).WithMessage("email already registered");
        RuleFor(user => user.Password).MinimumLength(8).DependentRules(() =>
        {
            RuleFor(user => user.ConfirmPassword).Must((model, field) => model.Password == field).
                WithMessage("ensure password matches confirmPassword");
        });
    }

    private bool UniqueEmail(string email)
    {
        var count = _context.Users.Count(user => user.Email == email);
        return count == 0;
    }
}