namespace CleanArchitectureBase.Application.QuestionSets.Common;

public class CreateMultipleChoiceDto
{
    public string? Text { get; set; }
    public bool IsAnswer { get; set; }
}

public class CreateMatchingPairDto
{
    public string? LeftItem { get; set; }
    public string? RightItem { get; set; }
}

public class CreateOrderingItemDto
{
    public string? Text { get; set; }
    public int CorrectOrder { get; set; }
}


public class CreateQuestionDto
{
    public Guid? QuestionSetId { get; set; }
    public string? Type { get; set; }
    public string? QuestionText { get; set; }
    public string? ExplainText { get; set; } //TODO: Thêm trường explain cho entity
    public required float Score { get; set; }
    public List<CreateMultipleChoiceDto>? MultipleChoices { get; set; }
    public List<CreateMatchingPairDto>? MatchingPairs { get; set; }
    public List<CreateOrderingItemDto>? OrderingItems { get; set; }
    public string? ShortAnswer { get; set; }
}
