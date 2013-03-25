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
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Package;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.MultiverseInterfaceStudio
{
    /// <summary>
    /// Project node factory for WoW projects.
    /// </summary>
    [Guid(GuidStrings.MultiverseInterfaceProjectFactory)]
    public class MultiverseInterfaceProjectFactory : ProjectFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiverseInterfaceProjectFactory"/> class.
        /// </summary>
        /// <param name="package">The package this project factory belongs to.</param>
        public MultiverseInterfaceProjectFactory(MultiverseInterfaceProjectPackage package)
            : base(package)
        {
        }

        /// <summary>
        /// Creates a WoW project node.
        /// </summary>
        /// <returns>An instance of the <see cref="MultiverseInterfaceProjectNode"/> class.</returns>
        protected override ProjectNode CreateProject()
        {
            // Create the project node instance
            MultiverseInterfaceProjectNode projectNode = new MultiverseInterfaceProjectNode(this.Package as MultiverseInterfaceProjectPackage);

            // Site the project node using the package's service provider
            projectNode.SetSite((IOleServiceProvider)((IServiceProvider)this.Package).GetService(typeof(IOleServiceProvider)));

            // Return the project node
            return projectNode;
        }
    }
}
