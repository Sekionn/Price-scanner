using Ganss.Excel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using price_bot.Enums;
using price_bot.FileReaders.FileModels;
using price_bot.Models;

namespace price_bot.FileWriter;
internal class FileWriter
{
    public async Task<bool> WriteTXTFile(List<IncorrectlyPricedProduct> products)
    {
        using (StreamWriter outputFile = new(Path.Combine(@"Forkerte priser\Forkerte priser.txt")))
        {
            foreach (var product in products)
            {
                outputFile.WriteLine($"{product.ProductName} Har en {(product.GrowthType == GrowthType.CostsLess ? "lavere pris" : "Højere pris")}");
                outputFile.WriteLine($"Varenummeret er {product.ProductNumber}");
                outputFile.WriteLine($"Butikkens pris er {product.CurrentPrice} DKK");
                outputFile.WriteLine($"Bog og ide's pris er {product.AlternatePrice} DKK");
                outputFile.WriteLine($"Differencen mellem priserne er {product.CurrentPrice - product.AlternatePrice} DKK");
                outputFile.WriteLine($"Link til produktet for dobbelt tjek: {product.Url}");
                outputFile.WriteLine($"-------------------------------------- Næste produkt ------------------------------------------------------");
            }

            Console.WriteLine("Textfile Has been created In folder called File");
        }

        return true;
    }

    public async Task<bool> WriteExcelFile(List<IncorrectlyPricedProduct> products)
    {
        var path = Path.Combine(@"Forkerte priser\Hello.xlsx");
        var excelMapper = new ExcelMapper();

        //excelMapper.AddMapping(typeof(IncorrectlyPricedProduct), ExcelMapper.LetterToIndex("G"), "ProductName");

        excelMapper.AddMapping<IncorrectlyPricedProduct>("Link", ip => ip.HyperLink).AsFormula();
        await excelMapper.SaveAsync(path, products, "products");
        var wb = WorkbookFactory.Create(path);

        for (int i = 0; i < products.Count; i++)
        {
            wb.GetSheetAt(0);
            var row = wb.GetSheetAt(0).GetRow(i + 1).RowStyle;
            switch (products[i].DifferentialPrice)
            {
                
                case > -11:
                    wb.GetSheetAt(0).GetRow(i + 1).RowStyle.FillBackgroundColor = 10;
                    break;
                default:
                    wb.GetSheetAt(0).GetRow(i + 1).RowStyle.FillBackgroundColor = 10;
                    break;
            }

            //Assert.That(c0.StringCellValue, Is.EqualTo("Date"));
            //Assert.That(((XSSFColor)c0.CellStyle.FillForegroundColorColor).ARGBHex, Is.EqualTo("FF00B0F0"));
            //Assert.That(c1.DateCellValue, Is.EqualTo(new DateTime(2021, 1, 1)));
            //Assert.That(c2.DateCellValue, Is.EqualTo(new DateTime(2022, 2, 1)));
            //Assert.That(c1.CellStyle.DataFormat, Is.EqualTo(0xa4));
            //Assert.That(c2.CellStyle.DataFormat, Is.EqualTo(0xa4));
        }

        //using (StreamWriter outputFile = new(Path.Combine(@"Forkerte priser\Hello.csv")))
        //{
        //    foreach (var product in products)
        //    {
        //        outputFile.Write($"{product.ProductName} Har en {(product.GrowthType == GrowthType.CostsLess ? "lavere pris" : "Højere pris")};");
        //        outputFile.Write($"Varenummeret er {product.ProductNumber};");
        //        outputFile.Write($"Butikkens pris er {product.CurrentPrice} DKK;");
        //        outputFile.Write($"Bog og ide's pris er {product.AlternatePrice} DKK;");
        //        outputFile.Write($"Differencen mellem priserne er {product.CurrentPrice - product.AlternatePrice} DKK;");
        //        outputFile.WriteLine($"Link til produktet for dobbelt tjek: {product.Url};");
        //        //outputFile.WriteLine($"-------------------------------------- Næste produkt ------------------------------------------------------");
        //    }
        //    Console.WriteLine("Textfile Has been created In folder called File");
        //}

        return true;
    }
}