using MarketPlaceBackend.Data;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceBackend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class CommentController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly Logger _logger;
    
    public CommentController(ApplicationDbContext db, Logger logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewComment(CommentDTO commentDto)
    {
        Comments comment = new Comments()
        {
            PostId = commentDto.PostId,
            UserId = commentDto.UserId,
            Content = commentDto.Content,
            CreatedAt = DateTime.UtcNow
        };
        
        await _db.Comments.AddAsync(comment);
        await _db.SaveChangesAsync();

        _logger.LogEvent($"User {commentDto.UserId} commented on Post {commentDto.PostId}");
        
        return NoContent();
    }

    [HttpGet]
    public IActionResult GetPostsComments(int postId)
    {
        var comments = _db.Comments
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.Id)
            .Select(c => new Comments
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    PostId = c.PostId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }
            ).ToList();

        return Ok(comments);
    }

    [HttpPut]
    public IActionResult UpdateComment(int commentId, UpdatedCommentDTOs commentDto)
    {
        var comment = _db.Comments
            .FirstOrDefault(c => c.Id == commentId);

        if (comment == null)
            return NotFound();

        comment.Content = commentDto.Content;

        _db.SaveChanges();
        
        _logger.LogEvent($"User {comment.UserId} updated Post {comment.Id}");

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var comment = await _db.Comments.FindAsync(commentId);

        if (comment == null)
            return NotFound();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
        
        _logger.LogEvent($"User {comment.UserId} deleted Comment {comment.Id}");

        return Ok();
    }
}









