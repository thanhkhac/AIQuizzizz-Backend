using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Tests.Service;
using CleanArchitectureBase.Domain.Constants;

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
        var test = await _context.Tests
            .Where(x => x.Id == rq.TestId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (test == null)
            throw new ErrorCodeException(ErrorCodes.TEST_NOT_FOUND, "Không tìm thấy test");
            
        var canDelete = await _testService.CanViewOrEditTest(test.ClassId, cancellationToken);
        if (!canDelete)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST, "Không có quyền xóa");
        
        test.IsDeleted = true;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return test.Id;
    }
}
