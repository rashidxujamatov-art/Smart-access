using System.Net;
using SmartAccess.Tests.Infrastructure;

namespace SmartAccess.Tests;

[Collection(DbCollection.Name)]
public sealed class HealthEndpointTests(PostgresFixture pg) : IDisposable
{
    private readonly ApiFactory _api = new(pg);

    [Fact]
    public async Task Health_returns_ok_with_database()
    {
        using var client = _api.CreateClient();

        var response = await client.GetAsync(new Uri("/api/v1/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    public void Dispose() => _api.Dispose();
}
