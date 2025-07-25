namespace CleanArchitectureBase.Application.FolderTest.Dto;

public class ResourceShareDto
{
    public Guid Id { get; set; }
    public List<SharingModelDto> SharingModel { get; set; } = new();
}

public class SharingModelDto
{
    public string? ShareMode { get; set; }
    public List<SharingUserDto>? SharingUsers { get; set; }
}

public class SharingUserDto
{
    public Guid UserId { get; set; }
    public string? FullName { get; set; }
}

public class UpdateSharing
{
    public List<UpdateSharingModelDto> SharingModel { get; set; } = new();
}

public class UpdateSharingModelDto
{
    public string? ShareMode { get; set; }
    public List<Guid>? SharingUsers { get; set; }
}
