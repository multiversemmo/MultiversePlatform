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

namespace CollisionLibTest
{
    partial class CollisionTestForm
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
            this.FourCollisionsButton = new System.Windows.Forms.Button();
            this.CollideWithRandomSphereTreeButton = new System.Windows.Forms.Button();
            this.RandomSpheresButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FourCollisionsButton
            // 
            this.FourCollisionsButton.Location = new System.Drawing.Point(28, 24);
            this.FourCollisionsButton.Name = "FourCollisionsButton";
            this.FourCollisionsButton.Size = new System.Drawing.Size(457, 26);
            this.FourCollisionsButton.TabIndex = 0;
            this.FourCollisionsButton.Text = "4 collision objects on a square; capsule mo in center colliding with each";
            this.FourCollisionsButton.UseVisualStyleBackColor = true;
            this.FourCollisionsButton.Click += new System.EventHandler(this.FourCollisionsButton_Click);
            // 
            // CollideWithRandomSphereTreeButton
            // 
            this.CollideWithRandomSphereTreeButton.Location = new System.Drawing.Point(28, 131);
            this.CollideWithRandomSphereTreeButton.Name = "CollideWithRandomSphereTreeButton";
            this.CollideWithRandomSphereTreeButton.Size = new System.Drawing.Size(457, 26);
            this.CollideWithRandomSphereTreeButton.TabIndex = 5;
            this.CollideWithRandomSphereTreeButton.Text = "Collide multi-part object with random sphere tree";
            this.CollideWithRandomSphereTreeButton.UseVisualStyleBackColor = true;
            // 
            // RandomSpheresButton
            // 
            this.RandomSpheresButton.Location = new System.Drawing.Point(28, 78);
            this.RandomSpheresButton.Name = "RandomSpheresButton";
            this.RandomSpheresButton.Size = new System.Drawing.Size(457, 26);
            this.RandomSpheresButton.TabIndex = 6;
            this.RandomSpheresButton.Text = "Generate lotsa randomly-placed collision objects in spheretree; and delete them";
            this.RandomSpheresButton.UseVisualStyleBackColor = true;
            this.RandomSpheresButton.Click += new System.EventHandler(this.RandomSpheresButton_Click);
            // 
            // CollisionTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 353);
            this.Controls.Add(this.RandomSpheresButton);
            this.Controls.Add(this.CollideWithRandomSphereTreeButton);
            this.Controls.Add(this.FourCollisionsButton);
            this.Name = "CollisionTestForm";
            this.Text = "Test Collision Library";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button FourCollisionsButton;
        private System.Windows.Forms.Button CollideWithRandomSphereTreeButton;
        private System.Windows.Forms.Button RandomSpheresButton;
    }
}

