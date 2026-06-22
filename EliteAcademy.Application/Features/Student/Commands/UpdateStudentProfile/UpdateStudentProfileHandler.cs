using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Commands.UpdateStudentProfile;

public class UpdateStudentProfileHandler : IRequestHandler<UpdateStudentProfileCommand, Result<bool>>
{
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;
    private readonly IFileStorage _fileStorage;

    public UpdateStudentProfileHandler(
        IUserManager userManager,
        IUserContextService userContextService,
        IFileStorage fileStorage)
    {
        _userManager = userManager;
        _userContextService = userContextService;
        _fileStorage = fileStorage;
    }

    public async Task<Result<bool>> Handle(UpdateStudentProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
        if (user == null)
            return Result<bool>.Fail("User not found.");

        user.FirstName = request.Dto.FirstName;
        user.LastName = request.Dto.LastName;

        if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
        {
            if (!string.IsNullOrWhiteSpace(user.ImageUrl))
                await _fileStorage.DeleteFileAsync(user.ImageUrl);
            user.ImageUrl = await _fileStorage.UploadFileAsync(request.ImageStream, request.ImageFileName, "uploads/profiles");
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<bool>.Fail(result.Errors.FirstOrDefault() ?? "Update failed.");

        return Result<bool>.Ok(true, "Profile updated.");
    }
}
