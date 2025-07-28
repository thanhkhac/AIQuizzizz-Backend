using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Plans;

[Authorize]
public class CreatePlanCommand : IRequest<Guid>
{
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public int DayDuration { get; set; }
    public bool CanLearn { get; set; } = false;
    public bool CanOpenTest { get; set; } = false;
    public bool CanCopyOrImportQuestionSet { get; set; } = false;
}

public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name không được để trống");
        
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price không được < 0");
        
        RuleFor(x => x.DayDuration)
            .GreaterThan(0).WithMessage("DayDuration không được < 0");
    }
}

public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    
    public CreatePlanCommandHandler(IApplicationDbContext context, IIdentityService identityService, IUser user)
    {
        _context = context;
        _identityService = identityService;
        _user = user;       
    }
    
    public async Task<Guid> Handle(CreatePlanCommand rq, CancellationToken cancellationToken)
    {
        var isAdmin = await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator);
        if (!isAdmin)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION, "Không có quyền tạo plan");

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = rq.Name,
            Price = rq.Price,
            DayDuration = rq.DayDuration,
            CanLearn = rq.CanLearn,
            CanOpenTest = rq.CanOpenTest,
            CanCopyOrImportQuestionSet = rq.CanCopyOrImportQuestionSet,
        };

        _context.Plans.Add(plan);

        await _context.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}
