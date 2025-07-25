using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.Tests;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.TestTemplates.Service;

public interface ITestTemplateService
{
    Task QuestionAccessForTestTemplate(List<CreateUpdateQuestionDto> question, CancellationToken cancellationToken);
    Task CanViewTesTemplate (Guid testTemplateId);
    Task CanUseTesTemplate (Guid testTemplateId);
    Task<TestTemplate> CanDeleteTestTemplate (Guid testTemplateId, CancellationToken cancellationToken);
    Task<TestTemplate> CanEditTestTemplate (Guid testTemplateId, CancellationToken cancellationToken);
}

public class TestTemplateService : ITestTemplateService
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    
    public TestTemplateService(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }

    public async Task QuestionAccessForTestTemplate(List<CreateUpdateQuestionDto> question, CancellationToken cancellationToken)
    {
        var questionSetPairs = question
            .Where(q => q.QuestionId.HasValue && q.QuestionId!=Guid.Empty)
            .Select(q => q.QuestionId)
            .ToList();

        if (questionSetPairs.Count == 0) return;

        var validQuestion = await _context.Questions
            .Where(q => questionSetPairs.Contains(q.Id))
            .Include(qs => qs.QuestionSet)
            .Where(qs => qs.QuestionSet != null)
            .GroupJoin(_context.QuestionSetUsers,
                qs => qs.QuestionSet!.Id,
                qsu => qsu.QuestionSetId,
                (qs, qsu) => new
                {
                    QuestionSet = qs, QuestionSetUser = qsu.FirstOrDefault(qsu => qsu.UserId == _user.UserId)
                })
            .Where(qs => qs.QuestionSet.CreatedBy.Equals(_user.UserId)
                         || qs.QuestionSetUser!.UserId.Equals(_user.UserId))
            .ToListAsync(cancellationToken);
        
        

        var invalidQuestion = questionSetPairs
            .Where(q => !validQuestion.Any(v => v.QuestionSet.Id == q!.Value))
            .ToList();

        if (invalidQuestion.Count > 0)
        {
            var errors = new Dictionary<string, string[]>
            {
                {
                    ErrorCodes.USER_NOT_ACCESS_TO_QUESTION_SET,
                    invalidQuestion.Select(q => $"Question {q!.Value} không có quyền truy cập hoặc không tồn tại")
                        .ToArray()
                }
            };

            throw new ErrorCodeException(errors);
        }
    }

    public async Task CanViewTesTemplate(Guid testTemplateId)
    {
        var accessToView = await _context.TestTemplateUsers
            .Where(t => t.UserId == _user.UserId && t.TestTemplateId == testTemplateId)
            .FirstOrDefaultAsync();
        
        var accessToViewInFolder = await _context.FolderTestTemplates
            .Include(x => x.Folder!)
            .ThenInclude(x => x.FolderUsers)
            .Where(x => x.TestTemplateId.Equals(testTemplateId) && x.Folder!.FolderUsers.Any(y => y.UserId == _user.UserId))
            .FirstOrDefaultAsync();
        
        if (accessToView == null && accessToViewInFolder == null)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE, "User không có quyền xem test template này");
    }

    public async Task CanUseTesTemplate(Guid testTemplateId)
    {
        var accessToView = await _context.TestTemplateUsers
            .Where(t => t.UserId.Equals(_user.UserId) && t.TestTemplateId.Equals(testTemplateId))
            .FirstOrDefaultAsync();
        
        if (accessToView == null)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE, "User không có quyền dùng test template này");
    }

    public async Task<TestTemplate> CanDeleteTestTemplate(Guid testTemplateId, CancellationToken cancellationToken)
    {
        var testTemplate = await _context.TestTemplates
            .Where(t => t.Id == testTemplateId && t.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (testTemplate == null)
            throw new ErrorCodeException(ErrorCodes.TEST_TEMPLATE_NOT_FOUND, "Không tìm thấy test template");
        
        if (!await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator,
                Domain.Constants.Roles.Moderator))
        {
            var userTestTemplate = await _context.TestTemplateUsers
                .Where(x => x.UserId == _user.UserId &&
                            x.TestTemplateId == testTemplateId &&
                            TestTemplateUserShareMode.Owner == x.ShareMode)
                .FirstOrDefaultAsync(cancellationToken);
            if (userTestTemplate == null)
                throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE,
                    "Không có quyền xóa");
        }

        return testTemplate;
    }

    public async Task<TestTemplate> CanEditTestTemplate(Guid testTemplateId, CancellationToken cancellationToken)
    {
        var testTemplate = await _context.TestTemplates
            .Where(t => t.Id == testTemplateId && t.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (testTemplate == null)
            throw new ErrorCodeException(ErrorCodes.TEST_TEMPLATE_NOT_FOUND, "Không tìm thấy test template");
        
        if (!await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator,
                Domain.Constants.Roles.Moderator))
        {
            var userTestTemplate = await _context.TestTemplateUsers
                .Where(x => x.UserId == _user.UserId &&
                            x.TestTemplateId == testTemplateId &&
                            (TestTemplateUserShareMode.Owner == x.ShareMode || TestTemplateUserShareMode.Editable == x.ShareMode))
                .FirstOrDefaultAsync(cancellationToken);
            if (userTestTemplate == null)
                throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE,
                    "Không có quyền edit");
        }

        return testTemplate;
    }
}
