using Coinbase.AdvancedTrade.Models;

namespace EZATB07.Library.Exchanges.Coinbase;

public interface ICoinBaseService
{
    Task<Order> ValidateAccounts(string productId);
    Task<Order> Buy(string productId, decimal buyMarkDownPercentage, string baseSize);
}