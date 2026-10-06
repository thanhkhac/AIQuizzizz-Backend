using CleanArchitectureBase.Application.Tags.Dto;

namespace CleanArchitectureBase.Application.QuestionSets.Dtos;

public class QuestionSetForListResponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int TotalQuestionCount { get; set; }
    public int RatingCount { get; set; }
    public double RatingAverage { get; set; }
    public string? VisibilityMode { get; set; }
    public int CompletedQuestionCount { get; set; }
    private string? _createBy;

    /// <summary>Tên hiển thị của chủ sở hữu; nếu FullName đang là email thì được che (a***@domain).</summary>
    public string? CreateBy
    {
        get => _createBy;
        set => _createBy = CleanArchitectureBase.Application.Common.Utils.DisplayNameHelper.MaskIfEmail(value);
    }
    public DateTimeOffset? CreatedAt { get; set; }
    public Guid? CreatedById { get; set; }
    public List<TagForListReponseDto> Tags { get; set; } = new();
    public DateTimeOffset? LastAccessByMe { get; set; }
}
