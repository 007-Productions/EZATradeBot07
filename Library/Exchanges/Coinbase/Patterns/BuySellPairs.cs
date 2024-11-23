using Coinbase.AdvancedTrade.Enums;
using Coinbase.AdvancedTrade.Models;
using Microsoft.Extensions.Logging;

namespace EZATB07.Library.Exchanges.Coinbase.Patterns;

//ToDo:kbdavis07:Need to eventually remove the ICoinbaseWrapper coinbaseWrapper from BuySellPairs class.
public class BuySellPairs(ICoinBaseService coinBaseService, ICoinbaseWrapper coinbaseWrapper, ILogger<BuySellPairs> logger)
{
    public async Task<bool> BuyLoopTillFundsRunOut(string productId, string baseSize, decimal startBuyMarkDownPercentage)
    {
        var account = await coinBaseService.ValidateAccounts(productId);

        if (account.Status == "Error") { return false; }

        var payingAccountBalance = decimal.Parse(account.OutstandingHoldAmount);
        var buyMarkDownPercentage = startBuyMarkDownPercentage;

        do
        {
            logger.LogInformation("buyMarkDownPercentage: {buyMarkDownPercentage}", buyMarkDownPercentage);

            var buyResult = await coinBaseService.Buy(productId, buyMarkDownPercentage, baseSize);

            if (buyResult.Status == "Error")
            {
                return false;
            }

            payingAccountBalance -= decimal.Parse(buyResult.OutstandingHoldAmount);
            buyMarkDownPercentage += 0.001m;

            logger.LogInformation("Remaining Balance: {payingAccountBalance}", payingAccountBalance);

        } while (payingAccountBalance > 0);

        return true;
    }
    public async Task MonitorAndTradeAsync(string productId, decimal buyPercentage, decimal sellPercentage)
    {
        while (true)
        {
            var bestBidPriceDecimal = await coinbaseWrapper.GetBestCurrentBidPrice(productId);


            // Calculate the buy price with the specified percentage
            var buyPrice = Math.Round(bestBidPriceDecimal * (1 - buyPercentage / 100), 6);

            // Place a buy order
            var buyOrder = await coinbaseWrapper.CreateLimitOrderAsync(productId, OrderSide.BUY, "1", buyPrice.ToString(), true);
            logger.LogInformation($"Placed buy order at {buyPrice}");

            // Wait for the buy order to be filled
            var filledBuyOrder = await WaitForOrderToBeFilledAsync(buyOrder.OrderId);
            if (filledBuyOrder == null)
            {
                logger.LogInformation("Buy order was not filled.");
                continue;
            }

            // Calculate the sell price with the specified percentage
            var buyTotalValueAfterFees = decimal.Parse(filledBuyOrder.TotalValueAfterFees);
            var sellPrice = Math.Round(buyTotalValueAfterFees * (1 + sellPercentage / 100), 6);

            // Place a sell order
            var sellOrder = await coinbaseWrapper.CreateLimitOrderAsync(productId, OrderSide.SELL, filledBuyOrder.FilledSize, sellPrice.ToString(), true);
            logger.LogInformation($"Placed sell order at {sellPrice}");

            // Wait for the sell order to be filled
            var filledSellOrder = await WaitForOrderToBeFilledAsync(sellOrder.OrderId);
            if (filledSellOrder == null)
            {
                logger.LogInformation("Sell order was not filled.");
                continue;
            }

            logger.LogInformation("Trade completed successfully.");
        }
    }
    private async Task<Order> WaitForOrderToBeFilledAsync(string orderId)
    {
        while (true)
        {
            var order = await coinbaseWrapper.GetOrderAsync(orderId);

            if (order.Status == "FILLED")
            {
                return order;
            }

            await Task.Delay(5000); // Wait for 5 seconds before checking again
        }
    }
}
