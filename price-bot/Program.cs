﻿using price_bot.FileWriter;
using price_bot.Networking;
internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Vil du gerne have tjekket bog og ide's priser?");
        Console.WriteLine("Skriv 'Ja' for at starte programmet");

        string? command = Console.ReadLine();

        if (command != null)
        {
            if (command.Equals("ja", StringComparison.CurrentCultureIgnoreCase))
            {
                bool timeNotSet = true;
                int delayInHours = 0;
                while (timeNotSet)
                {
                    Console.WriteLine("Hvor mange timer skal programmet vente før den starter?");
                    Console.WriteLine("skriv et tal");
                    var selectedDelay = Console.ReadLine();

                    if (selectedDelay != null)
                    {
                        if (int.TryParse(selectedDelay, out int r) == true)
                        {
                            delayInHours = r;
                            timeNotSet = false;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Tallet er accepteret, systemet venter nu i {delayInHours} time(r)");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.WriteLine("Det der blev tastet var ikke et gyldigt tal, prøv igen");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.BackgroundColor = ConsoleColor.Black;

                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine("Der er ikke tastet noget tal i inputtet");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                }

                var countdown = new price_bot.Timer.Timer(delayInHours);

                await countdown.Countdown();

                DateTime startDate = DateTime.Now;

                ProductRetreiver productRetreiver = new();
                var incorrectProducts = await productRetreiver.GetProductsWithIncorrectPrices();

                DirectoryInfo di = Directory.CreateDirectory("Forkerte priser");
                Console.WriteLine("The directory was created successfully at {0}.",
                    Directory.GetCreationTime("Forkerte priser"));

                FileWriter fileWriter = new();
                await fileWriter.WriteTXTFile(incorrectProducts);
                await fileWriter.WriteExcelFile(incorrectProducts);

                DateTime finishDate = DateTime.Now;

                Console.WriteLine($"Start tidspunkt: {startDate}");
                Console.WriteLine($"Slut tidspunkt: {finishDate}");
                Console.WriteLine($"Hvor lang tid tog det: {finishDate - startDate}");
            }
        }

        Console.WriteLine("Bye");
        Console.ReadLine();
    }
}