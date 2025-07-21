using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

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
public class SearchTestTemplateInFolderCommand : IRequest<PaginatedList<TestTemplateDto>>
{
    /// <summary>
    /// Id of the folder want to retrieve test templates
    /// </summary>
    public required Guid FolderId { get; set; }
    public required string? TestTemplateName { get; set; }
    public required string? SharedMode { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchTestTemplateInFolderCommandValidator : AbstractValidator<SearchTestTemplateInFolderCommand>
{
    public SearchTestTemplateInFolderCommandValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được null");
        
        RuleFor(x => x.SharedMode)
            .Must(mode => new[] {"Owner", "Editable", "ViewOnly"}.Contains(mode) || string.IsNullOrEmpty(mode))
            .WithMessage($"SharedMode phải là Owner, Editable, ViewOnly");
    }
}

public class SearchTestTemplateInFolderCommandHandler : IRequestHandler<SearchTestTemplateInFolderCommand, PaginatedList<TestTemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public SearchTestTemplateInFolderCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    /// <summary>
    /// The function searches for test templates in a folder based on name and share mode, returning a paginated list of test template details
    /// </summary>
    /// <param name="rq">Request contains FolderId, TestTemplateName, SharedMode, PageNumber, and PageSize information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<PaginatedList<TestTemplateDto>> Handle(SearchTestTemplateInFolderCommand rq, CancellationToken cancellationToken)
    {
        var accessUser = await _context.FolderUsers
            .Where(x => x.FolderId == rq.FolderId && x.UserId == _user.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        if (accessUser == null)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER, "User không có quyền trong folder");

        TestTemplateUserShareMode? sharedMode = null;
        if (!string.IsNullOrEmpty(rq.SharedMode) &&
            Enum.TryParse<TestTemplateUserShareMode>(rq.SharedMode, true, out var parsedSharedMode))
        {
            sharedMode = parsedSharedMode;
        }
        
        var testTemplates = _context.FolderTestTemplates
            .Include(ft => ft.TestTemplate)
            .ThenInclude(t => t!.TestTemplateQuestions)
            .Include(t => t.TestTemplate!.CreatedByUser)
            .Where(ft => ft.FolderId == rq.FolderId
            && (string.IsNullOrEmpty(rq.TestTemplateName) || ft.TestTemplate!.Name.ToLower().Contains(rq.TestTemplateName.ToLower()))
            && (sharedMode == null || ft.TestTemplate!.TestTemplateUsers.Any(t => t.ShareMode == sharedMode)))
            .Select(ft => new TestTemplateDto
            {
                TestTemplateId = ft.TestTemplate!.Id,
                Name = ft.TestTemplate.Name,
                NumberOfQuestion = ft.TestTemplate.TestTemplateQuestions.Count(),
                DateCreated = ft.TestTemplate.Created.UtcDateTime,
                CreatedBy = ft.TestTemplate.CreatedByUser!.FullName,
            });

        return await PaginatedList<TestTemplateDto>.CreateAsync(
            testTemplates,
            rq.PageNumber,
            rq.PageSize
        );
    }
}
