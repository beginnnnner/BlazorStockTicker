using Microsoft.AspNetCore.SignalR;
using BlazorStockTicker.Stocks;

namespace BlazorStockTicker.Hubs
{
    public class StockTickerHub : Hub
    {
        private readonly StockTicker _stockTicker;

        public StockTickerHub(StockTicker stockTicker)
        {
            // Broadcaster インスタンスを DI により取得して設定。
            // Program.cs で AddSingleton メソッドを使ってシング
            // ルトンになるようにしている
            _stockTicker = stockTicker;
        }

        // StockTicker.GetAllStock メソッドは株価情報を
        // IEnumerable<Stock> として返す
        public IEnumerable<Stock> GetAllStocks()
        {
            return _stockTicker.GetAllStocks();
        }
        public async Task<IEnumerable<Stock>> GetAllStocksAsync()
        {
            return await _stockTicker.GetAllStocksAsync();
        }
    }
}
