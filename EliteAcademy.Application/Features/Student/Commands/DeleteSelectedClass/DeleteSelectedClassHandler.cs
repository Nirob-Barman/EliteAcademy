using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Student.Commands.DeleteSelectedClass;

public class DeleteSelectedClassHandler : IRequestHandler<DeleteSelectedClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public DeleteSelectedClassHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(DeleteSelectedClassCommand request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;
        var preEnrollment = await _context.PreEnrollments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.PreEnrollmentId, cancellationToken);
        if (preEnrollment == null)
            return Result<bool>.Fail("Selection not found.");
        if (preEnrollment.StudentId != studentId)
            return Result<bool>.Fail("Not authorized.");
        if (preEnrollment.PaymentStatus != PaymentStatus.Pending)
            return Result<bool>.Fail("Cannot remove a paid selection.");

        _context.PreEnrollments.Remove(preEnrollment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Selection removed.");
    }
}
