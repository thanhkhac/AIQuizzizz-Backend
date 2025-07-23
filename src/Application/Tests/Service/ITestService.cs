using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests.Service;

public interface ITestService
{
    Task<List<Guid>> QuestionAccessAndCompareForTest(List<CreateUpdateQuestionDto> question, CancellationToken cancellationToken);
    Task<Test> CanEditTest(Guid testId, CancellationToken cancellationToken);
}

public class TestService : ITestService
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public TestService(
        IApplicationDbContext context,
        IUser user,
        IClassService classService,
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _classService = classService;
        _identityService = identityService;
    }

    public async Task<List<Guid>> QuestionAccessAndCompareForTest(List<CreateUpdateQuestionDto> question, CancellationToken cancellationToken)
    {
        var questionSetPairs = question
            .Where(q => q.QuestionId.HasValue && q.QuestionId!=Guid.Empty)
            .Select(q => q.QuestionId)
            .ToList();

        if (questionSetPairs.Count == 0) return new List<Guid>();
        
        var validQuestionInTestTemplate = await _context.TestTemplateQuestions
            .Include(x => x.Question)
            .Include(x => x.TestTemplate)
            .ThenInclude(x => x!.TestTemplateUsers)
            .Include(qs => qs.TestTemplate)
            .ThenInclude(x => x!.FolderTestTemplates)
            .ThenInclude(x => x.Folder)
            .ThenInclude(x => x!.FolderUsers)
            .Where(q => questionSetPairs.Contains(q.QuestionId))
            .Where(q => 
                q.TestTemplate!.TestTemplateUsers.Any(tu => tu.UserId == _user.UserId)
                || q.TestTemplate!.FolderTestTemplates.Any(ft => 
                    ft.Folder!.FolderUsers.Any(fu => fu.UserId == _user.UserId)))
            .Distinct()
            .ToListAsync(cancellationToken);
            
        
        var invalidQuestion = questionSetPairs
            .Where(q => !validQuestionInTestTemplate.Any(v => v.QuestionId.Equals(q!.Value)))
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
        
        return validQuestionInTestTemplate
            .Where(x =>
                CreateUpdateQuestionDto.Compare.CompareQuestion(x.Question!, question.First(q => q.QuestionId!.Equals(x.QuestionId))))
            .Select(x => x.QuestionId)
            .ToList();
    }

    public async Task<Test> CanEditTest(Guid testId, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .Where(x => x.Id == testId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (test == null)
            throw new ErrorCodeException(ErrorCodes.TEST_NOT_FOUND, "Không tìm thấy test");

        if (!await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator,
                Domain.Constants.Roles.Moderator))
        {
            var isLecturerOrOwnerInClass = await _classService.IsLecturerOrOwnerInClass(test.ClassId);
            if (!isLecturerOrOwnerInClass)
                throw new ErrorCodeException(ErrorCodes.NOT_FOUND_TEACHER_OR_OWNER_IN_CLASS, "Không phải lecturer hoặc owner của class");
        }
        
        return test;
    }
}
