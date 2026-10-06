using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using Google.Apis.Auth.OAuth2;

namespace CleanArchitectureBase.Infrastructure.Google;


public class GoogleAccessTokenProvider : IGoogleAccessTokenProvider
{
    private readonly GoogleCredential? _credential;

    public GoogleAccessTokenProvider(string? credentialJson)
    {
        // Không có credential -> tắt tính năng AI thay vì crash khi resolve service
        if (string.IsNullOrWhiteSpace(credentialJson))
            return;

        credentialJson = credentialJson.Trim('\'');

        _credential = GoogleCredential
            .FromJson(credentialJson)
            .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (_credential == null)
            throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED, "AI feature is disabled: Google credentials are not configured.");

        return await _credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}
