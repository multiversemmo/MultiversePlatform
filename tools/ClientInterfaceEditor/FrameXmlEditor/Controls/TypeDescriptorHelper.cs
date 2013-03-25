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
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	/// <summary>
	/// Static class for encapsulating ICustomTypeDescriptor implementation.
	/// </summary>
    public static class TypeDescriptorHelper
    {
        private const bool NO_CUSTOM_TYPE_DESC = true;

        public static AttributeCollection GetAttributes(ISerializableControl component)
        {
            if (IsInheritedControl(component))
                return AttributeCollection.FromExisting(TypeDescriptor.GetAttributes(component, NO_CUSTOM_TYPE_DESC), new ReadOnlyAttribute(true));

            return TypeDescriptor.GetAttributes(component, NO_CUSTOM_TYPE_DESC);
        }

        private static bool IsInheritedControl(ISerializableControl component)
        {
            BaseControl baseControl = component as BaseControl;
            if (baseControl != null)
                return baseControl.Inherited;

            return false;
        }

        public static string GetClassName(ISerializableControl component)
        {
            return TypeDescriptor.GetClassName(component, NO_CUSTOM_TYPE_DESC);
        }

        public static string GetComponentName(ISerializableControl component)
        {
            return TypeDescriptor.GetComponentName(component, NO_CUSTOM_TYPE_DESC);
        }

        public static TypeConverter GetConverter(ISerializableControl component)
        {
            return TypeDescriptor.GetConverter(component, NO_CUSTOM_TYPE_DESC);
        }

        public static EventDescriptor GetDefaultEvent(ISerializableControl component)
        {
            return TypeDescriptor.GetDefaultEvent(component, NO_CUSTOM_TYPE_DESC);
        }

        public static PropertyDescriptor GetDefaultProperty(ISerializableControl component)
        {
            return TypeDescriptor.GetDefaultProperty(component, NO_CUSTOM_TYPE_DESC);
        }

        public static object GetEditor(ISerializableControl component, Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(component, editorBaseType, NO_CUSTOM_TYPE_DESC);
        }

		/// <summary>
		/// Gets the events.
		/// </summary>
		/// <param name="component">The component.</param>
		/// <returns></returns>
		/// <remarks>It is not yet working</remarks>
        public static EventDescriptorCollection GetEvents(ISerializableControl component)
        {
			return TypeDescriptor.GetEvents(component, NO_CUSTOM_TYPE_DESC);
		}

        public static EventDescriptorCollection GetEvents(ISerializableControl component, Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(component, attributes, NO_CUSTOM_TYPE_DESC);
        }

        public static PropertyDescriptorCollection GetProperties(ISerializableControl component)
        {
            return TypeDescriptor.GetProperties(component, NO_CUSTOM_TYPE_DESC);
        }

		private static Attribute[] GetAttributes(SerializationObject serializationObject, string name)
		{
			LayoutFrameType layoutFrame = serializationObject as LayoutFrameType;

			var attibutes = new List<Attribute>();

			if (layoutFrame != null && layoutFrame.Properties.HasInheritedValue(name))
			{
				// set DefaultValue attribute if there is an inherited value
				object defaultValue = layoutFrame.InheritedObject.Properties[name];
				attibutes.Add(new DefaultValueAttribute(defaultValue));

				// add Inherited  attribute if it doesn't have a private value
				if (!layoutFrame.Properties.HasValue(name))
				{
					attibutes.Add(new InheritedAttribute());
				}
			}
			return attibutes.ToArray();
		}

        public static PropertyDescriptorCollection GetProperties(ISerializableControl component, Attribute[] attributes)
        {
            PropertyDescriptorCollection propertyDescriptorCollection = new PropertyDescriptorCollection(null);

            // add only direct properties of LayoutFrame - inherited properties are not added
            PropertyDescriptorCollection originalProperties = TypeDescriptor.GetProperties(component, attributes, true);
            foreach (PropertyDescriptor propertyDescriptor in originalProperties)
            {
				if (typeof(ISerializableControl).IsAssignableFrom(propertyDescriptor.ComponentType))
				{
					Attribute[] attributeArray = GetAttributes(component.SerializationObject, propertyDescriptor.Name);
					var pd = TypeDescriptor.CreateProperty(component.GetType(), propertyDescriptor, attributeArray);
					propertyDescriptorCollection.Add(new NotifierTypeDescriptor(component, pd));
				}
            }

            // add properties of the serialization object
            if (component.SerializationObject != null)
            {
                PropertyDescriptorCollection serializationProperties = TypeDescriptor.GetProperties(component.SerializationObject, attributes, true);
                
				foreach (PropertyDescriptor propertyDescriptor in serializationProperties)
                {
					Attribute[] attributeArray = GetAttributes(component.SerializationObject, propertyDescriptor.Name);
					var pd = TypeDescriptor.CreateProperty(component.SerializationObject.GetType(), propertyDescriptor, attributeArray);
                    propertyDescriptorCollection.Add(new NotifierTypeDescriptor(component, pd));
                }
            }

            // adding events
			IFrameControl frameControl = component as IFrameControl;
			if (frameControl != null)
			{
				PropertyDescriptor[] eventDescriptors = frameControl.GetEventDescriptors();
				foreach (PropertyDescriptor eventDescriptor in eventDescriptors)
				{
					propertyDescriptorCollection.Add(new NotifierTypeDescriptor(component, eventDescriptor));
				}
			}

			return propertyDescriptorCollection;
        }

        public static object GetPropertyOwner(ISerializableControl component, PropertyDescriptor pd)
        {
            if ((pd != null) && (pd.ComponentType == component.SerializationObject.GetType()))
                return component.SerializationObject;
            else
                return component;
        }
    }

}
