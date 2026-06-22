using EliteAcademy.Application.DTOs.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Queries.GetMyProfile;

public record GetMyProfileQuery : IRequest<Result<EditProfileDto>>;
