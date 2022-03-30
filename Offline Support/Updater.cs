using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Offline_Support
{
    class Updater
    {
        // create client, read string with version from my server and then when read
        // it will run the event function "versionReadComplete" and will have the read version
        // it's doing it like this with events so it could run in the background
        // and not freeze the app if server dies or user's internet dies
        public static void checkForUpdates()
        {
            // using "try" in case connection drops and exception gets thrown
            try
            {
                // enabling TLS as it's required for connection with github
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                WebClient webCl = new WebClient();
                webCl.DownloadStringCompleted += new DownloadStringCompletedEventHandler(versionReadComplete);
                webCl.DownloadStringAsync(new Uri("https://raw.githubusercontent.com/fl-wer/osu-Offline-Support/main/VERSION"));
            }
            catch { }
        }

        // this launches when version number is finally read from the server
        static void versionReadComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            // using "try" in case connection drops and exception gets thrown
            try
            {
                // if current version is not the latest version
                if (e.Result != Main.softwareVersion)
                {
                    // message box pops up and asks if it should open downlod page
                    DialogResult dialogResult = MessageBox.Show("New version available, open download link?", "Update", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    // it will open github page with relese download link for new version
                    if (dialogResult == DialogResult.Yes) Process.Start("https://github.com/fl-wer/osu-Offline-Support");
                }
            }
            catch { }
        }
    }
}
