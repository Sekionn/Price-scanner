using MathNet.Numerics.LinearAlgebra.Factorization;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using price_bot.Enums;
using price_bot.Interfaces;
using price_bot.Logging;
using price_bot.Models;

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

            CreateCellStyles(
                workbook,
                out HSSFCellStyle borderedCellStyle,
                out HSSFCellStyle borderedCellStyleNumbers,
                out HSSFCellStyle borderedCellStyleValuta);

            Dictionary<short, HSSFCellStyle> offerStyleCache = new Dictionary<short, HSSFCellStyle>();

            ISheet Sheet = workbook.CreateSheet("Forkerte priser");

            // Headere
            IRow HeaderRow = Sheet.CreateRow(0);

            // Checkmark fælter
            CreateCell(workbook, HeaderRow, 0, "Pris tjekket", borderedCellStyle, false, offerStyleCache);
            CreateCell(workbook, HeaderRow, 1, "Skal blokeres", borderedCellStyle, false, offerStyleCache);

            // Informationer
            CreateCell(workbook, HeaderRow, 2, "forfattere", borderedCellStyle, false, offerStyleCache);
            CreateCell(workbook, HeaderRow, 3, "Navn", borderedCellStyle, false, offerStyleCache);
            CreateCell(workbook, HeaderRow, 4, "EAN stregkode", borderedCellStyle, false, offerStyleCache);
            CreateCell(workbook, HeaderRow, 5, "Bog og ide's pris", borderedCellStyle, false, offerStyleCache);
            CreateCell(workbook, HeaderRow, 6, "Butikkens pris", borderedCellStyle, false, offerStyleCache);
            CreateCell(workbook, HeaderRow, 7, "Varenummer", borderedCellStyle, false, offerStyleCache);
            CreateCell(workbook, HeaderRow, 8, "Total tab/vind", borderedCellStyle, false, offerStyleCache);
            CreateCell(workbook, HeaderRow, 9, "Lagerbeholdning", borderedCellStyle, false, offerStyleCache);
            CreateCell(workbook, HeaderRow, 10, "Pris difference", borderedCellStyle, false, offerStyleCache);
            CreateCell(workbook, HeaderRow, 11, "Varekategorikode", borderedCellStyle, false, offerStyleCache);

            // Index til rækkerne, da første række jo er titler
            int RowIndex = 1;

            foreach (IncorrectlyPricedProduct product in products)
            {
                IRow CurrentRow = Sheet.CreateRow(RowIndex);

                // Checkmark fælter
                CreateCell(workbook, CurrentRow, 0, "", borderedCellStyle, product.specialOffer, offerStyleCache);
                CreateCell(workbook, CurrentRow, 1, "", borderedCellStyle, product.specialOffer, offerStyleCache);

                // Informationer
                if (product.Authors != null)
                {
                    CreateCell(workbook, CurrentRow, 2, product.Authors, borderedCellStyle, product.specialOffer, offerStyleCache);
                }
                else
                {
                    CreateCell(workbook, CurrentRow, 2, "", borderedCellStyle, product.specialOffer, offerStyleCache);
                }

                CreateCell(workbook, CurrentRow, 3, product.ProductName, borderedCellStyle, product.specialOffer, offerStyleCache);

                if (product.EAN != null)
                {
                    CreateCell(workbook, CurrentRow, 4, product.EAN, borderedCellStyleNumbers, product.specialOffer, offerStyleCache);
                }
                else
                {
                    CreateCell(workbook, CurrentRow, 4, "EAN kode ikke fundet", borderedCellStyle, product.specialOffer, offerStyleCache);
                }

                CreateNumericCell(workbook, CurrentRow, 5, product.AlternatePrice, borderedCellStyleValuta, product.specialOffer, offerStyleCache);
                CreateNumericCell(workbook, CurrentRow, 6, product.CurrentPrice, borderedCellStyleValuta, product.specialOffer, offerStyleCache);
                CreateNumericCell(workbook, CurrentRow, 7, int.Parse(product.ProductNumber), borderedCellStyleNumbers, product.specialOffer, offerStyleCache);
                CreateNumericCell(workbook, CurrentRow, 8, ((product.Stock * product.DifferentialPrice) * -1), borderedCellStyleValuta, product.specialOffer, offerStyleCache);
                CreateNumericCell(workbook, CurrentRow, 9, product.Stock, borderedCellStyleNumbers, product.specialOffer, offerStyleCache);
                CreateNumericCell(workbook, CurrentRow, 10, (product.DifferentialPrice * -1), borderedCellStyleValuta, product.specialOffer, offerStyleCache);

                if (product.CategoryCode != null)
                {
                    CreateCell(workbook, CurrentRow, 11, product.CategoryCode, borderedCellStyleNumbers, product.specialOffer, offerStyleCache);
                }
                else
                {
                    CreateCell(workbook, CurrentRow, 11, "Kategorikode ikke fundet", borderedCellStyle, product.specialOffer, offerStyleCache);
                }

                // CreateCell(workbook, CurrentRow, 9, product.Url, borderedCellStyle, product.specialOffer, offerStyleCache);

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

    private static void CreateCellStyles(
        HSSFWorkbook workbook,
        out HSSFCellStyle borderedCellStyle,
        out HSSFCellStyle borderedCellStyleNumbers,
        out HSSFCellStyle borderedCellStyleValuta)
    {
        borderedCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
        borderedCellStyle.BorderLeft = BorderStyle.Medium;
        borderedCellStyle.BorderTop = BorderStyle.Medium;
        borderedCellStyle.BorderRight = BorderStyle.Medium;
        borderedCellStyle.BorderBottom = BorderStyle.Medium;
        borderedCellStyle.VerticalAlignment = VerticalAlignment.Center;
        borderedCellStyle.Alignment = HorizontalAlignment.Left;
        borderedCellStyle.FillForegroundColor = HSSFColor.White.Index;
        borderedCellStyle.FillPattern = FillPattern.SolidForeground;

        borderedCellStyleNumbers = (HSSFCellStyle)workbook.CreateCellStyle();
        borderedCellStyleNumbers.BorderLeft = BorderStyle.Medium;
        borderedCellStyleNumbers.BorderTop = BorderStyle.Medium;
        borderedCellStyleNumbers.BorderRight = BorderStyle.Medium;
        borderedCellStyleNumbers.BorderBottom = BorderStyle.Medium;
        borderedCellStyleNumbers.VerticalAlignment = VerticalAlignment.Center;
        borderedCellStyleNumbers.Alignment = HorizontalAlignment.Right;
        borderedCellStyleNumbers.DataFormat = 1;
        borderedCellStyleNumbers.FillForegroundColor = HSSFColor.White.Index;
        borderedCellStyleNumbers.FillPattern = FillPattern.SolidForeground;

        borderedCellStyleValuta = (HSSFCellStyle)workbook.CreateCellStyle();
        borderedCellStyleValuta.BorderLeft = BorderStyle.Medium;
        borderedCellStyleValuta.BorderTop = BorderStyle.Medium;
        borderedCellStyleValuta.BorderRight = BorderStyle.Medium;
        borderedCellStyleValuta.BorderBottom = BorderStyle.Medium;
        borderedCellStyleValuta.VerticalAlignment = VerticalAlignment.Center;
        borderedCellStyleValuta.Alignment = HorizontalAlignment.Right;
        borderedCellStyleValuta.DataFormat = 2;
        borderedCellStyleValuta.FillForegroundColor = HSSFColor.White.Index;
        borderedCellStyleValuta.FillPattern = FillPattern.SolidForeground;
    }

    private static void CreateCell(
        HSSFWorkbook workbook,
        IRow currentRow,
        int cellIndex,
        string value,
        HSSFCellStyle style,
        bool isOffer,
        Dictionary<short, HSSFCellStyle> offerStyleCache)
    {
        ICell cell = currentRow.CreateCell(cellIndex);
        cell.SetCellValue(value);

        cell.CellStyle = isOffer
            ? GetOfferStyle(workbook, style, offerStyleCache)
            : style;
    }

    private static void CreateNumericCell(
        HSSFWorkbook workbook,
        IRow currentRow,
        int cellIndex,
        double value,
        HSSFCellStyle style,
        bool isOffer,
        Dictionary<short, HSSFCellStyle> offerStyleCache)
    {
        ICell cell = currentRow.CreateCell(cellIndex);
        cell.SetCellType(CellType.Numeric);
        cell.SetCellValue(value);

        cell.CellStyle = isOffer
            ? GetOfferStyle(workbook, style, offerStyleCache)
            : style;
    }

    private static HSSFCellStyle GetOfferStyle(
        HSSFWorkbook workbook,
        HSSFCellStyle baseStyle,
        Dictionary<short, HSSFCellStyle> offerStyleCache)
    {
        short baseStyleIndex = baseStyle.Index;

        if (offerStyleCache.TryGetValue(baseStyleIndex, out HSSFCellStyle cachedStyle))
        {
            return cachedStyle;
        }

        HSSFCellStyle clonedStyle = (HSSFCellStyle)workbook.CreateCellStyle();
        clonedStyle.CloneStyleFrom(baseStyle);
        clonedStyle.FillForegroundColor = HSSFColor.LightYellow.Index;
        clonedStyle.FillPattern = FillPattern.SolidForeground;

        offerStyleCache[baseStyleIndex] = clonedStyle;

        return clonedStyle;
    }
}