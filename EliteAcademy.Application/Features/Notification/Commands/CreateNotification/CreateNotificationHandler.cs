using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Domain.Entities;
using MediatR;

namespace EliteAcademy.Application.Features.Notification.Commands.CreateNotification;

public class CreateNotificationHandler : IRequestHandler<CreateNotificationCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public CreateNotificationHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = AppNotification.Create(
            request.UserId,
            request.Title,
            request.Message,
            request.Link);
        _context.AppNotifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
