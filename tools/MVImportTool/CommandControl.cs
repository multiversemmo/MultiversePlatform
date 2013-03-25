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
using System.Windows.Forms;

using FilterFlags = MVImportTool.RepositoryInstaller.InstallFilterFlags;
using OverwritePolicyFlags = MVImportTool.RepositoryInstaller.OverwritePolicyFlags;

namespace MVImportTool
{
    /// <summary>
    /// Custom control that sets up the Import command.
    /// </summary>
    public partial class CommandControl : UserControl
    {
        #region Properties

        internal ImportToolSettings Settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }
        ImportToolSettings m_Settings;

        #endregion Properties

        #region Construction & loading
        public CommandControl()
        {
            InitializeComponent();
        }

        private void CommandControl_Load( object sender, EventArgs e )
        {
            if( null != Settings )
            {
                ConvertCheckBox.Checked = Settings.ConvertSource;
                CommandFlagTextBox.Text = Settings.ConversionCommandFlags;
                InstallCheckBox.Checked = Settings.InstallResult;
                ConfirmInstallationCheckBox.Checked = Settings.ConfirmInstall;
                SetInstallFilterCheckBoxes( (FilterFlags) Settings.InstallFilter );
                SetOverwritePolicyRadioButtons( (OverwritePolicyFlags) Settings.OverwritePolicy );
            }
        }
        #endregion Construction & loading

        private void InstallCheckBox_CheckedChanged( object sender, EventArgs e )
        {
            CheckBox masterCopyEnableCheckBox = sender as CheckBox;

            if( null != masterCopyEnableCheckBox )
            {
                SetCopyEnable( masterCopyEnableCheckBox.Checked );
            }
        }

        private void SetCopyEnable( bool enableState )
        {
            InstallControlsPanel.Enabled = enableState;

            Settings.InstallResult = enableState;
        }

        // Set the filter check box states from the FilterFlags
        private void SetInstallFilterCheckBoxes( FilterFlags flags )
        {
            InstallMeshCheckBox.Checked     = Convert.ToBoolean( flags & FilterFlags.Mesh );
            InstallMaterialCheckBox.Checked = Convert.ToBoolean( flags & FilterFlags.Materials );
            InstallSkeletonCheckBox.Checked = Convert.ToBoolean( flags & FilterFlags.Skeleton );
            InstallPhysicsCheckBox.Checked  = Convert.ToBoolean( flags & FilterFlags.Physics );
            InstallTexturesCheckBox.Checked = Convert.ToBoolean( flags & FilterFlags.Textures );
        }

        // Determine which checkbox changed, and flip the correlated bit in the FilterFlags
        private void InstallFilterCheckBox_CheckedChanged( object sender, EventArgs e )
        {
            if( sender.Equals( InstallMeshCheckBox ) )
            {
                SetInstallFilterState( InstallMeshCheckBox.Checked, FilterFlags.Mesh );
            }
            else if( sender.Equals( InstallMaterialCheckBox ) )
            {
                SetInstallFilterState( InstallMaterialCheckBox.Checked, FilterFlags.Materials );
            }
            else if( sender.Equals( InstallSkeletonCheckBox ) )
            {
                SetInstallFilterState( InstallSkeletonCheckBox.Checked, FilterFlags.Skeleton );
            }
            else if( sender.Equals( InstallPhysicsCheckBox ) )
            {
                SetInstallFilterState( InstallPhysicsCheckBox.Checked, FilterFlags.Physics );
            }
            else if( sender.Equals( InstallTexturesCheckBox ) )
            {
                SetInstallFilterState( InstallTexturesCheckBox.Checked, FilterFlags.Textures );
            }
        }

        // Flip one bit in the InstallFilter flags
        private void SetInstallFilterState( bool state, FilterFlags flag )
        {
            if( state )
            {
                Settings.InstallFilter = (Settings.InstallFilter | Convert.ToUInt32( flag ));
            }
            else
            {
                Settings.InstallFilter = (Settings.InstallFilter & ~Convert.ToUInt32( flag ));
            }
        }

        private void ConvertCheckBox_CheckedChanged( object sender, EventArgs e )
        {
            Settings.ConvertSource = ConvertCheckBox.Checked;
        }

        private void ConfirmInstallationCheckBox_CheckedChanged( object sender, EventArgs e )
        {
            Settings.ConfirmInstall = ConfirmInstallationCheckBox.Checked;
        }

        #region Overwrite Policy radio button group
        private void OverwritePolicyRadioButton_CheckedChanged( object sender, EventArgs e )
        {
            if( AlwaysRadioButton.Equals( sender ) )
            {
                Settings.OverwritePolicy = Convert.ToUInt32( OverwritePolicyFlags.Always );
            }
            else if( AskRadioButton.Equals( sender ) )
            {
                Settings.OverwritePolicy = Convert.ToUInt32( OverwritePolicyFlags.Ask );
            }
            else if( NeverRadioButton.Equals( sender ) )
            {
                Settings.OverwritePolicy = Convert.ToUInt32( OverwritePolicyFlags.Never );
            }
        }

        private void SetOverwritePolicyRadioButtons( OverwritePolicyFlags setting )
        {
            switch( setting )
            {
                case OverwritePolicyFlags.Always: { AlwaysRadioButton.Checked = true; break; }
                case OverwritePolicyFlags.Never:  { NeverRadioButton.Checked  = true; break; }
                case OverwritePolicyFlags.Ask:    { AskRadioButton.Checked    = true; break; }
            }
        }
        #endregion Overwrite Policy radio button group

        private void CommandFlagTextBox_TextChanged( object sender, EventArgs e )
        {
            Settings.ConversionCommandFlags = CommandFlagTextBox.Text;
        }
    }
}
