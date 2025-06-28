using CleanArchitectureBase.Application.Classes.Common;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

public class AddQuestionSetCommand : IRequest<Guid>
{
    public required Guid ClassId { get; set; } 
    public required Guid QuestionSetId { get; set; } 
}

public class AddQuestionSetCommandValidator : AbstractValidator<AddQuestionSetCommand>
{
    public AddQuestionSetCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không thể trống");
        RuleFor(x => x.QuestionSetId)
            .NotEmpty().WithMessage("QuestionSetId không thể trống");
    }
}

public class AddQuestionSetCommandHandler : IRequestHandler<AddQuestionSetCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ClassValidationService _classValidationService;
    
    public AddQuestionSetCommandHandler(IApplicationDbContext context, ClassValidationService classValidationService)
    {
        _context = context;
        _classValidationService = classValidationService;
    } 
    
    public async Task<Guid> Handle(AddQuestionSetCommand rq, CancellationToken cancellationToken)
    {
        var (isOwner, classExists) = await _classValidationService.ValidateClassAccessAsync(rq.ClassId, cancellationToken);

        var questionSet = await _context.QuestionSets.Where(x => x.Id == rq.QuestionSetId)
            .FirstOrDefaultAsync(cancellationToken);
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND_IN_CLASS, "Bộ câu hỏi không tồn tại hoặc không thuộc lớp học này.");

        var classQuestion = await _context.ClassQuestionSets
            .Where(x => x.ClassId == classExists.Id && x.QuestionSetId == rq.QuestionSetId)
            .FirstOrDefaultAsync(cancellationToken);
        if (classQuestion != null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_ALREADY_IN_CLASS, "Bộ câu hỏi đã tồn tại trong lớp học này.");
        
        var classQuestionSet = new ClassQuestionSet
        {
            ClassId = classExists.Id,
            QuestionSetId = questionSet.Id,
            Class = classExists,
            QuestionSet = questionSet
        };
        
        _context.ClassQuestionSets.Add(classQuestionSet);
        await _context.SaveChangesAsync(cancellationToken);

        return rq.QuestionSetId;
    }
}
