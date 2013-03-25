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

namespace Microsoft.MultiverseInterfaceStudio.Services
{
    [Guid("9C7E3398-6BAC-4dcd-AB37-EF4817B309AD")]
    public interface IPythonLanguageService
    {
        /// <summary>
        /// Adds a FrameXML file to the list of files to be parsed.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        void AddFrameXmlFile(string path);

        /// <summary>
        /// Removes a FrameXML file from the list of files to be parsed.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        void RemoveFrameXmlFile(string path);

        /// <summary>
        /// Adds a Lua file to the list of files to be parsed.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        void AddPythonFile(string path);

        /// <summary>
        /// Removes a Lua file from the list of files to be parsed.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        void RemovePythonFile(string path);

        /// <summary>
        /// Clears all files from the list of files to be parsed.
        /// </summary>
        void Clear();
    }
}
