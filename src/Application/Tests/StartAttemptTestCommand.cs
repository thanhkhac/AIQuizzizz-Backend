using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

public class StartAttemptTestCommand : IRequest<AttemptDetailDto>
{
    public Guid TestId { get; set; }
}

public class StartAttemptTestCommandHandler : IRequestHandler<StartAttemptTestCommand, AttemptDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    private readonly IUser _user;
    
    public StartAttemptTestCommandHandler(IApplicationDbContext context, IClassService classService, IUser user)
    {
        _context = context;
        _classService = classService;
        _user = user;
    }
    
    public async Task<AttemptDetailDto> Handle(StartAttemptTestCommand rq, CancellationToken cancellationToken)
    {
        var test = await _context.Tests.Where(x => x.Id == rq.TestId)
            .FirstOrDefaultAsync(cancellationToken);
        if (test == null)
            throw new ErrorCodeException(ErrorCodes.TEST_NOT_FOUND, "Bài test không tồn tại");
        
        if (test.TimeStart < DateTime.UtcNow)
            throw new ErrorCodeException(ErrorCodes.TEST_IS_OVERDUE, "Hết hạn làm bài");
        
        await _classService.IsStudentInClass(test.ClassId);
        
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
        
        // var questions = _context.TestVersionQuestions
        //     .Include(x => x.Question)
        //     .Where(x => x.TestVersion!.Id == testVersionId)
        //     .Select(q => QuestionResponseDto.Mapper.FromEntity())
        
        _context.Attempts.Add(attempt);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return new AttemptDetailDto();
    }
}
