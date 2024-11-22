using Coinbase.AdvancedTrade.Enums;
using Coinbase.AdvancedTrade.Models;

namespace EZATB07.Library.Exchanges.Coinbase;
public interface ICoinbaseWrapper
{
    Task ConnectToWebSocket(string[] products, ChannelType channelType, string orderId);
    Task<OrderPreview> GetOrderPreviewAsync(string productId, OrderSide side, string baseSize, string limitPrice, bool postOnly);

    /// <summary>
    /// Creates a limit order with good-till-canceled (GTC) duration and optionally returns the full order details.
    /// </summary>
    /// <param name="productId">Product ID for which the order is being placed.</param>
    /// <param name="side">Side of the order (buy/sell).</param>
    /// <param name="baseSize">Base size of the order.</param>
    /// <param name="limitPrice">Limit price for the order.</param>
    /// <param name="postOnly"> Indicates if the order should only be posted.</param>
    /// <returns>A task representing the operation. The task result contains the order object if returnOrder is true. </returns>
    Task<Order> CreateLimitOrderAsync(string productId, OrderSide side, string baseSize, string limitPrice, bool postOnly);
}