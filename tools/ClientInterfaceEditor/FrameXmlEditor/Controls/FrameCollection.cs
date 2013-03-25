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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.ComponentModel;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	/// <summary>
	/// The Frames Collection. 
	/// </summary>
	/// <remarks>
	/// It is bound to a Control and works on its Controls collection.
	/// </remarks>
	public class FrameCollection : IEnumerable<IFrameControl>
	{
		private Control parent;

		/// <summary>
		/// Initializes a new instance of the <see cref="FrameCollection"/> class.
		/// </summary>
		/// <param name="parent">The parent control.</param>
		public FrameCollection(Control parent)
		{
			this.parent = parent;
		}

		/// <summary>
		/// Adds the frame(s) passed as parameter(s).
		/// </summary>
		/// <remarks>Any number of frames can be passed</remarks>
		/// <param name="frames">The frames.</param>
		public void Add(params IFrameControl[] frames)
		{
			this.AddRange(frames);
		}

		/// <summary>
		/// Adds the specified frames.
		/// </summary>
		/// <param name="frames">The frames.</param>
		public void AddRange(IFrameControl[] frames)
		{
			this.parent.Controls.AddRange(frames.OfType<Control>().ToArray<Control>());
		}

		/// <summary>
		/// Removes the specified frame.
		/// </summary>
		/// <param name="frame">The frame.</param>
		public void Remove(IFrameControl frame)
		{
			this.parent.Controls.Remove((Control)frame);
		}

		/// <summary>
		/// Clears this instance.
		/// </summary>
		public void Clear()
		{
			// backward processing is necessary because of the removals
			for (int i = parent.Controls.Count - 1; i >= 0; i--)
			{
				if (parent.Controls[i] is IFrameControl)
					parent.Controls.RemoveAt(i);
			}
		}

		#region IEnumerable<Frame> Members

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<IFrameControl> GetEnumerator()
		{
			var frames = parent.Controls.OfType<IFrameControl>().Where<IFrameControl>(frame => !frame.Inherited).SortControls();
			return
				frames.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// <remarks>Calls the generic enumerator</remarks>
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
