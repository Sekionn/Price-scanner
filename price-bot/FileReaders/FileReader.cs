using Ganss.Excel;
using price_bot.FileReaders.FileModels;
using price_bot.Models;

namespace price_bot.FileReaders;
public class FileReader
{
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
                catch
                {
                    Console.WriteLine("No files found in the files folder");
                    Console.WriteLine(file);

                    Console.ReadKey();
                }
            }

        }
        catch (Exception)
        {
            Console.WriteLine("No files found in the files folder");

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