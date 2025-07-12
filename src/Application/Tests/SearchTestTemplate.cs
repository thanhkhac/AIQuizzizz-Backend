using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

public class SearchTestTemplate : IRequest<PaginatedList<TestTemplateDto>>
{
    public required string? TestTemplateName { get; set; }
    public string? SharedMode { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchTestTemplateValidator : AbstractValidator<SearchTestTemplate>
{
    public SearchTestTemplateValidator()
    {
        RuleFor(x => x.SharedMode)
            .Must(mode => new[] {"Owner", "Editable", "ViewOnly"}.Contains(mode) || string.IsNullOrEmpty(mode))
            .WithMessage($"SharedMode phải là Owner, Editable, ViewOnly");
    }
}

public class SearchTestTemplateHandler : IRequestHandler<SearchTestTemplate, PaginatedList<TestTemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public SearchTestTemplateHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<PaginatedList<TestTemplateDto>> Handle(SearchTestTemplate rq, CancellationToken cancellationToken)
    {
        TestTemplateUserShareMode? sharedMode = null;
        if (!string.IsNullOrEmpty(rq.SharedMode) &&
            Enum.TryParse<TestTemplateUserShareMode>(rq.SharedMode, true, out var parsedSharedMode))
        {
            sharedMode = parsedSharedMode;
        }

        var testTemplates = _context.TestTemplateUsers
            .Include(t => t.TestTemplate)
            .Where(t => t.UserId == _user.UserId
                        && (string.IsNullOrEmpty(rq.TestTemplateName) ||
                            t.TestTemplate!.Name.Contains(rq.TestTemplateName))
                        && (sharedMode == null || t.ShareMode == sharedMode))
            .Select(t => new TestTemplateDto
            {
                TestTemplateId = t.TestTemplate!.Id,
                Name = t.TestTemplate.Name,
                NumberOfQuestion = t.TestTemplate.TestTemplateQuestions.Count(),
                DateCreated = t.TestTemplate.Created.UtcDateTime,
                CreatedBy = t.TestTemplate.CreatedByUser!.FullName,
            });
        
        return await PaginatedList<TestTemplateDto>.CreateAsync(
            testTemplates,
            rq.PageNumber,
            rq.PageSize
        );
    }
}
