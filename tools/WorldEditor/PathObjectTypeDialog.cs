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
using System.Xml;
using System.Windows.Forms;
using Multiverse.CollisionLib;

namespace Multiverse.Tools.WorldEditor {
    public partial class PathObjectTypeDialog : Form {
        public PathObjectTypeDialog() {
            InitializeComponent();
        }

        // If we're creating a path object type instance, this is filled
        // in when the user clicks OK.  If we're editing a path
        // object type instance, the values are into this instance when the
        // user clicks OK
        public PathObjectType pathObjectType = null;

        // If the pathObjectType instance is set, initialize the dialog
        // box elements from the instance
        private void PathObjectTypeDialog_Shown(object sender, EventArgs e) {
            if (pathObjectType != null) {
                pathObjectTypeName.Text = pathObjectType.name;
                pathObjectTypeHeight.Text = pathObjectType.height.ToString();
                pathObjectTypeWidth.Text = pathObjectType.width.ToString();
                pathObjectTypeSlope.Text = pathObjectType.maxClimbSlope.ToString();
                pathObjectTypeGridResolution.Text = pathObjectType.gridResolution.ToString();
                pathObjectTypeMaxDisjointDistance.Text = pathObjectType.maxDisjointDistance.ToString();
                pathObjectTypeMinimumFeatureSize.Text = pathObjectType.minimumFeatureSize.ToString();
                UpdateDialogTitleBar();
            }
        }
        
        private void okButton_Click(object sender, EventArgs e) {
            // Validate the fields
            string name = pathObjectTypeName.Text;
            if (name.Length == 0) {
                barf("The path object type name may not be null");
                return;
            }
            int minimumFeatureSize;
            float height, width, maxClimbSlope, gridResolution, maxDisjointDistance;
            if (parseFloat(pathObjectTypeHeight, out height, "path object type class height", float.MaxValue) &&
                parseFloat(pathObjectTypeWidth, out width, "path object type class width", float.MaxValue) &&
                parseFloat(pathObjectTypeSlope, out maxClimbSlope, "path object type class maximum climb slope", 1.0f) &&
                parseFloat(pathObjectTypeGridResolution, out gridResolution, "path object type class grid resolution fraction", 1.0f) &&
                parseFloat(pathObjectTypeMaxDisjointDistance, out maxDisjointDistance,
                    "path object type class distance between collision volumes", float.MaxValue) &&
                parseInt(pathObjectTypeMinimumFeatureSize, out minimumFeatureSize,
                    "maximum number of grid cells to ignore when constructing grid rectangles", int.MaxValue)) {
                // If we're editing, copy updated values to the instance
                if (pathObjectType != null) {
                    pathObjectType.AcceptValues(name, height, width, maxClimbSlope,
                        gridResolution, maxDisjointDistance, minimumFeatureSize);
                }
                    // Else create a new instance
                else {
                    pathObjectType = new PathObjectType(name, height, width, maxClimbSlope,
                        gridResolution, maxDisjointDistance, minimumFeatureSize);
                }
                WorldRoot.Instance.PathObjectTypes.AllObjectsDirty = true;
                DialogResult = DialogResult.OK;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
        }
        
        private void pathObjectTypeName_TextChanged(object sender, EventArgs e) {
            UpdateDialogTitleBar();
        }

        private void UpdateDialogTitleBar() {
            Text = "Editing Path Object Type " + pathObjectTypeName.Text;
        }

        private bool parseFloat(TextBox box, out float value, string description, float maxValue) {
            value = 0;
            try {
                value = float.Parse(box.Text);
            }
            catch (Exception) {
                barf("Error parsing " + description + "; '" + box.Text + " is not a legal floating point number"); 
                return false;
            }
            if (value >= maxValue) {
                barf(string.Format("The value of {0} + is '{1}', is larger than the maximum allowed, which is '{2}'",
                                   description, value, maxValue));
                return false;
            }
            return true;
        }
        
        private bool parseInt(TextBox box, out int value, string description, int maxValue) {
            value = 0;
            try {
                value = int.Parse(box.Text);
            }
            catch (Exception) {
                barf("Error parsing " + description + "; '" + box.Text + " is not a legal integer"); 
                return false;
            }
            if (value >= maxValue) {
                barf(string.Format("The value of {0} + is '{1}', is larger than the maximum allowed, which is '{2}'",
                                   description, value, maxValue));
                return false;
            }
            return true;
        }
        
        private void barf(string message) {
            MessageBox.Show(message, "Create Class Dialog Error",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

    }

}
