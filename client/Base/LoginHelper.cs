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
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

using log4net;

using Multiverse.Network;
using Multiverse.Patcher;
using Multiverse.Lib.LogUtil;

namespace Multiverse.Base {
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
    ///   This class is made available to the browser control, and provides 
    ///   the interface for the browser to choose a world, login, and display
    ///   any status information required.
    /// </summary>
    public class LoginHelper : UpdateHelper {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(LoginHelper));

        LoginForm parentForm;
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
        string logFile = null;
        bool patchMedia = true;

        List<WorldEntry> worldEntries = new List<WorldEntry>();

        public LoginHelper(LoginForm parentForm, LoginSettings loginSettings, 
                           NetworkHelper networkHelper, string logFile, bool patchMedia) : 
            base(parentForm) 
        {
            this.parentForm = parentForm;
            this.loginSettings = loginSettings;
            this.networkHelper = networkHelper;
            if (loginSettings.worldId != string.Empty)
                preferredWorldId = loginSettings.worldId;
            this.logFile = logFile;
            this.patchMedia = patchMedia;
        }

        public void ReloadSettings() {
            ParseConfig("../Config/login_settings.xml");
        }

        public void LoginMaster(string worldId) {
            // MessageBox.Show(string.Format("In Login: {0}:{1} => {2} {3}", username, password, worldId, remember));

            log.Info("LoginButton_Click");

            loginSettings.username = username;
            loginSettings.password = password;
            loginSettings.worldId = worldId;

            StatusMessage = "Logging In ...";
            NetworkHelperStatus rv = LoginMaster();
            if (rv != NetworkHelperStatus.Success)
                return;
            StatusMessage = "Resolving World ...";
            ResolveWorld();
        }

        protected NetworkHelperStatus LoginMaster() {
            NetworkHelperStatus rv = networkHelper.LoginMaster(loginSettings);
            log.InfoFormat("Login return: {0}", rv);
            switch (rv) {
                case NetworkHelperStatus.Success:
                    break;
                case NetworkHelperStatus.LoginFailure:
                    StatusMessage = "";
                    ErrorMessage = "Invalid username or password";
                    break;
                case NetworkHelperStatus.MasterTcpConnectFailure:
                    StatusMessage = "";
                    ErrorMessage = "Unable to connect to master tcp server";
                    break;
                default:
                    StatusMessage = "";
                    ErrorMessage = "Unable to login";
                    break;
            }
            return rv;
        }

        protected NetworkHelperStatus ResolveWorld() {
            // We may already have the world entry.. if so, skip the resolve
            if (networkHelper.HasWorldEntry(loginSettings.worldId)) {
                parentForm.DialogResult = DialogResult.OK;
                parentForm.Close();
                return NetworkHelperStatus.WorldResolveSuccess;
            }
            NetworkHelperStatus rv = networkHelper.ResolveWorld(loginSettings);
            switch (rv) {
                case NetworkHelperStatus.WorldResolveSuccess:
                    parentForm.DialogResult = DialogResult.OK;
                    log.InfoFormat("Success: {0}", rv);
                    parentForm.Close();
                    break;
                case NetworkHelperStatus.WorldResolveFailure:
                    StatusMessage = "";
                    ErrorMessage = "Unable to resolve world id";
                    break;
                case NetworkHelperStatus.MasterConnectFailure:
                    StatusMessage = "";
                    ErrorMessage = "Unable to connect to master rdp server";
                    break;
                default:
                    StatusMessage = "";
                    ErrorMessage = "Unable to resolve world";
                    break;
            }
            return rv;
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
                if (!File.Exists(configFile))
                    return;
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
                LogUtil.ExceptionLog.InfoFormat("Unable to load login settings: {0}", e.Message);
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

        public override bool NeedUpdate()
        {
            if (!patchMedia)
                return false;
            return base.NeedUpdate();
        }

        public bool SetWorld(string worldId)
        {
            string savedWorldId = loginSettings.worldId;
            if (!networkHelper.HasWorldEntry(loginSettings.worldId))
            {
                NetworkHelperStatus status = networkHelper.ResolveWorld(loginSettings);
                if (status != NetworkHelperStatus.WorldResolveSuccess)
                {
                    // revert the loginSettings
                    loginSettings.worldId = savedWorldId;
                    return false;
                }
            }
            WorldServerEntry entry = networkHelper.GetWorldEntry(worldId);
            parentForm.AbortUpdate();
            parentForm.Updater.FullScan = false;
            parentForm.Updater.BaseDirectory = entry.WorldRepository;
            parentForm.Updater.UpdateUrl = entry.UpdateUrl;
            parentForm.Updater.SetupLog(logFile);
            return true;
        }

        protected void DebugMessage(XmlNode node) {
            log.WarnFormat("Unhandled node type: {0} with parent of {1}", node.Name, node.ParentNode.Name);
        }

        protected void DebugMessage(XmlNode node, XmlAttribute attr) {
            log.WarnFormat("Unhandled node attribute: {0} with parent node of {1}", attr.Name, node.Name);
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
        public bool PatchMedia {
            get {
                return patchMedia;
            }
            set {
                patchMedia = value;
            }
        }
    }
}
