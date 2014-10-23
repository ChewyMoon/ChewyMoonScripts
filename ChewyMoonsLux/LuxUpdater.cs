#region

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;

#endregion

namespace ChewyMoonsLux
{
    internal class LuxUpdater
    {
        public static readonly string VersionUrl =
            "https://raw.githubusercontent.com/ChewyMoon/ChewyMoonScripts/master/ChewyMoonsLux/Version/version.txt";

        public static readonly string Version = "1.2";

        public static readonly string UpdateUrl =
            "https://github.com/ChewyMoon/ChewyMoonScripts/raw/master/Releases/ChewyMoonsLux.exe";

        public static void CheckForUpdates()
        {
            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += delegate
            {
                var updater = new Updater(VersionUrl, UpdateUrl, Version);

                if (updater.NeedUpdate)
                {
                    if (updater.Update())
                    {
                        //Success
                        Utilities.PrintChat("Sucessfully updated to version " + updater.UpdateVersion + "!");
                    }
                    else
                    {
                        Utilities.PrintChat("Update failed.");
                    }
                }
                else
                {
                    Utilities.PrintChat("You have the lastest version (" + Version + ")");
                }
            };
            bgWorker.RunWorkerAsync();
        }
    }

    internal class Updater
    {
        private readonly string _updatelink;

        private readonly WebClient _wc = new WebClient {Proxy = null};
        public bool NeedUpdate = false;
        public string UpdateVersion;

        public Updater(string versionlink, string updatelink, string localversion)
        {
            _updatelink = updatelink;
            UpdateVersion = _wc.DownloadString(versionlink);
            NeedUpdate = UpdateVersion != localversion;
        }

        public bool Update()
        {
            try
            {
                // ReSharper disable PossiblyMistakenUseOfParamsMethod
                if (File.Exists(Path.Combine(Assembly.GetExecutingAssembly().Location + ".bak")))
                {
                    File.Delete(Path.Combine(Assembly.GetExecutingAssembly().Location) + ".bak");
                }

                File.Move(Assembly.GetExecutingAssembly().Location,
                    Path.Combine(Assembly.GetExecutingAssembly().Location) + ".bak");
                _wc.DownloadFile(_updatelink, Path.Combine(Assembly.GetExecutingAssembly().Location));
                // ReSharper restore PossiblyMistakenUseOfParamsMethod
                return true;
            }
            catch (Exception ex)
            {
                Utilities.PrintChat("Error while trying to update. Message: " + ex.Message);
                return false;
            }
        }
    }
}