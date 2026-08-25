using MediatR;
using QueueLess.Application.Interfaces;

namespace QueueLess.Application.Features.Users.Commands;
public record UpdateUserProfileCommand(string FirstName, string LastName) : IRequest<Unit>;
public class UpdateUserProfileCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService) : IRequestHandler<UpdateUserProfileCommand, Unit>
{
    private readonly IIdentityService _identityService = identityService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    public async Task<Unit> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        await _identityService.UpdateProfileAsync(userId, request.FirstName, request.LastName);
        return Unit.Value;
    }
}