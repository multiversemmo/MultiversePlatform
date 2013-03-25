using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Multiverse.ToolBox
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
        private Label typeLabel;
        private ComboBox typeComboBox;
        private List<string> enumList = null;

        public NameValueAddEdit_Dialog(string Name, string value, List<string> enumList)
        {
            this.enumList = enumList;
            InitializeComponent();
            ValueTextbox.Visible = false;
            valueComboBox.Visible = true;
            typeComboBox.Visible = false;
            typeLabel.Visible = false;
            if (enumList == null)
            {
                return;
            }
            foreach (string enumEntry in enumList)
            {
                valueComboBox.Items.Add(enumEntry);
            }
            valueComboBox.SelectedIndex = 0;
            NameTextBoxText = Name;
            for (int i = 0; i < valueComboBox.Items.Count; i++)
            {
                if (String.Equals(valueComboBox.Items[i].ToString(), value))
                {
                    valueComboBox.SelectedIndex = i;
                }
            }
        }

        public NameValueAddEdit_Dialog(string Name, string value, string type, string[] validTypes)
        {
            InitializeComponent();
            valueComboBox.Visible = false;
            ValueTextbox.Visible = true;
            typeComboBox.Visible = true;
            typeLabel.Visible = true;

            foreach (string vtype in validTypes)
            {
                typeComboBox.Items.Add(vtype);
                if (String.Equals(type, vtype))
                {
                    typeComboBox.SelectedItem = vtype;
                }
            }

            this.NameTextbox.Text = Name;
            this.ValueTextbox.Text = value;
        }

        public NameValueAddEdit_Dialog(string Name, string value, string[] validTypes)
        {
            InitializeComponent();
            valueComboBox.Visible = false;
            ValueTextbox.Visible = true;
            typeComboBox.Visible = true;
            typeLabel.Visible = true;

            foreach (string vtype in validTypes)
            {
                typeComboBox.Items.Add(vtype);
            }
            typeComboBox.SelectedIndex = 0;
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
            this.typeLabel = new System.Windows.Forms.Label();
            this.typeComboBox = new System.Windows.Forms.ComboBox();
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
            this.NameValueAddEditDialogOkButton.Location = new System.Drawing.Point(24, 154);
            this.NameValueAddEditDialogOkButton.Name = "NameValueAddEditDialogOkButton";
            this.NameValueAddEditDialogOkButton.Size = new System.Drawing.Size(75, 23);
            this.NameValueAddEditDialogOkButton.TabIndex = 3;
            this.NameValueAddEditDialogOkButton.Text = "&OK";
            this.NameValueAddEditDialogOkButton.UseVisualStyleBackColor = true;
            // 
            // NameValueAddEditDialogCancelButton
            // 
            this.NameValueAddEditDialogCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.NameValueAddEditDialogCancelButton.Location = new System.Drawing.Point(231, 154);
            this.NameValueAddEditDialogCancelButton.Name = "NameValueAddEditDialogCancelButton";
            this.NameValueAddEditDialogCancelButton.Size = new System.Drawing.Size(75, 23);
            this.NameValueAddEditDialogCancelButton.TabIndex = 4;
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
            this.valueComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.valueComboBox.FormattingEnabled = true;
            this.valueComboBox.Location = new System.Drawing.Point(118, 70);
            this.valueComboBox.Name = "valueComboBox";
            this.valueComboBox.Size = new System.Drawing.Size(188, 21);
            this.valueComboBox.TabIndex = 4;
            this.valueComboBox.Visible = false;
            // 
            // typeLabel
            // 
            this.typeLabel.AutoSize = true;
            this.typeLabel.Location = new System.Drawing.Point(21, 114);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Size = new System.Drawing.Size(34, 13);
            this.typeLabel.TabIndex = 5;
            this.typeLabel.Text = "Type:";
            // 
            // typeComboBox
            // 
            this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeComboBox.FormattingEnabled = true;
            this.typeComboBox.Location = new System.Drawing.Point(118, 111);
            this.typeComboBox.Name = "typeComboBox";
            this.typeComboBox.Size = new System.Drawing.Size(188, 21);
            this.typeComboBox.TabIndex = 2;
            this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.typeComboBox_SelectedIndexChanged);
            // 
            // NameValueAddEdit_Dialog
            // 
            this.AcceptButton = this.NameValueAddEditDialogOkButton;
            this.CancelButton = this.NameValueAddEditDialogCancelButton;
            this.ClientSize = new System.Drawing.Size(318, 189);
            this.Controls.Add(this.typeComboBox);
            this.Controls.Add(this.typeLabel);
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

        public string TypeComboBoxSelectedItemAsString
        {
            get
            {
                return typeComboBox.SelectedItem as string;
            }
            set
            {
                foreach (string item in typeComboBox.Items)
                {
                    if (String.Equals(item, value))
                    {
                        typeComboBox.SelectedItem = item;
                    }
                }
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

        private void booleanVerifyevent(object sender, CancelEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (!ValidityHelperClass.isBoolean(textbox.Text))
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
            switch (typeComboBox.SelectedItem as string)
            {
                case "Float":
                    return ValidityHelperClass.isFloat(text);
                case "Uint":
                    return ValidityHelperClass.isUint(text);
                case "Int":
                    return ValidityHelperClass.isInt(text);
                case "Boolean":
                    return ValidityHelperClass.isBoolean(text);
            }
			return true;
		}

        public List<string> EnumList
        {
            get
            {
                return enumList;
            }
        }

        private void typeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (typeComboBox.SelectedItem as string)
            {
                case "Float":
                    valueType = "Float";
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.uintVerifyevent);
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.intVerifyevent);
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.booleanVerifyevent);
                    this.ValueTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
                    break;
                case "Uint":
                    valueType = "Uint";
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.intVerifyevent);
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.booleanVerifyevent);
                    this.ValueTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.uintVerifyevent);
                    break;
                case "Int":
                    valueType = "Int";
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.uintVerifyevent);
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.booleanVerifyevent);
                    this.ValueTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.intVerifyevent);
                    break;
                case "Boolean":
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.uintVerifyevent);
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.intVerifyevent);
                    this.ValueTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.booleanVerifyevent);
                    valueType = "Boolean";
                    break;
                case "String":
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.uintVerifyevent);
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.intVerifyevent);
                    this.ValueTextbox.Validating -= new System.ComponentModel.CancelEventHandler(this.booleanVerifyevent);
                    valueType = "String";
                    break;
            }
        }
	}
}