using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.InstructorApplication.Commands.ApplyForInstructor;

public class ApplyForInstructorHandler : IRequestHandler<ApplyForInstructorCommand, Result<InstructorApplicationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly IUserManager _userManager;

    public ApplyForInstructorHandler(
        IApplicationDbContext context,
        IUserContextService userContextService,
        IUserManager userManager)
    {
        _context = context;
        _userContextService = userContextService;
        _userManager = userManager;
    }

    public async Task<Result<InstructorApplicationDto>> Handle(ApplyForInstructorCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Result<InstructorApplicationDto>.Fail("You must be logged in to apply.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<InstructorApplicationDto>.Fail("User not found.");

        if (await _userManager.IsUserInRoleAsync(user, "Instructor"))
            return Result<InstructorApplicationDto>.Fail("You are already an instructor.");

        var existing = await _context.InstructorApplications.AsNoTracking().FirstOrDefaultAsync(
            a => a.ApplicantId == userId
              && (a.Status == InstructorApplicationStatus.Pending
               || a.Status == InstructorApplicationStatus.Approved),
            cancellationToken);

        if (existing != null)
        {
            var reason = existing.Status == InstructorApplicationStatus.Pending
                ? "You already have a pending application. Please wait for admin review."
                : "Your application has already been approved.";
            return Result<InstructorApplicationDto>.Fail(reason);
        }

        var dto = request.Dto;
        var domainResult = Domain.Entities.InstructorApplication.Create(
            userId, $"{user.FirstName} {user.LastName}".Trim(), user.Email, dto.Bio, dto.Expertise, dto.Motivation);

        if (!domainResult.IsSuccess)
            return Result<InstructorApplicationDto>.Fail(domainResult.Error);

        var entity = domainResult.Value!;
        _context.InstructorApplications.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<InstructorApplicationDto>.Ok(
            InstructorApplicationMapper.ToDto(entity),
            "Your application has been submitted. We will review it shortly.");
    }
}
