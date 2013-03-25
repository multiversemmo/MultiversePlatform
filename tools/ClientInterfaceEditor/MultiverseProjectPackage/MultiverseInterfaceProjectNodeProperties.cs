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

using System.ComponentModel;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Package;

namespace Microsoft.MultiverseInterfaceStudio
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid(GuidStrings.MultiverseInterfaceProjectNodeProperties)]
    public class MultiverseInterfaceProjectNodeProperties : ProjectNodeProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiverseInterfaceProjectNodeProperties"/> class.
        /// </summary>
        /// <param name="projectNode">The project node.</param>
        public MultiverseInterfaceProjectNodeProperties(MultiverseInterfaceProjectNode projectNode)
            : base(projectNode)
        {
        }

        /// <summary>
        /// Gets or sets the path to the World of Warcraft installation.
        /// </summary>
        [Browsable(false)]
        public string MultiverseInterfacePath
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.MultiversePath.ToString());
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.MultiversePath.ToString(), value);
            }
        }
    }
}
