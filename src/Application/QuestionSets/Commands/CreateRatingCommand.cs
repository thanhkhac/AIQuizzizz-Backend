using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets.Commands;

[Authorize]
public class CreateRatingCommand : IRequest<Guid>
{
    public Guid? QuestionSetId { get; set; }
    public int Rating { get; set; }
}

public class CreateRatingCommandValidator : AbstractValidator<CreateRatingCommand>
{
    public CreateRatingCommandValidator()
    {
        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionSetId không được trống");
        
        RuleFor(x => x.Rating)
            .GreaterThanOrEqualTo(0).WithMessage("Rating phải >= 0")
            .LessThanOrEqualTo(5).WithMessage("Rating phải =< 5");
    }
}

public class CreateRatingCommandHandler : IRequestHandler<CreateRatingCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IQuestionSetService _questionSetService;
    
    public CreateRatingCommandHandler(IApplicationDbContext context, IUser user, IQuestionSetService questionSetService)
    {
        _context = context;
        _user = user;
        _questionSetService = questionSetService;
    }
    
    
    public async Task<Guid> Handle(CreateRatingCommand rq, CancellationToken cancellationToken)
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
        if (rating != null)
        {
            rating.Rating = rq.Rating;
            await _context.SaveChangesAsync(cancellationToken);
            return rating.QuestionSetId;       
        }

        var newRating = new QuestionSetRating { QuestionSetId = rq.QuestionSetId!.Value, Rating = rq.Rating };
        
        var ratingsOfQuestionSet = _context.QuestionSetRatings
            .Where(x => x.QuestionSetId == rq.QuestionSetId);

        var averageRating = ratingsOfQuestionSet.Any() ? ratingsOfQuestionSet.Average(x => x.Rating) : 0;
        
        questionSet.RatingAverage = averageRating;
        
        await _context.QuestionSetRatings.AddAsync(newRating, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);

        return newRating.QuestionSetId;
    }
}
