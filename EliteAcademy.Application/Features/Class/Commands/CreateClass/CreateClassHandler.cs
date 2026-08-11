using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
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

        var domainResult = ClassEntity.Create(_userContextService.UserId!, dto.ClassName!, dto.AvailableSeats, dto.Price);
        if (!domainResult.IsSuccess)
            return Result<int>.Fail(domainResult.Error);

        var entity = domainResult.Value!;

        if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
            entity.ClassImage = await _fileStorage.UploadFileAsync(request.ImageStream, request.ImageFileName, "uploads/classes");

        _context.Classes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Ok(entity.Id, "Class submitted for approval.");
    }
}
