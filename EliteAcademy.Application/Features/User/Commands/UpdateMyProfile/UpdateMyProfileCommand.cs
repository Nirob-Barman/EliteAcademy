using EliteAcademy.Application.DTOs.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.UpdateMyProfile;

public record UpdateMyProfileCommand(EditProfileDto Dto, Stream? ImageStream, string? ImageFileName) : IRequest<Result<bool>>;
