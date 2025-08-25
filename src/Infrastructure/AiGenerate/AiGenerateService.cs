using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CleanArchitectureBase.Application.AiGenerate.Services;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Settings;
using CleanArchitectureBase.Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Infrastructure.AiGenerate;

//Tài nguyên:
//Upload file: https://ai.google.dev/api/files
//Sytem instruction: https://cloud.google.com/vertex-ai/generative-ai/docs/learn/prompts/system-instructions#gemini-system-instructions-code-samples-drest
public class AiGenerateService : IAiGenerateService
{
    private readonly HttpClient _httpClient;

    private readonly GeminiSettings _geminiSettings;

    private readonly IGoogleAccessTokenProvider _googleAccessTokenProvider;

    public AiGenerateService(HttpClient httpClient, IOptions<GeminiSettings> geminiSettings, IGoogleAccessTokenProvider googleAccessTokenProvider)
    {
        _httpClient = httpClient;
        _googleAccessTokenProvider = googleAccessTokenProvider;
        _geminiSettings = geminiSettings.Value;
    }

    public async Task<string> SendPromptWithFileAsync(
        string fileUri,
        string systemInstruction,
        string prompt,
        double temperature = 0,
        double topP = 1,
        CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new object[]
                {
                    new
                    {
                        text = systemInstruction
                    }
                }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new
                        {
                            fileData = new
                            {
                                fileUri
                            }
                        },
                        new
                        {
                            text = prompt
                        }
                    }
                }
            },
            generation_config = new
            {
                temperature = temperature,
                topP = 1,
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

        var requestUri = $"{_geminiSettings.GenerateUri}?key={_geminiSettings.ApiKey}";
        var response = await _httpClient.PostAsync(requestUri, requestContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED);


        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);

        var result = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return result ?? throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED);
    }


    public async Task<string> SendPromptAsync(
        string systemInstruction,
        string prompt,
        double temperature = 0.7,
        double topP = 1,
        CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new object[]
                {
                    new
                    {
                        text = systemInstruction
                    }
                }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            },
            generation_config = new
            {
                temperature = 0.7,
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

        var requestUri = $"{_geminiSettings.GenerateUri}?key={_geminiSettings.ApiKey}";
        var response = await _httpClient.PostAsync(requestUri, requestContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED);


        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var result = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return result ?? throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED);
    }


    public async Task DeleteFileAsync(string fileName)
    {
        var fileId = fileName.Replace("files/", "");

        var deleteUri = $"https://generativelanguage.googleapis.com/v1beta/files/{fileId}?key={_geminiSettings.ApiKey}";
        _httpClient.DefaultRequestHeaders.Authorization = null;

        var response = await _httpClient.DeleteAsync(deleteUri);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("ERROR:");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Reason: {response.ReasonPhrase}");
            Console.WriteLine($"Details: {errorContent}");
        }
        else
        {
            Console.WriteLine("File deleted successfully!");
        }
    }


    // {
    //     "file" : {
    //         "name" : "files/gujq7h01lsfd",
    //         "mimeType" : "application/pdf",
    //         "sizeBytes" : "1082273",
    //         "createTime" : "2025-08-05T18:39:46.157262Z",
    //         "updateTime" : "2025-08-05T18:39:46.157262Z",
    //         "expirationTime" : "2025-08-07T18:39:45.964007935Z",
    //         "sha256Hash" : "NzE1MzI5Zjg5ZDgwMmRiMTQ4ZWIwN2RjODVjMjk2YjA0MmMzZWFlNTAxYzM5YTkxODFkNjVlYWNhYTk2ZWViNw==",
    //         "uri" : "https://generativelanguage.googleapis.com/v1beta/files/gujq7h01lsfd",
    //         "state" : "ACTIVE",
    //         "source" : "UPLOADED"
    //     }
    // }


    public async Task<(string FileUri, string FileName)> UploadFileAsync(FileStreamData fileData, CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    
        var content = new StreamContent(fileData.Data!);

        var uploadUri = $"{_geminiSettings.UploadFileUri}?key={_geminiSettings.ApiKey}";

        var response = await _httpClient.PostAsync(uploadUri, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ErrorCodeException(ErrorCodes.FILE_UPLOAD_FAILED);
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseContent);

        var fileUri = doc.RootElement
            .GetProperty("file")
            .GetProperty("uri")
            .GetString();

        var fileName = doc.RootElement
            .GetProperty("file")
            .GetProperty("name")
            .GetString();

        if (string.IsNullOrEmpty(fileUri) || string.IsNullOrEmpty(fileName))
        {
            throw new ErrorCodeException(ErrorCodes.FILE_URI_NOTFOUND);
        }

        return (fileUri, fileName);
    }


    public async Task<int> CountToken(string text1, string text2, CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;


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
                            text = text2
                        },
                        new
                        {
                            text = text1
                        }
                    }
                }
            },
        };

        var json = JsonSerializer.Serialize(requestBody);
        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
        var requestUri =
            "https://aiplatform.googleapis.com/v1/projects/aiquizizz-ai-469816/locations/global/publishers/google/models/gemini-2.5-flash-lite:countTokens";
        var googleAccesstoken = await _googleAccessTokenProvider.GetAccessTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", googleAccesstoken);

        var response = await _httpClient.PostAsync(requestUri, requestContent, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new ErrorCodeException(ErrorCodes.COMMON_SERVER_INTERNAL_ERROR);
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);
        int totalTokens = doc.RootElement.GetProperty("totalTokens").GetInt32();
        return totalTokens;
    }

    public async Task<int> CountTokenWithFileAsync(string fileUri, string systemInstruction, string prompt,
        CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

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
                            file_data = new
                            {
                                file_uri = fileUri,
                                mime_type = "application/pdf"
                            }
                        },
                        new
                        {
                            text = prompt
                        },
                        new
                        {
                            text = systemInstruction
                        }
                    }
                }
            },
        };

        var json = JsonSerializer.Serialize(requestBody);
        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
        var requestUri =
            "https://aiplatform.googleapis.com/v1/projects/aiquizizz-ai-469816/locations/global/publishers/google/models/gemini-2.5-flash-lite:countTokens";
        var googleAccesstoken = await _googleAccessTokenProvider.GetAccessTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", googleAccesstoken);

        var response = await _httpClient.PostAsync(requestUri, requestContent, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ErrorCodeException(ErrorCodes.COMMON_SERVER_INTERNAL_ERROR);
        }

        using var doc = JsonDocument.Parse(responseJson);
        int totalTokens = doc.RootElement.GetProperty("totalTokens").GetInt32();
        return totalTokens;
    }


}
