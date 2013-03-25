using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Multiverse.ToolBox
{
    public partial class UserCommandEditor : Form
    {

        UserCommandMapping mapping;
        List<string> mouseCapable;
        

        public UserCommandEditor(UserCommandMapping commmandMapping, List<string> mouseCapableContexts)
        {
            InitializeComponent();
            this.mapping = commmandMapping;
            this.mouseCapable = mouseCapableContexts;
            editButton.Enabled = false;
            foreach (string cont in mapping.Context)
            {
                contextComboBox.Items.Add(cont);
            }

            contextComboBox.SelectedIndex = 0;
            foreach (string key in mapping.Key)
            {
                keyComboBox.Items.Add(key);
            }
            keyComboBox.SelectedIndex = 0;
            foreach (string modifier in mapping.Modifiers)
            {
                modifierComboBox.Items.Add(modifier);
            }
            modifierComboBox.SelectedIndex = 0;
            foreach (string action in mapping.Activities)
            {
                activityComboBox.Items.Add(action);
            }
            activityComboBox.SelectedIndex = 0;
        }

        public void contextComboBox_selectedIndexChanged(object sender, EventArgs ea)
        {
            eventsTreeView.Nodes.Clear();
            foreach (EventObject eo in mapping.GetEventsForContext(contextComboBox.SelectedItem as string))
            {
                TreeNode node = new TreeNode(eo.Text);
                node.Tag = eo;
                eventsTreeView.Nodes.Add(node);
                foreach (UserCommand com in mapping.GetCommandsForContext(contextComboBox.SelectedItem as string))
                {
                    if (ReferenceEquals(com.Event, eo))
                    {
                        TreeNode newNode = new TreeNode(parseCommandForString(com));
                        newNode.Tag = com;
                    }
                }
            }
            foreach (UserCommand command in mapping.GetCommandsForContext(contextComboBox.SelectedItem as string))
            {
                foreach (TreeNode evnode in eventsTreeView.Nodes)
                {
                    if (String.Equals((evnode.Tag as EventObject).EvString, command.EvString))
                    {
                        TreeNode newNode = new TreeNode(parseCommandForString(command));
                        newNode.Tag = command;
                        evnode.Nodes.Add(newNode);
                        break;
                    }
                }
            }
        }


        private void eventsTreeView_AfterSelect(object sender, EventArgs ea)
        {
            if(eventsTreeView.SelectedNode != null)
            {
                if (eventsTreeView.SelectedNode.Tag is EventObject)
                {
                    if (find_conflict(contextComboBox.SelectedItem as string, keyComboBox.SelectedItem as string, modifierComboBox.SelectedItem as string, activityComboBox.SelectedItem as string))
                    {
                        editButton.Enabled = false;
                        addButton.Enabled = false;
                        deleteCommandButton.Enabled = false;
                    }
                    else
                    {
                        editButton.Enabled = false;
                        addButton.Enabled = true;
                        deleteCommandButton.Enabled = false;
                    }
                }
                else
                {
                    editButton.Enabled = true;
                    addButton.Enabled = false;
                    deleteCommandButton.Enabled = true;
                    modifierComboBox.SelectedIndex = modifierComboBox.Items.IndexOf((eventsTreeView.SelectedNode.Tag as UserCommand).Modifier);
                    keyComboBox.SelectedIndex = keyComboBox.Items.IndexOf((eventsTreeView.SelectedNode.Tag as UserCommand).Key);
                    activityComboBox.SelectedIndex = activityComboBox.Items.IndexOf((eventsTreeView.SelectedNode.Tag as UserCommand).Activity);
                }
            }
        }


        public void comboBox_selectedIndexChanged(object sender, EventArgs ea)
        {
            if (find_conflict(contextComboBox.SelectedItem as string, keyComboBox.SelectedItem as string, modifierComboBox.SelectedItem as string, activityComboBox.SelectedItem as string))
            {
                editButton.Enabled = false;
                addButton.Enabled = false;
                if(eventsTreeView.SelectedNode.Tag is EventObject)
                {
                    deleteCommandButton.Enabled = false;
                }
                else
                {
                    deleteCommandButton.Enabled = true;
                }
                return;
            }
            if (eventsTreeView.SelectedNode != null && eventsTreeView.SelectedNode.Tag is EventObject)
            {
                addButton.Enabled = true;
                editButton.Enabled = false;
                deleteCommandButton.Enabled = false;
            }
            else
            {
                if (eventsTreeView.SelectedNode != null)
                {
                    addButton.Enabled = false;
                    editButton.Enabled = true;
                    deleteCommandButton.Enabled = true;
                }
                else
                {
                    addButton.Enabled = false;
                    editButton.Enabled = false;
                    deleteCommandButton.Enabled = false;
                }
            }
        }

        public void addButton_clicked(object sender, EventArgs ea)
        {
            if (eventsTreeView.SelectedNode.Tag is EventObject)
            {
                if ((eventsTreeView.SelectedNode.Tag as EventObject).MouseButtonEvent && !(Keys.LButton == mapping.ParseStringToKeyCode(keyComboBox.SelectedItem as string) || Keys.RButton == mapping.ParseStringToKeyCode(keyComboBox.SelectedItem as string) ||
                    Keys.MButton == mapping.ParseStringToKeyCode(keyComboBox.SelectedItem as string)))
                {
                    MessageBox.Show("Mouse related events must be mapped to mouse buttons", "Problem with mapping event", MessageBoxButtons.OK);
                    return;
                }

                UserCommand newCommand = new UserCommand(eventsTreeView.SelectedNode.Tag as EventObject, keyComboBox.SelectedItem as string, activityComboBox.SelectedItem as string, modifierComboBox.SelectedItem as string, (eventsTreeView.SelectedNode.Tag as EventObject).EvString, mapping);
                TreeNode newNode = new TreeNode(parseCommandForString(newCommand));
                mapping.Commands.Add(newCommand);
                newNode.Tag = newCommand;
                eventsTreeView.SelectedNode.Nodes.Add(newNode);
                eventsTreeView.ExpandAll();
            }
        }

        private string parseCommandForString(UserCommand command)
        {
            string text = "";
            if (!String.Equals(command.Modifier, "none"))
            {
                text = String.Format("{0}+{1}", command.Modifier, command.Key);
            }
            else
            {
                text = command.Key;
            }
            return text;
        }

        public void editButton_clicked(object sender, EventArgs ea)
        {
            if (eventsTreeView.SelectedNode.Tag is UserCommand)
            {
                if (!(eventsTreeView.SelectedNode.Tag as UserCommand).Event.MouseButtonEvent && (Keys.LButton == mapping.ParseStringToKeyCode(keyComboBox.SelectedItem as string) || Keys.RButton == mapping.ParseStringToKeyCode(keyComboBox.SelectedItem as string) ||
                    Keys.MButton == mapping.ParseStringToKeyCode(keyComboBox.SelectedItem as string)))
                {
                    MessageBox.Show("Mouse related events must be mapped to mouse buttons", "Problem with mapping event", MessageBoxButtons.OK);
                    return;
                }
                UserCommand com = (eventsTreeView.SelectedNode.Tag as UserCommand);
                com.Key = keyComboBox.SelectedItem as string;
                com.Modifier = modifierComboBox.SelectedItem as string;
                com.Activity = activityComboBox.SelectedItem as string;
                eventsTreeView.SelectedNode.Text = parseCommandForString(com);
            }
        }

        public void deleteButton_clicked(object sender, EventArgs ea)
        {
            if (eventsTreeView.SelectedNode.Tag is UserCommand)
            {
                UserCommand com = (eventsTreeView.SelectedNode.Tag as UserCommand);
                mapping.Commands.Remove(com);
                eventsTreeView.SelectedNode.Parent.Nodes.Remove(eventsTreeView.SelectedNode);
            }
        }

        public bool find_conflict(string context, string key, string modifier, string activity)
        {
            if (eventsTreeView.SelectedNode != null)
            {
                if (eventsTreeView.SelectedNode.Tag is EventObject && (eventsTreeView.SelectedNode.Tag as EventObject).MouseButtonEvent &&
                    !(String.Equals(key, Keys.RButton.ToString().ToUpper()) || String.Equals(key, Keys.LButton.ToString().ToUpper()) || String.Equals(key, Keys.MButton)))
                {
                    return true;
                }
                else
                {
                    if (eventsTreeView.SelectedNode.Tag is UserCommand && (eventsTreeView.SelectedNode.Tag as UserCommand).Event.MouseButtonEvent &&
                        !(String.Equals(key, Keys.RButton.ToString().ToUpper()) || String.Equals(key, Keys.LButton.ToString().ToUpper()) || String.Equals(key, Keys.MButton)))
                    {
                        return true;
                    }
                }
            }
            foreach (UserCommand command in mapping.Commands)
            {
                if((String.Equals(context, command.Context) || String.Equals(context, "global") || String.Equals(command.Context, "global")) && String.Equals(key, command.Key)
                    && String.Equals(modifier, command.Modifier) && String.Equals(activity, command.Activity) && !ReferenceEquals(command, eventsTreeView.SelectedNode.Tag))
                {
                    return true;
                }
                
                foreach (ExcludedKey exKey in mapping.ExcludedKeys)
                {
                    if (String.Equals(exKey.Key, key) && String.Equals(exKey.Modifier, modifier))
                    {
                        return true;
                    }
                }
                if ((!mouseCapable.Contains(context)) && (String.Equals(key, "RBUTTON") || String.Equals(key, "LBUTTON") ||
                    String.Equals(key, "MBUTTON")))
                {
                    return true;
                }

            }
            return false;
        }
    }
}