using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.TestTemplates.Dto;
using CleanArchitectureBase.Application.TestTemplates.Service;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.TestTemplates;

[Authorize]
public class GetTestTemplateDetailQuery : IRequest<TestTemplateDetailDto>
{
    public required Guid TestTemplateId { get; set; }
}

public class GetTestTemplateDetailQueryValidator : AbstractValidator<GetTestTemplateDetailQuery>
{
    public GetTestTemplateDetailQueryValidator()
    {
        RuleFor(x => x.TestTemplateId)
            .NotEmpty().WithMessage("TestTemplateId không được trống");
    }
}

public class GetTestTemplateDetailQueryHandler : IRequestHandler<GetTestTemplateDetailQuery, TestTemplateDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITestTemplateService _testTemplateService;

    public GetTestTemplateDetailQueryHandler(
        IApplicationDbContext context,
        ITestTemplateService testTemplateService)
    {
        _context = context;
        _testTemplateService = testTemplateService;
    }
    
    /// <summary>
    /// The function retrieves details of a test template, including its questions, and returns the test template details
    /// </summary>
    /// <param name="rq">Request contains TestTemplateId information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<TestTemplateDetailDto> Handle(GetTestTemplateDetailQuery rq, CancellationToken cancellationToken)
    {
        var testTemplate = await _context.TestTemplates
            .Where(t => t.Id == rq.TestTemplateId && t.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (testTemplate == null)
            throw new ErrorCodeException(ErrorCodes.TEST_TEMPLATE_NOT_FOUND, "TestTemplate không tồn tại");
        
        var canView = await _testTemplateService.CanViewTesTemplate(rq.TestTemplateId);
        if (!canView)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE, "User không có quyền xem test template này");
        
        var result = await _context.TestTemplates
            .Include(x => x.CreatedByUser)
            .Include(t => t.TestTemplateQuestions)
            .ThenInclude(t => t.Question)
            .Where(t => t.Id == rq.TestTemplateId)
            .Select(t => new TestTemplateDetailDto
            {
                TestTemplateId = t.Id,
                Name = t.Name,
                QuestionCount = t.TestTemplateQuestions.Count,
                Description = t.Description,
                CreateBy = t.CreatedByUser!.FullName,
                CreateAt = t.Created,
                Questions = t.TestTemplateQuestions
                    .Where(tq => tq.Question != null)
                    .Select(tq => QuestionResponseDto.Mapper.FromEntity(tq.Question!, true, false, true))
                    .ToList()
            }).FirstOrDefaultAsync(cancellationToken);
        
        return result != null ? result : new TestTemplateDetailDto();
    }
}
