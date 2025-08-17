using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.TestTemplates;

[Authorize]
public class SearchTestTemplateQuery : IRequest<PaginatedList<TestTemplateDto>>
{
    public string? TestTemplateName { get; set; }
    public string? ShareMode { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchTestTemplateQueryValidator : AbstractValidator<SearchTestTemplateQuery>
{
    public SearchTestTemplateQueryValidator()
    {
        RuleFor(x => x.ShareMode)
            .Must(mode => new[]
            {
                "Owner", "Editable", "ViewOnly"
            }.Contains(mode) || string.IsNullOrEmpty(mode))
            .WithMessage($"SharedMode phải là Owner, Editable, ViewOnly");
    }
}

public class SearchTestTemplateQueryHandler : IRequestHandler<SearchTestTemplateQuery, PaginatedList<TestTemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public SearchTestTemplateQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    /// <summary>
    /// The function searches for test templates based on name and share mode for the current user, returning a paginated list of test template details
    /// </summary>
    /// <param name="rq">Request contains TestTemplateName, SharedMode, PageNumber, and PageSize information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<PaginatedList<TestTemplateDto>> Handle(SearchTestTemplateQuery rq, CancellationToken cancellationToken)
    {
        TestTemplateUserShareMode? sharedMode = null;
        if (!string.IsNullOrEmpty(rq.ShareMode) &&
            Enum.TryParse<TestTemplateUserShareMode>(rq.ShareMode, true, out var parsedSharedMode))
        {
            sharedMode = parsedSharedMode;
        }

        var testTemplates = _context.TestTemplateUsers
            .Include(t => t.TestTemplate)
            .Where(t => t.UserId == _user.UserId
                        && (string.IsNullOrEmpty(rq.TestTemplateName) ||
                            t.TestTemplate!.Name.ToLower().Contains(rq.TestTemplateName.ToLower()))
                        && (sharedMode == null || t.ShareMode == sharedMode)
                        && t.TestTemplate!.IsDeleted == false)
            .Select(t => new TestTemplateDto
            {
                TestTemplateId = t.TestTemplate!.Id,
                Name = t.TestTemplate.Name,
                NumberOfQuestion = t.TestTemplate.TestTemplateQuestions.Count(),
                DateCreated = t.TestTemplate.Created.UtcDateTime,
                CreateBy = t.TestTemplate.CreatedByUser!.FullName,
            });

        return await PaginatedList<TestTemplateDto>.CreateAsync(
            testTemplates,
            rq.PageNumber,
            rq.PageSize
        );
    }
}
