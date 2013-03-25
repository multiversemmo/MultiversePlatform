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
using System.Collections.Generic;
// using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Axiom.Controllers;
using Axiom.Core;
using Axiom.FileSystem;
using Axiom.MathLib;
using Axiom.Scripting;
using Axiom.Serialization;

namespace Axiom.Graphics {
    /// <summary>
    ///     Class for managing Material settings.
    /// </summary>
    /// <remarks>
    ///     Materials control the eventual surface rendering properties of geometry. This class
    ///     manages the library of materials, dealing with programmatic registrations and lookups,
    ///     as well as loading predefined Material settings from scripts.
    ///     <p/>
    ///     When loaded from a script, a Material is in an 'unloaded' state and only stores the settings
    ///     required. It does not at that stage load any textures. This is because the material settings may be
    ///     loaded 'en masse' from bulk material script files, but only a subset will actually be required.
    ///     <p/>
    ///     Because this is a subclass of ResourceManager, any files loaded will be searched for in any path or
    ///     archive added to the resource paths/archives. See ResourceManager for details.
    ///     <p/>
    ///     For a definition of the material script format, see http://www.ogre3d.org/docs/manual/manual_16.html#SEC25.
    /// </summary>
    public class MaterialManager : ResourceManager {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static MaterialManager instance;

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        internal MaterialManager() {
            if (instance == null) {
                instance = this;

				this.SetDefaultTextureFiltering(TextureFiltering.Bilinear);
                defaultMaxAniso = 1;

                // Default scheme
                schemes = new Dictionary<string, int>();
                activeSchemeIndex = 0;
                activeSchemeName = DefaultSchemeName;
                schemes[activeSchemeName] = 0;
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static MaterialManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

        #region Delegates

        delegate void PassAttributeParser(string[] values, Pass pass);
        delegate void TextureUnitAttributeParser(string[] values, TextureUnitState texUnit);

        #endregion

        #region Fields

        /// <summary>
        ///     Default Texture filtering - minification.
        /// </summary>
        protected FilterOptions defaultMinFilter;
        /// <summary>
        ///     Default Texture filtering - magnification.
        /// </summary>
        protected FilterOptions defaultMagFilter;
        /// <summary>
        ///     Default Texture filtering - mipmapping.
        /// </summary>
        protected FilterOptions defaultMipFilter;
        /// <summary>
        ///     Default Texture anisotropy.
        /// </summary>
        protected int defaultMaxAniso;
		/// <summary>
		///		Used for parsing material scripts.
		/// </summary>
		protected MaterialSerializer serializer = new MaterialSerializer();
		protected TextureFiltering filtering;
        /// <summary>
        ///		Scheme name -> index. Never shrinks! Should be pretty static anyway
        /// </summary>
        protected Dictionary<string, int> schemes;
        /// <summary>
        ///     Current material scheme
        /// </summary>
        protected string activeSchemeName;
        /// <summary>
        ///     Current material scheme
        /// </summary>
        protected int activeSchemeIndex;

        #endregion Fields

        #region Properties

        /// <summary>
        ///    Sets the default anisotropy level to be used for loaded textures, for when textures are
        ///    loaded automatically (e.g. by Material class) or when 'Load' is called with the default
        ///    parameters by the application.
        /// </summary>
        public int DefaultAnisotropy {
            get {
                return defaultMaxAniso;
            }
            set {
                defaultMaxAniso = value;
            }
        }

        public int ActiveSchemeIndex {
            get {
                return activeSchemeIndex;
            }
        }

        public string ActiveScheme {
            get {
                return activeSchemeName;
            }
            set {
                int index;
                if (value != null && schemes.TryGetValue(value, out index)) {
                    activeSchemeName = value;
                    activeSchemeIndex = index;
                } else {
                    // Invalid scheme, use default
                    activeSchemeName = DefaultSchemeName;
                    activeSchemeIndex = 0;
                }
            }
        }

        public static string DefaultSchemeName {
            get {
                return "Default";
            }
        }
		
        public List<string> SchemeNames {
            get {
                List<string> names = new List<string>();
                foreach (string name in schemes.Keys)
                    names.Add(name);
                return names;
            }
        }
        
        #endregion Properties

        #region Methods

        /// <summary>
        ///     Sets up default materials and parses all material scripts.
        /// </summary>
        public void Initialize() {
            // Set up default material - don't use name contructor as we want to avoid applying defaults
            Material.defaultSettings = new Material();
            Material.defaultSettings.SetName("DefaultSettings");
            // Add a single technique and pass, non-programmable
            Material.defaultSettings.CreateTechnique().CreatePass();

            // just create the default BaseWhite material
            Material baseWhite = (Material)instance.Create("BaseWhite");
            baseWhite.Lighting = false;

            // parse all material scripts.
            // programs are parsed first since they may be referenced by materials
            ParseAllSources(".program");
            ParseAllSources(".material");

            // parse any materials defined as fx files
            FXReader.ParseAllSources(".fx");
            FXReader.ParseAllSources(".cgfx");

            // I don't think we have support for cgfx yet
            // FXReader.ParseAllSources(".cgfx");
        }

        /// <summary>
        ///     Sets the default texture filtering to be used for loaded textures, for when textures are
        ///     loaded automatically (e.g. by Material class) or when 'load' is called with the default
        ///     parameters by the application.
        /// </summary>
        /// <param name="options">Default options to use.</param>
        public virtual void SetDefaultTextureFiltering(TextureFiltering filtering) {
			this.filtering = filtering;
            switch (filtering) {
                case TextureFiltering.None:
                    SetDefaultTextureFiltering(FilterOptions.Point, FilterOptions.Point, FilterOptions.None);
                    break;
                case TextureFiltering.Bilinear:
                    SetDefaultTextureFiltering(FilterOptions.Linear, FilterOptions.Linear, FilterOptions.Point);
                    break;
                case TextureFiltering.Trilinear:
                    SetDefaultTextureFiltering(FilterOptions.Linear, FilterOptions.Linear, FilterOptions.Linear);
                    break;
                case TextureFiltering.Anisotropic:
                    SetDefaultTextureFiltering(FilterOptions.Anisotropic, FilterOptions.Anisotropic, FilterOptions.Linear);
                    break;
            }
        }
		public virtual TextureFiltering GetDefaultTextureFiltering() 
		{
			return filtering;
		}
    
        /// <summary>
        ///     Sets the default texture filtering to be used for loaded textures, for when textures are
        ///     loaded automatically (e.g. by Material class) or when 'load' is called with the default
        ///     parameters by the application.
        /// </summary>
        /// <param name="type">Type to configure.</param>
        /// <param name="options">Options to set for the specified type.</param>
        public virtual void SetDefaultTextureFiltering(FilterType type, FilterOptions options) {
            switch(type) {
                case FilterType.Min:
                    defaultMinFilter = options;
                    break;

                case FilterType.Mag:
                    defaultMagFilter = options;
                    break;

                case FilterType.Mip:
                    defaultMipFilter = options;
                    break;
            }
        }

        /// <summary>
        ///     Sets the default texture filtering to be used for loaded textures, for when textures are
        ///     loaded automatically (e.g. by Material class) or when 'load' is called with the default
        ///     parameters by the application.
        /// </summary>
        /// <param name="minFilter">Minification filter.</param>
        /// <param name="magFilter">Magnification filter.</param>
        /// <param name="mipFilter">Map filter.</param>
        public virtual void SetDefaultTextureFiltering(FilterOptions minFilter, FilterOptions magFilter, FilterOptions mipFilter) {
            defaultMinFilter = minFilter;
            defaultMagFilter = magFilter;
            defaultMipFilter = mipFilter;
        }

        /// <summary>
        ///     Gets the default texture filtering options for the specified filter type.
        /// </summary>
        /// <param name="type">Filter type to get options for.</param>
        /// <returns></returns>
        public virtual FilterOptions GetDefaultTextureFiltering(FilterType type) {
            switch(type) {
                case FilterType.Min:
                    return defaultMinFilter;

                case FilterType.Mag:
                    return defaultMagFilter;

                case FilterType.Mip:
                    return defaultMipFilter;
            }

            // make the compiler happy
            return FilterOptions.None;
        }

        /// <summary>
        ///		Look for material scripts in all known sources and parse them.
        /// </summary>
        /// <param name="extension">Extension to parse (i.e. ".material").</param>
        public void ParseAllSources(string extension) {
            Dictionary<string, Archive> filesToParse = new Dictionary<string, Archive>();

            // search archives
            for (int i = 0; i < archives.Count; i++) {
                Archive archive = (Archive)archives[i];
                string[] files = archive.GetFileNamesLike("", extension);
                for (int j = 0; j < files.Length; j++) {
                    if (!filesToParse.ContainsKey(files[j]))
                        filesToParse[files[j]] = archive;
                }
            }

            // search common archives
            for (int i = 0; i < commonArchives.Count; i++) {
                Archive archive = (Archive)commonArchives[i];
                string[] files = archive.GetFileNamesLike("", extension);
                for (int j = 0; j < files.Length; j++) {
                    if (!filesToParse.ContainsKey(files[j]))
                        filesToParse[files[j]] = archive;
                }
            }

            // sort the files
            List<string> fileList = new List<string>(filesToParse.Keys);
            fileList.Sort();

            // handle the scripts
            foreach (string file in fileList) {
                // parse the materials
                Stream data = filesToParse[file].ReadFile(file);
                serializer.ParseScript(data, file);
            }
        }

        /// <summary>
        ///		Internal method - returns index for a given material scheme name.
        ///    see Technique.SetSchemeName
        /// </summary>
        public int GetSchemeIndex(string schemeName) {
            int ret;
            System.Diagnostics.Debug.Assert(schemeName != null);
            System.Diagnostics.Debug.Assert(schemes != null);
            if (!schemes.TryGetValue(schemeName, out ret)) {
                ret = schemes.Count;
                schemes[schemeName] = ret;
            }
            return ret;
        }

        /// <summary>
        ///		Internal method - returns name for a given material scheme index.
        ///    see Technique.SetSchemeName
        /// </summary>
        public string GetSchemeName(int index) {
            foreach (KeyValuePair<string, int> pair in schemes) {
                if (pair.Value == index)
                    return pair.Key;
            }
            return DefaultSchemeName;
        }

        #endregion Methods

        #region ResourceManager Implementation

		/// <summary>
		///		Gets a material with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        public new Material GetByName(string name) {
            return (Material)base.GetByName(name);
        }

		
		/// <summary>
		///		Gets a material with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public new Material LoadExisting(string name) 
		{
			return (Material)base.LoadExisting(name);
		}

		public Material this[string name] 
		{
			get{return (Material)base.GetByName(name);}
		}
		
		public Material this[int handle] 
		{
			get{return (Material)base.GetByHandle(handle);}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override Resource Create(string name, bool isManual) {
            if(resourceList[name] != null)
                throw new AxiomException(string.Format("Cannot create a duplicate material named '{0}'.", name));

            // create a material
            Material material = new Material(name);

            Add(material);
				
            return material;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public Material Load(string name, int priority) {
            Material material = null;

            // if the resource isn't cached, create it
            if(!resourceList.ContainsKey(name)) {
                material = (Material)Create(name);
                base.Load(material, priority);
            }
            else {
                // get the cached version
                material = (Material)resourceList[name];
            }

            return material;
        }

        public Material Load(string name) {
            return Load(name, 0);
        }

        /// <summary>
        ///     Called when the engine is shutting down.
        /// </summary>
        public override void Dispose() {
            base.Dispose();

            if (instance == this) {
                instance = null;
            }
        }

        #endregion ResourceManager Implementation
    }
}
