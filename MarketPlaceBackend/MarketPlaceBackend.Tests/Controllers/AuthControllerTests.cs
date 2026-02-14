using MarketPlaceBackend.Controllers;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace MarketPlaceBackend.Tests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    // These are the mocked dependencies that every test in this
    // class will share. Declared here, reset in SetUp.
    private Mock<UserManager<IdentityUser>> _mockUserManager;
    private Mock<SignInManager<IdentityUser>> _mockSignInManager;
    private Mock<ILogger> _mockLogger;
    private AuthController _controller;

    [SetUp]
    public void SetUp()
    {
        // This runs before EVERY [Test] method, giving each test
        // a clean set of mocks with no leftover state.
        _mockUserManager = TestHelper.MockUserManager();
        _mockSignInManager = TestHelper.MockSignInManager(_mockUserManager);
        _mockLogger = new Mock<ILogger>();

        _controller = new AuthController(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockLogger.Object
        );
    }

    // ==========================================
    //              REGISTER TESTS
    // ==========================================
    [Test]
    public async Task Register_WithValidCredentials_ReturnsOk()
    {
        // ARRANGE - Set up the scenario
        _mockUserManager
            .Setup(x => x.CreateAsync(
                It.IsAny<IdentityUser>(),
                It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "ValidPass1!"
        };

        // ACT - Call the method under test
        var result = await _controller.Register(request);

        // ASSERT - Check that we got an OkObjectResult (200 status).
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task Register_WhenCreateFails_ReturnsBadRequest()
    {
        // ARRANGE - This time, simulate Identity rejecting the registration (e.g., duplicate email).
        var identityErrors = IdentityResult.Failed(
            new IdentityError { Description = "Email already taken" }
        );

        _mockUserManager
            .Setup(x => x.CreateAsync(
                It.IsAny<IdentityUser>(),
                It.IsAny<string>()))
            .ReturnsAsync(identityErrors);

        var request = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = "ValidPass1!"
        };

        // ACT
        var result = await _controller.Register(request);

        // ASSERT
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Register_OnSuccess_LogsEvent()
    {
        // ARRANGE
        _mockUserManager
            .Setup(x => x.CreateAsync(
                It.IsAny<IdentityUser>(),
                It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "ValidPass1!"
        };

        // ACT
        await _controller.Register(request);

        // ASSERT - Verify that LogEvent was called exactly once with a string containing "registered successfully".
        _mockLogger.Verify(x => x.LogEvent(It.Is<string>(s => s.Contains("registered successfully"))), Times.Once);
    }

    [Test]
    public async Task Register_OnFailure_LogsFailedAttempt()
    {
        // TODO: Setup CreateAsync to return Failed
        // TODO: Call Register
        // TODO: Verify LogEvent was called once with "Failed registration"
    }

    // ==========================================
    //              LOGIN TESTS
    // ==========================================

    [Test]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(
                "test@test.com",
                "ValidPass1!",
                false,
                true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        _mockUserManager
            .Setup(x => x.FindByEmailAsync("test@test.com"))
            .ReturnsAsync(new IdentityUser { Id = "898324", Email = "test@test.com" });

        // Act
        var result = await _controller.Login(new LoginRequest
        {
            Email = "test@test.com",
            Password = "ValidPass1!"
        });

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // TODO: Setup PasswordSignInAsync to return Failed
        // TODO: Call Login
        // TODO: Assert result is BadRequestObjectResult
        // TODO: Assert message is generic "Invalid credentials" (not revealing which field was wrong)
    }

    [Test]
    public async Task Login_WhenAccountLockedOut_ReturnsBadRequestWithLockoutMessage()
    {
        // TODO: Setup PasswordSignInAsync to return Lockedout
        // TODO: Call Login
        // TODO: Assert result is BadRequestObjectResult
        // TODO: Assert message mentions account locked
    }

    [Test]
    public async Task Login_OnSuccess_LogsLoginEvent()
    {
        // TODO: Setup PasswordSignInAsync to return Success
        // TODO: Setup FindByEmailAsync to return a user
        // TODO: Call Login
        // TODO: Verify LogEvent was called with "logged in successfully"
    }

    [Test]
    public async Task Login_OnFailure_LogsFailedAttempt()
    {
        // TODO: Setup PasswordSignInAsync to return Failed
        // TODO: Call Login
        // TODO: Verify LogEvent was called with "Failed login attempt"
    }

    [Test]
    public async Task Login_OnLockout_LogsLockoutEvent()
    {
        // TODO: Setup PasswordSignInAsync to return Lockedout
        // TODO: Call Login
        // TODO: Verify LogEvent was called with "Account lockout triggered"
    }

    // ==========================================
    //              LOGOUT TESTS
    // ==========================================

    [Test]
    public async Task Logout_WhenCalled_ReturnsOk()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "898324");

        // Act
        var result = await _controller.Logout();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task Logout_WhenCalled_CallsSignOutAsync()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Call Logout
        // TODO: Verify SignOutAsync was called exactly once
    }

    [Test]
    public async Task Logout_WhenCalled_LogsLogoutEvent()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Call Logout
        // TODO: Verify LogEvent was called with "logged out"
    }

    // ==========================================
    //              ME TESTS
    // ==========================================

    [Test]
    public async Task Me_WhenUserExists_ReturnsOkWithUserInfo()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "898324");

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync(new IdentityUser { Id = "898324", Email = "test@test.com" });

        // Act
        var result = await _controller.Me();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        var okResult = result as OkObjectResult;
        var value = okResult.Value;

        // Use reflection to read the anonymous object properties
        var id = value.GetType().GetProperty("id").GetValue(value);
        var email = value.GetType().GetProperty("email").GetValue(value);

        Assert.That(id, Is.EqualTo("898324"));
        Assert.That(email, Is.EqualTo("test@test.com"));
    }

    [Test]
    public async Task Me_WhenUserNotFound_ReturnsNotFound()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Setup FindByIdAsync to return null
        // TODO: Call Me
        // TODO: Assert result is NotFoundResult
    }

    // ==========================================
    //           UPDATE EMAIL TESTS
    // ==========================================

    [Test]
    public async Task UpdateEmail_WithValidRequest_ReturnsOk()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "898324");

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync(new IdentityUser { Id = "898324", Email = "old@test.com", UserName = "old@test.com" });

        _mockUserManager
            .Setup(x => x.UpdateAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.UpdateEmail(new UpdateEmailRequest
        {
            NewEmail = "new@test.com"
        });

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateEmail_WhenUserIdNull_ReturnsUnauthorized()
    {
        // TODO: Setup controller with NO user claims (unauthenticated)
        // TODO: Call UpdateEmail
        // TODO: Assert result is UnauthorizedResult
    }

    [Test]
    public async Task UpdateEmail_WhenUserNotFound_ReturnsNotFound()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Setup FindByIdAsync to return null
        // TODO: Call UpdateEmail
        // TODO: Assert result is NotFoundObjectResult
    }

    [Test]
    public async Task UpdateEmail_WhenUpdateFails_ReturnsBadRequest()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Setup FindByIdAsync to return a user
        // TODO: Setup UpdateAsync to return Failed with errors
        // TODO: Call UpdateEmail
        // TODO: Assert result is BadRequestObjectResult
    }

    [Test]
    public async Task UpdateEmail_UpdatesBothEmailAndUserName()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Setup FindByIdAsync to return a user
        // TODO: Setup UpdateAsync to return Success
        // TODO: Call UpdateEmail with new email
        // TODO: Verify user.Email AND user.UserName were both set to new email
    }

    [Test]
    public async Task UpdateEmail_OnSuccess_LogsEmailChange()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Setup FindByIdAsync to return a user with old email
        // TODO: Setup UpdateAsync to return Success
        // TODO: Call UpdateEmail
        // TODO: Verify LogEvent was called with old and new email in message
    }

    [Test]
    public async Task UpdateEmail_OnFailure_LogsFailedAttempt()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Setup FindByIdAsync to return a user
        // TODO: Setup UpdateAsync to return Failed
        // TODO: Call UpdateEmail
        // TODO: Verify LogEvent was called with "Failed email update"
    }

    // ==========================================
    //         UPDATE PASSWORD TESTS
    // ==========================================

    [Test]
    public async Task UpdatePassword_WithValidRequest_ReturnsOk()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "898324");

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync(new IdentityUser { Id = "898324" });

        _mockUserManager
            .Setup(x => x.ChangePasswordAsync(
                It.IsAny<IdentityUser>(),
                "OldPassword1!",
                "NewPassword1!"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.UpdatePassword(new UpdatePasswordRequest
        {
            CurrentPassword = "OldPassword1!",
            NewPassword = "NewPassword1!"
        });

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdatePassword_WhenUserNotFound_ReturnsNotFound()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Setup FindByIdAsync to return null
        // TODO: Call UpdatePassword
        // TODO: Assert result is NotFoundResult
    }

    [Test]
    public async Task UpdatePassword_WhenWrongCurrentPassword_ReturnsBadRequest()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Setup FindByIdAsync to return a user
        // TODO: Setup ChangePasswordAsync to return Failed
        // TODO: Call UpdatePassword with wrong current password
        // TODO: Assert result is BadRequestObjectResult
    }

    [Test]
    public async Task UpdatePassword_OnSuccess_LogsPasswordUpdate()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Setup FindByIdAsync to return a user
        // TODO: Setup ChangePasswordAsync to return Success
        // TODO: Call UpdatePassword
        // TODO: Verify LogEvent was called with "updated their password"
    }

    [Test]
    public async Task UpdatePassword_OnFailure_LogsFailedAttempt()
    {
        // TODO: Setup controller with authenticated user claims
        // TODO: Setup FindByIdAsync to return a user
        // TODO: Setup ChangePasswordAsync to return Failed
        // TODO: Call UpdatePassword
        // TODO: Verify LogEvent was called with "Failed to update password"
    }


}