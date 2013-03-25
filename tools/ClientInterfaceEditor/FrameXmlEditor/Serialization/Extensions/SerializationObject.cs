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
using System.ComponentModel;
using System.Xml.Serialization;
using System.Reflection;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	public partial class SerializationObject
	{
		static SerializationObject()
		{
			RegisterDefaultValues(typeof(SerializationObject));
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// By default it is only the pure class name without namespace and external class name
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			string text = this.GetType().Name;
			int index = text.LastIndexOfAny(new char[] { '.', '+' });
			if (index >= 0)
				text = text.Substring(index + 1);
			
			return 
				text;
		}

		private static Dictionary<Type, IDictionary<string, object>> allDefaultValues = new Dictionary<Type,IDictionary<string,object>>();

		protected static void RegisterDefaultValues(Type type)
		{
			IDictionary<string, object> defaultValues = new Dictionary<string, object>();
			foreach (PropertyInfo propertyInfo in type.GetProperties())
			{
				object[] defaultAttributes = propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false);
				if (defaultAttributes.Length > 0)
				{
					DefaultValueAttribute defaultAttribute = (DefaultValueAttribute)defaultAttributes[0];
					defaultValues.Add(propertyInfo.Name, defaultAttribute.Value);
				}
			}
			allDefaultValues.Add(type, defaultValues);
		}

		public object GetDefaultValue(string name)
		{
			Type type = this.GetType();
			if (allDefaultValues.ContainsKey(type))
			{
				var defaultValues = allDefaultValues[type];
				if (defaultValues.ContainsKey(name))
					return defaultValues[name];
			}

			return null;
		}
	}
}
