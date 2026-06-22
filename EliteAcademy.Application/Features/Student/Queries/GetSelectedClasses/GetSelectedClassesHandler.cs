using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Student.Queries.GetSelectedClasses;

public class GetSelectedClassesHandler : IRequestHandler<GetSelectedClassesQuery, Result<List<PreEnrollmentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;

    public GetSelectedClassesHandler(
        IApplicationDbContext context,
        IUserManager userManager,
        IUserContextService userContextService)
    {
        _context = context;
        _userManager = userManager;
        _userContextService = userContextService;
    }

    public async Task<Result<List<PreEnrollmentDto>>> Handle(GetSelectedClassesQuery request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;

        var preEnrollments = await _context.PreEnrollments.AsNoTracking()
            .Where(p => p.StudentId == studentId && p.PaymentStatus == PaymentStatus.Pending)
            .ToListAsync(cancellationToken);

        var users = await _userManager.GetAllUsersAsync();
        var instructorMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

        var dtos = new List<PreEnrollmentDto>();
        foreach (var pe in preEnrollments)
        {
            var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == pe.ClassId, cancellationToken);
            var instructorName = cls?.InstructorId != null
                ? instructorMap.GetValueOrDefault(cls.InstructorId, "")
                : "";
            dtos.Add(EnrollmentMapper.ToPreEnrollmentDto(pe, cls, instructorName));
        }

        return Result<List<PreEnrollmentDto>>.Ok(dtos);
    }
}
