using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Classes;

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
    private readonly IClassService _classService;
    
    public RemoveQuestionSetHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    public async Task<Guid> Handle(RemoveQuestionSetCommand rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId)
            .FirstOrDefaultAsync(cancellationToken);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy lớp");
        
        await _classService.IsLecturerOrOwnerInClass(rq.ClassId);

        var classQuestionSet = await _context.ClassQuestionSets
            .Where(x => x.QuestionSetId == rq.QuestionSetId && x.ClassId == classById.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (classQuestionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND_IN_CLASS, "Question set không tồn tại hoặc không trong class");
        
        _context.ClassQuestionSets.Remove(classQuestionSet);
        await _context.SaveChangesAsync(cancellationToken);
        
        return rq.QuestionSetId;
    }
}
