using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.Tags.Dto;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

[Authorize]
public class SearchOwnAndSharedQuestionSetQuery : IRequest<PaginatedList<QuestionSetForListResponseDto>>
{
    public string? Name { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public string? FilterBy { get; set; }
    public string? SortBy { get; set; } // "RecentAccess" or "Newest"
}

public class SearchOwnAndSharedQuestionSetQueryValidator : AbstractValidator<SearchOwnAndSharedQuestionSetQuery>
{
    public SearchOwnAndSharedQuestionSetQueryValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.FilterBy)
            .Must(filter => string.IsNullOrEmpty(filter) || new[]
            {
                "CreatedByMe", "ShareWithMe"
            }.Contains(filter))
            .WithMessage("FilterBy must be either 'CreatedByMe' or 'ShareWithMe'.");
    }
}

public class SearchOwnAndSharedQuestionSetQueryHandler : IRequestHandler<SearchOwnAndSharedQuestionSetQuery, PaginatedList<QuestionSetForListResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    public SearchOwnAndSharedQuestionSetQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<PaginatedList<QuestionSetForListResponseDto>> Handle(SearchOwnAndSharedQuestionSetQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.UserId!.Value;
        var normalizedFilter = request.FilterBy;
        var isFilterCreatedByMe = normalizedFilter == "CreatedByMe";
        var isFilterSharedWithMe = normalizedFilter == "ShareWithMe";

        var query = _context.QuestionSets
            .Include(q => q.CreatedByUser)
            .Include(q => q.QuestionSetUsers)
            .Include(q => q.AccessHistories)
            .Where(q => q.QuestionSetUsers.Any(qsu => qsu.UserId == userId));

        if (isFilterCreatedByMe)
        {
            query = query.Where(q =>
                q.QuestionSetUsers.Any(qsu =>
                    qsu.UserId == userId && qsu.ShareMode == QuestionSetUserShareMode.Owner));
        }
        else if (isFilterSharedWithMe)
        {
            query = query.Where(q =>
                q.QuestionSetUsers.Any(qsu =>
                    qsu.UserId == userId && qsu.ShareMode != QuestionSetUserShareMode.Owner));
        }


        if (!string.IsNullOrEmpty(request.Name))
        {
            var keyword = $"%{request.Name}%";
            query = query.Where(x => EF.Functions.Like(x.Name.ToLower(), keyword));
        }

        query = query.Include(q => q.QuestionSetTags).ThenInclude(qst => qst.Tag);

        var projectedQuery = query
            .Select(q => new
            {
                QuestionSet = q,
                LastAccessedAt = q.AccessHistories
                    .Where(ah => ah.UserId == userId)
                    .Select(ah => (DateTimeOffset?)ah.LastAccess)
                    .FirstOrDefault(),
                IsOwner = q.QuestionSetUsers
                    .Any(qsu => qsu.UserId == userId && qsu.ShareMode == QuestionSetUserShareMode.Owner)
            });
        
        if (string.IsNullOrEmpty(normalizedFilter))
        {
            projectedQuery = projectedQuery.Where(x =>
                x.IsOwner || (!x.IsOwner && x.LastAccessedAt != null));
        }
        else if (isFilterSharedWithMe)
        {
            projectedQuery = projectedQuery.Where(x => x.LastAccessedAt != null);
        }
        
        if (request.SortBy == "Newest")
            projectedQuery = projectedQuery.OrderByDescending(q => q.QuestionSet.Created);
        else //RecentAccess
            projectedQuery = projectedQuery
                .OrderBy(x => x.LastAccessedAt.HasValue ? 0 : 1)
                .ThenByDescending(x => x.LastAccessedAt);


        var pagedResult = await PaginatedList<QuestionSetForListResponseDto>.CreateAsync(
            projectedQuery.Select(x => new QuestionSetForListResponseDto
            {
                Id = x.QuestionSet.Id,
                Name = x.QuestionSet.Name,
                Description = x.QuestionSet.Description,
                RatingAverage = x.QuestionSet.RatingAverage,
                RatingCount = x.QuestionSet.RatingCount,
                CreateBy = x.QuestionSet.CreatedByUser!.FullName,
                CreatedById = x.QuestionSet.CreatedByUser!.Id,
                CreatedAt = x.QuestionSet.Created,
                TotalQuestionCount = x.QuestionSet.QuestionCount,

                Tags = x.QuestionSet.QuestionSetTags.Select(t => new TagForListReponseDto
                    {
                        Id = t.TagId,
                        Name = t.Tag!.Name.Substring(0,
                                       1)
                                   .ToUpper() +
                               t.Tag.Name.Substring(1),
                    })
                    .ToList(),
                LastAccessByMe = x.LastAccessedAt,
                VisibilityMode = x.QuestionSet.VisibilityMode.ToString(),
                CompletedQuestionCount = 0
            }),
            request.PageNumber,
            request.PageSize);

        return pagedResult;
    }
}
