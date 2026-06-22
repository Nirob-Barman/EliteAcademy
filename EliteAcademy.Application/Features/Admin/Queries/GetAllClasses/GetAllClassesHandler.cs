using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Mappers;
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
        var classes = await _context.Classes.AsNoTracking().ToListAsync(cancellationToken);
        var users = await _userManager.GetAllUsersAsync();
        var instructorMap = users.ToDictionary(
            u => u.Id ?? "",
            u => $"{u.FirstName} {u.LastName}".Trim());

        var dtos = classes
            .Select(c => ClassMapper.ToDto(c, instructorMap.GetValueOrDefault(c.InstructorId ?? "")))
            .ToList();

        return Result<List<ClassDto>>.Ok(dtos);
    }
}
