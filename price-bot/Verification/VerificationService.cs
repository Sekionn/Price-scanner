using price_bot.FileReaders;
using price_bot.Models;

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

    public static int VerifyLicense()
    {
        var license = GetLicenseData();

        return 1;
    }
}