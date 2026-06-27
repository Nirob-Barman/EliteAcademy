using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllClasses;

public class GetAllClassesHandler : IRequestHandler<GetAllClassesQuery, Result<PagedResult<ClassDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetAllClassesHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<PagedResult<ClassDto>>> Handle(GetAllClassesQuery request, CancellationToken cancellationToken)
    {
        var total = await _context.Classes.CountAsync(cancellationToken);

        var classDtos = await _context.Classes
            .AsNoTracking()
            .OrderByDescending(c => c.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
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

        var instructors = await _userManager.GetUsersByRoleAsync("Instructor");
        var instructorMap = instructors.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

        foreach (var dto in classDtos)
            dto.InstructorName = instructorMap.GetValueOrDefault(dto.InstructorId ?? "");

        return Result<PagedResult<ClassDto>>.Ok(new PagedResult<ClassDto>
        {
            Items = classDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
