namespace UrlShortener.IntegrationTests.Abstraction;
public abstract class BaseTest : IClassFixture<TestWebApplicationFactory>
{
    protected BaseTest()
    {
    }
}
