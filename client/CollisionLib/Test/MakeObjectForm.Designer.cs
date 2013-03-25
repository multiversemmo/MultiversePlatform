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
    partial class MakeObjectForm
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
            this.ShapeTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.AABBPanel = new System.Windows.Forms.Panel();
            this.label20 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.AABBMaxZ = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.AABBMaxY = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.AABBMaxX = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.AABBMinZ = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.AABBMinY = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.AABBMinX = new System.Windows.Forms.TextBox();
            this.CapsulePanel = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.CapsuleTopZ = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.CapsuleTopY = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.CapsuleTopX = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.CapsuleBottomZ = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.CapsuleBottomY = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.CapsuleBottomX = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.OBBPanel = new System.Windows.Forms.Panel();
            this.label32 = new System.Windows.Forms.Label();
            this.OBBAngleY = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.OBBExtent2 = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.OBBExtent1 = new System.Windows.Forms.TextBox();
            this.label27 = new System.Windows.Forms.Label();
            this.OBBExtent0 = new System.Windows.Forms.TextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.OBBCenterZ = new System.Windows.Forms.TextBox();
            this.label29 = new System.Windows.Forms.Label();
            this.OBBCenterY = new System.Windows.Forms.TextBox();
            this.label30 = new System.Windows.Forms.Label();
            this.OBBCenterX = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.OBBAngleX = new System.Windows.Forms.TextBox();
            this.SpherePanel = new System.Windows.Forms.Panel();
            this.label33 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SphereCenterZ = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SphereCenterY = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SphereCenterX = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.AddObjectButton = new System.Windows.Forms.Button();
            this.AABBPanel.SuspendLayout();
            this.CapsulePanel.SuspendLayout();
            this.OBBPanel.SuspendLayout();
            this.SpherePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ShapeTypeComboBox
            // 
            this.ShapeTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ShapeTypeComboBox.FormattingEnabled = true;
            this.ShapeTypeComboBox.Items.AddRange(new object[] {
            "Sphere",
            "Capsule",
            "AABB",
            "OBB"});
            this.ShapeTypeComboBox.Location = new System.Drawing.Point(133, 22);
            this.ShapeTypeComboBox.Name = "ShapeTypeComboBox";
            this.ShapeTypeComboBox.Size = new System.Drawing.Size(111, 21);
            this.ShapeTypeComboBox.TabIndex = 0;
            this.ShapeTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.ShapeTypeComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Choose Object Type";
            // 
            // AABBPanel
            // 
            this.AABBPanel.Controls.Add(this.label20);
            this.AABBPanel.Controls.Add(this.label14);
            this.AABBPanel.Controls.Add(this.AABBMaxZ);
            this.AABBPanel.Controls.Add(this.label15);
            this.AABBPanel.Controls.Add(this.AABBMaxY);
            this.AABBPanel.Controls.Add(this.label16);
            this.AABBPanel.Controls.Add(this.AABBMaxX);
            this.AABBPanel.Controls.Add(this.label17);
            this.AABBPanel.Controls.Add(this.AABBMinZ);
            this.AABBPanel.Controls.Add(this.label18);
            this.AABBPanel.Controls.Add(this.AABBMinY);
            this.AABBPanel.Controls.Add(this.label19);
            this.AABBPanel.Controls.Add(this.AABBMinX);
            this.AABBPanel.Location = new System.Drawing.Point(12, 68);
            this.AABBPanel.Name = "AABBPanel";
            this.AABBPanel.Size = new System.Drawing.Size(377, 246);
            this.AABBPanel.TabIndex = 10;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(5, 5);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(121, 20);
            this.label20.TabIndex = 15;
            this.label20.Text = "Specify AABB";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(106, 186);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(68, 13);
            this.label14.TabIndex = 14;
            this.label14.Text = "AABB Max Z";
            // 
            // AABBMaxZ
            // 
            this.AABBMaxZ.Location = new System.Drawing.Point(180, 185);
            this.AABBMaxZ.Name = "AABBMaxZ";
            this.AABBMaxZ.Size = new System.Drawing.Size(100, 20);
            this.AABBMaxZ.TabIndex = 13;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(106, 160);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(68, 13);
            this.label15.TabIndex = 12;
            this.label15.Text = "AABB Max Y";
            // 
            // AABBMaxY
            // 
            this.AABBMaxY.Location = new System.Drawing.Point(180, 159);
            this.AABBMaxY.Name = "AABBMaxY";
            this.AABBMaxY.Size = new System.Drawing.Size(100, 20);
            this.AABBMaxY.TabIndex = 11;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(106, 134);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(68, 13);
            this.label16.TabIndex = 10;
            this.label16.Text = "AABB Max X";
            // 
            // AABBMaxX
            // 
            this.AABBMaxX.Location = new System.Drawing.Point(180, 133);
            this.AABBMaxX.Name = "AABBMaxX";
            this.AABBMaxX.Size = new System.Drawing.Size(100, 20);
            this.AABBMaxX.TabIndex = 9;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(109, 88);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(65, 13);
            this.label17.TabIndex = 8;
            this.label17.Text = "AABB Min Z";
            // 
            // AABBMinZ
            // 
            this.AABBMinZ.Location = new System.Drawing.Point(180, 85);
            this.AABBMinZ.Name = "AABBMinZ";
            this.AABBMinZ.Size = new System.Drawing.Size(100, 20);
            this.AABBMinZ.TabIndex = 7;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(109, 62);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(65, 13);
            this.label18.TabIndex = 6;
            this.label18.Text = "AABB Min Y";
            // 
            // AABBMinY
            // 
            this.AABBMinY.Location = new System.Drawing.Point(180, 59);
            this.AABBMinY.Name = "AABBMinY";
            this.AABBMinY.Size = new System.Drawing.Size(100, 20);
            this.AABBMinY.TabIndex = 5;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(109, 36);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(65, 13);
            this.label19.TabIndex = 3;
            this.label19.Text = "AABB Min X";
            // 
            // AABBMinX
            // 
            this.AABBMinX.Location = new System.Drawing.Point(180, 33);
            this.AABBMinX.Name = "AABBMinX";
            this.AABBMinX.Size = new System.Drawing.Size(100, 20);
            this.AABBMinX.TabIndex = 2;
            // 
            // CapsulePanel
            // 
            this.CapsulePanel.Controls.Add(this.label9);
            this.CapsulePanel.Controls.Add(this.CapsuleTopZ);
            this.CapsulePanel.Controls.Add(this.label12);
            this.CapsulePanel.Controls.Add(this.CapsuleTopY);
            this.CapsulePanel.Controls.Add(this.label13);
            this.CapsulePanel.Controls.Add(this.CapsuleTopX);
            this.CapsulePanel.Controls.Add(this.label7);
            this.CapsulePanel.Controls.Add(this.CapsuleBottomZ);
            this.CapsulePanel.Controls.Add(this.label8);
            this.CapsulePanel.Controls.Add(this.CapsuleBottomY);
            this.CapsulePanel.Controls.Add(this.label10);
            this.CapsulePanel.Controls.Add(this.CapsuleBottomX);
            this.CapsulePanel.Controls.Add(this.label11);
            this.CapsulePanel.Controls.Add(this.textBox5);
            this.CapsulePanel.Controls.Add(this.label21);
            this.CapsulePanel.Location = new System.Drawing.Point(12, 68);
            this.CapsulePanel.Name = "CapsulePanel";
            this.CapsulePanel.Size = new System.Drawing.Size(374, 246);
            this.CapsulePanel.TabIndex = 11;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(97, 211);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(77, 13);
            this.label9.TabIndex = 14;
            this.label9.Text = "Capsule Top Z";
            // 
            // CapsuleTopZ
            // 
            this.CapsuleTopZ.Location = new System.Drawing.Point(180, 211);
            this.CapsuleTopZ.Name = "CapsuleTopZ";
            this.CapsuleTopZ.Size = new System.Drawing.Size(100, 20);
            this.CapsuleTopZ.TabIndex = 13;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(97, 185);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(77, 13);
            this.label12.TabIndex = 12;
            this.label12.Text = "Capsule Top Y";
            // 
            // CapsuleTopY
            // 
            this.CapsuleTopY.Location = new System.Drawing.Point(180, 185);
            this.CapsuleTopY.Name = "CapsuleTopY";
            this.CapsuleTopY.Size = new System.Drawing.Size(100, 20);
            this.CapsuleTopY.TabIndex = 11;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(97, 159);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(77, 13);
            this.label13.TabIndex = 10;
            this.label13.Text = "Capsule Top X";
            // 
            // CapsuleTopX
            // 
            this.CapsuleTopX.Location = new System.Drawing.Point(180, 159);
            this.CapsuleTopX.Name = "CapsuleTopX";
            this.CapsuleTopX.Size = new System.Drawing.Size(100, 20);
            this.CapsuleTopX.TabIndex = 9;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(83, 123);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(91, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Capsule Bottom Z";
            // 
            // CapsuleBottomZ
            // 
            this.CapsuleBottomZ.Location = new System.Drawing.Point(180, 120);
            this.CapsuleBottomZ.Name = "CapsuleBottomZ";
            this.CapsuleBottomZ.Size = new System.Drawing.Size(100, 20);
            this.CapsuleBottomZ.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(83, 97);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(91, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Capsule Bottom Y";
            // 
            // CapsuleBottomY
            // 
            this.CapsuleBottomY.Location = new System.Drawing.Point(180, 94);
            this.CapsuleBottomY.Name = "CapsuleBottomY";
            this.CapsuleBottomY.Size = new System.Drawing.Size(100, 20);
            this.CapsuleBottomY.TabIndex = 5;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(83, 71);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(91, 13);
            this.label10.TabIndex = 3;
            this.label10.Text = "Capsule Bottom X";
            // 
            // CapsuleBottomX
            // 
            this.CapsuleBottomX.Location = new System.Drawing.Point(180, 68);
            this.CapsuleBottomX.Name = "CapsuleBottomX";
            this.CapsuleBottomX.Size = new System.Drawing.Size(100, 20);
            this.CapsuleBottomX.TabIndex = 2;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(93, 29);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(81, 13);
            this.label11.TabIndex = 1;
            this.label11.Text = "Capsule Radius";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(180, 29);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(100, 20);
            this.textBox5.TabIndex = 0;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(5, 5);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(138, 20);
            this.label21.TabIndex = 16;
            this.label21.Text = "Specify Capsule";
            // 
            // OBBPanel
            // 
            this.OBBPanel.Controls.Add(this.label32);
            this.OBBPanel.Controls.Add(this.OBBAngleY);
            this.OBBPanel.Controls.Add(this.label24);
            this.OBBPanel.Controls.Add(this.label25);
            this.OBBPanel.Controls.Add(this.OBBExtent2);
            this.OBBPanel.Controls.Add(this.label26);
            this.OBBPanel.Controls.Add(this.OBBExtent1);
            this.OBBPanel.Controls.Add(this.label27);
            this.OBBPanel.Controls.Add(this.OBBExtent0);
            this.OBBPanel.Controls.Add(this.label28);
            this.OBBPanel.Controls.Add(this.OBBCenterZ);
            this.OBBPanel.Controls.Add(this.label29);
            this.OBBPanel.Controls.Add(this.OBBCenterY);
            this.OBBPanel.Controls.Add(this.label30);
            this.OBBPanel.Controls.Add(this.OBBCenterX);
            this.OBBPanel.Controls.Add(this.label31);
            this.OBBPanel.Controls.Add(this.OBBAngleX);
            this.OBBPanel.Location = new System.Drawing.Point(12, 68);
            this.OBBPanel.Name = "OBBPanel";
            this.OBBPanel.Size = new System.Drawing.Size(374, 246);
            this.OBBPanel.TabIndex = 12;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(130, 51);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(44, 13);
            this.label32.TabIndex = 18;
            this.label32.Text = "Y Angle";
            // 
            // OBBAngleY
            // 
            this.OBBAngleY.Location = new System.Drawing.Point(180, 45);
            this.OBBAngleY.Name = "OBBAngleY";
            this.OBBAngleY.Size = new System.Drawing.Size(100, 20);
            this.OBBAngleY.TabIndex = 17;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.Location = new System.Drawing.Point(5, 5);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(110, 20);
            this.label24.TabIndex = 16;
            this.label24.Text = "Specify OBB";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(128, 214);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(46, 13);
            this.label25.TabIndex = 14;
            this.label25.Text = "Extent 2";
            // 
            // OBBExtent2
            // 
            this.OBBExtent2.Location = new System.Drawing.Point(180, 214);
            this.OBBExtent2.Name = "OBBExtent2";
            this.OBBExtent2.Size = new System.Drawing.Size(100, 20);
            this.OBBExtent2.TabIndex = 13;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(128, 188);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(46, 13);
            this.label26.TabIndex = 12;
            this.label26.Text = "Extent 1";
            // 
            // OBBExtent1
            // 
            this.OBBExtent1.Location = new System.Drawing.Point(180, 188);
            this.OBBExtent1.Name = "OBBExtent1";
            this.OBBExtent1.Size = new System.Drawing.Size(100, 20);
            this.OBBExtent1.TabIndex = 11;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(128, 162);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(46, 13);
            this.label27.TabIndex = 10;
            this.label27.Text = "Extent 0";
            // 
            // OBBExtent0
            // 
            this.OBBExtent0.Location = new System.Drawing.Point(180, 162);
            this.OBBExtent0.Name = "OBBExtent0";
            this.OBBExtent0.Size = new System.Drawing.Size(100, 20);
            this.OBBExtent0.TabIndex = 9;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(126, 133);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(48, 13);
            this.label28.TabIndex = 8;
            this.label28.Text = "Center Z";
            // 
            // OBBCenterZ
            // 
            this.OBBCenterZ.Location = new System.Drawing.Point(180, 129);
            this.OBBCenterZ.Name = "OBBCenterZ";
            this.OBBCenterZ.Size = new System.Drawing.Size(100, 20);
            this.OBBCenterZ.TabIndex = 7;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(126, 107);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(48, 13);
            this.label29.TabIndex = 6;
            this.label29.Text = "Center Y";
            // 
            // OBBCenterY
            // 
            this.OBBCenterY.Location = new System.Drawing.Point(180, 103);
            this.OBBCenterY.Name = "OBBCenterY";
            this.OBBCenterY.Size = new System.Drawing.Size(100, 20);
            this.OBBCenterY.TabIndex = 5;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(126, 81);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(48, 13);
            this.label30.TabIndex = 3;
            this.label30.Text = "Center X";
            // 
            // OBBCenterX
            // 
            this.OBBCenterX.Location = new System.Drawing.Point(180, 77);
            this.OBBCenterX.Name = "OBBCenterX";
            this.OBBCenterX.Size = new System.Drawing.Size(100, 20);
            this.OBBCenterX.TabIndex = 2;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(130, 22);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(44, 13);
            this.label31.TabIndex = 1;
            this.label31.Text = "X Angle";
            // 
            // OBBAngleX
            // 
            this.OBBAngleX.Location = new System.Drawing.Point(180, 19);
            this.OBBAngleX.Name = "OBBAngleX";
            this.OBBAngleX.Size = new System.Drawing.Size(100, 20);
            this.OBBAngleX.TabIndex = 0;
            // 
            // SpherePanel
            // 
            this.SpherePanel.Controls.Add(this.label33);
            this.SpherePanel.Controls.Add(this.label6);
            this.SpherePanel.Controls.Add(this.SphereCenterZ);
            this.SpherePanel.Controls.Add(this.label5);
            this.SpherePanel.Controls.Add(this.SphereCenterY);
            this.SpherePanel.Controls.Add(this.label3);
            this.SpherePanel.Controls.Add(this.SphereCenterX);
            this.SpherePanel.Controls.Add(this.label2);
            this.SpherePanel.Controls.Add(this.textBox1);
            this.SpherePanel.Location = new System.Drawing.Point(11, 68);
            this.SpherePanel.Name = "SpherePanel";
            this.SpherePanel.Size = new System.Drawing.Size(372, 248);
            this.SpherePanel.TabIndex = 13;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label33.Location = new System.Drawing.Point(5, 5);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(131, 20);
            this.label33.TabIndex = 17;
            this.label33.Text = "Specify Sphere";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(119, 174);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Center Z";
            // 
            // SphereCenterZ
            // 
            this.SphereCenterZ.Location = new System.Drawing.Point(177, 171);
            this.SphereCenterZ.Name = "SphereCenterZ";
            this.SphereCenterZ.Size = new System.Drawing.Size(100, 20);
            this.SphereCenterZ.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(119, 148);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Center Y";
            // 
            // SphereCenterY
            // 
            this.SphereCenterY.Location = new System.Drawing.Point(177, 145);
            this.SphereCenterY.Name = "SphereCenterY";
            this.SphereCenterY.Size = new System.Drawing.Size(100, 20);
            this.SphereCenterY.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(119, 122);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Center X";
            // 
            // SphereCenterX
            // 
            this.SphereCenterX.Location = new System.Drawing.Point(177, 119);
            this.SphereCenterX.Name = "SphereCenterX";
            this.SphereCenterX.Size = new System.Drawing.Size(100, 20);
            this.SphereCenterX.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(127, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Radius";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(177, 58);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 9;
            // 
            // AddObjectButton
            // 
            this.AddObjectButton.Location = new System.Drawing.Point(276, 20);
            this.AddObjectButton.Name = "AddObjectButton";
            this.AddObjectButton.Size = new System.Drawing.Size(75, 23);
            this.AddObjectButton.TabIndex = 14;
            this.AddObjectButton.Text = "Add Object";
            this.AddObjectButton.UseVisualStyleBackColor = true;
            this.AddObjectButton.Click += new System.EventHandler(this.AddObjectButton_Click);
            // 
            // MakeObjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 329);
            this.Controls.Add(this.AddObjectButton);
            this.Controls.Add(this.ShapeTypeComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SpherePanel);
            this.Controls.Add(this.OBBPanel);
            this.Controls.Add(this.CapsulePanel);
            this.Controls.Add(this.AABBPanel);
            this.Name = "MakeObjectForm";
            this.Text = "Make Object";
            this.Load += new System.EventHandler(this.MakeObjectForm_Load);
            this.AABBPanel.ResumeLayout(false);
            this.AABBPanel.PerformLayout();
            this.CapsulePanel.ResumeLayout(false);
            this.CapsulePanel.PerformLayout();
            this.OBBPanel.ResumeLayout(false);
            this.OBBPanel.PerformLayout();
            this.SpherePanel.ResumeLayout(false);
            this.SpherePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox ShapeTypeComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel AABBPanel;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox AABBMaxZ;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox AABBMaxY;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox AABBMaxX;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox AABBMinZ;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox AABBMinY;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox AABBMinX;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Panel CapsulePanel;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox CapsuleTopZ;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox CapsuleTopY;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox CapsuleTopX;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox CapsuleBottomZ;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox CapsuleBottomY;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox CapsuleBottomX;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Panel OBBPanel;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.TextBox OBBAngleY;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox OBBExtent2;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox OBBExtent1;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox OBBExtent0;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.TextBox OBBCenterZ;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.TextBox OBBCenterY;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.TextBox OBBCenterX;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.TextBox OBBAngleX;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Panel SpherePanel;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox SphereCenterZ;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox SphereCenterY;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox SphereCenterX;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button AddObjectButton;
    }
}
