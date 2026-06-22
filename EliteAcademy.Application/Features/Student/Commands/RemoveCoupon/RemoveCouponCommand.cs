using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Commands.RemoveCoupon;

public record RemoveCouponCommand(int PreEnrollmentId) : IRequest<Result<bool>>;
