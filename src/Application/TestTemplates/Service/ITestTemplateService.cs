using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.Tests;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.TestTemplates.Service;

public interface ITestTemplateService
{
    Task TryQuestionAccessForTestTemplate(List<CreateUpdateQuestionDto> question, CancellationToken cancellationToken);
    Task<bool> CanViewTesTemplate (Guid testTemplateId);
    Task<bool> CanDeleteTestTemplate (Guid testTemplateId, CancellationToken cancellationToken);
    Task<bool> CanEditTestTemplate (Guid testTemplateId, CancellationToken cancellationToken);
    CheckUpdateQuestion CheckQuestionsForUpdate(List<CreateUpdateQuestionDto> questionDtos, List<Question> questions);
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

    public async Task TryQuestionAccessForTestTemplate(List<CreateUpdateQuestionDto> questions, CancellationToken cancellationToken)
    {
        var questionIds = questions
            .Where(q => q.QuestionId.HasValue && q.QuestionId != Guid.Empty)
            .Select(q => q.QuestionId!.Value)
            .Distinct()
            .ToList();

        if (!questionIds.Any())
        {
            return;
        }
        
        var validQuestion = await _context.Questions
            .Where(q => questionIds.Contains(q.Id))
            .Include(qs => qs.QuestionSet)
            .Where(qs => qs.QuestionSet != null)
            .GroupJoin(_context.QuestionSetUsers,
                qs => qs.QuestionSet!.Id,
                qsu => qsu.QuestionSetId,
                (qs, qsu) => new
                {
                    QuestionSet = qs.QuestionSet,
                    QuestionSetUser = qsu.FirstOrDefault(qsu => qsu.UserId == _user.UserId),
                    QuestionId = qs.Id,
                })
            .Where(qs => qs.QuestionSetUser!.UserId.Equals(_user.UserId)
            || QuestionSetVisibilityMode.Public == qs.QuestionSet!.VisibilityMode)
            .ToListAsync(cancellationToken);
        
        var invalidQuestionIds = questionIds
            .Where(q => !validQuestion.Any(v => v.QuestionId == q))
            .ToList();

        if (invalidQuestionIds.Count > 0)
        {
            var errors = new Dictionary<string, string[]>
            {
                {
                    ErrorCodes.USER_NOT_ACCESS_TO_QUESTION_SET, 
                    invalidQuestionIds.Select(qId => $"Question {qId} is not accessible or does not exist").ToArray()
                }
            };

            throw new ErrorCodeException(errors);
        }
    }

    public async Task<bool> CanViewTesTemplate(Guid testTemplateId)
    {
        var isAdmin = await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator);
        
        var accessToView = await _context.TestTemplateUsers
            .Where(t => t.UserId == _user.UserId && t.TestTemplateId == testTemplateId)
            .FirstOrDefaultAsync();
        
        var accessToViewInFolder = await _context.FolderTestTemplates
            .Include(x => x.Folder!)
            .ThenInclude(x => x.FolderUsers)
            .Where(x => x.TestTemplateId.Equals(testTemplateId) && x.Folder!.FolderUsers.Any(y => y.UserId == _user.UserId))
            .FirstOrDefaultAsync();
        
        if (accessToView == null && accessToViewInFolder == null && !isAdmin)
            return false;
        
        return true;
    }

    public async Task<bool> CanDeleteTestTemplate(Guid testTemplateId, CancellationToken cancellationToken)
    {
        if (!await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator,
                Domain.Constants.Roles.Moderator))
        {
            var userTestTemplate = await _context.TestTemplateUsers
                .Where(x => x.UserId == _user.UserId &&
                            x.TestTemplateId == testTemplateId &&
                            TestTemplateUserShareMode.Owner == x.ShareMode)
                .FirstOrDefaultAsync(cancellationToken);
            if (userTestTemplate == null)
                return false;
        }

        return true;
    }

    public async Task<bool> CanEditTestTemplate(Guid testTemplateId, CancellationToken cancellationToken)
    {
        if (!await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator,
                Domain.Constants.Roles.Moderator))
        {
            var userTestTemplate = await _context.TestTemplateUsers
                .Where(x => x.UserId == _user.UserId &&
                            x.TestTemplateId == testTemplateId &&
                            (TestTemplateUserShareMode.Owner == x.ShareMode || TestTemplateUserShareMode.Editable == x.ShareMode))
                .FirstOrDefaultAsync(cancellationToken);
            if (userTestTemplate == null)
                return false;
        }

        return true;
    }

    public CheckUpdateQuestion CheckQuestionsForUpdate(List<CreateUpdateQuestionDto> questionDtos, List<Question> questions)
    {
        if (questionDtos.Count == 0 || questionDtos.Count == 0) return new CheckUpdateQuestion();

        var notUpdateQuestionIds = questions
            .Where(x =>
                CreateUpdateQuestionDto.Compare.CompareQuestion(x, questionDtos.First(q => q.QuestionId!.Equals(x.Id))))
            .Select(x => x.Id)
            .ToList();

        var updateQuestionIds = questionDtos
            .Where(x => !notUpdateQuestionIds.Contains(x.QuestionId!.Value))
            .Select(x => x.QuestionId!.Value)
            .ToList();

        return new CheckUpdateQuestion
        {
            UpdateQuestionIds = updateQuestionIds,
            NotUpdateQuestionIds = notUpdateQuestionIds
        };
    }
}
