using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllStudents;

public class GetAllStudentsHandler : IRequestHandler<GetAllStudentsQuery, Result<List<AdminStudentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetAllStudentsHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<List<AdminStudentDto>>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = (await _userManager.GetUsersByRoleAsync("Student")).ToList();
        var enrollments = await _context.Enrollments.AsNoTracking().ToListAsync(cancellationToken);

        var dtos = students.Select(s => new AdminStudentDto
        {
            Id = s.Id,
            FullName = $"{s.FirstName} {s.LastName}".Trim(),
            Email = s.Email,
            EnrollmentCount = enrollments.Count(e => e.StudentId == s.Id),
            IsBanned = s.IsBanned,
            JoinedAt = DateTime.UtcNow
        }).ToList();

        return Result<List<AdminStudentDto>>.Ok(dtos);
    }
}
