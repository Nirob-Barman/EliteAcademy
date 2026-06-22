using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Commands.DeleteSelectedClass;

public record DeleteSelectedClassCommand(int PreEnrollmentId) : IRequest<Result<bool>>;
