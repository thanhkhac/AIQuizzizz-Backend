using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Questions.Dtos;

public class QuestionResponseWithUserAnswerDto : QuestionResponseDto
{
    public float ScoreGraded { get; set; }
    public UserAnswerDataDto UserAnswerData { get; set; } = null!;
}

public class UserAnswerDataDto
{
    public string? Type { get; set; }
    public List<Guid>? MultipleChoice { get; set; }
    public List<UserMatchingAnswerDto>? Matching { get; set; }
    public List<UserOrderingAnswerDto>? Ordering { get; set; }
    public string? ShortText { get; set; }
}

public class UserMatchingAnswerDto
{
    public Guid LeftId { get; set; }
    public Guid RightId { get; set; }
}

public class UserOrderingAnswerDto
{
    public Guid ItemId { get; set; }
    public int Order { get; set; }
}

