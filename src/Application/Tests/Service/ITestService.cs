using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Tests.Service;

public interface ITestService
{
    Task QuestionAccessForTestTemplate(List<CreateUpdateQuestionDto> question, CancellationToken cancellationToken);
    Task QuestionAccessForTest(List<CreateUpdateQuestionDto> question, CancellationToken cancellationToken);
}

public class TestService : ITestService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public TestService(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
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

    public async Task QuestionAccessForTest(List<CreateUpdateQuestionDto> question, CancellationToken cancellationToken)
    {
        var questionSetPairs = question
            .Where(q => q.QuestionId.HasValue && q.QuestionId!=Guid.Empty)
            .Select(q => q.QuestionId)
            .ToList();

        if (questionSetPairs.Count == 0) return;
        
        var validQuestionInTestTemplate = await _context.TestTemplateQuestions
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
            .Select(q => q.QuestionId)
            .Distinct()
            .ToListAsync(cancellationToken);
            
        
        var invalidQuestion = questionSetPairs
            .Where(q => !validQuestionInTestTemplate.Any(v => v == q!.Value))
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
}
