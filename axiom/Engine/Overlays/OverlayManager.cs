#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using log4net;
using Axiom.Core;
using Axiom.FileSystem;
using Axiom.Scripting;
using Axiom.Graphics;

namespace Axiom.Overlays {
    /// <summary>
    ///    Manages Overlay objects, parsing them from Ogre .overlay files and
    ///    storing a lookup library of them.
    /// </summary>
    public sealed class OverlayManager : ResourceManager {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static OverlayManager instance;

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        internal OverlayManager() {
            if (instance == null) {
                instance = this;
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static OverlayManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

        #region Fields
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OverlayManager));

        private int lastViewportWidth;
        private int lastViewportHeight;
        private bool viewportDimensionsChanged;
        private StringCollection loadedOverlays = new StringCollection();

        #endregion Fields

        public new Overlay GetByName(string name) {
            return (Overlay)base.GetByName(name);
        }

        /// <summary>
        ///		Creates and return a new overlay.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override Resource Create(string name, bool isManual) {
            Overlay overlay = new Overlay(name);
            Load(overlay, 1);
            return overlay;
        }

        /// <summary>
        ///		Internal method for queueing the visible overlays for rendering.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="queue"></param>
        /// <param name="viewport"></param>
        internal void QueueOverlaysForRendering(Camera camera, RenderQueue queue, Viewport viewport) {
            // Flag for update pixel-based OverlayElements if viewport has changed dimensions
            if(lastViewportWidth != viewport.ActualWidth ||
                lastViewportHeight != viewport.ActualHeight) {

                viewportDimensionsChanged = true;
                lastViewportWidth = viewport.ActualWidth;
                lastViewportHeight = viewport.ActualHeight;
            }
            else {
                viewportDimensionsChanged = false;
            }

            // TODO: optimize this resource list to avoid the foreach
            foreach(Overlay overlay in resourceList.Values) {
                overlay.FindVisibleObjects(camera, queue);
            }
        }

        public override void Dispose() {
            base.Dispose();
            instance = null;
        }

        #region Properties

        /// <summary>
        ///		Gets if the viewport has changed dimensions. 
        /// </summary>
        /// <remarks>
        ///		This is used by pixel-based GuiControls to work out if they need to reclaculate their sizes.
        ///	</remarks>																				  
        public bool HasViewportChanged {
            get { return viewportDimensionsChanged; }
        }

        /// <summary>
        ///		Gets the height of the destination viewport in pixels.
        /// </summary>
        public int ViewportHeight {
            get { return lastViewportHeight; } 
        }

        /// <summary>
        ///		Gets the width of the destination viewport in pixels.
        /// </summary>
        public int ViewportWidth {
            get { return lastViewportWidth; }
        }

        #endregion

        #region Script parsing methods

        /// <summary>
        ///    Load a specific overlay file by name.
        /// </summary>
        /// <remarks>
        ///    This is required from allowing .overlay scripts to include other overlay files.  It
        ///    is not guaranteed what order the files will be loaded in, so this can be used to ensure
        ///    depencies in a script are loaded prior to the script itself being loaded.
        /// </remarks>
        /// <param name="fileName"></param>
        public void LoadAndParseOverlayFile(string fileName) {
            if(loadedOverlays.Contains(fileName)) {
                log.InfoFormat("Skipping load of overlay include: {0}, as it is already loaded.", fileName);
                return;
            }

            // file has not been loaded, so load it now

            // look in local resource data
            Stream data = this.FindResourceData(fileName);

            if(data == null) {
                // wasnt found, so look in common resource data.
                data = ResourceManager.FindCommonResourceData(fileName);
            }

            // parse the overlay script
            ParseOverlayScript(data);
            data.Dispose();
        }

        /// <summary>
        ///    Parses all overlay files in resource folders and archives.
        /// </summary>
        public void ParseAllSources() {
            string extension = ".overlay";

            // search archives
            for(int i = 0; i < archives.Count; i++) {
                Archive archive = (Archive)archives[i];
                string[] files = archive.GetFileNamesLike("", extension);

                for(int j = 0; j < files.Length; j++) {
                    Stream data = archive.ReadFile(files[j]);

                    // parse the materials
                    ParseOverlayScript(data);
                }
            }

            // search common archives
            for(int i = 0; i < commonArchives.Count; i++) {
                Archive archive = (Archive)commonArchives[i];
                string[] files = archive.GetFileNamesLike("", extension);

                for(int j = 0; j < files.Length; j++) {
                    Stream data = archive.ReadFile(files[j]);

                    // parse the materials
                    ParseOverlayScript(data);
                }
            }
        }

        /// <summary>
        ///    Parses an attribute belonging to an Overlay.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="overlay"></param>
        private void ParseAttrib(string line, Overlay overlay) {
            string[] parms = line.Split(' ');

            if(parms[0].ToLower() == "zorder") {
                overlay.ZOrder = int.Parse(parms[1]);
            }
            else {
                ParseHelper.LogParserError(parms[0], overlay.Name, "Invalid overlay attribute.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="script"></param>
        /// <param name="line"></param>
        /// <param name="overlay"></param>
        /// <param name="isTemplate"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private bool ParseChildren(TextReader script, string line, Overlay overlay, bool isTemplate, OverlayElementContainer parent) {
            bool ret = false;
            int skipParam = 0;

            string[] parms = line.Split(' ', '(', ')');
            
            // split on lines with a ) will have an extra blank array element, so lets get rid of it
            if(parms[parms.Length - 1].Length == 0) {
                string[] tmp = new string[parms.Length - 1];
                Array.Copy(parms, 0, tmp, 0, parms.Length - 1);
                parms = tmp;
            }

            if(isTemplate) {
                // the first param = 'template' on a new child element
                if(parms[0] == "template") {
                    skipParam++;
                }
            }

            // top level component cannot be an element, it must be a container unless it is a template
            if(parms[0 + skipParam] == "container" || (parms[0 + skipParam] == "element" && (isTemplate || parent != null))) {
                string templateName = "";
                ret = true;

                // nested container/element
                if(parms.Length > 3 + skipParam) {
                    if(parms.Length != 5 + skipParam) {
                        log.WarnFormat("Bad element/container line: {0} in {1} - {2}, expecting ':' templateName", line, parent.Type, parent.Name); 
                        ParseHelper.SkipToNextCloseBrace(script);
                        return ret;
                    }
                    if(parms[3 + skipParam] != ":") {
                        log.WarnFormat("Bad element/container line: {0} in {1} - {2}, expecting ':' for element inheritance.", line, parent.Type, parent.Name);
                        ParseHelper.SkipToNextCloseBrace(script);
                        return ret;
                    }

                    // get the template name
                    templateName = parms[4 + skipParam];
                }
                else if(parms.Length != 3 + skipParam) {
                    log.WarnFormat("Bad element/container line: {0} in {1} - {2}, expecting 'element type(name)'.", line, parent.Type, parent.Name);
                    ParseHelper.SkipToNextCloseBrace(script);
                    return ret;
                }

                ParseHelper.SkipToNextOpenBrace(script);
                bool isContainer = (parms[0 + skipParam] == "container");
                ParseNewElement(script, parms[1 + skipParam], parms[2 + skipParam], isContainer, overlay, isTemplate, templateName, parent);
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="overlay"></param>
        /// <param name="element"></param>
        private void ParseElementAttrib(string line, Overlay overlay, OverlayElement element) {
            string[] parms = line.Split(' ');

            // get a string containing only the params
            string paramLine = line.Substring(line.IndexOf(' ', 0) + 1);

            // set the param, and hopefully it exists
            if(!element.SetParam(parms[0].ToLower(), paramLine)) {
                log.WarnFormat("Bad element attribute line: {0} for element '{1}'", line, element.Name);
            }
        }

        /// <summary>
        ///    Overloaded.  Calls overload with default of empty template name and null for the parent container.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="isContainer"></param>
        /// <param name="overlay"></param>
        /// <param name="isTemplate"></param>
        private void ParseNewElement(TextReader script, string type, string name, bool isContainer, Overlay overlay, bool isTemplate) {
            ParseNewElement(script, type, name, isContainer, overlay, isTemplate, "", null);
        }

        /// <summary>
        ///    Parses a new element
        /// </summary>
        /// <param name="script"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="isContainer"></param>
        /// <param name="overlay"></param>
        /// <param name="isTemplate"></param>
        /// <param name="templateName"></param>
        /// <param name="parent"></param>
        private void ParseNewElement(TextReader script, string type, string name, bool isContainer, Overlay overlay, bool isTemplate,
            string templateName, OverlayElementContainer parent) {
        
            string line;
            OverlayElement element = OverlayElementManager.Instance.CreateElementFromTemplate(templateName, type, name, isTemplate);

            if(parent != null) {
                // add this element to the parent container
                parent.AddChild(element);
            }
            else if(overlay != null) {
                overlay.AddElement((OverlayElementContainer)element);
            }

            while((line = ParseHelper.ReadLine(script)) != null) {
                // inore blank lines and comments
                if(line.Length > 0 && !line.StartsWith("//")) {
                    if(line == "}") {
                        // finished element
                        break;
                    }
                    else {
                        if(isContainer && ParseChildren(script, line, overlay, isTemplate, (OverlayElementContainer)element)) {
                            // nested children, so don't reparse it
                        }
                        else {
                            // element attribute
                            ParseElementAttrib(line, overlay, element);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///    Parses a 3D mesh which will be used in the overlay.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="meshName"></param>
        /// <param name="entityName"></param>
        /// <param name="overlay"></param>
        public void ParseNewMesh(TextReader script, string meshName, string entityName, Overlay overlay) {
        }

        /// <summary>
        ///    Parses an individual .overlay file.
        /// </summary>
        /// <param name="data"></param>
        public void ParseOverlayScript(Stream data) {
            string line = "";
            Overlay overlay = null;

            StreamReader script = new StreamReader(data, System.Text.Encoding.ASCII);

            // keep reading the file until we hit the end
            while((line = ParseHelper.ReadLine(script)) != null) {
                bool isTemplate = false;

                // ignore comments and blank lines
                if(line.Length > 0 && !line.StartsWith("//")) {
                    // does another overlay have to be included
                    if(line.StartsWith("#include")) {
                        // TODO: Handle included overlays
                        continue;
                    }

                    if(overlay == null) {
                        // no current overlay
                        // check to see if there is a template
                        if(line.StartsWith("template")) {
                            isTemplate = true;
                        }
                        else {
                            // the line in this case should be the name of the overlay
                            overlay = (Overlay)Create(line);
                            // cause the next line (open brace) to be skipped
                            ParseHelper.SkipToNextOpenBrace(script);

                            continue;
                        }
                    }
                    if(overlay != null || isTemplate) {
                        // already in overlay
                        string[] parms = line.Split(' ', '(', ')');
                        
                        // split on lines with a ) will have an extra blank array element, so lets get rid of it
                        if(parms[parms.Length - 1].Length == 0) {
                            string[] tmp = new string[parms.Length - 1];
                            Array.Copy(parms, 0, tmp, 0, parms.Length - 1);
                            parms = tmp;
                        }

                        if(line == "}") {
                            // finished overlay
                            overlay = null;
                            isTemplate = false;
                        }
                        else if(ParseChildren(script, line, overlay, isTemplate, null)) {
                        }
                        else if(parms[0] == "entity") {
                            // 3D element
                            if(parms.Length != 3) {
                                log.WarnFormat("Bad entity line: {0} in {1}, expected format - entity meshName(entityName)'", line, overlay.Name);
                                } // if parms...
                            else {
                                ParseHelper.SkipToNextOpenBrace(script);
                                ParseNewMesh(script, parms[1], parms[2], overlay);
                            }
                        }
                        else {
                            // must be an attribute
                            if(!isTemplate) {
                                ParseAttrib(line, overlay);
                            }
                        }
                    }
                }
            } 
        }

        #endregion
    }
}
