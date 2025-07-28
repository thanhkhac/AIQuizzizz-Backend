using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Dtos;
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
        var userId = _user.UserId;
        var normalizedFilter = request.FilterBy?.Trim();
        var keyword = request.Name?.Trim().ToLower();

        var query = _context.QuestionSets
            .Include(q => q.CreatedByUser)
            .Include(q => q.QuestionSetTags)
            .ThenInclude(y => y.Tag)
            .Include(q => q.QuestionSetUsers)
            .Where(q => q.QuestionSetUsers.Any(qsu => qsu.UserId == userId)); 

        if (normalizedFilter == "CreatedByMe")
        {
            query = query.Where(q =>
                q.QuestionSetUsers.Any(qsu =>
                    qsu.UserId == userId && qsu.ShareMode == QuestionSetUserShareMode.Owner));
        }
        else if (normalizedFilter == "ShareWithMe")
        {
            query = query.Where(q =>
                q.QuestionSetUsers.Any(qsu =>
                    qsu.UserId == userId && qsu.ShareMode != QuestionSetUserShareMode.Owner));
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(x => EF.Functions.Like(x!.Name.ToLower(), keyword));
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
