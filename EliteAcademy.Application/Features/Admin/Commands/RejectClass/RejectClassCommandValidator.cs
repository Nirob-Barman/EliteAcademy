using FluentValidation;

namespace EliteAcademy.Application.Features.Admin.Commands.RejectClass;

public class RejectClassCommandValidator : AbstractValidator<RejectClassCommand>
{
    public RejectClassCommandValidator()
    {
        RuleFor(x => x.Feedback).NotEmpty().MaximumLength(500);
    }
}
