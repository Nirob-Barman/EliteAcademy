using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllStudents;

public class GetAllStudentsHandler : IRequestHandler<GetAllStudentsQuery, Result<PagedResult<AdminStudentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetAllStudentsHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<PagedResult<AdminStudentDto>>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var allStudents = (await _userManager.GetUsersByRoleAsync("Student")).ToList();
        var total = allStudents.Count;
        var pageStudents = allStudents
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var pageStudentIds = pageStudents.Select(s => s.Id).Where(id => id != null).ToList();

        var countMap = new Dictionary<string, int>();
        if (pageStudentIds.Count > 0)
        {
            var enrollmentCounts = await _context.Enrollments
                .AsNoTracking()
                .Where(e => pageStudentIds.Contains(e.StudentId))
                .GroupBy(e => e.StudentId)
                .Select(g => new { StudentId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);
            countMap = enrollmentCounts.ToDictionary(e => e.StudentId!, e => e.Count);
        }

        var dtos = pageStudents.Select(s => new AdminStudentDto
        {
            Id = s.Id,
            FullName = $"{s.FirstName} {s.LastName}".Trim(),
            Email = s.Email,
            EnrollmentCount = countMap.GetValueOrDefault(s.Id ?? "", 0),
            IsBanned = s.IsBanned,
            JoinedAt = DateTime.UtcNow
        }).ToList();

        return Result<PagedResult<AdminStudentDto>>.Ok(new PagedResult<AdminStudentDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
