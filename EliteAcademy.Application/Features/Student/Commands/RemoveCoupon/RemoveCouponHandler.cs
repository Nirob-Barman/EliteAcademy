using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Student.Commands.RemoveCoupon;

public class RemoveCouponHandler : IRequestHandler<RemoveCouponCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public RemoveCouponHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(RemoveCouponCommand request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;
        var pe = await _context.PreEnrollments.FirstOrDefaultAsync(p => p.Id == request.PreEnrollmentId, cancellationToken);
        if (pe == null || pe.StudentId != studentId)
            return Result<bool>.Fail("Selection not found.");

        var removeResult = pe.RemoveCoupon();
        if (!removeResult.IsSuccess)
            return Result<bool>.Fail(removeResult.Error);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Coupon removed.");
    }
}
