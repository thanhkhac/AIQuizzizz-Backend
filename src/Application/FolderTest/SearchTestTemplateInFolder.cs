using CleanArchitectureBase.Application.Classes.Lecturer;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.FolderTest;

public class TestTemplateDto
{
    public string? Name { get; set; }
    public int NumberOfQuestion { get; set; }
    public DateTime? DateCreated { get; set; }
}

[Authorize]
public class SearchTestTemplateInFolder : IRequest<PaginatedList<TestTemplateDto>>
{
    public required Guid FolderId { get; set; }
    public required string? TestTemplateName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchTestTemplateInFolderValidator : AbstractValidator<SearchTestTemplateInFolder>
{
    public SearchTestTemplateInFolderValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được null");
    }
}

public class SearchTestTemplateInFolderHandler : IRequestHandler<SearchTestTemplateInFolder, PaginatedList<TestTemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public SearchTestTemplateInFolderHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<PaginatedList<TestTemplateDto>> Handle(SearchTestTemplateInFolder rq, CancellationToken cancellationToken)
    {
        var accessUser = await _context.FolderUsers
            .Where(x => x.FolderId == rq.FolderId && x.UserId == _user.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        if (accessUser == null)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER, "User không có quyền trong folder");

        var testTemplates = _context.TestTemplates
            .Join(_context.FolderTestTemplates,
                t => t.Id,
                ft => ft.TestTemplateId,
                (t, ft) => new { TestTemplate = t, FolderTestTemplate = ft })
            .Where(ft => ft.FolderTestTemplate.FolderId == rq.FolderId)
            .GroupJoin(_context.TestTemplateQuestions,
                ft => ft.TestTemplate.Id,
                tq => tq.TestTemplateId,
                (ft, tq) => new { ft.TestTemplate, ft.FolderTestTemplate, tq })
            .Select(x => new TestTemplateDto
            {
                Name = x.TestTemplate.Name,
                NumberOfQuestion = x.tq.Count(),
                DateCreated = x.TestTemplate.Created.UtcDateTime
            });

        return await PaginatedList<TestTemplateDto>.CreateAsync(
            testTemplates,
            rq.PageNumber,
            rq.PageSize
        );
    }
}
