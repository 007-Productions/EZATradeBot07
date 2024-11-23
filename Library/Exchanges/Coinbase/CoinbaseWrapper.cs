using Coinbase.AdvancedTrade;
using Coinbase.AdvancedTrade.Enums;
using Coinbase.AdvancedTrade.Models;
using EZATB07.Library.Exchanges.Coinbase.Models;
using Newtonsoft.Json;

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

    public Task<List<Account>> ListAccounts(int limit = 49, string? cursor = null) => _coinbaseClient!.Accounts.ListAccountsAsync(limit, cursor);
   
    public Task<List<Order>> ListOrders(string? productId = null, OrderStatus[]? orderStatus = null, DateTime? 
                                             startDate = null, DateTime? endDate = null, 
                                             OrderType? orderType = null, OrderSide? orderSide = null) => 
        _coinbaseClient!.Orders.ListOrdersAsync(productId, orderStatus, startDate, endDate, orderType, orderSide);

    public async Task<decimal> GetLowestBuyOrderPrice(string productId)
    {
        var orders = await _coinbaseClient!.Orders!.ListOrdersAsync(productId: productId, orderSide: OrderSide.BUY);
        var relevantOrders = orders.Where(o => o.Status == "FILLED" || o.Status == "OPEN");

        if (!relevantOrders.Any())
        {
            Console.WriteLine("No relevant orders found.");
            return 0m;
        }

        var lowestOrder = relevantOrders.MinBy(o => decimal.Parse(o.OrderConfiguration.LimitGtc.LimitPrice));

        if (lowestOrder == null || !decimal.TryParse(lowestOrder.OrderConfiguration.LimitGtc.LimitPrice, out var result))
        {
            Console.WriteLine("Failed to parse the lowest order price.");
            return 0m;
        }

        return result;
    }

    public async Task<decimal> GetBestCurrentBidPrice(string productId) =>
        (await _coinbaseClient!.Products.GetBestBidAskAsync(new List<string> { productId }))
        .FirstOrDefault()?.Bids.FirstOrDefault()?.Price is string bestBidPrice
        ? decimal.Parse(bestBidPrice)
        : throw new InvalidOperationException("No best bid price available.");
    
    public Task<List<ProductBook>> GetBestBidAskAsync(List<string> productIds) =>_coinbaseClient!.Products.GetBestBidAskAsync(productIds);
    public Task<List<Order>> GetAllOrders() => _coinbaseClient!.Orders.ListOrdersAsync();
    public async Task<Order> GetOrderAsync(string order_id) => await _coinbaseClient!.Orders.GetOrderAsync(order_id);
    


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
        // Parse the limitPrice to a decimal
        if (decimal.TryParse(limitPrice, out var limitPriceDecimal))
        {
            // Check if the limitPrice has more than 6 decimal places
            if (decimal.Round(limitPriceDecimal, 6) != limitPriceDecimal)
            {
                // Round to 6 decimal places
                limitPriceDecimal = Math.Round(limitPriceDecimal, 6);

                // Convert back to string
                limitPrice = limitPriceDecimal.ToString("F6");
            }
        }
        else
        {
            throw new ArgumentException("Invalid limit price format", nameof(limitPrice));
        }

        try
        {
            return await _coinbaseClient!.Orders.CreateLimitOrderGTCAsync(productId, side, baseSize, limitPrice, postOnly, true);
        }
        catch (Exception ex)
        {

            throw;
        }

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