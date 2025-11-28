using Deed.Tests.Common.Mocks;

namespace Deed.Tests.Integration;

public class UnitTest1
{
    [Fact]
    public async Task CreateCapital_ShouldReturnId()
    {
        // Arrange
        var client = new TestClient();

        // Act
        await client.SendRequestAsync(HttpMethod.Post, "api/capitals");

        // Assert
    }
}
