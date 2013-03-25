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
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml
{
	public static class SerializationObjectSorter
	{
		public static IEnumerable<SerializationObject> SortRootObjects(this IEnumerable<SerializationObject> objects)
		{
			var nonLayoutFrames = from o in objects
								  where !(o is LayoutFrameType)
								  select o;

			foreach (SerializationObject o in nonLayoutFrames)
				yield return o;

			var virtuals = from layoutFrame in objects.OfType<LayoutFrameType>().SortLayoutFrames()
						   where layoutFrame.@virtual
						   select layoutFrame;

			foreach (SerializationObject o in virtuals)
				yield return o;

			var nonVirtuals = from layoutFrame in objects.OfType<LayoutFrameType>().SortLayoutFrames()
							  where !layoutFrame.@virtual
							  select layoutFrame;

			foreach (SerializationObject o in nonVirtuals)
				yield return o;
		}

		private static IEnumerable<LayoutFrameType> SortLayoutFrames(this IEnumerable<LayoutFrameType> layoutFrames)
		{
			SortedDictionary<string, LayoutFrameType> layoutFrameDictionary = new SortedDictionary<string, LayoutFrameType>();

			foreach (LayoutFrameType layoutFrame in layoutFrames)
				layoutFrameDictionary.Add(layoutFrame.name, layoutFrame);

			// holds the list of layoutFrames dependent on the key
			Dictionary<LayoutFrameType, IEnumerable<LayoutFrameType>> layoutFrameDependencies = new Dictionary<LayoutFrameType, IEnumerable<LayoutFrameType>>();

			// init dictionary 
			foreach (LayoutFrameType layoutFrame in layoutFrames)
			{
				layoutFrameDependencies.Add(layoutFrame, layoutFrame.GetDependencies(layoutFrameDictionary));
			}

			List<LayoutFrameType> orderedLayoutFrames = new List<LayoutFrameType>();

			bool hasProcessedItems;
			do
			{
				var dependentOnPrevious =
					from dependency in layoutFrameDependencies
					where dependency.Value.All<LayoutFrameType>(layoutFrame => orderedLayoutFrames.Contains(layoutFrame))
					select dependency.Key;

				foreach (var layoutFrame in dependentOnPrevious)
					yield return layoutFrame;

				// maintain ordered and remained frames
				var addedLayoutFrames = dependentOnPrevious.ToArray<LayoutFrameType>();

				foreach (var addedLayoutFrame in addedLayoutFrames)
					layoutFrameDependencies.Remove(addedLayoutFrame);

				orderedLayoutFrames.AddRange(addedLayoutFrames);

				hasProcessedItems = addedLayoutFrames.Length > 0;

			}
			while (hasProcessedItems);

			// add remaining controls (probably containig circular references)
			foreach (LayoutFrameType layoutFrame in layoutFrameDependencies.Keys)
				yield return layoutFrame;
		}

		private static IEnumerable<LayoutFrameType> GetDependencies(this LayoutFrameType layoutFrame, IDictionary<string, LayoutFrameType> dictionary)
		{
			List<string> names = new List<string>();
			if (!String.IsNullOrEmpty(layoutFrame.inherits))
				names.Add(layoutFrame.inherits);

			names.AddRange(from anchor in layoutFrame.Anchors
						   where anchor.relativePointSpecified && !String.IsNullOrEmpty(anchor.relativeTo)
						   select anchor.relativeTo);

			return from name in names
				   where dictionary.ContainsKey(name)
				   select dictionary[name];
		}
	}
}
