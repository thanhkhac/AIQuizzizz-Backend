using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Tags.Dto;

namespace CleanArchitectureBase.Application.Tags;

public class SearchTagQuery : IRequest<List<TagForListReponseDto>>
{
    public string? Name { get; set; }
    public int PageSize { get; set; }
}

public class SearchTagQueryValidator : AbstractValidator<SearchTagQuery>
{
    public SearchTagQueryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Tag name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}

public class SearchTagQueryHandler : IRequestHandler<SearchTagQuery, List<TagForListReponseDto>>
{
    private readonly IApplicationDbContext _context;
    public SearchTagQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagForListReponseDto>> Handle(SearchTagQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tags.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var keyword = $"%{request.Name}%";
            query = query.Where(x => EF.Functions.Like(x.Name.ToLower(), keyword.ToLower()));
        }
        query = query.OrderByDescending(x => x.QuestionSetCount);
        query = query.Take(request.PageSize);
        var result = await query.Select(x => new TagForListReponseDto
        {
            Id = x.Id,
            Name = x.Name,
            QuestionSetCount = x.QuestionSetCount,
        }).ToListAsync(cancellationToken);
        return result;
    }
    
}
