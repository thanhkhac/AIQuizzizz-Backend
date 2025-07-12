using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets;

public class UpdateQuestionSetHistoryCommand : IRequest<Unit>
{
    public Guid QuestionSetId { get; set; }
    public List<QuestionHistoryUpdate> Questions { get; set; } = new();
}

public class QuestionHistoryUpdate
{
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
}

public class UpdateQuestionSetHistoryCommandValidator : AbstractValidator<UpdateQuestionSetHistoryCommand>
{
    public UpdateQuestionSetHistoryCommandValidator()
    {
        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionSetId không được để trống");

        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("Danh sách câu hỏi không được để trống");
    }
}

public class UpdateQuestionSetHistoryCommandHandler : IRequestHandler<UpdateQuestionSetHistoryCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IQuestionSetService _questionSetService;

    public UpdateQuestionSetHistoryCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IQuestionSetService questionSetService)
    {
        _context = context;
        _user = user;
        _questionSetService = questionSetService;
    }

    public async Task<Unit> Handle(UpdateQuestionSetHistoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.UserId ?? throw new UnauthorizedAccessException();

        // Kiểm tra quyền truy cập question set
        if (!await _questionSetService.CanUserViewQuestionSet(userId, request.QuestionSetId))
            throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN);

        // Lấy các history hiện có của user cho các câu hỏi này
        var existingHistories = await _context.UserQuestionSetHistories
            .Where(h => h.UserId == userId &&
                        request.Questions.Select(q => q.QuestionId).Contains(h.QuestionId))
            .ToDictionaryAsync(h => h.QuestionId, cancellationToken);

        foreach (var question in request.Questions)
        {
            if (existingHistories.TryGetValue(question.QuestionId, out var history))
            {
                history.IsCorrect = question.IsCorrect;
            }
            else
            {
                // Tạo history mới
                var newHistory = new UserQuestionSetHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    QuestionId = question.QuestionId,
                    IsCorrect = question.IsCorrect,
                };
                _context.UserQuestionSetHistories.Add(newHistory);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
