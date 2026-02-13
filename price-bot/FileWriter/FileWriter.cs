using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using price_bot.Enums;
using price_bot.Interfaces;
using price_bot.Logging;
using price_bot.Models;
using System.IO;
using System.Text.Json;

namespace price_bot.FileWriter;
public class FileWriter
{
    public static async Task WriteJSONFile(IJSONSerializable data, string filename)
    {
        var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (!File.Exists(Path.Combine(applicationDataPath, $"price-scanner")))
        {
            Directory.CreateDirectory(Path.Combine(applicationDataPath, $"price-scanner"));
        }

        File.WriteAllText(Path.Combine(applicationDataPath, $"price-scanner\\{filename}.json"),
            JsonConvert.SerializeObject(data));
        return;
    }

    public static bool WriteTXTFile(List<IncorrectlyPricedProduct> products)
    {
        using (StreamWriter outputFile = new(Path.Combine(@"Forkerte priser\Forkerte priser.txt")))
        {
            foreach (var product in products)
            {
                try
                {
                    outputFile.WriteLine($"{product.ProductName} Har en {(product.GrowthType == GrowthType.CostsLess ? "lavere pris" : "Højere pris")}");
                    outputFile.WriteLine($"Varenummeret er {product.ProductNumber}");
                    outputFile.WriteLine($"Butikkens pris er {product.CurrentPrice} DKK");
                    outputFile.WriteLine($"Bog og ide's pris er {product.AlternatePrice} DKK");
                    outputFile.WriteLine($"Differencen mellem priserne er {product.CurrentPrice - product.AlternatePrice} DKK");
                    outputFile.WriteLine($"Varebeholdningen er: {product.Stock}");
                    outputFile.WriteLine($"Link til produktet for dobbelt tjek: {product.Url}");
                    outputFile.WriteLine($"-------------------------------------- Næste produkt ------------------------------------------------------");
                }
                catch (Exception e)
                {
                    var logger = new LoggingService<FileWriter>();
                    logger.CreateError(e.Message);
                    Console.ReadKey();
                    throw;
                }


            }

            Console.WriteLine("Textfile Has been created In folder called File");
        }

        return true;
    }

    public static bool WriteExcelFile(List<IncorrectlyPricedProduct> products)
    {
        try
        {
            var path = Path.Combine(@"Forkerte priser\Forkerte priser.xls");

            HSSFWorkbook workbook = new HSSFWorkbook();
            CreateCellStyles(workbook, out HSSFCellStyle borderedCellStyle, out HSSFCellStyle borderedCellStyleNumbers, out HSSFCellStyle borderedCellStyleValuta);

            ISheet Sheet = workbook.CreateSheet("Forkerte priser");
            //Headere
            IRow HeaderRow = Sheet.CreateRow(0);

            //Checkmark fælter
            CreateCell(HeaderRow, 0, "Pris tjekket", borderedCellStyle);
            CreateCell(HeaderRow, 1, "Skal blokeres", borderedCellStyle);

            //Informationer
            CreateCell(HeaderRow, 2, "Navn", borderedCellStyle);
            CreateCell(HeaderRow, 3, "EAN stregkode", borderedCellStyle);
            CreateCell(HeaderRow, 4, "Bog og ide's pris", borderedCellStyle);
            CreateCell(HeaderRow, 5, "Butikkens pris", borderedCellStyle);
            CreateCell(HeaderRow, 6, "Varenummer", borderedCellStyle);
            CreateCell(HeaderRow, 7, "Total tab/vind", borderedCellStyle);
            CreateCell(HeaderRow, 8, "Lagerbeholdning", borderedCellStyle);
            CreateCell(HeaderRow, 9, "Pris difference", borderedCellStyle);

            //CreateCell(HeaderRow, 9, "Link", borderedCellStyle);

            //Index til rækkerne, da første række jo er titler
            int RowIndex = 1;

            foreach (IncorrectlyPricedProduct product in products)
            {
                IRow CurrentRow = Sheet.CreateRow(RowIndex);
                //Checkmark fælter
                CreateCell(CurrentRow, 0, "", borderedCellStyle);
                CreateCell(CurrentRow, 1, "", borderedCellStyle);

                //Informationer
                CreateCell(CurrentRow, 2, product.ProductName, borderedCellStyle);
                if (product.EAN != null)
                {
                    CreateCell(CurrentRow, 3, product.EAN, borderedCellStyleNumbers);
                }
                else
                {
                    CreateCell(CurrentRow, 3, "EAN kode ikke fundet", borderedCellStyle);
                }

                CreateNumericCell(CurrentRow, 4, product.AlternatePrice, borderedCellStyleValuta);
                CreateNumericCell(CurrentRow, 5, product.CurrentPrice, borderedCellStyleValuta);
                CreateNumericCell(CurrentRow, 6, int.Parse(product.ProductNumber), borderedCellStyleNumbers);
                CreateNumericCell(CurrentRow, 7, ((product.Stock * product.DifferentialPrice) * -1), borderedCellStyleValuta);
                CreateNumericCell(CurrentRow, 8, product.Stock, borderedCellStyleNumbers);
                CreateNumericCell(CurrentRow, 9, (product.DifferentialPrice * -1), borderedCellStyleValuta);

                //CreateCell(CurrentRow, 9, product.Url, borderedCellStyle);

                RowIndex++;
            }

            int lastColumNum = Sheet.GetRow(0).LastCellNum;
            for (int i = 0; i <= lastColumNum; i++)
            {
                Sheet.AutoSizeColumn(i);
                GC.Collect();
            }

            using (var fileData = new FileStream(path, FileMode.Create))
            {
                workbook.Write(fileData);
            }
        }
        catch (Exception e)
        {
            var logger = new LoggingService<FileWriter>();
            logger.CreateError(e.Message);
            Console.ReadKey();
            throw;
        }


        return true;
    }

    private static void CreateCellStyles(HSSFWorkbook workbook, out HSSFCellStyle borderedCellStyle, out HSSFCellStyle borderedCellStyleNumbers, out HSSFCellStyle borderedCellStyleValuta)
    {
        borderedCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
        borderedCellStyle.BorderLeft = BorderStyle.Medium;
        borderedCellStyle.BorderTop = BorderStyle.Medium;
        borderedCellStyle.BorderRight = BorderStyle.Medium;
        borderedCellStyle.BorderBottom = BorderStyle.Medium;
        borderedCellStyle.VerticalAlignment = VerticalAlignment.Center;
        borderedCellStyle.Alignment = HorizontalAlignment.Left;

        borderedCellStyleNumbers = (HSSFCellStyle)workbook.CreateCellStyle();
        borderedCellStyleNumbers.BorderLeft = BorderStyle.Medium;
        borderedCellStyleNumbers.BorderTop = BorderStyle.Medium;
        borderedCellStyleNumbers.BorderRight = BorderStyle.Medium;
        borderedCellStyleNumbers.BorderBottom = BorderStyle.Medium;
        borderedCellStyleNumbers.VerticalAlignment = VerticalAlignment.Center;
        borderedCellStyleNumbers.Alignment = HorizontalAlignment.Right;
        borderedCellStyleNumbers.DataFormat = 1;

        borderedCellStyleValuta = (HSSFCellStyle)workbook.CreateCellStyle();
        borderedCellStyleValuta.BorderLeft = BorderStyle.Medium;
        borderedCellStyleValuta.BorderTop = BorderStyle.Medium;
        borderedCellStyleValuta.BorderRight = BorderStyle.Medium;
        borderedCellStyleValuta.BorderBottom = BorderStyle.Medium;
        borderedCellStyleValuta.VerticalAlignment = VerticalAlignment.Center;
        borderedCellStyleValuta.Alignment = HorizontalAlignment.Right;
        borderedCellStyleValuta.DataFormat = 2;
    }

    private static void CreateCell(IRow currentRow, int cellIndex, string value, HSSFCellStyle style, bool? isHyperlink = false)
    {
        ICell cell = currentRow.CreateCell(cellIndex);
        cell.SetCellValue(value);
        cell.CellStyle = style;
    }

    private static void CreateNumericCell(IRow currentRow, int cellIndex, double value, HSSFCellStyle style)
    {
        ICell cell = currentRow.CreateCell(cellIndex);
        cell.SetCellType(CellType.Numeric);

        
        cell.SetCellValue(value);
        cell.CellStyle = style;
    }
}