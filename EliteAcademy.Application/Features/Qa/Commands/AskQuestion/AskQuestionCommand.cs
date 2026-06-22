using EliteAcademy.Application.DTOs.QA;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Qa.Commands.AskQuestion;

public record AskQuestionCommand(QaAskDto Dto) : IRequest<Result<bool>>;
