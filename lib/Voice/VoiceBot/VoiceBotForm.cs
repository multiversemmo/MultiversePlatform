using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Multiverse.Voice;
using log4net;
using Multiverse.Lib.LogUtil;

namespace VoiceBot
{
    public partial class VoiceBotConsole : Form
    {
        int voiceBotsToStart;
        int voiceBotsStarted;
        int voiceBotsConnected;
        string voiceServerHost;
        int voiceServerPort;
        int secondsBetweenStarts;
        int maxWaitTilNextPlayback;
        long lowOidOfRange;
        long highOidOfRange;
        long voiceGroupOid;
        string[] speexFiles;
        bool createSequence;
        Dictionary<long, VoiceManager> voiceBots;
        VoiceManager loginStatusBot;
        bool running;
        Dictionary<long, long> ignoredOids;

        public VoiceBotConsole()
        {
            InitializeComponent();
            // initialize logging
            LogUtil.InitializeLogging("../", "", "../Logs/VoiceBots.log");
            LogUtil.SetLogLevel(log4net.Core.Level.Info, true);
            loginStatusRadioButton.Checked = true;
        }

        private void SetEnables(bool enabled)
        {
            createSequenceRadioButton.Enabled = enabled;
            loginStatusRadioButton.Enabled = enabled;
            sequenceGroupBox.Enabled = enabled && createSequence;
            loginStatusGroupBox.Enabled = enabled && !createSequence;
            speexFileDirectoryTextBox.Enabled = enabled;
            maxWaitTilNextPlaybackTextBox.Enabled = enabled;
            browseDirectoriesButton.Enabled = enabled;
            voiceGroupOidTextBox.Enabled = enabled;
            SetStartStop(enabled);
        }
        
        private void SetStartStop(bool start) 
        {
            startButton.Enabled = start;
            stopButton.Enabled = !start;
        }

        private void EnableTimers(bool ui, bool startBots) {
            updateUITimer.Enabled = ui;
            tickTimer.Enabled = ui;
            startBotsTimer.Enabled = startBots;
        }

        private void browseDirectoriesButton_Click(object sender, EventArgs e)
        {
            if (speexFolderBrowserDialog.ShowDialog() == DialogResult.OK)
                speexFileDirectoryTextBox.Text = speexFolderBrowserDialog.SelectedPath;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            lowOidOfRange = Int64.Parse(lowOidOfRangeTextBox.Text);
            highOidOfRange = Int64.Parse(highOidOfRangeTextBox.Text);
            voiceGroupOid = Int64.Parse(voiceGroupOidTextBox.Text);
            speexFiles = Directory.GetFiles(speexFileDirectoryTextBox.Text, "*.speex");
            maxWaitTilNextPlayback = Int32.Parse(maxWaitTilNextPlaybackTextBox.Text);
            voiceServerHost = voiceServerHostTextBox.Text;
            voiceServerPort = 5051;
            ignoredOids = new Dictionary<long, long>();
            char[] chars = new char[2];
            chars[0] = ',';
            chars[1] = ' ';
            string[] stringOids = ignoredOidsTextBox.Text.Split(chars);
            foreach (string stringOid in stringOids) {
                long oid = Int64.Parse(stringOid);
                ignoredOids[oid] = oid;
            }
            if (createSequence) {
                voiceBotsToStart = Int32.Parse(voiceBotsToStartTextBox.Text);
                secondsBetweenStarts = Int32.Parse(secondsBetweenStartsTextBox.Text);
                if (voiceBotsToStart > 0) {
                    PrepareToStart();
                    startBotsTimer.Interval = 1000 * secondsBetweenStarts;
                    EnableTimers(true, true);
                }
            }
            else {
                PrepareToStart();
                EnableTimers(true, false);
                log.InfoFormat("VoiceBotForm.startButton_Click: Starting listener for logins");
                // Run a listener for logins
                loginStatusBot = VoiceManager.CreateLoginStatusListener(OnBotConnect, OnLoginStatus,
                    "voice_server_host", voiceServerHost, "voice_server_port", voiceServerPort, 
                    "player_oid", -1L, "group_oid", 0L);
                voiceBots[-1L] = loginStatusBot;
            }
        }

        private void PrepareToStart() {
            SetEnables(false);
            voiceBotsStarted = 0;
            voiceBotsConnected = 0;
            voiceBots = new Dictionary<long, VoiceManager>();
            running = true;
        }

        private void OnBotConnect(long playerOid, bool success) {
            if (success)
                voiceBotsConnected++;
        }
        
        private string LoginString(bool login) {
            return login ? "login" : "logout";
        }
        
        ///<summary>
        ///    If we're paying attention to login status messages,
        ///    check if the oid is within the oid bounds, and if so, 
        ///    if it's a login, create a voice bot for it, and if it's
        ///    a logout, dispose of that voice bot
        ///</summary>
        private void OnLoginStatus(long playerOid, bool login) {
            VoiceManager vm;
            log.InfoFormat("VoiceBotForm.OnLoginStatus: Received {0} for player oid {1}", LoginString(login), playerOid);
            if (playerOid < lowOidOfRange || playerOid > highOidOfRange) {
                // We ignore this one because it fails the range check
                log.InfoFormat("VoiceBotForm.OnLoginStatus: Ignoring {0} for player oid {1} because it's outside the range {2}..{3}",
                    LoginString(login), playerOid, lowOidOfRange, highOidOfRange);
                return;
            }
            if (ignoredOids.ContainsKey(playerOid)) {
                log.InfoFormat("VoiceBotForm.OnLoginStatus: Ignoring {0} for player oid {1} because it's one of the ignored oids",
                    LoginString(login), playerOid);
                return;
            }
            voiceBots.TryGetValue(playerOid, out vm);
            if (login) {
                if (vm != null)
                    log.ErrorFormat("VoiceBotForm.OnLoginStatus: Received login for oid {0}, but that oid is already logged in!",
                        playerOid);
                else {
                    log.InfoFormat("VoiceBotForm.OnLoginStatus: Creating bot for oid {0}", playerOid);
                    Thread.Sleep(500);
                    voiceBots[playerOid] = VoiceManager.CreateBot(speexFiles, OnBotConnect, 
                        "voice_server_host", voiceServerHost, "voice_server_port", voiceServerPort, 
                        "player_oid", playerOid, "group_oid", voiceGroupOid,
                        "max_wait_til_next_playback", maxWaitTilNextPlayback,
                        "default_volume", 1.5f);
                }
            }
            else {
                if (vm == null)
                    log.ErrorFormat("VoiceBotForm.OnLoginStatus: Received logout for oid {0}, but that oid is not logged in!",
                        playerOid);
                else {
                    log.InfoFormat("VoiceBotForm.OnLoginStatus: Removing bot for oid {0}", playerOid);
                    voiceBots.Remove(playerOid);
                    vm.Dispose();
                }
            }
        }

        private void VoiceBotConsole_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (running)
                StopBots();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            StopBots();
        }

        private void StopBots() {
            foreach (VoiceManager vm in voiceBots.Values) {
                if (vm != null)
                    vm.Dispose();
            }
            voiceBots.Clear();
            running = false;
            EnableTimers(false, false);
            SetEnables(true);
        }

        private void updateUITimer_Tick(object sender, EventArgs e)
        {
            voiceBotsConnectedTextBox.Text = voiceBotsConnected.ToString();
            voiceBotsStartedTextBox.Text = voiceBotsStarted.ToString();
        }

        private void startBotsTimer_Tick(object sender, EventArgs e)
        {
            if (!running || voiceBotsStarted >= voiceBotsToStart)
                startBotsTimer.Enabled = false;
            else {
                voiceBots[voiceBotsStarted] =
                    VoiceManager.CreateBot(speexFiles, OnBotConnect, 
                        "voice_server_host", voiceServerHost, "voice_server_port", voiceServerPort, 
                        "player_oid", lowOidOfRange + voiceBotsStarted, "group_oid", voiceGroupOid);
                voiceBotsStarted++;
            }
        }

        private void tickTimer_Tick(object sender, EventArgs e)
        {
            if (createSequence && voiceBotsConnected == voiceBotsToStart)
                tickTimer.Enabled = false;
            else {
                foreach (VoiceManager vm in voiceBots.Values)
                    vm.Tick();
            }
        }

        private void createSequenceRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            createSequence = true;
            SetEnables(true);
        }

        private void loginStatusRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            createSequence = false;
            SetEnables(true);
        }

        // The logger used throughout the VoiceManager
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(VoiceBotConsole));

    }
}
