using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Plans;

public class NumberOfNewClassDto
{
    public int Month { get; set; }
    public int Revenue { get; set; }
}

[Authorize]
public class GetNumberOfNewClassByYearQuery : IRequest<List<NumberOfNewClassDto>>
{
    public int Year { get; set; }
}

public class GetNumberOfNewClassByYearQueryValidator : AbstractValidator<GetNumberOfNewClassByYearQuery>
{
    public GetNumberOfNewClassByYearQueryValidator()
    {
        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(2000).WithMessage("Year phải lớn hơn 2000");
    }
}

public class GetNumberOfNewClassByYearQueryHandler : IRequestHandler<GetNumberOfNewClassByYearQuery, List<NumberOfNewClassDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    
    public GetNumberOfNewClassByYearQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IUser user)
    {
        _context = context;
        _identityService = identityService;
        _user = user;
    }
    
    public async Task<List<NumberOfNewClassDto>> Handle(GetNumberOfNewClassByYearQuery request, CancellationToken cancellationToken)
    {
        bool isAdmin = await _identityService
            .IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator);
        if (!isAdmin)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION);

        var numberOfClass = _context.Classes
            .Where(x => x.Created.Year == request.Year)
            .Select(x => new { CreateAt = x.Created.Month, Id = x.Id });
        
        var result = new List<NumberOfNewClassDto>(); 

        for (int i = 1; i <= 12; i++)
        {
            var numberOfClassByMonth = numberOfClass.Where(x => x.CreateAt == i).Count();
            result.Add(new NumberOfNewClassDto
            {
                Month = i,
                Revenue = numberOfClassByMonth
            });
        }
        
        return result;
    }
}
