using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using BlazorStockTicker.Hubs;

namespace BlazorStockTicker.Stocks
{
    public class StockTicker
    {
        private readonly IHubContext<StockTickerHub> _hubContext;
        private readonly ConcurrentDictionary<string, Stock> _stocks;
        private readonly double _rangePercent;
        private readonly TimeSpan _updateInterval;
        private readonly Timer _timer;
        private volatile bool _updatingStockPrices;

        // コンストラクタ
        public StockTicker(IHubContext<StockTickerHub> hubContext)
        {
            // SignalR コンテキストを DI により取得して設定
            _hubContext = hubContext;

            // 株価情報を保持する
            _stocks = new ConcurrentDictionary<string, Stock>();

            // 株価情報の初期値を設定
            InitializeStockPrices(_stocks);

            // 株価は下の TryUpdateStockPrice メソッドでランダムに
            // 変更されるが、その上限・下限を 2% に設定している
            _rangePercent = .002;

            // Timer を使って UpdateStockPrices メソッドを呼ぶ間隔
            _updateInterval = TimeSpan.FromMilliseconds(250);

#nullable disable
            // _updateInterval (250 ミリ秒間隔) で UpdateStockPrices
            // メソッドが呼ばれるよう Timer を設定
            _timer = new Timer(UpdateStockPrices,
                               null,
                               _updateInterval,
                               _updateInterval);
#nullable enable

            // 株価情報が更新中であることを示すフラグ
            _updatingStockPrices = false;
        }

        private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateStockPrices(new object());

        }

        // 株価情報の初期値を設定するヘルパメソッド
        private void InitializeStockPrices(
            ConcurrentDictionary<string, Stock> stocks)
        {
            var init = new List<Stock>
            {
                new Stock { Symbol = "MSFT", Price = 30.31m },
                new Stock { Symbol = "APPL", Price = 578.18m },
                new Stock { Symbol = "GOOG", Price = 570.30m }
            };
            stocks.Clear();
            // bool TryAdd (TKey key, TValue value) メソッドで
            // キー/値ペアを追加する。成功すると true を返す。
            // キーが既に存在する場合は false を返す
            init.ForEach(init => _stocks.TryAdd(init.Symbol, init));
        }

        // ConcurrentDictionary<string, Stock> のインスタンス　
        // から Value プロパティで IEnumerable<Stock> を取得
        public IEnumerable<Stock> GetAllStocks()
        {
            return _stocks.Values;
        }
        public async Task<IEnumerable<Stock>> GetAllStocksAsync()
        {
            return _stocks.Values;
        }

        private readonly object _updateStockPricesLock = new object();

        // Timer を使って 250ms 毎に以下のメソッドが呼ばれる。
        // 株価に変更があった場合は BroadcastStockPrice メソッ
        // ドで株価情報をクライアントへ送信する
        private void UpdateStockPrices(object state)
        {
            lock (_updateStockPricesLock)
            {
                if (!_updatingStockPrices)
                {
                    _updatingStockPrices = true;

                    // _stocks.Values は IEnumerable<Stock>
                    foreach (var stock in _stocks.Values)
                    {
                        if (TryUpdateStockPrice(stock))
                        {
                            // 株価に変更があった場合は株価
                            // 情報をクライアントへ送信
                            BroadcastStockPrice(stock);
                        }
                    }

                    _updatingStockPrices = false;
                }
            }
        }

        private readonly Random _updateOrNotRandom = new Random();

        private bool TryUpdateStockPrice(Stock stock)
        {
            // 0.0 以上 1.0 未満のランダム値
            var r = _updateOrNotRandom.NextDouble();
            if (r > .1)
            {
                return false;
            }

            // 株価をランダムに変更する
            var random = new Random((int)Math.Floor(stock.Price));
            var percentChange = random.NextDouble() * _rangePercent;
            var pos = random.NextDouble() > .51;
            var change = Math.Round(stock.Price * (decimal)percentChange, 2);
            change = pos ? change : -change;

            stock.Price += change;
            return true;
        }

        // 株価情報をクライアントへ送信する
        private void BroadcastStockPrice(Stock stock)
        {
            _hubContext.Clients.All.SendAsync("UpdateStockPrice", stock);
        }
    }
}
