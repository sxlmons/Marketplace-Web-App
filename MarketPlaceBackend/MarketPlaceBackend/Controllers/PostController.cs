using MarketPlaceBackend.Data;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace MarketPlaceBackend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PostController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger _logger;
    private readonly string _imageStorage;

    public PostController(ApplicationDbContext db, ILogger logger, IWebHostEnvironment env)
    {
        _db = db;
        _logger = logger;
        _imageStorage = Path.Combine(env.ContentRootPath, "ImageStorage");
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateNewPost([FromForm] CreatePostDTO dto, [FromForm] List<IFormFile>? images)
    {
        // Grab userId from cookie
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return BadRequest("Cannot Validate User");

        if (images.Count > 5)
            return BadRequest("Max 5 Images Allowed");

        // Build the entity from the DTO + server-side data
        var post = new Posts
        {
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            PhotoCount = images?.Count ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        if (images != null && images.Count > 0)
        {
            var imageDir = Path.Combine(_imageStorage, userId, post.Id.ToString());
            Directory.CreateDirectory(imageDir);

            for (int i = 0; i < images.Count; i++)
            {
                var file = images[i];
                var extension = Path.GetExtension(file.FileName);
                var filePath = Path.Combine(imageDir, $"{i + 1}{extension}");

                await using var stream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true
                );

                await file.CopyToAsync(stream);
            }
        }

        _logger.LogEvent($"User {userId} created Post {post.Id}");

        return Ok(new { postId = post.Id });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetLatestPostsWithLimit(int limit)
    {
        var posts = await _db.Posts
            .OrderByDescending(p => p.Id)
            .Take(limit)
            .Select(p => new Posts
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Title = p.Title,
                    Description = p.Description,
                    PhotoCount = p.PhotoCount
                }
            ).ToListAsync();
        
        return Ok(posts);
    }
 
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeletePost(int postId)
    {
        // this grabs the userid of the http request
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var post = await _db.Posts.FindAsync(postId);
        if (post == null)
            return NotFound();

        // Check ownwership
        if (post.UserId != userId)
            return Forbid();

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        
        var imageDir = Path.Combine(
            _imageStorage,
            post.UserId,
            post.Id.ToString()
        );
        
        if (Directory.Exists(imageDir))
            Directory.Delete(imageDir, recursive: true);
        
        _logger.LogEvent($"User {post.UserId} deleted Post {post.Id}");

        return Ok();
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdatePost(int postId, [FromForm] UpdatedPostDTO updatedPostDto, [FromForm] List<IFormFile>? images)
    {
        // Grab userId from cookie
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var post = await _db.Posts.FindAsync(postId);
        if (post == null)
            return NotFound();

        // Ownership check
        if (post.UserId != userId)
            return Forbid();

        post.Title = updatedPostDto.Title;
        post.Description = updatedPostDto.Description;
        post.UpdatedAt = DateTime.UtcNow;
        
        if (images != null && images.Count > 0)
        {
            if (images.Count > 10)
                return BadRequest("Max 10 Images Allowed");

            var imageDir = Path.Combine(
                _imageStorage,
                post.UserId.ToString(),
                post.Id.ToString()
            );

            // Delete existing images
            if (Directory.Exists(imageDir))
                Directory.Delete(imageDir, recursive: true);

            Directory.CreateDirectory(imageDir);

            for (int i = 0; i < images.Count; i++)
            {
                var file = images[i];
                var extension = Path.GetExtension(file.FileName);
                var filePath = Path.Combine(imageDir, $"{i + 1}{extension}");

                await using var stream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true
                );

                await file.CopyToAsync(stream);
            }

            post.PhotoCount = images.Count;
        }

        await _db.SaveChangesAsync();

        _logger.LogEvent($"User {post.UserId} updated Post {post.Id}");

        return Ok();
    }
    
    [HttpGet]
    public IActionResult GetSinglePostInfo(int postId)
    {
        var post = _db.Posts.FirstOrDefault(p => p.Id == postId);

        if (post == null)
            return NotFound();

        return Ok(post);
    }
}