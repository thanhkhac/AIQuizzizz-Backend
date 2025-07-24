namespace CleanArchitectureBase.Application.FolderTest.Dto;

public class ResourceShareDto
{
    public Guid Id { get; set; }
    public List<SharingModelDto> SharingModel { get; set; } = new();
}

public class SharingModelDto
{
    public string? SharedMode { get; set; }
    public List<SharingUserDto>? SharingUsers { get; set; }
}

public class SharingUserDto
{
    public Guid UserId { get; set; }
    public string? FullName { get; set; }
}
