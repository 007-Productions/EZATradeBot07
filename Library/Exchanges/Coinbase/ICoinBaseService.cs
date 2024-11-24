using Coinbase.AdvancedTrade.Models;
using EZATB07.Library.Exchanges.Coinbase.Models;

namespace EZATB07.Library.Exchanges.Coinbase;

public interface ICoinBaseService
{
    Task<ResultDTO> ValidateBuyPayAccounts(string productId);
    Task<ResultDTO<Order>> Buy(ResultDTO accounts, string productId, decimal buyMarkDownPercentage, string baseSize);
    ResultDTO CreateAccountErrorResult(string errorMessage, Exception? ex = null, bool? isRetryable = false);
    ResultDTO<Order> CreateOrderErrorResult(string? errorMessage, Exception? ex = null, bool? isRetryable = false);
}