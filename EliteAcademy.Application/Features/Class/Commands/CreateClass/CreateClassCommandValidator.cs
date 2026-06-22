using FluentValidation;

namespace EliteAcademy.Application.Features.Class.Commands.CreateClass;

public class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(x => x.Dto.ClassName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.AvailableSeats).GreaterThan(0);
    }
}
