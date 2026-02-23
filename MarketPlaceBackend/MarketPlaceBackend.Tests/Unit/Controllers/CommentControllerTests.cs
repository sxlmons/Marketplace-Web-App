using MarketPlaceBackend.Controllers;
using MarketPlaceBackend.Data;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Models;
using MarketPlaceBackend.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MarketPlaceBackend.Tests.Unit.Controllers;

[TestFixture]
public class CommentControllerTests
{
    private ApplicationDbContext _context;
    private CommentController _controller;
    private Mock<ILogger> _mockLogger;

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
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger>();
        _controller = new CommentController(_context, _mockLogger.Object);

        // Default empty user context so User is never null.
        // Tests that need auth will call TestHelper.SetUserClaims.
        TestHelper.SetEmptyUserContext(_controller);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task CreateNewComment_UserNull_ReturnsBadRequest()
    {
        var dto = new CommentDTO { PostId = 1, Content = "Test" };

        var result = await _controller.CreateNewComment(dto);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        Assert.That(_context.Comments.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task CreateNewComment_SavesCorrectValues()
    {
        TestHelper.SetUserClaims(_controller, "user1");

        var dto = new CommentDTO { PostId = 5, Content = "Hello" };

        await _controller.CreateNewComment(dto);

        var saved = _context.Comments.First();

        Assert.That(saved.UserId, Is.EqualTo("user1"));
        Assert.That(saved.PostId, Is.EqualTo(5));
        Assert.That(saved.Content, Is.EqualTo("Hello"));
    }

    [Test]
    public async Task CreateNewComment_AllowsMultipleCommentsSameUser()
    {
        TestHelper.SetUserClaims(_controller, "user1");

        await _controller.CreateNewComment(new CommentDTO { PostId = 1, Content = "A" });
        await _controller.CreateNewComment(new CommentDTO { PostId = 1, Content = "B" });

        Assert.That(_context.Comments.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task CreateNewComment_AllowsDifferentUsersSamePost()
    {
        TestHelper.SetUserClaims(_controller, "user1");
        await _controller.CreateNewComment(new CommentDTO { PostId = 1, Content = "A" });

        TestHelper.SetUserClaims(_controller, "user2");
        await _controller.CreateNewComment(new CommentDTO { PostId = 1, Content = "B" });

        Assert.That(_context.Comments.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task CreateNewComment_LargeContent_Succeeds()
    {
        TestHelper.SetUserClaims(_controller, "user1");

        var largeText = new string('X', 5000);

        await _controller.CreateNewComment(new CommentDTO
        {
            PostId = 1,
            Content = largeText
        });

        Assert.That(_context.Comments.First().Content, Is.EqualTo(largeText));
    }

    [Test]
    public async Task CreateNewComment_LogsEvent()
    {
        TestHelper.SetUserClaims(_controller, "user1");

        await _controller.CreateNewComment(new CommentDTO
        {
            PostId = 1,
            Content = "Log Test"
        });

        _mockLogger.Verify(
            x => x.LogEvent(It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Test]
    public void GetPostsComments_ReturnsEmpty_WhenNoneExist()
    {
        var result = _controller.GetPostsComments(10);

        var ok = result as OkObjectResult;
        var list = ok.Value as List<Comments>;

        Assert.That(list, Is.Not.Null);
        Assert.That(list.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetPostsComments_FiltersByPostId()
    {
        SeedComment(1, "u1", 1, "A");
        SeedComment(2, "u2", 2, "B");

        var result = _controller.GetPostsComments(1);

        var list = ((OkObjectResult)result).Value as List<Comments>;

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list.First().Content, Is.EqualTo("A"));
    }

    [Test]
    public void GetPostsComments_OrdersDescendingById()
    {
        SeedComment(1, "u1", 1, "First");
        SeedComment(2, "u1", 1, "Second");

        var result = _controller.GetPostsComments(1);

        var list = ((OkObjectResult)result).Value as List<Comments>;

        Assert.That(list.First().Id, Is.EqualTo(2));
    }

    [Test]
    public void GetPostsComments_ReturnsSeparateInstances()
    {
        SeedComment(1, "u1", 1, "Test");

        var result = _controller.GetPostsComments(1);
        var list = ((OkObjectResult)result).Value as List<Comments>;

        list.First().Content = "Changed";

        var dbValue = _context.Comments.First().Content;

        Assert.That(dbValue, Is.Not.EqualTo("Changed"));
    }

    [Test]
    public void UpdateComment_UserNull_ReturnsBadRequest()
    {
        var result = _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "X" });

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void UpdateComment_NotFound_ReturnsNotFound()
    {
        TestHelper.SetUserClaims(_controller, "user1");

        var result = _controller.UpdateComment(999, new UpdatedCommentDTOs { Content = "X" });

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public void UpdateComment_UserMismatch_ReturnsBadRequest()
    {
        SeedComment(1, "owner", 1, "Old");

        TestHelper.SetUserClaims(_controller, "intruder");

        var result = _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "Hack" });

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void UpdateComment_ChangesContentOnly()
    {
        var original = SeedComment(1, "owner", 1, "Old");

        TestHelper.SetUserClaims(_controller, "owner");

        _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "New" });

        var updated = _context.Comments.First();

        Assert.That(updated.Content, Is.EqualTo("New"));
        Assert.That(updated.PostId, Is.EqualTo(original.PostId));
        Assert.That(updated.UserId, Is.EqualTo(original.UserId));
    }

    [Test]
    public void UpdateComment_DoesNotCreateNewRow()
    {
        SeedComment(1, "owner", 1, "Old");

        TestHelper.SetUserClaims(_controller, "owner");

        _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "New" });

        Assert.That(_context.Comments.Count(), Is.EqualTo(1));
    }

    [Test]
    public void UpdateComment_CanUpdateMultipleTimes()
    {
        SeedComment(1, "owner", 1, "Old");

        TestHelper.SetUserClaims(_controller, "owner");

        _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "Mid" });
        _controller.UpdateComment(1, new UpdatedCommentDTOs { Content = "Final" });

        Assert.That(_context.Comments.First().Content, Is.EqualTo("Final"));
    }

    [Test]
    public async Task DeleteComment_UserNull_ReturnsBadRequest()
    {
        var result = await _controller.DeleteComment(1);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteComment_NotFound_ReturnsNotFound()
    {
        TestHelper.SetUserClaims(_controller, "user1");

        var result = await _controller.DeleteComment(999);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteComment_UserMismatch_ReturnsBadRequest()
    {
        SeedComment(1, "owner", 1, "Test");

        TestHelper.SetUserClaims(_controller, "intruder");

        var result = await _controller.DeleteComment(1);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteComment_RemovesOnlyTarget()
    {
        SeedComment(1, "owner", 1, "Delete");
        SeedComment(2, "owner", 1, "Keep");

        TestHelper.SetUserClaims(_controller, "owner");

        await _controller.DeleteComment(1);

        Assert.That(_context.Comments.Count(), Is.EqualTo(1));
        Assert.That(_context.Comments.First().Id, Is.EqualTo(2));
    }

    [Test]
    public async Task DeleteComment_DoubleDelete_ReturnsNotFoundSecondTime()
    {
        SeedComment(1, "owner", 1, "Delete");

        TestHelper.SetUserClaims(_controller, "owner");

        await _controller.DeleteComment(1);
        var second = await _controller.DeleteComment(1);

        Assert.That(second, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteComment_LogsEvent()
    {
        SeedComment(1, "owner", 1, "Delete");

        TestHelper.SetUserClaims(_controller, "owner");

        await _controller.DeleteComment(1);

        _mockLogger.Verify(
            x => x.LogEvent(It.IsAny<string>()),
            Times.AtLeastOnce);
    }
}