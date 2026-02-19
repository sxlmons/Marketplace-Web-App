using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using MarketPlaceBackend.DTOs;

namespace MarketPlaceBackend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    
    private readonly ILogger _logger;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager, 
        ILogger logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound();

        return Ok(new
        {
            id = user.Id,
            email = user.Email
        });
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogEvent($"User {user.Id} registered successfully with email {user.Email}");
            return Ok(new { message = "Registration successful" });
        }

        _logger.LogEvent($"Failed registration attempt for email {request.Email}");
        return BadRequest(result.Errors);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(
            request.Email, 
            request.Password, 
            isPersistent: false, 
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            _logger.LogEvent($"User {user.Id} logged in successfully");
            return Ok(new { message = "Login successful" });
        }

        if (result.IsLockedOut)
        {
            _logger.LogEvent($"Account lockout triggered for email {request.Email}");
            return BadRequest(new { message = "Account locked. Try again later." });
        }

        _logger.LogEvent($"Failed login attempt for email {request.Email}");
        return BadRequest(new { message = "Invalid credentials" });
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _signInManager.SignOutAsync();

        _logger.LogEvent($"User {userId} logged out");
        return Ok(new { message = "Logged out" });
    }

    [HttpPatch]
    [Authorize]
    public async Task<IActionResult> UpdateEmail(UpdateEmailRequest request)
    {
        // grab current user id from auth cookie
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        // fetch user from db
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        // update property
        var oldEmail = user.Email;
        user.Email = request.NewEmail;
        user.UserName = request.NewEmail; // email is same as usr name

        // Save to db
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            _logger.LogEvent($"User {userId} updated email from {oldEmail} to {request.NewEmail}");
            return Ok(new { message = "Email updated." });
        }

        _logger.LogEvent($"Failed email update attempt for user {userId}");
        return BadRequest(result.Errors);
    }

    [HttpPatch]
    [Authorize]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound();

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword
        );

        if (result.Succeeded)
        {
            _logger.LogEvent($"User {userId} updated their password");
            return Ok(new { message = "Password updated successfully" });
        }

        _logger.LogEvent($"Failed to update password for {userId}");
        return BadRequest(result.Errors);
    }
}