using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Questions.Services;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets;

[Authorize]
public class GetQuestionSetLearnQuestionsQuery : IRequest<List<QuestionResponseDto>>
{
    public Guid QuestionSetId { get; set; }
    public int QuestionCount { get; set; }
}

public class GetQuestionSetLearnQuestionsQueryHandler : IRequestHandler<GetQuestionSetLearnQuestionsQuery, List<QuestionResponseDto>>
{

    private IApplicationDbContext _context;
    private IQuestionSetService _questionSetService;
    private IQuestionService _questionService;
    private IPlanService _planService;
    private IUser _user;

    public GetQuestionSetLearnQuestionsQueryHandler(IApplicationDbContext context, IQuestionSetService questionSetService, IUser user,
        IQuestionService questionService, IPlanService planService)
    {
        _context = context;
        _questionSetService = questionSetService;
        _user = user;
        _questionService = questionService;
        _planService = planService;
    }

    public async Task<List<QuestionResponseDto>> Handle(GetQuestionSetLearnQuestionsQuery request, CancellationToken cancellationToken)
    {
        // Check xem người dùng có quyền học không
        if (!await _planService.CanLearn(_user.UserId!.Value))
            throw new ErrorCodeException(ErrorCodes.PLAN_REQUIRE_PLAN, "You are not allowed to learn");

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

        var result = await _questionService.GetQuestionsBySetIdForLearnAsync(request.QuestionSetId, _user.UserId!.Value, request.QuestionCount,
            cancellationToken);
        return result;
    }
}
