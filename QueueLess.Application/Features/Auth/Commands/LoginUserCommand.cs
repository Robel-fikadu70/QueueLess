using MediatR;
using QueueLess.Application.DTOs.Auth;
using QueueLess.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.Auth.Commands;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<AuthResponseDto>;

public class LoginUserCommandHandler(IIdentityService identityService) : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IIdentityService _identityService = identityService;

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.LoginAsync(request.Email, request.Password);
    }
}