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

namespace Multiverse.Tools.WorldEditor
{
	public partial class NameValueAddEdit_Dialog : Form
	{
		public TextBox ValueTextbox;
		private Label nameLabel;
		private Label valueLabel;
		private Button NameValueAddEditDialogOkButton;
		public TextBox NameTextbox;
		public ComboBox valueComboBox;
		private Button NameValueAddEditDialogCancelButton;
		private string valueType;
	
		public NameValueAddEdit_Dialog(string Name, string value, string type, List<string> enumList)
		{

			InitializeComponent();
			if (!String.Equals(type,""))
			{
				if (String.Equals(type, "enum") && enumList != null)
				{
					ValueTextbox.Visible = false;
					valueComboBox.Visible = true;
					foreach (string enumEntry in enumList)
					{
						valueComboBox.Items.Add(enumEntry);
					}
					if (!String.Equals(value,""))
					{
						for (int i = 0; i < valueComboBox.Items.Count; i++)
						{
							if (String.Equals(valueComboBox.Items[i].ToString(),value))
							{
								valueComboBox.SelectedIndex = i;
							}
						}
					}
				}
				else
				{
					switch (type)
					{
						case "Float":
							valueType = "Float";
							this.ValueTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
						break;
						case "uInt":
							valueType = "uInt";
							this.ValueTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.uintVerifyevent);
						break;
						case "Int":
							valueType = "int";
							this.ValueTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.intVerifyevent);

						break;
					}

					if (!String.Equals(type, ""))
					{
						valueLabel.Text = String.Format("Value({0}):", type);
						if (!String.Equals("", value))
						{
							ValueTextbox.Text = value;
						}
					}
				}
			}
			this.NameTextbox.Text = Name;
			this.ValueTextbox.Text = value;
		}

		private void InitializeComponent()
		{
            this.ValueTextbox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.valueLabel = new System.Windows.Forms.Label();
            this.NameValueAddEditDialogOkButton = new System.Windows.Forms.Button();
            this.NameValueAddEditDialogCancelButton = new System.Windows.Forms.Button();
            this.NameTextbox = new System.Windows.Forms.TextBox();
            this.valueComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // ValueTextbox
            // 
            this.ValueTextbox.Location = new System.Drawing.Point(118, 70);
            this.ValueTextbox.Name = "ValueTextbox";
            this.ValueTextbox.Size = new System.Drawing.Size(188, 20);
            this.ValueTextbox.TabIndex = 1;
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(21, 32);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(38, 13);
            this.nameLabel.TabIndex = 2;
            this.nameLabel.Text = "Name:";
            // 
            // valueLabel
            // 
            this.valueLabel.AutoSize = true;
            this.valueLabel.Location = new System.Drawing.Point(21, 73);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new System.Drawing.Size(37, 13);
            this.valueLabel.TabIndex = 3;
            this.valueLabel.Text = "Value:";
            // 
            // NameValueAddEditDialogOkButton
            // 
            this.NameValueAddEditDialogOkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.NameValueAddEditDialogOkButton.Location = new System.Drawing.Point(24, 106);
            this.NameValueAddEditDialogOkButton.Name = "NameValueAddEditDialogOkButton";
            this.NameValueAddEditDialogOkButton.Size = new System.Drawing.Size(75, 23);
            this.NameValueAddEditDialogOkButton.TabIndex = 2;
            this.NameValueAddEditDialogOkButton.Text = "&OK";
            this.NameValueAddEditDialogOkButton.UseVisualStyleBackColor = true;
            // 
            // NameValueAddEditDialogCancelButton
            // 
            this.NameValueAddEditDialogCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.NameValueAddEditDialogCancelButton.Location = new System.Drawing.Point(231, 106);
            this.NameValueAddEditDialogCancelButton.Name = "NameValueAddEditDialogCancelButton";
            this.NameValueAddEditDialogCancelButton.Size = new System.Drawing.Size(75, 23);
            this.NameValueAddEditDialogCancelButton.TabIndex = 3;
            this.NameValueAddEditDialogCancelButton.Text = "&Cancel";
            this.NameValueAddEditDialogCancelButton.UseVisualStyleBackColor = true;
            // 
            // NameTextbox
            // 
            this.NameTextbox.AcceptsTab = true;
            this.NameTextbox.HideSelection = false;
            this.NameTextbox.Location = new System.Drawing.Point(118, 29);
            this.NameTextbox.Name = "NameTextbox";
            this.NameTextbox.Size = new System.Drawing.Size(188, 20);
            this.NameTextbox.TabIndex = 0;
            // 
            // valueComboBox
            // 
            this.valueComboBox.FormattingEnabled = true;
            this.valueComboBox.Location = new System.Drawing.Point(118, 70);
            this.valueComboBox.Name = "valueComboBox";
            this.valueComboBox.Size = new System.Drawing.Size(188, 21);
            this.valueComboBox.TabIndex = 4;
            this.valueComboBox.Visible = false;
            // 
            // NameValueAddEdit_Dialog
            // 
            this.AcceptButton = this.NameValueAddEditDialogOkButton;
            this.CancelButton = this.NameValueAddEditDialogCancelButton;
            this.ClientSize = new System.Drawing.Size(318, 141);
            this.Controls.Add(this.NameTextbox);
            this.Controls.Add(this.NameValueAddEditDialogCancelButton);
            this.Controls.Add(this.NameValueAddEditDialogOkButton);
            this.Controls.Add(this.valueLabel);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.ValueTextbox);
            this.Controls.Add(this.valueComboBox);
            this.Name = "NameValueAddEdit_Dialog";
            this.Text = "Name/Value Pair Add/Edit Dialog";
            this.ResumeLayout(false);
            this.PerformLayout();

		}


		public string NameTextBoxText
		{
			get
			{
				return NameTextbox.Text;
			}
			set
			{
				NameTextbox.Text = value;
			}
		}

		public string ValueTextBoxText
		{
			get
			{
				return ValueTextbox.Text;
			}
			set
			{
				ValueTextbox.Text = value;
			}
		}

		public float ValueTextBoxASFloat
		{
			get
			{
				return float.Parse(ValueTextbox.Text);
			}
			set
			{
				ValueTextBoxText = value.ToString();
			}
		}

		public int ValueTextBoxAsInt
		{
			get
			{
				return int.Parse(ValueTextbox.Text);
			}
			set
			{
				ValueTextBoxText = value.ToString();
			}
		}

		public uint ValueTextBoxAsUint
		{
			get
			{
				return uint.Parse(ValueTextbox.Text);
			}
			set
			{
				ValueTextBoxText = value.ToString();
			}
		}


		private void floatVerifyevent(object sender, CancelEventArgs e)
		{
			TextBox textbox = (TextBox)sender;

			if (!ValidityHelperClass.isFloat(textbox.Text))
			{
				Color textColor = Color.Red;
				textbox.ForeColor = textColor;
			}
			else
			{
				Color textColor = Color.Black;
				textbox.ForeColor = textColor;
			}
		}

		private void uintVerifyevent(object sender, CancelEventArgs e)
		{
			TextBox textbox = (TextBox) sender;
			if (!ValidityHelperClass.isUint(textbox.Text))
			{
				Color textColor = Color.Red;
				textbox.ForeColor = textColor;
			}
			else
			{
				Color textColor = Color.Black;
			}

		}

		private void intVerifyevent(object sender, CancelEventArgs e)
		{
			TextBox textbox = (TextBox)sender;
			if (!ValidityHelperClass.isUint(textbox.Text))
			{
				Color textColor = Color.Red;
				textbox.ForeColor = textColor;
			}
			else
			{
				Color textColor = Color.Black;
			}
		}



		public bool okButton_validating()
		{

			string text = ValueTextbox.Text;
			switch (valueType)
			{
				case "Float":
					return ValidityHelperClass.isFloat(text);
					break;
				case "uInt":
					return ValidityHelperClass.isUint(text);
					break;
				case "int":
					return ValidityHelperClass.isInt(text);
					break;
			}
			return true;
		}

	}
}
