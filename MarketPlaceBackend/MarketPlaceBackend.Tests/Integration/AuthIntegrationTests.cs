using System.Net;
using System.Net.Http.Json;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Tests.Helpers;

namespace MarketPlaceBackend.Tests.Integration;

[TestFixture]
public class AuthIntegrationTests
{
    private TestWebApplicationFactory _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }
    
    [Test]
    public async Task Register_ThenLogin_Succeeds()
    {
        var email = $"{Guid.NewGuid()}@test.com";

        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = email, Password = "ValidPass1!" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = "ValidPass1!" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task Login_ThenMe_ReturnsUserInfo()
    {
        // Register
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = "test@test.com", Password = "ValidPass1!" });

        // Login
        await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "test@test.com", Password = "ValidPass1!" });

        // Me
        var response = await _client.GetAsync("/api/auth/me");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Login_ThenLogout_ThenMe_ReturnsUnauthorized()
    {
        // Register
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = "test@test.com", Password = "ValidPass1!" });

        // Login
        await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "test@test.com", Password = "ValidPass1!" });

        // Logout
        await _client.PostAsync("/api/auth/logout", null);

        // Me
        var response = await _client.GetAsync("/api/auth/me");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_ThenUpdateEmail_Succeeds()
    {
        // Register
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = "test@test.com", Password = "ValidPass1!" });

        // Login
        await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "test@test.com", Password = "ValidPass1!" });

        // Update email
        var response = await _client.PatchAsJsonAsync("/api/auth/updateemail",
            new UpdateEmailRequest { NewEmail = "updated@test.com" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Login_ThenUpdatePassword_Succeeds()
    {
        // Register
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = "test@test.com", Password = "ValidPass1!" });

        // Login
        await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "test@test.com", Password = "ValidPass1!" });

        // Update password
        var response = await _client.PatchAsJsonAsync("/api/auth/updatepassword",
            new UpdatePasswordRequest { CurrentPassword = "ValidPass1!", NewPassword = "NewPass1!" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

}
