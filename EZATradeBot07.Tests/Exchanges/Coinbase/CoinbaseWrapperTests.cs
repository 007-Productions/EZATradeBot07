using Xunit;
using EZATB07.Library.Exchanges.Coinbase;
using Coinbase.AdvancedTrade.Enums;
using WebSocketManager = Coinbase.AdvancedTrade.WebSocketManager;

namespace EZATB07.Library.Exchanges.Coinbase.Tests;
public class CoinbaseWrapperTests : IClassFixture<CoinbaseWrapperTestFixture>
{
    private readonly CoinbaseWrapperTestFixture _fixture;
    protected CoinbaseWrapper _coinbaseWrapper;
    protected WebSocketManager _webSocketManager;

    public CoinbaseWrapperTests(CoinbaseWrapperTestFixture fixture)
    {
        _fixture = fixture;
        _coinbaseWrapper = new CoinbaseWrapper(_fixture.ApiKey, _fixture.ApiSecret);
    }

    [Fact()]
    public async Task GetFilledBuyOrderToPlaceNewSellOrderPair()
    {
        var orders = await _coinbaseWrapper.GetAllOrders();

        var filledBuyOrder = orders.Where(o => o.Status == "FILLED" && o.Side == "BUY").Take(1).SingleOrDefault();

        // Ensure there is a filled buy order
        Assert.NotNull(filledBuyOrder);

        var orderInfo = await _coinbaseWrapper.GetOrderAsync(filledBuyOrder!.OrderId);

        // Ensure orderInfo is not null
        Assert.NotNull(orderInfo);

        var productId = orderInfo.ProductId;
        var side = OrderSide.SELL;
        var baseSize = orderInfo.FilledSize;
        var postOnly = true;

        // Calculate the new limit price by adding 1% to TotalValueAfterFees
        var limitPrice = Math.Round((decimal.Parse(orderInfo.AverageFilledPrice) * 1.01m), 6);

        // Get the best bid price
        var bestBidAsk = await _coinbaseWrapper.GetBestBidAskAsync(new List<string> { productId });
        var bestBidPrice = bestBidAsk.FirstOrDefault()?.Bids.FirstOrDefault()?.Price;

        // Ensure there is a best bid price
        Assert.NotNull(bestBidPrice);

        // Add 5% markup to the best bid price
        var bestBidPriceWithMarkup = Math.Round(decimal.Parse(bestBidPrice) * 1.05m, 6);

        // Use the best bid price with markup if the current limit price is less than the best bid price with markup
        if (limitPrice < bestBidPriceWithMarkup)
        {
            limitPrice = bestBidPriceWithMarkup;
        }

        // Convert limitPrice to string for the order preview and creation
        var limitPriceString = limitPrice.ToString();

        // Get the order preview
        var preview = await _coinbaseWrapper.GetOrderPreviewAsync(productId, side, baseSize, limitPriceString, postOnly);
        Assert.NotNull(preview);
        Assert.True(preview.IsValid, preview.Message);

        // Calculate the profit
        var buyTotalValueAfterFees = decimal.Parse(orderInfo.TotalValueAfterFees);
        var sellTotalValueAfterFees = preview.TotalPriceWithFee;
        var profit = sellTotalValueAfterFees - buyTotalValueAfterFees;

        Console.WriteLine($"Profit: {profit}");

        // 0.020538

        try
        {
            // Place the new sell order
            var result = await _coinbaseWrapper.CreateLimitOrderAsync(productId, side, baseSize, limitPriceString, postOnly);
            Assert.NotNull(result);
        }
        catch (Exception ex)
        {
            var error = ex.Message;
            throw;
        }

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
        var orderId = "77f3f8f2-f24b-460f-a531-e835a55fe38b";

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

        await _coinbaseWrapper.ConnectToWebSocket(new string[] { productId }, ChannelType.User, orderId);

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
