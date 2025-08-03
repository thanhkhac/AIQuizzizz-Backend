using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.FolderTest.Dto;

namespace CleanArchitectureBase.Application.QuestionSets.Commands;

public class UpdateSharingQuestionSetCommand
{
    [JsonIgnore]
    public Guid QuestionSetId { get; set; }
    public string? VisibilityMode { get; set; }
    public List<UpsertSharingModelDto> SharingModel { get; set; } = new();
    public List<Guid>? DeleteUserIds { get; set; } = new();
}
