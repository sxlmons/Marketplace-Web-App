using MarketPlaceBackend.Controllers;
using MarketPlaceBackend.Data;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Models;
using MarketPlaceBackend.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace MarketPlaceBackend.Tests.Unit.Controllers;

[TestFixture]
public class PostControllerTests
{
    // Dependencies shared across all tests, reset in SetUp.
    private ApplicationDbContext _db;
    private Mock<ILogger> _mockLogger;
    private Mock<IWebHostEnvironment> _mockEnv;
    private PostController _controller;

    [SetUp]
    public void SetUp()
    {
        // Each test gets its own in-memory database so tests
        // never leak state into each other. The unique name
        // (via Guid) guarantees a fresh DB every time.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger>();

        // PostController uses env.ContentRootPath to build the
        // image storage path. We point it at a temp directory
        // so file operations don't fail or hit real paths.
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

        _controller = new PostController(
            _db,
            _mockLogger.Object,
            _mockEnv.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    private static IFormFile CreateMockFile(string fileName = "test.jpg")
    {
        var content = "fake image content"u8.ToArray();
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "images", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    [Test]
    public async Task CreateNewPost_WithValidData_ReturnsOkWithPostId()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        var dto = new CreatePostDTO
        {
            Title = "Test Post",
            Description = "A test description"
        };
        var images = new List<IFormFile>(); // no images

        // Act
        var result = await _controller.CreateNewPost(dto, images);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        // Verify the post was actually saved to the database
        var savedPost = await _db.Posts.FirstOrDefaultAsync();
        Assert.That(savedPost, Is.Not.Null);
        Assert.That(savedPost.Title, Is.EqualTo("Test Post"));
        Assert.That(savedPost.UserId, Is.EqualTo("user-123"));
    }

    [Test]
    public async Task CreateNewPost_WhenUserIdNull_ReturnsBadRequest()
    {
        // In test setup for this case:
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // no claims at all

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
        
        // Arrange - no claims set, so userId will be null
        var dto = new CreatePostDTO
        {
            Title = "Test",
            Description = "Desc"
        };

        // Act
        var result = await _controller.CreateNewPost(dto, new List<IFormFile>());

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateNewPost_WithMoreThan5Images_ReturnsBadRequest()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        var dto = new CreatePostDTO
        {
            Title = "Test",
            Description = "Desc"
        };

        // Create 6 mock files to exceed the limit
        var images = Enumerable.Range(1, 6)
            .Select(i => CreateMockFile($"image{i}.jpg"))
            .ToList();

        // Act
        var result = await _controller.CreateNewPost(dto, images);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateNewPost_OnSuccess_LogsCreationEvent()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        var dto = new CreatePostDTO
        {
            Title = "Log Test",
            Description = "Desc"
        };

        // Act
        await _controller.CreateNewPost(dto, new List<IFormFile>());

        // Assert
        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("user-123") &&
                s.Contains("created Post"))),
            Times.Once);
    }

    [Test]
    public async Task CreateNewPost_WithImages_SetsPhotoCount()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        var dto = new CreatePostDTO
        {
            Title = "With Images",
            Description = "Has 3 images"
        };

        var images = Enumerable.Range(1, 3)
            .Select(i => CreateMockFile($"img{i}.jpg"))
            .ToList();

        // Act
        await _controller.CreateNewPost(dto, images);

        // Assert
        var savedPost = await _db.Posts.FirstOrDefaultAsync();
        Assert.That(savedPost.PhotoCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetLatestPostsWithLimit_ReturnsOkWithPosts()
    {
        // Arrange - seed a few posts into the in-memory DB
        _db.Posts.AddRange(
            new Posts { UserId = "u1", Title = "Post 1", Description = "Desc 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Posts { UserId = "u2", Title = "Post 2", Description = "Desc 2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Posts { UserId = "u3", Title = "Post 3", Description = "Desc 3", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await _db.SaveChangesAsync();

        // Act
        var result = await _controller.GetLatestPostsWithLimit(2);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        var okResult = result as OkObjectResult;
        var posts = okResult.Value as List<Posts>;
        Assert.That(posts.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetLatestPostsWithLimit_ReturnsPostsInDescendingOrder()
    {
        // Arrange
        _db.Posts.AddRange(
            new Posts { UserId = "u1", Title = "First", Description = "D", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Posts { UserId = "u1", Title = "Second", Description = "D", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Posts { UserId = "u1", Title = "Third", Description = "D", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await _db.SaveChangesAsync();

        // Act
        var result = await _controller.GetLatestPostsWithLimit(3);

        // Assert
        var okResult = result as OkObjectResult;
        var posts = okResult.Value as List<Posts>;

        // The post with the highest Id should come first
        Assert.That(posts[0].Id, Is.GreaterThan(posts[1].Id));
        Assert.That(posts[1].Id, Is.GreaterThan(posts[2].Id));
    }

    [Test]
    public async Task GetLatestPostsWithLimit_WhenNoPosts_ReturnsEmptyList()
    {
        // Act - empty database
        var result = await _controller.GetLatestPostsWithLimit(5);

        // Assert
        var okResult = result as OkObjectResult;
        var posts = okResult.Value as List<Posts>;
        Assert.That(posts, Is.Empty);
    }

    [Test]
    public async Task GetSinglePostInfo_WhenPostExists_ReturnsOkWithPost()
    {
        // Arrange
        var post = new Posts
        {
            UserId = "user-123",
            Title = "My Post",
            Description = "Details",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        // Act
        var result = _controller.GetSinglePostInfo(post.Id);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        var okResult = result as OkObjectResult;
        var returnedPost = okResult.Value as Posts;
        Assert.That(returnedPost.Title, Is.EqualTo("My Post"));
    }

    [Test]
    public void GetSinglePostInfo_WhenPostNotFound_ReturnsNotFound()
    {
        // Act - no posts in DB, ask for id 999
        var result = _controller.GetSinglePostInfo(999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DeletePost_WhenOwnerDeletes_ReturnsOk()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        var post = new Posts
        {
            UserId = "user-123",
            Title = "To Delete",
            Description = "D",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        // Act
        var result = await _controller.DeletePost(post.Id);

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());

        // Verify it's actually gone from the DB
        var deleted = await _db.Posts.FindAsync(post.Id);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task DeletePost_WhenPostNotFound_ReturnsNotFound()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        // Act
        var result = await _controller.DeletePost(999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DeletePost_WhenNotOwner_ReturnsForbid()
    {
        // Arrange - post belongs to "owner-456"
        var post = new Posts
        {
            UserId = "owner-456",
            Title = "Not Mine",
            Description = "D",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        // But the authenticated user is "user-123"
        TestHelper.SetUserClaims(_controller, "user-123");

        // Act
        var result = await _controller.DeletePost(post.Id);

        // Assert
        Assert.That(result, Is.InstanceOf<ForbidResult>());

        // Verify the post was NOT deleted
        var stillExists = await _db.Posts.FindAsync(post.Id);
        Assert.That(stillExists, Is.Not.Null);
    }

    [Test]
    public async Task DeletePost_OnSuccess_LogsDeletionEvent()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        var post = new Posts
        {
            UserId = "user-123",
            Title = "Log Delete",
            Description = "D",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        // Act
        await _controller.DeletePost(post.Id);

        // Assert
        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("user-123") &&
                s.Contains("deleted Post"))),
            Times.Once);
    }

    [Test]
    public async Task UpdatePost_WithValidData_ReturnsOk()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        var post = new Posts
        {
            UserId = "user-123",
            Title = "Original Title",
            Description = "Original Desc",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        var updatedDto = new UpdatedPostDTO
        {
            Title = "Updated Title",
            Description = "Updated Desc"
        };

        // Act
        var result = await _controller.UpdatePost(post.Id, updatedDto, null);

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());

        // Verify the post was updated in the DB
        var updatedPost = await _db.Posts.FindAsync(post.Id);
        Assert.That(updatedPost.Title, Is.EqualTo("Updated Title"));
        Assert.That(updatedPost.Description, Is.EqualTo("Updated Desc"));
    }

    [Test]
    public async Task UpdatePost_WhenPostNotFound_ReturnsNotFound()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        var dto = new UpdatedPostDTO
        {
            Title = "Whatever",
            Description = "Whatever"
        };

        // Act
        var result = await _controller.UpdatePost(999, dto, null);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdatePost_WhenNotOwner_ReturnsForbid()
    {
        // Arrange - post belongs to someone else
        var post = new Posts
        {
            UserId = "owner-456",
            Title = "Not Mine",
            Description = "D",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        TestHelper.SetUserClaims(_controller, "user-123");

        var dto = new UpdatedPostDTO
        {
            Title = "Hacked",
            Description = "Hacked"
        };

        // Act
        var result = await _controller.UpdatePost(post.Id, dto, null);

        // Assert
        Assert.That(result, Is.InstanceOf<ForbidResult>());

        // Verify original data was NOT changed
        var unchanged = await _db.Posts.FindAsync(post.Id);
        Assert.That(unchanged.Title, Is.EqualTo("Not Mine"));
    }

    [Test]
    public async Task UpdatePost_WithMoreThan10Images_ReturnsBadRequest()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        var post = new Posts
        {
            UserId = "user-123",
            Title = "Has Images",
            Description = "D",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        var dto = new UpdatedPostDTO
        {
            Title = "Updated",
            Description = "Updated"
        };

        // 11 images exceeds the max of 10
        var images = Enumerable.Range(1, 11)
            .Select(i => CreateMockFile($"img{i}.jpg"))
            .ToList();

        // Act
        var result = await _controller.UpdatePost(post.Id, dto, images);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdatePost_OnSuccess_LogsUpdateEvent()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        var post = new Posts
        {
            UserId = "user-123",
            Title = "Original",
            Description = "D",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        var dto = new UpdatedPostDTO
        {
            Title = "Updated",
            Description = "Updated"
        };

        // Act
        await _controller.UpdatePost(post.Id, dto, null);

        // Assert
        _mockLogger.Verify(
            x => x.LogEvent(It.Is<string>(s =>
                s.Contains("user-123") &&
                s.Contains("updated Post"))),
            Times.Once);
    }

    [Test]
    public async Task UpdatePost_SetsUpdatedAtTimestamp()
    {
        // Arrange
        TestHelper.SetUserClaims(_controller, "user-123");

        var originalTime = DateTime.UtcNow.AddHours(-1);
        var post = new Posts
        {
            UserId = "user-123",
            Title = "Original",
            Description = "D",
            CreatedAt = originalTime,
            UpdatedAt = originalTime
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        var dto = new UpdatedPostDTO
        {
            Title = "Updated",
            Description = "Updated"
        };

        // Act
        await _controller.UpdatePost(post.Id, dto, null);

        // Assert - UpdatedAt should be newer than the original
        var updatedPost = await _db.Posts.FindAsync(post.Id);
        Assert.That(updatedPost.UpdatedAt, Is.GreaterThan(originalTime));
    }
}