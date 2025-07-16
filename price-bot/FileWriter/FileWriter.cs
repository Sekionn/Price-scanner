using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using price_bot.Enums;
using price_bot.Logging;
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
                try
                {
                    outputFile.WriteLine($"{product.ProductName} Har en {(product.GrowthType == GrowthType.CostsLess ? "lavere pris" : "Højere pris")}");
                    outputFile.WriteLine($"Varenummeret er {product.ProductNumber}");
                    outputFile.WriteLine($"Butikkens pris er {product.CurrentPrice} DKK");
                    outputFile.WriteLine($"Bog og ide's pris er {product.AlternatePrice} DKK");
                    outputFile.WriteLine($"Differencen mellem priserne er {product.CurrentPrice - product.AlternatePrice} DKK");
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

    public async Task<bool> WriteExcelFile(List<IncorrectlyPricedProduct> products)
    {
        try
        {
            var path = Path.Combine(@"Forkerte priser\Forkerte priser.xls");

            HSSFWorkbook workbook = new HSSFWorkbook();

            HSSFCellStyle borderedCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
            borderedCellStyle.BorderLeft = BorderStyle.Medium;
            borderedCellStyle.BorderTop = BorderStyle.Medium;
            borderedCellStyle.BorderRight = BorderStyle.Medium;
            borderedCellStyle.BorderBottom = BorderStyle.Medium;
            borderedCellStyle.VerticalAlignment = VerticalAlignment.Center;

            ISheet Sheet = workbook.CreateSheet("Forkerte priser");
            //Headere
            IRow HeaderRow = Sheet.CreateRow(0);

            CreateCell(HeaderRow, 0, "Pris tjekket", borderedCellStyle);
            CreateCell(HeaderRow, 1, "Skal blokeres", borderedCellStyle);
            CreateCell(HeaderRow, 2, "Pris difference", borderedCellStyle);
            CreateCell(HeaderRow, 3, "Bog og ide's pris", borderedCellStyle);
            CreateCell(HeaderRow, 4, "Butikkens pris", borderedCellStyle);
            CreateCell(HeaderRow, 5, "Varenummer", borderedCellStyle);
            CreateCell(HeaderRow, 6, "Navn", borderedCellStyle);
            CreateCell(HeaderRow, 7, "Link", borderedCellStyle);

            //Index til rækkerne, da første række jo er titler
            int RowIndex = 1;

            foreach (IncorrectlyPricedProduct product in products)
            {
                IRow CurrentRow = Sheet.CreateRow(RowIndex);
                CreateCell(CurrentRow, 0, "", borderedCellStyle);
                CreateCell(CurrentRow, 1, "", borderedCellStyle);
                CreateCell(CurrentRow, 2, product.DifferentialPrice.ToString(), borderedCellStyle);
                CreateCell(CurrentRow, 3, product.AlternatePrice.ToString(), borderedCellStyle);
                CreateCell(CurrentRow, 4, product.CurrentPrice.ToString(), borderedCellStyle);
                CreateCell(CurrentRow, 5, product.ProductNumber.ToString(), borderedCellStyle);
                CreateCell(CurrentRow, 6, product.ProductName, borderedCellStyle);
                CreateCell(CurrentRow, 7, product.Url, borderedCellStyle);
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

    private static void CreateCell(IRow currentRow, int cellIndex, string value, HSSFCellStyle style, bool? isHyperlink = false)
    {
        ICell cell = currentRow.CreateCell(cellIndex);
        if (isHyperlink == true)
        {
            try
            {
                style.WrapText = true;
                cell.SetCellFormula("HYPERLINK(INDIRECT(\"H\" & ROW()), \"Hello\")");
            }
            catch (Exception e)
            {
                var logger = new LoggingService<FileWriter>();
                logger.CreateError($"{e.Message}: formula: {value}");
                Console.ReadKey();
                throw;
            }
        }
        cell.SetCellValue(value);
        cell.CellStyle = style;
    }
}