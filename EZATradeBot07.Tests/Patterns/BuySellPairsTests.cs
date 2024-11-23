using Xunit;
using Coinbase.AdvancedTrade.Enums;
using EZATB07.Library.Exchanges.Coinbase;
using EZATB07.Library.Exchanges.Coinbase.Tests;
using Microsoft.Extensions.Logging;

public class BuySellPairsTests : IClassFixture<CoinbaseWrapperTestFixture>
{
    private readonly ILogger<BuySellPairs> _logger;
    private readonly CoinbaseWrapperTestFixture _fixture;
    private readonly ICoinbaseWrapper _coinbaseWrapper;
    private readonly BuySellPairs _buySellPairs;

    public BuySellPairsTests(CoinbaseWrapperTestFixture fixture)
    {
        // Configure logger to log to the console
        using var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole();});
        
        _logger = loggerFactory.CreateLogger<BuySellPairs>();

        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _coinbaseWrapper = new CoinbaseWrapper(_fixture.ApiKey, _fixture.ApiSecret); 
        _buySellPairs = new BuySellPairs(_coinbaseWrapper, _logger);
    }

    [Fact()]
    public async Task BuyLoopTillFundsRunOutTestAsync()
    {
        var result = await _buySellPairs.BuyLoopTillFundsRunOut("DEGEN-USDC", "5", 0.5m);
    }

    [Fact]
    public async Task BuyAsync_ShouldPlaceBuyOrderSuccessfully()
    {
        // Arrange
        var productId = "DEGEN-USDC";
        var buyMarkDownPercentage = 0.5m;
        var baseSize = "5";

        // Act
        var result = await _buySellPairs.BuyAsync(productId, buyMarkDownPercentage, baseSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OrderSide.BUY.ToString(), result.Side);
        Assert.Equal(productId, result.ProductId);
    }

    [Fact]
    public async Task BuyAsync_ShouldLogError_WhenExceptionIsThrown()
    {
        // Arrange
        var productId = "INVALID-PRODUCT";
        var buyMarkDownPercentage = 5m;
        var baseSize = "0.01";

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _buySellPairs.BuyAsync(productId, buyMarkDownPercentage, baseSize));
    }
}
