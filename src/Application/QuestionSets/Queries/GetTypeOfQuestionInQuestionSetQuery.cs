using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Plans.Service;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

public class TypeOfQuestionInQuestionSetDto
{
    public string? Type { get; set; }
    public int Count { get; set; }
}

[Authorize]
public class GetTypeOfQuestionInQuestionSetQuery : IRequest<List<TypeOfQuestionInQuestionSetDto>>
{
    public Guid QuestionSetId { get; set; }
}

public class GetTypeOfQuestionInQuestionSetQueryValidator : AbstractValidator<GetTypeOfQuestionInQuestionSetQuery>
{
    public GetTypeOfQuestionInQuestionSetQueryValidator()
    {
        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionSetId không được rỗng");
    }
}

public class GetTypeOfQuestionInQuestionSetQueryHandler : IRequestHandler<GetTypeOfQuestionInQuestionSetQuery,
    List<TypeOfQuestionInQuestionSetDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IQuestionSetService _questionSetService;
    private readonly IUser _user;
    private readonly IPlanService _planService;
    
    public GetTypeOfQuestionInQuestionSetQueryHandler(IApplicationDbContext context, IQuestionSetService questionSetService, IUser user,
        IPlanService planService)
    {
        _context = context;
        _questionSetService = questionSetService;
        _user = user;
        _planService = planService;
    }
    
    public async Task<List<TypeOfQuestionInQuestionSetDto>> Handle(GetTypeOfQuestionInQuestionSetQuery rq, CancellationToken cancellationToken)
    {
        var questionSet = await _context.QuestionSets
            .Include(q => q.Questions)
            .Where(qs => qs.Id.Equals(rq.QuestionSetId))
            .FirstOrDefaultAsync(cancellationToken);
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);
        
        var checkPlan = await _planService.CanLearn(_user.UserId!.Value);
        if (!checkPlan)
            throw new ErrorCodeException(ErrorCodes.PLAN_REQUIRE_PLAN);
        
        var canView = await _questionSetService.CanUserViewQuestionSet(_user.UserId, questionSet);
        if (!canView)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_ACCESS_TO_QUESTION_SET);
        
        var questionTypes = await _context.Questions
            .Where(x => x.QuestionSetId.Equals(rq.QuestionSetId) && !x.IsDeleted)
            .GroupBy(x => x.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        
        return questionTypes.Select(x => new TypeOfQuestionInQuestionSetDto { Type = x.Type.ToString(), Count = x.Count }).ToList();
    }
}

