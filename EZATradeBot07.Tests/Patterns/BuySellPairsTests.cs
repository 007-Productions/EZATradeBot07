using EZATB07.Library.Exchanges.Coinbase;
using EZATB07.Library.Exchanges.Coinbase.Models;
using EZATB07.Library.Exchanges.Coinbase.Patterns;
using EZATB07.Library.Exchanges.Coinbase.Tests;
using Microsoft.Extensions.Logging;

public class BuySellPairsTests : IClassFixture<CoinbaseWrapperTestFixture>
{
    private readonly ILogger<BuySellPairs> _logger;
    private readonly ILogger<CoinBaseService> _coinBaseServicelogger;
    private readonly CoinbaseWrapperTestFixture _fixture;
    private readonly ICoinbaseWrapper _coinbaseWrapper;
    private readonly ICoinBaseService _coinBaseService;
    private readonly BuySellPairs _buySellPairs;

    public BuySellPairsTests(CoinbaseWrapperTestFixture fixture)
    {
        // Configure logger to log to the console
        using var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole();});
        
        _logger = loggerFactory.CreateLogger<BuySellPairs>();

        _coinBaseServicelogger = loggerFactory.CreateLogger<CoinBaseService>();

        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _coinbaseWrapper = new CoinbaseWrapper(_fixture.ApiKey, _fixture.ApiSecret);
        _coinBaseService = new CoinBaseService(_coinbaseWrapper, _coinBaseServicelogger);
        _buySellPairs = new BuySellPairs(_coinBaseService, _coinbaseWrapper, _logger);
    }

    [Fact()]
    public async Task BuyLoopTillFundsRunOutTestAsync()
    {
        var result = await _buySellPairs.BuyLoopTillFundsRunOut("DEGEN-USDC", "1", 0.05m);

        Assert.Equal(Status.Success, result.Status);
    }

}
