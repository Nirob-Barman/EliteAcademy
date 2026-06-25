using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using EliteAcademy.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Commands.RejectClass;

public class RejectClassHandler : IRequestHandler<RejectClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public RejectClassHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context            = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(RejectClassCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Feedback))
            return Result<bool>.Fail("Feedback is required when rejecting a class.");

        var entity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Class not found.");

        entity.Status    = ClassStatus.Rejected;
        entity.Feedback  = request.Feedback;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _userContextService.UserId;

        if (!string.IsNullOrWhiteSpace(entity.InstructorId))
            entity.AddDomainEvent(new ClassRejectedEvent(entity.Id, entity.InstructorId, entity.ClassName!, request.Feedback));

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Class rejected.");
    }
}
