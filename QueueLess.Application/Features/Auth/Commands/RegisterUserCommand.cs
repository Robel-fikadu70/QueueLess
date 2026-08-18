using MediatR;
using QueueLess.Application.DTOs.Auth;
using QueueLess.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.Auth.Commands;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<AuthResponseDto>;

public class RegisterUserCommandHandler(IIdentityService identityService) : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IIdentityService _identityService = identityService;

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RegisterAsync(
            request.Email, 
            request.Password, 
            request.FirstName, 
            request.LastName);
    }
}