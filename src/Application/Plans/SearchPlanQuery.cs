using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Plans.Dto;

namespace CleanArchitectureBase.Application.Plans;

public class SearchPlanQuery : IRequest<List<PlanDetailDto>>
{
    public bool? IsActive { get; set; }
}

public class SearchPlanQueryHandler : IRequestHandler<SearchPlanQuery, List<PlanDetailDto>>
{
    private readonly IApplicationDbContext _context;
    public SearchPlanQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlanDetailDto>> Handle(SearchPlanQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Plans.IgnoreQueryFilters()
            .Where(x => x.IsDeleted == false);
        if (request.IsActive.HasValue)
        {
            request.IsActive = request.IsActive.Value;
        }

        query = query.OrderBy(x => x.Price);
        var result = await query.Select(x => new PlanDetailDto
        {
            Id = x.Id,
            Name = x.Name,
            Price = x.Price,
            Duration = x.Duration,
            Unit = x.Unit,
            CanLearn = x.CanLearn,
            CanOpenTest = x.CanOpenTest,
            CanCopyOrImportQuestionSet = x.CanCopyOrImportQuestionSet,
            IsActive = x.IsActive
        }).ToListAsync(cancellationToken);
        return result;
    }
}
