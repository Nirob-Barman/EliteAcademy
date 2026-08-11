using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Commands.ApproveClass;

public class ApproveClassHandler : IRequestHandler<ApproveClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public ApproveClassHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(ApproveClassCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Class not found.");

        var approveResult = entity.Approve();
        if (!approveResult.IsSuccess)
            return Result<bool>.Fail(approveResult.Error);

        entity.UpdatedBy = _userContextService.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Class approved.");
    }
}
