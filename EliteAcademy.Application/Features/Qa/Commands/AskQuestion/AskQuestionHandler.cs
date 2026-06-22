using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Student;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Qa.Commands.AskQuestion;

public class AskQuestionHandler : IRequestHandler<AskQuestionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public AskQuestionHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(AskQuestionCommand request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;

        var domainResult = QaQuestion.Create(studentId, request.Dto.ClassId, request.Dto.QuestionText);
        if (!domainResult.IsSuccess)
            return Result<bool>.FailField("QuestionText", domainResult.Error);

        var enrolled = await _context.Enrollments.AnyAsync(
            e => e.StudentId == studentId && e.ClassId == request.Dto.ClassId,
            cancellationToken);

        if (!enrolled)
            return Result<bool>.Fail("You must be enrolled to ask a question.");

        _context.QaQuestions.Add(domainResult.Value!);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Question posted.");
    }
}
