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

namespace MVImportTool
{
    public partial class ImporterConfigurationDialog : Form
    {
        internal ImportToolSettings Settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }
        ImportToolSettings m_Settings;

        public string ConversionToolExeFile
        {
            get { return importerConfiguration1.ConversionToolExeFile; }
            set { importerConfiguration1.ConversionToolExeFile = value; }
        }

        public string WorkingFolder
        {
            get { return importerConfiguration1.WorkingFolder; }
            set { importerConfiguration1.WorkingFolder = value; }
        }

        public ImporterConfigurationDialog()
        {
            InitializeComponent();
        }

        private void ImporterConfigurationDialog_Load( object sender, EventArgs e )
        {
            WorkingFolder = Settings.WorkingFolder;
            ConversionToolExeFile = Settings.ConversionToolExeFile;
        }

        private void OkButton_Click( object sender, EventArgs e )
        {
            Settings.WorkingFolder = WorkingFolder;
            Settings.ConversionToolExeFile = ConversionToolExeFile;
        }
    }
}
