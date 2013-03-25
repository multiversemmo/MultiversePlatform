using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Multiverse.Voice;
using log4net;
using Multiverse.Lib.LogUtil;

namespace VoiceTest
{
    public partial class VoiceTestForm : Form
    {
        private VoiceManager voiceMgr = null;
        
        // The bit-wise inversion of the account id
        private int authenticationToken = ~(4130);

        private string serverHostName = "localhost";
        
        private int currentMicNumber = 0;
                
        private string[] microphoneDevices = null;
        
        public VoiceTestForm()
        {
            InitializeComponent();
            // initialize logging
            LogUtil.InitializeLogging("../", "", "../Logs/VoiceTest.log");
            LogUtil.SetLogLevel(log4net.Core.Level.Debug, true);
            oidTextBox.Text = new Random().Next().ToString();
        }

        private void playSoundButton_Click(object sender, EventArgs e)
        {
            if (openSpeexFileDialog.ShowDialog() == DialogResult.OK) {
                InitVoiceManager(false);
                VoiceManager.VoiceChannel voiceChannel = voiceMgr.AddVoiceChannel(-1L, 1, false, recordVoicesSpeexCheckBox.Checked);
                voiceChannel.StartPlayback(openSpeexFileDialog.FileName);
            }
        }

        private void InitVoiceManager(bool connectToServer) {
            DisposeVoiceMgr();
            voiceMgr = VoiceManager.Configure(null, null,
                             // Constructor parameters
                             "connect_to_server", connectToServer, 
                             "voice_server_host", serverHostName, "voice_server_port", 5051,
                             "authentication_token", authenticationToken, 
                             "player_oid", System.Int64.Parse(oidTextBox.Text), "group_oid", System.Int64.Parse(groupOidTextBox.Text),
                             "mic_device_number", 0, "use_tcp", useTcpCheckBox.Checked, "mic_record_wav", recordMicWAVCheckBox.Checked,
                             "mic_record_speex", recordMicSpeexCheckBox.Checked, "voices_record_speex", recordVoicesSpeexCheckBox.Checked,
                             "listen_to_yourself", listenToYourselfCheckBox.Checked,
                             // Encodec settings
                             "sampling_rate", 8000, "complexity", 3, "quality", 4,
                             // Preprocessor parameters
                             "agc_enable", true, "agc_level", micLevelTrackBar.Value, 
                             "vad_enable", true, "vad_prob_start", 40
                             );
            statisticsTimer.Enabled = true;
        }

        private void connectToServerButton_Click(object sender, EventArgs e) {
            connectToServerButton.Enabled = false;
            stopVoiceManagerButton.Enabled = true;
            serverHostName = serverHostTextBox.Text;
            enableConfig(false);
            InitVoiceManager(true);
        }

        private void stopVoiceManagerButton_Click(object sender, EventArgs e) {
            VoiceManager.MicrophoneChannel micChannel = voiceMgr.GetMicrophoneChannel(0);
            micChannel.StopRecording();
            setAllStartingButtons(true);
            stopVoiceManagerButton.Enabled = false;
            enableConfig(true);
            DisposeVoiceMgr();
            micActiveLabel.Text = "Mic Active? No";
            incomingVoicesLabel.Text = "Incoming Voices: 0";
        }
        
        private void configVMButton_Click(object sender, EventArgs e) {
            voiceMgr = VoiceManager.Configure(configArgsTextBox.Text, null);
        }

        private void reconfigButton_Click(object sender, EventArgs e) {
            voiceMgr = VoiceManager.Reconfigure(voiceMgr, configArgsTextBox.Text, null);
        }

        private void enableConfig(bool enable) {
            recordMicWAVCheckBox.Enabled = enable;
            recordMicSpeexCheckBox.Enabled = enable;
            recordVoicesSpeexCheckBox.Enabled = enable;
            listenToYourselfCheckBox.Enabled = enable;
            enableVADCheckBox.Enabled = enable;
            serverHostTextBox.Enabled = enable;
            oidTextBox.Enabled = enable;
            groupOidTextBox.Enabled = enable;
            useTcpCheckBox.Enabled = enable;
            setAllStartingButtons(enable);
        }

        private void setAllStartingButtons(bool state) {
            playSoundButton.Enabled = state;
            connectToServerButton.Enabled = state;
            getMicDevicesButton.Enabled = state;
            changeMicButton.Enabled = state;
            configVMButton.Enabled = state;
        }


        private void statisticsTimer_Tick(object sender, EventArgs e) {
            VoiceManager vm = voiceMgr;
            if (vm != null) {
                vm.Tick();
                packetsSentLabel.Text = "Packets Sent: " + vm.PacketsSentCounter;
                packetsReceivedLabel.Text = "Packets Received: " + vm.PacketsReceivedCounter;
                micActiveLabel.Text = "Mic Active? " + (vm.MicActive ? "Yes" : "No");
                incomingVoicesLabel.Text = "Incoming Voices: " + vm.VoiceCount;
                micWAVFramesLabel.Text = "WAV Frames Written: " + (int)(vm.MicWAVBytesRecordedCounter / 320);
                micSpeexFramesWrittenLabel.Text = "Speex Frames Written: " + vm.MicSpeexFramesRecordedCounter;
                voiceSpeexFramesWrittenLabel.Text = "Speex Frames Written: " + vm.VoicesSpeexFramesRecordedCounter;
            }
        }

        private void getMicDevicesButton_Click(object sender, EventArgs e)
        {
            DisposeVoiceMgr();
            voiceMgr = VoiceManager.Configure(null, null, "connect_to_server", false);
            microphoneDevices = voiceMgr.GetAllMicrophoneDevices();
            deviceCountLabel.Text = "DeviceCount: " + microphoneDevices.Length;
            showMicDevice();
        }

        private bool showMicDevice() 
        {
            if (currentMicNumber >= microphoneDevices.Length) {
                deviceStringLabel.Text = "Device: Mic # Out Of Range!";
                return false;
            }
            else {
               deviceStringLabel.Text = "Device: " + microphoneDevices[currentMicNumber];
               return true;
            }
        }

        private void changeMicButton_Click(object sender, EventArgs e)
        {
            currentMicNumber = Int32.Parse(chosenMicDevice.Text);
            if (showMicDevice())
                voiceMgr = VoiceManager.Reconfigure(voiceMgr, "connect_to_server false mic_device_number " + currentMicNumber, null);
        }

        private void DisposeVoiceMgr() {
            if (voiceMgr != null) {
                voiceMgr.Dispose();
                voiceMgr = null;
            }
        }
        
        private void VoiceTestForm_FormClosing(object sender, FormClosingEventArgs e) {
            DisposeVoiceMgr();
        }

    }
}