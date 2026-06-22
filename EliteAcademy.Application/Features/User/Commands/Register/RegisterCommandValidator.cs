using FluentValidation;

namespace EliteAcademy.Application.Features.User.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Model.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Model.Password).NotEmpty().MinimumLength(6);
    }
}
