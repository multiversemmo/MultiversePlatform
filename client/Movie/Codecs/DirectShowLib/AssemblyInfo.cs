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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;


[assembly : AssemblyTitle("DirectShow Net Library")]
[assembly : AssemblyDescription(".NET Interfaces for calling DirectShow.  See http://directshownet.sourceforge.net/")]
[assembly : AssemblyConfiguration("")]
[assembly : AssemblyCompany("")]
[assembly : Guid("6D0386CE-37E6-4f77-B678-07C584105DC6")]
[assembly : AssemblyVersion("1.5.0.*")]
#if DEBUG
[assembly : AssemblyProduct("Debug Version")]
#else
[assembly : AssemblyProduct("Release Version")]
#endif
[assembly : AssemblyCopyright("GNU Lesser General Public License v2.1")]
[assembly : AssemblyTrademark("")]
[assembly : AssemblyCulture("")]
[assembly : AssemblyDelaySign(false)]
// Path is relative to the resulting executable (\Bin\Debug)
#if USING_NET11
[assembly : AssemblyKeyFile("..\\..\\DShowNET.snk")]
#endif
[assembly : AssemblyKeyName("")]
[assembly : ComVisible(false)]
[assembly : CLSCompliant(true)]
[assembly : SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode=true)]
