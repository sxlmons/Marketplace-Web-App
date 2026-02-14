using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;

namespace MarketPlaceBackend.Tests.Helpers;

public static class TestHelper
{
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
}