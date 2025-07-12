using CleanArchitectureBase.Application.Classes.Lecturer;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.FolderTest;

public class TestTemplateDto
{
    public Guid TestTemplateId { get; set; }
    public string? Name { get; set; }
    public int NumberOfQuestion { get; set; }
    public DateTime? DateCreated { get; set; }
    public string? CreatedBy { get; set; }
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

        var testTemplates = _context.FolderTestTemplates
            .Include(ft => ft.TestTemplate)
            .ThenInclude(t => t!.TestTemplateQuestions)
            .Where(ft => ft.FolderId == rq.FolderId)
            .Select(ft => new TestTemplateDto
            {
                TestTemplateId = ft.TestTemplate!.Id,
                Name = ft.TestTemplate.Name,
                NumberOfQuestion = ft.TestTemplate.TestTemplateQuestions.Count(),
                DateCreated = ft.TestTemplate.Created.UtcDateTime
            });

        return await PaginatedList<TestTemplateDto>.CreateAsync(
            testTemplates,
            rq.PageNumber,
            rq.PageSize
        );
    }
}
