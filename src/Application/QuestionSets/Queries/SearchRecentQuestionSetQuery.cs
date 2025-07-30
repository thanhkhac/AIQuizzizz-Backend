using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Tags.Dto;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

[Authorize]
public class SearchRecentQuestionSetQuery : IRequest<PaginatedList<QuestionSetForListResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchRecentQuestionSetQueryValidator : AbstractValidator<SearchRecentQuestionSetQuery>
{
    public SearchRecentQuestionSetQueryValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);
    }
}

public class SearchRecentQuestionSetQueryHandler : IRequestHandler<SearchRecentQuestionSetQuery, PaginatedList<QuestionSetForListResponseDto>>
{

    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    public SearchRecentQuestionSetQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<PaginatedList<QuestionSetForListResponseDto>> Handle(SearchRecentQuestionSetQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.UserId!.Value;


        var query = _context.UserQuestionSetAccessHistories
            .Where(ah => ah.UserId == userId)
            .GroupBy(ah => ah.QuestionSetId)
            .Select(g => new
            {
                QuestionSetId = g.Key,
                LastAccess = g.Max(x => x.LastAccess)
            })
            .Join(_context.QuestionSets
                    .Include(qs => qs.CreatedByUser)
                    .Include(qs => qs.QuestionSetUsers)
                    .Include(qs => qs.AccessHistories)
                    .Include(qs => qs.ClassQuestionSets).ThenInclude(cqs => cqs.Class).ThenInclude(c => c.ClassUsers)
                    .Include(qs => qs.QuestionSetTags).ThenInclude(qst => qst.Tag),
                g => g.QuestionSetId,
                qs => qs.Id,
                (g, qs) => new
                {
                    qs,
                    g.LastAccess
                })
            .Where(x =>
                x.qs.VisibilityMode == QuestionSetVisibilityMode.Public ||
                (x.qs.VisibilityMode == QuestionSetVisibilityMode.Private &&
                 x.qs.QuestionSetUsers.Any(qsu => qsu.UserId == userId)) ||
                (x.qs.VisibilityMode == QuestionSetVisibilityMode.OnlyClass &&
                 x.qs.ClassQuestionSets.Any(cqs =>
                     cqs.Class.ClassUsers.Any(cu => cu.UserId == userId)))
            )
            .OrderByDescending(x => x.LastAccess)
            .Select(x => new QuestionSetForListResponseDto
            {
                Id = x.qs.Id,
                Name = x.qs.Name,
                Description = x.qs.Description,
                NumberOfQuestions = x.qs.QuestionCount,
                CreateBy = x.qs.CreatedByUser != null ? x.qs.CreatedByUser.FullName : string.Empty,
                CreatedAt = x.qs.Created,
                CreatedById = x.qs.CreatedBy,
                RatingCount = x.qs.RatingCount,
                RatingAverage = x.qs.RatingAverage,
                LastAccessByMe = x.LastAccess,
                Tags = x.qs.QuestionSetTags
                    .Where(tag => tag.Tag != null)
                    .Select(tag => new TagForListReponseDto
                    {
                        Id = tag.TagId,
                        Name = tag.Tag!.Name.Substring(0, 1).ToUpper() + tag.Tag.Name.Substring(1),
                        QuestionSetCount = tag.Tag.QuestionSetCount
                    }).ToList()
            });


        return await PaginatedList<QuestionSetForListResponseDto>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize
        );
    }
}
