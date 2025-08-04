using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Questions.Services;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Application.Tags.Dto;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

public class SearchPublicQuestionSetQuery : IRequest<PaginatedList<QuestionSetForListResponseDto>>
{
    public string? Name { get; set; }
    public List<Guid>? TagIds { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public string? SortBy { get; set; }
}

public class SearchPublicQuestionSetByNameQueryValidator : AbstractValidator<SearchPublicQuestionSetQuery>
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

public class SearchPublicQuestionSetByNameQueryHandler : IRequestHandler<SearchPublicQuestionSetQuery, PaginatedList<QuestionSetForListResponseDto>>
{
    private readonly IQuestionSetService _questionSetService;
    public SearchPublicQuestionSetByNameQueryHandler(IQuestionSetService questionSetService)
    {
        _questionSetService = questionSetService;
    }

    public async Task<PaginatedList<QuestionSetForListResponseDto>> Handle(SearchPublicQuestionSetQuery request, CancellationToken cancellationToken)
    {
        var result = await _questionSetService.SearchPublicQuestionSetsAsync
        (
            name: request.Name,
            tagIds: request.TagIds,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            sortBy: request.SortBy,
            cancellationToken: cancellationToken,
            confidenceFactor: 5
        );

        return result;
    }
}
