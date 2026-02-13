using price_bot.FileReaders;
using price_bot.Models;
using price_bot.Networking;

namespace price_bot.Verification;
public class VerificationService
{
    public static async Task UpdateLicenseData(License data)
    {
        await FileWriter.FileWriter.WriteJSONFile(data, "license");

        return;
    }

    public static License? GetLicenseData()
    {
        return FileReader.ReadJson<License>("license");
    }

    public static async Task<int> ActivateLicense()
    {
        var license = GetLicenseData();
        if (license == null)
        {
            return 0;
        }
        LicenseVerifier licenseVerifier = new();

        return await licenseVerifier.ActivatOrDeactivateLicense(license, true);
    }

    public static async Task<int> DeactivateLicense()
    {
        var license = GetLicenseData();
        if (license == null)
        {
            return 0;
        }
        LicenseVerifier licenseVerifier = new();

        return await licenseVerifier.ActivatOrDeactivateLicense(license, false);
    }
}