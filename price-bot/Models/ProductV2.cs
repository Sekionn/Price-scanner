namespace price_bot.Models;
public class ProductV2
{
    public required string url { get; init; }
    public required string productNumber { get; init; }
    public required string eanNumber { get; init; }
    public required string title { get; init; }
    public required string author { get; init; }
    public required double price { get; init; }
}