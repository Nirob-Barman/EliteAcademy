using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Qa.Commands.DeleteAnswer;

public record DeleteAnswerCommand(int AnswerId) : IRequest<Result<bool>>;
