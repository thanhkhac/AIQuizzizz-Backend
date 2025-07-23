using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Tests.Service;

namespace CleanArchitectureBase.Application.Tests;

[Authorize]
public class DeleteTestCommand : IRequest<Guid>
{
    public required Guid TestId { get; init; }
}

public class DeleteTestCommandValidator : AbstractValidator<DeleteTestCommand>
{
    public DeleteTestCommandValidator()
    {
        RuleFor(x => x.TestId)
            .NotEmpty().WithMessage("TestId không được trống");
    }
}

public class DeleteTestCommandHandler : IRequestHandler<DeleteTestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITestService _testService;

    public DeleteTestCommandHandler(IApplicationDbContext context, ITestService testService)
    {
        _context = context;
        _testService = testService;
    }
    
    public async Task<Guid> Handle(DeleteTestCommand rq, CancellationToken cancellationToken)
    {
        var test = await _testService.CanEditTest(rq.TestId, cancellationToken);
        
        test.IsDeleted = true;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return test.Id;
    }
}
