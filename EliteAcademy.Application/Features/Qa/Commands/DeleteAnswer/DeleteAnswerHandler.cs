using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Qa.Commands.DeleteAnswer;

public class DeleteAnswerHandler : IRequestHandler<DeleteAnswerCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public DeleteAnswerHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(DeleteAnswerCommand request, CancellationToken cancellationToken)
    {
        var instructorId = _userContextService.UserId!;

        var answer = await _context.QaAnswers.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken);

        if (answer == null)
            return Result<bool>.Fail("Answer not found.");

        if (answer.InstructorId != instructorId)
            return Result<bool>.Fail("Not authorized.");

        _context.QaAnswers.Remove(answer);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Answer deleted.");
    }
}
