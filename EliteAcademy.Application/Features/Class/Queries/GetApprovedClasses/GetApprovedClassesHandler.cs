using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Mappers;
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
        _context     = context;
        _userManager = userManager;
    }

    public async Task<Result<List<ClassDto>>> Handle(GetApprovedClassesQuery request, CancellationToken cancellationToken)
    {
        var classes = await _context.Classes
            .AsNoTracking()
            .Where(c => c.Status == ClassStatus.Approved)
            .ToListAsync(cancellationToken);

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
