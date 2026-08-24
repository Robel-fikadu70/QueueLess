
using MediatR;
using QueueLess.Application.DTOs.Users;
using QueueLess.Application.Interfaces;

namespace QueueLess.Application.Features.Users.Queries;

public record GetUserProfileQuery: IRequest<UserProfileDto>;
public class GetUserProfileQueryHandler(IIdentityService identityService, ICurrentUserService currentUserService) : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IIdentityService _identityService = identityService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        return await _identityService.GetProfileAsync(userId);
    }
}