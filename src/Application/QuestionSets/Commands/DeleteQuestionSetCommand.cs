using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets.Commands;

[Authorize]
public class DeleteQuestionSetCommand : IRequest<Unit>
{
    public Guid QuestionSetId { get; set; }
}

public class DeleteQuestionSetCommandValidator : AbstractValidator<DeleteQuestionSetCommand>
{
    public DeleteQuestionSetCommandValidator()
    {
        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionSetId không được để trống");
    }
}

public class DeleteQuestionSetCommandHandler : IRequestHandler<DeleteQuestionSetCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IQuestionSetService _questionSetService;

    public DeleteQuestionSetCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IQuestionSetService questionSetService)
    {
        _context = context;
        _user = user;
        _questionSetService = questionSetService;
    }

    public async Task<Unit> Handle(DeleteQuestionSetCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.UserId!.Value;

        // Kiểm tra question set có tồn tại và active không
        var questionSet = await _questionSetService.GetActiveQuestionSet(request.QuestionSetId, cancellationToken);
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND);

        // Kiểm tra quyền xóa
        var canDelete = await _questionSetService.CanUserDeleteQuestionSet(userId, request.QuestionSetId);
        if (!canDelete)
            throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN);

        // Soft delete
        questionSet.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
