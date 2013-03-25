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
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.Collections.Generic;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	public class NotifierTypeDescriptor : PropertyDescriptor
	{
		private PropertyDescriptor pd = null;
		private ISerializableControl serializableControl = null;

		public NotifierTypeDescriptor(ISerializableControl serializableControl, PropertyDescriptor pd)
			: base(pd)
		{
			this.serializableControl = serializableControl;
			this.pd = pd;
		}

		public override string Category
		{
			get { return this.pd.Category; }
		}

		private LayoutFrameType GetLayoutFrame()
		{
			return serializableControl.SerializationObject as LayoutFrameType;
		}

		public override bool CanResetValue(object component)
		{
			LayoutFrameType layoutFrame = GetLayoutFrame();

			return (layoutFrame != null) ?
				layoutFrame.Properties.HasValue(this.Name) :
				pd.CanResetValue(component);
		}

		public override void ResetValue(object component)
		{
			LayoutFrameType layoutFrame = GetLayoutFrame();

			if (layoutFrame != null)
			{
				// make the component aware of the change
				pd.SetValue(component, null);
				// remove the private value 
				layoutFrame.Properties.Remove(this.Name);
			}
			else
				pd.ResetValue(component);

			OnPropertyChanged();
		}

		public override Type ComponentType
		{
			get { return pd.ComponentType; }
		}

		public override object GetValue(object component)
		{
			return pd.GetValue(component);
		}

		public override bool IsReadOnly
		{
			get { return pd.IsReadOnly; }
		}

		public override Type PropertyType
		{
			get { return pd.PropertyType; }
		}

		public override void SetValue(object component, object value)
		{
			pd.SetValue(component, value);

			OnPropertyChanged();
		}

		private void OnPropertyChanged()
		{
			// notify control about the change; 
			// the control will notify the DesignerLoader
			PropertyChangedEventArgs e = new PropertyChangedEventArgs(pd.Name);
			serializableControl.OnPropertyChanged(e);
		}

		public override bool ShouldSerializeValue(object component)
		{
			return pd.ShouldSerializeValue(component);
		}
	}

}
