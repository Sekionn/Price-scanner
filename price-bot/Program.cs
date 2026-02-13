using NPOI.HPSF;
using price_bot.Demo_feature;
using price_bot.FileWriter;
using price_bot.Logging;
using price_bot.Models;
using price_bot.Networking;
using price_bot.Updater;
using System.Diagnostics;
using Velopack;
using price_bot.Verification;
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



        VelopackApp.Build().Run();
#if !DEBUG
        bool updateReady = await VelopackUpdaterService.CheckForUpdates();

        if (updateReady)
        {
            Console.WriteLine("We are updating...");
            await VelopackUpdaterService.ApplyUpdate();
        } 
        else 
        {
#endif

        var verified = await WaitForVerification();
        if (verified)
        {
            await RunProgram(versionRetreiver, demoController);
        }
        else
        {
            Console.ReadKey();
        }
#if !DEBUG
        }
#endif

    }

    private static async Task<bool> WaitForVerification()
    {
        License? license = VerificationService.GetLicenseData();
        LicenseVerifier licenseVerifier = new LicenseVerifier();
        bool licenseChecked = false;
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
                    licenseChecked = true;
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

        if (!licenseChecked) 
        {
            var response = await licenseVerifier.VerifyLicenseAsync(license);

            if (response == null)
            {
                Console.WriteLine("Your license was not found, contact sebastian for help");
                license = null;
            }
        }

        if (license != null)
        {
            var response = await licenseVerifier.GetActiveStatus(license);

            if (!response.HasValue)
            {
                return false;
            }
            else
            {
                if (response.Value == true)
                {
                    Console.WriteLine("Your license is currently in use elsewhere.");
                }

                return !response.Value;
            }
        }

        return false;
    }

    private static async Task RunProgram(VersionRetreiver versionRetreiver, DemoController demoController)
    {
        await VerificationService.ActivateLicense();

        var logger = new LoggingService<Program>();
        string command = "";
        while (!command!.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
        {
            Console.Clear();

            if (!versionRetreiver.IsDemoVersion() || !demoController.HasDemoFinished())
            {
                Console.WriteLine("Vil du gerne have tjekket bog og ide's priser?");
                Console.WriteLine("Du har nu følgende kommandoer:");
                Console.WriteLine("start (for at starte programmet)");
                Console.WriteLine("excel (for at tilføje en ny excel fil)");
                Console.WriteLine("forkert (åbner mappen hvor den generede fil med de forkerte priser ligger)");
                Console.WriteLine("exit (dette vil slukke programmet)");

                command = Console.ReadLine()!;
                if (command == "exit") 
                {
                    break;
                }
            }
            else
            {
                Console.WriteLine("Din prøveperiode er udløbet kontakt Sebastian for at købe programmet");
                command = "exit";
            }

            if (command != null)
            {
                var files = Directory.GetFiles(Path.GetFullPath(@"Filer"), "*.xlsx").Length;

                while (files <= 0 || command!.Equals("excel", StringComparison.CurrentCultureIgnoreCase))
                {
                    Process.Start(new ProcessStartInfo { FileName = Path.GetFullPath(@"Filer"), UseShellExecute = true });

                    Console.WriteLine("Du kan nu tilføje din excel fil, når dette er gjort kan du skrive start for at starte programmet");
                    command = Console.ReadLine()!;

                    files = Directory.GetFiles(Path.GetFullPath(@"Filer"), "*.xlsx").Length;
                }

                if (command.Equals("forkert", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (File.Exists(Path.GetFullPath("Forkerte priser\\Forkerte priser.xls")))
                    {
                        Process.Start(new ProcessStartInfo { FileName = Path.GetFullPath(@"Forkerte priser"), UseShellExecute = true });
                    }
                    else
                    {
                        Console.WriteLine("På nuværende tidspunkt er der ikke nogen forkert pris fil tilgængelig");
                        Console.WriteLine("for at lave om på det kan du bruge kommandoen 'start'");
                        Thread.Sleep(100);
                    }
                }
                else if (command.Equals("start", StringComparison.CurrentCultureIgnoreCase))
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

                    Process.Start(new ProcessStartInfo { FileName = Path.GetFullPath(@"Forkerte priser"), UseShellExecute = true });
                    Process.Start(new ProcessStartInfo { FileName = Path.GetFullPath(@"Forkerte priser\Forkerte priser.xls"), UseShellExecute = true });

                    DateTime finishDate = DateTime.Now;

                    logger.CreateLog($"Start tidspunkt: {startDate}");
                    logger.CreateLog($"Slut tidspunkt: {finishDate}");
                    logger.CreateLog($"Hvor lang tid tog det: {finishDate - startDate}");
                }
            }
        }
        await VerificationService.DeactivateLicense();
        Console.WriteLine("Farvel");
        Thread.Sleep(100);
    }
}