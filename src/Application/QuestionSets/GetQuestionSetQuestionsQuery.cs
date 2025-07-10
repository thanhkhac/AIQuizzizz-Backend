using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Questions.Services;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets;

public class GetQuestionSetQuestionsQuery : IRequest<List<QuestionResponseDto>>
{
    public Guid QuestionSetId { get; set; }
}

public class GetQuestionSetQuestionsQueryHandler : IRequestHandler<GetQuestionSetQuestionsQuery, List<QuestionResponseDto>>
{

    private IApplicationDbContext _context;
    private IQuestionSetService _questionSetService;
    private IQuestionService _questionService;
    private IUser _user;

    public GetQuestionSetQuestionsQueryHandler(IApplicationDbContext context, IQuestionSetService questionSetService, IUser user,
        IQuestionService questionService)
    {
        _context = context;
        _questionSetService = questionSetService;
        _user = user;
        _questionService = questionService;
    }

    public async Task<List<QuestionResponseDto>> Handle(GetQuestionSetQuestionsQuery request, CancellationToken cancellationToken)
    {
        var questionSet = await _context.QuestionSets
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x =>
                x.Id == request.QuestionSetId
                && x.IsDeleted == false
                && x.CreatedByUser != null
                && x.CreatedByUser.IsDeleted == false
                && x.CreatedByUser.IsBanned == false, cancellationToken: cancellationToken);

        if (questionSet == null) throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);
        var canView = await _questionSetService.CanUserViewQuestionSet(_user.UserId, questionSet);
        if (canView == false) throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN, "You are not allowed to view this question set");

        var result = await _questionService.GetQuestionsBySetIdForDetailAndLearnAsync(request.QuestionSetId, _user.UserId);
        return result;
    }
}
