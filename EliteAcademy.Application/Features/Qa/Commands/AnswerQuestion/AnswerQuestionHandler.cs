using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Student;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Qa.Commands.AnswerQuestion;

public class AnswerQuestionHandler : IRequestHandler<AnswerQuestionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public AnswerQuestionHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(AnswerQuestionCommand request, CancellationToken cancellationToken)
    {
        var instructorId = _userContextService.UserId!;

        var question = await _context.QaQuestions.AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == request.Dto.QuestionId, cancellationToken);

        if (question == null)
            return Result<bool>.Fail("Question not found.");

        var cls = await _context.Classes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == question.ClassId, cancellationToken);

        var domainResult = QaAnswer.Create(instructorId, request.Dto.QuestionId, request.Dto.AnswerText, cls);
        if (!domainResult.IsSuccess)
            return Result<bool>.FailField("AnswerText", domainResult.Error);

        _context.QaAnswers.Add(domainResult.Value!);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Answer posted.");
    }
}
