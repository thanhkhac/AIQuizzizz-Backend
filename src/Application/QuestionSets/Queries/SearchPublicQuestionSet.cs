using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Tags.Dto;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;


public class SearchPublicQuestionSetByNameQuery : IRequest<PaginatedList<QuestionSetForListResponseDto>>
{
    public string? Name { get; set; }
    public List<Guid>? TagIds { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public string? SortBy { get; set; }
}

public class SearchPublicQuestionSetByNameQueryValidator : AbstractValidator<SearchPublicQuestionSetByNameQuery>
{
    public SearchPublicQuestionSetByNameQueryValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.SortBy)
            .Must(sort => string.IsNullOrEmpty(sort) || new[]
            {
                "Newest", "Rating"
            }.Contains(sort))
            .WithMessage("SortBy must be either 'Newest' or 'Rating'.");
    }
}

public class SearchPublicQuestionSetByNameQueryHandler : IRequestHandler<SearchPublicQuestionSetByNameQuery, PaginatedList<QuestionSetForListResponseDto>>
{

    private readonly IApplicationDbContext _context;
    public SearchPublicQuestionSetByNameQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<QuestionSetForListResponseDto>> Handle(SearchPublicQuestionSetByNameQuery request, CancellationToken cancellationToken)
    {
        //Ngưỡng tin cậy
        const int m = 5;

        double globalAverage = 0;

        var ratedQuestionSets = _context.QuestionSets
            .Include(x => x.QuestionSetTags)
            .ThenInclude(y => y.Tag)
            .Where(q => q.RatingCount > 0);

        if (await ratedQuestionSets.AnyAsync(cancellationToken))
        {
            globalAverage = await ratedQuestionSets.AverageAsync(q => q.RatingAverage, cancellationToken);
        }


        var query = _context.QuestionSets
            .Include(x => x.CreatedByUser).AsQueryable();
            
        if (request.TagIds != null && request.TagIds.Any())
        {
            query = query.Where(x => x.QuestionSetTags.Any(y => request.TagIds.Contains(y.TagId)));
        }
        
        if (!string.IsNullOrEmpty(request.Name))
        {
            var keyword = $"%{request.Name}%";
            query = query.Where(x => EF.Functions.Like(x.Name.ToLower(), keyword));
        }

        if (request.SortBy?.ToLower() == "newest")
        {
            query = query.OrderByDescending(q => q.Created);
        }
        else // để mặc định là rating
        {
            query = query.OrderByDescending(q =>
                (q.RatingCount + m) == 0
                    ? 0
                    : (q.RatingCount / (double)(q.RatingCount + m)) * q.RatingAverage +
                      (m / (double)(q.RatingCount + m)) * globalAverage);
        }

        return await PaginatedList<QuestionSetForListResponseDto>.CreateAsync(
            query.Select(qs => new QuestionSetForListResponseDto
            {
                Id = qs.Id,
                Name = qs.Name,
                Description = qs.Description,
                NumberOfQuestions = qs.QuestionCount,
                CreateBy = qs.CreatedByUser != null ? qs.CreatedByUser.FullName : string.Empty,
                RatingCount = qs.RatingCount,
                RatingAverage = qs.RatingAverage,
                Tags = qs.QuestionSetTags
                    .Where(x => x.Tag != null)
                    .Select(x => new TagForListReponseDto
                    {
                        Id = x.TagId,
                        Name = x.Tag!.Name.Substring(0, 1).ToUpper() + x.Tag.Name.Substring(1),
                        QuestionSetCount = x.Tag.QuestionSetCount
                    })
                    .ToList()
            }),
            request.PageNumber,
            request.PageSize
        );
    }
}
