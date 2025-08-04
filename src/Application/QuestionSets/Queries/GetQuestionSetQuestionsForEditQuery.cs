using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Application.Plans.Service;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets;

[Authorize]
public class GetQuestionSetQuestionsForEditQuery : IRequest<List<CreateUpdateQuestionDto>>
{
    public Guid QuestionSetId { get; set; }
}

public class GetQuestionSetQuestionsForEditQueryHandler : IRequestHandler<GetQuestionSetQuestionsForEditQuery, List<CreateUpdateQuestionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IQuestionSetService _questionSetService;
    private readonly IUser _user;

    public GetQuestionSetQuestionsForEditQueryHandler(IApplicationDbContext context, IQuestionSetService questionSetService, IUser user,
        IPlanService planService)
    {
        _context = context;
        _questionSetService = questionSetService;
        _user = user;
    }

    public async Task<List<CreateUpdateQuestionDto>> Handle(GetQuestionSetQuestionsForEditQuery request, CancellationToken cancellationToken)
    {
        var questionSet = await _questionSetService.GetActiveQuestionSet(request.QuestionSetId, cancellationToken);
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);

        var canUserEdit = await _questionSetService.CanUserEditQuestionSet(_user.UserId!.Value, questionSet.Id);
        if (canUserEdit == false) throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN, "You are not allowed to view this question set");

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
