using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using log4net;

namespace Axiom.Media
{
    public class ExternalTextureSourceManager
    {
        #region Singleton implementation
        private static ExternalTextureSourceManager instance = new ExternalTextureSourceManager();
        public static ExternalTextureSourceManager Instance { get { return instance; } }
        internal ExternalTextureSourceManager()
        {
            CurrExternalTextureSource = null;
        }
        ~ExternalTextureSourceManager()
        {
            TextureSystems.Clear();
        }
        #endregion

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ExternalTextureSourceManager));

        /// <summary>
        /// Member variables
        /// </summary>
        private ExternalTextureSource CurrExternalTextureSource;
        protected IDictionary TextureSystems = new Hashtable();

        /// <summary>
        /// The currently active texture source plugin.
        /// </summary>
        public ExternalTextureSource CurrentPlugin { get { return CurrExternalTextureSource; } }

        /// <summary>
        /// Sets the currently active external texture source by name.
        /// </summary>
        /// <param name="texturePluginType">
        /// The string name of the plugin to activate.
        /// </param>
        public void SetCurrentPlugin(string texturePluginType)
        {
            IDictionaryEnumerator i = (IDictionaryEnumerator) TextureSystems.GetEnumerator();
		    while(i.MoveNext())
		    {
                if((i.Key as string) == texturePluginType)
                {
                    CurrExternalTextureSource = (ExternalTextureSource)i.Value;
                    CurrExternalTextureSource.Initialize();
                    return;
                }
		    }
		    CurrExternalTextureSource = null;
		    log.ErrorFormat("ExternalTextureSourceManager.SetCurrentPlugin({0}) did not find plugin", 
                            texturePluginType);
        }
	
		/// <summary>
		/// Calls the destroy method of all registered plugins... 
		/// Only the owner plugin should perform the destroy action.
		/// </summary>
        protected void DestroyAdvancedTexture(string textureName)
        {
            IDictionaryEnumerator i = (IDictionaryEnumerator) TextureSystems.GetEnumerator();
		    while(i.MoveNext())
		    {
                //Broadcast to every registered System... Only the true one will destroy texture
                (i.Value as ExternalTextureSource).DestroyAdvancedTexture(textureName);
            }
        }

		/// <summary>
		/// Returns the plugin which registered itself with a specific name 
		/// (eg. "video"), or null if specified plugin not found
		/// </summary>
        public ExternalTextureSource ExternalTextureSource(string texturePluginType)
        {
            IDictionaryEnumerator i = (IDictionaryEnumerator)TextureSystems.GetEnumerator();
            while (i.MoveNext())
            {
                if ((i.Key as string) == texturePluginType)
                {
                    return (i.Value as ExternalTextureSource);
                }
            }
            return null;
        }

		/// <summary>
		/// Called from plugin to register itself
		/// </summary>
		/// <param name="texturePluginType">The string name of the plugin</param>
		/// <param name="textureSystem">The actual texture system</param>
        public void SetExternalTextureSource(string texturePluginType, ExternalTextureSource textureSystem)
        {
            log.InfoFormat("Registering Texture Controller: Type = {0}, Name = {1}",
                           texturePluginType, textureSystem.PluginName);

            IDictionaryEnumerator i = (IDictionaryEnumerator)TextureSystems.GetEnumerator();
		    while(i.MoveNext())
		    {
                if((i.Key as string) == texturePluginType)
                {
                    ExternalTextureSource ets = (i.Value as ExternalTextureSource);
                    log.InfoFormat("Shutting Down Texture Controller: {0} to be replaced by: {1}",
                                   ets.PluginName, textureSystem.PluginName);
                    ets.Shutdown();
                    break;
                }
            }
            // Add it to the map, or replace the existing entry
		    TextureSystems[texturePluginType] = textureSystem;
        }
    }
}
