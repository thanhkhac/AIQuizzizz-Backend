using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.FolderTest;

public class TestTemplateDto
{
    public Guid TestTemplateId { get; set; }
    public string? FolderName { get; set; }
    public string? Name { get; set; }
    public int NumberOfQuestion { get; set; }
    public DateTime? DateCreated { get; set; }
    public string? CreateBy { get; set; }
}

public class SearchTestTemplateInFolderDto
{
    public string? FolderName { get; set; }
    public PaginatedList<TestTemplateDto> TestTemplates { get; set; } = null!;
}

[Authorize]
public class SearchTestTemplateInFolderQuery : IRequest<SearchTestTemplateInFolderDto>
{
    /// <summary>
    /// Id of the folder want to retrieve test templates
    /// </summary>
    public Guid? FolderId { get; set; }
    public string? TestTemplateName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchTestTemplateInFolderQueryValidator : AbstractValidator<SearchTestTemplateInFolderQuery>
{
    public SearchTestTemplateInFolderQueryValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được null");
    }
}

public class SearchTestTemplateInFolderQueryHandler : IRequestHandler<SearchTestTemplateInFolderQuery, SearchTestTemplateInFolderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public SearchTestTemplateInFolderQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    /// <summary>
    /// The function searches for test templates in a folder based on name and share mode, returning a paginated list of test template details
    /// </summary>
    /// <param name="rq">Request contains FolderId, TestTemplateName, SharedMode, PageNumber, and PageSize information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<SearchTestTemplateInFolderDto> Handle(SearchTestTemplateInFolderQuery rq, CancellationToken cancellationToken)
    {
        var accessUser = await _context.FolderUsers
            .Include(x => x.Folder)
            .Where(x => x.FolderId == rq.FolderId && x.UserId == _user.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        if (accessUser == null)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER, "User không có quyền trong folder");

        accessUser.IsViewed = true;
        
        var testTemplates = _context.FolderTestTemplates
            .Include(ft => ft.TestTemplate)
            .ThenInclude(t => t!.TestTemplateQuestions)
            .Include(t => t.TestTemplate!.CreatedByUser)
            .Where(ft => ft.FolderId == rq.FolderId && ft.Folder != null && ft.Folder.IsDeleted == false && ft.TestTemplate!.IsDeleted == false
            && (string.IsNullOrEmpty(rq.TestTemplateName) || ft.TestTemplate!.Name.ToLower().Contains(rq.TestTemplateName.ToLower())))
            .Select(ft => new TestTemplateDto
            {
                TestTemplateId = ft.TestTemplate!.Id,
                Name = ft.TestTemplate.Name,
                FolderName = ft.Folder!.Name,
                NumberOfQuestion = ft.TestTemplate.TestTemplateQuestions.Count(),
                DateCreated = ft.TestTemplate.Created.UtcDateTime,
                CreateBy = ft.TestTemplate.CreatedByUser!.FullName,
            });

        var pageTestTemplates = await PaginatedList<TestTemplateDto>.CreateAsync(
            testTemplates,
            rq.PageNumber,
            rq.PageSize
        );
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return new SearchTestTemplateInFolderDto
        {
            FolderName = accessUser.Folder!.Name,
            TestTemplates = pageTestTemplates
        };
    }
}
