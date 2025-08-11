using CleanArchitectureBase.Application.Common.Interfaces;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace CleanArchitectureBase.Infrastructure.Google;


public class GoogleAccessTokenProvider : IGoogleAccessTokenProvider
{
    private readonly GoogleCredential _credential;

    public GoogleAccessTokenProvider(string credentialJson)
    {
        if (string.IsNullOrEmpty(credentialJson))
            throw new ArgumentNullException(nameof(credentialJson), "Credential JSON is required");

        credentialJson = credentialJson.Trim('\'');

        _credential = GoogleCredential
            .FromJson(credentialJson)
            .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
    }

    public async Task<string> GetAccessTokenAsync()
    {
        return await _credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}

