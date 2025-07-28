using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets;

[Authorize]
public class GetQuestionSetQuestionsForCopyQuery : IRequest<List<CreateUpdateQuestionDto>>
{
    public Guid QuestionSetId { get; set; }
}

public class GetQuestionSetQuestionsForCopyQueryHandler : IRequestHandler<GetQuestionSetQuestionsForCopyQuery, List<CreateUpdateQuestionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IQuestionSetService _questionSetService;
    private readonly IUser _user;
    private readonly IPlanService _planService;


    public GetQuestionSetQuestionsForCopyQueryHandler(IApplicationDbContext context, IQuestionSetService questionSetService, IUser user,
        IPlanService planService)
    {
        _context = context;
        _questionSetService = questionSetService;
        _user = user;
        _planService = planService;
    }

    public async Task<List<CreateUpdateQuestionDto>> Handle(GetQuestionSetQuestionsForCopyQuery request, CancellationToken cancellationToken)
    {
        var questionSet = await _questionSetService.GetActiveQuestionSet(request.QuestionSetId, cancellationToken);
        if (questionSet == null) throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);
        
        //TODO: bổ sung hàm khác để những bộ public cũng có thể lấy dữ liêệu
        var canUserEdit = await _questionSetService.CanUserEditQuestionSet(_user.UserId!.Value, questionSet.Id);
        if (canUserEdit == false) throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN, "You are not allowed to view this question set");

        var canCopyOrImport = await _planService.CanCopyOrImportQuestionSet(_user.UserId!.Value);
        if (canCopyOrImport == false)
            throw new ErrorCodeException(ErrorCodes.PLAN_REQUIRE_PLAN,
                "You are not allowed to copy or import question set. Please buy a plan to copy or import question set.");

        var questions = await _context.Questions
            .Where(q => q.QuestionSetId == request.QuestionSetId && q.IsDeleted == false)
            .ToListAsync(cancellationToken);

        // Chuyển đổi Questions thành CreateUpdateQuestionDto
        var questionDtos = questions
            .Select(q => CreateUpdateQuestionDto.Deserializer.Deserialize(q))
            .ToList();

        return questionDtos;
    }
}
