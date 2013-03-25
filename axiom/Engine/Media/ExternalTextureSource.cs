using System;
using System.Collections.Generic;
using System.Text;

/***************************************************************************
 ExternalTextureSource.cs  -  
	Base class that texture plugins need to derive from. This provides the hooks
	neccessary for a plugin developer to easily extend the functionality of dynamic textures.
	It makes creation/destruction of dynamic textures more streamlined. While the plugin
	will need to talk with Ogre for the actual modification of textures, this class allows
	easy integration with Ogre apps. Material script files can be used to aid in the 
	creation of dynamic textures. Functionality can be added that is not defined here
	through the use of the base dictionary. For an exmaple of how to use this class and the
	string interface see ffmpegVideoPlugIn.

-------------------
date                 : Jan 1 2004
email                : pjcast@yahoo.com
***************************************************************************/

namespace Axiom.Media
{
    public abstract class ExternalTextureSource
    {
	    /// <summary>
        /// Enum for type of texture play mode
	    /// </summary>
 	    public enum TexturePlayMode
	    {
		    TextureEffectPause = 0,			//! Video starts out paused
		    TextureEffectPlay_ASAP = 1,		//! Video starts playing as soon as posible
		    TextureEffectPlay_Looping = 2	//! Video Plays Instantly && Loops
	    };

	    /** IMPORTANT: **Plugins must override default dictionary name!** 
	    Base class that texture plugins derive from. Any specific 
	    requirements that the plugin needs to have defined before 
	    texture/material creation must be define using the stringinterface
	    before calling create defined texture... or it will fail, though, it 
	    is up to the plugin to report errors to the log file, or raise an 
	    exception if need be. */

        /// <summary>
        /// Constructor
        /// </summary>
        public ExternalTextureSource()
        {
            SetTextureTecPassStateLevel(0, 0, 0);
        }

		/// <summary>
		/// Used for attaching texture to Technique, State, and texture unit layer
		/// </summary>
        private int TechniqueLevel, PassLevel, StateLevel;
		public void GetTextureTecPassStateLevel( out int t, out int p, out int s )
				{ t = TechniqueLevel;	p = PassLevel;	s = StateLevel; }
        public void SetTextureTecPassStateLevel(int t, int p, int s)
                { TechniqueLevel = t; PassLevel = p; StateLevel = s; }

		/// <summary>
		/// Returns the string name of this PlugIn (as set by the PlugIn)
		/// </summary>
        protected string pluginName;
        public string PluginName { get { return pluginName; } }

		//Pure virtual functions that plugins must Override

		/// <summary>
        /// Call this function from manager to init system
		/// </summary>
		/// <returns>True if the system initialized correctly, false if not.</returns>
		public abstract bool Initialize();

		/// <summary>
        /// Shuts down Plugin
		/// </summary>
        public abstract void Shutdown();

		/// <summary>
		/// Creates a texture into an already defined material or one that is created new
		/// (it's up to plugin to use a material or create one)
		/// Before calling, ensure that needed params have been defined via the stringInterface
		/// or regular methods
		/// </summary>
        public abstract void CreateDefinedTexture(string materialName);

		/// <summary>
		/// What this destroys is dependent on the plugin... See specific plugin
		/// doc to know what is all destroyed (normally, plugins will destroy only
		/// what they created, or used directly - ie. just texture unit)
		/// </summary>
        public abstract void DestroyAdvancedTexture(string textureName);

        /// <summary>
        /// Set the parameters of this object via string values and names
        /// </summary>
        /// <param name="parameter">The parameter to change</param>
        /// <param name="value">The new value of the parameter</param>
        public abstract void SetParameter(string parameter, string value);
    }
}
