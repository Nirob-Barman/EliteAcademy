using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using ClassEntity = EliteAcademy.Domain.Entities.Instructor.Class;

namespace EliteAcademy.Application.Features.Class.Commands.CreateClass;

public class CreateClassHandler : IRequestHandler<CreateClassCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly IFileStorage _fileStorage;

    public CreateClassHandler(
        IApplicationDbContext context,
        IUserContextService userContextService,
        IFileStorage fileStorage)
    {
        _context = context;
        _userContextService = userContextService;
        _fileStorage = fileStorage;
    }

    public async Task<Result<int>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var entity = new ClassEntity
        {
            ClassName = dto.ClassName,
            AvailableSeats = dto.AvailableSeats,
            Price = dto.Price,
            InstructorId = _userContextService.UserId,
            Status = ClassStatus.Pending,
            CreatedBy = _userContextService.UserId,
            CreatedAt = DateTime.UtcNow
        };

        if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
            entity.ClassImage = await _fileStorage.UploadFileAsync(request.ImageStream, request.ImageFileName, "uploads/classes");

        _context.Classes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Ok(entity.Id, "Class submitted for approval.");
    }
}
