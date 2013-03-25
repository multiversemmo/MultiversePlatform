/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

#region Using directives

using Axiom.Collections;
using Axiom.Configuration;
using Axiom.Core;
using Axiom.Graphics;


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

#endregion

namespace Multiverse.Base
{
	partial class ConfigDialog : Form
	{
		public DisplaySettings settings;
		List<DisplayMode> validModes = null;
        int inhibit = 0;

		const string LogoPicture = "../logopicture.jpg";

		public ConfigDialog(DisplaySettings settings) {
			this.settings = new DisplaySettings(settings);
            InitializeComponent();
            inhibit = 0;
            this.DialogResult = DialogResult.None;
            List<RenderSystem> renderSystems = Root.Instance.RenderSystems;
			foreach (RenderSystem rs in renderSystems)
				renderSystemComboBox.Items.Add(rs.Name);
			renderSystemComboBox.SelectedIndex = 0;

			logoPicture.Load(LogoPicture);
			logoPicture.Size = logoPicture.Image.Size;
            ApplySettingsToUI();
		}

		private string GetDisplayString(DisplayMode mode) {
			return string.Format("{0} x {1} at {2} bit color",
								 mode.Width, mode.Height, mode.Depth);
		}

		private string GetDisplayString(bool val) {
			if (val)
				return "Yes";
			else
                return "No";
		}

        private bool YesOrNoToBool(string yesOrNo) {
            return yesOrNo == "Yes";
        }
                
        private void ApplySettingsToUI() {
			DisplayConfig config = settings.renderSystem.ConfigOptions;
			List<DisplayMode> displayModes = config.FullscreenModes;
			validModes = new List<DisplayMode>();
			for (int i = 0; i < displayModes.Count; ++i) {
				if (displayModes[i].Width >= settings.minScreenWidth && displayModes[i].Height >= settings.minScreenHeight) {
			        DisplayMode mode = displayModes[i];
                    validModes.Add(mode);
                    videoModeComboBox.Items.Add(GetDisplayString(mode));
                }
            }
            DisplayMode displayMode = settings.displayMode;
            int index = videoModeComboBox.Items.IndexOf(GetDisplayString(displayMode));
            if (index < 0)
                videoModeComboBox.SelectedIndex = 0;
            else
                videoModeComboBox.SelectedIndex = index;
            setRadioButtonPair(fullScreenYesButton, displayMode.Fullscreen, fullScreenNoButton);
            fullScreenGroupBox.Visible = settings.allowFullScreen;
            setRadioButtonPair(vsyncYesButton, settings.vSync, vsyncNoButton);
            setRadioButtonPair(nvPerfHUDYesButton, settings.allowNVPerfHUD, nvPerfHUDNoButton);

            Dictionary<string, ConfigOption>rsConfigOptions = settings.renderSystem.GetConfigOptions();

            string currentAAVal = "None";
            if (rsConfigOptions.ContainsKey("Anti Aliasing"))
            {
                ConfigOption aaConfig = rsConfigOptions["Anti Aliasing"];
                // add the possible values to the combo box
                foreach (string s in aaConfig.possibleValues)
                {
                    aaComboBox.Items.Add(s);
                }

                // if current value from settings is valid, use it, otherwise default to None
                if (aaConfig.possibleValues.Contains(settings.antiAliasing))
                {
                    currentAAVal = settings.antiAliasing;
                }
            }
            else
            {
                // render system doesn't have AA settings
                aaComboBox.Items.Add("None");
            }
            aaComboBox.SelectedIndex = aaComboBox.Items.IndexOf(currentAAVal);
        }

        private void setRadioButtonPair(RadioButton button1, bool button1Checked, RadioButton button2) {
            button1.Checked = button1Checked;
            button2.Checked = !button1Checked;
        }
        
        private void CaptureUISettings() {
            DisplayMode m = validModes[videoModeComboBox.SelectedIndex];
            settings.displayMode = new DisplayMode(m.Width, m.Height, m.Depth, settings.allowFullScreen && fullScreenYesButton.Checked);
            settings.vSync = vsyncYesButton.Checked;
            settings.allowNVPerfHUD = nvPerfHUDYesButton.Checked;
            settings.antiAliasing = aaComboBox.Text;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            CaptureUISettings();
            this.DialogResult = DialogResult.OK;
        }

        private void ConfigDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.None)
                this.DialogResult = DialogResult.Cancel;
        }

        private void fullScreenYesButton_CheckedChanged(object sender, EventArgs e) {
            if (inhibit == 0) {
                inhibit++;
                fullScreenNoButton.Checked = !fullScreenYesButton.Checked;
                inhibit--;
            }
        }

        private void fullScreenNoButton_CheckedChanged(object sender, EventArgs e) {
            if (inhibit == 0) {
                inhibit++;
                fullScreenYesButton.Checked = !fullScreenNoButton.Checked;
                inhibit--;
            }
        }

        private void vsyncYesButton_CheckedChanged(object sender, EventArgs e) {
            if (inhibit == 0) {
                inhibit++;
                vsyncYesButton.Checked = !vsyncNoButton.Checked;
                inhibit--;
            }
        }

        private void vsyncNoButton_CheckedChanged(object sender, EventArgs e) {
            if (inhibit == 0) {
                inhibit++;
                vsyncNoButton.Checked = !vsyncYesButton.Checked;
                inhibit--;
            }
        }

        private void nvPerfHUDYesButton_CheckedChanged(object sender, EventArgs e) {
            if (inhibit == 0) {
                inhibit++;
                nvPerfHUDNoButton.Checked = !nvPerfHUDYesButton.Checked;
                inhibit--;
            }
        }

        private void nvPerfHUDNoButton_CheckedChanged(object sender, EventArgs e) {
            if (inhibit == 0) {
                inhibit++;
                nvPerfHUDYesButton.Checked = !nvPerfHUDNoButton.Checked;
                inhibit--;
            }
        }

        private void f(object sender, EventArgs e)
        {

        }

    }
}
