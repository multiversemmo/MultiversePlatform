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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml
{
	/// <summary>
	/// Monitors selection change events sent by the Visual Studio
	/// </summary>
	public class SelectionEventsMonitor : IVsSelectionEvents
	{
		#region construction - singleton

		private SelectionEventsMonitor() { }

		private static SelectionEventsMonitor instance = null;

		public static SelectionEventsMonitor Instance
		{
			get
			{
				if (instance == null)
					instance = new SelectionEventsMonitor();
				return instance;
			}
		}

		#endregion

		#region IVsSelectionEvents Members

		public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
		{
			return VSConstants.S_OK;
		}

		public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
		{
			if (elementid == (uint)VSConstants.VSSELELEMID.SEID_WindowFrame)
			{
				IVsWindowFrame newFrame = (IVsWindowFrame)varValueNew;
				object value;
				int result = newFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out value);
				if (result != null && result == (int)VSConstants.S_OK)
				{
					Trace.WriteLine(String.Format(">>> Active Designer: {0}", value));
					FrameXmlDesignerLoader.ActiveItemID = Convert.ToUInt32(value);
				}
			}
			return VSConstants.S_OK;
		}

		public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
		{
			return VSConstants.S_OK;
		}

		#endregion
	}
}
