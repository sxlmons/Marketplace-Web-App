using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using MarketPlaceBackend.DTOs;

namespace MarketPlaceBackend.Tests.Helpers;

public static class TestHelper
{
    // UNIT TEST HELPERS

    // UserManager requires an IUserStore in its constructor.
    // We mock both the store and the manager itself so we can
    // control what methods like CreateAsync return.
    public static Mock<UserManager<IdentityUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        return new Mock<UserManager<IdentityUser>>(
            store.Object, null, null, null, null, null, null, null, null
        );
        // Those nulls are optional dependencies (password hasher,
        // validators, etc.) that UserManager accepts but doesn't
        // need for our mocking purposes.
    }

    // SignInManager requires UserManager + a bunch of HTTP-related
    // services. We satisfy the constructor but won't actually use
    // it in the Register test.
    public static Mock<SignInManager<IdentityUser>> MockSignInManager(
        Mock<UserManager<IdentityUser>> userManager)
    {
        return new Mock<SignInManager<IdentityUser>>(
            userManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object, null, null, null, null);
    }

    // [CALL THIS WHEN A METHOD REQUIRES AUTH]
    public static void SetUserClaims(ControllerBase controller, string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    public static void SetEmptyUserContext(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // INTEGRATION TEST HELPERS

    // Registers and logs in a user so the HttpClient
    // has an auth cookie for subsequent requests.
    public static async Task RegisterAndLogin(
        HttpClient client,
        string email = "test@test.com",
        string password = "ValidPass1!")
    {
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = email, Password = password });

        await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = password });
    }

    // Creates a post via multipart form data and returns
    // the postId from the JSON response. Requires the
    // client to already be authenticated.
    public static async Task<int> CreatePostAndGetId(
        HttpClient client,
        string title = "Test Post",
        string description = "Test Description")
    {
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(title), "Title");
        formContent.Add(new StringContent(description), "Description");

        var response = await client.PostAsync("/api/post/createnewpost", formContent);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("postId").GetInt32();
    }
}