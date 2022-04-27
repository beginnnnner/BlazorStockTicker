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

// *** �ǉ� ***
builder.Services.AddSignalR();
// StockTickerHub �ŃR���X�g���N�^�o�R DI �ɂ��V���O��
// �g���C���X�^���X���擾�ł���悤�ȉ��̐ݒ���s��
builder.Services.AddSingleton<StockTicker>();

// SyncFusion Service ��ǉ�
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
