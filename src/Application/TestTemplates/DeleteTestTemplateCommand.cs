using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.TestTemplates.Service;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.TestTemplates;

[Authorize]
public class DeleteTestTemplateCommand : IRequest<Guid>
{
    public required Guid TestTemplateId { get; set; }
}

public class DeleteTestTemplateCommandValidator : AbstractValidator<DeleteTestTemplateCommand>
{
    public DeleteTestTemplateCommandValidator()
    {
        RuleFor(x => x.TestTemplateId)
            .NotEmpty().WithMessage("TestId không được trống");
    }
}

public class DeleteTestTemplateCommandHandler : IRequestHandler<DeleteTestTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITestTemplateService _templateService;

    public DeleteTestTemplateCommandHandler(IApplicationDbContext context, ITestTemplateService templateService)
    {
        _context = context;
        _templateService = templateService;
    }
    
    public async Task<Guid> Handle(DeleteTestTemplateCommand rq, CancellationToken cancellationToken)
    {
        var testTemplate = await _context.TestTemplates
            .Where(t => t.Id == rq.TestTemplateId && t.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (testTemplate == null)
            throw new ErrorCodeException(ErrorCodes.TEST_TEMPLATE_NOT_FOUND, "Không tìm thấy test template");
            
        var canDelete = await _templateService.CanDeleteTestTemplate(rq.TestTemplateId, cancellationToken);
        if (!canDelete)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE,
                "Không có quyền xóa Test Template này");
        
        testTemplate.IsDeleted = true;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return testTemplate.Id;
    }
}
