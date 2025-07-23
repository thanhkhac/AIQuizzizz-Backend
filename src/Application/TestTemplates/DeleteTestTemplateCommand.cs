using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.TestTemplates.Service;

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
        var testTemplate = await _templateService.CanDeleteTestTemplate(rq.TestTemplateId, cancellationToken);
        
        testTemplate.IsDeleted = true;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return testTemplate.Id;
    }
}
