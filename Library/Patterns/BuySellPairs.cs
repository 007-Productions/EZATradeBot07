using Coinbase.AdvancedTrade.Enums;
using Coinbase.AdvancedTrade.Models;
using EZATB07.Library.Exchanges.Coinbase;
using Microsoft.Extensions.Logging;

public class BuySellPairs(ICoinbaseWrapper coinbaseWrapper, ILogger<BuySellPairs> logger)
{
    public async Task<bool> BuyLoopTillFundsRunOut(string productId, string baseSize, decimal startBuyMarkDownPercentage)
    {
        var account = await ValidateAccounts(productId);

        if (account.Status == "Error") { return false; }

        var payingAccountBalance = decimal.Parse(account.OutstandingHoldAmount);
        var buyMarkDownPercentage = startBuyMarkDownPercentage;

        do
        {
            logger.LogInformation("buyMarkDownPercentage: {buyMarkDownPercentage}", buyMarkDownPercentage);

            var buyResult = await BuyAsync(productId, buyMarkDownPercentage, baseSize);

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



    public async Task<Order> BuyAsync(string productId, decimal buyMarkDownPercentage, string baseSize)
    {
        var account = await ValidateAccounts(productId);

        if (account.Status == "Error") { return account; }

        var payingAccountBalance = decimal.Parse(account.OutstandingHoldAmount);

        var lowestBuyOrderPrice = await coinbaseWrapper.GetLowestBuyOrderPrice(productId);
        var bestCurrentBidPrice = await coinbaseWrapper.GetBestCurrentBidPrice(productId);

        var newBuyOrderPriceWithMarkDown = Math.Round(Math.Min(lowestBuyOrderPrice, bestCurrentBidPrice) * (1 - buyMarkDownPercentage / 100), 6);

        if (newBuyOrderPriceWithMarkDown <= 0)
        {
            var errorMessage = "Calculated buy order price with markdown is less than or equal to zero!";
            logger.LogWarning(errorMessage);
            return new Order() { Status = "Error", RejectMessage = errorMessage };
        }

        var orderPreview = await coinbaseWrapper.GetOrderPreviewAsync(productId, OrderSide.BUY, baseSize, newBuyOrderPriceWithMarkDown.ToString(), true);

        if (orderPreview.TotalPriceWithFee >= payingAccountBalance)
        {
            var errorMessage = $"Order Price:{orderPreview.TotalPriceWithFee} is more than the Available Balance:{payingAccountBalance}!";
            logger.LogWarning(errorMessage);
            return new Order() { Status = "Error", RejectMessage = errorMessage };
        }

        try
        {
            var buyOrder = await coinbaseWrapper.CreateLimitOrderAsync(productId, OrderSide.BUY, baseSize, newBuyOrderPriceWithMarkDown.ToString(), true);
            logger.LogInformation("Placed Buy order at {UtcNow}: ProductId:{productId} BaseSize:{baseSize}  Total Order Price:{AverageFilledPrice}", DateTime.UtcNow, productId, baseSize, buyOrder.OutstandingHoldAmount);
            return buyOrder;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while placing a buy order.");
            return new Order() { Status = "Error", RejectMessage = ex.Message };
        }
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

    private async Task<Order> ValidateAccounts(string productId)
    {
        var currencies = productId.Split('-');

        if (currencies.Length != 2)
        {
            return new Order() { Status = "Error", RejectMessage = "Invalid productId format. Expected format: 'BUYING-PAYING'" };
        }

        var buyingCurrency = currencies[0];
        var payingCurrency = currencies[1];


        var payingAccount = (await coinbaseWrapper.ListAccounts()).FirstOrDefault(a => a.Currency == payingCurrency);

        if (payingAccount == null)
        {
            return new Order() { Status = "Error", RejectMessage = $"No account found for Paying Account:{payingCurrency}" };
        }

        var payingAccountBalance = decimal.Parse(payingAccount.AvailableBalance.Value);

        if (payingAccountBalance <= 0)
        {
            var errorMessage = $"No {payingCurrency} balance available!";
            logger.LogWarning(errorMessage);
            return new Order() { Status = "Error", RejectMessage = errorMessage };
        }

        var buyingAccount = (await coinbaseWrapper.ListAccounts()).FirstOrDefault(a => a.Currency == buyingCurrency);

        if (buyingAccount == null)
        {
            return new Order() { Status = "Error", RejectMessage = $"No account found for currency: {buyingCurrency}" };
        }

        logger.LogInformation($"Paying with Currency:{payingCurrency} Available Balance: {payingAccountBalance}");

        logger.LogInformation($"Buying Currency:{buyingAccount.Currency} Available Balance: {buyingAccount.AvailableBalance.Value}, Hold: {buyingAccount.Hold.Value}");

        return new Order() { Status = "Valid", OutstandingHoldAmount = payingAccount.AvailableBalance.Value };
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
