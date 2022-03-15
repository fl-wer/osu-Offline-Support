using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Media;

namespace Offline_Support
{
    public partial class Main : Form
    {
        // ### functions imported from windows dll and turned into c#

        // puts found address to provided variable
        // returns true or false whether it found the address or not
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
        IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesRead);

        // puts found address to provided variable
        // returns true or false whether it found the address or not
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
        IntPtr hProcess, IntPtr lpBaseAddress, out IntPtr lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesRead);

        // initialize, ignore this
        public Main() { InitializeComponent(); }

        // build version
        public static string softwareVersion = "4";

        // information file names, these are files that store all
        // kind of information and are saved in software default folder in variable below
        // all those files will have "INF" at the end so you know these
        public static string apiKeyINF = "93q2qp";
        public static string alwaysOnTopINF = "p44gz4";
        public static string ignoreUpdatesINF = "ev26p4";

        // folder that holds all information files, it's in %TEMP%
        string softwareFolder = "19db20f2-b775-420b-9668-02b08bd50fbc";

        string apiKey = ""; // holds encrypted api key that is being used to call requests to osu! api
        bool apiKeyExists = false; // being used to check if key file exists, meaning if key is saved and if key is inside too

        // background will work on different things depending on number for backgroundWorkerStage
        // every stage will check if game is still running every now and then
        // code is supporting unexpected stage degradation, eg from 3 to 1
        // 1 = looking for game process, first time running osu or maybe it closed
        // 2 = find signature addresses and then pointers out of those addresses
        // 3 = read game data using pointers and if map changed read map's data and display
        byte backgroundWorkerStage = 1;

        // scores displayed per page on the form, maybe there will be an option to
        // get this changed in the future so I'm putting it here for below
        static byte scoresPerPage = 8;

        // chosen page that wi0ll be changed using keys, eg page 1 = scores from 1 to 8, page 2 = scores from 9 to 17
        static byte globalLeaderboardPage = 1;

        // these are arrays that hold each element on the form so it's easier to manage in code
        // they are being initialized in function called initializeTools()
        PictureBox[] profilePicForm = new PictureBox[scoresPerPage];
        PictureBox[] rankForm = new PictureBox[scoresPerPage];
        Label[] usernameForm = new Label[scoresPerPage];
        Label[] scoreForm = new Label[scoresPerPage];
        Label[] modsForm = new Label[scoresPerPage];
        Label[] accForm = new Label[scoresPerPage];
        Label[] ppForm = new Label[scoresPerPage];

        // loaded scores from json parsing function
        BeatmapScore[] parsedScores;

        // holds last mapId, lastMods, this is to check if it changed by comparing read id and last id
        int lastMapId = 0; int lastMods = 0; int lastGameMode = 0;

        // indicates if we're requesting for mod leaderboards and not normal one
        bool modLeaderboard = false;

        // ### hotkey triggered functions
        // ***
        void hotKeyPreviousPage(object sender, HotKeyEventArgs e)
        {
            // if key is right arrow (event trigger checks for shift)
            if (e.Key == Keys.Left)
            {
                // maximum page based on scores per page and 50 default output scores from osu
                int highestPage = (int)Math.Ceiling((double)50 / scoresPerPage);

                // making sure we're not going too high in pages
                if (globalLeaderboardPage > 1)
                {
                    // if key is left arrow (event trigger checks for shift)
                    if (e.Key == Keys.Left)
                    {
                        // changing page used for form assignment
                        globalLeaderboardPage -= 1;

                        // reloading scores into form using global leaderboard page
                        assignScoresToForm(parsedScores, globalLeaderboardPage);

                        // changing form control to display current page
                        currentPage.Text = "PAGE " + globalLeaderboardPage + "/" + highestPage;
                    }
                }
            }
        }

        // goes to next page and reloads forms with shift + right arrow
        void hotKeyNextPage(object sender, HotKeyEventArgs e)
        {
            // maximum page based on scores per page and 50 default output scores from osu
            int highestPage = (int)Math.Ceiling((double)50 / scoresPerPage);

            // if key is right arrow (event trigger checks for shift)
            if (e.Key == Keys.Right)
            {
                // making sure we're not going too high in pages
                if (globalLeaderboardPage < highestPage)
                {
                    // changing page used for form assignment
                    globalLeaderboardPage += 1;

                    // reloading scores into form using global leaderboard page
                    assignScoresToForm(parsedScores, globalLeaderboardPage);

                    // changing form control to display current page
                    currentPage.Text = "PAGE " + globalLeaderboardPage + "/" + highestPage;
                }
            }
        }

        void hotKeyEnableModLeaderboard(object sender, HotKeyEventArgs e)
        {
            // if key is down arrow (event trigger checks for shift)
            if (e.Key == Keys.Down)
            {
                // changing bool to opposite
                modLeaderboard = !modLeaderboard;

                // changing text on the form accordignly to mod leaderboard change
                if (modLeaderboard)
                {
                    logMessage.Text = "MOD LEADERBOARD ENABLED";
                    logMessage.ForeColor = Color.LimeGreen;
                }
                else
                {
                    logMessage.Text = "MOD LEADERBOARD DISABLED";
                    logMessage.ForeColor = Color.Red;
                }

                // reloading leaderboard by changing last map id
                // it will think that current id is different from current one
                // and will look for map leaderboard again
                lastMapId = 0;
            }
        }

        // runs once on startup
        private void Main_Load(object sender, EventArgs e)
        {
            // disable cross thread actions flag
            CheckForIllegalCrossThreadCalls = false;

            // hide scores with curtain, don't show until they load
            hideScores();

            // registering key binds for some functions
            HotKeyManager.RegisterHotKey(Keys.Left, KeyModifiers.Shift);
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(hotKeyPreviousPage);

            HotKeyManager.RegisterHotKey(Keys.Right, KeyModifiers.Shift);
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(hotKeyNextPage);

            HotKeyManager.RegisterHotKey(Keys.Down, KeyModifiers.Shift);
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(hotKeyEnableModLeaderboard);

            // if software folder doesn't exist = make it
            if (!Directory.Exists(Path.GetTempPath() + softwareFolder))
                Directory.CreateDirectory(Path.GetTempPath() + softwareFolder);

            // ### load information files + settings
            // load always on top saved option
            if (File.Exists(Path.GetTempPath() + softwareFolder + "\\" + alwaysOnTopINF) &&
            File.ReadAllText(Path.GetTempPath() + softwareFolder + "\\" + alwaysOnTopINF) == "True")
            {
                // disable always on top for the software
                TopMost = !TopMost;

                // change checkbox for menu strip option
                ((ToolStripMenuItem)quickSupportContextMenuStrip.Items[0]).Checked = !((ToolStripMenuItem)quickSupportContextMenuStrip.Items[0]).Checked;
            }
            // else it will just stay on default which would just be False anyway

            // loading ignore updates INF file and changing context menu strip accordingly
            string tempUpdatesINFPath = Path.GetTempPath() + softwareFolder + "\\" + ignoreUpdatesINF;
            if (File.Exists(tempUpdatesINFPath))
            {
                // ignore updates enabled and saved in options therefore checkbox will be ticked
                if (File.ReadAllText(tempUpdatesINFPath) == "True") ((ToolStripMenuItem)quickSupportContextMenuStrip.Items[1]).Checked = true;
                else ((ToolStripMenuItem)quickSupportContextMenuStrip.Items[1]).Checked = false;
            }
            else ((ToolStripMenuItem)quickSupportContextMenuStrip.Items[1]).Checked = false;

            // check if there's new version of software available before doing anything
            // if there is a new version it will open new message box with two options - yes, no
            // if you click yes it will open github link with downloads for releases
            // checking status of "ignore updates" option (from context strip menu)
            // previously updated by file reading above
            if (!((ToolStripMenuItem)quickSupportContextMenuStrip.Items[1]).Checked)
                Updater.checkForUpdates();

            // check if api key saved and load it if so
            if (File.Exists(Path.GetTempPath() + softwareFolder + "\\" + apiKeyINF))
            {
                // temporarily holds encrypted key
                string tempApiKey = File.ReadAllText(Path.Combine(Path.GetTempPath() + softwareFolder + "\\" + apiKeyINF));

                // simple checks to make sure there was something inside that api key file
                if (tempApiKey != "" && tempApiKey != null)
                {
                    apiKey = tempApiKey; // assigning to main variable for further use
                    apiKeyExists = true; // bool for checks to confirm key was saved and found sound
                }
            }

            // open window looking for key if file not found or key not inside the file
            if (!apiKeyExists) { ApiKey keyForm = new ApiKey(); keyForm.ShowDialog(); }
            else // else key was found and saved, start loop lookjing for map leaderboards and everything else
            {
                // initialize form arrays, assign variables with forms from designer
                initializeTools();

                // start background worker tool, everything will be working there now
                backgroundWorker.RunWorkerAsync();
            }
        }

        // change checkbox for menu strip on always on top and change always on top option
        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // disable always on top for the software, this variable is global
            TopMost = !TopMost;

            // change checkbox for menu strip option
            ((ToolStripMenuItem)quickSupportContextMenuStrip.Items[0]).Checked = !((ToolStripMenuItem)quickSupportContextMenuStrip.Items[0]).Checked;

            // save always on top option to information file so user won't have to change this every time they open program
            if (TopMost) File.WriteAllText(Path.GetTempPath() + softwareFolder + "\\" + alwaysOnTopINF, "True");
            else File.WriteAllText(Path.GetTempPath() + softwareFolder + "\\" + alwaysOnTopINF, "False");
        }

        // button on menu strip that removes key file and restarts app allowing user to type in new key
        private void resetAPIKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // check if api key saved and if so delete the file for key refresh
            if (File.Exists(Path.GetTempPath() + softwareFolder + "\\" + apiKeyINF))
                File.Delete(Path.GetTempPath() + softwareFolder + "\\" + apiKeyINF);

            // restart app to re-read apiKey file and obviously it won't find it on next start and ask user for it again
            Process.Start(AppDomain.CurrentDomain.FriendlyName);
            Application.Exit();
        }

        // button on menu strip that changesif software should look for update or just ignore it
        private void ignoreUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // change checkbox for menu strip option
            ((ToolStripMenuItem)quickSupportContextMenuStrip.Items[1]).Checked = !((ToolStripMenuItem)quickSupportContextMenuStrip.Items[1]).Checked;

            // save checkbox status for reread on next run
            File.WriteAllText(Path.GetTempPath() + softwareFolder + "\\" + ignoreUpdatesINF,
            ((ToolStripMenuItem)quickSupportContextMenuStrip.Items[1]).Checked.ToString());
        }

        // show custom message box with warning or error icon
        public static void showError(string message) { MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        public static void showInfo(string message) { MessageBox.Show(message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); }

        // check if process is running or not, returns true or false accordingly
        bool isProcessOpen(string processName)
        {
            // going through all open processes
            foreach (Process process in Process.GetProcesses())
            {
                // if found osu assign process to Process variable and go to next stage
                if (process.ProcessName == processName) return true;
            }

            // if went through all processes and process not found returns false
            return false;
        }

        // shows or hides scores by moving big label with background to cover them or to move away and show them
        // inLoadingState indicates if we're waiting for osu website to get back to us
        // during that time we shouldn't be able to change pages
        void showScores() { scoresCurtain.Location = new Point(30000, -298); }
        void hideScores() { scoresCurtain.Location = new Point(-87, -298); }

        // reads map id memory by reading multi level pointer
        int readMapIdMemory(IntPtr pointer, int offset, IntPtr gameProcessHandle)
        {
            // this is actually the real pointer grabbed from game function
            IntPtr firstRead = (IntPtr)0;

            if (ReadProcessMemory(gameProcessHandle, pointer, out firstRead, 0x04, out IntPtr qn5jc5))
            {
                // second read is the address pointer is pointing at
                IntPtr secondRead = (IntPtr)0;

                if (ReadProcessMemory(gameProcessHandle, firstRead, out secondRead, 0x04, out IntPtr v3j3j4))
                {
                    // third read is the address pointer is actual map id
                    IntPtr thirdRead = (IntPtr)0;

                    // adding offset to pointer, offset is the same for all signatures for map id
                    secondRead += offset;

                    // getting last value = map id
                    if (ReadProcessMemory(gameProcessHandle, secondRead, out thirdRead, 0x04, out IntPtr ndp5k7))
                        return (int)thirdRead;
                }
            }

            // returns 0 if it couldn't read the memory
            return 0;
        }

        // reads enabled mods memory by reading multi level pointer
        int readEnabledModsMemory(IntPtr pointer, IntPtr gameProcessHandle)
        {
            // this is actually the real pointer grabbed from game function
            IntPtr firstRead = (IntPtr)0;

            // actual enabled mods after extracting real pointer (above variable)
            IntPtr enabledMods = (IntPtr)0;

            // reading through signature address and then through extracted address that holds mods
            if (ReadProcessMemory(gameProcessHandle, pointer, out firstRead, 0x04, out IntPtr qn5jc5))
            {
                if (ReadProcessMemory(gameProcessHandle, firstRead, out enabledMods, 0x04, out IntPtr eqy7wj))
                    return (int)enabledMods;
            }

            // reading memory failed for some reason and returning 0
            return 0;
        }

        // reads game modes by reading multi level pointer
        // reads enabled mods memory by reading multi level pointer
        int readGameModeMemory(IntPtr pointer, IntPtr gameProcessHandle)
        {
            // this is actually the real pointer grabbed from game function
            IntPtr firstRead = (IntPtr)0;

            // actual enabled mods after extracting real pointer (above variable)
            IntPtr gameMode = (IntPtr)0;

            // reading through signature address and then through extracted address that holds mods
            if (ReadProcessMemory(gameProcessHandle, pointer, out firstRead, 0x04, out IntPtr qn5jc5))
            {
                if (ReadProcessMemory(gameProcessHandle, firstRead, out gameMode, 0x04, out IntPtr eqy7wj))
                {
                    // checking if correct address was used and correct value gathered
                    // there are only game modes from 0 to 4
                    if ((int)gameMode > -1 && (int)gameMode < 4) return (int)gameMode;
                    else return 0;
                }
            }

            // reading memory failed for some reason and returning 0
            return 0;
        }

        // initialize form arrays, assign variables with forms from designer
        void initializeTools()
        {
            for (int i = 0; i < scoresPerPage; i++)
            {
                profilePicForm[i] = (PictureBox)Controls.Find("profilePic" + i.ToString(), true)[0];
                rankForm[i] = (PictureBox)Controls.Find("rank" + i.ToString(), true)[0];
                usernameForm[i] = (Label)Controls.Find("username" + i.ToString(), true)[0];
                scoreForm[i] = (Label)Controls.Find("score" + i.ToString(), true)[0];
                modsForm[i] = (Label)Controls.Find("mods" + i.ToString(), true)[0];
                accForm[i] = (Label)Controls.Find("acc" + i.ToString(), true)[0];
                ppForm[i] = (Label)Controls.Find("pp" + i.ToString(), true)[0];
            }
        }

        // it's reading scores through and assigning info to form controls
        // leaderboard page is a form page, because we only see x scores at a time
        // so we can use keybinds to move between "pages" first 8 plays, next 8 plays etc
        void assignScoresToForm(BeatmapScore[] scores, byte leaderboardPage)
        {
            // this is used for form arrays, they will always start from 0, they're not like scores
            int formCounter = 0;

            // remove 1 from leaderboard page so when u're using function you can use page 1 instead of page 0
            leaderboardPage -= 1;

            // going through all of the scores that we have and assigning to form controls
            for (int i = leaderboardPage * scoresPerPage; i < (scoresPerPage + (leaderboardPage * scoresPerPage)); i++)
            {
                // checking if there are scores to load still in the array
                if (i < scores.Length)
                {
                    // changing visible form texts to returned parsed json score info
                    usernameForm[formCounter].Text = scores[i].username;
                    scoreForm[formCounter].Text = scores[i].score;
                    modsForm[formCounter].Text = scores[i].enabled_mods;

                    // if there are misses, display them next to accuracy, looks ok and it's extra info
                    if (scores[i].countmiss != "0") accForm[formCounter].Text = scores[i].countmiss + "x " + scores[i].accuracy;
                    else accForm[formCounter].Text = scores[i].accuracy;

                    ppForm[formCounter].Text = scores[i].pp;

                    // changing rank image according to the rank of the score
                    if (scores[i].rank == "XH") rankForm[formCounter].Image = Properties.Resources.rankXH;
                    else if (scores[i].rank == "X") rankForm[formCounter].Image = Properties.Resources.rankX;
                    else if (scores[i].rank == "SH") rankForm[formCounter].Image = Properties.Resources.rankSH;
                    else if (scores[i].rank == "S") rankForm[formCounter].Image = Properties.Resources.rankS;
                    else if (scores[i].rank == "A") rankForm[formCounter].Image = Properties.Resources.rankA;
                    else if (scores[i].rank == "B") rankForm[formCounter].Image = Properties.Resources.rankB;
                    else if (scores[i].rank == "C") rankForm[formCounter].Image = Properties.Resources.rankC;
                    else if (scores[i].rank == "D") rankForm[formCounter].Image = Properties.Resources.rankD;

                    // profile picture by reading it from osu api with user_id
                    if (leaderboardPage == 0) profilePicForm[formCounter].ImageLocation = "https://a.ppy.sh/" + scores[i].user_id;
                    else profilePicForm[formCounter].Image = Properties.Resources.blankScoreBg;
                }
                else // no scores left, assigning blank stuff out to controls on the form
                {
                    usernameForm[formCounter].Text = ""; scoreForm[formCounter].Text = ""; modsForm[formCounter].Text = "";
                    accForm[formCounter].Text = ""; ppForm[formCounter].Text = "";

                    rankForm[formCounter].Image = Properties.Resources.blankScoreBg;
                    profilePicForm[formCounter].Image = Properties.Resources.blankScoreBg;
                }

                // used for form arrays
                formCounter += 1;
            }
        }

        // this bacgkround worker has multiple stages, explained more in "backgroundWorkerStage"
        // variable initialization above on the top of this page in comments
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // will hold gameProcess that can be used throughout usage of this background worker
            Process gameProcess = null;

            // ### pointers 
            // map id pointer address, address holds number for "https://osu.ppy.sh/api/get_scores"
            IntPtr mapIdPointer = (IntPtr)0;

            // enabled mods pointer, for enabled mods in game, enabled mods in game will be used
            // for mod leaderboard, changed with mod change and will stay for changing maps too
            IntPtr enabledModsPointer = (IntPtr)0;

            // game mode pointer, for chosen game mode in game, this is for osu api
            // as if there's a non default game mode map you have to specify game mode in request to get the scores data
            IntPtr gameModePointer = (IntPtr)0;

            // bitwise enum representing a combination of enabled mods
            // for full explanation with visualization go to https://github.com/ppy/osu-api/wiki/Dokumentasi-osu%21-api
            IntPtr modCombinationPointer = (IntPtr)0;

            while (true)
            {
                // 3 = read game data using pointers and if map changed read map's data and display
                if (backgroundWorkerStage == 3)
                {
                    // checking if osu is still running and going to stage 1 if not
                    if (!isProcessOpen("osu!")) backgroundWorkerStage = 1;

                    // reads current mapId from memory by using mapIdPointer and specially created function
                    int currentMapId = readMapIdMemory(mapIdPointer, 0xC8, gameProcess.Handle);

                    // reads enabled in-game mods for mod leaderboards
                    int currentEnabledMods = readEnabledModsMemory(enabledModsPointer, gameProcess.Handle);

                    // reads current game mode, it's for leaderboard with other game modes
                    // has to be specified if you want to use non-default mod
                    int currentGameMode = readGameModeMemory(gameModePointer, gameProcess.Handle);

                    // current id != old id meaning user changed their map in map list
                    // current mods != old mods meaning user changed selected mods in game
                    // both above require reload and this is why we're checking if they changed
                    if ((currentMapId != lastMapId && currentMapId != 0) || ((currentEnabledMods != lastMods) && modLeaderboard)
                        || currentGameMode != lastGameMode)
                    {
                        // resetting leaderboard page when changing map, just a preference
                        // it means that if we go to last page on leaderboard and switch map
                        // it will load first page again
                        globalLeaderboardPage = 1;

                        // also changing selected page on the form (visually) for page 1/x
                        // adding PAGE 1 and removiong first 6 characters that hold PAGE x
                        currentPage.Text = "PAGE 1" + currentPage.Text.Remove(0, 6);

                        // web client used for sending requests, in our case api requests to osu api
                        WebClient webClient = new WebClient();

                        // full leaderboard for map we sent api request with
                        string mapScoresRaw = "";

                        // hiding scores before changing them so the transition from maps is visible
                        hideScores();

                        // showing loading label text, big loading text in the middle when waiting for osu api
                        loadingText.Location = new Point(-3, -1);

                        // clearing profile picture images in form as it takes time for them to load
                        // when locations is changed
                        foreach (PictureBox pfp in profilePicForm)
                            pfp.Image = Properties.Resources.blankScoreBg;

                        try
                        {
                            // requesting for scores from osu website
                            if (!modLeaderboard)
                            {
                                mapScoresRaw = webClient.DownloadString("https://osu.ppy.sh/api/get_scores?k=" +
                                Crypto.Decrypt(apiKey, "7b03b040f85b47419e2ba3ad2630897f") + "&b=" + currentMapId
                                + "&m=" + currentGameMode);
                            }
                            else
                            {
                                mapScoresRaw = webClient.DownloadString("https://osu.ppy.sh/api/get_scores?k=" +
                                Crypto.Decrypt(apiKey, "7b03b040f85b47419e2ba3ad2630897f") + "&b=" + currentMapId
                                + "&mods=" + currentEnabledMods + "&m=" + currentGameMode);
                            }
                        }
                        catch { }

                        // remove loading button because it's not doing anything anymore, all info received
                        loadingText.Location = new Point(30000, 0);

                        // saving map as last map, and enabled mods as last mods so it won't try reading this again
                        lastMapId = currentMapId;
                        lastMods = currentEnabledMods;
                        lastGameMode = currentGameMode;

                        // checking if we got request back with at least one score
                        // every row has "score_id" so it's good for checking
                        if (mapScoresRaw.Contains("score_id"))
                        {
                            // creaint manager for osuJsonParse class for functions + tools
                            osuJsonParse osuJsonManager = new osuJsonParse();

                            // converting received json to easily readible array
                            parsedScores = osuJsonManager.parseBeatmapScores(mapScoresRaw);

                            // going through all of the scores that we have and assigning to form controls
                            assignScoresToForm(parsedScores, globalLeaderboardPage);

                            // showing scores after putting all info in form
                            showScores();
                        }

                        // changing status log on the form accordingly
                        logStatus.Text = "STATUS: ALL GOOD, HAVE FUN";
                        logStatus.ForeColor = Color.LimeGreen;
                    }

                    // quick sleep between every loop just for optimization
                    Thread.Sleep(100);
                }

                // 2 = find signature addresses and then pointers out of these addresses
                else if (backgroundWorkerStage == 2)
                {
                    // changing status log on the form accordingly
                    logStatus.Text = "STATUS: SCANNING MEMORY, OPEN MAP LIST";
                    logStatus.ForeColor = Color.Yellow;

                    // manager object for all of the signature related functions + tools
                    SignatureManager signatureManager = new SignatureManager();
                    SignatureTemplate[] mapIdSignature = new SignatureTemplate[2];
                    SignatureTemplate[] enabledModsSignature = new SignatureTemplate[1];
                    SignatureTemplate[] gameModeSignature = new SignatureTemplate[2];

                    // ### signature groups
                    // they hold signature and offset that will be added so the address of
                    // signature + offset will hold the actual address of a pointer
                    mapIdSignature[0] = new SignatureTemplate("85 C0 74 05 33 D2 89 55 DC 53", 0x0C);
                    mapIdSignature[1] = new SignatureTemplate("8B CE 89 4D CC 8B 4D 08", 0x14);

                    enabledModsSignature[0] = new SignatureTemplate("DB 85 38 FF FF FF 83", 0x0E);

                    gameModeSignature[0] = new SignatureTemplate("01 00 00 85 ?? ?? ?? ?? A1 ?? ?? ?? ?? C3", 0x09);
                    gameModeSignature[1] = new SignatureTemplate("55 8B EC 56 8B F1 39 15", 0x08);

                    // refreshing pointers for stage unexpected degradation support
                    mapIdPointer = (IntPtr)0;
                    enabledModsPointer = (IntPtr)0;
                    gameModePointer = (IntPtr)0;

                    // going throgh each index of an array, we have 2 for backups
                    // if one of them goes down after an update
                    // below is for map id
                    foreach (SignatureTemplate sigTemplate in mapIdSignature)
                    {
                        // scanning for signature address
                        mapIdPointer = signatureManager.signatureScan(sigTemplate.signature,
                        gameProcess.Handle, (IntPtr)sigTemplate.offset, (IntPtr)0, (IntPtr)int.MaxValue);

                        // found signature address and doesn't have too look using the other ones (backups)
                        if (mapIdPointer != (IntPtr)0) break;

                        // checking if osu is still running and going to stage 1 if not
                        if (!isProcessOpen("osu!")) backgroundWorkerStage = 1;
                    }

                    // going throgh each index of an array, we have 1
                    // if one of them goes down after an update
                    // below is for enabled mods
                    foreach (SignatureTemplate sigTemplate in enabledModsSignature)
                    {
                        // scanning for signature address
                        enabledModsPointer = signatureManager.signatureScan(sigTemplate.signature,
                        gameProcess.Handle, (IntPtr)sigTemplate.offset, (IntPtr)0, (IntPtr)int.MaxValue);

                        // found signature address and doesn't have too look using the other ones (backups)
                        if (enabledModsPointer != (IntPtr)0) break;

                        // checking if osu is still running and going to stage 1 if not
                        if (!isProcessOpen("osu!")) backgroundWorkerStage = 1;
                    }

                    // going throgh each index of an array, we have 1
                    // if one of them goes down after an update
                    // below is for game mode
                    foreach (SignatureTemplate sigTemplate in gameModeSignature)
                    {
                        // scanning for signature address
                        gameModePointer = signatureManager.signatureScan(sigTemplate.signature,
                        gameProcess.Handle, (IntPtr)sigTemplate.offset, (IntPtr)0, (IntPtr)int.MaxValue);

                        // found signature address and doesn't have too look using the other ones (backups)
                        if (gameModePointer != (IntPtr)0) break;

                        // checking if osu is still running and going to stage 1 if not
                        if (!isProcessOpen("osu!")) backgroundWorkerStage = 1;
                    }

                    // checking if osu is still running and going to stage 1 if not
                    if (!isProcessOpen("osu!")) backgroundWorkerStage = 1;

                    // found signature addresses and moving to next stage
                    if (mapIdPointer != (IntPtr)0 && enabledModsPointer != (IntPtr)0)
                        backgroundWorkerStage = 3;
                }

                // 1 = looking for game process, first time running osu or maybe it closed
                else if (backgroundWorkerStage == 1)
                {
                    gameProcess = null; // making it null for unexpected stage degradation support
                    hideScores(); // we hide scores so there's indication that osu was closed

                    // changing log status, text on the bottom of the form, first row
                    logStatus.Text = "STATUS: WAITING FOR OSU!";
                    logStatus.ForeColor = Color.Red;

                    // searching through processes to see if osu! is open
                    foreach (Process process in Process.GetProcesses())
                    {
                        // if found osu assign process to Process variable and go to next stage
                        // not uing a function so we could assign process to gameProcess variable
                        if (process.ProcessName == "osu!")
                        {
                            gameProcess = process;
                            backgroundWorkerStage = 2;
                        }
                    }

                    // break between each check for performance
                    Thread.Sleep(300);
                }
            }
        }
    }
}
