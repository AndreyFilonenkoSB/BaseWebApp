using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BaseWebApp.Api.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BaseWebApp.IntegrationTests;

// This class will contain all tests for the AuthController
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    // The constructor gets the WebApplicationFactory injected by xUnit
    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        // We create a client that can send requests to our in-memory API
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithNewUser_ReturnsOk()
    {
        // Arrange
        var uniqueEmail = $"testuser_{Guid.NewGuid()}@test.com";
        var request = new AuthRequest(uniqueEmail, "Password123!");

        // Act
        // Use the client to send a POST request to the register endpoint
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        // Check that the response has a success status code (200-299)
        response.EnsureSuccessStatusCode(); 
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var uniqueEmail = $"testuser_{Guid.NewGuid()}@test.com";
        var password = "Password123!";
        var registerRequest = new AuthRequest(uniqueEmail, password);
        
        // First, register a user so we can log in
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new AuthRequest(uniqueEmail, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // We can also check the response body
        var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
        responseData.Should().NotBeNull();
        responseData.Token.Should().NotBeNullOrEmpty();
    }
    
    // A simple record to help deserialize the login response
    private record LoginResponse(string Token);
}