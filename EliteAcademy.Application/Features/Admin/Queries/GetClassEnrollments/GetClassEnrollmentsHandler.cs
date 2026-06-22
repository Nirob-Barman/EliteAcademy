using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Queries.GetClassEnrollments;

public class GetClassEnrollmentsHandler : IRequestHandler<GetClassEnrollmentsQuery, Result<AdminClassEnrollmentsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetClassEnrollmentsHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<AdminClassEnrollmentsDto>> Handle(GetClassEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (cls == null)
            return Result<AdminClassEnrollmentsDto>.Fail("Class not found.");

        var allUsers = (await _userManager.GetAllUsersAsync()).ToDictionary(u => u.Id ?? "");
        var instructorName = allUsers.TryGetValue(cls.InstructorId ?? "", out var inst)
            ? $"{inst.FirstName} {inst.LastName}".Trim()
            : "Unknown";

        var enrollments = await _context.Enrollments.AsNoTracking()
            .Where(e => e.ClassId == request.ClassId)
            .ToListAsync(cancellationToken);

        var rows = enrollments.Select(e =>
        {
            allUsers.TryGetValue(e.StudentId ?? "", out var student);
            return new StudentEnrollmentRowDto
            {
                StudentId = e.StudentId,
                StudentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "Unknown",
                Email = student?.Email,
                EnrolledAt = e.EnrolledAt
            };
        }).ToList();

        return Result<AdminClassEnrollmentsDto>.Ok(new AdminClassEnrollmentsDto
        {
            ClassId = cls.Id,
            ClassName = cls.ClassName,
            InstructorName = instructorName,
            Price = cls.Price,
            AvailableSeats = cls.AvailableSeats,
            Enrollments = rows
        });
    }
}
