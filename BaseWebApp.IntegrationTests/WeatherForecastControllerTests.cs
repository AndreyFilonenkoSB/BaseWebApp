using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BaseWebApp.Api;
using BaseWebApp.Api.Contracts;
using FluentAssertions;
using Xunit;

namespace BaseWebApp.IntegrationTests;

public class WeatherForecastControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public WeatherForecastControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetWeatherForecast_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/weatherforecast");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWeatherForecast_WithValidToken_ReturnsSuccess()
    {
        var client = _factory.CreateClient();

        // ARRANGE
        var userEmail = $"testuser_{Guid.NewGuid()}@test.com";
        var userPassword = "Password123!";
        var registerRequest = new AuthRequest(userEmail, userPassword);
    
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // --- THIS IS THE NEW DEBUGGING CODE ---
        // Instead of immediately failing, let's check the status code and read the body if it's not a success.
        if (!registerResponse.IsSuccessStatusCode)
        {
            var errorContent = await registerResponse.Content.ReadAsStringAsync();
            // This will make the test fail, but show us the JSON error from our middleware.
            Assert.Fail($"Registration failed with status {registerResponse.StatusCode}. Response: {errorContent}");
        }
        // --- END DEBUGGING CODE ---
    
        var loginRequest = new AuthRequest(userEmail, userPassword);
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode(); // We can keep this one for now
    
        var tokenData = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var token = tokenData!.Token;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // ACT
        var response = await client.GetAsync("/weatherforecast");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var weatherData = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        weatherData.Should().HaveCount(5);
    }
    
    private record LoginResponse(string Token);
}