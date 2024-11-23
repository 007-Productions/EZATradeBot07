using Microsoft.Extensions.Configuration;

namespace EZATB07.Library.Exchanges.Coinbase.Tests
{
    public class CoinbaseWrapperTestFixture : IDisposable
    {
        public string ApiKey { get; private set; }
        public string ApiSecret { get; private set; }

        public CoinbaseWrapperTestFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<CoinbaseWrapperTestFixture>()
                .Build();

            ApiKey = configuration["COINBASE_CLOUD_TRADING_API_KEY"];
            ApiSecret = configuration["COINBASE_CLOUD_TRADING_API_SECRET"];
        }

        public void Dispose()
        {
            // Cleanup if necessary
        }
    }
}
