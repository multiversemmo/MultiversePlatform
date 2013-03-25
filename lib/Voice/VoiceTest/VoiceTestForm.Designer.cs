namespace VoiceTest
{
    partial class VoiceTestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.playSoundButton = new System.Windows.Forms.Button();
            this.connectToServerButton = new System.Windows.Forms.Button();
            this.serverHostTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.stopVoiceManagerButton = new System.Windows.Forms.Button();
            this.recordMicWAVCheckBox = new System.Windows.Forms.CheckBox();
            this.recordMicSpeexCheckBox = new System.Windows.Forms.CheckBox();
            this.recordVoicesSpeexCheckBox = new System.Windows.Forms.CheckBox();
            this.packetsSentLabel = new System.Windows.Forms.Label();
            this.packetsReceivedLabel = new System.Windows.Forms.Label();
            this.micActiveLabel = new System.Windows.Forms.Label();
            this.incomingVoicesLabel = new System.Windows.Forms.Label();
            this.statisticsTimer = new System.Windows.Forms.Timer(this.components);
            this.voiceSpeexFramesWrittenLabel = new System.Windows.Forms.Label();
            this.micSpeexFramesWrittenLabel = new System.Windows.Forms.Label();
            this.micWAVFramesLabel = new System.Windows.Forms.Label();
            this.openSpeexFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.micLevelTrackBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.enableVADCheckBox = new System.Windows.Forms.CheckBox();
            this.listenToYourselfCheckBox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.oidTextBox = new System.Windows.Forms.TextBox();
            this.useTcpCheckBox = new System.Windows.Forms.CheckBox();
            this.groupOidTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.getMicDevicesButton = new System.Windows.Forms.Button();
            this.deviceCountLabel = new System.Windows.Forms.Label();
            this.chosenMicDevice = new System.Windows.Forms.TextBox();
            this.changeMicButton = new System.Windows.Forms.Button();
            this.deviceStringLabel = new System.Windows.Forms.Label();
            this.configVMButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.configArgsTextBox = new System.Windows.Forms.TextBox();
            this.reconfigButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.micLevelTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // playSoundButton
            // 
            this.playSoundButton.Location = new System.Drawing.Point(59, 522);
            this.playSoundButton.Name = "playSoundButton";
            this.playSoundButton.Size = new System.Drawing.Size(223, 31);
            this.playSoundButton.TabIndex = 0;
            this.playSoundButton.Text = "Play Recorded Speex Frames";
            this.playSoundButton.UseVisualStyleBackColor = true;
            this.playSoundButton.Click += new System.EventHandler(this.playSoundButton_Click);
            // 
            // connectToServerButton
            // 
            this.connectToServerButton.Location = new System.Drawing.Point(59, 410);
            this.connectToServerButton.Name = "connectToServerButton";
            this.connectToServerButton.Size = new System.Drawing.Size(223, 31);
            this.connectToServerButton.TabIndex = 3;
            this.connectToServerButton.Text = "Connect To Voice Server";
            this.connectToServerButton.UseVisualStyleBackColor = true;
            this.connectToServerButton.Click += new System.EventHandler(this.connectToServerButton_Click);
            // 
            // serverHostTextBox
            // 
            this.serverHostTextBox.Location = new System.Drawing.Point(208, 29);
            this.serverHostTextBox.Name = "serverHostTextBox";
            this.serverHostTextBox.Size = new System.Drawing.Size(196, 22);
            this.serverHostTextBox.TabIndex = 4;
            this.serverHostTextBox.Text = "localhost";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(78, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Voice Server Host";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // stopVoiceManagerButton
            // 
            this.stopVoiceManagerButton.Enabled = false;
            this.stopVoiceManagerButton.Location = new System.Drawing.Point(59, 468);
            this.stopVoiceManagerButton.Name = "stopVoiceManagerButton";
            this.stopVoiceManagerButton.Size = new System.Drawing.Size(223, 31);
            this.stopVoiceManagerButton.TabIndex = 6;
            this.stopVoiceManagerButton.Text = "Stop Voice Manager";
            this.stopVoiceManagerButton.UseVisualStyleBackColor = true;
            this.stopVoiceManagerButton.Click += new System.EventHandler(this.stopVoiceManagerButton_Click);
            // 
            // recordMicWAVCheckBox
            // 
            this.recordMicWAVCheckBox.AutoSize = true;
            this.recordMicWAVCheckBox.Location = new System.Drawing.Point(93, 145);
            this.recordMicWAVCheckBox.Name = "recordMicWAVCheckBox";
            this.recordMicWAVCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.recordMicWAVCheckBox.Size = new System.Drawing.Size(133, 21);
            this.recordMicWAVCheckBox.TabIndex = 7;
            this.recordMicWAVCheckBox.Text = "Record Mic/WAV";
            this.recordMicWAVCheckBox.UseVisualStyleBackColor = true;
            // 
            // recordMicSpeexCheckBox
            // 
            this.recordMicSpeexCheckBox.AutoSize = true;
            this.recordMicSpeexCheckBox.Location = new System.Drawing.Point(93, 172);
            this.recordMicSpeexCheckBox.Name = "recordMicSpeexCheckBox";
            this.recordMicSpeexCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.recordMicSpeexCheckBox.Size = new System.Drawing.Size(141, 21);
            this.recordMicSpeexCheckBox.TabIndex = 8;
            this.recordMicSpeexCheckBox.Text = "Record Mic/Speex";
            this.recordMicSpeexCheckBox.UseVisualStyleBackColor = true;
            // 
            // recordVoicesSpeexCheckBox
            // 
            this.recordVoicesSpeexCheckBox.AutoSize = true;
            this.recordVoicesSpeexCheckBox.Location = new System.Drawing.Point(93, 199);
            this.recordVoicesSpeexCheckBox.Name = "recordVoicesSpeexCheckBox";
            this.recordVoicesSpeexCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.recordVoicesSpeexCheckBox.Size = new System.Drawing.Size(162, 21);
            this.recordVoicesSpeexCheckBox.TabIndex = 9;
            this.recordVoicesSpeexCheckBox.Text = "Record Voices/Speex";
            this.recordVoicesSpeexCheckBox.UseVisualStyleBackColor = true;
            // 
            // packetsSentLabel
            // 
            this.packetsSentLabel.AutoSize = true;
            this.packetsSentLabel.Location = new System.Drawing.Point(296, 407);
            this.packetsSentLabel.Name = "packetsSentLabel";
            this.packetsSentLabel.Size = new System.Drawing.Size(107, 17);
            this.packetsSentLabel.TabIndex = 10;
            this.packetsSentLabel.Text = "Packets Sent: 0";
            // 
            // packetsReceivedLabel
            // 
            this.packetsReceivedLabel.AutoSize = true;
            this.packetsReceivedLabel.Location = new System.Drawing.Point(296, 433);
            this.packetsReceivedLabel.Name = "packetsReceivedLabel";
            this.packetsReceivedLabel.Size = new System.Drawing.Size(133, 17);
            this.packetsReceivedLabel.TabIndex = 11;
            this.packetsReceivedLabel.Text = "Packets Received 0";
            // 
            // micActiveLabel
            // 
            this.micActiveLabel.AutoSize = true;
            this.micActiveLabel.Location = new System.Drawing.Point(296, 465);
            this.micActiveLabel.Name = "micActiveLabel";
            this.micActiveLabel.Size = new System.Drawing.Size(101, 17);
            this.micActiveLabel.TabIndex = 12;
            this.micActiveLabel.Text = "Mic Active? No";
            // 
            // incomingVoicesLabel
            // 
            this.incomingVoicesLabel.AutoSize = true;
            this.incomingVoicesLabel.Location = new System.Drawing.Point(296, 491);
            this.incomingVoicesLabel.Name = "incomingVoicesLabel";
            this.incomingVoicesLabel.Size = new System.Drawing.Size(126, 17);
            this.incomingVoicesLabel.TabIndex = 13;
            this.incomingVoicesLabel.Text = "Incoming Voices: 0";
            // 
            // statisticsTimer
            // 
            this.statisticsTimer.Interval = 250;
            this.statisticsTimer.Tick += new System.EventHandler(this.statisticsTimer_Tick);
            // 
            // voiceSpeexFramesWrittenLabel
            // 
            this.voiceSpeexFramesWrittenLabel.AutoSize = true;
            this.voiceSpeexFramesWrittenLabel.Location = new System.Drawing.Point(296, 200);
            this.voiceSpeexFramesWrittenLabel.Name = "voiceSpeexFramesWrittenLabel";
            this.voiceSpeexFramesWrittenLabel.Size = new System.Drawing.Size(163, 17);
            this.voiceSpeexFramesWrittenLabel.TabIndex = 16;
            this.voiceSpeexFramesWrittenLabel.Text = "Speex Frames Written: 0";
            this.voiceSpeexFramesWrittenLabel.Visible = false;
            // 
            // micSpeexFramesWrittenLabel
            // 
            this.micSpeexFramesWrittenLabel.AutoSize = true;
            this.micSpeexFramesWrittenLabel.Location = new System.Drawing.Point(296, 173);
            this.micSpeexFramesWrittenLabel.Name = "micSpeexFramesWrittenLabel";
            this.micSpeexFramesWrittenLabel.Size = new System.Drawing.Size(163, 17);
            this.micSpeexFramesWrittenLabel.TabIndex = 15;
            this.micSpeexFramesWrittenLabel.Text = "Speex Frames Written: 0";
            this.micSpeexFramesWrittenLabel.Visible = false;
            // 
            // micWAVFramesLabel
            // 
            this.micWAVFramesLabel.AutoSize = true;
            this.micWAVFramesLabel.Location = new System.Drawing.Point(296, 145);
            this.micWAVFramesLabel.Name = "micWAVFramesLabel";
            this.micWAVFramesLabel.Size = new System.Drawing.Size(155, 17);
            this.micWAVFramesLabel.TabIndex = 14;
            this.micWAVFramesLabel.Text = "WAV Frames Written: 0";
            this.micWAVFramesLabel.Visible = false;
            // 
            // openSpeexFileDialog
            // 
            this.openSpeexFileDialog.Filter = "All Speex Files|*.speex|All Files|*.*";
            // 
            // micLevelTrackBar
            // 
            this.micLevelTrackBar.Location = new System.Drawing.Point(64, 323);
            this.micLevelTrackBar.Maximum = 20;
            this.micLevelTrackBar.Name = "micLevelTrackBar";
            this.micLevelTrackBar.Size = new System.Drawing.Size(395, 53);
            this.micLevelTrackBar.TabIndex = 17;
            this.micLevelTrackBar.Value = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(194, 303);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 17);
            this.label2.TabIndex = 18;
            this.label2.Text = "Microphone Volume";
            // 
            // enableVADCheckBox
            // 
            this.enableVADCheckBox.AutoSize = true;
            this.enableVADCheckBox.Checked = true;
            this.enableVADCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableVADCheckBox.Location = new System.Drawing.Point(93, 226);
            this.enableVADCheckBox.Name = "enableVADCheckBox";
            this.enableVADCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.enableVADCheckBox.Size = new System.Drawing.Size(222, 21);
            this.enableVADCheckBox.TabIndex = 19;
            this.enableVADCheckBox.Text = "Enable Voice Activity Detection";
            this.enableVADCheckBox.UseVisualStyleBackColor = true;
            // 
            // listenToYourselfCheckBox
            // 
            this.listenToYourselfCheckBox.AutoSize = true;
            this.listenToYourselfCheckBox.Location = new System.Drawing.Point(93, 253);
            this.listenToYourselfCheckBox.Name = "listenToYourselfCheckBox";
            this.listenToYourselfCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.listenToYourselfCheckBox.Size = new System.Drawing.Size(330, 21);
            this.listenToYourselfCheckBox.TabIndex = 20;
            this.listenToYourselfCheckBox.Text = "Send Voice Frames From Your Mic Back To You";
            this.listenToYourselfCheckBox.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(114, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 17);
            this.label3.TabIndex = 21;
            this.label3.Text = "Player\'s OID";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // oidTextBox
            // 
            this.oidTextBox.Location = new System.Drawing.Point(209, 67);
            this.oidTextBox.Name = "oidTextBox";
            this.oidTextBox.Size = new System.Drawing.Size(195, 22);
            this.oidTextBox.TabIndex = 22;
            // 
            // useTcpCheckBox
            // 
            this.useTcpCheckBox.AutoSize = true;
            this.useTcpCheckBox.Checked = true;
            this.useTcpCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useTcpCheckBox.Location = new System.Drawing.Point(425, 29);
            this.useTcpCheckBox.Name = "useTcpCheckBox";
            this.useTcpCheckBox.Size = new System.Drawing.Size(150, 21);
            this.useTcpCheckBox.TabIndex = 23;
            this.useTcpCheckBox.Text = "Connect Using TCP";
            this.useTcpCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupOidTextBox
            // 
            this.groupOidTextBox.Location = new System.Drawing.Point(209, 104);
            this.groupOidTextBox.Name = "groupOidTextBox";
            this.groupOidTextBox.Size = new System.Drawing.Size(195, 22);
            this.groupOidTextBox.TabIndex = 25;
            this.groupOidTextBox.Text = "5";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(85, 109);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(115, 17);
            this.label4.TabIndex = 24;
            this.label4.Text = "Voice Group OID";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // getMicDevicesButton
            // 
            this.getMicDevicesButton.Location = new System.Drawing.Point(59, 576);
            this.getMicDevicesButton.Name = "getMicDevicesButton";
            this.getMicDevicesButton.Size = new System.Drawing.Size(223, 31);
            this.getMicDevicesButton.TabIndex = 26;
            this.getMicDevicesButton.Text = "Get Microphone Devices";
            this.getMicDevicesButton.UseVisualStyleBackColor = true;
            this.getMicDevicesButton.Click += new System.EventHandler(this.getMicDevicesButton_Click);
            // 
            // deviceCountLabel
            // 
            this.deviceCountLabel.AutoSize = true;
            this.deviceCountLabel.Location = new System.Drawing.Point(296, 583);
            this.deviceCountLabel.Name = "deviceCountLabel";
            this.deviceCountLabel.Size = new System.Drawing.Size(96, 17);
            this.deviceCountLabel.TabIndex = 27;
            this.deviceCountLabel.Text = "Device Count:";
            // 
            // chosenMicDevice
            // 
            this.chosenMicDevice.Location = new System.Drawing.Point(222, 631);
            this.chosenMicDevice.Name = "chosenMicDevice";
            this.chosenMicDevice.Size = new System.Drawing.Size(60, 22);
            this.chosenMicDevice.TabIndex = 28;
            this.chosenMicDevice.Text = "0";
            // 
            // changeMicButton
            // 
            this.changeMicButton.Location = new System.Drawing.Point(59, 627);
            this.changeMicButton.Name = "changeMicButton";
            this.changeMicButton.Size = new System.Drawing.Size(141, 31);
            this.changeMicButton.TabIndex = 29;
            this.changeMicButton.Text = "Change Mic";
            this.changeMicButton.UseVisualStyleBackColor = true;
            this.changeMicButton.Click += new System.EventHandler(this.changeMicButton_Click);
            // 
            // deviceStringLabel
            // 
            this.deviceStringLabel.AutoSize = true;
            this.deviceStringLabel.Location = new System.Drawing.Point(296, 634);
            this.deviceStringLabel.Name = "deviceStringLabel";
            this.deviceStringLabel.Size = new System.Drawing.Size(55, 17);
            this.deviceStringLabel.TabIndex = 30;
            this.deviceStringLabel.Text = "Device:";
            // 
            // configVMButton
            // 
            this.configVMButton.Location = new System.Drawing.Point(59, 684);
            this.configVMButton.Name = "configVMButton";
            this.configVMButton.Size = new System.Drawing.Size(189, 31);
            this.configVMButton.TabIndex = 31;
            this.configVMButton.Text = "Configure VoiceManager";
            this.configVMButton.UseVisualStyleBackColor = true;
            this.configVMButton.Click += new System.EventHandler(this.configVMButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(252, 710);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 17);
            this.label5.TabIndex = 32;
            this.label5.Text = "Args:";
            // 
            // configArgsTextBox
            // 
            this.configArgsTextBox.Location = new System.Drawing.Point(299, 707);
            this.configArgsTextBox.Name = "configArgsTextBox";
            this.configArgsTextBox.Size = new System.Drawing.Size(284, 22);
            this.configArgsTextBox.TabIndex = 33;
            // 
            // reconfigButton
            // 
            this.reconfigButton.Location = new System.Drawing.Point(59, 721);
            this.reconfigButton.Name = "reconfigButton";
            this.reconfigButton.Size = new System.Drawing.Size(189, 31);
            this.reconfigButton.TabIndex = 34;
            this.reconfigButton.Text = "Reconfigure VoiceManager";
            this.reconfigButton.UseVisualStyleBackColor = true;
            this.reconfigButton.Click += new System.EventHandler(this.reconfigButton_Click);
            // 
            // VoiceTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 774);
            this.Controls.Add(this.reconfigButton);
            this.Controls.Add(this.configArgsTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.configVMButton);
            this.Controls.Add(this.deviceStringLabel);
            this.Controls.Add(this.changeMicButton);
            this.Controls.Add(this.chosenMicDevice);
            this.Controls.Add(this.deviceCountLabel);
            this.Controls.Add(this.getMicDevicesButton);
            this.Controls.Add(this.groupOidTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.useTcpCheckBox);
            this.Controls.Add(this.oidTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listenToYourselfCheckBox);
            this.Controls.Add(this.enableVADCheckBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.micLevelTrackBar);
            this.Controls.Add(this.voiceSpeexFramesWrittenLabel);
            this.Controls.Add(this.micSpeexFramesWrittenLabel);
            this.Controls.Add(this.micWAVFramesLabel);
            this.Controls.Add(this.incomingVoicesLabel);
            this.Controls.Add(this.micActiveLabel);
            this.Controls.Add(this.packetsReceivedLabel);
            this.Controls.Add(this.packetsSentLabel);
            this.Controls.Add(this.recordVoicesSpeexCheckBox);
            this.Controls.Add(this.recordMicSpeexCheckBox);
            this.Controls.Add(this.recordMicWAVCheckBox);
            this.Controls.Add(this.stopVoiceManagerButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.serverHostTextBox);
            this.Controls.Add(this.connectToServerButton);
            this.Controls.Add(this.playSoundButton);
            this.Name = "VoiceTestForm";
            this.Text = "Voice Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VoiceTestForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.micLevelTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button playSoundButton;
        private System.Windows.Forms.Button connectToServerButton;
        private System.Windows.Forms.TextBox serverHostTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button stopVoiceManagerButton;
        private System.Windows.Forms.CheckBox recordMicWAVCheckBox;
        private System.Windows.Forms.CheckBox recordMicSpeexCheckBox;
        private System.Windows.Forms.CheckBox recordVoicesSpeexCheckBox;
        private System.Windows.Forms.Label packetsSentLabel;
        private System.Windows.Forms.Label packetsReceivedLabel;
        private System.Windows.Forms.Label micActiveLabel;
        private System.Windows.Forms.Label incomingVoicesLabel;
        private System.Windows.Forms.Timer statisticsTimer;
        private System.Windows.Forms.Label voiceSpeexFramesWrittenLabel;
        private System.Windows.Forms.Label micSpeexFramesWrittenLabel;
        private System.Windows.Forms.Label micWAVFramesLabel;
        private System.Windows.Forms.OpenFileDialog openSpeexFileDialog;
        private System.Windows.Forms.TrackBar micLevelTrackBar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox enableVADCheckBox;
        private System.Windows.Forms.CheckBox listenToYourselfCheckBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox oidTextBox;
        private System.Windows.Forms.CheckBox useTcpCheckBox;
        private System.Windows.Forms.TextBox groupOidTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button getMicDevicesButton;
        private System.Windows.Forms.Label deviceCountLabel;
        private System.Windows.Forms.TextBox chosenMicDevice;
        private System.Windows.Forms.Button changeMicButton;
        private System.Windows.Forms.Label deviceStringLabel;
        private System.Windows.Forms.Button configVMButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox configArgsTextBox;
        private System.Windows.Forms.Button reconfigButton;
    }
}

