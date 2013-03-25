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
using Microsoft.MultiverseInterfaceStudio.FrameXml.Controls;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml
{
	/// <summary>
	/// Iterates through the passed frameXmlHierarchy
	/// </summary>
    public class LayoutFrameCollection : IEnumerable<LayoutFrameType>
    {
		private Serialization.Ui frameXmlHierarchy;

		public LayoutFrameCollection(Serialization.Ui frameXmlHierarchy)
		{
			this.frameXmlHierarchy = frameXmlHierarchy;
		}

		/// <summary>
		/// Main iterator logic
		/// </summary>
		/// <param name="serializationObject">The serialization object.</param>
		/// <returns></returns>
		private IEnumerable<LayoutFrameType> GetInternalElements(SerializationObject serializationObject)
		{
			if (serializationObject != null)
			{
				// return current element
				if (serializationObject is LayoutFrameType)
					yield return serializationObject as LayoutFrameType;

				// return child layers
				FrameType frame = serializationObject as FrameType;
				if (frame != null)
				{
					foreach (var layers in frame.LayersList)
						foreach (var layer in layers.Layer)
							foreach (var layerable in layer.Layerables.OfType<LayoutFrameType>())
								yield return layerable;
				}

				// return child controls
				foreach (var childObject in serializationObject.Controls)
				{
					var childElements = GetInternalElements(childObject);
					foreach (var element in childElements)
						yield return element;
				}
			}
		}

		/// <summary>
		/// Gets the <see cref="Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization.LayoutFrameType"/> with the specified name.
		/// </summary>
		/// <value></value>
		public LayoutFrameType this[string name]
		{
			get
			{
				if (name == null)
					return null;

				var q = from element in this
						where element.name == name
						select element;

				return q.FirstOrDefault<LayoutFrameType>();
			}
		}


		#region IEnumerable<LayoutFrameType> Members

		public IEnumerator<LayoutFrameType> GetEnumerator()
		{
			return GetInternalElements(frameXmlHierarchy).GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
