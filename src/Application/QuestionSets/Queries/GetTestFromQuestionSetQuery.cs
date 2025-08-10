using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

[Authorize]
public class GetTestFromQuestionSetQuery : IRequest<List<QuestionResponseDto>>
{
    public Guid? QuestionSetId { get; set; }
    public int NumberOfQuestion { get; set; }
    public List<string> QuestionTypes { get; set; } = new();
}

public class GetTestFromQuestionSetQueryValidator : AbstractValidator<GetTestFromQuestionSetQuery>
{
    public GetTestFromQuestionSetQueryValidator()
    {
        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionId không được trống");
        RuleFor(x => x.NumberOfQuestion)
            .GreaterThan(0).WithMessage("NumberOfQuestion phải > 0")
            .LessThan(50).WithMessage("NumberOfQuestion giới hạn là 50");
        RuleFor(x => x.QuestionTypes)
            .NotEmpty().WithMessage("QuestionTypes không được bỏ trống")
            .Must(x => x.Count > 0).WithMessage("Số lượng type lớn hơn 0")
            .ForEach(type =>
                type.Must(x => new[] {"MultipleChoice", "Matching", "Ordering", "ShortText"}.Contains(x))
                    .WithMessage($"Loại câu hỏi phải là MultipleChoice, Matching, Ordering, ShortText"));
    }
}

public class GetTestFromQuestionSetQueryHandler : IRequestHandler<GetTestFromQuestionSetQuery, List<QuestionResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IQuestionSetService _questionSetService;
    private readonly IUser _user;
    
    public GetTestFromQuestionSetQueryHandler(
        IApplicationDbContext context,
        IQuestionSetService questionSetService,
        IUser user)
    {
        _context = context;
        _questionSetService = questionSetService;
        _user = user;
    }
    
    public async Task<List<QuestionResponseDto>> Handle(GetTestFromQuestionSetQuery rq, CancellationToken cancellationToken)
    {
        var questionSet = await _context.QuestionSets
            .Include(q => q.Questions)
            .Where(qs => qs.Id.Equals(rq.QuestionSetId))
            .FirstOrDefaultAsync(cancellationToken);
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);
        
        var canView = await _questionSetService.CanUserViewQuestionSet(_user.UserId, questionSet);
        if (!canView)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_ACCESS_TO_QUESTION_SET);

        if (rq.NumberOfQuestion > questionSet.Questions.Count)
            throw new ErrorCodeException(ErrorCodes.INVALID_NUMBER_OF_QUESTIONS);
        
        var typeToQuestions = new Dictionary<string, List<Question>>
        {
            ["MultipleChoice"] = questionSet.Questions.Where(q => q.Type == QuestionType.MultipleChoice).ToList(),
            ["Matching"]       = questionSet.Questions.Where(q => q.Type == QuestionType.Matching).ToList(),
            ["Ordering"]       = questionSet.Questions.Where(q => q.Type == QuestionType.Ordering).ToList(),
            ["ShortText"]      = questionSet.Questions.Where(q => q.Type == QuestionType.ShortText).ToList()
        };        
        
        List<Question> questions = new();

        while (questions.Count < rq.NumberOfQuestion)
        {
            var random = new Random();
            
            string randomType = rq.QuestionTypes[random.Next(rq.QuestionTypes.Count)];

            var questionList = typeToQuestions[randomType];
            
            if (questionList.Count == 0)
            {
                rq.QuestionTypes.Remove(randomType);
                continue;
            }
            
            var question = questionList[random.Next(questionList.Count)];
            
            questions.Add(question);
            
            questionList.Remove(question);
        }
        
        return questions.Select(x =>
            QuestionResponseDto.Mapper.FromEntity(x, true)).ToList();
    }
}
