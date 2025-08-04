using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Application.Plans.Service;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Questions.Services;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

[Authorize]
public class GetQuestionSetLearnQuestionsQuery : IRequest<GetQuestionSetLearnQuestionsQueryDto>
{
    public Guid QuestionSetId { get; set; }
    public int QuestionCount { get; set; }
}

public class GetQuestionSetLearnQuestionsQueryDto
{
    public int CompletedQuestionCount { get; set; }
    public int TotalQuestionCount { get; set; }
    public  List<QuestionResponseDto> Questions { get; set; } = new ();
}

public class GetQuestionSetLearnQuestionsQueryValidator : AbstractValidator<GetQuestionSetLearnQuestionsQuery>
{
    public GetQuestionSetLearnQuestionsQueryValidator()
    {
        RuleFor(x => x.QuestionCount)
            .GreaterThan(0).WithMessage("Số lượng câu hỏi phải lớn hơn 0")
            .LessThanOrEqualTo(10).WithMessage("Số lượng câu hỏi tối đa là 10");

        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionSetId không được để trống");
    }

}

public class GetQuestionSetLearnQuestionsQueryHandler : IRequestHandler<GetQuestionSetLearnQuestionsQuery,GetQuestionSetLearnQuestionsQueryDto>
{

    private IQuestionSetService _questionSetService;
    private IQuestionService _questionService;
    private IPlanService _planService;
    private IUser _user;

    public GetQuestionSetLearnQuestionsQueryHandler(IQuestionSetService questionSetService, IUser user,
        IQuestionService questionService, IPlanService planService)
    {
        _questionSetService = questionSetService;
        _user = user;
        _questionService = questionService;
        _planService = planService;
    }

    public async Task<GetQuestionSetLearnQuestionsQueryDto> Handle(GetQuestionSetLearnQuestionsQuery request, CancellationToken cancellationToken)
    {
        // Check xem người dùng có quyền học không
        if (!await _planService.CanLearn(_user.UserId!.Value))
            throw new ErrorCodeException(ErrorCodes.PLAN_REQUIRE_PLAN, "You are not allowed to learn");

        var questionSet = await _questionSetService.GetActiveQuestionSet(request.QuestionSetId, cancellationToken);
        if (questionSet == null) throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);
        
        var canView = await _questionSetService.CanUserViewQuestionSet(_user.UserId, questionSet);
        if (canView == false) throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN, "You are not allowed to view this question set");

        var result = await _questionService.GetQuestionsBySetIdForLearnAsync(request.QuestionSetId, _user.UserId!.Value, request.QuestionCount,
            cancellationToken);
            
        var finalResult = new GetQuestionSetLearnQuestionsQueryDto
        {
            CompletedQuestionCount = result.CompletedQuestions,
            TotalQuestionCount = result.TotalQuestions,
            Questions = result.Questions
        };
            
        return finalResult;
    }
}
