using price_bot.Logging;
using price_bot.Models;
using System.Text.Json;

namespace price_bot.Networking;

public class LicenseVerifier : BaseClient
{
    string baseUrl = "https://price-bot.juuls-trinkets.com/";
    LoggingService<LicenseVerifier> _logger = new LoggingService<LicenseVerifier>();

    public async Task<Guid?> VerifyLicenseAsync(License license)
    {
        string response = await Post(baseUrl + $"shop/verified", license);
        try
        {
            return JsonSerializer.Deserialize<Guid?>(response);
        }
        catch (Exception e)
        {
            _logger.CreateError(e.Message);

            return null;
        }
        
    }

    public async Task<bool?> GetActiveStatus(License license)
    {
        string response = await Get(baseUrl + $"shop/active/{license.ShopId}");

        try
        {
            return JsonSerializer.Deserialize<bool?>(response);
        }
        catch (Exception e)
        {
            _logger.CreateError(e.Message);

            Console.WriteLine("Your license was not found, so could not check if it is active");
            Console.ReadKey();
            return null;
        }
    }

    public async Task<int> ActivatOrDeactivateLicense(License license, bool inUse)
    {
        string response = await Patch(baseUrl + $"shop/active", new { id = license.ShopId, in_use = inUse });

        try
        {
            return JsonSerializer.Deserialize<int>(response);
        }
        catch (Exception e)
        {
            _logger.CreateError(e.Message);

            Console.WriteLine("Noget gik galt så licensen er ikke ikke blevet aktiveret eller deaktiveret");
            Console.WriteLine("Dette kan betyde at du ikke kan aktivere programmet næste gang du starter det");
            Console.WriteLine("Kontakt Sebastian for hjælp");
            Console.ReadKey();
            return 0;
        }
    }
}