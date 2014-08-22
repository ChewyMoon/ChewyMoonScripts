using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChewyMoonsLux
{
    class LuxUpdater
    {
        public static void CheckForUpdates()
        {
            //TODO
        }
    }

    internal class Updater
    {
        private readonly string _updatelink;

        private readonly WebClient _wc = new WebClient { Proxy = null };
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

                File.Move(Assembly.GetExecutingAssembly().Location, Path.Combine(Assembly.GetExecutingAssembly().Location) + ".bak");
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
