using Coinbase.AdvancedTrade.Enums;
using Coinbase.AdvancedTrade.Models;
using Microsoft.Extensions.Logging;

namespace EZATB07.Library.Exchanges.Coinbase;

public class CoinBaseService(ICoinbaseWrapper coinbaseWrapper, ILogger<CoinBaseService> logger) : ICoinBaseService
{

    public async Task<Order> Buy(string productId, decimal buyMarkDownPercentage, string baseSize)
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

    public async Task<Order> ValidateAccounts(string productId)
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
}
