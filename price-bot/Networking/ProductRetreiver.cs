using price_bot.Enums;
using price_bot.FileReaders;
using price_bot.Logging;
using price_bot.Models;
using price_bot.Style;
using System.Text.Json;

namespace price_bot.Networking;
public class ProductRetreiver
{
    static readonly HttpClient client = new();

    public async Task<List<IncorrectlyPricedProduct>> GetProductsWithIncorrectPrices()
    {
        FileReader fileReader = new();
        var products = FileReader.ReadExcel();
        ProgressBarService progressBarService = new ProgressBarService();
        List<IncorrectlyPricedProduct> incorrectProducts = [];

        if (products == null || products.Count <= 0)
        {
            Console.WriteLine("Der var ingen varer i excel filen");
            return incorrectProducts;
        }
        var logger = new LoggingService<ProductRetreiver>();

        int productNumberCount = 1;
        foreach (var product in products)
        {
            ProgressBarService.UpdateProgressBar(products.Count, productNumberCount);

            //Console.WriteLine($"Tjekker varenummer: {product.ProductNumber}");
            logger.CreateLog($"Checked productnumber count {productNumberCount}");
            var websiteProduct = await GetProductFromAPI(product);

            if (websiteProduct != null)
            {
                if (!product.Price.Equals(websiteProduct.price))
                {
                    //Console.WriteLine($"Pris der ikke stemmer overens fundet på varenummer: {product.ProductNumber}");
                    incorrectProducts.Add(new IncorrectlyPricedProduct
                    {
                        CurrentPrice = product.Price,
                        AlternatePrice = websiteProduct.price,
                        ProductName = product.ProductName,
                        ProductNumber = product.ProductNumber,
                        Stock = product.Stock,
                        GrowthType = product.Price > websiteProduct.price ? GrowthType.CostsLess : GrowthType.CostsMore,
                        Url = websiteProduct.url,
                        EAN = product.EAN
                    });
                }
                else
                {
                    //Console.WriteLine($"Pris stemmer overens på varenummer: {product.ProductNumber}");
                }
            }
            else
            {
                //TODO: tilføj ny måde at vise advarsler på
                //Console.WriteLine($"Advarsel: vare ikke fundet på bog og ide's hjemmeside under varenummer: {product.ProductNumber}");
            }
            Thread.Sleep(1000);
            productNumberCount++;
        }

        Console.Write("\n");
        return [.. incorrectProducts.OrderBy(p => p.GrowthType).OrderBy(p => p.DifferentialPrice)];
    }

    internal async Task<Product?> GetProductFromAPI(AlstroemsProduct product)
    {
        HttpResponseMessage response = await client.GetAsync($"https://www.bog-ide.dk/api/search/all?PageIndex=0&PageSize=4&Search={product.ProductNumber}");
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        SearchDTO searchData = JsonSerializer.Deserialize<SearchDTO>(responseBody)!;

        return searchData.products.results.Where(p => p.productNumber.Equals(product.ProductNumber)).FirstOrDefault();
    }
}