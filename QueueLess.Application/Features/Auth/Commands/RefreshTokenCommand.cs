using MediatR;
using QueueLess.Application.Interfaces;

namespace QueueLess.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string Token) : IRequest<TokenResult>;

public class RefreshTokenCommandHandler(IIdentityService identityService) : IRequestHandler<RefreshTokenCommand, TokenResult>
{
    private readonly IIdentityService _identitiyService = identityService;

    public async Task<TokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _identitiyService.RefreshTokenAsync(request.Token);
    }
}