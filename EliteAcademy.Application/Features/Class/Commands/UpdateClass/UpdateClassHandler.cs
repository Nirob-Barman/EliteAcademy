using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Class.Commands.UpdateClass;

public class UpdateClassHandler : IRequestHandler<UpdateClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly IFileStorage _fileStorage;

    public UpdateClassHandler(
        IApplicationDbContext context,
        IUserContextService userContextService,
        IFileStorage fileStorage)
    {
        _context            = context;
        _userContextService = userContextService;
        _fileStorage        = fileStorage;
    }

    public async Task<Result<bool>> Handle(UpdateClassCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var entity = await _context.Classes
            .FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

        if (entity == null)
            return Result<bool>.Fail("Class not found.");

        if (entity.InstructorId != _userContextService.UserId)
            return Result<bool>.Fail("You do not own this class.");

        entity.ClassName      = dto.ClassName;
        entity.AvailableSeats = dto.AvailableSeats;
        entity.Price          = dto.Price;
        entity.UpdatedBy      = _userContextService.UserId;
        entity.UpdatedAt      = DateTime.UtcNow;

        if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
        {
            if (!string.IsNullOrWhiteSpace(entity.ClassImage))
                await _fileStorage.DeleteFileAsync(entity.ClassImage);

            entity.ClassImage = await _fileStorage.UploadFileAsync(request.ImageStream, request.ImageFileName, "uploads/classes");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Class updated.");
    }
}
