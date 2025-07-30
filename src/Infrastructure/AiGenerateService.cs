using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;
using Microsoft.Extensions.Configuration;

namespace CleanArchitectureBase.Infrastructure;

public class AiGenerateService : IAiGenerateService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _uploadApiUrl;
    private readonly string _generateContentApiUrl;
    private readonly Dictionary<string, string> _defaultPrompts;

    public AiGenerateService(IConfiguration configuration, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiApi:ApiKey"] ?? throw new ErrorCodeException(ErrorCodes.API_KEY_NOTFOUND);
        _uploadApiUrl = configuration["GeminiApi:UploadFileUri"] ?? throw new ErrorCodeException(ErrorCodes.API_KEY_NOTFOUND);
        _generateContentApiUrl = configuration["GeminiApi:GenerateUri"] ?? throw new ErrorCodeException(ErrorCodes.GENERATE_URI_NOTFOUND);
        _defaultPrompts = configuration.GetSection("GeminiApi:DefaultPrompts")
            .Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();

    }

    public async Task<string> SendPromptWithFileAsync(FileStreamData fileData)
    {
        var fileUri = await UploadFileAsync(fileData);
        
        var extractFile = await SendPromptAsync(fileUri, _defaultPrompts.GetValueOrDefault("ExtractFile") ?? 
                                                         throw new ErrorCodeException(ErrorCodes.PROMPT_NOT_FOUND));
        
        // var generateQuestions = await SendPromptAsync(fileUri, _defaultPrompts.GetValueOrDefault("GenerateQuestion") ?? 
        //                                                        throw new ErrorCodeException(ErrorCodes.PROMPT_NOT_FOUND));
        return extractFile;
    }

    private async Task<string> UploadFileAsync(FileStreamData fileData)
    {
        var content = new StreamContent(fileData.Data!);

        var uploadUri = $"{_uploadApiUrl}?key={_apiKey}";
        
        var response = await _httpClient.PostAsync(uploadUri, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
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

    private async Task<string> SendPromptAsync(string fileUri, string prompt)
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
                        new { text = prompt },
                        new { fileData = new { fileUri } }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

        var requestUri = $"{_generateContentApiUrl}?key={_apiKey}";
        var response = await _httpClient.PostAsync(requestUri, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED);
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var summary = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return summary ?? "No summary found.";
    }
}

