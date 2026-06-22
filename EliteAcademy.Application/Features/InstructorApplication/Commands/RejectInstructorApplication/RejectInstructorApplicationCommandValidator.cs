using FluentValidation;

namespace EliteAcademy.Application.Features.InstructorApplication.Commands.RejectInstructorApplication;

public class RejectInstructorApplicationCommandValidator : AbstractValidator<RejectInstructorApplicationCommand>
{
    public RejectInstructorApplicationCommandValidator()
    {
        RuleFor(x => x.AdminNotes).NotEmpty().MaximumLength(1000);
    }
}
