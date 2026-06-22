using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Student.Queries.GetEnrollmentStatus;

public class GetEnrollmentStatusHandler : IRequestHandler<GetEnrollmentStatusQuery, Result<(HashSet<int> Selected, HashSet<int> Enrolled)>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetEnrollmentStatusHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<(HashSet<int> Selected, HashSet<int> Enrolled)>> Handle(GetEnrollmentStatusQuery request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;

        var selectedIds = (await _context.PreEnrollments.AsNoTracking()
                .Where(p => p.StudentId == studentId && p.PaymentStatus == PaymentStatus.Pending)
                .ToListAsync(cancellationToken))
            .Select(p => p.ClassId)
            .ToHashSet();

        var enrolledIds = (await _context.Enrollments.AsNoTracking()
                .Where(e => e.StudentId == studentId)
                .ToListAsync(cancellationToken))
            .Select(e => e.ClassId)
            .ToHashSet();

        return Result<(HashSet<int>, HashSet<int>)>.Ok((selectedIds, enrolledIds));
    }
}
