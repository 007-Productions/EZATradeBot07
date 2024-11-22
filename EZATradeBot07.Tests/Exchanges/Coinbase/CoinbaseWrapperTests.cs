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
    public async Task GetOrderPreview()
    {
        // Use _fixture.ApiKey and _fixture.ApiSecret in your test
        Assert.NotNull(_fixture.ApiKey);
        Assert.NotNull(_fixture.ApiSecret);

        var productId = "DEGEN-USDC";
        var side = OrderSide.BUY;
        var baseSize = "4";
        var limitPrice = "0.020027";
        var postOnly = true;

        var expectedTotalPrice = 0.080108m;
        var expectedFee = 0.000481m;
        var expectedTotalPriceWithFee = 0.080589m;

        // Get the order preview
        var preview = await _coinbaseWrapper.GetOrderPreviewAsync(productId, side, baseSize, limitPrice, postOnly);

        Assert.NotNull(preview);
        Assert.True(preview.IsValid, preview.Message);
        Assert.Equal(preview.TotalPrice, expectedTotalPrice);
        Assert.Equal(preview.Fee, expectedFee);
        Assert.Equal(preview.TotalPriceWithFee, expectedTotalPriceWithFee);
    }




    [Fact]
    public async Task CreateLimitOrderAsyncTestAsync()
    {
        // Use _fixture.ApiKey and _fixture.ApiSecret in your test
        Assert.NotNull(_fixture.ApiKey);
        Assert.NotNull(_fixture.ApiSecret);

        var productId = "DEGEN-USDC";
        var side = OrderSide.BUY;
        var baseSize = "1";
        var limitPrice = "0.019695";
        var postOnly = true;

        // Get the order preview
        var preview = await _coinbaseWrapper.GetOrderPreviewAsync(productId, side, baseSize, limitPrice, postOnly);
        Assert.NotNull(preview);
        Assert.True(preview.IsValid, preview.Message);

        // Your test implementation here
        var result = await _coinbaseWrapper.CreateLimitOrderAsync(productId, side, baseSize, limitPrice, postOnly);

        Assert.NotNull(result);
    }
}
