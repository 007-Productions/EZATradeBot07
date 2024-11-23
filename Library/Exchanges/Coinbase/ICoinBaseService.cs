using Coinbase.AdvancedTrade.Models;
using EZATB07.Library.Exchanges.Coinbase.Models;

namespace EZATB07.Library.Exchanges.Coinbase;

public interface ICoinBaseService
{
    Task<ResultDTO<Order>> Buy(string productId, decimal buyMarkDownPercentage, string baseSize);
}