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
	public partial class NameValueDialog : Form
	{
		NameValueObject nameValueCollecton = new NameValueObject();
		NameValueTemplateCollection temColl;
		public NameValueDialog(NameValueObject obj, string type, NameValueTemplateCollection temCol)
		{
			InitializeComponent();
			temColl = temCol;
			NameValueCollection.SetNameValueObject(obj.nameValuePairs);
			loadTemplateListBox(temCol.List(type));
			loadNameListBox();
		}

		public NameValueObject NameValueCollection
		{
			get
			{
				return nameValueCollecton;
			}
		}

		private void loadNameListBox()
		{
            valueItem item;
			nameListBox.Items.Clear();
			foreach(string Name in NameValueCollection.NameValuePairKeyList())
			{
                string value;
                valueItem valItem = NameValueCollection.LookUp(Name);
				nameListBox.Items.Add(String.Format("{0}={1}", Name, (NameValueCollection.LookUp(Name) as valueItem).value));
			}
		}

		private void loadTemplateListBox(List<string> name)
		{
			foreach(string template in name)
			{
				templatesListBox.Items.Add(template);
			}
		}

		public void addNVPButton_click(object obj, EventArgs e)
		{
			bool showAgain = false;
			using (NameValueAddEdit_Dialog dlg = new NameValueAddEdit_Dialog("", "", "", null))
			{
				DialogResult dlgResult = dlg.ShowDialog();
				do{
					if (dlgResult == DialogResult.OK)
					{
						showAgain = false;
						if (dlgResult == DialogResult.OK)
						{
							// do validation here
							// if validation fails, set showAgain to true
							showAgain = ((dlgResult == DialogResult.OK) && (!dlg.okButton_validating()));
						}
					}
				}while (showAgain);
					
				if (dlgResult == DialogResult.OK)
				{
					NameValueCollection.AddNameValuePair(dlg.NameTextBoxText, dlg.ValueTextBoxText);
					nameListBox.Items.Add(String.Format("{0}={1}", dlg.NameTextBoxText, dlg.ValueTextBoxText));
					return;
				}
				else if (dlgResult == DialogResult.Cancel)
				{
					return;
				}
			}
		}

        public void editNVPButton_click(object obj, EventArgs e)
        {
            valueItem value;
            if (nameListBox.Items.Count > 0 && nameListBox.SelectedItem != null)
            {
                string name = nameListBox.SelectedItem.ToString();
                value = NameValueCollection.LookUp(name);

                NameValueAddEdit_Dialog dlg;
                if (value == null)
                {
                    return;
                }
                if (String.Equals(value.type, "enum"))
                {
                    dlg = new NameValueAddEdit_Dialog(name, value.value, value.type, value.enumList);
                }
                else
                {
                    dlg = new NameValueAddEdit_Dialog(name, value.value, value.type, null);
                }
                DialogResult dlgResult = dlg.ShowDialog();
                if (dlgResult == DialogResult.OK)
                {
                    NameValueCollection.EditNameValuePair(nameListBox.SelectedItem.ToString(), dlg.NameTextBoxText, dlg.ValueTextBoxText, value.type);
                }
                dlg.ValueTextbox.Visible = true;
                dlg.ValueTextbox.Visible = false;
            }

        }
		public void deleteNVPButton_click(object obj, EventArgs e)
		{
			if (nameListBox.SelectedItem != null)
			{
				NameValueCollection.RemoveNameValuePair(nameListBox.SelectedItem.ToString());
				nameListBox.Items.Remove(nameListBox.SelectedItem);
			}
		}

		public void templatesListBox_selectedIndexChanged(object obj, EventArgs e)
		{
			if (templatesListBox.SelectedIndex != -1)
			{
				string templateName = templatesListBox.SelectedItem.ToString();
				nameListBox.Items.Clear();
				List<string> list = temColl.NameValuePropertiesList(templateName);
				foreach (string name in list)
				{
					nameListBox.Items.Add(name);
				}
			}
			else
			{
				nameListBox.Items.Clear();
				foreach (string name in NameValueCollection.NameValuePairKeyList())
				{
					nameListBox.Items.Add(name);
				}
			}
		}

		private void addTemplateButton_clicked(object sender, EventArgs e)
		{
			if (templatesListBox.SelectedItem != null)
			{
				string tempName = templatesListBox.SelectedItem.ToString();
				List<string> propList = temColl.NameValuePropertiesList(tempName);
				foreach (string prop in propList)
				{
					string type = temColl.PropertyType(tempName, prop);
					string def = temColl.DefaultValue(tempName, prop);
					if (def == "")
					{
						switch (type)
						{
							case "float":
							case "int":
							case "uint":
								def = "0";
								break;
							case "string":
							case "enum":
								def = "";
								break;
						}
					}
					temColl.Enum(tempName, prop);
					if (String.Equals(type, "Enum"))
					{
						NameValueCollection.AddNameValuePair(prop, def, temColl.Enum(tempName, prop));
						nameListBox.Items.Add(prop);
					}
					else
					{
						nameListBox.Items.Add(prop);
						NameValueCollection.AddNameValuePair(prop, def);
					}
				}
			}
		}

		private void addNVPButton_clicked(object sender, EventArgs e)
		{
			using (NameValueAddEdit_Dialog dlg = new NameValueAddEdit_Dialog("", "", "", null))
			{
				dlg.Text = "Name Value Pair Add Dialog";
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					nameListBox.Items.Add(dlg.NameTextBoxText);
					NameValueCollection.AddNameValuePair(dlg.NameTextBoxText, dlg.ValueTextBoxText);
				}
			}
		}

		private void editNVPButton_clicked(object sender, EventArgs e)
		{
			valueItem val;
			string oldName = nameListBox.SelectedItem.ToString();
			val = NameValueCollection.LookUp(oldName);
			using (NameValueAddEdit_Dialog dlg = new NameValueAddEdit_Dialog(nameListBox.SelectedItem.ToString(), val.value, val.type,null))
			{
				dlg.Text =  "Name Value Pair Edit Dialog";
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					NameValueCollection.EditNameValuePair(oldName, dlg.NameTextBoxText, dlg.NameTextBoxText, val.type);
				}
			}
		}

		private void deleteNVButton_clicked(object sender, EventArgs e)
		{
			string name = nameListBox.SelectedItem.ToString();
			nameListBox.Items.Remove(nameListBox.SelectedItem);
			NameValueCollection.RemoveNameValuePair(name);
		}

		private void helpButton_clicked(object sender, EventArgs e)
		{
			Button but = sender as Button;
			WorldEditor.LaunchDoc(but.Tag as string);
		}

	}
}
