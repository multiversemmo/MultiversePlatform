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

namespace Multiverse.Tools.ModelViewer {
    partial class BoneDisplay {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.boneTreeView = new System.Windows.Forms.TreeView();
            this.bonePositionLabel = new System.Windows.Forms.Label();
            this.bonePosition = new System.Windows.Forms.Label();
            this.boneNameLabel = new System.Windows.Forms.Label();
            this.boneRotationLabel = new System.Windows.Forms.Label();
            this.boneRotation = new System.Windows.Forms.Label();
            this.relPositionLabel = new System.Windows.Forms.Label();
            this.relRotationLabel = new System.Windows.Forms.Label();
            this.relPosition = new System.Windows.Forms.Label();
            this.relRotation = new System.Windows.Forms.Label();
            this.boneRotation2 = new System.Windows.Forms.Label();
            this.relRotation2 = new System.Windows.Forms.Label();
            this.bindPositionLabel = new System.Windows.Forms.Label();
            this.bindPosition = new System.Windows.Forms.Label();
            this.bindRotationLabel = new System.Windows.Forms.Label();
            this.bindRotation = new System.Windows.Forms.Label();
            this.bindRotation2 = new System.Windows.Forms.Label();
            this.animationTime = new System.Windows.Forms.Label();
            this.keyFrame1Rotation2 = new System.Windows.Forms.Label();
            this.keyFrame1Rotation = new System.Windows.Forms.Label();
            this.keyFrame1RotationLabel = new System.Windows.Forms.Label();
            this.keyFrame1Position = new System.Windows.Forms.Label();
            this.keyFrame1PositionLabel = new System.Windows.Forms.Label();
            this.keyFrame2Rotation2 = new System.Windows.Forms.Label();
            this.keyFrame2Rotation = new System.Windows.Forms.Label();
            this.keyFrame2RotationLabel = new System.Windows.Forms.Label();
            this.keyFrame2Position = new System.Windows.Forms.Label();
            this.keyFrame2PositionLabel = new System.Windows.Forms.Label();
            this.animationName = new System.Windows.Forms.Label();
            this.keyFrame1TimeLabel = new System.Windows.Forms.Label();
            this.keyFrame2TimeLabel = new System.Windows.Forms.Label();
            this.keyFrame2Time = new System.Windows.Forms.Label();
            this.keyFrame1Time = new System.Windows.Forms.Label();
            this.prevKeyframeLabel = new System.Windows.Forms.Label();
            this.nextKeyframeLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // boneTreeView
            // 
            this.boneTreeView.Location = new System.Drawing.Point(7, 12);
            this.boneTreeView.Name = "boneTreeView";
            this.boneTreeView.Size = new System.Drawing.Size(349, 438);
            this.boneTreeView.TabIndex = 0;
            this.boneTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.boneTreeView_AfterSelect);
            // 
            // bonePositionLabel
            // 
            this.bonePositionLabel.AutoSize = true;
            this.bonePositionLabel.Location = new System.Drawing.Point(362, 58);
            this.bonePositionLabel.Name = "bonePositionLabel";
            this.bonePositionLabel.Size = new System.Drawing.Size(44, 13);
            this.bonePositionLabel.TabIndex = 1;
            this.bonePositionLabel.Text = "Position";
            // 
            // bonePosition
            // 
            this.bonePosition.AutoSize = true;
            this.bonePosition.Location = new System.Drawing.Point(471, 58);
            this.bonePosition.Name = "bonePosition";
            this.bonePosition.Size = new System.Drawing.Size(43, 13);
            this.bonePosition.TabIndex = 2;
            this.bonePosition.Text = "position";
            // 
            // boneNameLabel
            // 
            this.boneNameLabel.AutoSize = true;
            this.boneNameLabel.Location = new System.Drawing.Point(362, 35);
            this.boneNameLabel.Name = "boneNameLabel";
            this.boneNameLabel.Size = new System.Drawing.Size(63, 13);
            this.boneNameLabel.TabIndex = 3;
            this.boneNameLabel.Text = "Bone Name";
            // 
            // boneRotationLabel
            // 
            this.boneRotationLabel.AutoSize = true;
            this.boneRotationLabel.Location = new System.Drawing.Point(362, 71);
            this.boneRotationLabel.Name = "boneRotationLabel";
            this.boneRotationLabel.Size = new System.Drawing.Size(47, 13);
            this.boneRotationLabel.TabIndex = 4;
            this.boneRotationLabel.Text = "Rotation";
            // 
            // boneRotation
            // 
            this.boneRotation.AutoSize = true;
            this.boneRotation.Location = new System.Drawing.Point(471, 71);
            this.boneRotation.Name = "boneRotation";
            this.boneRotation.Size = new System.Drawing.Size(42, 13);
            this.boneRotation.TabIndex = 5;
            this.boneRotation.Text = "rotation";
            // 
            // relPositionLabel
            // 
            this.relPositionLabel.AutoSize = true;
            this.relPositionLabel.Location = new System.Drawing.Point(362, 116);
            this.relPositionLabel.Name = "relPositionLabel";
            this.relPositionLabel.Size = new System.Drawing.Size(86, 13);
            this.relPositionLabel.TabIndex = 6;
            this.relPositionLabel.Text = "Relative Position";
            // 
            // relRotationLabel
            // 
            this.relRotationLabel.AutoSize = true;
            this.relRotationLabel.Location = new System.Drawing.Point(362, 129);
            this.relRotationLabel.Name = "relRotationLabel";
            this.relRotationLabel.Size = new System.Drawing.Size(89, 13);
            this.relRotationLabel.TabIndex = 7;
            this.relRotationLabel.Text = "Relative Rotation";
            // 
            // relPosition
            // 
            this.relPosition.AutoSize = true;
            this.relPosition.Location = new System.Drawing.Point(471, 116);
            this.relPosition.Name = "relPosition";
            this.relPosition.Size = new System.Drawing.Size(43, 13);
            this.relPosition.TabIndex = 8;
            this.relPosition.Text = "position";
            // 
            // relRotation
            // 
            this.relRotation.AutoSize = true;
            this.relRotation.Location = new System.Drawing.Point(471, 129);
            this.relRotation.Name = "relRotation";
            this.relRotation.Size = new System.Drawing.Size(42, 13);
            this.relRotation.TabIndex = 9;
            this.relRotation.Text = "rotation";
            // 
            // boneRotation2
            // 
            this.boneRotation2.AutoSize = true;
            this.boneRotation2.Location = new System.Drawing.Point(471, 84);
            this.boneRotation2.Name = "boneRotation2";
            this.boneRotation2.Size = new System.Drawing.Size(63, 13);
            this.boneRotation2.TabIndex = 10;
            this.boneRotation2.Text = "rotationXYZ";
            // 
            // relRotation2
            // 
            this.relRotation2.AutoSize = true;
            this.relRotation2.Location = new System.Drawing.Point(471, 142);
            this.relRotation2.Name = "relRotation2";
            this.relRotation2.Size = new System.Drawing.Size(63, 13);
            this.relRotation2.TabIndex = 11;
            this.relRotation2.Text = "rotationXYZ";
            // 
            // bindPositionLabel
            // 
            this.bindPositionLabel.AutoSize = true;
            this.bindPositionLabel.Location = new System.Drawing.Point(362, 172);
            this.bindPositionLabel.Name = "bindPositionLabel";
            this.bindPositionLabel.Size = new System.Drawing.Size(68, 13);
            this.bindPositionLabel.TabIndex = 12;
            this.bindPositionLabel.Text = "Bind Position";
            // 
            // bindPosition
            // 
            this.bindPosition.AutoSize = true;
            this.bindPosition.Location = new System.Drawing.Point(471, 172);
            this.bindPosition.Name = "bindPosition";
            this.bindPosition.Size = new System.Drawing.Size(43, 13);
            this.bindPosition.TabIndex = 13;
            this.bindPosition.Text = "position";
            // 
            // bindRotationLabel
            // 
            this.bindRotationLabel.AutoSize = true;
            this.bindRotationLabel.Location = new System.Drawing.Point(362, 185);
            this.bindRotationLabel.Name = "bindRotationLabel";
            this.bindRotationLabel.Size = new System.Drawing.Size(71, 13);
            this.bindRotationLabel.TabIndex = 14;
            this.bindRotationLabel.Text = "Bind Rotation";
            // 
            // bindRotation
            // 
            this.bindRotation.AutoSize = true;
            this.bindRotation.Location = new System.Drawing.Point(471, 185);
            this.bindRotation.Name = "bindRotation";
            this.bindRotation.Size = new System.Drawing.Size(42, 13);
            this.bindRotation.TabIndex = 15;
            this.bindRotation.Text = "rotation";
            // 
            // bindRotation2
            // 
            this.bindRotation2.AutoSize = true;
            this.bindRotation2.Location = new System.Drawing.Point(471, 198);
            this.bindRotation2.Name = "bindRotation2";
            this.bindRotation2.Size = new System.Drawing.Size(63, 13);
            this.bindRotation2.TabIndex = 16;
            this.bindRotation2.Text = "rotationXYZ";
            // 
            // animationTime
            // 
            this.animationTime.AutoSize = true;
            this.animationTime.Location = new System.Drawing.Point(471, 12);
            this.animationTime.Name = "animationTime";
            this.animationTime.Size = new System.Drawing.Size(55, 13);
            this.animationTime.TabIndex = 31;
            this.animationTime.Text = "Time: time";
            // 
            // keyFrame1Rotation2
            // 
            this.keyFrame1Rotation2.AutoSize = true;
            this.keyFrame1Rotation2.Location = new System.Drawing.Point(471, 287);
            this.keyFrame1Rotation2.Name = "keyFrame1Rotation2";
            this.keyFrame1Rotation2.Size = new System.Drawing.Size(63, 13);
            this.keyFrame1Rotation2.TabIndex = 36;
            this.keyFrame1Rotation2.Text = "rotationXYZ";
            // 
            // keyFrame1Rotation
            // 
            this.keyFrame1Rotation.AutoSize = true;
            this.keyFrame1Rotation.Location = new System.Drawing.Point(471, 271);
            this.keyFrame1Rotation.Name = "keyFrame1Rotation";
            this.keyFrame1Rotation.Size = new System.Drawing.Size(42, 13);
            this.keyFrame1Rotation.TabIndex = 35;
            this.keyFrame1Rotation.Text = "rotation";
            // 
            // keyFrame1RotationLabel
            // 
            this.keyFrame1RotationLabel.AutoSize = true;
            this.keyFrame1RotationLabel.Location = new System.Drawing.Point(362, 271);
            this.keyFrame1RotationLabel.Name = "keyFrame1RotationLabel";
            this.keyFrame1RotationLabel.Size = new System.Drawing.Size(47, 13);
            this.keyFrame1RotationLabel.TabIndex = 34;
            this.keyFrame1RotationLabel.Text = "Rotation";
            // 
            // keyFrame1Position
            // 
            this.keyFrame1Position.AutoSize = true;
            this.keyFrame1Position.Location = new System.Drawing.Point(471, 255);
            this.keyFrame1Position.Name = "keyFrame1Position";
            this.keyFrame1Position.Size = new System.Drawing.Size(43, 13);
            this.keyFrame1Position.TabIndex = 33;
            this.keyFrame1Position.Text = "position";
            // 
            // keyFrame1PositionLabel
            // 
            this.keyFrame1PositionLabel.AutoSize = true;
            this.keyFrame1PositionLabel.Location = new System.Drawing.Point(362, 255);
            this.keyFrame1PositionLabel.Name = "keyFrame1PositionLabel";
            this.keyFrame1PositionLabel.Size = new System.Drawing.Size(44, 13);
            this.keyFrame1PositionLabel.TabIndex = 32;
            this.keyFrame1PositionLabel.Text = "Position";
            // 
            // keyFrame2Rotation2
            // 
            this.keyFrame2Rotation2.AutoSize = true;
            this.keyFrame2Rotation2.Location = new System.Drawing.Point(471, 371);
            this.keyFrame2Rotation2.Name = "keyFrame2Rotation2";
            this.keyFrame2Rotation2.Size = new System.Drawing.Size(63, 13);
            this.keyFrame2Rotation2.TabIndex = 41;
            this.keyFrame2Rotation2.Text = "rotationXYZ";
            // 
            // keyFrame2Rotation
            // 
            this.keyFrame2Rotation.AutoSize = true;
            this.keyFrame2Rotation.Location = new System.Drawing.Point(471, 355);
            this.keyFrame2Rotation.Name = "keyFrame2Rotation";
            this.keyFrame2Rotation.Size = new System.Drawing.Size(42, 13);
            this.keyFrame2Rotation.TabIndex = 40;
            this.keyFrame2Rotation.Text = "rotation";
            // 
            // keyFrame2RotationLabel
            // 
            this.keyFrame2RotationLabel.AutoSize = true;
            this.keyFrame2RotationLabel.Location = new System.Drawing.Point(362, 355);
            this.keyFrame2RotationLabel.Name = "keyFrame2RotationLabel";
            this.keyFrame2RotationLabel.Size = new System.Drawing.Size(47, 13);
            this.keyFrame2RotationLabel.TabIndex = 39;
            this.keyFrame2RotationLabel.Text = "Rotation";
            // 
            // keyFrame2Position
            // 
            this.keyFrame2Position.AutoSize = true;
            this.keyFrame2Position.Location = new System.Drawing.Point(471, 339);
            this.keyFrame2Position.Name = "keyFrame2Position";
            this.keyFrame2Position.Size = new System.Drawing.Size(43, 13);
            this.keyFrame2Position.TabIndex = 38;
            this.keyFrame2Position.Text = "position";
            // 
            // keyFrame2PositionLabel
            // 
            this.keyFrame2PositionLabel.AutoSize = true;
            this.keyFrame2PositionLabel.Location = new System.Drawing.Point(362, 339);
            this.keyFrame2PositionLabel.Name = "keyFrame2PositionLabel";
            this.keyFrame2PositionLabel.Size = new System.Drawing.Size(44, 13);
            this.keyFrame2PositionLabel.TabIndex = 37;
            this.keyFrame2PositionLabel.Text = "Position";
            // 
            // animationName
            // 
            this.animationName.AutoSize = true;
            this.animationName.Location = new System.Drawing.Point(362, 12);
            this.animationName.Name = "animationName";
            this.animationName.Size = new System.Drawing.Size(85, 13);
            this.animationName.TabIndex = 42;
            this.animationName.Text = "Animation: name";
            // 
            // keyFrame1TimeLabel
            // 
            this.keyFrame1TimeLabel.AutoSize = true;
            this.keyFrame1TimeLabel.Location = new System.Drawing.Point(362, 239);
            this.keyFrame1TimeLabel.Name = "keyFrame1TimeLabel";
            this.keyFrame1TimeLabel.Size = new System.Drawing.Size(30, 13);
            this.keyFrame1TimeLabel.TabIndex = 43;
            this.keyFrame1TimeLabel.Text = "Time";
            // 
            // keyFrame2TimeLabel
            // 
            this.keyFrame2TimeLabel.AutoSize = true;
            this.keyFrame2TimeLabel.Location = new System.Drawing.Point(362, 323);
            this.keyFrame2TimeLabel.Name = "keyFrame2TimeLabel";
            this.keyFrame2TimeLabel.Size = new System.Drawing.Size(30, 13);
            this.keyFrame2TimeLabel.TabIndex = 44;
            this.keyFrame2TimeLabel.Text = "Time";
            // 
            // keyFrame2Time
            // 
            this.keyFrame2Time.AutoSize = true;
            this.keyFrame2Time.Location = new System.Drawing.Point(471, 323);
            this.keyFrame2Time.Name = "keyFrame2Time";
            this.keyFrame2Time.Size = new System.Drawing.Size(26, 13);
            this.keyFrame2Time.TabIndex = 45;
            this.keyFrame2Time.Text = "time";
            // 
            // keyFrame1Time
            // 
            this.keyFrame1Time.AutoSize = true;
            this.keyFrame1Time.Location = new System.Drawing.Point(471, 239);
            this.keyFrame1Time.Name = "keyFrame1Time";
            this.keyFrame1Time.Size = new System.Drawing.Size(26, 13);
            this.keyFrame1Time.TabIndex = 46;
            this.keyFrame1Time.Text = "time";
            // 
            // prevKeyframeLabel
            // 
            this.prevKeyframeLabel.AutoSize = true;
            this.prevKeyframeLabel.Location = new System.Drawing.Point(362, 223);
            this.prevKeyframeLabel.Name = "prevKeyframeLabel";
            this.prevKeyframeLabel.Size = new System.Drawing.Size(95, 13);
            this.prevKeyframeLabel.TabIndex = 47;
            this.prevKeyframeLabel.Text = "Previous Keyframe";
            // 
            // nextKeyframeLabel
            // 
            this.nextKeyframeLabel.AutoSize = true;
            this.nextKeyframeLabel.Location = new System.Drawing.Point(362, 307);
            this.nextKeyframeLabel.Name = "nextKeyframeLabel";
            this.nextKeyframeLabel.Size = new System.Drawing.Size(76, 13);
            this.nextKeyframeLabel.TabIndex = 48;
            this.nextKeyframeLabel.Text = "Next Keyframe";
            // 
            // BoneDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(798, 462);
            this.Controls.Add(this.nextKeyframeLabel);
            this.Controls.Add(this.prevKeyframeLabel);
            this.Controls.Add(this.keyFrame1Time);
            this.Controls.Add(this.keyFrame2Time);
            this.Controls.Add(this.keyFrame2TimeLabel);
            this.Controls.Add(this.keyFrame1TimeLabel);
            this.Controls.Add(this.keyFrame2Rotation2);
            this.Controls.Add(this.keyFrame2Rotation);
            this.Controls.Add(this.keyFrame2RotationLabel);
            this.Controls.Add(this.keyFrame2Position);
            this.Controls.Add(this.keyFrame2PositionLabel);
            this.Controls.Add(this.keyFrame1Rotation2);
            this.Controls.Add(this.keyFrame1Rotation);
            this.Controls.Add(this.keyFrame1RotationLabel);
            this.Controls.Add(this.keyFrame1Position);
            this.Controls.Add(this.keyFrame1PositionLabel);
            this.Controls.Add(this.animationName);
            this.Controls.Add(this.animationTime);
            this.Controls.Add(this.bindRotation2);
            this.Controls.Add(this.bindRotation);
            this.Controls.Add(this.bindRotationLabel);
            this.Controls.Add(this.bindPosition);
            this.Controls.Add(this.bindPositionLabel);
            this.Controls.Add(this.relRotation2);
            this.Controls.Add(this.boneRotation2);
            this.Controls.Add(this.relRotation);
            this.Controls.Add(this.relPosition);
            this.Controls.Add(this.relRotationLabel);
            this.Controls.Add(this.relPositionLabel);
            this.Controls.Add(this.boneRotation);
            this.Controls.Add(this.boneRotationLabel);
            this.Controls.Add(this.boneNameLabel);
            this.Controls.Add(this.bonePosition);
            this.Controls.Add(this.bonePositionLabel);
            this.Controls.Add(this.boneTreeView);
            this.Name = "BoneDisplay";
            this.ShowInTaskbar = false;
            this.Text = "Bone Display";
            this.Load += new System.EventHandler(this.BoneDisplay_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView boneTreeView;
        private System.Windows.Forms.Label bonePositionLabel;
        private System.Windows.Forms.Label bonePosition;
        private System.Windows.Forms.Label boneNameLabel;
        private System.Windows.Forms.Label boneRotationLabel;
        private System.Windows.Forms.Label boneRotation;
        private System.Windows.Forms.Label relPositionLabel;
        private System.Windows.Forms.Label relRotationLabel;
        private System.Windows.Forms.Label relPosition;
        private System.Windows.Forms.Label relRotation;
        private System.Windows.Forms.Label boneRotation2;
        private System.Windows.Forms.Label relRotation2;
        private System.Windows.Forms.Label bindPositionLabel;
        private System.Windows.Forms.Label bindPosition;
        private System.Windows.Forms.Label bindRotationLabel;
        private System.Windows.Forms.Label bindRotation;
        private System.Windows.Forms.Label bindRotation2;
        private System.Windows.Forms.Label animationTime;
        private System.Windows.Forms.Label keyFrame1Rotation2;
        private System.Windows.Forms.Label keyFrame1Rotation;
        private System.Windows.Forms.Label keyFrame1RotationLabel;
        private System.Windows.Forms.Label keyFrame1Position;
        private System.Windows.Forms.Label keyFrame1PositionLabel;
        private System.Windows.Forms.Label keyFrame2Rotation2;
        private System.Windows.Forms.Label keyFrame2Rotation;
        private System.Windows.Forms.Label keyFrame2RotationLabel;
        private System.Windows.Forms.Label keyFrame2Position;
        private System.Windows.Forms.Label keyFrame2PositionLabel;
        private System.Windows.Forms.Label animationName;
        private System.Windows.Forms.Label keyFrame1TimeLabel;
        private System.Windows.Forms.Label keyFrame2TimeLabel;
        private System.Windows.Forms.Label keyFrame2Time;
        private System.Windows.Forms.Label keyFrame1Time;
        private System.Windows.Forms.Label prevKeyframeLabel;
        private System.Windows.Forms.Label nextKeyframeLabel;
    }
}
