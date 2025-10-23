using price_bot.Logging;

namespace price_bot.Demo_feature;
public class DemoController
{
    public bool HasDemoStarted()
    {
        var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return File.Exists(Path.Combine(applicationDataPath, "price-scanner\\Demo.txt"));
    }

    public bool HasDemoFinished()
    {
        var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        StreamReader sr = new StreamReader(Path.Combine(applicationDataPath, "price-scanner\\Demo.txt"));
        string line = sr.ReadLine();

        DateOnly startDate = DateOnly.Parse(line);
        DateOnly endDate = startDate.AddDays(183);

        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);

        return currentDate.CompareTo(endDate) > 0;
    }

    public bool StartDemo()
    {
        try
        {
            var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var directory = Directory.CreateDirectory(Path.Combine(applicationDataPath, "price-scanner"));

            using (StreamWriter w = File.AppendText(directory + "\\Demo.txt"))
            {
                var date = DateOnly.FromDateTime(DateTime.Now);
                w.Write($"{date}");
            }

            Console.WriteLine("Din proeveperiode på 6 maaneder er nu begyndt, held og lykke");
        }
        catch (Exception e)
        {
            var logger = new LoggingService<DemoController>();
            logger.CreateError(e.Message);
            Console.ReadKey();
            throw;
        }

        return true;
    }

    public DateOnly GetDemoStartDate()
    {
        return new DateOnly();
    }
}