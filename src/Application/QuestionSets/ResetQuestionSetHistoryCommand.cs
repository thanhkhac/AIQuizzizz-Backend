using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Application.UserQuestionSetHistories.Services;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets;

[Authorize]
public class ResetQuestionSetHistoryCommand : IRequest<Unit>
{
    public Guid QuestionSetId { get; set; }
}

public class ResetQuestionSetHistoryCommandValidator : AbstractValidator<ResetQuestionSetHistoryCommand>
{
    public ResetQuestionSetHistoryCommandValidator()
    {
        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionSetId không được để trống");
    }
}

public class ResetQuestionSetHistoryCommandHandler : IRequestHandler<ResetQuestionSetHistoryCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IQuestionSetService _questionSetService;
    private readonly IUserQuestionSetHistoryService _userQuestionSetHistoryService;

    public ResetQuestionSetHistoryCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IQuestionSetService questionSetService, IUserQuestionSetHistoryService userQuestionSetHistoryService)
    {
        _context = context;
        _user = user;
        _questionSetService = questionSetService;
        _userQuestionSetHistoryService = userQuestionSetHistoryService;
    }

    public async Task<Unit> Handle(ResetQuestionSetHistoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.UserId!.Value;
        
        var questionSet = await _questionSetService.GetActiveQuestionSet(request.QuestionSetId, cancellationToken);
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);

        var canView = await _questionSetService.CanUserViewQuestionSet(userId, questionSet);
        if (canView == false) throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN, "You are not allowed to view this question set");

        await _userQuestionSetHistoryService.DeleteHistoriesByUserAndSetAsync(userId, request.QuestionSetId, cancellationToken);

        return Unit.Value;
    }
}
