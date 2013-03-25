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
using System.Windows.Forms;
using System.Collections;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	/// <summary>
	/// Enumerates the controls in the root control passed
	/// </summary>
	public class BaseControlCollection : IEnumerable<BaseControl>
	{
		private Ui rootControl = null;

		public BaseControlCollection(Ui rootControl)
		{
			this.rootControl = rootControl;
		}

		/// <summary>
		/// Gets the elements. Recursive iterator logic.
		/// </summary>
		/// <param name="control">The control.</param>
		/// <returns></returns>
		private IEnumerable<BaseControl> GetElements(Control control)
		{
			if (control != null)
			{
				foreach (var baseControl in control.Controls.OfType<BaseControl>())
				{
					yield return baseControl;
					var childControls = GetElements(baseControl);
					foreach (var childControl in childControls)
						yield return childControl;
				}
			}
		}

		/// <summary>
		/// Gets the <see cref="Microsoft.MultiverseInterfaceStudio.FrameXml.Controls.BaseControl"/> with the specified name.
		/// </summary>
		/// <value></value>
		public BaseControl this[string name]
		{
			get
			{
				if (name == null)
					return null;

				var controls = from control in this
							   where control.Site.Name == name
							   select control;

				return controls.FirstOrDefault();
			}
		}

		public BaseControl this[string name, Control parent]
		{
			get
			{
				var parentName = parent != null && parent.Site != null ?
					parent.Site.Name : null;

				name = LayoutFrameType.GetExpandedName(name, parentName);

				return this[name];
			}
		}

		#region IEnumerable<BaseControl> Members

		public IEnumerator<BaseControl> GetEnumerator()
		{
			return GetElements(rootControl).GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
