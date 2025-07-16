﻿using Microsoft.Extensions.Logging;
using price_bot.Enums;

namespace price_bot.Logging;
public class LoggingService<TClass>
{
    private readonly ILogger<TClass> _logger;

    public LoggingService() 
    {
        DirectoryInfo di = Directory.CreateDirectory("Logs");
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        _logger = loggerFactory.CreateLogger<TClass>();
        
    }

    public void CreateLog(string log)
    {
        _logger.LogInformation(log);
        LogWrite(log, LogType.Information);
    }

    public void CreateError(string error)
    {
        _logger.LogError(error);
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