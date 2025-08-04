using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.Tags.Dto;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets.Services;

public interface IQuestionSetService
{
    public Task<bool> CanUserEditQuestionSet(Guid userId, Guid questionSetId);
    public Task<bool> CanUserViewQuestionSet(Guid? userId, Guid questionSetId);
    public Task<bool> CanUserDeleteQuestionSet(Guid userId, Guid questionSetId);
    public Task<bool> CanUserViewQuestionSet(Guid? userId, QuestionSet questionSet);
    public Task<QuestionSetPermissionsDto> GetPermissions(Guid userId, Guid questionSetId);
    public Task<QuestionSet?> GetActiveQuestionSet(Guid questionSetId, CancellationToken cancellationToken);
    public Task<PaginatedList<QuestionSetForListResponseDto>> SearchPublicQuestionSetsAsync(
        string? name,
        List<Guid>? tagIds,
        string? sortBy,
        int confidenceFactor,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
        
    Task<PaginatedList<QuestionSetForListResponseDto>> SearchOwnAndSharedQuestionSetsAsync(
        Guid userId,
        string? name,
        int pageNumber,
        int pageSize,
        string? filterBy,
        string? sortBy,
        CancellationToken cancellationToken);
        
}

