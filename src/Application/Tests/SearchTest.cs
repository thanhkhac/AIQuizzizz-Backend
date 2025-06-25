using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

public class TestSearchResultDto
{
    public required string Name { get; set; }
    public int? NumberOfQuestions { get; set; }
    public required double TimeLimit { get; set; }
    public DateTime? DateCreated { get; set; }
    public int? NumberOfCompletion { get; set; }
    public TestStatus Status { get; set; }
}   

public class SearchTest : IRequest<List<TestSearchResultDto>>
{
    public required Guid ClassId { get; set; }
    public string? TestName { get; set; }
    public TestStatus? Status { get; set; }
}

public class SearchTestValidator : AbstractValidator<SearchTest>
{
    public SearchTestValidator()
    {
        RuleFor(v => v.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");
    }
}

public class SearchTestHandler : IRequestHandler<SearchTest, List<TestSearchResultDto>>
{
    private readonly IApplicationDbContext _context;
    
    public SearchTestHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<TestSearchResultDto>> Handle(SearchTest rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes.FindAsync(rq.ClassId);
        if (classById == null)  
            throw new ErrorCodeException(ErrorCodes.CLASS_NOT_FOUND, "Lớp học không tồn tại"); 
        
        
            
        throw new NotImplementedException();
    }
}
