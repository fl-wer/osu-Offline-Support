using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Offline_Support
{
    public partial class ApiKey : Form
    {
        // initialize, ignore this
        public ApiKey() { InitializeComponent(); }

        // path where all INF files are stored
        string configFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
            "\\fl-wer\\Offline Suport\\";

        // pc documents path, used for all INF files
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private void ApiKey_Load(object sender, EventArgs e)
        {
            // disable cross thread actions flag
            CheckForIllegalCrossThreadCalls = false;

            // if software folder doesn't exist = make it
            if (!Directory.Exists(configFilesPath))
                Directory.CreateDirectory(configFilesPath);
        }

        // open osu website with api key if user clicks on "osu! api key" label
        private void keyLinkLab_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { Process.Start("https://github.com/ppy/osu-api/wiki"); }

        // encrypt and save key to a file + restart app for easier settings saving / reading
        private void keySubmitBtn_Click(object sender, EventArgs e)
        {
            // simple string length checks to make sure there's no rubbish being entered there
            if (keyTextBox.Text.Length < 50 && keyTextBox.Text.Length > 30)
            {
                // encrypted api key that will be written to the key file
                string toWrite = Crypto.Encrypt(keyTextBox.Text, "7b03b040f85b47419e2ba3ad2630897f"); // encryption with key

                // write encrypted key to the file
                File.WriteAllText(configFilesPath + Main.apiKeyINF, toWrite);

                // restart app to re-read apiKey file and obviously it won't find it on next start and ask user for it again
                Process.Start(AppDomain.CurrentDomain.FriendlyName);
                Application.Exit();
            }
            // string length too weird to accept, probably user fucked up here
            else Main.showError("Incorrect key.");
        }

        // if form is being closed then it just closes the app, key has to be submitted with SUBMIT button
        private void ApiKey_FormClosed(object sender, FormClosedEventArgs e) { Application.Exit(); }
    }
}
