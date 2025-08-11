using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

public class GetClassesHaveQuestionSetDto
{
    public Guid ClassId { get; set; }
    public string? ClassName { get; set; }
    public bool IsAdded { get; set; }
}

public class GetClassesHaveQuestionSetQuery : IRequest<List<GetClassesHaveQuestionSetDto>>
{
    public Guid QuestionSetId { get; set; }
}

public class GetClassesHaveQuestionSetQueryValidator : AbstractValidator<GetClassesHaveQuestionSetQuery>
{
    public GetClassesHaveQuestionSetQueryValidator()
    {
        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionSetId không được trống");
    }
}

public class
    GetClassesHaveQuestionSetQueryHandler : IRequestHandler<GetClassesHaveQuestionSetQuery,
    List<GetClassesHaveQuestionSetDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public GetClassesHaveQuestionSetQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<List<GetClassesHaveQuestionSetDto>> Handle(GetClassesHaveQuestionSetQuery request, CancellationToken cancellationToken)
    {
        var questionSet = await _context.QuestionSets
            .Where(x => x.Id == request.QuestionSetId)
            .FirstOrDefaultAsync(cancellationToken);
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);
        
        var userClass = await _context.ClassUsers
            .Include(x => x.Class)
            .ThenInclude(x => x.ClassQuestionSets)
            .Where(x => x.UserId == _user.UserId
            && (ClassShareMode.Owner == x.ShareMode || ClassShareMode.Teacher == x.ShareMode))
            .Select(x => new GetClassesHaveQuestionSetDto
            {
                ClassId = x.ClassId,
                ClassName = x.Class.Name,
                IsAdded = x.Class.ClassQuestionSets.Any(y =>
                    y.QuestionSetId == request.QuestionSetId)
            })
            .ToListAsync(cancellationToken);

        return userClass;
    }
}
