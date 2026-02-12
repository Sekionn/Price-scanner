using price_bot.Demo_feature;
using price_bot.FileWriter;
using price_bot.Logging;
using price_bot.Models;
using price_bot.Networking;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var versionRetreiver = new VersionRetreiver();
        var demoController = new DemoController();

        if (versionRetreiver.IsDemoVersion())
        {

            if (!demoController.HasDemoStarted())
            {
                demoController.StartDemo();
            }
            
        }

        var logger = new LoggingService<Program>();

        License? license = VerificationService.GetLicenseData();
        LicenseVerifier licenseVerifier = new LicenseVerifier();

        while (license == null)
        {
            Console.Clear();
            Console.WriteLine("Der er ikke fundet en licens tilknyttet denne installation.");
            Console.WriteLine("Hvis du har en kan du indtaste den nu, ellers kontakt udvikleren af systemet.");
            Console.WriteLine("Indtast email:");

            var email = Console.ReadLine();

            Console.WriteLine("Indtast licens noeglen:");

            var key = Console.ReadLine();
            if (email != null && key != null)
            {
                License tempLicense = new(email, key);

                tempLicense.ShopId = await licenseVerifier.VerifyLicenseAsync(tempLicense);

                if (tempLicense.ShopId.HasValue)
                {
                    license = tempLicense;

                    await VerificationService.UpdateLicenseData(license);

                    Console.WriteLine("Denne licens er nu verificeret og du har nu adgang til systemet god fornoejelse.");
                }
                else
                {
                    Console.WriteLine("Email eller licens noegle er ikke indtastet korrekt proev igen");
                }

            }
            else
            {
                Console.WriteLine("Email eller licens noegle er ikke indtastet korrekt proev igen");
            }
        }

        Console.Clear();

        string? command = null;

        if (!versionRetreiver.IsDemoVersion() || !demoController.HasDemoFinished())
        {
            Console.WriteLine("Vil du gerne have tjekket bog og ide's priser?");
            Console.WriteLine("Skriv 'Ja' for at starte programmet");

            command = Console.ReadLine();
        }
        else
        {
            Console.WriteLine("Din proeveperiode er udloebet kontakt Sebastian for at koebe programmet");
        }

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
                logger.CreateLog($"The directory was created successfully at {Directory.GetCreationTime("Forkerte priser")}.");

                FileWriter.WriteTXTFile(incorrectProducts);
                FileWriter.WriteExcelFile(incorrectProducts);

                DateTime finishDate = DateTime.Now;

                logger.CreateLog($"Start tidspunkt: {startDate}");
                logger.CreateLog($"Slut tidspunkt: {finishDate}");
                logger.CreateLog($"Hvor lang tid tog det: {finishDate - startDate}");
            }
        }

        Console.WriteLine("Farvel");
        Console.ReadLine();
    }
}