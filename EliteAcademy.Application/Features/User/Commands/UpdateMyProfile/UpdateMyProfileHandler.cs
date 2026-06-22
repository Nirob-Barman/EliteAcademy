using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.UpdateMyProfile;

public class UpdateMyProfileHandler : IRequestHandler<UpdateMyProfileCommand, Result<bool>>
{
    private readonly IUserManager _userManager;
    private readonly IFileStorage _fileStorage;
    private readonly IUserContextService _userContextService;

    public UpdateMyProfileHandler(
        IUserManager userManager,
        IFileStorage fileStorage,
        IUserContextService userContextService)
    {
        _userManager = userManager;
        _fileStorage = fileStorage;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
        if (user == null) return Result<bool>.Fail("User not found.");

        var dto = request.Dto;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Gender = dto.Gender;
        user.DateOfBirth = dto.DateOfBirth;
        user.Address = dto.Address;

        if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
        {
            if (!string.IsNullOrWhiteSpace(user.ImageUrl))
                await _fileStorage.DeleteFileAsync(user.ImageUrl);
            user.ImageUrl = await _fileStorage.UploadFileAsync(request.ImageStream, request.ImageFileName, "uploads/profiles");
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<bool>.Fail(result.Errors.FirstOrDefault() ?? "Update failed.");

        return Result<bool>.Ok(true, "Profile updated successfully.");
    }
}
