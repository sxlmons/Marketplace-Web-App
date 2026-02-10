using Microsoft.AspNetCore.Mvc;
using MarketPlaceBackend.Data;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceBackend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ImageController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly Logger _logger;
    private readonly string _imageStorage;

    public ImageController(ApplicationDbContext db, Logger logger, IWebHostEnvironment env)
    {
        _db = db;
        _logger = logger;
        _imageStorage = Path.Combine(env.ContentRootPath, "ImageStorage");
    }
    
    [HttpGet]
    public IActionResult GetSingleThumbNail(int postId)
    {
        var post = _db.Posts.FirstOrDefault(p => p.Id == postId);

        if (post == null)
            return NotFound();
        
        var thumbnailPath = Path.Combine(_imageStorage, post.UserId, postId.ToString());

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
    public IActionResult GetPhotoForPost(int postId, int imageId)
    {
        var post = _db.Posts.FirstOrDefault(p => p.Id == postId);

        if (post == null)
            return NotFound();

        if (imageId < 1)
            return BadRequest("Invalid ImageId");
        
        var imagePath = Path.Combine(_imageStorage, post.UserId, postId.ToString());
        
        if (!Directory.Exists(imagePath))
            return NotFound();

        var files = Directory.GetFiles(imagePath)
            .Where(f => f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".png") || f.ToLower().EndsWith(".jpeg"))
            .OrderBy(f => f)
            .ToArray();
        
        if (files.Length == 0 || files.Length < imageId)
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
}