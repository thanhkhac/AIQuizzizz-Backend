namespace CleanArchitectureBase.Application.Common.Models;

public class FileData
{
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public byte[]? Data { get; set; } 
}

public class FileStreamData
{
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public Stream? Data { get; set; }
}
