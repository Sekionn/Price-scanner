using price_bot.Enums;
using price_bot.FileReaders;
using price_bot.Logging;
using price_bot.Models;
using price_bot.Style;
using System.Net.Http.Json;
using System.Text.Json;

namespace price_bot.Networking;
public class ProductRetreiver
{
    static readonly string baseUrl = "https://scraper.juuls-trinkets.com/";
    static readonly HttpClient client = new();
    LoggingService<ProductRetreiver> _logger = new LoggingService<ProductRetreiver>();

    public async Task<List<IncorrectlyPricedProduct>> GetProductsWithIncorrectPricesFromScraper()
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

        const int batchSize = 50;

        for (int i = 0; i < products.Count; i += batchSize)
        {
            ProgressBarService.UpdateProgressBar(products.Count, i);

            var batch = products
                .Skip(i)
                .Take(batchSize)
                .ToList();
            
            _logger.CreateLog($"Checked productnumber count {i}");
            var scrapedProducts = await GetProductFromScraperAPI(batch.Select((p) => new ProductPriceLookupRequestDto { identifier = p.ProductNumber, name = p.ProductName, productType = p.ChainCategoryCode}).ToList());

            if (scrapedProducts != null)
            {
                foreach (var scrapedProduct in scrapedProducts)
                {
                    var productData = products.Where((p) => p.ProductNumber == scrapedProduct.productNumber).First();

                    if (!productData.Price.Equals(scrapedProduct.price))
                    {
                        incorrectProducts.Add(new IncorrectlyPricedProduct
                        {
                            CurrentPrice = productData.Price,
                            AlternatePrice = scrapedProduct.price,
                            ProductName = productData.ProductName,
                            ProductNumber = productData.ProductNumber,
                            Stock = productData.Stock,
                            GrowthType = productData.Price > scrapedProduct.price ? GrowthType.CostsLess : GrowthType.CostsMore,
                            Url = scrapedProduct.url,
                            EAN = scrapedProduct.eanNumber != null ? scrapedProduct.eanNumber : productData.EAN,
                            CategoryCode = productData.CategoryCode,
                            Authors = scrapedProduct.author,
                            specialOffer = scrapedProduct.specialOffer
                        });
                    }
                }
            }
            else
            {
                _logger.CreateError($"Noget gik galt under efterspørgslen af følgende varenumre: {string.Join(',', batch)}");
            }


        }

        Console.Write("\n");
        return [.. incorrectProducts.OrderBy(p => p.GrowthType).OrderBy(p => p.DifferentialPrice)];
    }


    internal async Task<Product?> GetProductFromAPI(AlstroemsProduct product)
    {
        HttpResponseMessage response = await client.GetAsync($"https://www.bog-ide.dk/api/search/all?PageIndex=0&PageSize=4&Search={product.ProductNumber}");

        try
        {
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            SearchDTO searchData = JsonSerializer.Deserialize<SearchDTO>(responseBody)!;

            return searchData.products.results.Where(p => p.productNumber.Equals(product.ProductNumber)).FirstOrDefault();
        }
        catch (Exception e)
        {
            _logger.CreateError(e.Message);
            return null;
        }
    }

    internal async Task<List<ProductV2>> GetProductFromScraperAPI(List<ProductPriceLookupRequestDto> productNumbers) {
        try
        {
            HttpResponseMessage response = await client.PostAsync($"{baseUrl}api/prices/batch", JsonContent.Create(productNumbers));

            
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.CreateError(
                    $"Scraper API failed. Status: {(int)response.StatusCode} {response.StatusCode}. Body: {responseBody}"
                );

                return null;
            }

            return JsonSerializer.Deserialize<List<ProductV2>>(responseBody)!;
        }
        catch (Exception e)
        {
            _logger.CreateError(e.ToString());
            return null;
        }
    }
}