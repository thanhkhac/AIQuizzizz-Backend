using CleanArchitectureBase.Application.Classes.Common;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

[Authorize]
public class RemoveQuestionSetCommand : IRequest<Guid>
{
    public required Guid ClassId { get; set; } 
    public required Guid QuestionSetId { get; set; }  
}

public class RemoveQuestionSetValidator : AbstractValidator<RemoveQuestionSetCommand>
{
    public RemoveQuestionSetValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không thể trống");
        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionSetId không thể trống");
    }
}

public class RemoveQuestionSetHandler : IRequestHandler<RemoveQuestionSetCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ClassValidationService _classValidationService;
    
    public RemoveQuestionSetHandler(IApplicationDbContext context, ClassValidationService classValidationService)
    {
        _context = context;
        _classValidationService = classValidationService;
    }
    
    public async Task<Guid> Handle(RemoveQuestionSetCommand rq, CancellationToken cancellationToken)
    {
        var (isOwner, classExists) = await _classValidationService.ValidateClassAccessAsync(rq.ClassId, cancellationToken);

        var classQuestionSet = await _context.ClassQuestionSets
            .Where(x => x.QuestionSetId == rq.QuestionSetId && x.ClassId == classExists.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (classQuestionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND_IN_CLASS, "Question set không tồn tại hoặc không trong class");
        
        _context.ClassQuestionSets.Remove(classQuestionSet);
        await _context.SaveChangesAsync(cancellationToken);
        
        return rq.QuestionSetId;
    }
}
