using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Users;

public class UserProfileDto
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Id { get; set; }
}

[Authorize]
public class GetProfileQuery : IRequest<UserProfileDto>
{

}

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetProfileQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<UserProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.DomainUsers.Where(x => x.Id == _user.UserId).FirstOrDefaultAsync();
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, $"User with id {_user.UserId} not found");
        var result = new UserProfileDto { Email = user.Email, FullName = user.FullName, Id = user.Id };
        return result;
    }
}
