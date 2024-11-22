using Coinbase.AdvancedTrade;
using Coinbase.AdvancedTrade.Enums;
using Coinbase.AdvancedTrade.Models;

namespace EZATB07.Library.Exchanges.Coinbase;

public class CoinbaseWrapper : ICoinbaseWrapper
{
    protected CoinbaseClient? _coinbaseClient;
    protected WebSocketManager? _webSocketManager;

    public CoinbaseWrapper(string CoinBase_Cloud_Trading_API_Key, string CoinBase_Cloud_Trading_API_Secret)
    {
        if (string.IsNullOrEmpty(CoinBase_Cloud_Trading_API_Key)) { throw new ArgumentNullException(nameof(CoinBase_Cloud_Trading_API_Key)); }
        if (string.IsNullOrEmpty(CoinBase_Cloud_Trading_API_Secret)) { throw new ArgumentNullException(nameof(CoinBase_Cloud_Trading_API_Secret)); }

        _coinbaseClient = new CoinbaseClient(CoinBase_Cloud_Trading_API_Key, CoinBase_Cloud_Trading_API_Secret);
        _webSocketManager = _coinbaseClient?.WebSocket;
    }

    // Create limit buy order and retrieve order details
    //Order order = await _coinbaseClient!.Orders.CreateLimitOrderGTCAsync(
    //    productId: "BTC-USDC",
    //    side: OrderSide.BUY,
    //    baseSize: "0.0001",
    //    limitPrice: "10000",
    //    postOnly: true,
    //    returnOrder: true
    //);

    /// <summary>
    /// Creates a limit order with good-till-canceled (GTC) duration and optionally returns the full order details.
    /// </summary>
    /// <param name="productId">Product ID for which the order is being placed.</param>
    /// <param name="side">Side of the order (buy/sell).</param>
    /// <param name="baseSize">Base size of the order.</param>
    /// <param name="limitPrice">Limit price for the order.</param>
    /// <param name="postOnly"> Indicates if the order should only be posted.</param>
    /// <returns>A task representing the operation. The task result contains the order object if returnOrder is true. </returns>
    public async Task<Order> CreateLimitOrderAsync(string productId, OrderSide side, string baseSize, string limitPrice, bool postOnly)
    {
        return await _coinbaseClient!.Orders.CreateLimitOrderGTCAsync(productId, side, baseSize, limitPrice, postOnly, true);
    }
}
