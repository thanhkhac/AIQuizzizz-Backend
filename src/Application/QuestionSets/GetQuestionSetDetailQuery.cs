using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Application.Users.Common;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets;

public class GetQuestionSetDetailQuery : IRequest<QuestionSetDetailDto>
{
    public Guid QuestionSetId { get; set; }
}

public class GetQuestionSetDetailQueryHandler : IRequestHandler<GetQuestionSetDetailQuery, QuestionSetDetailDto>
{
    private IApplicationDbContext _context;
    private IQuestionSetService _questionSetService;
    private IUser _user;

    public GetQuestionSetDetailQueryHandler(IApplicationDbContext context, IQuestionSetService questionSetService, IUser user)
    {
        _context = context;
        _questionSetService = questionSetService;
        _user = user;
    }

    public async Task<QuestionSetDetailDto> Handle(GetQuestionSetDetailQuery request, CancellationToken cancellationToken)
    {
        var questionSet = await _questionSetService.GetActiveQuestionSet(request.QuestionSetId, cancellationToken);
        if (questionSet == null) throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);
        
        var canView = await _questionSetService.CanUserViewQuestionSet(_user.UserId, questionSet);
        if (canView == false) throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN, "You are not allowed to view this question set");

        QuestionSetDetailDto result = new QuestionSetDetailDto
        {
            Id = questionSet.Id,
            Name = questionSet.Name,
            Description = questionSet.Description,
            VisibilityMode = questionSet.VisibilityMode.ToString(),
            QuestionCount = questionSet.QuestionCount,
            IsDeleted = questionSet.IsDeleted,
            CreatedAt = questionSet.Created,
            CreatedBy = new CreatedByDto
            {
                Id = questionSet.CreatedByUser!.Id,
                FullName = questionSet.CreatedByUser!.FullName,
                Email = questionSet.CreatedByUser!.Email,
            }
        };

        return result;
    }
}
