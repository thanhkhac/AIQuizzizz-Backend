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
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ITestTemplateService _testTemplateService;

    public GetTestTemplateDetailQueryHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService,
        ITestTemplateService testTemplateService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
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
        
        var isAdmin = await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator);

        if(!isAdmin) await _testTemplateService.CanViewTesTemplate(rq.TestTemplateId);

        var result = await _context.TestTemplates
            .Include(t => t.TestTemplateQuestions)
            .ThenInclude(t => t.Question)
            .Where(t => t.Id == rq.TestTemplateId)
            .Select(t => new TestTemplateDetailDto
            {
                TestTemplateId = t.Id,
                Name = t.Name,
                QuestionCount = t.TestTemplateQuestions.Count,
                Questions = t.TestTemplateQuestions
                    .Where(tq => tq.Question != null)
                    .Select(tq => QuestionResponseDto.Mapper.FromEntity(tq.Question!, true))
                    .ToList()
            }).FirstOrDefaultAsync(cancellationToken);
        
        return result != null ? result : new TestTemplateDetailDto();
    }
}
