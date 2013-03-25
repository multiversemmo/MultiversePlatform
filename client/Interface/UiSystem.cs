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
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml;
using System.IO;

using Axiom.Utility;
using Axiom.Input;

using log4net;

using Multiverse.Gui;
using Multiverse.Lib.LogUtil;

using FontFamily = System.Drawing.FontFamily;

#endregion

namespace Multiverse.Interface
{
    public delegate void BindingHandler(string keystate);
    
    public class Binding {
        string name;
        string header;
        bool runOnUp;
        BindingHandler bindingHandler;

        public string Name {
            get {
                return name;
            }
            set {
                name = value;
            }
        }
        public string Header {
            get {
                return header;
            }
            set {
                header = value;
            }
        }
        public bool RunOnUp {
            get {
                return runOnUp;
            }
            set {
                runOnUp = value;
            }
        }
        public BindingHandler BindingHandler {
            get {
                return bindingHandler;
            }
            set {
                bindingHandler = value;
            }
        }
    }

    public class EventScript
    {
        public int scriptId;
        public string eventName;
        public string scriptCode;
        public object eventHandler;

        public static StringBuilder BuildCodeBuffer(string code) {
            StringBuilder codeBuilder = new StringBuilder();
            char[] lineDelims = { '\n' };
            string[] lines = code.Split(lineDelims);
            int indent = 0;
            foreach (string line in lines) {
                int trimmedLen = line.Trim().Length;
                if (trimmedLen != 0 && indent == 0) {
                    // non-empty line
                    indent = line.Length - line.TrimStart(null).Length;
                }
                // If it is all whitespace, or a comment, just print it
                if (trimmedLen == 0 || line.TrimStart(null).StartsWith("#"))
                    codeBuilder.AppendLine(line);
                else
                    codeBuilder.AppendLine(line.Substring(indent));
            }
            return codeBuilder;
        }

        /// <summary>
        ///   Generate script code to for the various script styled tags in 
        ///   the interface xml.  For a region of code in the script tags, 
        ///   a method will be generated containing the code, and will be 
        ///   assigned to the widget's event handler.
        /// </summary>
        public void SetupScriptEvent() {
            StringBuilder codeBuilder = BuildCodeBuffer(scriptCode);
            List<string> args = new List<string>();
            args.Add("this");
            args.Add("args");

            switch (eventName) {
                case "OnLoad":
                case "OnHide":
                case "OnShow":
                case "OnReceiveDrag":
                case "OnDragStart":
                case "OnDragStop":
                case "OnSizeChanged":
                case "OnEnterPressed":
                case "OnEscapePressed":
                case "OnTabPressed":
                case "OnSpacePressed":
                    UiSystem.EventScripts[scriptId].EventHandler =
                        UiScripting.SetupDelegate<EventHandler>(codeBuilder.ToString(), args);
                    break;
                case "OnEvent":
                    UiSystem.EventScripts[scriptId].EventHandler =
                        UiScripting.SetupDelegate<GenericEventHandler>(codeBuilder.ToString(), args);
                    break;
                case "OnUpdate":
                case "OnValueChanged":
                case "OnHorizontalScroll":
                case "OnVerticalScroll":
                case "OnMouseWheel":
                    UiSystem.EventScripts[scriptId].EventHandler =
                        UiScripting.SetupDelegate<FloatEventHandler>(codeBuilder.ToString(), args);
                    break;
                case "OnKeyUp":
                case "OnKeyDown":
                case "OnChar":
                    UiSystem.EventScripts[scriptId].EventHandler =
                        UiScripting.SetupDelegate<KeyboardEventHandler>(codeBuilder.ToString(), args);
                    break;
                case "OnEnter":
                case "OnLeave":
                case "OnMouseUp":
                case "OnMouseDown":
                case "OnClick":
                case "OnDoubleClick":
                    UiSystem.EventScripts[scriptId].EventHandler =
                        UiScripting.SetupDelegate<MouseEventHandler>(codeBuilder.ToString(), args);
                    break;
                case "OnScrollRangeChanged":
                    UiSystem.EventScripts[scriptId].EventHandler =
                        UiScripting.SetupDelegate<ExtendedEventHandler>(codeBuilder.ToString(), args);
                    break;
                default:
                    break;
            }
        }
        public object EventHandler {
            get {
                return eventHandler;
            }
            set {
                eventHandler = value;
            }
        }
    }

    public class UiSystem
    {
        public static string BaseLibraryFile = "Library.py";
        public static string StringMapFile = "StringMap.py";

        protected static Dictionary<string, ConstructorInfo> regionFactoryInfo = 
            new Dictionary<string, ConstructorInfo>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(UiSystem));

        public event EventHandler Load;
        public event EventHandler Unload;

        private static TimingMeter onUpdateMeter = MeterManager.GetMeter("OnUpdate", "UiSystem");

        protected static List<string> scriptFiles = new List<string>();

        // Cursors have different priorities.  The default cursor is the 
        // pointer, and is at the end of this list.  An earlier entry would 
        // be things like a picked up inventory icon or ability.  The
        // highest level (first) entries would be cursors for context 
        // specific actions like talk or attack.
        protected static List<ImageWindow> cursors = new List<ImageWindow>();

        // Dictionary of objects that are available in the scripting system's 
        // global namespace.
        protected static Dictionary<string, object> globalObjects =
            new Dictionary<string, object>();

        // Dictionary of bindings (from actioncode to method)
        protected static Dictionary<string, Binding> bindingMap =
            new Dictionary<string, Binding>();
        // Dictionary of strings for messages
        protected static Dictionary<string, string> stringMap =
            new Dictionary<string, string>();
        // Dictionary of named frames
        protected static Dictionary<string, Region> frameMap =
            new Dictionary<string, Region>();
        // Dictionary of virtual frames
        protected static Dictionary<string, Region> virtualFrameMap =
            new Dictionary<string, Region>();
        protected static Dictionary<int, EventScript> eventScripts =
            new Dictionary<int, EventScript>();
        public static int debugIndent = 0;
        public static int lastGeneratedId = 0;

        protected KeyBindings keyBindings;
        protected static Frame captureFrame = null;

        // Dictionary of event names and subscribers
        public static Dictionary<string, List<Frame>> eventRegistry =
            new Dictionary<string, List<Frame>>();

        protected static List<Frame> mouseOverFrames =
            new List<Frame>();
        protected static bool mouseDirty = false;

        // I'm tracking this, but I'm not sure what I'm supposed to do with it.
        // It seems like I should know about which frame has focus, but I have
        // other systems like the activeWindow from GuiSystem that tells me 
        // which window (or LayeredEditBox) has focus.
        protected Region focusFrame = null;
        protected Region defaultFocusFrame = null;

        protected Window window;
        /// <summary>
        ///   This is a mapping from a layout frame to the list of layout 
        ///   frames that are anchored to that layout frame
        /// </summary>
        protected Dictionary<Region, List<Region>> anchorLinks =
            new Dictionary<Region, List<Region>>();
        /// <summary>
        ///   This is the list of top level frames
        /// </summary>
        protected List<Region> frames = new List<Region>();
        /// <summary>
        ///   This is a list of elements that are anchored to the top
        ///   level window.
        /// </summary>
        protected List<Region> topElements = new List<Region>();

        public void LoadInterfaceFile(string file) {
            log.InfoFormat("Loading interface component from {0}", file);
            Stream stream = AssetManager.Instance.FindResourceData("Interface", file);
            XmlDocument document = new XmlDocument();
            document.Load(stream);
            foreach (XmlNode childNode in document.ChildNodes) {
                switch (childNode.Name) {
                    case "xml":
                        break;
                    case "Ui":
                        ReadNode(childNode);
                        break;
                    default:
                        if (childNode.NodeType != XmlNodeType.Comment)
                            log.InfoFormat("Unhandled xml tag: {0}", childNode.Name);
                        break;
                }
            }
            stream.Close();
        }

        public void LoadBindingsFile(string file) {
            log.InfoFormat("Loading bindings data from {0}", file);
            Stream stream = AssetManager.Instance.FindResourceData("Interface", file);
            XmlDocument document = new XmlDocument();
            document.Load(stream);
            foreach (XmlNode childNode in document.ChildNodes) {
                switch (childNode.Name) {
                    case "xml":
                        break;
                    case "Bindings":
                        ReadBindings(childNode);
                        break;
                    default:
                        if (childNode.NodeType != XmlNodeType.Comment)
                            log.InfoFormat("Unhandled xml tag: {0}", childNode.Name);
                        break;
                }
            }
            stream.Close();
        }

        public void LoadKeyBindings(string file) {
            log.InfoFormat("Loading bindings data from {0}", file);
            Stream stream = AssetManager.Instance.FindResourceData("Interface", file);
            keyBindings = new KeyBindings();
            keyBindings.Load(stream);
            stream.Close();
        }

        public static int AddEventScript(string eventName, string scriptCode) {
            EventScript script = new EventScript();
            script.scriptCode = scriptCode;
            script.eventName = eventName;
            script.scriptId = eventScripts.Count;
            eventScripts[script.scriptId] = script;
            return script.scriptId;
        }

        public static bool GetImage(string texture, out TextureAtlas imageset, out TextureInfo image) {
            imageset = null;
            image = null;
            bool createdImageset = false;
            string imageName = null;
            string imagesetName = null;
            string[] vals = null;
            if (texture != null) {
                char[] delims = { '/', '\\' };
                vals = texture.Split(delims);
            }
            if (vals != null && vals.Length >= 3) {
                imagesetName = vals[1];
                imageName = vals[2];
                if (!AtlasManager.Instance.ContainsKey(imagesetName)) {
                    // Load the additional plugin imageset
                    string imagesetFile =
                        AssetManager.Instance.ResolveResourceData("Imageset", imagesetName + ".xml");
                    if (imagesetFile == null) {
                        log.WarnFormat("Invalid imageset: {0}", imagesetName);
                        return false;
                    }
                    imageset = AtlasManager.Instance.CreateAtlas(imagesetFile);
                    createdImageset = true;
                }
                imageset = AtlasManager.Instance.GetTextureAtlas(imagesetName);
                if (imageset.ContainsKey(imageName))
                    image = imageset.GetTextureInfo(imageName);
            }
            return createdImageset;
        }

        public static string GenerateWindowName(string prefix) {
            ++lastGeneratedId;
            return string.Format("{0}_autoWindow:{1}", prefix, lastGeneratedId);
        }

        /// <summary>
        ///   Set the cursor.  Lower priority cursors have precedent.
        ///   Setting the texture to null will clear that priority level
        ///   of cursor, allowing lower priority curors (higher value) to
        ///   be active.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="texture"></param>
        public static void SetCursor(int priority, string texture) {
            TextureAtlas imageset = null;
            TextureInfo image = null;
            ImageWindow cursor = null;
            if (texture != null) {
                UiSystem.GetImage(texture, out imageset, out image);
                cursor = new ImageWindow("_cursor_" + priority);
                cursor.SetImage(image);
                // Make sure we have the size of the mouse cursor set.
                cursor.Size = new SizeF(32, 32);
            }
            while (cursors.Count <= priority)
                cursors.Add(null);
            cursors[priority] = cursor;
            RestoreCursor();
        }

        public static void ClearCursor() {
            // And now set the cursor to null to avoid display
            if (GuiSystem.Instance.DefaultCursor != null) {
                GuiSystem.Instance.DefaultCursor = null;
                GuiSystem.Instance.CurrentCursor = null;
            }
        }

        /// <summary>
        ///   Use the data in the cursors list to set up our cursor.
        ///   We will set our default and current cursor to the first
        ///   non-null cursor in the list.
        /// </summary>
        public static void RestoreCursor() {
            foreach (ImageWindow cursor in cursors) {
                if (cursor != null) {
                    GuiSystem.Instance.DefaultCursor = cursor;
                    GuiSystem.Instance.CurrentCursor = cursor;
                    break;
                }
            }
        }

        public static void RunBinding(string name) {
            RunBinding(name, "down");
        }

        public static void RunBinding(string name, string keystate) {
            if (bindingMap.ContainsKey(name)) {
                Binding binding = bindingMap[name];
                if (!binding.RunOnUp && keystate != "down")
                    return;
                try {
                    binding.BindingHandler(keystate);
                } catch (Exception e) {
                    LogUtil.ExceptionLog.ErrorFormat("Unable to run binding for {0}: {1}", name, e);
                }
            }
        }

        public void Setup() {
            foreach (Region region in UiSystem.FrameMap.Values) {
                try {
                    region.UpdateVisibility();
                } catch (Exception e) {
                    LogUtil.ExceptionLog.ErrorFormat("Unable to update visibility for a widget: {0}", e);
                }
            }
        }

        public void Cleanup() {
            ClearKeyBindings();
            ClearBindings();
            ClearFrames();
            OnUnload(new EventArgs());
        }

        protected void OnLoad(EventArgs e) {
            if (Load != null)
                Load(this, e);
        }
        protected void OnUnload(EventArgs e) {
            if (Unload != null)
                Unload(this, e);
        }

        public void ClearKeyBindings() {
            keyBindings = null;
        }

        public void ClearBindings() {
            UiSystem.bindingMap.Clear();
        }

        public void ClearFrames() {
            if (this.FocusedFrame != null) {
                EditBox editBox = this.FocusedFrame as EditBox;
                editBox.ClearFocus();
            }
            if (UiSystem.CaptureFrame != null) {
                Frame frame = UiSystem.CaptureFrame as Frame;
                frame.CaptureMouse(false);
            }
            foreach (Region frame in UiSystem.FrameMap.Values)
                frame.Dispose(false);
            foreach (Region frame in UiSystem.VirtualFrameMap.Values)
                frame.Dispose(false);
            UiSystem.scriptFiles.Clear();
            UiSystem.FrameMap.Clear();
            UiSystem.VirtualFrameMap.Clear();
            UiSystem.CaptureFrame = null; 
            anchorLinks = new Dictionary<Region, List<Region>>();
            frames = new List<Region>();
            focusFrame = null;
            defaultFocusFrame = null;
            topElements = new List<Region>();
        }

        public static void DebugDump() {
            foreach (Region frame in UiSystem.FrameMap.Values)
                DebugDump(0, frame);
        }

        public static void DebugDump(int indentLevel, InterfaceLayer ilayer) {
            string indent = new string(' ', indentLevel * 2);
            string msg = " " + ilayer.FrameStrata + "/" + ilayer.FrameLevel + "/" + ilayer.LayerLevel;
            if (ilayer is Region)
                log.DebugFormat("{0}{1}{2}", indent, ((Region)ilayer).Name, msg);
            else
                log.DebugFormat("{0} - {1}{2}", indent, ilayer, msg);
            if (ilayer is Frame) {
                Frame frame = (Frame)ilayer;
                foreach (InterfaceLayer childFrame in frame.Elements) {
                    DebugDump(indentLevel + 1, childFrame);
                }
            } else if (ilayer is Layer) {
                Layer frame = (Layer)ilayer;
                foreach (InterfaceLayer childFrame in frame.Elements) {
                    DebugDump(indentLevel + 1, childFrame);
                }
            }
        }

        public static void DebugDump(int indentLevel, Window window) {
            string indent = new string(' ', indentLevel * 2);
            LayerLevel level = LayerLevel.Unknown;
            FrameStrata strata = FrameStrata.Unknown;
            string textureName = "no texture";
            if (window is LayeredStaticImage) {
                level = ((LayeredStaticImage)window).LayerLevel;
                textureName = ((LayeredStaticImage)window).Image.Texture.Name;
                strata = ((LayeredStaticImage)window).FrameStrata;
            } else if (window is LayeredStaticText) {
                level = ((LayeredStaticText)window).LayerLevel;
                textureName = "*font*";
                strata = ((LayeredStaticText)window).FrameStrata;
            }
            Debug.Assert(window.Name == null || !window.Name.StartsWith("ScriptErrors"));
            log.InfoFormat("{0}Window: {1} {2} {3} {4} {5} {6} {7} {8}",
                indent, window.Name, window.Size, window.Position,
                window.DerivedPosition, window.Visible, level, strata, textureName);
            int childCount = window.ChildCount;
            for (int i = 0; i < childCount; ++i) {
                Window child = window.GetChildAtIndex(i);
                DebugDump(indentLevel + 1, child);
            }
        }

        internal void PrepareFrame(Window topWindow, Frame frame) {
            frame.ResolveParentStrings();
            frame.ResolveAnchors();
            frame.Ui = this;
            foreach (Region.Anchor anchor in frame.Anchors)
                RegisterAnchor(anchor);
            frame.Prepare(window);
            frame.SetupWindowPosition();

            frame.SetupEventScripts();
            frame.OnLoad(new EventArgs());
            frame.UpdateVisibility();
        }

        public void Prepare(Window topWindow) {
            window = topWindow;
            // window.MetricsMode = MetricsMode.Absolute;
            foreach (Region frame in frames)
                frame.ResolveParentStrings();
            foreach (Region frame in frames)
                frame.ResolveAnchors();
            foreach (Region frame in UiSystem.FrameMap.Values)
                frame.Ui = this;
            foreach (Region frame in UiSystem.FrameMap.Values)
                foreach (Region.Anchor anchor in frame.Anchors)
                    RegisterAnchor(anchor);
            foreach (Region frame in frames)
                // Prepare all top level frames and all named non-frame objects
                frame.Prepare(topWindow);
            foreach (Region frame in frames)
                frame.SetupWindowPosition();
        }

        public void RegisterAnchor(Region.Anchor anchor) {
            Region target = anchor.GetRelativeElement();
            Region source = anchor.GetElement();

            if (target == null)
            {
                // anchor off of the top level
                if (!topElements.Contains(source))
                    topElements.Add(source);
                return;
            }

            if (!anchorLinks.ContainsKey(target))
                anchorLinks[target] = new List<Region>();
            List<Region> sourceList = anchorLinks[target];
            if (!sourceList.Contains(source))
                sourceList.Add(source);
        }

        /// <summary>
        ///   The frame has changed its anchors, so we need to remove 
        ///   any existing links to this object, and add the entries for 
        ///   our anchor list again.
        /// </summary>
        /// <param name="frame"></param>
        public void NotifyAnchorsChanged(Region frame) {
            foreach (List<Region> val in anchorLinks.Values)
                val.Remove(frame);
            foreach (Region.Anchor anchor in frame.Anchors)
                RegisterAnchor(anchor);
        }

        /// <summary>
        ///   Notify all the frames that are anchored to the target frame
        ///   that the target frame has changed position or size.  This 
        ///   will call SetupWindowPosition on each anchored frame to
        ///   update the position and size of these frames.
        /// </summary>
        /// <param name="target"></param>
        public void NotifyAnchored(Region target) {
            // Debug.Assert(target.Name != "QuestLogQuestDescription");
            Debug.Assert(target != null);
            if (!anchorLinks.ContainsKey(target))
                return;
            List<Region> sourceList = anchorLinks[target];
            foreach (Region frame in sourceList) {
                if (!frame.IsPrepared)
                    continue;
                frame.ComputePlacement();
                frame.SetWindowProperties();
                NotifyAnchored(frame);
            }
        }

        public void SetGlobal(string name, object val) {
            globalObjects[name] = val;
        }

        /// <summary>
        ///  Calls to this must follow a call to UiScripting.SetupInterpreter
        /// </summary>
        public void SetupInterpreter() {
            // We should have already set up the interpreter
            //   UiScripting.SetupInterpreter();
            // Set up any global objects that can be used by scripting code
            foreach (string key in globalObjects.Keys) {
                log.InfoFormat("Set up global variable: {0}", key);
                UiScripting.SetVariable(key, globalObjects[key]);
            }
            // Run the base ui library file
            UiScripting.RunFile(BaseLibraryFile, null);
            UiScripting.RunFile(StringMapFile, null);
        }

        public void CompileCode() {
            // Set up the global names for the frames
            foreach (Region frame in UiSystem.FrameMap.Values)
                UiScripting.SetVariable(frame.Name, frame);
            // Run the various script files associated with the interfaces

            foreach (string scriptFile in UiSystem.scriptFiles) {
                bool rv = UiScripting.RunFile(scriptFile, null);
                if (!rv)
                    log.ErrorFormat("Failed to run script file: {0}", scriptFile);
            }
            // Compile all the event scripts
            foreach (EventScript eventScript in eventScripts.Values)
                eventScript.SetupScriptEvent();
            // string eventCode = UiSystem.GetScriptEventCode();
            //if (eventCode != null)
            //    UiScripting.RunScript(eventCode);

            // Set up the event handler code for the various widgets
            foreach (Region frame in UiSystem.FrameMap.Values) {
                if (frame is Frame) {
                    Frame tmp = frame as Frame;
                    tmp.SetupEventScripts();
                }
            }

            // foreach (LayoutFrame frame in UiSystem.FrameMap.Values)
            //	log.DebugFormat("LayoutFrame concrete frame: {0}", frame.Name);

            // foreach (LayoutFrame frame in UiSystem.VirtualFrameMap.Values)
            //	log.DebugFormat("LayoutFrame virtual frame: {0}", frame.Name);

            //foreach (LayoutFrame frame in frames)
            //    DebugDump(0, frame);

            // DebugDump(0, client.XmlUiWindow);

            // Run the OnLoad event if appropriate
            // I want to be able to create frames from the OnLoad handler.
            // I will not be able to call the OnLoad for those.
            List<Region> regions = new List<Region>(UiSystem.FrameMap.Values);
            foreach (Region region in regions) {
                try {
                    if (region is Frame) {
                        Frame frame = region as Frame;
                        frame.OnLoad(new EventArgs());
                        frame.UpdateVisibility();
                    }
                } catch (Exception e) {
                    LogUtil.ExceptionLog.ErrorFormat("Unable to run OnLoad event handler for a widget: {0}", e);
                }
            }
        }

        protected static int FrameInFront(Frame l, Frame r) {
            if (l.FrameStrata > r.FrameStrata) 
                return -1;
            else if (l.FrameStrata < r.FrameStrata)
                return 1;
            if (l.FrameLevel > r.FrameLevel)
                return -1;
            else if (l.FrameLevel < r.FrameLevel)
                return 1;
            if (l.LayerLevel > r.LayerLevel)
                return -1;
            else if (l.LayerLevel < r.LayerLevel)
                return 1;
            return 0;
        }

        public void HandleMouseDown(object sender, MouseEventArgs args) {
            // First see if the widget with capture should handle this event
            Frame frame = UiSystem.CaptureFrame;
            if (frame != null && frame.IsMouseEnabled() && !frame.IsHidden) {
                log.DebugFormat("Handling mouse down through capture frame: {0} ({1})", frame, frame.Name);
                frame.OnMouseDown(args);
                args.Handled = true;
                return;
            }
            if (GuiSystem.Instance.CurrentCursor != null) {
                PointF pt = GuiSystem.Instance.MousePosition;
                log.InfoFormat("Mouse Position: {0}", pt);
                List<Frame> hitFrames = GetFrames(pt);
                // These hit frames are sorted from front to back, so that
                // a widget in front that handles the mouse event will cause the
                // event to not get passed to frames that are behind.  There is
                // a minor question about whether the automatic handling (such
                // as that used by the pressed handler of a button) should mark
                // the event as handled.
                hitFrames.Sort(UiSystem.FrameInFront);
                foreach (Frame f in hitFrames) {
                    log.InfoFormat("Considering frame: {0}", f.Name);
                    try {
                        if (f != null && f.IsMouseEnabled() && !f.IsHidden) {
                            log.DebugFormat("Handling mouse down at {0} through hit frame: {1} ({2})", pt, f, f.Name);
                            f.OnMouseDown(args);
                            args.Handled = true;
                            return;
                        }
                    } catch (Exception ex) {
                        LogUtil.ExceptionLog.WarnFormat("Exception in frame event handler: {0}", ex);
                    }
                }
            }
            string bindingName = keyBindings.GetBinding(args.Button, args.Modifiers);
            HandleBinding(bindingName, args, true);
        }
        public void HandleMouseUp(object sender, MouseEventArgs args) {
            // First see if the widget with capture should handle this event
            Frame frame = UiSystem.CaptureFrame;
            if (frame != null && frame.IsMouseEnabled() && !frame.IsHidden) {
                log.DebugFormat("Handling mouse up through capture frame: {0} ({1})", frame, frame.Name);
                frame.OnMouseUp(args);
                args.Handled = true;
                return;
            }
            if (GuiSystem.Instance.CurrentCursor != null) {
                PointF pt = GuiSystem.Instance.MousePosition;
                List<Frame> hitFrames = GetFrames(pt);
                // These hit frames are sorted from front to back, so that
                // a widget in front that handles the mouse event will cause the
                // event to not get passed to frames that are behind.  There is
                // a minor question about whether the automatic handling (such
                // as that used by the pressed handler of a button) should mark
                // the event as handled.
                hitFrames.Sort(UiSystem.FrameInFront);
                foreach (Frame f in hitFrames) {
                    try {
                        if (f != null && f.IsMouseEnabled() && !f.IsHidden) {
                            log.DebugFormat("Handling mouse up at {0} through hit frame: {1} ({2})", pt, f, f.Name);
                            f.OnMouseUp(args);
                            args.Handled = true;
                            return;
                        }
                    } catch (Exception ex) {
                        LogUtil.ExceptionLog.WarnFormat("Exception in frame event handler: {0}", ex);
                    }
                }
            }
            string bindingName = keyBindings.GetBinding(args.Button, args.Modifiers);
            HandleBinding(bindingName, args, false);
        }

        /// <summary>
        ///   Currently, this is only called if the event was not already 
        ///   handled by a lower level object.  For example, the LayeredEditBox
        ///   has its own code to handle keyboard input, so an object of that
        ///   type may intercept and handle the key before we get it.
        ///   It may make sense to move that logic up into this library 
        ///   instead at some point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void HandleKeyDown(object sender, KeyEventArgs args) {
            string bindingName = keyBindings.GetBinding(args.Key, args.Modifiers);
            HandleBinding(bindingName, args, true);
        }
        /// <summary>
        ///   Currently, this is only called if the event was not already 
        ///   handled by a lower level object.  For example, the LayeredEditBox
        ///   has its own code to handle keyboard input, so an object of that
        ///   type may intercept and handle the key before we get it.
        ///   It may make sense to move that logic up into this library 
        ///   instead at some point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void HandleKeyUp(object sender, KeyEventArgs args) {
            string bindingName = keyBindings.GetBinding(args.Key, args.Modifiers);
            HandleBinding(bindingName, args, false);
        }

        protected void HandleBinding(string bindingName, InputEventArgs args, bool down) {
            if (bindingName == null)
                return;
            if (!bindingMap.ContainsKey(bindingName)) 
                return;
            Binding binding = bindingMap[bindingName];
            if (binding == null)
                return;
            if (down) {
                try {
                    binding.BindingHandler("down");
                } catch (Exception ex) {
                   LogUtil.ExceptionLog.ErrorFormat("Unable to run key binding event handler: {0}", ex);
                }
                args.Handled = true;
            } else if (binding.RunOnUp) {
                try {
                    binding.BindingHandler("up");
                } catch (Exception ex) {
                    LogUtil.ExceptionLog.ErrorFormat("Unable to run key binding event handler: {0}", ex);
                }
                args.Handled = true;
            }
        }

        public void HandleKeyPress(object sender, KeyEventArgs args) {
            return;
        }

        /// <summary>
        ///   Inject a mouse up event into the system, calling the 
        ///   OnMouseUp handler for any frame for the capture frame, 
        ///   and also for any frame that we are over.
        /// </summary>
        /// <param name="button"></param>
        //public static void InjectMouseUp(Axiom.Input.MouseButtons button) {
        //    MouseEventArgs e = GuiSystem.Instance.CreateMouseEventArgs(button);
        //    if (captureFrame != null) {
        //        if (captureFrame.HasMouseUpEvent) {
        //            if (!captureFrame.IsVisible())
        //                log.Debug("Invisible window: " + captureFrame.Name);
        //            captureFrame.OnMouseUp(e);
        //        }
        //    }
        //    Point pt = GuiSystem.Instance.MousePosition;
        //    List<Region> hitFrames = GetFrames(pt);
        //    foreach (Region entry in hitFrames) {
        //        try {
        //            if (!(entry is Frame))
        //                continue;
        //            Frame frame = entry as Frame;
        //            if (frame.HasMouseUpEvent) {
        //                if (!frame.IsVisible())
        //                    log.Debug("Invisible window: " + frame.Name);
        //                frame.OnMouseUp(e);
        //            }
        //        } catch (Exception ex) {
        //            log.WarnFormat("Exception in frame event handler: {0}", ex);
        //        }
        //    }
        //}

        public static void InjectMouseMove(MouseEventArgs e) {
            mouseDirty = true;
            if (captureFrame != null) {
                if (captureFrame.HasMouseMoveEvent) {
                    if (!captureFrame.IsVisible())
                        log.InfoFormat("Invisible window: {0}", captureFrame.Name);
                    captureFrame.OnMouseMove(e);
                    e.Handled = true;
                }
            }
        }

        public static bool InjectMouseWheel(float wheelStep) {
            bool handled = false;
            FloatEventArgs e = new FloatEventArgs();
            e.data = wheelStep;
            PointF pt = GuiSystem.Instance.MousePosition;
            List<Frame> hitFrames = GetFrames(pt);
            foreach (Frame frame in hitFrames) {
                try {
                    if (frame.HasMouseWheelEvent) {
                        if (!frame.IsVisible())
                            log.InfoFormat("Invisible window: {0}", frame.Name);
                        frame.OnMouseWheel(e);
                        handled = true;
                    }
                } catch (Exception ex) {
                    LogUtil.ExceptionLog.WarnFormat("Exception in frame event handler: {0}", ex);
                }
            }
            return handled;
        }

        // This is the system for periodic tick events -
        // these typically come in 5x/sec
        public static void InjectTick() {
        }

        // This runs through all widgets.. It would be more efficient
        // to maintain a list of widgets that are interested in updates
        public static void OnUpdate(float elapsed) {
            onUpdateMeter.Enter();
            FloatEventArgs args = new FloatEventArgs();
            args.data = elapsed;
            foreach (Region entry in FrameMap.Values) {
                try {
                    if (!(entry is Frame))
                        continue;
                    Frame frame = entry as Frame;
                    frame.OnUpdate(args);
                } catch (Exception ex) {
                    LogUtil.ExceptionLog.WarnFormat("Exception in frame event handler: {0}", ex);
                }
            }
            onUpdateMeter.Exit();
        }

        public void OnResize()
        {
            List<Region> dirtyElements = new List<Region>(topElements);
            foreach (Region entry in dirtyElements)
                entry.SetupWindowPosition();
            if (window != null)
            {
                window.Dirty = true;
                window.SetChildrenDirty();
            }
        }

        /// <summary>
        ///   Based on the mouse position, determine which frames we now cover
        ///   that we did not previously, and which frames we used to cover,
        ///   but do not now.  Call the OnEnter/OnLeave methods accordingly.
        /// </summary>
        public static void UpdateMouseOver() {
            MouseEventArgs e = GuiSystem.Instance.CreateMouseEventArgs();
            PointF pt = GuiSystem.Instance.MousePosition;
            List<Frame> coveredFrames = new List<Frame>();
            List<Frame> hitFrames = GetFrames(pt);
            foreach (Frame frame in hitFrames) {
                try {
                    if (frame.HasEnterEvent || frame.HasLeaveEvent)
                        coveredFrames.Add(frame);
                } catch (Exception ex) {
                    LogUtil.ExceptionLog.WarnFormat("Exception in frame event handler: {0}", ex);
                }
            }
            foreach (Frame frame in mouseOverFrames) {
                try {
                    log.DebugFormat("Mouse position of {0} was over frame: {1} ({2})", pt, frame, frame.Name);
                    if (coveredFrames.Contains(frame))
                        continue;
                    frame.OnLeave(e);
                } catch (Exception ex) {
                    LogUtil.ExceptionLog.WarnFormat("Exception in frame event handler: {0}", ex);
                }
            }
            foreach (Frame frame in coveredFrames) {
                try {
                    log.DebugFormat("Mouse position of {0} is over frame: {1} ({2})", pt, frame, frame.Name);
                    if (mouseOverFrames.Contains(frame))
                        continue;
                    frame.OnEnter(e);
                } catch (Exception ex) {
                    LogUtil.ExceptionLog.WarnFormat("Exception in frame event handler: {0}", ex);
                }
            }
            mouseOverFrames = coveredFrames;
            mouseDirty = false;
        }

        /// <summary>
        ///   Inject a mouse down event into the system, calling the 
        ///   OnMouseDown handler for any frame we are over.
        /// </summary>
        /// <param name="button"></param>
        //public static void InjectMouseDown(Axiom.Input.MouseButtons button) {
        //    MouseEventArgs e = GuiSystem.Instance.CreateMouseEventArgs(button);
        //    Point pt = GuiSystem.Instance.MousePosition;
        //    List<Region> hitFrames = GetFrames(pt);
        //    foreach (Region entry in hitFrames) {
        //        try {
        //            if (!(entry is Frame))
        //                continue;
        //            Frame frame = entry as Frame;
        //            if (frame.HasMouseDownEvent) {
        //                if (!frame.IsVisible())
        //                    log.Debug("Invisible window: " + frame.Name);
        //                frame.OnMouseDown(e);
        //            }
        //        } catch (Exception ex) {
        //            log.WarnFormat("Exception in frame event handler: {0}", ex);
        //        }
        //    }
        //}

        /// <summary>
        ///   Inject a mouse click event into the system, calling the 
        ///   OnClick handler for any frame we are over.
        /// </summary>
        /// <param name="button"></param>
        public static void InjectClick(Axiom.Input.MouseButtons button) {
            MouseEventArgs e = GuiSystem.Instance.CreateMouseEventArgs(button);
            PointF pt = GuiSystem.Instance.MousePosition;
            List<Frame> hitFrames = GetFrames(pt);
            hitFrames.Sort(UiSystem.FrameInFront);
            foreach (Frame frame in hitFrames)
            {
                try {
                    if (frame is Button) {
                        Button b = frame as Button;
                        if (b.HasClickEvent) {
                            if (!b.IsVisible())
                                log.InfoFormat("Invisible window: {0}", b.Name);
                            log.DebugFormat("Got mouse click at {0} over frame {1} ({2}).", pt, frame, frame.Name);
                            b.OnClick(e);
                            break;
                        }
                    }
                } catch (Exception ex) {
                    LogUtil.ExceptionLog.WarnFormat("Exception in frame event handler: {0}", ex);
                }
            }
        }

        // Return a list of the visible frames that cover this point.
        public static List<Frame> GetFrames(PointF pt) {
            List<Frame> rv = new List<Frame>();
            foreach (Region region in UiSystem.FrameMap.Values) {
                if (region is Frame && region.CheckHit(pt))
                    rv.Add(region as Frame);
                //log.InfoFormat("Mouse click at {0} hit {5} {1} with level {2} at {3} with dims {4}",
                //                    pt, frame.Name, frame.Layer, frame.Position, frame.Size, frame.GetInterfaceName());
            }
            return rv;
        }

        public static void RegisterEvent(string eventName, Frame frame) {
            if (!eventRegistry.ContainsKey(eventName))
                eventRegistry[eventName] = new List<Frame>();
            if (!eventRegistry[eventName].Contains(frame))
                eventRegistry[eventName].Add(frame);
        }

        public static void UnregisterEvent(string eventName, Frame frame) {
            if (!eventRegistry.ContainsKey(eventName))
                return;
            if (eventRegistry[eventName].Contains(frame))
                eventRegistry[eventName].Remove(frame);
        }

        public static void DispatchEvent(GenericEventArgs eventArgs) {
            string eventName = eventArgs.eventType;
            if (!eventRegistry.ContainsKey(eventName))
                return;
            List<Frame> subscribers = eventRegistry[eventName];
            foreach (Frame frame in subscribers) {
                try {
                    frame.OnEvent(eventArgs);
                } catch (Exception ex) {
                    LogUtil.ExceptionLog.WarnFormat("Exception in frame event handler: {0}", ex);
                }
            }
        }

        public Region GetFrame(string key) {
            return frameMap[key];
        }

        public Window Window {
            get {
                return window;
            }
        }

        public Region FocusedFrame {
            get {
                return focusFrame;
            }
            set {
                focusFrame = value;
            }
        }

        public Region DefaultFocusedFrame {
            get {
                return defaultFocusFrame;
            }
            set {
                defaultFocusFrame = value;
            }
        }

        public static Frame CaptureFrame {
            get {
                return captureFrame;
            }
            set {
                captureFrame = value;
            }
        }

        public static bool MouseDirty {
            get {
                return mouseDirty;
            }
        }

        #region Xml Parsing Methods

        protected virtual bool HandleAttribute(XmlAttribute attr) {
            return false;
        }

        public static Binding ReadBinding(XmlNode node) {
            Binding binding = new Binding();
            foreach (XmlAttribute attr in node.Attributes) {
                switch (attr.Name) {
                    case "name":
                        binding.Name = attr.Value;
                        break;
                    case "runOnUp":
                        binding.RunOnUp = bool.Parse(attr.Value);
                        break;
                    case "header":
                        binding.Header = attr.Value;
                        break;
                    default:
                        log.InfoFormat("Unexpected attribute: {0}", attr.Name);
                        break;
                }
            }
            StringBuilder codeBuilder = EventScript.BuildCodeBuffer(node.InnerXml);
            List<string> args = new List<string>();
            args.Add("keystate");
            binding.BindingHandler = 
                UiScripting.SetupDelegate<BindingHandler>(codeBuilder.ToString(), args);
            return binding;
        }
        public static void RegisterRegionFactories() {
            RegisterRegionFactory("Browser", typeof(Browser));
            RegisterRegionFactory("Button", typeof(Button));
            RegisterRegionFactory("CheckButton", typeof(CheckButton));
            RegisterRegionFactory("ColorSelect", typeof(ColorSelect));
            RegisterRegionFactory("EditBox", typeof(EditBox));
            RegisterRegionFactory("FontString", typeof(FontString));
            RegisterRegionFactory("Frame", typeof(Frame));
            RegisterRegionFactory("GameTooltip", typeof(GameTooltip));
            RegisterRegionFactory("Region", typeof(Region));
            RegisterRegionFactory("MessageFrame", typeof(MessageFrame));
            RegisterRegionFactory("Model", typeof(Model));
            RegisterRegionFactory("MovieFrame", typeof(MovieFrame));
            RegisterRegionFactory("ScrollFrame", typeof(ScrollFrame));
            RegisterRegionFactory("ScrollingMessageFrame", typeof(ScrollingMessageFrame));
            RegisterRegionFactory("SimpleHTML", typeof(SimpleHTML));
            RegisterRegionFactory("Slider", typeof(Slider));
            RegisterRegionFactory("StatusBar", typeof(StatusBar));
            RegisterRegionFactory("Texture", typeof(Texture));
        }

        public static void RegisterRegionFactory(string key, Type t) {
            ConstructorInfo ci = t.GetConstructor(Type.EmptyTypes);
            regionFactoryInfo[key] = ci;
        }
        public Region CreateFrame(string frameType, string frameName, Region uiParent, string inherits) {
            Region region = null;
            ConstructorInfo ci = null;
            if (regionFactoryInfo.TryGetValue(frameType, out ci))
                region = (Region)ci.Invoke(null);
            else
                return null;
            region.UiParent = uiParent;
            region.SetName(frameName);
            region.SetInheritTarget(inherits);
            if (region.Name == null)
                region.GenerateName(null);
            UiSystem.FrameMap[region.Name] = region;
            // ui parent cannot be a layer
            if (region.UiParent != null) {
                Debug.Assert(region.UiParent is Frame);
                ((Frame)region.UiParent).AddElement(region);
            }
            if (region is Frame)
                PrepareFrame(window, (Frame)region);
            return region;
        }

        public static Region ReadFrame(XmlNode node,
                                       Region uiParent,
                                       Layer layer) {
            Region frame = null;
            ConstructorInfo ci = null;
            switch(node.Name) {
                case "Script":
                    HandleScriptElement(node);
                    return null;
                case "Include":
                    return null;
                default:
                    if (regionFactoryInfo.TryGetValue(node.Name, out ci))
                        frame = (Region)ci.Invoke(null);
                    else
                        return null;
                    break;
            }
            ReadFrame(frame, node, uiParent, layer);
            return frame;
        }

        public static void ReadFrame(Region frame, XmlNode node,
                                     Region uiParent, Layer layer) {
            frame.UiParent = uiParent;
            frame.ReadNode(node);
            if (frame.Name == null)
                frame.GenerateName(null);
            if (!frame.Name.StartsWith("$parent")) {
                if (frame.IsVirtual)
                    UiSystem.VirtualFrameMap[frame.Name] = frame;
                else {
                    Debug.Assert(!UiSystem.FrameMap.ContainsKey(frame.Name),
                                 string.Format("Newly generated frame '{0}' conflicts with existing frame", frame.Name));
                    log.InfoFormat("Adding to FrameMap: {0}", frame.Name);
                    UiSystem.FrameMap[frame.Name] = frame;
                }
            }
            if (frame == null)
                return;
            if (layer != null) {
                layer.AddElement(frame);
                return;
            }
            if (frame.UiParent != null) {
                Debug.Assert(frame.UiParent is Frame);
                ((Frame)frame.UiParent).AddElement(frame);
            }
        }

        private static void HandleScriptElement(XmlNode node) {
            if (node is XmlElement) {
                XmlElement element = (XmlElement)node;
                if (!element.HasAttribute("file"))
                    return;
                string scriptFile = element.Attributes["file"].Value;
                scriptFiles.Add(scriptFile);
            }
        }

        protected virtual bool HandleElement(XmlElement node) {
            Region frame = UiSystem.ReadFrame(node, null, null);
            if (frame == null) {
                // The script element has no corresponding frame
                if (node.Name == "Script")
                    return true;
                return false;
            }
            // If it is a concrete top level frame, add it.
            if (frame.UiParent == null && !frame.IsVirtual)
                frames.Add(frame);
            log.InfoFormat("Added frame: {0}", frame.Name);
            return true;
        }

        public virtual void ReadBindings(XmlNode node) {
            string lastHeader = null;
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "Binding": {
                            Binding binding = ReadBinding(childNode);
                            if (binding.Header != null)
                                binding.Header = lastHeader;
                            else
                                lastHeader = binding.Header;
                            bindingMap.Add(binding.Name, binding);
                        }
                        break;
                    default:
                        log.InfoFormat("Unhandled element: {0}", childNode.Name);
                        break;
                }
            }
        }


        public virtual void ReadNode(XmlNode node) {
            foreach (XmlAttribute attr in node.Attributes)
                if (!HandleAttribute(attr))
                    log.WarnFormat("Unhandled attribute: {0}", attr.Name);

            foreach (XmlNode childNode in node.ChildNodes) {
                if (!(childNode is XmlElement))
                    log.WarnFormat("Ignoring non-element child: {0}", childNode.Name);
                else
                    if (!HandleElement(childNode as XmlElement))
                        log.WarnFormat("Unhandled element: {0}", childNode.Name);
            }
        }

        #endregion

        public static Dictionary<string, string> StringMap {
            get { return stringMap; }
        }
        public static Dictionary<string, Region> FrameMap {
            get { return frameMap; }
        }
        public static Dictionary<string, Region> VirtualFrameMap {
            get { return virtualFrameMap; }
        }
        public static Dictionary<int, EventScript> EventScripts {
            get { return eventScripts; }
        }
    }
}
