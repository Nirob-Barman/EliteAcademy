using EliteAcademy.Application.DTOs.QA;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Qa.Commands.AnswerQuestion;

public record AnswerQuestionCommand(QaAnswerFormDto Dto) : IRequest<Result<bool>>;
