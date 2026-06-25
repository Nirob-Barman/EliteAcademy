using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Class.Queries.GetClassById;

public class GetClassByIdHandler : IRequestHandler<GetClassByIdQuery, Result<ClassDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetClassByIdHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<ClassDto>> Handle(GetClassByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity == null)
            return Result<ClassDto>.Fail("Class not found.");

        var user = entity.InstructorId != null
            ? await _userManager.FindByIdAsync(entity.InstructorId)
            : null;
        var instructorName = user == null ? "" : $"{user.FirstName} {user.LastName}".Trim();

        return Result<ClassDto>.Ok(ClassMapper.ToDto(entity, instructorName));
    }
}
