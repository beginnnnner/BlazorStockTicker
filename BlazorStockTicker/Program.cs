using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using BlazorStockTicker.Data;
using BlazorStockTicker.Stocks;
using BlazorStockTicker.Hubs;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// *** 追加 ***
builder.Services.AddSignalR();
// StockTickerHub でコンストラクタ経由 DI によりシングル
// トンインスタンスを取得できるよう以下の設定を行う
builder.Services.AddSingleton<StockTicker>();

// SyncFusion Service を追加
builder.Services.AddSyncfusionBlazor(options => { options.IgnoreScriptIsolation = true; });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<StockTickerHub>("/stockTickerHub");
app.MapFallbackToPage("/_Host");

app.Run();
