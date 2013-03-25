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

#region license

/*
DirectShowLib - Provide access to DirectShow interfaces via .NET
Copyright (C) 2006
http://sourceforge.net/projects/directshownet/

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#endregion

using System;
using System.Runtime.InteropServices;

namespace DirectShowLib
{
    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

    /// <summary>
    /// From KSTOPOLOGY_CONNECTION
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct KSTopologyConnection
    {
        public int FromNode;
        public int FromNodePin;
        public int ToNode;
        public int ToNodePin;
    }

#endif

    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [ComImport,
    Guid("720D4AC0-7533-11D0-A5D6-28DB04C10000"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IKsTopologyInfo
    {
        [PreserveSig]
        int get_NumCategories(
            [Out] out int pdwNumCategories
            );

        [PreserveSig]
        int get_Category(
            [In] int dwIndex,
            [Out] out Guid pCategory
            );

        [PreserveSig]
        int get_NumConnections(
            [Out] out int pdwNumConnections
            );

        [PreserveSig]
        int get_ConnectionInfo(
            [In] int dwIndex,
            [Out] out KSTopologyConnection pConnectionInfo
            );

        [PreserveSig]
        int get_NodeName(
            [In] int dwNodeId,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string pwchNodeName,
            [In] int dwBufSize,
            [Out] out int pdwNameLen
            );

        [PreserveSig]
        int get_NumNodes(
            [Out] out int pdwNumNodes
            );

        [PreserveSig]
        int get_NodeType(
            [In] int dwNodeId,
            [Out] out Guid pNodeType
            );

        [PreserveSig]
        int CreateNodeInstance( 
            [In] int dwNodeId, 
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid iid, 
            [Out, MarshalAs(UnmanagedType.IUnknown)] out Object ppvObject  
            ); 
    }

    [ComImport,
    Guid("1ABDAECA-68B6-4F83-9371-B413907C7B9F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISelector
    {
        [PreserveSig]
        int get_NumSources([Out] out int pdwNumSources);

        [PreserveSig]
        int get_SourceNodeId([Out] out int pdwPinId);

        [PreserveSig]
        int put_SourceNodeId([In] int dwPinId);
    }

    [ComImport,
    Guid("11737C14-24A7-4bb5-81A0-0D003813B0C4"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IKsNodeControl
    {
        [PreserveSig]
        int put_NodeId([In] int dwNodeId);

        [PreserveSig]
        int put_KsControl([In] IntPtr pKsControl); // PVOID
    }

#endif

    #endregion
}
