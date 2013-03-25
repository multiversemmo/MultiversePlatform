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

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	public static class ControlSorter
	{
		public static IEnumerable<TControl> SortControls<TControl>(this IEnumerable<TControl> controls)
			where TControl : ISerializableControl
		{
			return controls.OfType<BaseControl>().SortControls().OfType<TControl>();
		}

		public static IEnumerable<BaseControl> SortControls(this IEnumerable<BaseControl> controls)
		{
			// holds the list of controls dependent on the key
			Dictionary<BaseControl, IEnumerable<BaseControl>> controlDependencies = new Dictionary<BaseControl, IEnumerable<BaseControl>>();

			// init dictionary 
			foreach (BaseControl control in controls)
			{
				controlDependencies.Add(control, control.GetDependencies());
			}

			List<BaseControl> orderedControls = new List<BaseControl>();

			bool hasProcessedItems;
			do
			{
				var dependentOnPrevious =
					from dependency in controlDependencies
					where dependency.Value.All<BaseControl>(control => orderedControls.Contains(control))
					select dependency.Key;

				foreach (var control in dependentOnPrevious)
					yield return control;

				// maintain ordered and remained controls
				var addedControls = dependentOnPrevious.ToArray<BaseControl>();

				foreach (var addedControl in addedControls)
					controlDependencies.Remove(addedControl);

				orderedControls.AddRange(addedControls);

				hasProcessedItems = addedControls.Length > 0;

			}
			while (hasProcessedItems);

			// add remaining controls (probably containig circular references)
			foreach (BaseControl control in controlDependencies.Keys)
				yield return control;
		}

		public static IEnumerable<BaseControl> GetDependencies(this BaseControl control)
		{
			var deps =
				from anchor in control.Anchors
				where !String.IsNullOrEmpty(anchor.relativeTo)
				select control.DesignerLoader.BaseControls[anchor.relativeTo, control.Parent];

			return
				deps.OfType<BaseControl>();
		}
	}
}
