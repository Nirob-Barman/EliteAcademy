using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Qa.Commands.DeleteQuestion;

public class DeleteQuestionHandler : IRequestHandler<DeleteQuestionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public DeleteQuestionHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.UserId!;

        var question = await _context.QaQuestions.AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

        if (question == null)
            return Result<bool>.Fail("Question not found.");

        var isInstructor = _userContextService.IsInRole("Instructor");
        if (!isInstructor && question.StudentId != userId)
            return Result<bool>.Fail("Not authorized.");

        _context.QaQuestions.Remove(question);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Question deleted.");
    }
}
