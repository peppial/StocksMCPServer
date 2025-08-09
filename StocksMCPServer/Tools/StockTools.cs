using Microsoft.Extensions.Logging;

namespace StocksMCPServer.Tools;

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;


[McpServerToolType]
public sealed class StockTools
{
    private static readonly string ALPHA_VANTAGE_API_KEY = Environment.GetEnvironmentVariable("ALPHA_VANTAGE_API_KEY");

    [McpServerTool(UseStructuredContent = true), Description("Get portfolio performance.")]
    public static async Task<object> GetPerformance(
        HttpClient client,
        [Description("Stock symbols")] string[] stocks)
    {

        List<string> data = await GetPortfolioPerformance(client, stocks);
        return data;
    }

    private static async Task<string> GetStockPrice(HttpClient client, string symbol)
    {
        var url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={ALPHA_VANTAGE_API_KEY}";
        var response = await client.GetStringAsync(url);
        var data = JsonDocument.Parse(response);

        var globalQuote = data.RootElement.GetProperty("Global Quote");
        globalQuote.TryGetProperty("10. change percent", out var changePercent);
        if (string.IsNullOrEmpty(changePercent.ToString())) return "";
        return symbol + ": " + changePercent;

    }

    private static async Task<List<string>> GetPortfolioPerformance(HttpClient client, string[] symbols)
    {
        List<string> portfolio = [];

        foreach (var symbol in symbols)
        {
            var stockData = await GetStockPrice(client, symbol);
            portfolio.Add(stockData);

        }

        return portfolio;
    }
}
