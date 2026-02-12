using Ganss.Excel;
using price_bot.FileReaders.FileModels;
using price_bot.Logging;
using price_bot.Models;
using System.Text.Json;

namespace price_bot.FileReaders;
public class FileReader
{
    public static T? ReadJson<T>(string filename)
    {
        var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if(!File.Exists(Path.Combine(applicationDataPath, $"price-scanner\\{filename}.json")))
        {
            return default;
        }

        using (StreamReader r = new StreamReader(Path.Combine(applicationDataPath, $"price-scanner\\{filename}.json")))
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            return JsonSerializer.Deserialize<T>(r.ReadToEnd(), options);
        }
    }

    public static List<AlstroemsProduct> ReadExcel()
    {
        Console.WriteLine("\nTjekker efter excel fil");
        List<AlstroemsExcel> products = [];
        try
        {
            var files = Directory.GetFiles(Path.GetFullPath(@"Filer"), "*.xlsx");

            foreach (var file in files)
            {
                try
                {
                    var excelFile = new ExcelMapper(file).Fetch<AlstroemsExcel>().ToList();
                    if (excelFile != null)
                    {
                        products.AddRange(excelFile);
                    }
                }
                catch(Exception e)
                {
                    var logger = new LoggingService<FileReader>();
                    logger.CreateError(e.Message);

                    Console.ReadKey();
                    throw;
                }
            }

        }
        catch (Exception e)
        {
            var logger = new LoggingService<FileReader>();
            logger.CreateError(e.Message);

            Console.ReadKey();
            throw;
        }
        
        var AlstroemsProducts = products.Where(p => Int64.TryParse(p.Nummer, out long result) is true).Select(p => p.Convert()).ToList();

        if (AlstroemsProducts.Count < products.Count)
        {
            Console.WriteLine("nogle rækker er blevet fjernet fra listen, tjek venligst listens varenumre, steder med fejl hedder:");
            products.Where(p => !AlstroemsProducts.Any(ap => ap.ProductNumber.ToString() == p.Nummer)).ToList().ForEach(i => Console.WriteLine(i.Nummer));
        }

        return AlstroemsProducts!;
    }
}