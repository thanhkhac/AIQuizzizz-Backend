namespace CleanArchitectureBase.Application.FolderTest.Dto;

public class ResourceShareDto
{
    public Guid Id { get; set; }
    public List<SharingModelDto> SharingModel { get; set; } = new();
}

public class SharingModelDto
{
    public string? ShareMode { get; set; }
    public Guid UserId { get; set; }
    public string? FullName { get; set; }
}


public class UpsertSharingModelDto
{
    public string? ShareMode { get; set; }
    public Guid SharingUserId { get; set; }
}
