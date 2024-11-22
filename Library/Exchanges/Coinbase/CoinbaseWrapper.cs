using Coinbase.AdvancedTrade;
using Coinbase.AdvancedTrade.Enums;
using Coinbase.AdvancedTrade.Models;
using Coinbase.AdvancedTrade.Models.WebSocket;
using EZATB07.Library.Exchanges.Coinbase.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;

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

    public async Task ConnectToWebSocket(string[] products, ChannelType channelType, string orderId)
    {
        if (_webSocketManager == null)
        {
            throw new InvalidOperationException("WebSocketManager is not initialized.");
        }

        await _webSocketManager.ConnectAsync();

        await _webSocketManager.SubscribeAsync(products, channelType);

        _webSocketManager!.UserMessageReceived += (sender, userData) =>
        {
            Console.WriteLine($"Received User data at {DateTime.UtcNow}");
        };

        _webSocketManager.MessageReceived += (sender, e) =>
        {
            Console.WriteLine($"Raw message received at {DateTime.UtcNow}: {e.StringData}");

            var webSocketModel = JsonConvert.DeserializeObject<WebSocketModel>(e.StringData);

            if (webSocketModel != null)
            {
                foreach (var evt in webSocketModel.events)
                {
                    foreach (var order in evt.orders)
                    {
                        if (order.order_id == orderId)
                        {
                            // Handle the order update
                            Console.WriteLine($"Order {order.order_id} update received.");
                        }
                    }
                }
            }
        };
    }


    public async Task<OrderPreview> GetOrderPreviewAsync(string productId, OrderSide side, string baseSize, string limitPrice, bool postOnly)
    {
        // Calculate the total price
        decimal baseSizeDecimal = decimal.Parse(baseSize);
        decimal limitPriceDecimal = decimal.Parse(limitPrice);
        decimal totalPrice = baseSizeDecimal * limitPriceDecimal;

        // Determine the fee percentage
        decimal feePercentage = postOnly ? 0.006m : 0.012m;
        decimal fee = Math.Round(totalPrice * feePercentage, 6);
        decimal totalPriceWithFee = totalPrice + fee;

        // Simulate validation logic
        bool isValid = true;
        string message = "Order preview is valid.";

        // Add your validation logic here
        if (totalPrice <= 0)
        {
            isValid = false;
            message = "Invalid total price.";
        }

        // Return the order preview
        return await Task.FromResult(new OrderPreview
        {
            ProductId = productId,
            Side = side,
            BaseSize = baseSize,
            LimitPrice = limitPrice,
            PostOnly = postOnly,
            TotalPrice = totalPrice,
            Fee = fee,
            TotalPriceWithFee = totalPriceWithFee,
            IsValid = isValid,
            Message = message
        });
    }

    public async Task<Order> CreateLimitOrderAsync(string productId, OrderSide side, string baseSize, string limitPrice, bool postOnly)
    {
        return await _coinbaseClient!.Orders.CreateLimitOrderGTCAsync(productId, side, baseSize, limitPrice, postOnly, true);
    }

    //public void SubscribeToOrderUpdates(string orderId, Action<Order> onOrderUpdate)
    //{
    //    _coinbaseClient

    //    if (_webSocketManager == null)
    //    {
    //        throw new InvalidOperationException("WebSocketManager is not initialized.");
    //    }

    //    _webSocketManager.On

    //   _webSocketManager += (sender, order) =>
    //    {
    //        if (order.OrderId == orderId)
    //        {
    //            onOrderUpdate(order);
    //        }
    //    };

    //    _webSocketManager.Connect();
    //}

}

public class OrderPreview
{
    public string ProductId { get; set; }
    public OrderSide Side { get; set; }
    public string BaseSize { get; set; }
    public string LimitPrice { get; set; }
    public bool PostOnly { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Fee { get; set; }
    public decimal TotalPriceWithFee { get; set; }
    public bool IsValid { get; set; }
    public string Message { get; set; }
}