using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Account;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result<string>>
{
    private readonly IUserManager _userManager;
    private readonly IFileStorage _fileStorage;

    public RegisterHandler(IUserManager userManager, IFileStorage fileStorage)
    {
        _userManager = userManager;
        _fileStorage = fileStorage;
    }

    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var model = request.Model;

        if (!string.IsNullOrWhiteSpace(model.Email) && await _userManager.FindByEmailAsync(model.Email) != null)
            return Result<string>.FailField(nameof(model.Email), "This email is already registered.");

        var user = new AppUser
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Address = model.Address,
            Gender = model.Gender,
            DateOfBirth = model.DateOfBirth
        };

        if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
            user.ImageUrl = await _fileStorage.UploadFileAsync(request.ImageStream, request.ImageFileName, "uploads/profiles");

        var (succeeded, userId, errors) = await _userManager.CreateAsync(user, model.Password!);

        if (!succeeded)
        {
            var fieldErrors = new Dictionary<string, string>();

            foreach (var error in errors ?? new List<string>())
            {
                if (error.Contains("email", StringComparison.OrdinalIgnoreCase))
                    fieldErrors[nameof(model.Email)] = error;
                else if (error.Contains("password", StringComparison.OrdinalIgnoreCase))
                    fieldErrors[nameof(model.Password)] = error;
            }

            if (fieldErrors.Count > 0)
            {
                return new Result<string>
                {
                    Success = false,
                    FieldErrors = fieldErrors,
                    Errors = errors,
                    Message = "Registration failed"
                };
            }

            return Result<string>.Fail(errors!, "Registration failed");
        }

        var roleResult = await _userManager.AddToRoleAsync(new AppUser { Id = userId }, "Student");

        if (!roleResult.Succeeded)
        {
            await _userManager.RemoveFromRoleAsync(new AppUser { Id = userId }, "Student");
            return Result<string>.Fail("Failed to assign default role to user.");
        }

        return Result<string>.Ok(userId, "Registration successful");
    }
}
