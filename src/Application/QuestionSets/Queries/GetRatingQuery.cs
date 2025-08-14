using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets.Queries;

public class GetRatingDto
{
    public int Rating { get; set; }
}

[Authorize]
public class GetRatingQuery : IRequest<GetRatingDto>
{
    public Guid? QuestionSetId { get; set; }
}

public class GetRatingQueryValidator : AbstractValidator<GetRatingQuery>
{
    public GetRatingQueryValidator()
    {
        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionSetId không được trống");
    }
}

public class GetRatingQueryHandler : IRequestHandler<GetRatingQuery, GetRatingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IQuestionSetService _questionSetService;
    
    public GetRatingQueryHandler(IApplicationDbContext context, IUser user, IQuestionSetService questionSetService)
    {
        _context = context;
        _user = user;
        _questionSetService = questionSetService;
    }
    
    public async Task<GetRatingDto> Handle(GetRatingQuery rq, CancellationToken cancellationToken)
    {
        var questionSet = await _context.QuestionSets
            .Where(x => x.Id == rq.QuestionSetId)
            .FirstOrDefaultAsync(cancellationToken);
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);
        
        var canView = await _questionSetService.CanUserViewQuestionSet(_user.UserId, questionSet);
        if (!canView)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_ACCESS_TO_QUESTION_SET);

        var rating = await _context.QuestionSetRatings
            .Where(x => x.QuestionSetId == rq.QuestionSetId && x.CreatedBy.Equals(_user.UserId))
            .FirstOrDefaultAsync(cancellationToken);
        
        return new GetRatingDto { Rating = rating?.Rating ?? 0 };
    }
}
