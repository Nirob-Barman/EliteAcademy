using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Class.Queries.GetClassesByInstructor;

public class GetClassesByInstructorHandler : IRequestHandler<GetClassesByInstructorQuery, Result<List<ClassDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;

    public GetClassesByInstructorHandler(
        IApplicationDbContext context,
        IUserManager userManager,
        IUserContextService userContextService)
    {
        _context            = context;
        _userManager        = userManager;
        _userContextService = userContextService;
    }

    public async Task<Result<List<ClassDto>>> Handle(GetClassesByInstructorQuery request, CancellationToken cancellationToken)
    {
        var instructorId = _userContextService.UserId!;
        var user = await _userManager.FindByIdAsync(instructorId);
        var instructorName = user == null ? "" : $"{user.FirstName} {user.LastName}".Trim();

        var classes = await _context.Classes
            .AsNoTracking()
            .Where(c => c.InstructorId == instructorId)
            .ToListAsync(cancellationToken);

        var dtos = classes
            .Select(c => ClassMapper.ToDto(c, instructorName))
            .ToList();

        return Result<List<ClassDto>>.Ok(dtos);
    }
}
