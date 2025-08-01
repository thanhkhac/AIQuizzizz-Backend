using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Settings;
using CleanArchitectureBase.Application.Users.Common;
using CleanArchitectureBase.Domain.Constants;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitectureBase.Infrastructure.Identity;

public interface IGoogleAuthService
{
    Task<GoogleUserDto> ExchangeCodeForUserInfoAsync(string authorizationCode, string redirectUri);
}

internal class GoogleTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}

internal class GoogleUserInfoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool VerifiedEmail { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
}


public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleSettings _googleSettings;

    public GoogleAuthService(HttpClient httpClient, IOptions<GoogleSettings> googleSettings)
    {
        _httpClient = httpClient;
        _googleSettings = googleSettings.Value;
    }

    public async Task<GoogleUserDto> ExchangeCodeForUserInfoAsync(string authorizationCode, string redirectUri)
    {
        try
        {
            var tokenResponse = await ExchangeCodeForTokenAsync(authorizationCode, redirectUri);
            var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken);
            
            return userInfo;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to exchange authorization code", ex);
        }
    }

    private async Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string authorizationCode, string redirectUri)
    {
        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _googleSettings.ClientId),
            new KeyValuePair<string, string>("client_secret", _googleSettings.ClientSecret),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", redirectUri)
        });

        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to exchange authorization code for token");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();
        
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new Exception("Invalid token response from Google");
        }

        return tokenResponse;
    }

    private async Task<GoogleUserDto> GetUserInfoAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to get user info from Google");
        }

        var userInfo = await response.Content.ReadFromJsonAsync<GoogleUserInfoResponse>();
        
        if (userInfo == null)
        {
            throw new Exception("Failed to parse user info from Google");
        }

        return new GoogleUserDto
        {
            Id = userInfo.Id,
            Email = userInfo.Email,
            Name = userInfo.Name,
            GivenName = userInfo.GivenName,
            FamilyName = userInfo.FamilyName,
            Picture = userInfo.Picture,
            EmailVerified = userInfo.VerifiedEmail
        };
    }
}

