using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Commands.RejectClass;

public class RejectClassHandler : IRequestHandler<RejectClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public RejectClassHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(RejectClassCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Class not found.");

        var rejectResult = entity.Reject(request.Feedback);
        if (!rejectResult.IsSuccess)
            return Result<bool>.Fail(rejectResult.Error);

        entity.UpdatedBy = _userContextService.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Class rejected.");
    }
}
