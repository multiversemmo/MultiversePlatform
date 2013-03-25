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

namespace Microsoft.MultiverseInterfaceStudio
{
    internal static class Guids
    {
        public static readonly Guid MultiverseInterfaceProjectPackage = new Guid(GuidStrings.MultiverseInterfaceProjectPackage);
        public static readonly Guid MultiverseInterfaceProjectCmdSet = new Guid(GuidStrings.MultiverseInterfaceProjectCmdSet);
        public static readonly Guid MultiverseInterfaceProjectFactory = new Guid(GuidStrings.MultiverseInterfaceProjectFactory);
        public static readonly Guid MultiverseInterfaceProjectNode = new Guid(GuidStrings.MultiverseInterfaceProjectNode);
        public static readonly Guid MultiverseInterfaceProjectNodeProperties = new Guid(GuidStrings.MultiverseInterfaceProjectNodeProperties);

        public static readonly Guid MultiverseInterfacePythonFileNode = new Guid(GuidStrings.MultiverseInterfacePythonFileNode);
        public static readonly Guid MultiverseInterfaceTocFileNode = new Guid(GuidStrings.MultiverseInterfaceTocFileNode);
        public static readonly Guid MultiverseInterfaceXmlFileNode = new Guid(GuidStrings.MultiverseInterfaceXmlFileNode);

        public static readonly Guid GeneralPropertyPage = new Guid(GuidStrings.GeneralPropertyPage);

        public static readonly Guid FrameXmlDesignerOptionPage = new Guid(GuidStrings.FrameXmlDesignerOptionPage);
    };
}
