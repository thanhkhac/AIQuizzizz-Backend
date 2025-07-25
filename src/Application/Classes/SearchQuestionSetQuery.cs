using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes;

public class SearchQuestionSetDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int NumberOfQuestions { get; set; }
    public string? CreateBy { get; set; }
}

[Authorize]
public class SearchQuestionSetQuery : IRequest<PaginatedList<SearchQuestionSetDto>>
{
    /// <summary>
    /// Id of the class want to retrieve question sets
    /// </summary>
    public required Guid ClassId { get; set; }
    public string? Name { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchQuestionSetQueryValidator : AbstractValidator<SearchQuestionSetQuery>
{
    public SearchQuestionSetQueryValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được null");
    }
}

public class SearchQuestionSetQueryHandler : IRequestHandler<SearchQuestionSetQuery, PaginatedList<SearchQuestionSetDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    
    public SearchQuestionSetQueryHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }

    /// <summary>
    /// The function searches for question sets within a class based on name and share mode, returning a paginated list of question set details
    /// </summary>
    /// <param name="rq">Request contains ClassId, Name, ShareMode, PageNumber, and PageSize information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<PaginatedList<SearchQuestionSetDto>> Handle(SearchQuestionSetQuery rq,
        CancellationToken cancellationToken)
    {
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId)
            .FirstOrDefaultAsync(cancellationToken);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy lớp");

        var isUserInClass = await _classService.IsUserInClass(rq.ClassId);
        if (!isUserInClass)
            throw new ErrorCodeException(ErrorCodes.NOT_FOUND_USER_IN_CLASS, "User không có trong lớp");

        var questionSet = _context.QuestionSets
            .Include(x => x.CreatedByUser)
            .Include(x => x.Questions)
            .Include(x => x.ClassQuestionSets)
            .Include(qs => qs.QuestionSetUsers)
            .Where(qs => qs.ClassQuestionSets.Any(x => x.ClassId == rq.ClassId) &&
                         (string.IsNullOrEmpty(rq.Name) || qs.Name.ToLower().Contains(rq.Name.ToLower())));

        return await PaginatedList<SearchQuestionSetDto>.CreateAsync(
            questionSet.Select(qs => new SearchQuestionSetDto
            {
                Id = qs.Id,
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
