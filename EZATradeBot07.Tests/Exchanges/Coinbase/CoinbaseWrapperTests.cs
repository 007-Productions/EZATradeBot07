using Coinbase.AdvancedTrade;
using Coinbase.AdvancedTrade.Enums;

namespace EZATB07.Library.Exchanges.Coinbase.Tests;
public class CoinbaseWrapperTests : IClassFixture<CoinbaseWrapperTestFixture>
{
    private readonly CoinbaseWrapperTestFixture _fixture;
    protected CoinbaseWrapper _coinbaseWrapper;
    protected WebSocketManager? _webSocketManager;

    public CoinbaseWrapperTests(CoinbaseWrapperTestFixture fixture)
    {
        _fixture = fixture;
        _coinbaseWrapper = new CoinbaseWrapper(_fixture.ApiKey, _fixture.ApiSecret);
    }

    [Fact]
    public async Task CreateLimitOrderAsyncTestAsync()
    {
        // Use _fixture.ApiKey and _fixture.ApiSecret in your test
        Assert.NotNull(_fixture.ApiKey);
        Assert.NotNull(_fixture.ApiSecret);

        // Your test implementation here
        var result = await _coinbaseWrapper.CreateLimitOrderAsync("DEGEN-USDC", OrderSide.BUY, "4", "0.020027", true);

        Assert.NotNull(result);
    }
}
