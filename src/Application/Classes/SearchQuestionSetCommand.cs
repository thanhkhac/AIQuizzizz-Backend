using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes;

public class SearchQuestionSetDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int NumberOfQuestions { get; set; }
    public string? CreateBy { get; set; }
}

public class SearchQuestionSetCommand : IRequest<PaginatedList<SearchQuestionSetDto>>
{
    public required Guid ClassId { get; set; }
    public string? Name { get; set; }
    public string? ShareMode { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchQuestionSetValidator : AbstractValidator<SearchQuestionSetCommand>
{
    public SearchQuestionSetValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được null");
        
        RuleFor(x => x.ShareMode)
            .Must(mode => new[] {"Owner", "Editable", "ViewOnly"}.Contains(mode) || string.IsNullOrEmpty(mode))
            .WithMessage($"SharedMode phải là Owner, Editable, ViewOnly");
    }
}

public class SearchQuestionSetCommandHandler : IRequestHandler<SearchQuestionSetCommand, PaginatedList<SearchQuestionSetDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    
    public SearchQuestionSetCommandHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }

    public async Task<PaginatedList<SearchQuestionSetDto>> Handle(SearchQuestionSetCommand rq,
        CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId)
            .FirstOrDefaultAsync(cancellationToken);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy lớp");

        await _classService.IsUserInClass(rq.ClassId);

        var questionSet = _context.QuestionSets
            .Include(x => x.CreatedByUser)
            .Include(x => x.Questions)
            .Include(x => x.ClassQuestionSets)
            .Include(qs => qs.QuestionSetUsers)
            .Where(qs => qs.ClassQuestionSets.Any(x => x.ClassId == rq.ClassId) &&
                         (string.IsNullOrEmpty(rq.Name) || qs.Name.ToLower().Contains(rq.Name.ToLower())));

        if (!string.IsNullOrEmpty(rq.ShareMode) && Enum.TryParse<QuestionSetUserShareMode>(rq.ShareMode, out var shareMode))
        {
            questionSet = questionSet.Where(qs => qs.QuestionSetUsers.Any(qsu => qsu.ShareMode == shareMode));
        }

        return await PaginatedList<SearchQuestionSetDto>.CreateAsync(
            questionSet.Select(qs => new SearchQuestionSetDto
            {
                Name = qs.Name,
                Description = qs.Description,
                NumberOfQuestions = qs.Questions.Count,
                CreateBy = qs.CreatedByUser != null ? qs.CreatedByUser.FullName : string.Empty
            }),
            rq.PageNumber,
            rq.PageSize
        );
    }
}
