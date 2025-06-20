﻿using Ganss.Excel;
using price_bot.Enums;

namespace price_bot.Models;
public class IncorrectlyPricedProduct
{
    [Column("Pris difference")]
    public double DifferentialPrice { get => CurrentPrice - AlternatePrice; }
    [Column("Bog og ide's pris")]
    public required double AlternatePrice { get; init; }
    [Column("Butikkens pris")]
    public required double CurrentPrice { get; init; }

    [Column("Varenummer")]
    public required string ProductNumber { get; init; }
    [Column("Navn")]
    public required string ProductName { get; init; }
    
    public required GrowthType GrowthType { get; init; }
    [Ignore]
    public required string Url { get; init; }
    [Column("Link")]
    [Formula]
    public string HyperLink { get => $"HYPERLINK(\"{Url}\")"; }
}