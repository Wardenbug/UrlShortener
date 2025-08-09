using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using UrlShortener.Api.Endpoints.DTOs;
using UrlShortener.IntegrationTests.Abstraction;

namespace UrlShortener.IntegrationTests;
public class CreateLinkTests : BaseTest
{
    private readonly HttpClient _httpClient;

    public CreateLinkTests(TestWebApplicationFactory webAppFactory)
    {
        _httpClient = webAppFactory.CreateClient();
    }

    [Fact]
    public async Task CreateLink_WithValidUrl_ShouldReturnShortUrl()
    {
        // Arrange
        var request = new CreateShortLinkRequest("https://www.google.com");

        // Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/shorten", request);

        // Assert
        response.EnsureSuccessStatusCode();
        ShortenResponse? result = await response.Content.ReadFromJsonAsync<ShortenResponse>();

        Assert.NotNull(result?.ShortUrl);
        Assert.Contains("http", result.ShortUrl, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(request.OriginalUrl, result.ShortUrl);
    }

    [Fact]
    public async Task CreateLink_WithInvalidUrl_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateShortLinkRequest("invalid_url");

        // Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/shorten", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string errorMessage = await response.Content.ReadAsStringAsync();

        Assert.Contains("Invalid URL format", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateLink_ForSameUrl_ShouldReturnDifferentValue()
    {
        // Arrange
        var request = new CreateShortLinkRequest("https://www.google.com");
        var request2 = new CreateShortLinkRequest("https://www.google.com");

        // Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/shorten", request);
        HttpResponseMessage response2 = await _httpClient.PostAsJsonAsync("/shorten", request2);

        ShortenResponse? result = await response.Content.ReadFromJsonAsync<ShortenResponse>();
        ShortenResponse? result2 = await response2.Content.ReadFromJsonAsync<ShortenResponse>();
        // Assert
        Assert.NotEqual(result?.ShortUrl, result2?.ShortUrl);
    }
}
