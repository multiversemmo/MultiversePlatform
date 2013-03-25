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
using System.Collections;
using System.Drawing;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	public class PropertyBag
	{
		#region inner dictionary implementation

		private SortedDictionary<string, object> dictionary = new SortedDictionary<string, object>();

		private LayoutFrameType owner = null;

		public PropertyBag(LayoutFrameType owner)
		{
			this.owner = owner;
		}

		#endregion

		#region public interface

        public object this[string name]
		{
			get
			{
				if (dictionary.ContainsKey(name))
					return dictionary[name];

				if (!IsSerializing)
				{
					if (owner != null && owner.InheritedObject != null)
						return owner.InheritedObject.Properties[name];
				}

				return owner.GetDefaultValue(name);
			}
			set
			{
				// TODO: solve issue: inherited property value cannot be set back to default
				if (IsEmpty(value) || 
					(value.Equals(owner.GetDefaultValue(name))))
					dictionary.Remove(name);
				else
					dictionary[name] = value;
			}
		}

        public T GetValue<T>(string name)
		{
			object value = this[name];
			if (value == null)
				value = default(T);
			return (T)value;
		}

        public bool HasValue(string name)
        {
            return dictionary.ContainsKey(name) && !IsEmpty(dictionary[name]);
        }

        public bool HasInheritedValue(string name)
        {
            if (owner != null)
            {
                LayoutFrameType layoutFrame = owner.InheritedObject;

                while (layoutFrame != null)
                {
                    if (layoutFrame.Properties.HasValue(name))
                        return true;

                    layoutFrame = layoutFrame.InheritedObject;
                }
            }

            return false;
        }

		#endregion

		#region array handling

		public T[] GetArray<T>(string name)
		{
			return this.HasValue(name) ?
				new T[] { this.GetValue<T>(name) } :
				new T[] { };
		}

		public void SetArray<T>(string name, T[] values)
		{
			if (values != null && values.Length > 0)
				this[name] = values[0];
			else
				this.Remove(name);
		}

		#endregion

		#region list handling - obsolete

		//public List<T> GetList<T>(string name)
		//{
		//    List<T> value;
		//    if (!dictionary.ContainsKey(name))
		//    {
		//        value = new List<T>();
		//        dictionary.Add(name, value);
		//    }
		//    else
		//    {
		//        value = dictionary[name] as List<T>;
		//    }

		//    if (value.Count > 0)
		//        return value;

		//    if (!IsSerializing)
		//    {
		//        if (owner != null && owner.InheritedObject != null)
		//            return owner.InheritedObject.Properties.GetList<T>(name);
		//    }

		//    return value;
		//}

		//public void SetList<T>(string name, List<T> value)
		//{
		//    if (value == null)
		//        value = new List<T>();
		//    dictionary[name] = value;
		//}

		#endregion

		#region helper methods

		private bool IsEmpty(object value)
		{
			if (value == null)
				return true;

			if (value is string)
				return String.IsNullOrEmpty(value as string);

			if (value is IList)
				return (value as IList).Count == 0;

			if (value is Color)
				return ((Color)value).IsEmpty;

			return false;
		}

		private bool IsSerializing
		{
			get
			{
				FrameXmlDesignerLoader activeDesignerLoader = FrameXmlDesignerLoader.ActiveDesignerLoader;
				if (activeDesignerLoader == null)
					return true;

				return activeDesignerLoader.IsSerializing;
			}
		}

		#endregion

		internal void Remove(string name)
		{
			dictionary.Remove(name);
		}
	}
}
