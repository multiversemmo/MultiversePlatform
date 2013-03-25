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

using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Package.Automation;

using EnvDTE;

namespace Microsoft.MultiverseInterfaceStudio
{
    /// <summary>
    /// World of Warcraft project automation object.
    /// </summary>
    [ComVisible(true)]
    public class OAWowProject : OAProject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAWowProject"/> class.
        /// </summary>
        /// <param name="projectNode">The project node.</param>
        internal OAWowProject(MultiverseInterfaceProjectNode projectNode)
            : base(projectNode)
        {
        }

        /// <summary>
        /// Gets a collection of all properties that pertain to the Project object.
        /// </summary>
        /// <value></value>
        public override Properties Properties
        {
            get
            {
                return new OAWowProjectProperties(this.Project.NodeProperties);
            }
        }
    }
}
