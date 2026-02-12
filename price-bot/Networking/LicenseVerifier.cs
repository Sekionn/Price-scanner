using price_bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace price_bot.Networking;

public class LicenseVerifier : BaseClient
{
    string baseUrl = "https://localhost:443/";

    public async Task<Guid> VerifyLicenseAsync(License license)
    {
        string response = await Post(baseUrl + $"shop/verified", license);

        return JsonSerializer.Deserialize<Guid>(response);
    }
}