using Velopack;
using Velopack.Sources;

namespace price_bot.Updater
{
    public class VelopackUpdaterService
    {
        const string updatePath = "https://github.com/Sekionn/Price-scanner";

        public static async Task<bool> CheckForUpdates()
        {
            var mgr = new UpdateManager(new GithubSource(updatePath, null, false));

            // check for new version
            var newVersion = await mgr.CheckForUpdatesAsync();
            if (newVersion == null)
                return false; // no update available

            return true;
        }

        public static async Task ApplyUpdate()
        {
            var mgr = new UpdateManager(new GithubSource(updatePath, null, false));

            // check for new version
            var newVersion = await mgr.CheckForUpdatesAsync();
            if (newVersion == null)
                return; // no update available

            // download new version
            await mgr.DownloadUpdatesAsync(newVersion);

            // install new version and restart app
            mgr.ApplyUpdatesAndRestart(newVersion);
        }
    }
}