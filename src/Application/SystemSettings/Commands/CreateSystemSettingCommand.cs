using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.SystemSettings.Commands;

[Authorize ( Roles = Domain.Constants.Roles.Administrator + "," + Domain.Constants.Roles.Moderator )]
public class CreateSystemSettingCommand : IRequest<Guid>
{
    public int InputCostPerMillionTokens { get; set; }
    public int OutputCostPerMillionTokens { get; set; }
    public int FixedSystemFee { get; set; }
    public int MaxInputToken { get; set; }
    public int MaxOutputToken { get; set; }
}

public class CreateSystemSettingCommandValidator : AbstractValidator<CreateSystemSettingCommand>
{
    public CreateSystemSettingCommandValidator()
    {
        RuleFor(x => x.InputCostPerMillionTokens)
            .GreaterThanOrEqualTo(0).WithMessage("Input cost phải >= 0");

        RuleFor(x => x.OutputCostPerMillionTokens)
            .GreaterThanOrEqualTo(0)
            .LessThan(1_000_000).WithMessage("Output cost phải >= 0");

        RuleFor(x => x.FixedSystemFee)
            .GreaterThanOrEqualTo(0).WithMessage("System fee phải >= 0");

        RuleFor(x => x.MaxInputToken)
            .GreaterThan(0).WithMessage("Max input token phải > 0");

        RuleFor(x => x.MaxOutputToken)
            .GreaterThan(0)
            .LessThan(65_000)
            .WithMessage("Max output token phải > 0");
    }
}

public class CreateSystemSettingCommandHandler : IRequestHandler<CreateSystemSettingCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    
    public CreateSystemSettingCommandHandler(IApplicationDbContext context, IIdentityService identityService, IUser user)
    {
        _context = context;
        _identityService = identityService;
        _user = user;
    }
    
    public async Task<Guid> Handle(CreateSystemSettingCommand rq, CancellationToken cancellationToken)
    {

        var systemSetting = new SystemSetting
        {
            Id = Guid.NewGuid(),
            InputCostPerMillionTokens = rq.InputCostPerMillionTokens,
            OutputCostPerMillionTokens = rq.OutputCostPerMillionTokens,
            FixedSystemFee = rq.FixedSystemFee,
            MaxInputToken = rq.MaxInputToken,
            MaxOutputToken = rq.MaxOutputToken
        };
        
        _context.SystemSettings.Add(systemSetting);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return systemSetting.Id;
    }
}
