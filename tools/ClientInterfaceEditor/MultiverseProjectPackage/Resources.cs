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

/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Reflection;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace Microsoft.MultiverseInterfaceStudio
{
    /// <summary>
    /// This class represent resource storage and management functionality.
    /// </summary>
    internal sealed class Resources
    {
        public const string FormatList = "FormatList";
        public const string GeneralCaption = "GeneralCaption";
        public const string FailedToLoadTemplateFileMessage = "Failed to add template file to project";

        public const string MultiverseInterface = "MultiverseInterface";

        private static Resources loader;
        private static object syncObject;

        private ResourceManager resourceManager;

        /// <summary>
        /// public explicitly defined default constructor.
        /// </summary>
        public Resources()
		{
            resourceManager = new ResourceManager("Microsoft.MultiverseInterface.Resources", Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Gets the public synchronization object.
        /// </summary>
        private static Object SyncObject
        {
            get
            {
                if (syncObject == null)
                {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref syncObject, o, null);
                }

                return syncObject;
            }
        }
        /// <summary>
        /// Gets information about a specific culture.
        /// </summary>
        private static CultureInfo Culture
        {
            get { return CultureInfo.CurrentUICulture; }
        }

        /// <summary>
        /// Gets convenient access to culture-specific resources at runtime.
        /// </summary>
        public static ResourceManager ResourceManager
        {
            get
            {
                return GetLoader().resourceManager;
            }
        }

        /// <summary>
        /// Provide access to the public SR loader object.
        /// </summary>
        /// <returns>Instance of the Resources object.</returns>
        private static Resources GetLoader()
		{
            if (loader == null)
			{
                lock (SyncObject)
				{
                   if (loader == null)
				   {
                       loader = new Resources();
                   }
               }
            }

            return loader;
        }
        /// <summary>
        /// Provide access to resource string value.
        /// </summary>
        /// <param name="name">Received string name.</param>
        /// <param name="args">Arguments for the String.Format method.</param>
        /// <returns>Returns resources string value or null if error occured.</returns>
        public static string GetString(string name, params object[] args)
        {
            Resources resourcesInstance = GetLoader();

            if (resourcesInstance == null)
                return null;

            string res = resourcesInstance.resourceManager.GetString(name, Resources.Culture);

            if (args != null && args.Length > 0)
            {
                return String.Format(CultureInfo.CurrentCulture, res, args);
            }
            else
            {
                return res;
            }
        }
        /// <summary>
        /// Provide access to resource string value.
        /// </summary>
        /// <param name="name">Received string name.</param>
        /// <returns>Returns resources string value or null if error occured.</returns>
        public static string GetString(string name)
        {
            Resources resourcesInstance = GetLoader();

            if (resourcesInstance == null)
                return null;

            return resourcesInstance.resourceManager.GetString(name, Resources.Culture);
        }

        /// <summary>
        /// Provide access to resource object value.
        /// </summary>
        /// <param name="name">Received object name.</param>
        /// <returns>Returns resources object value or null if error occured.</returns>
        public static object GetObject(string name)
        {
            Resources resourcesInstance = GetLoader();

            if (resourcesInstance == null)
                return null;

            return resourcesInstance.resourceManager.GetObject(name, Resources.Culture);
        }
    }
}
