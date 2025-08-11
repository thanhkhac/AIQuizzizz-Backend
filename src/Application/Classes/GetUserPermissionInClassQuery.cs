using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Classes;

[Authorize]
public class GetUserPermissionInClassQuery : IRequest<string?>
{
    public required Guid ClassId { get; set; }
}

public class GetUserPermissionInClassQueryValidator : AbstractValidator<GetUserPermissionInClassQuery>
{
    public GetUserPermissionInClassQueryValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("FolderId không được null");
    }
}

public class GetUserPermissionInClassQueryHandler : IRequestHandler<GetUserPermissionInClassQuery, string?>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IClassService _classService;
    
    public GetUserPermissionInClassQueryHandler(IApplicationDbContext context, IUser user, IClassService classService)
    {
        _context = context;
        _user = user;
        _classService = classService;       
    }
    
    public async Task<string?> Handle(GetUserPermissionInClassQuery rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId)
            .FirstOrDefaultAsync(cancellationToken);
        
        var inClass = await _classService.IsUserInClass(rq.ClassId);
        if (!inClass)
            throw new ErrorCodeException(ErrorCodes.NOT_FOUND_USER_IN_CLASS);

        var permission = _context.ClassUsers
            .Where(x => x.ClassId == rq.ClassId && x.UserId == _user.UserId)
            .Select(x => x.ShareMode)
            .FirstOrDefault();
        
        return permission.ToString();
    }
}
