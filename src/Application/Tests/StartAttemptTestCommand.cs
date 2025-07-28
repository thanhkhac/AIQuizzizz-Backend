using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Application.Tests.Service;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

[Authorize]
public class StartAttemptTestCommand : IRequest<AttemptDetailDto>
{
    /// <summary>
    /// Id of the test the student chose to take
    /// </summary>
    public Guid TestId { get; set; }
}

public class StartAttemptTestCommandHandler : IRequestHandler<StartAttemptTestCommand, AttemptDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITestService _testService;
    private readonly IUser _user;
    
    public StartAttemptTestCommandHandler(IApplicationDbContext context, ITestService testService, IUser user)
    {
        _context = context;
        _testService = testService;
        _user = user;
    }
    
    /// <summary>
    /// The function creates test attempts for students and returns the test questions
    /// </summary>
    /// <param name="rq">Request contains TestId information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<AttemptDetailDto> Handle(StartAttemptTestCommand rq, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .Where(x => x.Id == rq.TestId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (test == null)
            throw new ErrorCodeException(ErrorCodes.TEST_NOT_FOUND, "Bài test không tồn tại");
        
        await _testService.TryCheckCanAttemptTest(test);

        var attemptUser = _context.Attempts
            .Where(x => x.UserId == _user.UserId && x.TestId == rq.TestId)
            .ToList();
        if (attemptUser.Count > test.MaxAttempt)
            throw new ErrorCodeException(ErrorCodes.MAX_ATTEMPT_IN_THIS_TEST, "Đã hết lượt làm bài");
        
        var testVersionId = await _context.TestVersions
            .Where(x => x.Test!.Id == test.Id)
            .Select(x => x.Id)
            .OrderBy(_ => Guid.NewGuid())
            .FirstOrDefaultAsync(cancellationToken);
        
        if (testVersionId == Guid.Empty)
        {
            throw new InvalidOperationException("Không tìm thấy phiên bản bài test nào.");
        }

        var attempt = new Attempt
        {
            Id = Guid.NewGuid(),
            TestVersionId = testVersionId,
            TimeStart = DateTime.UtcNow,
            TimeFinish = DateTime.UtcNow.AddMinutes(test.TimeLimit),
            Score = 0,
            UserId = _user.UserId ?? Guid.Empty,
            TestId = rq.TestId
        };

        var attemptDetail = new AttemptDetailDto
        {
            AttemptId = attempt.Id,
            Name = test.Name,
            TimeStart = test.TimeStart,
            TimeEnd = test.TimeFinish,
            TimeLimit = test.TimeLimit,
        };

        var questions = _context.TestVersionQuestions
            .Include(x => x.Question)
            .Where(x => x.TestVersion!.Id == testVersionId)
            .OrderBy(x => x.Order)
            .Select(q => QuestionAttemptDetailDto.Mapper.FromEntity(q.Question!));
        
        attemptDetail.Questions = questions.ToList();
        attemptDetail.QuestionCount = questions.Count();
        
        _context.Attempts.Add(attempt);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return attemptDetail;
    }
}
