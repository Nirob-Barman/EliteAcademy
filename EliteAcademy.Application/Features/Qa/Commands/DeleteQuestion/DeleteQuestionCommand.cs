using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Qa.Commands.DeleteQuestion;

public record DeleteQuestionCommand(int QuestionId) : IRequest<Result<bool>>;
