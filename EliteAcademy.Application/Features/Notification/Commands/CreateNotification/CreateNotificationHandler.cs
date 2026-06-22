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
        _context.AppNotifications.Add(new AppNotification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Link = request.Link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
