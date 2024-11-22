using System;
using System.Linq;
using System.Threading.Tasks;
using EZATB07.Library.Exchanges.Coinbase;
using Coinbase.AdvancedTrade.Enums;

public class BuySellPairs
{
    private readonly ICoinbaseWrapper _coinbaseWrapper;
    private readonly Logger _logger;

    public BuySellPairs(ICoinbaseWrapper coinbaseWrapper, Logger logger)
    {
        _coinbaseWrapper = coinbaseWrapper;
        _logger = logger;
    }

    public async Task MonitorAndTradeAsync(string productId, decimal buyPercentage, decimal sellPercentage)
    {
        while (true)
        {
            // Get the best bid price
            var bestBidAsk = await _coinbaseWrapper.GetBestBidAskAsync(new List<string> { productId });
            var bestBidPrice = bestBidAsk.FirstOrDefault()?.Bids.FirstOrDefault()?.Price;

            // Ensure there is a best bid price
            if (bestBidPrice == null)
            {
                _logger.Log("No best bid price available.");
                await Task.Delay(10000); // Wait for 10 seconds before retrying
                continue;
            }

            var bestBidPriceDecimal = decimal.Parse(bestBidPrice);

            // Calculate the buy price with the specified percentage
            var buyPrice = Math.Round(bestBidPriceDecimal * (1 - buyPercentage / 100), 6);

            // Place a buy order
            var buyOrder = await _coinbaseWrapper.CreateLimitOrderAsync(productId, OrderSide.BUY, "1", buyPrice.ToString(), true);
            _logger.Log($"Placed buy order at {buyPrice}");

            // Wait for the buy order to be filled
            var filledBuyOrder = await WaitForOrderToBeFilledAsync(buyOrder.OrderId);
            if (filledBuyOrder == null)
            {
                _logger.Log("Buy order was not filled.");
                continue;
            }

            // Calculate the sell price with the specified percentage
            var buyTotalValueAfterFees = decimal.Parse(filledBuyOrder.TotalValueAfterFees);
            var sellPrice = Math.Round(buyTotalValueAfterFees * (1 + sellPercentage / 100), 6);

            // Place a sell order
            var sellOrder = await _coinbaseWrapper.CreateLimitOrderAsync(productId, OrderSide.SELL, filledBuyOrder.FilledSize, sellPrice.ToString(), true);
            _logger.Log($"Placed sell order at {sellPrice}");

            // Wait for the sell order to be filled
            var filledSellOrder = await WaitForOrderToBeFilledAsync(sellOrder.OrderId);
            if (filledSellOrder == null)
            {
                _logger.Log("Sell order was not filled.");
                continue;
            }

            _logger.Log("Trade completed successfully.");
        }
    }

    private async Task<Order> WaitForOrderToBeFilledAsync(string orderId)
    {
        while (true)
        {
            var order = await _coinbaseWrapper.GetOrderAsync(orderId);
            if (order.Status == "FILLED")
            {
                return order;
            }

            await Task.Delay(5000); // Wait for 5 seconds before checking again
        }
    }
}
