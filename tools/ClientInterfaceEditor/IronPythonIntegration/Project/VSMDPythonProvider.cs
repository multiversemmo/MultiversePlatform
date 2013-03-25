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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using Microsoft.VisualStudio.Designer.Interfaces;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.Samples.VisualStudio.IronPythonProject
{
	internal class VSMDPythonProvider : IVSMDCodeDomProvider, IDisposable
	{
		private IronPython.CodeDom.PythonProvider provider;
		private VSLangProj.VSProject vsproject;

		public VSMDPythonProvider(VSLangProj.VSProject project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			vsproject = project;

			// Create the provider
			this.ReferencesEvents_ReferenceRemoved(null);
			vsproject.Events.ReferencesEvents.ReferenceAdded += new VSLangProj._dispReferencesEvents_ReferenceAddedEventHandler(ReferencesEvents_ReferenceAdded);
			vsproject.Events.ReferencesEvents.ReferenceRemoved += new VSLangProj._dispReferencesEvents_ReferenceRemovedEventHandler(ReferencesEvents_ReferenceRemoved);
			vsproject.Events.ReferencesEvents.ReferenceChanged += new VSLangProj._dispReferencesEvents_ReferenceChangedEventHandler(ReferencesEvents_ReferenceRemoved);
		}

		#region Event Handlers
		/// <summary>
		/// When a reference is added, add it to the provider
		/// </summary>
		/// <param name="reference">Reference being added</param>
		void ReferencesEvents_ReferenceAdded(VSLangProj.Reference reference)
		{
			provider.AddReference(reference.Path);
		}

		/// <summary>
		/// When a reference is removed/changed, let the provider know
		/// </summary>
		/// <param name="reference">Reference being removed</param>
		void ReferencesEvents_ReferenceRemoved(VSLangProj.Reference reference)
		{
			// Because our provider only has an AddReference method and no way to
			// remove them, we end up having to recreate it.
			provider = new IronPython.CodeDom.PythonProvider();
			if (vsproject.References != null)
			{
				foreach (VSLangProj.Reference currentReference in vsproject.References)
				{
					provider.AddReference(currentReference.Path);
				}
			}
		}
		#endregion

		#region IVSMDCodeDomProvider Members
		object IVSMDCodeDomProvider.CodeDomProvider
		{
			get { return provider; }
		}
		#endregion

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			if (vsproject != null)
			{
				vsproject = null;
				vsproject.Events.ReferencesEvents.ReferenceAdded -= new VSLangProj._dispReferencesEvents_ReferenceAddedEventHandler(ReferencesEvents_ReferenceAdded);
				vsproject.Events.ReferencesEvents.ReferenceRemoved -= new VSLangProj._dispReferencesEvents_ReferenceRemovedEventHandler(ReferencesEvents_ReferenceRemoved);
				vsproject.Events.ReferencesEvents.ReferenceChanged -= new VSLangProj._dispReferencesEvents_ReferenceChangedEventHandler(ReferencesEvents_ReferenceRemoved);
			}
		}

		#endregion
	}
}
