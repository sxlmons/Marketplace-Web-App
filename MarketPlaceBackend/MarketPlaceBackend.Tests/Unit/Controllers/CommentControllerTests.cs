using System.Security.Claims;
using MarketPlaceBackend.Controllers;
using MarketPlaceBackend.Data;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace MarketPlaceBackend.Tests.Unit.Controllers;

public class CommentControllerTests
{
    private ApplicationDbContext _context;
    private CommentController _controller;
    private Mock<ILogger> _mockLogger;

    private void SetUser(string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    private Comments SeedComment(int id, string userId, int postId, string content)
    {
        var comment = new Comments
        {
            Id = id,
            UserId = userId,
            PostId = postId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        _context.SaveChanges();
        return comment;
    }

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger>();
        _controller = new CommentController(_context, _mockLogger.Object);
    }

    // =====================================================
    // CREATE COMMENT TESTS
    // =====================================================

    [Test]
    public async Task CreateNewComment_UserNull_ReturnsBadRequest()
    {
        var dto = new CommentDTO { PostId = 1, Content = "Test" };

        var result = await _controller.CreateNewComment(dto);

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        Assert.AreEqual(0, _context.Comments.Count());
    }

    [Test]
    public async Task CreateNewComment_SavesCorrectValues()
    {
        SetUser("user1");

        var dto = new CommentDTO { PostId = 5, Content = "Hello" };

        await _controller.CreateNewComment(dto);

        var saved = _context.Comments.First();

        Assert.AreEqual("user1", saved.UserId);
        Assert.AreEqual(5, saved.PostId);
        Assert.AreEqual("Hello", saved.Content);
    }

    [Test]
    public async Task CreateNewComment_AllowsMultipleCommentsSameUser()
    {
        SetUser("user1");

        await _controller.CreateNewComment(new CommentDTO { PostId = 1, Content = "A" });
        await _controller.CreateNewComment(new CommentDTO { PostId = 1, Content = "B" });

        Assert.AreEqual(2, _context.Comments.Count());
    }

    [Test]
    public async Task CreateNewComment_AllowsDifferentUsersSamePost()
    {
        SetUser("user1");
        await _controller.CreateNewComment(new CommentDTO { PostId = 1, Content = "A" });

        SetUser("user2");
        await _controller.CreateNewComment(new CommentDTO { PostId = 1, Content = "B" });

        Assert.AreEqual(2, _context.Comments.Count());
    }

    [Test]
    public async Task CreateNewComment_LargeContent_Succeeds()
    {
        SetUser("user1");

        var largeText = new string('X', 5000);

        await _controller.CreateNewComment(new CommentDTO
        {
            PostId = 1,
            Content = largeText
        });

        Assert.AreEqual(largeText, _context.Comments.First().Content);
    }

    [Test]
    public async Task CreateNewComment_LogsEvent()
    {
        SetUser("user1");

        await _controller.CreateNewComment(new CommentDTO
        {
            PostId = 1,
            Content = "Log Test"
        });

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    // =====================================================
    // GET COMMENTS TESTS
    // =====================================================

    [Test]
    public void GetPostsComments_ReturnsEmpty_WhenNoneExist()
    {
        var result = _controller.GetPostsComments(10);

        var ok = result as OkObjectResult;
        var list = ok.Value as List<Comments>;

        Assert.NotNull(list);
        Assert.AreEqual(0, list.Count);
    }

    [Test]
    public void GetPostsComments_FiltersByPostId()
    {
        SeedComment(1, "u1", 1, "A");
        SeedComment(2, "u2", 2, "B");

        var result = _controller.GetPostsComments(1);

        var list = ((OkObjectResult)result).Value as List<Comments>;

        Assert.AreEqual(1, list.Count);
        Assert.AreEqual("A", list.First().Content);
    }

    [Test]
    public void GetPostsComments_OrdersDescendingById()
    {
        SeedComment(1, "u1", 1, "First");
        SeedComment(2, "u1", 1, "Second");

        var result = _controller.GetPostsComments(1);

        var list = ((OkObjectResult)result).Value as List<Comments>;

        Assert.AreEqual(2, list.First().Id);
    }

    [Test]
    public void GetPostsComments_ReturnsSeparateInstances()
    {
        SeedComment(1, "u1", 1, "Test");

        var result = _controller.GetPostsComments(1);
        var list = ((OkObjectResult)result).Value as List<Comments>;

        list.First().Content = "Changed";

        var dbValue = _context.Comments.First().Content;

        Assert.AreNotEqual("Changed", dbValue);
    }

    // =====================================================
    // UPDATE COMMENT TESTS
    // =====================================================

    [Test]
    public void UpdateComment_UserNull_ReturnsBadRequest()
    {
        var result = _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "X" });

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public void UpdateComment_NotFound_ReturnsNotFound()
    {
        SetUser("user1");

        var result = _controller.UpdateComment(999, new UpdatedCommentDTOs { Content = "X" });

        Assert.IsInstanceOf<NotFoundResult>(result);
    }

    [Test]
    public void UpdateComment_UserMismatch_ReturnsBadRequest()
    {
        SeedComment(1, "owner", 1, "Old");

        SetUser("intruder");

        var result = _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "Hack" });

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public void UpdateComment_ChangesContentOnly()
    {
        var original = SeedComment(1, "owner", 1, "Old");

        SetUser("owner");

        _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "New" });

        var updated = _context.Comments.First();

        Assert.AreEqual("New", updated.Content);
        Assert.AreEqual(original.PostId, updated.PostId);
        Assert.AreEqual(original.UserId, updated.UserId);
    }

    [Test]
    public void UpdateComment_DoesNotCreateNewRow()
    {
        SeedComment(1, "owner", 1, "Old");

        SetUser("owner");

        _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "New" });

        Assert.AreEqual(1, _context.Comments.Count());
    }

    [Test]
    public void UpdateComment_CanUpdateMultipleTimes()
    {
        SeedComment(1, "owner", 1, "Old");

        SetUser("owner");

        _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "Mid" });
        _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "Final" });

        Assert.AreEqual("Final", _context.Comments.First().Content);
    }

    // =====================================================
    // DELETE COMMENT TESTS
    // =====================================================

    [Test]
    public async Task DeleteComment_UserNull_ReturnsBadRequest()
    {
        var result = await _controller.DeleteComment(1);

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task DeleteComment_NotFound_ReturnsNotFound()
    {
        SetUser("user1");

        var result = await _controller.DeleteComment(999);

        Assert.IsInstanceOf<NotFoundResult>(result);
    }

    [Test]
    public async Task DeleteComment_UserMismatch_ReturnsBadRequest()
    {
        SeedComment(1, "owner", 1, "Test");

        SetUser("intruder");

        var result = await _controller.DeleteComment(1);

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task DeleteComment_RemovesOnlyTarget()
    {
        SeedComment(1, "owner", 1, "Delete");
        SeedComment(2, "owner", 1, "Keep");

        SetUser("owner");

        await _controller.DeleteComment(1);

        Assert.AreEqual(1, _context.Comments.Count());
        Assert.AreEqual(2, _context.Comments.First().Id);
    }

    [Test]
    public async Task DeleteComment_DoubleDelete_ReturnsNotFoundSecondTime()
    {
        SeedComment(1, "owner", 1, "Delete");

        SetUser("owner");

        await _controller.DeleteComment(1);
        var second = await _controller.DeleteComment(1);

        Assert.IsInstanceOf<NotFoundResult>(second);
    }

    [Test]
    public async Task DeleteComment_LogsEvent()
    {
        SeedComment(1, "owner", 1, "Delete");

        SetUser("owner");

        await _controller.DeleteComment(1);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}