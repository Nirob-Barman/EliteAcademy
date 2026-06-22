using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Student.Commands.SelectClass;

public class SelectClassHandler : IRequestHandler<SelectClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public SelectClassHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(SelectClassCommand request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;

        var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);

        var domainResult = PreEnrollment.Create(studentId, cls);
        if (!domainResult.IsSuccess)
            return Result<bool>.Fail(domainResult.Error);

        if (await _context.PreEnrollments.AnyAsync(p => p.StudentId == studentId && p.ClassId == request.ClassId && p.PaymentStatus == PaymentStatus.Pending, cancellationToken))
            return Result<bool>.Fail("Class is already in your selections.");

        if (await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.ClassId == request.ClassId, cancellationToken))
            return Result<bool>.Fail("You are already enrolled in this class.");

        _context.PreEnrollments.Add(domainResult.Value!);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Class added to selections.");
    }
}
