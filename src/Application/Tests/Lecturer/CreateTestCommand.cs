using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Common;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests.Lecturer;

public class CreateTestCommand : IRequest<Guid>
{
    public required string Name { get; set; }
    public Guid? ClassId { get; set; }
    public required int TimeLimit { get; set; }
    public required int QuestionCount { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required GradeAttemptMethod GradeAttemptMethod { get; set; }
    public required GradeQuestionMethod GradeQuestionMethod { get; set; }
    public bool? IsShowCorrectAnswerInReview { get; set; }
    public List<CreateQuestionDto> Questions { get; set; } = new ();
}

public class CreateTestCommandValidator : AbstractValidator<CreateTestCommand>
{
    public CreateTestCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên bài kiểm tra không được để trống")
            .MaximumLength(200).WithMessage("Tên bài kiểm tra không được vượt quá 200 ký tự");

        RuleFor(x => x.TimeLimit)
            .GreaterThan(0).WithMessage("Thời gian làm bài phải lớn hơn 0")
            .LessThanOrEqualTo(180).WithMessage("Thời gian làm bài không được vượt quá 180 phút");

        RuleFor(x => x.QuestionCount)
            .GreaterThan(0).WithMessage("Số lượng câu hỏi phải lớn hơn 0")
            .LessThanOrEqualTo(100).WithMessage("Số lượng câu hỏi không được vượt quá 100");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Thời gian bắt đầu không được để trống")
            .Must(startTime => startTime > DateTime.UtcNow)
            .WithMessage("Thời gian bắt đầu phải lớn hơn thời gian hiện tại");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("Thời gian kết thúc không được để trống")
            .Must((command, endTime) => endTime > command.StartTime)
            .WithMessage("Thời gian kết thúc phải lớn hơn thời gian bắt đầu");

        RuleFor(x => x.GradeAttemptMethod)
            .IsInEnum().WithMessage("Phương thức tính điểm lần thi không hợp lệ");

        RuleFor(x => x.GradeQuestionMethod)
            .IsInEnum().WithMessage("Phương thức tính điểm câu hỏi không hợp lệ");
        
        RuleForEach(x => x.Questions)
            .SetValidator((command, question) => new QuestionCreateDtoValidator());
    }
    
    private bool IsValidQuestionType(CreateQuestionDto createQuestion)
    {
        bool isValid = createQuestion.Type switch
        {
            nameof(QuestionType.MultipleChoice) => createQuestion.MultipleChoices != null && createQuestion.MultipleChoices.Any(),
            nameof(QuestionType.Matching) => createQuestion.MatchingPairs != null && createQuestion.MatchingPairs.Any(),
            nameof(QuestionType.Ordering) => createQuestion.OrderingItems != null && createQuestion.OrderingItems.Any(),
            nameof(QuestionType.ShortText) => !string.IsNullOrWhiteSpace(createQuestion.ShortAnswer),
            _ => false
        };

        return isValid;
    }
}
