using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Tests;

public class AttemptTestDetail : IRequest<TestDetailDto>
{
    public required Guid AttemptId { get; set; }
}

public class GetTestDetailValidator : AbstractValidator<AttemptTestDetail>
{
    public GetTestDetailValidator()
    {
        RuleFor(x => x.AttemptId)
            .NotEmpty().WithMessage("AttemptId không đươợc rỗng");
    }
}

public class GetTestDetailHandler : IRequestHandler<AttemptTestDetail, TestDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IClassService _classService;
    
    public GetTestDetailHandler(IApplicationDbContext context, IUser user, IClassService classService)
    {
        _context = context;
        _user = user;
        _classService = classService;
    }

    public async Task<TestDetailDto> Handle(AttemptTestDetail request, CancellationToken cancellationToken)
    {
        var attempt = await _context.Attempts.Where(x => x.Id == request.AttemptId)
            .Include(x => x.Test)
            .Include(x => x.TestVersion)
            .FirstOrDefaultAsync(cancellationToken);
        if (attempt == null || attempt.TestVersion == null || attempt.Test == null) 
            throw new ErrorCodeException(ErrorCodes.TEST_NOT_FOUND, "Bài test không tồn tại");
        
        
        
        throw new NotImplementedException();
    }
}
