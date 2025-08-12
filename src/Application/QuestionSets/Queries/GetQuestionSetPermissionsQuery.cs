using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Services;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

public record GetQuestionSetPermissionsQuery : IRequest<QuestionSetPermissionsDto>
{
    public Guid QuestionSetId { get; init; }
}

public class GetQuestionSetPermissionsQueryHandler : IRequestHandler<GetQuestionSetPermissionsQuery, QuestionSetPermissionsDto>
{
    private readonly IQuestionSetService _questionSetService;
    private readonly IUser _currentUser;

    public GetQuestionSetPermissionsQueryHandler(
        IQuestionSetService questionSetService,
        IUser currentUser)
    {
        _questionSetService = questionSetService;
        _currentUser = currentUser;
    }

    public async Task<QuestionSetPermissionsDto> Handle(GetQuestionSetPermissionsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return new QuestionSetPermissionsDto
            {
                CanEdit = false,
                CanDelete = false
            };

        return await _questionSetService.GetPermissions(_currentUser.UserId.Value, request.QuestionSetId);
    }
}
