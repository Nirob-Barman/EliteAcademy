using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Class.Queries.GetApprovedClasses;

public class GetApprovedClassesHandler : IRequestHandler<GetApprovedClassesQuery, Result<List<ClassDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetApprovedClassesHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<List<ClassDto>>> Handle(GetApprovedClassesQuery request, CancellationToken cancellationToken)
    {
        var classDtos = await _context.Classes
            .AsNoTracking()
            .Where(c => c.Status == ClassStatus.Approved)
            .Select(c => new ClassDto
            {
                Id = c.Id,
                ClassName = c.ClassName,
                ClassImage = c.ClassImage,
                InstructorId = c.InstructorId,
                AvailableSeats = c.AvailableSeats,
                Price = c.Price,
                Status = c.Status,
                ReviewCount = _context.Reviews.Count(r => r.ClassId == c.Id),
                AverageRating = (double?)_context.Reviews
                    .Where(r => r.ClassId == c.Id)
                    .Average(r => (double?)r.Rating) ?? 0
            })
            .ToListAsync(cancellationToken);

        var instructors = await _userManager.GetUsersByRoleAsync("Instructor");
        var instructorMap = instructors.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

        foreach (var dto in classDtos)
            dto.InstructorName = instructorMap.GetValueOrDefault(dto.InstructorId ?? "");

        return Result<List<ClassDto>>.Ok(classDtos);
    }
}
