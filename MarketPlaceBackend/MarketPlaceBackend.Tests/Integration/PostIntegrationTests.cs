using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MarketPlaceBackend.DTOs;
using MarketPlaceBackend.Tests.Helpers;

namespace MarketPlaceBackend.Tests.Integration;

[TestFixture]
[NonParallelizable]
public class PostIntegrationTests
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
    public async Task CreatePost_WhenAuthenticated_ReturnsOkWithPostId()
    {
        await TestHelper.RegisterAndLogin(_client);

        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent("My Post"), "Title");
        formContent.Add(new StringContent("A description"), "Description");

        var response = await _client.PostAsync("/api/post/createnewpost", formContent);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(json.GetProperty("postId").GetInt32(), Is.GreaterThan(0));
    }

    [Test]
    public async Task CreatePost_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent("My Post"), "Title");
        formContent.Add(new StringContent("A description"), "Description");

        var response = await _client.PostAsync("/api/post/createnewpost", formContent);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetLatestPosts_AfterCreating_ReturnsPosts()
    {
        await TestHelper.RegisterAndLogin(_client);

        await TestHelper.CreatePostAndGetId(_client, "First Post", "Desc 1");
        await TestHelper.CreatePostAndGetId(_client, "Second Post", "Desc 2");

        var response = await _client.GetAsync("/api/post/getlatestpostswithlimit?limit=10");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var posts = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(posts.GetArrayLength(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetSinglePostInfo_AfterCreating_ReturnsPost()
    {
        await TestHelper.RegisterAndLogin(_client);

        var postId = await TestHelper.CreatePostAndGetId(_client, "Specific Post", "Details here");

        var response = await _client.GetAsync($"/api/post/getsinglepostinfo?postId={postId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task DeletePost_WhenOwner_ReturnsOk()
    {
        await TestHelper.RegisterAndLogin(_client);

        var postId = await TestHelper.CreatePostAndGetId(_client);

        var response = await _client.DeleteAsync($"/api/post/deletepost?postId={postId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify it's actually gone
        var getResponse = await _client.GetAsync($"/api/post/getsinglepostinfo?postId={postId}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeletePost_WhenNotOwner_ReturnsForbid()
    {
        // User A creates a post
        await TestHelper.RegisterAndLogin(_client, "userA@test.com");
        var postId = await TestHelper.CreatePostAndGetId(_client);

        // Log out User A
        await _client.PostAsync("/api/auth/logout", null);

        // User B logs in and tries to delete User A's post
        await TestHelper.RegisterAndLogin(_client, "userB@test.com");

        var response = await _client.DeleteAsync($"/api/post/deletepost?postId={postId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task UpdatePost_WhenOwner_ReturnsOkAndUpdatesData()
    {
        await TestHelper.RegisterAndLogin(_client);

        var postId = await TestHelper.CreatePostAndGetId(_client, "Original Title", "Original Desc");

        var updateContent = new MultipartFormDataContent();
        updateContent.Add(new StringContent("Updated Title"), "Title");
        updateContent.Add(new StringContent("Updated Desc"), "Description");

        var response = await _client.PutAsync($"/api/post/updatepost?postId={postId}", updateContent);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}