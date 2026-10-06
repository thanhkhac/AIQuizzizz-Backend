using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Plans;

[Authorize(Roles = Domain.Constants.Roles.Administrator)]
public class CreateUpdatePlanCommand : IRequest<Guid>
{
    public Guid? PlanId { get; set; }
    public string? Name { get; set; }
    public required int Price { get; set; }
    public int Duration { get; set; }
    public string? Unit { get; set; }
    public bool CanLearn { get; set; } = false;
    public bool CanOpenTest { get; set; } = false;
    public bool CanCopyOrImportQuestionSet { get; set; } = false;
    public bool CanUploadImage { get; set; } = false;
    public bool CanUploadVideo { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

public class CreatePlanCommandValidator : AbstractValidator<CreateUpdatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Duration)
            .GreaterThan(0);

        RuleFor(x => x.Unit)
            .NotEmpty()
            .Must(unit => PlanUnit.Normalize(unit) != null)
            .WithMessage("Đơn vị thời gian phải là 'Day', 'Month' hoặc 'Year'");
    }
}

public class CreatePlanCommandHandler : IRequestHandler<CreateUpdatePlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CreatePlanCommandHandler(IApplicationDbContext context, IIdentityService identityService, IUser user)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Guid> Handle(CreateUpdatePlanCommand rq, CancellationToken cancellationToken)
    {
        if (rq.PlanId != null)
        {
            var existedPlan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == rq.PlanId, cancellationToken);
            if (existedPlan == null)
                throw new ErrorCodeException(ErrorCodes.PLAN_NOT_FOUND);

            existedPlan.Name = rq.Name!;
            existedPlan.Duration = rq.Duration;
            existedPlan.Unit = PlanUnit.Normalize(rq.Unit)!;
            existedPlan.CanLearn = rq.CanLearn;
            existedPlan.CanOpenTest = rq.CanOpenTest;
            existedPlan.CanCopyOrImportQuestionSet = rq.CanCopyOrImportQuestionSet;
            existedPlan.CanUploadImage = rq.CanUploadImage;
            existedPlan.CanUploadVideo = rq.CanUploadVideo;
            existedPlan.IsActive = rq.IsActive;
            if (existedPlan.Price != rq.Price)
            {
                var latestPlanPriceHistory = await _context.PlanPriceHistories
                    .Where(x => x.PlanId == rq.PlanId)
                    .OrderByDescending(x => x.DateStart)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (latestPlanPriceHistory != null)
                    latestPlanPriceHistory.DateFinish = DateTimeOffset.UtcNow;
                var newPlanPriceHistory = new PlanPriceHistory
                {
                    PlanId = rq.PlanId.Value,
                    Price = rq.Price,
                    DateStart = DateTimeOffset.UtcNow,
                    DateFinish = null,
                };
                _context.PlanPriceHistories.Add(newPlanPriceHistory);
            }

            existedPlan.Price = rq.Price;

            await _context.SaveChangesAsync(cancellationToken);

            return rq.PlanId.Value;
        }
        else
        {
            var plan = new Plan
            {
                Id = Guid.NewGuid(),
                Name = rq.Name!,
                Price = rq.Price,
                Duration = rq.Duration,
                Unit = PlanUnit.Normalize(rq.Unit)!,
                CanLearn = rq.CanLearn,
                CanOpenTest = rq.CanOpenTest,
                CanCopyOrImportQuestionSet = rq.CanCopyOrImportQuestionSet,
                CanUploadImage = rq.CanUploadImage,
                CanUploadVideo = rq.CanUploadVideo,
                IsActive = rq.IsActive,
            };

            var newPlanPriceHistory = new PlanPriceHistory
            {
                PlanId = plan.Id,
                Price = rq.Price,
                DateStart = DateTimeOffset.UtcNow,
                DateFinish = null,
            };

            _context.Plans.Add(plan);
            _context.PlanPriceHistories.Add(newPlanPriceHistory);
            await _context.SaveChangesAsync(cancellationToken);

            return plan.Id;
        }
    }
}

/// <summary>Chuẩn hoá đơn vị thời gian của gói về "Day" | "Month" | "Year" (không phân biệt hoa thường)</summary>
public static class PlanUnit
{
    public static string? Normalize(string? unit) => unit?.Trim().ToLowerInvariant() switch
    {
        "day" => "Day",
        "month" => "Month",
        "year" => "Year",
        _ => null
    };
}
