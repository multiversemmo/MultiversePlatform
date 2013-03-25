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
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;
using System.Diagnostics;

using Multiverse.Network;

namespace Multiverse.Test {
    public class WorldEntry {
        public string worldId;
        public string worldName;
        public bool isDefault;
        public WorldEntry(string id, string name, bool def) {
            worldId = id;
            worldName = name;
            isDefault = def;
        }
    }

    /// <summary>
    ///   This class is made available to the browser control.
    /// </summary>
    public class LoginHelper {
        LoginSettings loginSettings;
        NetworkHelper networkHelper;

        string username = string.Empty;
        string password = string.Empty;
        string statusText = string.Empty;
        string errorText = string.Empty;
        string worldId = string.Empty;
        bool rememberUsername = false;
        bool fullScan = false;
        string preferredWorldId = string.Empty;

        List<WorldEntry> worldEntries = new List<WorldEntry>();

        public LoginHelper(LoginSettings loginSettings, NetworkHelper networkHelper) {
            this.loginSettings = loginSettings;
            this.networkHelper = networkHelper;
            if (loginSettings.worldId != string.Empty)
                preferredWorldId = loginSettings.worldId;
        }
        public void ReloadSettings() {
            ParseConfig("../Config/login_settings.xml");
        }

        public void LoginMaster(string worldId) {
            // MessageBox.Show(string.Format("In Login: {0}:{1} => {2} {3}", username, password, worldId, remember));

            Trace.TraceInformation("LoginButton_Click");

            loginSettings.username = username;
            loginSettings.password = password;
            loginSettings.worldId = worldId;

            StatusMessage = "Connecting ...";
            NetworkHelperStatus rv = networkHelper.LoginMaster(loginSettings);
            System.Diagnostics.Trace.TraceInformation("Login return: " + rv);

            switch (rv) {
                case NetworkHelperStatus.WorldResolveSuccess:
                    System.Diagnostics.Trace.TraceInformation("Success: " + rv);
                    break;
                case NetworkHelperStatus.WorldResolveFailure:
                    StatusMessage = "";
                    ErrorMessage = "Unable to resolve world id";
                    break;
                case NetworkHelperStatus.LoginFailure:
                    StatusMessage = "";
                    ErrorMessage = "Invalid username or password";
                    break;
                case NetworkHelperStatus.MasterTcpConnectFailure:
                case NetworkHelperStatus.MasterConnectFailure:
                    StatusMessage = "";
                    ErrorMessage = "Unable to connect to master server";
                    break;
                default:
                    StatusMessage = "";
                    ErrorMessage = "Unable to login";
                    break;
            }
        }

        /// <summary>
        ///   Get the number of worlds that are locally defined (either from 
        ///   something akin to a quicklist, or passed on the command line).
        /// </summary>
        /// <returns></returns>
        public int GetWorldCount()
        {
            return worldEntries.Count;
        }
        /// <summary>
        ///   Get the world entry at a given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public WorldEntry GetWorldEntry(int index)
        {
            return worldEntries[index];
        }

        /// <summary>
        ///   This is used so that the client can pass in a preferred world
        ///   with a flag like --world_id and we can select the associated option
        ///   from the script in the login page.
        /// </summary>
        /// <returns>the preferred world id if it has been set, or the empty string if it has not</returns>
        public string GetPreferredWorld() {
            return preferredWorldId;
        }        

        internal void ParseConfig(string configFile) {
            worldEntries.Clear();
            worldId = string.Empty;
            password = string.Empty;
            username = string.Empty;
            try {
                Stream stream = File.OpenRead(configFile);
                try {
                    XmlDocument document = new XmlDocument();
                    document.Load(stream);
                    foreach (XmlNode childNode in document.ChildNodes) {
                        switch (childNode.Name) {
                            case "LoginSettings":
                                ReadLoginSettings(childNode);
                                break;
                            default:
                                DebugMessage(childNode);
                                break;
                        }
                    }
                } finally {
                    stream.Close();
                }
            } catch (Exception e) {
                Trace.TraceInformation("Unable to load login settings: " + e);
            }         
        }

        protected void ReadLoginSettings(XmlNode node) {
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "accounts":
                        ReadAccounts(childNode);
                        break;
                    case "worlds":
                        ReadWorlds(childNode);
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
        }

        protected void ReadAccounts(XmlNode node) {
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "account":
                        ReadAccount(childNode);
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
#if OLD_SCHOOL
			if (this.Username == "" && usernameComboBox.Items.Count > 0)
				this.Username = (string)usernameComboBox.Items[0];
#endif
        }

        protected void ReadWorlds(XmlNode node) {
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "world":
                        ReadWorld(childNode);
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
        }

        protected void ReadAccount(XmlNode node) {
            bool isDefault = false;
            foreach (XmlAttribute attr in node.Attributes) {
                switch (attr.Name) {
                    case "username":
                        username = attr.Value;
                        // TODO: implement me
                        // usernameComboBox.Items.Add(username);
                        break;
                    case "password":
                        password = attr.Value;
                        // TODO: implement me
                        //if (!passwordSet) {
                        //    passwordTextBox.Text = password;
                        //    passwordSet = true;
                        //}
                        break;
                    case "default":
                        isDefault = bool.Parse(attr.Value);
                        break;
                    default:
                        DebugMessage(node, attr);
                        break;
                }
            }
#if OLD_SCHOOL
			if (isDefault) {
				this.Username = username;
				this.passwordTextBox.Text = password;
			}
#endif
        }

        protected void ReadWorld(XmlNode node) {
            bool isDefault = false;
            string worldName = string.Empty;
            string worldId = string.Empty;
            foreach (XmlAttribute attr in node.Attributes) {
                switch (attr.Name) {
                    case "name":
                        worldName = attr.Value;
                        break;
                    case "id":
                        worldId = attr.Value;
                        break;
                    case "default":
                        isDefault = bool.Parse(attr.Value);
                        break;
                    default:
                        DebugMessage(node, attr);
                        break;
                }
            }
            worldEntries.Add(new WorldEntry(worldId, worldName, isDefault));
        }

        protected void DebugMessage(XmlNode node) {
            Trace.WriteLine("Unhandled node type: " + node.Name +
                            " with parent of " + node.ParentNode.Name);
        }

        protected void DebugMessage(XmlNode node, XmlAttribute attr) {
            Trace.WriteLine("Unhandled node attribute: " + attr.Name +
                            " with parent node of " + node.Name);
        }

        public string Username {
            get {
                return username;
            }
            set {
                username = value;
            }
        }
        public string Password {
            get {
                return password;
            }
            set {
                password = value;
            }
        }
        public string StatusMessage {
            get {
                return statusText;
            }
            set {
                statusText = value;
            }
        }
        public string ErrorMessage {
            get {
                return errorText;
            }
            set {
                errorText = value;
            }
        }
        public bool FullScan {
            get {
                return fullScan;
            }
            set {
                fullScan = value;
            }
        }
        public bool RememberUsername {
            get {
                return rememberUsername;
            }
            set {
                rememberUsername = value;
            }
        }
    }
}
