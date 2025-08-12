using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Services;
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
            
        RuleFor(x => x.SortBy)
            .Must(filter => string.IsNullOrEmpty(filter) || new[]
            {
                "RecentAccess", "Newest"
            }.Contains(filter))
            .WithMessage("FilterBy must be either \"RecentAccess\" or \"Newest\".");
    }
}

public class SearchOwnAndSharedQuestionSetQueryHandler : IRequestHandler<SearchOwnAndSharedQuestionSetQuery, PaginatedList<QuestionSetForListResponseDto>>
{
    private readonly IQuestionSetService _questionSetService;
    private readonly IUser _user;
    public SearchOwnAndSharedQuestionSetQueryHandler(IApplicationDbContext context, IUser user, IQuestionSetService questionSetService)
    {
        _user = user;
        _questionSetService = questionSetService;
    }

    public async Task<PaginatedList<QuestionSetForListResponseDto>> Handle(SearchOwnAndSharedQuestionSetQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.UserId!.Value;
        return await _questionSetService.SearchOwnAndSharedQuestionSetsAsync(
            userId,
            request.Name,
            request.PageNumber,
            request.PageSize,
            request.FilterBy,
            request.SortBy,
            cancellationToken);
    }
}
