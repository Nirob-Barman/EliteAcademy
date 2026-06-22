using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Student.Queries.GetEnrolledClasses;

public class GetEnrolledClassesHandler : IRequestHandler<GetEnrolledClassesQuery, Result<List<EnrollmentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;

    public GetEnrolledClassesHandler(
        IApplicationDbContext context,
        IUserManager userManager,
        IUserContextService userContextService)
    {
        _context = context;
        _userManager = userManager;
        _userContextService = userContextService;
    }

    public async Task<Result<List<EnrollmentDto>>> Handle(GetEnrolledClassesQuery request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;

        var enrollments = await _context.Enrollments.AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .ToListAsync(cancellationToken);

        var users = await _userManager.GetAllUsersAsync();
        var instructorMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

        var dtos = new List<EnrollmentDto>();
        foreach (var enrollment in enrollments)
        {
            var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == enrollment.ClassId, cancellationToken);
            var instructorName = cls?.InstructorId != null
                ? instructorMap.GetValueOrDefault(cls.InstructorId, "")
                : "";
            dtos.Add(EnrollmentMapper.ToEnrollmentDto(enrollment, cls, instructorName));
        }

        return Result<List<EnrollmentDto>>.Ok(dtos);
    }
}
