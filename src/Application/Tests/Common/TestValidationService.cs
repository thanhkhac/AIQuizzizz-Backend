using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Tests.Common;

public class TestValidationService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public TestValidationService(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task ValidateQuestionAccessAsync(List<CreateQuestionDto> question , CancellationToken cancellationToken)
    {
        var questionSetPairs = question
            .Where(q => q.QuestionId.HasValue)
            .Select(q => q.QuestionId)
            .ToList();

        if (questionSetPairs.Count == 0) return;

        var validQuestion = await _context.Questions
            .Where(q => questionSetPairs.Contains(q.Id))
            .Include(qs => qs.QuestionSet)
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
                    invalidQuestion.Select(q => $"Question {q!.Value} không có quyền truy cập hoặc không tồn tại").ToArray()   
                }
            };
            
            throw new ErrorCodeException(errors);
        } 
    }
}
