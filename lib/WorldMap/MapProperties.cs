using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Xml;

namespace Multiverse.Lib.WorldMap
{
    public interface IObjectWithProperties
    {
        MapProperties Properties
        {
            get;
        }

        List<IObjectWithProperties> PropertyParents
        {
            get;
        }
    }

    #region MapPropertyDescriptor class definition
    public class MapPropertyDescriptor : PropertyDescriptor
    {
        private string propName;
        private MapProperties properties;

        public MapPropertyDescriptor(MapProperties properties, string propName)
            :
            base(propName, new Attribute[0])
        {
            this.propName = propName;
            this.properties = properties;
        }

        public override Type ComponentType
        {
            get { return properties.GetProperty(propName).GetType(); }
        }

        public override bool IsReadOnly
        {
            get { return (Attributes.Matches(ReadOnlyAttribute.Yes)); }
        }

        public override Type PropertyType
        {
            get { return properties.GetProperty(propName).Type; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            // Have the property bag raise an event to get the current value
            // of the property.

            return properties.GetProperty(propName).Value;
        }

        public override void ResetValue(object component)
        {
            
        }

        public override void SetValue(object component, object value)
        {
            properties.SetValue(propName, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
    #endregion

    public class MapProperty
    {
        protected string name;
        protected string category;
        protected string description;
        protected Type type;
        protected object value;
        protected MapProperties collection;

        public MapProperty(MapProperties collection, string name, string category, string description, Type type, object value)
        {
            this.collection = collection;
            this.name = name;
            this.category = category;
            this.description = description;
            this.type = type;
            this.value = value;
        }

        public MapProperty(MapProperties collection, XmlReader r)
        {
            string valueString = null;

            this.collection = collection;
            
            // parse attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Name":
                        name = r.Value;
                        break;
                    case "Category":
                        category = r.Value;
                        break;
                    case "Description":
                        description = r.Value;
                        break;
                    case "Type":
                        type = Type.GetType(r.Value);
                        break;
                    case "Value":
                        valueString = r.Value;
                        break;
                }
            }

            r.MoveToElement();

            switch (type.FullName)
            {
                case "System.String":
                    value = valueString;
                    break;
                default:
                    throw new Exception("Attempt to parse MapProperty of unknown type: " + type.FullName);
            }
        }

        /// <summary>
        /// This constructor is used to copy a property into a new collection.  Typically used
        /// when creating an instance of the property in a child object.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="src"></param>
        public MapProperty(MapProperties collection, MapProperty src)
            :this(collection, src.name, src.category, src.description, src.type, src.value)
        {
        }

        public void ToXml(XmlWriter w)
        {
            w.WriteStartElement("Property");
            w.WriteAttributeString("Name", name);
            w.WriteAttributeString("Category", category);
            w.WriteAttributeString("Description", description);
            w.WriteAttributeString("Type", type.FullName);
            w.WriteAttributeString("Value", value.ToString());
            w.WriteEndElement(); // Property
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Category
        {
            get
            {
                return category;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public Type Type
        {
            get
            {
                return type;
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                Debug.Assert(type.IsInstanceOfType(value));
                this.value = value;
            }
        }
    }

    public class MapProperties : ICustomTypeDescriptor
    {
        protected IObjectWithProperties containingObject;
        protected Dictionary<string, MapProperty> properties;

        public MapProperties(IObjectWithProperties containingObject)
        {
            this.containingObject = containingObject;
            properties = new Dictionary<string, MapProperty>();
        }

        public void NewProperty(string name, string category, string description, Type type, object value)
        {
            MapProperty prop = new MapProperty(this, name, category, description, type, value);
            properties[name] = prop;
        }

        public void ParseProperty(XmlReader r)
        {
            MapProperty prop = new MapProperty(this, r);
            properties[prop.Name] = prop;
        }

        public void ToXml(XmlWriter w)
        {
            foreach (MapProperty prop in properties.Values)
            {
                prop.ToXml(w);
            }
        }
        
        public object GetValue(string name)
        {
            MapProperty prop = GetProperty(name);

            if (prop != null)
            {
                return prop.Value;
            }
            else
            {
                throw new Exception("attempted to get MapProperty that doesn't exist");
            }

        }

        protected MapProperty GetPropertyFromParents(string name)
        {
            foreach (IObjectWithProperties parent in containingObject.PropertyParents)
            {
                MapProperty prop = parent.Properties.GetProperty(name);
                if (prop != null)
                {
                    return prop;
                }
            }

            return null;
        }

        public MapProperty GetProperty(string name)
        {
            if (properties.ContainsKey(name))
            {
                return properties[name];
            }
            else 
            {
                return GetPropertyFromParents(name);
            }
        }

        public void SetValue(string name, object value)
        {
            if (properties.ContainsKey(name))
            {
                // if the property exists at this level, then just set it
                properties[name].Value = value;
            }
            else
            {
                // look at ancestors to find the property
                MapProperty src = GetProperty(name);
                if (src != null)
                {
                    // if the property exists on an ancestor, then copy it down to this level, and set the value
                    // in the local copy.
                    MapProperty prop = new MapProperty(this, src);
                    prop.Value = value;

                    properties[name] = prop;
                }
                else
                {
                    throw new Exception("attempt to set MapProperty that does not exist in hierarchy");
                }
            }
        }

        protected void AddPropertiesToList(List<MapProperty> propList)
        {
            // add my properties to the list if it is not already there
            foreach (MapProperty prop in properties.Values)
            {
                if (!propList.Contains(prop))
                {
                    propList.Add(prop);
                }
            }

            // add parent properties to the list
            foreach (IObjectWithProperties parent in containingObject.PropertyParents)
            {
                parent.Properties.AddPropertiesToList(propList);
            }
        }

        public List<MapProperty> GetPropertyList()
        {
            List<MapProperty> propList = new List<MapProperty>();

            AddPropertiesToList(propList);

            return propList;
        }

        protected void AddPropertyNamesToList(List<string> nameList)
        {
            // add my properties to the list if it is not already there
            foreach (MapProperty prop in properties.Values)
            {
                if (!nameList.Contains(prop.Name))
                {
                    nameList.Add(prop.Name);
                }
            }

            // add parent properties to the list
            foreach (IObjectWithProperties parent in containingObject.PropertyParents)
            {
                parent.Properties.AddPropertyNamesToList(nameList);
            }
        }

        public List<string> GetPropertyNames()
        {
            List<string> nameList = new List<string>();

            AddPropertyNamesToList(nameList);

            return nameList;
        }

        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            List<string> propNames = GetPropertyNames();

            if (propNames.Count == 0)
            {
                return null;
            }
            else
            {
                return new MapPropertyDescriptor(this, propNames[0]);
            }
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            List<string> propNames = GetPropertyNames();

            PropertyDescriptor[] descriptors = new PropertyDescriptor[propNames.Count];
            int i = 0;
            foreach ( string name in propNames )
            {
                descriptors[i] = new MapPropertyDescriptor(this, name);
                i++;
            }

            return new PropertyDescriptorCollection(descriptors);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }
}
