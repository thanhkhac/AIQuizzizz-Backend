using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Questions.Dtos;

public class QuestionResponseDto
{
    public Guid Id { get; set; }
    public Guid QuestionSetId { get; set; }
    public string Type { get; set; } = null!;
    public TextFormat TextFormat { get; set; }
    public string QuestionText { get; set; } = null!;
    public float Score { get; set; }
    public bool Completed { get; set; }
    public QuestionDataDto QuestionData { get; set; } = null!;
}

public class QuestionDataDto
{
    public List<MultipleChoiceItemDto>? MultipleChoice { get; set; }
    public MatchingDataDto? Matching { get; set; }
    public List<OrderingItemDto>? Ordering { get; set; }
    public string? ShortText { get; set; }
}

public class MultipleChoiceItemDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public bool IsAnswer { get; set; }
}

public class MatchingDataDto
{
    public List<MatchingItemDto> LeftItems { get; set; } = new();
    public List<MatchingItemDto> RightItems { get; set; } = new();
    public List<MatchDto> Matches { get; set; } = new();
}

public class MatchingItemDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
}

public class MatchDto
{
    public Guid LeftId { get; set; }
    public Guid RightId { get; set; }
}

public class OrderingItemDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public int CorrectOrder { get; set; }
}
