using MarketPlaceBackend.Controllers;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MarketPlaceBackend.Tests.Unit.Controllers;

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

        await _controller.Register(request);

        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("Failed registration attempt"))),
            Times.Once);
    }

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
        _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                false,
                true))
            .ReturnsAsync(SignInResult.Failed);

        var result = await _controller.Login(new LoginRequest
        {
            Email = "test@test.com",
            Password = "WrongPassword"
        });

        var badRequest = result as BadRequestObjectResult;

        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest.Value.ToString(), Does.Contain("Invalid credentials"));
    }


    [Test]
    public async Task Login_WhenAccountLockedOut_ReturnsBadRequestWithLockoutMessage()
    {
         _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                false,
                true))
            .ReturnsAsync(SignInResult.LockedOut);

        var result = await _controller.Login(new LoginRequest
        {
            Email = "test@test.com",
            Password = "WrongPassword"
        });

        var badRequest = result as BadRequestObjectResult;

        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest.Value.ToString(), Does.Contain("Account locked"));
    }

    [Test]
    public async Task Login_OnSuccess_LogsLoginEvent()
    {
         _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                false,
                true))
            .ReturnsAsync(SignInResult.Success);

         _mockUserManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new IdentityUser { Id = "898324" });

        await _controller.Login(new LoginRequest
        {
            Email = "test@test.com",
            Password = "ValidPass1!"
        });

        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("logged in successfully"))),
            Times.Once);
    }

    [Test]
    public async Task Login_OnFailure_LogsFailedAttempt()
    {
        _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                false,
                true))
            .ReturnsAsync(SignInResult.Failed);

        await _controller.Login(new LoginRequest
        {
            Email = "test@test.com",
            Password = "WrongPassword"
        });

        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("Failed login attempt"))),
            Times.Once);
    }

    [Test]
    public async Task Login_OnLockout_LogsLockoutEvent()
    {
         _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                false,
                true))
            .ReturnsAsync(SignInResult.LockedOut);

        await _controller.Login(new LoginRequest
        {
            Email = "test@test.com",
            Password = "WrongPassword"
        });

        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("Account lockout triggered"))),
            Times.Once);
    }

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
        TestHelper.SetUserClaims(_controller, "898324");

        await _controller.Logout();

        _mockSignInManager.Verify(
            x => x.SignOutAsync(),
            Times.Once);
    }

    [Test]
    public async Task Logout_WhenCalled_LogsLogoutEvent()
    {
        TestHelper.SetUserClaims(_controller, "898324");

        await _controller.Logout();

        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("logged out"))),
            Times.Once);
    }

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
        TestHelper.SetUserClaims(_controller, "898324");

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync((IdentityUser)null);

        var result = await _controller.Me();

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

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
        TestHelper.SetEmptyUserContext(_controller);
            
        var result = await _controller.UpdateEmail(new UpdateEmailRequest
        {
            NewEmail = "new@test.com"
        });

        Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test]
    public async Task UpdateEmail_WhenUserNotFound_ReturnsNotFound()
    {
        TestHelper.SetUserClaims(_controller, "898324");

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync((IdentityUser)null);

        var result = await _controller.UpdateEmail(new UpdateEmailRequest
        {
            NewEmail = "new@test.com"
        });

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task UpdateEmail_WhenUpdateFails_ReturnsBadRequest()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "898324");

        var user = new IdentityUser
        {
            Id = "898324",
            Email = "old@test.com",
            UserName = "old@test.com"
        };

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync(user);

        var failedResult = IdentityResult.Failed(
            new IdentityError { Description = "Email invalid" }
        );

        _mockUserManager
            .Setup(x => x.UpdateAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(failedResult);

        // Act
        var result = await _controller.UpdateEmail(new UpdateEmailRequest
        {
            NewEmail = "bademail"
        });

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateEmail_UpdatesBothEmailAndUserName()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "898324");

        var user = new IdentityUser
        {
            Id = "898324",
            Email = "old@test.com",
            UserName = "old@test.com"
        };

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync(user);

        _mockUserManager
            .Setup(x => x.UpdateAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _controller.UpdateEmail(new UpdateEmailRequest
        {
            NewEmail = "new@test.com"
        });

        // Assert
        Assert.That(user.Email, Is.EqualTo("new@test.com"));
        Assert.That(user.UserName, Is.EqualTo("new@test.com"));
    }

    [Test]
    public async Task UpdateEmail_OnSuccess_LogsEmailChange()
    {
       // Arrange
        TestHelper.SetUserClaims(_controller, "898324");

        var user = new IdentityUser
        {
            Id = "898324",
            Email = "old@test.com",
            UserName = "old@test.com"
        };

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync(user);

        _mockUserManager
            .Setup(x => x.UpdateAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _controller.UpdateEmail(new UpdateEmailRequest
        {
            NewEmail = "new@test.com"
        });

        // Assert
        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("old@test.com") &&
                s.Contains("new@test.com"))),
            Times.Once);
    }

    [Test]
    public async Task UpdateEmail_OnFailure_LogsFailedAttempt()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "898324");

        var user = new IdentityUser
        {
            Id = "898324",
            Email = "old@test.com"
        };

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync(user);

        var failedResult = IdentityResult.Failed(
            new IdentityError { Description = "Update failed" }
        );

        _mockUserManager
            .Setup(x => x.UpdateAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(failedResult);

        // Act
        await _controller.UpdateEmail(new UpdateEmailRequest
        {
            NewEmail = "new@test.com"
        });

        // Assert
        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("Failed email update"))),
            Times.Once);
    }


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
        TestHelper.SetUserClaims(_controller, "898324");

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync((IdentityUser)null);

        var result = await _controller.UpdatePassword(new UpdatePasswordRequest
        {
            CurrentPassword = "Old",
            NewPassword = "New"
        });

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdatePassword_WhenWrongCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "898324");

        var user = new IdentityUser { Id = "898324" };

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync(user);

        var failedResult = IdentityResult.Failed(
            new IdentityError { Description = "Incorrect password" }
        );

        _mockUserManager
            .Setup(x => x.ChangePasswordAsync(
                user,
                "WrongPassword",
                "NewPassword1!"))
            .ReturnsAsync(failedResult);

        // Act
        var result = await _controller.UpdatePassword(new UpdatePasswordRequest
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword1!"
        });

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdatePassword_OnSuccess_LogsPasswordUpdate()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "898324");

        var user = new IdentityUser { Id = "898324" };

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync(user);

        _mockUserManager
            .Setup(x => x.ChangePasswordAsync(
                user,
                "OldPassword1!",
                "NewPassword1!"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _controller.UpdatePassword(new UpdatePasswordRequest
        {
            CurrentPassword = "OldPassword1!",
            NewPassword = "NewPassword1!"
        });

        // Assert
        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("updated their password"))),
            Times.Once);
    }

    [Test]
    public async Task UpdatePassword_OnFailure_LogsFailedAttempt()
    {
         // Arrange
        TestHelper.SetUserClaims(_controller, "898324");

        var user = new IdentityUser { Id = "898324" };

        _mockUserManager
            .Setup(x => x.FindByIdAsync("898324"))
            .ReturnsAsync(user);

        var failedResult = IdentityResult.Failed(
            new IdentityError { Description = "Incorrect password" }
        );

        _mockUserManager
            .Setup(x => x.ChangePasswordAsync(
                user,
                "WrongPassword",
                "NewPassword1!"))
            .ReturnsAsync(failedResult);

        // Act
        await _controller.UpdatePassword(new UpdatePasswordRequest
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword1!"
        });

        // Assert
        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("Failed to update password"))),
            Times.Once);
    }


}