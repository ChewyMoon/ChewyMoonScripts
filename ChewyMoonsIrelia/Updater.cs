using System.ComponentModel;
using System.Net;

namespace ChewyMoonsIrelia
{
    class Updater
    {
        public static readonly string VersionUrl = "https://raw.githubusercontent.com/ChewyMoon/ChewyMoonScripts/master/ChewyMoonsIrelia/Version/version.txt";
        public static readonly string Version = "1.0-RELEASE";

        public static void CheckForUpdates()
        {
            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += delegate(object sender, DoWorkEventArgs args)
            {
                var webClient = new WebClient();
                var newVersion = webClient.DownloadString(VersionUrl);

                if (newVersion != Version)
                {
                    Utilities.PrintChat("A new update is available (" + newVersion + ")! Please update.");
                }
                else
                {
                    Utilities.PrintChat("You have the latest version ( " + Version + ")");
                }

            };
            bgWorker.RunWorkerAsync();
        }
    }
}
