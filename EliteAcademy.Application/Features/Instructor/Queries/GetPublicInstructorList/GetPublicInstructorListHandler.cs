using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Instructor.Queries.GetPublicInstructorList;

public class GetPublicInstructorListHandler : IRequestHandler<GetPublicInstructorListQuery, Result<List<InstructorProfileDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetPublicInstructorListHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<List<InstructorProfileDto>>> Handle(GetPublicInstructorListQuery request, CancellationToken cancellationToken)
    {
        var instructors = (await _userManager.GetUsersByRoleAsync("Instructor")).ToList();
        var instructorIds = instructors.Select(u => u.Id ?? "").ToHashSet();

        var classData = await _context.Classes
            .AsNoTracking()
            .Where(c => c.Status == ClassStatus.Approved && instructorIds.Contains(c.InstructorId!))
            .Select(c => new { c.Id, c.InstructorId })
            .ToListAsync(cancellationToken);

        var classCountMap = classData
            .GroupBy(c => c.InstructorId ?? "")
            .ToDictionary(g => g.Key, g => g.Count());

        Dictionary<string, int> studentCountMap;
        if (classData.Count == 0)
        {
            studentCountMap = new Dictionary<string, int>();
        }
        else
        {
            var classIds = classData.Select(c => c.Id).ToList();
            var classToInstructor = classData.ToDictionary(c => c.Id, c => c.InstructorId ?? "");

            var enrollmentData = await _context.Enrollments
                .AsNoTracking()
                .Where(e => classIds.Contains(e.ClassId))
                .Select(e => new { e.ClassId, e.StudentId })
                .ToListAsync(cancellationToken);

            studentCountMap = enrollmentData
                .GroupBy(e => classToInstructor.GetValueOrDefault(e.ClassId, ""))
                .ToDictionary(g => g.Key, g => g.Select(e => e.StudentId).Distinct().Count());
        }

        var dtos = instructors.Select(u => new InstructorProfileDto
        {
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            ImageUrl = u.ImageUrl,
            ClassCount = classCountMap.GetValueOrDefault(u.Id ?? ""),
            StudentCount = studentCountMap.GetValueOrDefault(u.Id ?? "")
        }).ToList();

        return Result<List<InstructorProfileDto>>.Ok(dtos);
    }
}
