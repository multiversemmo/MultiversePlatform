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
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Axiom.Core {
	/// <summary>
	///		Class which manages the platform settings required to run.
	/// </summary>
	public sealed class PlatformManager {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static IPlatformManager instance;

		/// <summary>
		/// Gets if the operating system that this is running on is Windows (as opposed to a Unix-based one such as Linux or Mac OS X)
		/// </summary>
		/// <remarks>
		/// The Windows version strings start with "Microsoft Windows" followed by CE, NT, or 98 and the version number,
		/// however Microsoft Win32S is used with the 32-bit simulation layer on 16-bit systems so we should just check for the presence of Microsoft
		/// Unix-based operating systems start with Unix
		/// The Environment.OSVersion.Platform is 128 for Unix-based platforms (an additional enum value added that by the name Unix),
		/// however under .NET 2.0 Unix is supposed to be 3 but may still be 128 under Mono
		/// Additionally, GNU Portable .NET likely doesn't provide this same value, so just check for the presence of Windows in the string name
		/// </remarks>
		public static bool WindowsOS {
			get {
				//return ((int)Environment.OSVersion.Platform) == 128;	//if is a unix-based operating system (running Mono), not sure if this will work for GNU Portable .NET
				string os = Environment.OSVersion.ToString();
				return os.IndexOf("Microsoft") != -1;
			}
		}

		/// <summary>
		/// Intelligently selects the best platform from the list of possible platform manager assembly file paths
		/// </summary>
		/// <param name="files"></param>
		/// <remarks>
		
		/// </remarks>
		/// <returns></returns>
		private static Assembly GetPlatformManagerAssembly() 
        {
            Assembly entryAssy = Assembly.GetEntryAssembly();

            FileInfo assyInfo = new FileInfo( Assembly.GetEntryAssembly().Location );

            FileInfo[] platformInfos = assyInfo.Directory.GetFiles( "Axiom.Platforms.*.dll" );

            if( platformInfos.Length == 0 ) 
            {
				throw new PluginException( String.Format(
                    "No PlatformManager found in '{0}'; an assembly named 'Axiom.Platforms.*.dll' is required",
                    assyInfo.DirectoryName ) );
			}

            // This is a little dodgy on non-Win32 platforms: we just take the first
            // platform assembly found, which may or may not be suitable.
            // TODO: When we support non-Win32, make this robutst!
            string platformAssemblyName = platformInfos[ 0 ].FullName;

            if( WindowsOS ) 
            {
                foreach( FileInfo info in platformInfos ) 
                {
                    if( -1 != info.Name.IndexOf( "Win32" ) ) 
                    {
                        platformAssemblyName = info.FullName;
                    }
                }
			}

            log.InfoFormat( "Loading PlatformManager '{0}'", platformAssemblyName );

            return Assembly.LoadFrom( platformAssemblyName );
		}

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        internal PlatformManager() {
           
        }

        internal static void LoadInstance() 
		{
			if (instance == null) 
			{
                Assembly platformManagerAssembly = GetPlatformManagerAssembly();

				// look for the type in the loaded assembly that implements IPlatformManager
				foreach (Type type in platformManagerAssembly.GetTypes()) 
				{
					if (type.GetInterface("IPlatformManager") != null) 
					{
						instance = (IPlatformManager)platformManagerAssembly.CreateInstance(type.FullName);
						return;
					}
				}

				throw new PluginException( String.Format(
                    "The PlatformManager assembly '{0}' does not implement the required 'IPlatformManager' interface",
                    platformManagerAssembly.FullName ) );
			}
		}

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger( typeof( PlatformManager ) );

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static IPlatformManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation
	}
}
