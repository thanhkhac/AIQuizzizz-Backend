using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.QuestionSets.Dtos;

namespace CleanArchitectureBase.Application.QuestionSets;

public class SearchPublicQuestionSetByNameQuery : IRequest<PaginatedList<SearchQuestionSetDto>>
{
    public string? Name { get; set; }
    public Guid? TagId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchPublicQuestionSetByNameQueryValidator : AbstractValidator<SearchPublicQuestionSetByNameQuery>
{
    public SearchPublicQuestionSetByNameQueryValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
            
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);
    }
}

public class SearchPublicQuestionSetByNameQueryHandler : IRequestHandler<SearchPublicQuestionSetByNameQuery, PaginatedList<SearchQuestionSetDto>>
{

    private readonly IApplicationDbContext _context;
    public SearchPublicQuestionSetByNameQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<SearchQuestionSetDto>> Handle(SearchPublicQuestionSetByNameQuery request, CancellationToken cancellationToken)
    {
        //Ngưỡng tin cậy
        const int m = 5;

        var globalAverage = await _context.QuestionSets
            .Where(q => q.RatingCount > 0)
            .AverageAsync(q => q.RatingAverage, cancellationToken);

        var query = _context.QuestionSets
            .Include(x => x.CreatedByUser).AsQueryable();
        if (request.TagId != null)
        {
            query = query.Include(x => x.QuestionSetTags).Where(x => x.QuestionSetTags.Any(y => y.TagId == request.TagId));
        }
        if (!string.IsNullOrEmpty(request.Name))
        {
            var keyword = $"%{request.Name}%";
            query = query.Where(x => EF.Functions.Like(x.Name, keyword));
        }

        query = query.OrderByDescending(q =>
            (q.RatingCount + m) == 0 
                ? 0 
                : (q.RatingCount / (double)(q.RatingCount + m)) * q.RatingAverage +
                  (m / (double)(q.RatingCount + m)) * globalAverage);

        return await PaginatedList<SearchQuestionSetDto>.CreateAsync(
            query.Select(qs => new SearchQuestionSetDto
            {
                Id = qs.Id,
                Name = qs.Name,
                Description = qs.Description,
                NumberOfQuestions = qs.QuestionCount,
                CreateBy = qs.CreatedByUser != null ? qs.CreatedByUser.FullName : string.Empty,
                RatingCount = qs.RatingCount,
                RatingAverage = qs.RatingAverage
            }),
            request.PageNumber,
            request.PageSize
        );
    }
}
