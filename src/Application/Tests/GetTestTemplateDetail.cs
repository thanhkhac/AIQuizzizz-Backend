using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Tests;

public class GetTestTemplateDetail : IRequest<TestTemplateDetailDto>
{
    public required Guid TestTemplateId { get; set; }
}

public class GetTestTemplateDetailValidator : AbstractValidator<GetTestTemplateDetail>
{
    public GetTestTemplateDetailValidator()
    {
        RuleFor(x => x.TestTemplateId)
            .NotEmpty().WithMessage("TestTemplateId không được trống");
    }
}

public class GetTestTemplateDetailHandler : IRequestHandler<GetTestTemplateDetail, TestTemplateDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public GetTestTemplateDetailHandler(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }
    
    public async Task<TestTemplateDetailDto> Handle(GetTestTemplateDetail rq, CancellationToken cancellationToken)
    {
        var testTemplate = await _context.TestTemplates.Where(t => t.Id == rq.TestTemplateId && t.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (testTemplate == null)
            throw new ErrorCodeException(ErrorCodes.TEST_TEMPLATE_NOT_FOUND, "TestTemplate không tồn tại");
        
        var isAdmin = await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator);
        
        var accessToView = await _context.TestTemplateUsers
            .Where(t => t.UserId == _user.UserId && t.TestTemplateId == rq.TestTemplateId)
            .FirstOrDefaultAsync(cancellationToken);
        if (accessToView == null && !isAdmin)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE, "User không có quyền xem test template này");

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
