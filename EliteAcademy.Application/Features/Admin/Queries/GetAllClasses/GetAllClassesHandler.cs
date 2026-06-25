using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllClasses;

public class GetAllClassesHandler : IRequestHandler<GetAllClassesQuery, Result<List<ClassDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetAllClassesHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<List<ClassDto>>> Handle(GetAllClassesQuery request, CancellationToken cancellationToken)
    {
        var classDtos = await _context.Classes
            .AsNoTracking()
            .Select(c => new ClassDto
            {
                Id = c.Id,
                ClassName = c.ClassName,
                ClassImage = c.ClassImage,
                InstructorId = c.InstructorId,
                AvailableSeats = c.AvailableSeats,
                Price = c.Price,
                Status = c.Status,
                Feedback = c.Feedback
            })
            .ToListAsync(cancellationToken);

        var users = await _userManager.GetAllUsersAsync();
        var instructorMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

        foreach (var dto in classDtos)
            dto.InstructorName = instructorMap.GetValueOrDefault(dto.InstructorId ?? "");

        return Result<List<ClassDto>>.Ok(classDtos);
    }
}
