namespace Deed.Tests.Integration.Infrastructure;

[Collection("Integration")]
public abstract class IntegrationTest
{
    protected readonly HttpClient Client;

    protected IntegrationTest(DeedApiFactory factory)
    {
        Client = factory.CreateClient();
    }
}
