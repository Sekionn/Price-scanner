using Microsoft.Extensions.Logging;
using price_bot.Enums;

namespace price_bot.Logging;
public class LoggingService<TClass>
{

    public LoggingService() 
    {
        Directory.CreateDirectory("Logs");
    }

    public void CreateLog(string log)
    {
        LogWrite(log, LogType.Information);
    }

    public void CreateError(string error)
    {
        LogWrite(error, LogType.Error);
    }

    public void LogWrite(string logMessage, LogType logType)
    {
        try
        {
            using (StreamWriter w = File.AppendText(Path.Combine(@"Logs",logType == LogType.Information ? "log.txt" : "Error.txt")))
            {
                Log(logMessage, w, logType);
            }
        }
        catch (Exception ex)
        {
            Console.ReadKey();
        }
    }

    public void Log(string logMessage, TextWriter txtWriter, LogType logType)
    {
        try
        {
            txtWriter.Write($"\r\n{(logType == LogType.Information ? "log" : "error")} Entry : ");
            txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            txtWriter.WriteLine("  :");
            txtWriter.WriteLine("  :{0}", logMessage);
            txtWriter.WriteLine("-------------------------------");
        }
        catch (Exception ex)
        {
            Console.ReadKey();
        }
    }
}