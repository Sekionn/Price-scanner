namespace price_bot.Models;
public class ProductPriceLookupRequestDto
{
    public required string identifier { get; init; }
    public required string name { get; init; }
    public required string productType { get; init; }
}