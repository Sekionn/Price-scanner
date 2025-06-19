namespace price_bot.Models;
public class Product
{
    public required double price { get; init; }
    public required string productNumber { get; init; }
    public required string title { get; init; }
    public required string url { get; init; }
}