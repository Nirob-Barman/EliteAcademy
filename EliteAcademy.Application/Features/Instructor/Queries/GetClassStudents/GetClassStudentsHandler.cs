using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Instructor.Queries.GetClassStudents;

public class GetClassStudentsHandler : IRequestHandler<GetClassStudentsQuery, Result<List<ClassStudentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;

    public GetClassStudentsHandler(
        IApplicationDbContext context,
        IUserManager userManager,
        IUserContextService userContextService)
    {
        _context = context;
        _userManager = userManager;
        _userContextService = userContextService;
    }

    public async Task<Result<List<ClassStudentDto>>> Handle(GetClassStudentsQuery request, CancellationToken cancellationToken)
    {
        var instructorId = _userContextService.UserId!;
        var cls = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);

        if (cls == null || cls.InstructorId != instructorId)
            return Result<List<ClassStudentDto>>.Fail("Class not found.");

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.ClassId == request.ClassId)
            .ToListAsync(cancellationToken);

        var users = await _userManager.GetAllUsersAsync();
        var userMap = users.ToDictionary(u => u.Id ?? "", u => u);

        var dtos = enrollments.Select(e =>
        {
            var user = userMap.GetValueOrDefault(e.StudentId ?? "");
            return new ClassStudentDto
            {
                StudentId = e.StudentId,
                FullName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown",
                Email = user?.Email,
                EnrolledAt = e.EnrolledAt
            };
        }).ToList();

        return Result<List<ClassStudentDto>>.Ok(dtos);
    }
}
