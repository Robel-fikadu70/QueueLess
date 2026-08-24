using MediatR;
using QueueLess.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.Staff.Commands;

public record RegisterStaffMemberCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<string>;

public class RegisterStaffMemberCommandHandler(IIdentityService identityService) : IRequestHandler<RegisterStaffMemberCommand, string>
{
    private readonly IIdentityService _identityService = identityService;

    public async Task<string> Handle(RegisterStaffMemberCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RegisterStaffAsync(
            request.Email, 
            request.Password, 
            request.FirstName, 
            request.LastName);
    }
}