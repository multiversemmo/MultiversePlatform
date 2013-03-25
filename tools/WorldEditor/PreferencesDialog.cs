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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Multiverse.ToolBox;
using Multiverse.AssetRepository;

namespace Multiverse.Tools.WorldEditor
{
    public partial class Preferences_Dialog : Form
    {
        WorldEditor app;

        List<string> repositoryDirectoryList = new List<string>();

        public Preferences_Dialog(WorldEditor app)
        {
            this.app = app;
            InitializeComponent();
        }

        private void designateAssetRepositoryDirectoriesButton_Click(object sender, EventArgs e)
        {
            DesignateRepositories();
        }

        private void ChangeRepositoriesButton_Click(object sender, EventArgs e)
        {
            DesignateRepositories();
        }
        
        private void DesignateRepositories() 
        {
            DesignateRepositoriesDialog designateRepositoriesDialog = new DesignateRepositoriesDialog(repositoryDirectoryList);

            DialogResult result = designateRepositoriesDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                List<string> dirs = designateRepositoriesDialog.RepositoryDirectoryList;
                repositoryDirectoryList = new List<string>(dirs);
                repositoryDirectoryListLabel.Text = RepositoryClass.Instance.MakeRepositoryDirectoryListString(dirs);
//                 filesListBox.Items.Clear();
//                 foreach (string dir in dirs)
//                     filesListBox.Items.Add(dir);
            }
        }


        #region View Panel
        public bool DisplayOceanCheckbox
        {
            get
            {
                return displayOceanCheckbox.Checked;
            }
            set
            {
                displayOceanCheckbox.Checked = value;
            }
        }

        public bool DisplayFogEffects
        {
            get
            {
                return fogEffectsCheckBox.Checked;
            }
            set
            {
                fogEffectsCheckBox.Checked = value;
            }
        }

        public bool LightEffectsDisplay
        {
            get
            {
                return lightEffectsCheckBox.Checked;
            }
            set
            {
                lightEffectsCheckBox.Checked = value;
            }
        }


        public bool ShadowsDisplay
        {
            get
            {
                return shadowsCheckBox.Checked;
            }
            set
            {
                shadowsCheckBox.Checked = value;
            }
        }

        public bool DisplayRegionMarkers
        {
            get
            {
                return displayRegionMarkerCheckBox.Checked;
            }
            set
            {
                displayRegionMarkerCheckBox.Checked = value;
            }
        }

        public bool DisplayRoadMarkers
        {
            get
            {
                return displayRoadMarkerCheckBox.Checked;
            }
            set
            {
                displayRoadMarkerCheckBox.Checked = value;
            }
        }

        public bool DisplayMarkerPoints
        {
            get
            {
                return displayMarkerPointsCheckBox.Checked;
            }
            set
            {
                displayMarkerPointsCheckBox.Checked = value;
            }
        }

        public bool DisplayPointLightMarkers
        {
            get
            {
                return displayPointLightMarkersCheckbox.Checked;
            }
            set
            {
                displayPointLightMarkersCheckbox.Checked = value;
            }
        }

        public bool DisableAllMarkerPoints
        {
            get
            {
                return disableAllMarkersDisplayCheckBox.Checked;
            }
            set
            {
                disableAllMarkersDisplayCheckBox.Checked = value;
            }
        }

        public bool DisplayTerrainDecals
        {
            get
            {
                return displayTerrainDecalsCheckBox.Checked;
            }
            set
            {
                displayTerrainDecalsCheckBox.Checked = value;
            }
        }

        public bool CameraFollowsTerrain
        {
            get
            {
                return cameraFollowsTerrainCheckBox.Checked;
            }
            set
            {
                cameraFollowsTerrainCheckBox.Checked = value;
                if (value == true)
                {
                    cameraStaysAboveTerrainCheckbox.Checked = value;
                }
            }
        }

        public bool CameraStaysAboveTerrain
        {
            get
            {
                return cameraStaysAboveTerrainCheckbox.Checked;
            }
            set
            {
                cameraStaysAboveTerrainCheckbox.Checked = value;
            }
        }


        public float CameraNearDistanceFloat
        {
            get
            {
                float ret;
                if (float.TryParse(cameraNearDistanceTextBox.Text, out ret))
                {
                    return ret;
                }
                else
                {
                    return 1f;
                }
            }
            set
            {
                cameraNearDistanceTextBox.Text = value.ToString();
            }
        }

        public bool MaxFramesPerSecondEnabled
        {
            get
            {
                return maxFramesPerSecondCheckBox.Checked;
            }
            set
            {
                maxFramesPerSecondCheckBox.Checked = value;
            }
        }

        public uint MaxFramesPerSesconduInt
        {
            get
            {
                uint ret;
                if (uint.TryParse(maxFPSTextbox.Text, out ret))
                {
                    return ret;
                }
                else
                {
                    return 10;
                }
            }
            set
            {
                maxFPSTextbox.Text = value.ToString();
            }
        }

        public bool LockCameraToObject
        {
            get
            {
                return lockCameraToObjectCheckbox.Checked;
            }
            set
            {
                lockCameraToObjectCheckbox.Checked = value;
            }
        }

        public bool DisableVideoPlayback
        {
            get
            {
                return disableVideoPlaybackCheckbox.Checked;
            }
            set
            {
                disableVideoPlaybackCheckbox.Checked = value;
            }
        }

        #endregion View Panel

        #region AutoSave Panel
        public bool AutoSaveEnabled
        {
            get
            {
                return autoSaveCheckBox.Checked;
            }
            set
            {
                autoSaveCheckBox.Checked = value;
            }
        }

        public uint AutoSaveTimeuInt
        {
            get
            {
                uint i;
                if (uint.TryParse(autoSaveTimeTextBox.Text, out i))
                {
                    return i;
                }
                else
                {
                    return (uint)30;
                }
            }
            set
            {
                autoSaveTimeTextBox.Text = value.ToString();
            }
        }

        #endregion AutoSave Panel


        #region AssetRepositoryPanel

        #endregion AssetRepositoryPanel

        #region CameraControlPanel

        public float CameraDefaultSpeedTextBoxAsFloat
        {
            get
            {
                float speed;
                if (float.TryParse(cameraDefaultSpeedTextBox.Text, out speed))
                {
                    return speed;
                }
                return 0f;
            }
            set
            {
                cameraDefaultSpeedTextBox.Text = value.ToString();
            }
        }

        public float CameraSpeedIncrementTextBoxAsFloat
        {
            get
            {
                float inc;
                if(float.TryParse(cameraSpeedIncrementTextBox.Text, out inc))
                {
                    return inc;
                }
                return 0f;
            }
            set
            {
                cameraSpeedIncrementTextBox.Text = value.ToString();
            }
        }

        public float PresetCameraSpeed1TextBoxAsFloat
        {
            get
            {
                float speed;
                if (float.TryParse(presetCameraSpeed1TextBox.Text, out speed))
                {
                    return speed;
                }
                return 0f;
            }
            set
            {
                presetCameraSpeed1TextBox.Text = value.ToString();
            }
        }

        public float PresetCameraSpeed2TextBoxAsFloat
        {
            get
            {
                float speed;
                if (float.TryParse(presetCameraSpeed2TextBox.Text, out speed))
                {
                    return speed;
                }
                return 0f;
            }
            set
            {
                presetCameraSpeed2TextBox.Text = value.ToString();
            }
        }

        public float PresetCameraSpeed3TextBoxAsFloat
        {
            get
            {
                float speed;
                if (float.TryParse(presetCameraSpeed3TextBox.Text, out speed))
                {
                    return speed;
                }
                return 0f;
            }
            set
            {
                presetCameraSpeed3TextBox.Text = value.ToString();
            }
        }

        public float PresetCameraSpeed4TextBoxAsFloat
        {
            get
            {
                float speed;
                if (float.TryParse(persetCameraSpeed4TextBox.Text, out speed))
                {
                    return speed;
                }
                return 0f;
            }
            set
            {
                persetCameraSpeed4TextBox.Text = value.ToString();
            }
        }

        public bool AccelerateCameraCheckBoxChecked
        {
            get
            {
                return accelerateCameraCheckBox.Checked;
            }
            set
            {
                accelerateCameraCheckBox.Checked = value;
            }
        }

        public float CameraAccelerationRateTextBoxAsFloat
        {
            get
            {
                float acc;
                if(float.TryParse(cameraAccelerationRateTextBox.Text, out acc))
                {
                    return acc;
                }
                return 0f;
            }
            set
            {
                cameraAccelerationRateTextBox.Text = value.ToString();
            }
        }

        public float CameraAccelerationIncrementTextBoxAsFloat
        {
            get
            {
                float inc;
                if (float.TryParse(cameraAccelerationIncrementTextBox.Text, out inc))
                {
                    return inc;
                }
                return 0f;
            }
            set
            {
                cameraAccelerationIncrementTextBox.Text = value.ToString();
            }
        }

        public float PresetCameraAcceleration1TextBoxAsFloat
        {
            get
            {
                float acc;
                if (float.TryParse(presetCameraAcceleration1TextBox.Text, out acc))
                {
                    return acc;
                }
                return 0f;
            }
            set
            {
                presetCameraAcceleration1TextBox.Text = value.ToString();
            }
        }

        public float PresetCameraAcceleration2TextBoxAsFloat
        {
            get
            {
                float acc;
                if (float.TryParse(presetCameraAcceleration2TextBox.Text, out acc))
                {
                    return acc;
                }
                return 0f;
            }
            set
            {
                presetCameraAcceleration2TextBox.Text = value.ToString();
            }
        }

        public float PresetCameraAcceleration3TextBoxAsFloat
        {
            get
            {
                float acc;
                if (float.TryParse(presetCameraAcceleration3TextBox.Text, out acc))
                {
                    return acc;
                }
                return 0f;
            }
            set
            {
                presetCameraAcceleration3TextBox.Text = value.ToString();
            }
        }

        public float PresetCameraAcceleration4TextBoxAsFloat
        {
            get
            {
                float acc;
                if (float.TryParse(presetBarCameraAcceleration4TextBox.Text, out acc))
                {
                    return acc;
                }
                return 0f;
            }
            set
            {
                presetBarCameraAcceleration4TextBox.Text = value.ToString();
            }
        }

        public float CameraTurnRateTextBoxAsFloat
        {
            get
            {
                float dps;
                if (float.TryParse(cameraTurnRateTextBox.Text, out dps))
                {
                    return dps;
                }
                return 0f;
            }
            set
            {
                cameraTurnRateTextBox.Text = value.ToString();
            }
        }

        public float MouseWheelMultiplierTextBoxAsFloat
        {
            get
            {
                float mult;
                if (float.TryParse(mouseWheelMultiplierTextBox.Text, out mult))
                {
                    return mult;
                }
                return 0f;
            }
            set
            {
                mouseWheelMultiplierTextBox.Text = value.ToString();
            }
        }

        public float Preset1MWMTextBoxAsFloat
        {
            get
            {
                float preset;
                if (float.TryParse(presetMWM1TextBox.Text, out preset))
                {
                    return preset;
                }
                return 0f;
            }
            set
            {
                presetMWM1TextBox.Text = value.ToString();
            }

        }

        public float Preset2MWMTextBoxAsFloat
        {
            get
            {
                float preset;
                if (float.TryParse(presetMWM2TextBox.Text, out preset))
                {
                    return preset;
                }
                return 0f;
            }
            set
            {
                presetMWM2TextBox.Text = value.ToString();
            }
        }

        public float Preset3MWMTextBoxAsFloat
        {
            get
            {
                float preset;
                if (float.TryParse(presetMWM3TextBox.Text, out preset))
                {
                    return preset;
                }
                return 0f;
            }
            set
            {
                presetMWM3TextBox.Text = value.ToString();
            }
        }

        public float Preset4MWMTextBoxAsFloat
        {
            get
            {
                float preset;
                if (float.TryParse(presetMWM4TextBox.Text, out preset))
                {
                    return preset;
                }
                return 0f;
            }
            set
            {
                presetMWM4TextBox.Text = value.ToString();
            }
        }

        public List<string> RepositoryDirectoryList
        {
            get
            {
                return repositoryDirectoryList;
            }
            set
            {
                repositoryDirectoryList = new List<string>(value);
                repositoryDirectoryListLabel.Text = RepositoryClass.Instance.MakeRepositoryDirectoryListString(repositoryDirectoryList);
//                 filesListBox.Items.Clear();
//                 foreach (string dir in value) {
//                     filesListBox.Items.Add(dir);
//                 }
            }
        }

        #endregion CameraControlPanel

        private void floatValidateEvent(object sender, CancelEventArgs e)
        {
            if (!ValidityHelperClass.isFloat(((TextBox)sender).Text))
            {
                Color textColor = Color.Red;
                ((TextBox)sender).ForeColor = textColor;
            }
            else
            {
				Color textColor = Color.Black;
				((TextBox)sender).ForeColor = textColor;
            }
        }

        private void intValidateEvent(object sender, CancelEventArgs e)
        {
            if (!ValidityHelperClass.isInt(((TextBox)sender).Text))
            {
                Color textColor = Color.Red;
                ((TextBox)sender).ForeColor = textColor;
            }
            else
            {
                Color textColor = Color.Black;
                ((TextBox)sender).ForeColor = textColor;
            }
        }

        private void uintAutoSaveValidateEvent(object sender, CancelEventArgs e)
        {
            if (!ValidityHelperClass.isUint(((TextBox)sender).Text) || uint.Parse(((TextBox)sender).Text) == 0)
            {
                Color textColor = Color.Red;
                ((TextBox)sender).ForeColor = textColor;
            }
            else
            {
                Color textColor = Color.Black;
                ((TextBox)sender).ForeColor = textColor;
            }
        }

        private void uintValidateEvent(object sender, CancelEventArgs e)
        {
            if (!ValidityHelperClass.isUint(((TextBox)sender).Text))
            {
                Color textColor = Color.Red;
                ((TextBox)sender).ForeColor = textColor;
            }
            else
            {
                Color textColor = Color.Black;
                ((TextBox)sender).ForeColor = textColor;
            }
        }


        public bool okButton_validating()
        {
            if (!ValidityHelperClass.isUint(autoSaveTimeTextBox.Text) || uint.Parse(autoSaveTimeTextBox.Text) == 0)
            {
                return false;
            }
            else
            {
                if (!maxFPSValidate())
                {
                    return false;
                }
                else
                {
                    if (!ValidityHelperClass.isFloat(cameraNearDistanceTextBox.Text))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void displayRoadMarkerCheckBox_CheckedChange(object sender, EventArgs e)
        {
            if (displayRoadMarkerCheckBox.Checked && disableAllMarkersDisplayCheckBox.Checked)
            {
                disableAllMarkersDisplayCheckBox.Checked = false;
            }
            else
            {
                if (!displayMarkerPointsCheckBox.Checked && !displayRoadMarkerCheckBox.Checked &&
                    !displayRegionMarkerCheckBox.Checked && !displayPointLightMarkersCheckbox.Checked &&
                    !disableAllMarkersDisplayCheckBox.Checked)
                {
                    disableAllMarkersDisplayCheckBox.Checked = true;
                }
            }
        }

        private void displayMarkerPointCheckBox_CheckedChange(object sender, EventArgs e)
        {
            if (displayMarkerPointsCheckBox.Checked && disableAllMarkersDisplayCheckBox.Checked)
            {
                disableAllMarkersDisplayCheckBox.Checked = false;
            }
            else
            {
                if (!displayMarkerPointsCheckBox.Checked && !displayRoadMarkerCheckBox.Checked &&
                    !displayRegionMarkerCheckBox.Checked && !displayPointLightMarkersCheckbox.Checked &&
                    !disableAllMarkersDisplayCheckBox.Checked)
                {
                    disableAllMarkersDisplayCheckBox.Checked = true;
                }
            }
        }

        private void displayRegionMarkerCheckBox_CheckedChange(object sender, EventArgs e)
        {
            if (displayRegionMarkerCheckBox.Checked && disableAllMarkersDisplayCheckBox.Checked)
            {
                disableAllMarkersDisplayCheckBox.Checked = false;
            }
            else
            {
                if (!displayMarkerPointsCheckBox.Checked && !displayRoadMarkerCheckBox.Checked &&
                    !displayRegionMarkerCheckBox.Checked && !displayPointLightMarkersCheckbox.Checked &&
                    !disableAllMarkersDisplayCheckBox.Checked)
                {
                    disableAllMarkersDisplayCheckBox.Checked = true;
                }
            }
        }

        private void displayPointLightMarkerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (displayPointLightMarkersCheckbox.Checked && disableAllMarkersDisplayCheckBox.Checked)
            {
                disableAllMarkersDisplayCheckBox.Checked = false;
            }
            else
            {
                if (!displayMarkerPointsCheckBox.Checked && !displayRoadMarkerCheckBox.Checked &&
                    !displayRegionMarkerCheckBox.Checked && !displayPointLightMarkersCheckbox.Checked &&
                    !disableAllMarkersDisplayCheckBox.Checked)
                {
                    disableAllMarkersDisplayCheckBox.Checked = true;
                }
            }
        }

        private void disableAllMarkerDisplayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (disableAllMarkersDisplayCheckBox.Checked)
            {
                displayMarkerPointsCheckBox.Checked = false;
                displayRegionMarkerCheckBox.Checked = false;
                displayRoadMarkerCheckBox.Checked = false;
                displayPointLightMarkersCheckbox.Checked = false;
            }
        }
        
        private void disableAllMarkersDisplayCheckBox_Click(object sender, EventArgs e)
        {
            if (!disableAllMarkersDisplayCheckBox.Checked)
            {
                displayMarkerPointsCheckBox.Checked = true;
                displayRegionMarkerCheckBox.Checked = true;
                displayRoadMarkerCheckBox.Checked = true;
                displayPointLightMarkersCheckbox.Checked = true;
            }
        }



        private void maxFPSTextbox_Validate(object sender, EventArgs e)
        {
            maxFPSValidate();
        }

        private bool maxFPSValidate()
        {
            if (maxFramesPerSecondCheckBox.Checked)
            {
                if (ValidityHelperClass.isUint(maxFPSTextbox.Text) && uint.Parse(maxFPSTextbox.Text) > 0)
                {
                    return true;
                }
                maxFPSTextbox.ForeColor = Color.Red;
                return false;
            }
            return true;
        }


        private void cameraFollowsTerrainCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (cameraFollowsTerrainCheckBox.Checked && !cameraStaysAboveTerrainCheckbox.Checked)
            {
                cameraStaysAboveTerrainCheckbox.Checked = true;
            }
        }

        private void maxFramesPerSecondCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (maxFramesPerSecondCheckBox.Checked)
            {
                maxFPSTextbox.Enabled = true;
            }
            else
            {
                maxFPSTextbox.Enabled = false;
            }
        }

    }
}
