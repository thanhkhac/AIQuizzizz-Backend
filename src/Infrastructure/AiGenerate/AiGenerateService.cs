using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Settings;
using CleanArchitectureBase.Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Infrastructure.AiGenerate;

public class AiGenerateService : IAiGenerateService
{
    private readonly HttpClient _httpClient;

    private readonly GeminiSettings _geminiSettings;

    public AiGenerateService(HttpClient httpClient, IOptions<GeminiSettings> geminiSettings)
    {
        _httpClient = httpClient;
        _geminiSettings = geminiSettings.Value;
    }

    public async Task<string> SendPromptWithFileAsync(
        FileStreamData fileData,
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var fileUri = await UploadFileAsync(fileData);

        var extractFile = await SendPromptWithFileUriAsync(fileUri, prompt);

        var result = await SendPromptWithFileUriAsync(fileUri, prompt);
        return result;
    }


    private async Task<string> UploadFileAsync(FileStreamData fileData)
    {
        var content = new StreamContent(fileData.Data!);

        var uploadUri = $"{_geminiSettings.UploadFileUri}?key={_geminiSettings.ApiKey}";

        var response = await _httpClient.PostAsync(uploadUri, content);

        if (!response.IsSuccessStatusCode)
        {
            throw new ErrorCodeException(ErrorCodes.FILE_UPLOAD_FAILED);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseContent);

        var fileUri = doc.RootElement
            .GetProperty("file")
            .GetProperty("uri")
            .GetString();

        if (string.IsNullOrEmpty(fileUri))
        {
            throw new ErrorCodeException(ErrorCodes.FILE_URI_NOTFOUND);
        }

        return fileUri;
    }

    private async Task<string> SendPromptWithFileUriAsync(string fileUri, string prompt)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new
                        {
                            text = prompt,
                        }
                    }
                },
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        // new
                        // {
                        //     text = prompt
                        // },
                        new
                        {
                            fileData = new
                            {
                                fileUri
                            }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

        var requestUri = $"{_geminiSettings.GenerateUri}?key={_geminiSettings.ApiKey}";
        var response = await _httpClient.PostAsync(requestUri, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED);
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var result = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return result ?? "No summary found.";
    }


}
