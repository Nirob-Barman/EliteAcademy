using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Commands.PayForClass;

public record PayForClassCommand(int PreEnrollmentId) : IRequest<Result<bool>>;
