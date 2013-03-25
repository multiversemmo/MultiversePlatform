namespace VoiceBot
{
    partial class VoiceBotConsole
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
            this.label3 = new System.Windows.Forms.Label();
            this.speexFileDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.browseDirectoriesButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.voiceBotsConnectedTextBox = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.speexFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.label7 = new System.Windows.Forms.Label();
            this.voiceBotsStartedTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.voiceServerHostTextBox = new System.Windows.Forms.TextBox();
            this.updateUITimer = new System.Windows.Forms.Timer(this.components);
            this.startBotsTimer = new System.Windows.Forms.Timer(this.components);
            this.tickTimer = new System.Windows.Forms.Timer(this.components);
            this.createSequenceRadioButton = new System.Windows.Forms.RadioButton();
            this.loginStatusRadioButton = new System.Windows.Forms.RadioButton();
            this.sequenceGroupBox = new System.Windows.Forms.GroupBox();
            this.secondsBetweenStartsTextBox = new System.Windows.Forms.TextBox();
            this.voiceBotsToStartTextBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.firstBotOidTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.loginStatusGroupBox = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.highOidOfRangeTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lowOidOfRangeTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.voiceGroupOidTextBox = new System.Windows.Forms.TextBox();
            this.maxWaitTilNextPlaybackTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ignoredOidsTextBox = new System.Windows.Forms.TextBox();
            this.sequenceGroupBox.SuspendLayout();
            this.loginStatusGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(95, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(208, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Directory containing speex files:";
            // 
            // speexFileDirectoryTextBox
            // 
            this.speexFileDirectoryTextBox.Location = new System.Drawing.Point(309, 61);
            this.speexFileDirectoryTextBox.Name = "speexFileDirectoryTextBox";
            this.speexFileDirectoryTextBox.Size = new System.Drawing.Size(348, 22);
            this.speexFileDirectoryTextBox.TabIndex = 5;
            this.speexFileDirectoryTextBox.Text = "C:\\Multiverse\\Client\\Lib\\Voice\\VoiceTest\\bin\\Logs";
            // 
            // browseDirectoriesButton
            // 
            this.browseDirectoriesButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.browseDirectoriesButton.Location = new System.Drawing.Point(677, 54);
            this.browseDirectoriesButton.Name = "browseDirectoriesButton";
            this.browseDirectoriesButton.Size = new System.Drawing.Size(41, 29);
            this.browseDirectoriesButton.TabIndex = 6;
            this.browseDirectoriesButton.Text = "...";
            this.browseDirectoriesButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.browseDirectoriesButton.UseVisualStyleBackColor = true;
            this.browseDirectoriesButton.Click += new System.EventHandler(this.browseDirectoriesButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(40, 473);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(279, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "Number of Voice Bots connected to server:";
            // 
            // voiceBotsConnectedTextBox
            // 
            this.voiceBotsConnectedTextBox.Enabled = false;
            this.voiceBotsConnectedTextBox.Location = new System.Drawing.Point(325, 470);
            this.voiceBotsConnectedTextBox.Name = "voiceBotsConnectedTextBox";
            this.voiceBotsConnectedTextBox.Size = new System.Drawing.Size(47, 22);
            this.voiceBotsConnectedTextBox.TabIndex = 7;
            this.voiceBotsConnectedTextBox.Text = "0";
            // 
            // startButton
            // 
            this.startButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startButton.Location = new System.Drawing.Point(301, 361);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(93, 44);
            this.startButton.TabIndex = 9;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stopButton.Location = new System.Drawing.Point(423, 361);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(93, 44);
            this.stopButton.TabIndex = 10;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // speexFolderBrowserDialog
            // 
            this.speexFolderBrowserDialog.ShowNewFolderButton = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(111, 432);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(197, 17);
            this.label7.TabIndex = 16;
            this.label7.Text = "Number of Voice Bots started:";
            // 
            // voiceBotsStartedTextBox
            // 
            this.voiceBotsStartedTextBox.Enabled = false;
            this.voiceBotsStartedTextBox.Location = new System.Drawing.Point(325, 429);
            this.voiceBotsStartedTextBox.Name = "voiceBotsStartedTextBox";
            this.voiceBotsStartedTextBox.Size = new System.Drawing.Size(47, 22);
            this.voiceBotsStartedTextBox.TabIndex = 15;
            this.voiceBotsStartedTextBox.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(57, 15);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(122, 17);
            this.label8.TabIndex = 18;
            this.label8.Text = "Voice Server Host";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // voiceServerHostTextBox
            // 
            this.voiceServerHostTextBox.Location = new System.Drawing.Point(187, 12);
            this.voiceServerHostTextBox.Name = "voiceServerHostTextBox";
            this.voiceServerHostTextBox.Size = new System.Drawing.Size(196, 22);
            this.voiceServerHostTextBox.TabIndex = 17;
            this.voiceServerHostTextBox.Text = "localhost";
            // 
            // updateUITimer
            // 
            this.updateUITimer.Interval = 1000;
            this.updateUITimer.Tick += new System.EventHandler(this.updateUITimer_Tick);
            // 
            // startBotsTimer
            // 
            this.startBotsTimer.Interval = 1000;
            this.startBotsTimer.Tick += new System.EventHandler(this.startBotsTimer_Tick);
            // 
            // tickTimer
            // 
            this.tickTimer.Tick += new System.EventHandler(this.tickTimer_Tick);
            // 
            // createSequenceRadioButton
            // 
            this.createSequenceRadioButton.AutoSize = true;
            this.createSequenceRadioButton.Location = new System.Drawing.Point(130, 149);
            this.createSequenceRadioButton.Name = "createSequenceRadioButton";
            this.createSequenceRadioButton.Size = new System.Drawing.Size(193, 21);
            this.createSequenceRadioButton.TabIndex = 22;
            this.createSequenceRadioButton.TabStop = true;
            this.createSequenceRadioButton.Text = "Create a sequence of bots";
            this.createSequenceRadioButton.UseVisualStyleBackColor = true;
            this.createSequenceRadioButton.CheckedChanged += new System.EventHandler(this.createSequenceRadioButton_CheckedChanged);
            // 
            // loginStatusRadioButton
            // 
            this.loginStatusRadioButton.AutoSize = true;
            this.loginStatusRadioButton.Location = new System.Drawing.Point(496, 149);
            this.loginStatusRadioButton.Name = "loginStatusRadioButton";
            this.loginStatusRadioButton.Size = new System.Drawing.Size(193, 21);
            this.loginStatusRadioButton.TabIndex = 23;
            this.loginStatusRadioButton.TabStop = true;
            this.loginStatusRadioButton.Text = "Listen to login status msgs";
            this.loginStatusRadioButton.UseVisualStyleBackColor = true;
            this.loginStatusRadioButton.CheckedChanged += new System.EventHandler(this.loginStatusRadioButton_CheckedChanged);
            // 
            // sequenceGroupBox
            // 
            this.sequenceGroupBox.Controls.Add(this.secondsBetweenStartsTextBox);
            this.sequenceGroupBox.Controls.Add(this.voiceBotsToStartTextBox);
            this.sequenceGroupBox.Controls.Add(this.label12);
            this.sequenceGroupBox.Controls.Add(this.firstBotOidTextBox);
            this.sequenceGroupBox.Controls.Add(this.label10);
            this.sequenceGroupBox.Controls.Add(this.label11);
            this.sequenceGroupBox.Location = new System.Drawing.Point(30, 191);
            this.sequenceGroupBox.Name = "sequenceGroupBox";
            this.sequenceGroupBox.Size = new System.Drawing.Size(364, 143);
            this.sequenceGroupBox.TabIndex = 24;
            this.sequenceGroupBox.TabStop = false;
            this.sequenceGroupBox.Text = "Bot sequence parameters";
            // 
            // secondsBetweenStartsTextBox
            // 
            this.secondsBetweenStartsTextBox.Location = new System.Drawing.Point(246, 64);
            this.secondsBetweenStartsTextBox.Name = "secondsBetweenStartsTextBox";
            this.secondsBetweenStartsTextBox.Size = new System.Drawing.Size(47, 22);
            this.secondsBetweenStartsTextBox.TabIndex = 16;
            this.secondsBetweenStartsTextBox.Text = "1";
            // 
            // voiceBotsToStartTextBox
            // 
            this.voiceBotsToStartTextBox.Location = new System.Drawing.Point(246, 28);
            this.voiceBotsToStartTextBox.Name = "voiceBotsToStartTextBox";
            this.voiceBotsToStartTextBox.Size = new System.Drawing.Size(47, 22);
            this.voiceBotsToStartTextBox.TabIndex = 15;
            this.voiceBotsToStartTextBox.Text = "10";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(93, 104);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(147, 17);
            this.label12.TabIndex = 14;
            this.label12.Text = "OID of 1st bot started:";
            // 
            // firstBotOidTextBox
            // 
            this.firstBotOidTextBox.Location = new System.Drawing.Point(246, 101);
            this.firstBotOidTextBox.Name = "firstBotOidTextBox";
            this.firstBotOidTextBox.Size = new System.Drawing.Size(99, 22);
            this.firstBotOidTextBox.TabIndex = 13;
            this.firstBotOidTextBox.Text = "1000";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(13, 67);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(227, 17);
            this.label10.TabIndex = 10;
            this.label10.Text = "Seconds between Voice Bot starts:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(43, 31);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(197, 17);
            this.label11.TabIndex = 9;
            this.label11.Text = "Number of Voice Bots to start:";
            // 
            // loginStatusGroupBox
            // 
            this.loginStatusGroupBox.Controls.Add(this.label2);
            this.loginStatusGroupBox.Controls.Add(this.ignoredOidsTextBox);
            this.loginStatusGroupBox.Controls.Add(this.label9);
            this.loginStatusGroupBox.Controls.Add(this.highOidOfRangeTextBox);
            this.loginStatusGroupBox.Controls.Add(this.label5);
            this.loginStatusGroupBox.Controls.Add(this.lowOidOfRangeTextBox);
            this.loginStatusGroupBox.Location = new System.Drawing.Point(423, 191);
            this.loginStatusGroupBox.Name = "loginStatusGroupBox";
            this.loginStatusGroupBox.Size = new System.Drawing.Size(360, 143);
            this.loginStatusGroupBox.TabIndex = 25;
            this.loginStatusGroupBox.TabStop = false;
            this.loginStatusGroupBox.Text = "Range of OIDs listened for";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(53, 71);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(178, 17);
            this.label9.TabIndex = 27;
            this.label9.Text = "Largest OID in login range:";
            // 
            // highOidOfRangeTextBox
            // 
            this.highOidOfRangeTextBox.Location = new System.Drawing.Point(233, 68);
            this.highOidOfRangeTextBox.Name = "highOidOfRangeTextBox";
            this.highOidOfRangeTextBox.Size = new System.Drawing.Size(99, 22);
            this.highOidOfRangeTextBox.TabIndex = 26;
            this.highOidOfRangeTextBox.Text = "10000000";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(44, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(183, 17);
            this.label5.TabIndex = 23;
            this.label5.Text = "Smallest OID in login range:";
            // 
            // lowOidOfRangeTextBox
            // 
            this.lowOidOfRangeTextBox.Location = new System.Drawing.Point(233, 34);
            this.lowOidOfRangeTextBox.Name = "lowOidOfRangeTextBox";
            this.lowOidOfRangeTextBox.Size = new System.Drawing.Size(99, 22);
            this.lowOidOfRangeTextBox.TabIndex = 22;
            this.lowOidOfRangeTextBox.Text = "10";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(435, 12);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(211, 17);
            this.label6.TabIndex = 27;
            this.label6.Text = "Voice Group OID for Voice Bots:";
            // 
            // voiceGroupOidTextBox
            // 
            this.voiceGroupOidTextBox.Location = new System.Drawing.Point(652, 12);
            this.voiceGroupOidTextBox.Name = "voiceGroupOidTextBox";
            this.voiceGroupOidTextBox.Size = new System.Drawing.Size(99, 22);
            this.voiceGroupOidTextBox.TabIndex = 26;
            this.voiceGroupOidTextBox.Text = "1";
            // 
            // maxWaitTilNextPlaybackTextBox
            // 
            this.maxWaitTilNextPlaybackTextBox.Location = new System.Drawing.Point(309, 99);
            this.maxWaitTilNextPlaybackTextBox.Name = "maxWaitTilNextPlaybackTextBox";
            this.maxWaitTilNextPlaybackTextBox.Size = new System.Drawing.Size(47, 22);
            this.maxWaitTilNextPlaybackTextBox.TabIndex = 29;
            this.maxWaitTilNextPlaybackTextBox.Text = "4";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(134, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 17);
            this.label1.TabIndex = 28;
            this.label1.Text = "Max wait til next playback:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(132, 106);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 17);
            this.label2.TabIndex = 29;
            this.label2.Text = "Ignored OIDs:";
            // 
            // ignoredOidsTextBox
            // 
            this.ignoredOidsTextBox.Location = new System.Drawing.Point(233, 101);
            this.ignoredOidsTextBox.Name = "ignoredOidsTextBox";
            this.ignoredOidsTextBox.Size = new System.Drawing.Size(99, 22);
            this.ignoredOidsTextBox.TabIndex = 28;
            this.ignoredOidsTextBox.Text = "1087";
            // 
            // VoiceBotConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(807, 677);
            this.Controls.Add(this.maxWaitTilNextPlaybackTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.voiceGroupOidTextBox);
            this.Controls.Add(this.loginStatusGroupBox);
            this.Controls.Add(this.sequenceGroupBox);
            this.Controls.Add(this.loginStatusRadioButton);
            this.Controls.Add(this.createSequenceRadioButton);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.voiceServerHostTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.voiceBotsStartedTextBox);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.voiceBotsConnectedTextBox);
            this.Controls.Add(this.browseDirectoriesButton);
            this.Controls.Add(this.speexFileDirectoryTextBox);
            this.Controls.Add(this.label3);
            this.Name = "VoiceBotConsole";
            this.Text = "Voice Bot Console";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VoiceBotConsole_FormClosing);
            this.sequenceGroupBox.ResumeLayout(false);
            this.sequenceGroupBox.PerformLayout();
            this.loginStatusGroupBox.ResumeLayout(false);
            this.loginStatusGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox speexFileDirectoryTextBox;
        private System.Windows.Forms.Button browseDirectoriesButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox voiceBotsConnectedTextBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.FolderBrowserDialog speexFolderBrowserDialog;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox voiceBotsStartedTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox voiceServerHostTextBox;
        private System.Windows.Forms.Timer updateUITimer;
        private System.Windows.Forms.Timer startBotsTimer;
        private System.Windows.Forms.Timer tickTimer;
        private System.Windows.Forms.RadioButton createSequenceRadioButton;
        private System.Windows.Forms.RadioButton loginStatusRadioButton;
        private System.Windows.Forms.GroupBox sequenceGroupBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox firstBotOidTextBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox secondsBetweenStartsTextBox;
        private System.Windows.Forms.TextBox voiceBotsToStartTextBox;
        private System.Windows.Forms.GroupBox loginStatusGroupBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox highOidOfRangeTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox lowOidOfRangeTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox voiceGroupOidTextBox;
        private System.Windows.Forms.TextBox maxWaitTilNextPlaybackTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ignoredOidsTextBox;
    }
}

