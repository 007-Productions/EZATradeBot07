using Coinbase.AdvancedTrade.Enums;
using Coinbase.AdvancedTrade.Models;
using EZATB07.Library.Exchanges.Coinbase.Models;
using Microsoft.Extensions.Logging;

namespace EZATB07.Library.Exchanges.Coinbase;

public class CoinBaseService(ICoinbaseWrapper coinbaseWrapper, ILogger<CoinBaseService> logger) : ICoinBaseService
{
    public async Task<ResultDTO<Order>> Buy(ResultDTO accounts, string productId, decimal buyMarkDownPercentage, string baseSize)
    {
        var payingAccountBalance = decimal.Parse(accounts.PayingAccount.AvailableBalance.Value);

        var lowestBuyOrderPrice = await coinbaseWrapper.GetLowestBuyOrderPrice(productId);
        var bestCurrentBidPrice = await coinbaseWrapper.GetBestCurrentBidPrice(productId);

        var newBuyOrderPriceWithMarkDown = Math.Round(Math.Min(lowestBuyOrderPrice, bestCurrentBidPrice) * (1 - buyMarkDownPercentage / 100), 6);

        if (newBuyOrderPriceWithMarkDown <= 0)
        {
            return CreateOrderErrorResult("Calculated buy order price with markdown is less than or equal to zero!");
        }

        var orderPreview = await coinbaseWrapper.GetOrderPreviewAsync(productId, OrderSide.BUY, baseSize, newBuyOrderPriceWithMarkDown.ToString(), true);

        if (orderPreview.TotalPriceWithFee >= payingAccountBalance)
        {
            return CreateOrderErrorResult($"Order Price:{orderPreview.TotalPriceWithFee} is more than the Available Balance:{payingAccountBalance}!");
        }

        try
        {
            var buyOrder = await coinbaseWrapper.CreateLimitOrderAsync(productId, OrderSide.BUY, baseSize, newBuyOrderPriceWithMarkDown.ToString(), true);

            if(buyOrder.Status == "ERROR")
            {
                return CreateOrderErrorResult($"Failed to place buy order! Error:{buyOrder.RejectMessage}");
            }

            OrderSuccessResult(productId, baseSize, buyOrder);


            return new ResultDTO<Order>() { Data = buyOrder};
        }
        catch (Exception ex)
        {
           return CreateOrderErrorResult($"An error occurred while placing a buy order! Error:{ex.Message}", ex);
        }
    }
    
    public async Task<ResultDTO> ValidateBuyPayAccounts(string productId)
    {
        var currencies = productId.Split('-');

        if (currencies.Length != 2)
        {
            return CreateAccountErrorResult("Invalid productId format. Expected format: 'BUYING-PAYING'");
        }

        var buyingCurrency = currencies[0];
        var payingCurrency = currencies[1];

        var accounts = await coinbaseWrapper.ListAccounts();

        var payingAccount = accounts.FirstOrDefault(a => a.Currency == payingCurrency);
        var buyingAccount = accounts.FirstOrDefault(a => a.Currency == buyingCurrency);

        if (payingAccount == null)
        {
            return CreateAccountErrorResult($"No account found for Paying Account:{payingCurrency}");
        }

        var payingAccountBalance = decimal.Parse(payingAccount.AvailableBalance.Value);

        if (payingAccountBalance <= 0)
        {
            return CreateAccountErrorResult($"Type:Zero Balance Paying Account:{payingCurrency} Balance:{payingAccountBalance}!");
        }

        if (buyingAccount == null)
        {
            return CreateAccountErrorResult($"No account found for currency: {buyingCurrency}");
        }

        logger.LogInformation($"Paying with Currency:{payingCurrency} Available Balance: {payingAccountBalance}");
        logger.LogInformation($"Buying Currency:{buyingAccount.Currency} Available Balance: {buyingAccount.AvailableBalance.Value}, Hold: {buyingAccount.Hold.Value}");

        return new ResultDTO() { Status = Status.Valid, BuyingAccount = buyingAccount, PayingAccount = payingAccount };
    }
    
    public ResultDTO CreateAccountErrorResult(string errorMessage, Exception? ex = null, bool? isRetryable = false)
    {
        logger.LogError(ex, errorMessage);
        return new ResultDTO(errorMessage, isRetryable);
    }
    public ResultDTO<Order> CreateOrderErrorResult(string? errorMessage, Exception? ex = null, bool? isRetryable = false)
    {
        logger.LogError(ex, errorMessage);
        return new ResultDTO<Order> {  IsRetryable = isRetryable, Status = Status.Error, Message = errorMessage };
    }

    private void OrderSuccessResult(string productId, string baseSize, Order buyOrder)
    {
        logger.LogInformation("Placed Buy Order at {UtcNow} => OrderId:{OrderId} ProductId:{productId} BaseSize:{baseSize} Limit Price:{AverageFilledPrice}  Total Order Price:{OutstandingHoldAmount}", buyOrder.OrderId, DateTime.UtcNow, productId, baseSize, buyOrder.OrderConfiguration.LimitGtc.LimitPrice, buyOrder.OutstandingHoldAmount);
    }
}
