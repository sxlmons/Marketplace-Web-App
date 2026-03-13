using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Tests.Helpers;

namespace MarketPlaceBackend.Tests.Integration;

[TestFixture]
[NonParallelizable]
public class CommentIntegrationTests
{
    private TestWebApplicationFactory _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task CreateComment_WhenAuthenticated_ReturnsNoContent()
    {
        await TestHelper.RegisterAndLogin(_client);
        var postId = await TestHelper.CreatePostAndGetId(_client);

        var response = await _client.PostAsJsonAsync("/api/comment/createnewcomment",
            new CommentDTO { PostId = postId, Content = "Great post!" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateComment_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/comment/createnewcomment",
            new CommentDTO { PostId = 1, Content = "Should fail" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetPostsComments_AfterCreating_ReturnsComments()
    {
        await TestHelper.RegisterAndLogin(_client);
        var postId = await TestHelper.CreatePostAndGetId(_client);

        await _client.PostAsJsonAsync("/api/comment/createnewcomment",
            new CommentDTO { PostId = postId, Content = "Comment 1" });

        await _client.PostAsJsonAsync("/api/comment/createnewcomment",
            new CommentDTO { PostId = postId, Content = "Comment 2" });

        var response = await _client.GetAsync($"/api/comment/getpostscomments?postId={postId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var comments = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(comments.GetArrayLength(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetPostsComments_WhenNoComments_ReturnsEmptyArray()
    {
        await TestHelper.RegisterAndLogin(_client);
        var postId = await TestHelper.CreatePostAndGetId(_client);

        var response = await _client.GetAsync($"/api/comment/getpostscomments?postId={postId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var comments = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(comments.GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task UpdateComment_WhenOwner_ReturnsOk()
    {
        await TestHelper.RegisterAndLogin(_client);
        var postId = await TestHelper.CreatePostAndGetId(_client);

        // Create a comment
        await _client.PostAsJsonAsync("/api/comment/createnewcomment",
            new CommentDTO { PostId = postId, Content = "Original" });

        // Get the comment ID from the list
        var getResponse = await _client.GetAsync($"/api/comment/getpostscomments?postId={postId}");
        var comments = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        var commentId = comments[0].GetProperty("id").GetInt32();

        // Update it
        var response = await _client.PutAsJsonAsync(
            $"/api/comment/updatecomment?commentId={commentId}",
            new UpdatedCommentDTOs { Content = "Updated content" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task DeleteComment_WhenOwner_ReturnsOk()
    {
        await TestHelper.RegisterAndLogin(_client);
        var postId = await TestHelper.CreatePostAndGetId(_client);

        // Create a comment
        await _client.PostAsJsonAsync("/api/comment/createnewcomment",
            new CommentDTO { PostId = postId, Content = "To delete" });

        // Get the comment ID
        var getResponse = await _client.GetAsync($"/api/comment/getpostscomments?postId={postId}");
        var comments = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        var commentId = comments[0].GetProperty("id").GetInt32();

        // Delete it
        var response = await _client.DeleteAsync($"/api/comment/deletecomment?commentId={commentId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify it's gone
        var afterDelete = await _client.GetAsync($"/api/comment/getpostscomments?postId={postId}");
        var remaining = await afterDelete.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(remaining.GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteComment_WhenNotOwner_ReturnsBadRequest()
    {
        // User A creates a post and a comment
        await TestHelper.RegisterAndLogin(_client, "userA@test.com");
        var postId = await TestHelper.CreatePostAndGetId(_client);

        await _client.PostAsJsonAsync("/api/comment/createnewcomment",
            new CommentDTO { PostId = postId, Content = "User A's comment" });

        // Get the comment ID
        var getResponse = await _client.GetAsync($"/api/comment/getpostscomments?postId={postId}");
        var comments = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        var commentId = comments[0].GetProperty("id").GetInt32();

        // Log out User A
        await _client.PostAsync("/api/auth/logout", null);

        // User B logs in and tries to delete User A's comment
        await TestHelper.RegisterAndLogin(_client, "userB@test.com");

        var response = await _client.DeleteAsync($"/api/comment/deletecomment?commentId={commentId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}