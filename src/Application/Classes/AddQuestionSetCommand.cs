using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes;

[Authorize]
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
    private readonly IClassService _classService;
    private readonly IUser _user;
    
    public AddQuestionSetCommandHandler(
        IApplicationDbContext context,
        IClassService classService,
        IUser user)
    {
        _context = context;
        _classService = classService;
        _user = user;
    } 
    
    public async Task<Guid> Handle(AddQuestionSetCommand rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId)
            .FirstOrDefaultAsync(cancellationToken);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy lớp");
        
        await _classService.IsLecturerOrOwnerInClass(rq.ClassId);

        var questionSet = await _context.QuestionSets.Where(x => x.Id == rq.QuestionSetId)
            .FirstOrDefaultAsync(cancellationToken);    
        if (questionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_NOT_FOUND, "Bộ câu hỏi không tồn tại");

        if (QuestionSetVisibilityMode.Public != questionSet.VisibilityMode &&
            !_user.UserId!.Value.Equals(questionSet.CreatedBy))
        {
            throw new ErrorCodeException(ErrorCodes.NOT_HAVE_PERMISSION_TO_ADD_QUESTION_SET,
                "Không có quyền add question set");
        }

        var classQuestion = await _context.ClassQuestionSets
            .Where(x => x.ClassId == classById.Id && x.QuestionSetId == rq.QuestionSetId)
            .FirstOrDefaultAsync(cancellationToken);
        if (classQuestion != null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_SET_ALREADY_IN_CLASS, "Bộ câu hỏi đã tồn tại trong lớp học này.");
        
        var classQuestionSet = new ClassQuestionSet
        {
            ClassId = classById.Id,
            QuestionSetId = questionSet.Id,
            Class = classById,
            QuestionSet = questionSet
        };
        
        _context.ClassQuestionSets.Add(classQuestionSet);
        await _context.SaveChangesAsync(cancellationToken);

        return rq.QuestionSetId;
    }
}
