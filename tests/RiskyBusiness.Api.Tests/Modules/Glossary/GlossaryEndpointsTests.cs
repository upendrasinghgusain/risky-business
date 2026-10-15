using System.Net;
using System.Net.Http.Json;
using RiskyBusiness.Api.Modules.Glossary;

namespace RiskyBusiness.Api.Tests.Modules.Glossary;

public sealed class GlossaryEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Ready_check_is_healthy_when_database_is_reachable()
    {
        var response = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Created_term_can_be_read_back_and_is_listed()
    {
        var termName = UniqueTerm("Binder");

        var create = await _client.PostAsJsonAsync("/api/glossary",
            new CreateGlossaryTermRequest(termName, "Authority delegated by an insurer to a coverholder."));

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<GlossaryTermResponse>();
        Assert.NotNull(created);
        Assert.Equal($"/api/glossary/{created.Id}", create.Headers.Location?.OriginalString);

        var fetched = await _client.GetFromJsonAsync<GlossaryTermResponse>(create.Headers.Location);
        Assert.Equal(created, fetched);

        var listed = await _client.GetFromJsonAsync<GlossaryTermResponse[]>("/api/glossary");
        Assert.Contains(listed!, t => t.Id == created.Id);
    }

    [Fact]
    public async Task Duplicate_term_is_rejected_regardless_of_case()
    {
        var termName = UniqueTerm("Premium");
        await _client.PostAsJsonAsync("/api/glossary", new CreateGlossaryTermRequest(termName, "First definition."));

        var duplicate = await _client.PostAsJsonAsync("/api/glossary",
            new CreateGlossaryTermRequest($"  {termName.ToLowerInvariant()} ", "Second definition."));

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Theory]
    [InlineData("", "A definition.")]
    [InlineData("   ", "A definition.")]
    [InlineData("Term", "")]
    public async Task Missing_term_or_definition_is_rejected(string term, string definition)
    {
        var response = await _client.PostAsJsonAsync("/api/glossary", new CreateGlossaryTermRequest(term, definition));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_term_returns_not_found()
    {
        var response = await _client.GetAsync($"/api/glossary/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Tests share one database, so each uses its own term name.
    private static string UniqueTerm(string prefix) => $"{prefix} {Guid.NewGuid():N}";
}
