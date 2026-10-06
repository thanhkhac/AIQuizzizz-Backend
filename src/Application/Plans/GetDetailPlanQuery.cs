using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Plans.Dto;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Plans;

[Authorize]
public class GetDetailPlanQuery : IRequest<PlanDetailDto>
{
    public required Guid PlanId { get; init; } 
}

public class GetDetailPlanQueryValidator : AbstractValidator<GetDetailPlanQuery>
{
    public GetDetailPlanQueryValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("PlanId không được để trống");
    }
}

public class GetDetailPlanQueryHandler : IRequestHandler<GetDetailPlanQuery, PlanDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetDetailPlanQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<PlanDetailDto> Handle(GetDetailPlanQuery rq, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans
            .Where(x => x.Id.Equals(rq.PlanId) && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (plan == null)
            throw new ErrorCodeException(ErrorCodes.PLAN_NOT_FOUND, "Không tìm thấy plan");

        return new PlanDetailDto
        {
            Id = rq.PlanId,
            Name = plan.Name,
            Price = plan.Price,
            Duration = plan.Duration,
            Unit = plan.Unit,
            CanCopyOrImportQuestionSet = plan.CanCopyOrImportQuestionSet,
            CanUploadImage = plan.CanUploadImage,
            CanUploadVideo = plan.CanUploadVideo,
            CanLearn = plan.CanLearn,
            CanOpenTest = plan.CanOpenTest,
            IsActive = plan.IsActive
        };
    }
}
