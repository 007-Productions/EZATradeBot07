using Coinbase.AdvancedTrade.Enums;
using Coinbase.AdvancedTrade.Models;

namespace EZATB07.Library.Exchanges.Coinbase;
public interface ICoinbaseWrapper
{
    Task<List<Account>> ListAccounts(int limit = 49, string? cursor = null);
    Task<decimal> GetLowestBuyOrderPrice(string productId);
    Task<decimal> GetBestCurrentBidPrice(string productId);
    Task<List<Order>> ListOrders(string? productId = null, OrderStatus[]? orderStatus = null, DateTime?
                                             startDate = null, DateTime? endDate = null,
                                             OrderType? orderType = null, OrderSide? orderSide = null);
    Task<Order> GetOrderAsync(string order_id);
    Task<List<ProductBook>> GetBestBidAskAsync(List<string> productIds);
    Task<List<Order>> GetAllOrders();
    Task ConnectToWebSocket(string[] products, ChannelType channelType, string orderId);
    Task<OrderPreview> GetOrderPreviewAsync(string productId, OrderSide side, string baseSize, string limitPrice, bool postOnly);
    Task<Order> CreateLimitOrderAsync(string productId, OrderSide side, string baseSize, string limitPrice, bool postOnly);
}