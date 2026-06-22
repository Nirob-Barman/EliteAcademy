using EliteAcademy.Application.DTOs.QA;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Qa.Queries.GetClassQa;

public record GetClassQaQuery(int ClassId) : IRequest<Result<List<QaQuestionDto>>>;
