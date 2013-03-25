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
using System.ComponentModel;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.Drawing.Design;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	public class EventPropertyDescriptor : PropertyDescriptor
	{
		private const string categoryName = "Events";

		private static readonly Attribute[] attributes = new Attribute[] {
			new EditorAttribute(typeof(EventEditor), typeof(UITypeEditor))
		};

		public EventPropertyDescriptor(string name)
			: base(name, EventPropertyDescriptor.attributes)
		{
			this.eventName = (EventChoice)Enum.Parse(typeof(EventChoice), name);

			// this.cate
		}

		public override string Category
		{
			get { return EventPropertyDescriptor.categoryName; }
		}

		private EventChoice eventName;

		private ScriptsType FindContainingScripts(IEnumerable<ScriptsType> allScripts)
		{
			foreach (ScriptsType scripts in allScripts)
			{
				if (scripts.Events.ContainsKey(this.eventName))
				{
					return scripts;
				}
			}

			return null;
		}

		private void ClearEvent(IList<ScriptsType> allScripts)
		{
			ScriptsType scripts = this.FindContainingScripts(allScripts);
			if (scripts != null)
			{
				scripts.Events.Remove(this.eventName);

				if (scripts.Events.Count == 0)
				{
					allScripts.Remove(scripts);
				}
			}
		}

		public override bool CanResetValue(object component)
		{
			return true;
		}

		public override Type ComponentType
		{
			get { return typeof(IFrameControl); }
		}

		public override object GetValue(object component)
		{
			IFrameControl frameControl = (component as IFrameControl);
			if (frameControl == null)
			{
				return string.Empty;
			}

			FrameType frame = (FrameType)frameControl.SerializationObject;
			ScriptsType scripts = this.FindContainingScripts(frame.Scripts);
			
			return ((scripts == null) || (!scripts.Events.ContainsKey(this.eventName))) ?
				string.Empty : scripts.Events[this.eventName];

		}

		public override bool IsReadOnly
		{
			get { return false; }
		}

		public override Type PropertyType
		{
			get { return typeof(string); }
		}

		public override void ResetValue(object component)
		{
			IFrameControl frameControl = (component as IFrameControl);
			if (frameControl == null)
			{
				return;
			}

			FrameType frame = (FrameType)frameControl.SerializationObject;
			this.ClearEvent(frame.Scripts);
		}

		public override void SetValue(object component, object value)
		{
			IFrameControl frameControl = (component as IFrameControl);
			if (frameControl == null)
			{
				return;
			}

			FrameType frame = (FrameType)frameControl.SerializationObject;

			string valueAsString = (value as string);

			if (string.IsNullOrEmpty(valueAsString))
			{
				this.ClearEvent(frame.Scripts);
			}
			else
			{
				ScriptsType scripts = this.FindContainingScripts(frame.Scripts);
				
				if (scripts == null)
				{
					if (frame.Scripts.Count > 0)
					{
						scripts = frame.Scripts[0];
					}
					else
					{
						scripts = new ScriptsType();
						frame.Scripts.Add(scripts);
					}
				}

				scripts.Events[this.eventName] = valueAsString;
			}
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

        public override object GetEditor(Type editorBaseType)
        {
            return base.GetEditor(editorBaseType);
        }
	}
}
