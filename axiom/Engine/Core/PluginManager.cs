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
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Axiom.Core
{
	/// <summary>
	/// Summary description for PluginManager.
	/// </summary>
	public class PluginManager : IDisposable
	{
		#region Singleton implementation

		/// <summary>
		///     Singleton instance of this class.
		/// </summary>
		private static PluginManager instance;

		/// <summary>
		///     Internal constructor.  This class cannot be instantiated externally.
		/// </summary>
		internal PluginManager()
		{
			if (instance == null)
			{
				instance = this;
			}
		}

		/// <summary>
		///     Gets the singleton instance of this class.
		/// </summary>
		public static PluginManager Instance
		{
			get { return instance; }
		}

		#endregion Singleton implementation

		#region Fields

		/// <summary>
		///		List of loaded plugins.
		/// </summary>
		private ArrayList plugins = new ArrayList();

		#endregion Fields

		#region Methods
		
        /// <summary>
		///		Loads all plugins specified in the plugins section of the app.config file.
		/// </summary>
		public void LoadAll()
		{
			// TODO: Make optional, using scanning again in the meantim
			// trigger load of the plugins app.config section
			//ArrayList newPlugins = (ArrayList)ConfigurationSettings.GetConfig("plugins");
            ArrayList newPlugins = ScanForPlugins(".", "Axiom.*.dll");
			foreach (IPlugin plugin in newPlugins)
			{
				LoadPlugin(plugin);
			}
        }

        /// <summary>
        /// Scans for plugin files in the given directory and loads them.
        /// </summary>
        public void LoadPlugins(string directory, string wildcard)
        {
            ArrayList newPlugins = ScanForPlugins(directory, wildcard);
            foreach (IPlugin plugin in newPlugins)
            {
                LoadPlugin(plugin);
            }
        }

		/// <summary>
		///		Scans for plugin files in the current directory.
		/// </summary>
		/// <returns></returns>
		protected ArrayList ScanForPlugins(string directory, string wildcard)
		{
			ArrayList ans = new ArrayList();

			string[] files = Directory.GetFiles(directory, wildcard);

			foreach (string file in files)
			{
				// TODO: Temp fix, allow exlusions in the app.config
				if (file != "Axiom.Engine.dll")
				{
					try
					{
						Assembly assembly = Assembly.LoadFrom(file);

						foreach (Type type in assembly.GetTypes())
						{
							//there may be other interfaces named IPlugin used for other assemblies, so check the full type
							if ((type.GetInterface("IPlugin") == typeof (IPlugin)) && (!type.IsInterface))
							{
								try
								{
									IPlugin plugin = (IPlugin) Activator.CreateInstance(type);
									if (plugin != null)
										ans.Add(plugin);
									else
										LogManager.Instance.Write("Failed to create instance of plugin of type {0}.", type);
								}
								catch (Exception e)
								{
									LogManager.Instance.WriteException("Failed to create instance of plugin of type {0} from assembly {1}", type, assembly.FullName);
									LogManager.Instance.WriteException(e.Message);
								}
							}
						}
					}
					catch (BadImageFormatException)
					{
						// ignore native assemblies which will throw this exception when loaded
					}
				}
			}

			return ans;
		}

        /// <summary>
        /// Registers all loaded plugins with scripting once the interpreter is loaded.
        /// </summary>
        public ArrayList GetAssemblies()
        {
            ArrayList ans = new ArrayList();
            foreach (object plugin in plugins)
            {
                Assembly pluginAssembly = Assembly.GetAssembly(plugin.GetType());
                if (!ans.Contains(pluginAssembly))
                {
                    ans.Add(pluginAssembly);
                }
            }
            return ans;
        }

		/// <summary>
		///		Unloads all currently loaded plugins.
		/// </summary>
		public void UnloadAll()
		{
			// loop through and stop all loaded plugins
			for (int i = 0; i < plugins.Count; i++)
			{
				IPlugin plugin = (IPlugin) plugins[i];

				LogManager.Instance.Write("Unloading plugin {0} from {1}", plugin, GetAssemblyTitle(plugin));

				plugin.Stop();
			}

			// clear the plugin list
			plugins.Clear();
		}

		public static string GetAssemblyTitle(object instance)
		{
			return GetAssemblyTitle(instance.GetType());
		}

		public static string GetAssemblyTitle(Type type)
		{
			Assembly assembly = type.Assembly;
			AssemblyTitleAttribute title = (AssemblyTitleAttribute) Attribute.GetCustomAttribute(
			                               	(Assembly) assembly, typeof (AssemblyTitleAttribute));
			if (title == null)
				return assembly.GetName().Name;
			return title.Title;
		}

		public static string GetAssemblyName(object instance)
		{
			return GetAssemblyName(instance.GetType());
		}

		public static string GetAssemblyName(Type type)
		{
			return type.Assembly.GetName().Name;
		}


		/// <summary>
		///		Loads a plugin of the given class name from the given assembly, and calls Start() on it.
		///		This function does NOT add the plugin to the PluginManager's
		///		list of plugins.
		/// </summary>
		/// <param name="assemblyName">The assembly filename ("xxx.dll")</param>
		/// <param name="className">The class ("MyNamespace.PluginClassname") that implemented IPlugin.</param>
		/// <returns>The loaded plugin.</returns>
		private bool LoadPlugin(IPlugin plugin)
		{
			try
			{
				plugin.Start();

				LogManager.Instance.Write("Loaded plugin {0} from {1}", plugin, GetAssemblyTitle(plugin));
                plugins.Add(plugin);
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Instance.WriteException(ex.ToString());
				return false;
			}
		}

		#endregion Methods

		#region IDisposable Implementation

		public void Dispose()
		{
			if (instance != null)
			{
				instance = null;

				UnloadAll();
			}
		}

		#endregion IDiposable Implementation
	}

	/// <summary>
	/// The plugin configuration handler
	/// </summary>
	public class PluginConfigurationSectionHandler : IConfigurationSectionHandler
	{
		public object Create(object parent, object configContext, XmlNode section)
		{
			ArrayList plugins = new ArrayList();

			// grab the plugin nodes
			XmlNodeList pluginNodes = section.SelectNodes("plugin");

			// loop through each plugin node and load the plugins
			for (int i = 0; i < pluginNodes.Count; i++)
			{
				XmlNode pluginNode = pluginNodes[i];

				// grab the attributes for loading these plugins
				XmlAttribute assemblyAttribute = pluginNode.Attributes["assembly"];
				XmlAttribute classAttribute = pluginNode.Attributes["class"];

				plugins.Add(new ObjectCreator(assemblyAttribute.Value, classAttribute.Value).CreateInstance());
			}

			return plugins;
		}
	}

}