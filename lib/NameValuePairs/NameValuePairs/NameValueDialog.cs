using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Multiverse.ToolBox
{
	public partial class NameValueDialog : Form
	{
        protected static string helpURL = "http://update.multiverse.net/wiki/index.php/Using_World_Editor_Version_1.0";
		NameValueObject nameValueCollection = new NameValueObject();
		NameValueTemplateCollection temColl;
        string[] validAddTypes = { "String", "Int", "Uint", "Float", "Boolean" };
		public NameValueDialog(NameValueObject obj, string type, NameValueTemplateCollection temCol)
		{
			InitializeComponent();
			temColl = temCol;
			NameValueCollection.SetNameValueObject(obj.nameValuePairs);
			loadTemplateListBox(temCol.List(type));
            if (templatesListBox.Items.Count > 0)
            {
                templatesListBox.SelectedIndex = 0;
                addTemplateButton.Enabled = true;
            }
            else
            {
                addTemplateButton.Enabled = false;
            }
			loadNameListBox();
            if (nameListBox.Items.Count == 0)
            {
                editNVPButton.Enabled = false;
                deleteNVPButton.Enabled = false;
            }
            else
            {
                nameListBox.SelectedIndex = 0;
            }
		}

		public NameValueObject NameValueCollection
		{
			get
			{
				return nameValueCollection;
			}
		}

		private void loadNameListBox()
		{
            // valueItem item;
			nameListBox.Items.Clear();
			foreach(string Name in NameValueCollection.NameValuePairKeyList())
			{
                // string value;
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
            DialogResult dlgResult = DialogResult.Cancel;
			using (NameValueAddEdit_Dialog dlg = new NameValueAddEdit_Dialog("", "", validAddTypes))
			{
				do{
                    showAgain = false;
                    dlgResult = dlg.ShowDialog();
					if (dlgResult == DialogResult.OK)
					{
                        // do validation here
                        // if validation fails, set showAgain to true
                        showAgain = ((dlgResult == DialogResult.OK) && (!dlg.okButton_validating()));
                        if (nameValueCollection.NameValuePairKeyList().Contains(dlg.NameTextBoxText))
                        {
                            MessageBox.Show("You have entered a name that is already in the Name Value Object. You should edit it if you want to change its value. Two pairs may not have the same name.", "Error adding name value pair", MessageBoxButtons.OK);
                            showAgain = true;
                        }
					}

				}while (showAgain);
					
				if (dlgResult == DialogResult.OK)
				{
					NameValueCollection.AddNameValuePair(dlg.NameTextBoxText, dlg.ValueTextBoxText, dlg.TypeComboBoxSelectedItemAsString);
				}
				else if (dlgResult == DialogResult.Cancel)
				{
					return;
				}
			}
            loadNameListBox();
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
                if (String.Equals(value.type, "Enum"))
                {
                    dlg = new NameValueAddEdit_Dialog(name, value.value, value.enumList);
                }
                else
                {
                    dlg = new NameValueAddEdit_Dialog(name, value.value, value.type, validAddTypes);
                }
                DialogResult dlgResult = dlg.ShowDialog();
                if (dlgResult == DialogResult.OK)
                {
                    if (String.Equals(value.type, "Enum"))
                    {
                        NameValueCollection.EditNameValuePair(nameListBox.SelectedItem.ToString(), dlg.NameTextBoxText, dlg.ValueTextBoxText, dlg.valueComboBox.SelectedItem.ToString(), value.enumList);
                    }
                    else
                    {
                        NameValueCollection.EditNameValuePair(nameListBox.SelectedItem.ToString(), dlg.NameTextBoxText, dlg.ValueTextBoxText, value.type, null);
                    }
                }
                dlg.ValueTextbox.Visible = true;
                dlg.ValueTextbox.Visible = false;
            }
            loadNameListBox();
        }
		public void deleteNVPButton_click(object obj, EventArgs e)
		{
			if (nameListBox.SelectedItem != null)
			{
				NameValueCollection.RemoveNameValuePair(nameListBox.SelectedItem.ToString().Substring(0, nameListBox.SelectedItem.ToString().IndexOf('=')));
			}
            loadNameListBox();
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
                    if (nameValueCollection.NameValuePairKeyList().Contains(prop))
                    {
                        MessageBox.Show("A property from the template you tried to add has the same name as a property already in the Name Value object. This is not allowed.", "Error Adding Template", MessageBoxButtons.OK);
                        return;
                    }
                }
				foreach (string prop in propList)
				{
					string type = temColl.PropertyType(tempName, prop);
					string def = temColl.DefaultValue(tempName, prop);
					if (String.Equals(def,""))
					{
						switch (type)
						{
							case "Float":
							case "Int":
							case "Uint":
								def = "0";
								break;
							case "String":
                                def = "";
                                break;
                            case "Boolean":
                                def = "true";
                                break;
							case "Enum":
                                def = temColl.Enum(tempName, prop)[0];
								break;
						}
					}
					temColl.Enum(tempName, prop);
					if (String.Equals(type, "Enum"))
					{
                        List<string> en = temColl.Enum(tempName, prop);
						NameValueCollection.AddNameValuePair(prop, def, en);
						nameListBox.Items.Add(String.Format("{0}={1}", prop, def));
					}
					else
					{
						NameValueCollection.AddNameValuePair(prop, def, type);
                        string listBoxText = String.Format("{0}={1}", prop, def);
                        nameListBox.Items.Add(listBoxText);
					}
				}
			}
		}


		private void editNVPButton_clicked(object sender, EventArgs e)
		{
			valueItem val;
            List<string> enumList = null;
            if (nameListBox.SelectedIndex >= 0)
            {
                string oldName = nameListBox.SelectedItem.ToString().Substring(0, nameListBox.SelectedItem.ToString().IndexOf('='));
                val = NameValueCollection.LookUp(oldName);
                if (val.enumList.Count > 0)
                {
                    enumList = val.enumList;
                }
                if (String.Equals(val.type, "Enum"))
                {
                    using (NameValueAddEdit_Dialog dlg = new NameValueAddEdit_Dialog(oldName, val.value, enumList))
                    {
                        dlg.Text = "Name Value Pair Edit Dialog";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            NameValueCollection.EditNameValuePair(oldName, dlg.NameTextBoxText, dlg.valueComboBox.SelectedItem.ToString(), val.type, enumList);
                        }
                    }
                }
                else
                {
                    using(NameValueAddEdit_Dialog dlg = new NameValueAddEdit_Dialog(oldName, val.value, val.type, validAddTypes))
                    {
                        dlg.Text = "Name Value Pair Edit Dialog";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            NameValueCollection.EditNameValuePair(oldName, dlg.NameTextBoxText, dlg.ValueTextBoxText, dlg.TypeComboBoxSelectedItemAsString, null);
                        }
                    }
                }
                loadNameListBox();
            }
		}

		private void deleteNVButton_clicked(object sender, EventArgs e)
		{
			string name = nameListBox.SelectedItem.ToString();
			NameValueCollection.RemoveNameValuePair(name);
            loadNameListBox();
		}

		private void helpButton_clicked(object sender, EventArgs e)
		{
			Button but = sender as Button;
            string anchor = but.Tag as string;
            string target = String.Format("{0}#{1}", helpURL, anchor);
            System.Diagnostics.Process.Start(target);
		}

        private void nameListBox_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (nameListBox.SelectedIndex >= 0)
            {
                editNVPButton.Enabled = true;
                deleteNVPButton.Enabled = true;
            }
            else
            {
                editNVPButton.Enabled = false;
                deleteNVPButton.Enabled = false;
            }
        }
	}
}