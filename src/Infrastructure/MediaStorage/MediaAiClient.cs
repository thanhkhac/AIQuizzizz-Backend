using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Settings;
using CleanArchitectureBase.Application.MediaFiles.Services;
using Hangfire;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Infrastructure.MediaStorage;

/// <summary>
/// HTTP client tới service media-ai (FastAPI): /media/process, /moderate/image, /media/delete
/// </summary>
public class MediaAiClient : IMediaAiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public MediaAiClient(HttpClient httpClient, IOptions<MediaSettings> settings)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(settings.Value.MediaAiBaseUrl.TrimEnd('/') + "/");
        if (!string.IsNullOrEmpty(settings.Value.MediaAiApiKey))
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", settings.Value.MediaAiApiKey);
    }

    public async Task<MediaProcessResult> ProcessAsync(Stream file, string fileName, string contentType, Guid mediaId,
        string kind, CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(file);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent(mediaId.ToString()), "mediaId");
        form.Add(new StringContent(kind), "kind");

        using var response = await _httpClient.PostAsync("media/process", form, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<MediaProcessResult>(JsonOptions, cancellationToken))!;
    }

    public async Task<ModerationResult> ModerateImageAsync(string objectKey, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("moderate/image", new { objectKey }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<ModerationResult>(JsonOptions, cancellationToken))!;
    }

    public async Task DeleteAsync(IEnumerable<string> keys, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("media/delete", new { keys = keys.ToList() }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var code = "MEDIA_PROCESS_FAILED";
        var message = body;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String)
                code = err.GetString()!;
            if (doc.RootElement.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                message = msg.GetString()!;
        }
        catch (JsonException)
        {
            // body không phải JSON
        }

        throw new MediaAiException(code, (int)response.StatusCode, message);
    }
}

public class HangfireModerationScheduler : IModerationScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireModerationScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void EnqueueImageModeration(Guid mediaId)
    {
        _backgroundJobClient.Enqueue<IModerationService>(s => s.ModerateImageAsync(mediaId));
    }
}
