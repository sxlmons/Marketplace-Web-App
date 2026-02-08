using Microsoft.AspNetCore.Mvc;
using MarketPlaceBackend.Data;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceBackend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PostController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly Logger _logger;
    private readonly string _imageStorage;

    public PostController(ApplicationDbContext db, Logger logger, IWebHostEnvironment env)
    {
        _db = db;
        _logger = logger;
        _imageStorage = Path.Combine(env.ContentRootPath, "ImageStorage");
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateNewPost([FromForm] Posts PostDTO, [FromForm] List<IFormFile>? images)
    {
        if (images.Count > 10)
            return BadRequest("Max 10 Images Allowed");

        PostDTO.PhotoCount = images.Count;
        _db.Posts.Add(PostDTO);

        await _db.SaveChangesAsync();

        var imageDir = Path.Combine(_imageStorage, PostDTO.UserId.ToString(), PostDTO.Id.ToString());
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

        _logger.LogEvent($"User {PostDTO.UserId} created Post {PostDTO.Id}");
        
        return Ok();
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
                    
                }
            ).ToListAsync();
        
        return Ok(posts);
    }

    [HttpGet]
    public IActionResult GetSingleThumbNail(int userId, int postId)
    {
        var thumbnailPath = Path.Combine(_imageStorage, userId.ToString(), postId.ToString());

        if (!Directory.Exists(thumbnailPath))
            return NotFound();

        var files = Directory.GetFiles(thumbnailPath)
            .Where(f => f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".png"))
            .OrderBy(f => f)
            .ToArray();
        
        if (files.Length == 0)
            return NotFound();

        var firstImagePath = files[0];
        
        var fileBytes = System.IO.File.ReadAllBytes(firstImagePath);
        
        var contentType = Path.GetExtension(firstImagePath).ToLower() switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
        
        return File(fileBytes, contentType);
    }

    [HttpGet]
    public IActionResult GetPhotoForPost(int userId, int postId, int imageId)
    {
        var imagePath = Path.Combine(_imageStorage, userId.ToString(), postId.ToString());
        
        if (!Directory.Exists(imagePath))
            return NotFound();

        var files = Directory.GetFiles(imagePath)
            .Where(f => f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".png") || f.ToLower().EndsWith(".jpeg"))
            .OrderBy(f => f)
            .ToArray();
        
        if (files.Length == 0)
            return NotFound();

        var firstImagePath = files[imageId - 1];
        
        var fileBytes = System.IO.File.ReadAllBytes(firstImagePath);
        
        var contentType = Path.GetExtension(firstImagePath).ToLower() switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
        
        return File(fileBytes, contentType);
    }

    // needs to be authenticated 
    [HttpDelete]
    public async Task<IActionResult> DeletePost(int postId)
    {
        var post = await _db.Posts.FindAsync(postId);

        if (post == null)
            return NotFound();
        
        // Authenticate 

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        
        var imageDir = Path.Combine(
            _imageStorage,
            post.UserId.ToString(),
            post.Id.ToString()
        );
        
        if (Directory.Exists(imageDir))
            Directory.Delete(imageDir, recursive: true);
        
        _logger.LogEvent($"User {post.UserId} deleted Post {post.Id}");

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePost(int postId, [FromForm] UpdatedPostDTO updatedPostDto, [FromForm] List<IFormFile>? images)
    {
        var post = await _db.Posts.FindAsync(postId);
        if (post == null)
            return NotFound();
        
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