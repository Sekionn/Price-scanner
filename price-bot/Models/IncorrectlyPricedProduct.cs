using Ganss.Excel;
using price_bot.Enums;

namespace price_bot.Models;
public class IncorrectlyPricedProduct
{
    public double DifferentialPrice { get => CurrentPrice - AlternatePrice; }
    public required double AlternatePrice { get; init; }
    public required double CurrentPrice { get; init; }

    public required string ProductNumber { get; init; }
    public required string ProductName { get; init; }

    public required int Stock { get; init; }
    public required GrowthType GrowthType { get; init; }
    public required string Url { get; init; }
    public string EAN { get; init; }

}