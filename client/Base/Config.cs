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

#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Diagnostics;

using log4net;

using Axiom.Collections;
using Axiom.Configuration;
using Axiom.Core;
using Axiom.Graphics;

using Multiverse.Utility;

#endregion

namespace Multiverse.Base
{
    public class DisplaySettings : ConfigCategory {

        public DisplayMode displayMode;
        public RenderSystem renderSystem;
        public string renderSystemName;
        public bool vSync = true;
        public string antiAliasing = "None";
        public bool allowNVPerfHUD = false;
        public int minScreenWidth = 800;
        public int minScreenHeight = 600;
        public bool allowFullScreen = true;

        public DisplaySettings(int minScreenWidth, int minScreenHeight, bool allowFullScreen) {
            this.minScreenWidth = minScreenWidth;
            this.minScreenHeight = minScreenHeight;
            this.allowFullScreen = allowFullScreen;
        }
        
        public DisplaySettings(DisplaySettings other) {
            other.CopyTo(this);
        }

        public void CopyTo(DisplaySettings dest) {
            dest.displayMode = this.displayMode;
            dest.renderSystem = this.renderSystem;
            dest.renderSystemName = this.renderSystemName;
            dest.vSync = this.vSync;
            dest.antiAliasing = this.antiAliasing;
            dest.allowNVPerfHUD = this.allowNVPerfHUD;
            dest.minScreenWidth = minScreenWidth;
            dest.minScreenHeight = minScreenHeight;
            dest.allowFullScreen = allowFullScreen;
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DisplaySettings));

        public override string GetName() {
            return "DisplayConfig";
        }

        public static DisplaySettings LoadConfig(int minScreenWidth, int minScreenHeight, bool allowFullScreen)
        {
            DisplaySettings settings = new DisplaySettings(minScreenWidth, minScreenHeight, allowFullScreen);
            XmlElement element = ConfigManager.Instance.GetCategoryData(settings);
            if (element != null)
                settings.ReadCategoryData(element);
            return settings;
        }

        public override void ReadCategoryData(XmlElement configNode) {
            try {
                int width = int.Parse(GetElementText(configNode, "Width"));
                int height = int.Parse(GetElementText(configNode, "Height"));
                int depth = int.Parse(GetElementText(configNode, "Depth"));
                bool fullScreen = bool.Parse(GetElementText(configNode, "Fullscreen"));
                renderSystemName = GetElementText(configNode, "RenderSystem");
                vSync = bool.Parse(GetElementText(configNode, "VSync"));
                antiAliasing = GetElementText(configNode, "AntiAliasing");
                allowNVPerfHUD = bool.Parse(GetElementText(configNode, "AllowNVPerfHUD"));
                displayMode = new DisplayMode(Math.Max(width, minScreenWidth), Math.Max(height, minScreenHeight), 
                    depth, allowFullScreen && fullScreen);
                List<RenderSystem> renderSystems = Root.Instance.RenderSystems;
                foreach (RenderSystem r in renderSystems)
                {
                    if (r.Name == renderSystemName)
                    {
                        renderSystem = r;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                log.WarnFormat("Unable to parse DisplayConfig configuration");
            }
        }

        public static void SaveDisplaySettings(DisplaySettings settings) {
            ConfigManager.Instance.WriteCategoryData(settings);
        }
        

        public override void WriteCategoryData(XmlElement element) {
            AddElement(element, "RenderSystem", renderSystemName);
            AddElement(element, "Width", displayMode.Width.ToString());
            AddElement(element, "Height", displayMode.Height.ToString());
            AddElement(element, "Depth", displayMode.Depth.ToString());
            AddElement(element, "Fullscreen", displayMode.Fullscreen.ToString());
            AddElement(element, "VSync", vSync.ToString());
            AddElement(element, "AntiAliasing", antiAliasing);
            AddElement(element, "AllowNVPerfHUD", allowNVPerfHUD.ToString());
        }
    }

    /// <summary>
    ///    This class loads and persists world-specific voice chat
    ///    configuration information.  The constructor takes a world
    ///    name.
    /// </summary>
    public class VoiceChatConfig : ConfigCategory {
        private static VoiceChatConfig instance = null;
        private static bool defaultEnabled = false;
        private static bool defaultInputEnabled = true;
        private string worldName;
        private bool enabled = defaultEnabled;
        private bool inputEnabled = defaultInputEnabled;
        private string micDevice = "";
        private string playbackDevice = "";
        private int micLevel = 10;
        private float playbackLevel = 1.0f;
        private bool vadEnabled = true;

        public VoiceChatConfig(string worldName) {
            this.worldName = worldName;
            instance = this;
        }

        public void LoadCategoryData() {
            XmlElement element = ConfigManager.Instance.GetCategoryData(this, "WorldName", worldName);
            if (element != null)
                ReadCategoryData(element);
        }

        public void WriteCategoryData() {
            ConfigManager.Instance.WriteCategoryData(this, "WorldName", worldName);
        }

        public override void ReadCategoryData(XmlElement configNode) {
            worldName = GetElementText(configNode, "WorldName");
            enabled = bool.Parse(GetElementText(configNode, "Enabled"));
            inputEnabled = bool.Parse(GetElementText(configNode, "InputEnabled"));
            micDevice = GetElementText(configNode, "MicDevice");
            playbackDevice = GetElementText(configNode, "PlaybackDevice");
            micLevel = int.Parse(GetElementText(configNode, "MicLevel"));
            playbackLevel = float.Parse(GetElementText(configNode, "PlaybackLevel"));
            vadEnabled = bool.Parse(GetElementText(configNode, "VADEnabled"));
        }

        public override void WriteCategoryData(XmlElement element) {
            AddElement(element, "WorldName", worldName);
            AddElement(element, "Enabled", enabled.ToString());
            AddElement(element, "InputEnabled", inputEnabled.ToString());
            AddElement(element, "MicDevice", micDevice);
            AddElement(element, "PlaybackDevice", playbackDevice);
            AddElement(element, "MicLevel", micLevel.ToString());
            AddElement(element, "PlaybackLevel", playbackLevel.ToString());
            AddElement(element, "VADEnabled", vadEnabled.ToString());
        }

        public override string GetName() {
            return "VoiceChatConfig";
        }

        public static VoiceChatConfig Instance {
            get {
                return instance;
            }
        }
        
        public static bool DefaultEnabled {
            get {
                return defaultEnabled;
            }
            set {
                defaultEnabled = value;
            }
        }
        
        public static bool DefaultInputEnabled {
            get {
                return defaultInputEnabled;
            }
            set {
                defaultInputEnabled = value;
            }
        }
        
        public bool Enabled {
            get {
                return enabled;
            }
            set {
                enabled = value;
            }
        }
        
        public bool InputEnabled {
            get {
                return inputEnabled;
            }
            set {
                inputEnabled = value;
            }
        }
        
        public string MicDevice {
            get {
                return micDevice;
            }
            set {
                micDevice = value;
            }
        }

        public string PlaybackDevice {
            get {
                return playbackDevice;
            }
            set {
                playbackDevice = value;
            }
        }
        
        public int MicLevel {
            get {
                return micLevel;
            }
            set {
                micLevel = value;
            }
        }

        public float PlaybackLevel {
            get {
                return playbackLevel;
            }
            set {
                playbackLevel = value;
            }
        }

        public bool VADEnabled {
            get {
                return vadEnabled;
            }
            set {
                vadEnabled = value;
            }
        }
        
    }

}
