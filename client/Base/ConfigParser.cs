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
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Net;

using log4net;

using Multiverse.Network;

namespace Multiverse.Base {
    /// <summary>
    ///   This class is used to parse the world_settings.xml file with 
    ///   information about which server we are connecting to.  Before 
    ///   the official master server was deployed, this was the way to
    ///   specify most of the information about a world, but these days
    ///   it is mostly useful for development when you are not connected
    ///   to the network.
    /// </summary>
    public class ConfigParser {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ConfigParser));

        public class LoopbackWorldResponse {
            public WorldServerEntry worldServerEntry;
            public byte[] idToken;
            public byte[] oldToken;
        }

        string clientLoginUrl;
        string filename;
        List<LoopbackWorldResponse> worldServerEntries = new List<LoopbackWorldResponse>();

        public ConfigParser(string filename) {
            this.clientLoginUrl = null;
            this.filename = filename;
        }

        public LoopbackWorldResponse GetWorldServerEntry(string worldId) {
            if (worldServerEntries.Count == 0)
                return null;
            if (worldId == null || worldId == string.Empty)
                return worldServerEntries[0];
            foreach (LoopbackWorldResponse response in worldServerEntries) {
                if (response.worldServerEntry.WorldName == worldId)
                    return response;
            }
            return null;
        }

        public bool Load() {
            if (!File.Exists(filename))
                return false;
            Stream stream = File.Open(filename, FileMode.Open);
            XmlDocument document = new XmlDocument();
            document.Load(stream);
            foreach (XmlNode child in document.ChildNodes) {
                switch (child.Name) {
                    case "client_config":
                        ReadClientConfig(child);
                        break;
                }
            }
            stream.Close();
            return true;
        }

        public void ReadClientConfig(XmlNode node) {
            foreach (XmlNode child in node.ChildNodes) {
                switch (child.Name) {
                    case "loopback_world_response":
                        ReadLoopbackWorldResponse(child);
                        break;
                    case "login_url":
                        if (child.Attributes["href"] != null)
                            clientLoginUrl = child.Attributes["href"].Value;
                        else
                            log.Warn("login_url element missing href attribute");
                        break;
                }
            }
        }

        public void ReadLoopbackWorldResponse(XmlNode node) {
            // WorldResponse response;
            string world_id = null;
            string update_url = null;
            string patcher_url = null;
            string hostname = null;
            int port = 0;
            List<string> world_repositories = new List<string>();
            bool standalone = false;
            string startup_script = null;
            if (node.Attributes["world_id"] == null)
                log.Warn("loopback_world_response element missing world_id attribute");
            else
                world_id = node.Attributes["world_id"].Value;
            LoopbackWorldResponse loopbackWorldResponse = new LoopbackWorldResponse();
            foreach (XmlNode child in node.ChildNodes) {
                switch (child.Name) {
                    case "account":
                        if (child.Attributes["id_token"] != null) {
                            string token = child.Attributes["id_token"].Value;
                            loopbackWorldResponse.idToken = Convert.FromBase64String(token);
                        } else if (child.Attributes["id_number"] != null) {
                            string number = child.Attributes["id_number"].Value;
                            int id = int.Parse(number);

                            // Make an old style token for older servers
                            loopbackWorldResponse.oldToken = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(id));

                            // Build a fake token to send to the login server
                            OutgoingMessage tokenBuilder = new OutgoingMessage();
                            tokenBuilder.Write((byte)1);      // version
                            tokenBuilder.Write((byte)1);      // type
                            tokenBuilder.Write("master");     // issuer
                            tokenBuilder.Write(1L);           // token id
                            tokenBuilder.Write(1L);           // key id
                            tokenBuilder.Write(0L);           // expiry
                            tokenBuilder.Write((byte)24);     // TreeMap type
                            tokenBuilder.Write(1);            // num entries
                            tokenBuilder.Write("account_id"); // entry key
                            tokenBuilder.Write((byte)3);      // int type
                            tokenBuilder.Write(id);           // account_id
                            tokenBuilder.Write((byte)0);      // authenticator
                            loopbackWorldResponse.idToken = tokenBuilder.GetBytes();
                        } else {
                            log.Warn("account element missing multiverse_id attribute");
                        }
                        break;
                    case "update_url":
                        if (child.Attributes["href"] != null)
                            update_url = child.Attributes["href"].Value;
                        else
                            log.Warn("update_url element missing href attribute");
                        break;
                    case "patcher_url":
                        if (child.Attributes["href"] != null)
                            patcher_url = child.Attributes["href"].Value;
                        else
                            log.Warn("patcher_url element missing href attribute");
                        break;
                    case "server":
                        if (child.Attributes["hostname"] != null)
                            hostname = child.Attributes["hostname"].Value;
                        else
                            log.Warn("server element missing hostname attribute");
                        if (child.Attributes["port"] != null) {
                            if (!int.TryParse(child.Attributes["port"].Value, out port))
                                log.Warn("server element has invalid port attribute");
                        } else
                            log.Warn("server element missing port attribute");
                        break;
                    case "world_repository":
                        if (child.Attributes["world_id"] != null)
                            world_repositories.Add(Path.Combine(WorldServerEntry.WorldsFolder, child.Attributes["world_id"].Value));
                        else if (child.Attributes["path"] != null)
                            world_repositories.Add(child.Attributes["path"].Value);
                        else
                            log.Warn("world_repository element missing world_id or path attribute");
                        break;
                    case "standalone":
                        if (child.Attributes["script"] != null) {
                            standalone = true;
                            startup_script = child.Attributes["script"].Value;
                        } else
                            log.Warn("standalone element missing script attribute");
                        break;
                }
            }
            loopbackWorldResponse.worldServerEntry = new WorldServerEntry(world_id, hostname, port, patcher_url, update_url);
            loopbackWorldResponse.worldServerEntry.WorldRepositoryDirectories = world_repositories;
            loopbackWorldResponse.worldServerEntry.Standalone = standalone;
            loopbackWorldResponse.worldServerEntry.StartupScript = startup_script;
            worldServerEntries.Add(loopbackWorldResponse);
        }

        public string LoginUrl {
            get {
                return clientLoginUrl;
            }
        }
    }
}
