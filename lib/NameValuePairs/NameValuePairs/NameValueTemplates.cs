using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Multiverse.ToolBox
{
	public class Property
	{
		public string Name;
		public string Type;
		public string Default;
		public List<string> Enum;
	}

	public class NameValueTemplate
	{
		public string Name;
		public bool Waypoint = false;
		public bool Boundary = false;
		public bool	Road = false;
		public bool Object = false;
        public bool SpawnGenerator = false;
        public bool Asset = false;
		public Dictionary<string, Property> properties;
	}


	public class NameValueTemplateCollection
	{
		Dictionary<string,NameValueTemplate> NameValueTemplates; 


		public NameValueTemplateCollection(string directoryName)
		{
			NameValueTemplates = new Dictionary<string, NameValueTemplate>();
			if (Directory.Exists(directoryName))
			{
				foreach(string filename in Directory.GetFiles(directoryName))
				{
					if (filename.EndsWith(".xml") || filename.EndsWith(".Xml") || filename.EndsWith(".XML"))
					{
						TextReader r = new StreamReader(filename);
						FromXML(r);
						r.Close();
					}
				}
			}
		}

		public List<string> List(string type)
		{
			List<string> list = new List<string>();
			foreach (string Name in NameValueTemplates.Keys)
			{
				NameValueTemplate template;
				NameValueTemplates.TryGetValue(Name, out template);
				if (template != null)
				{
					if ((String.Equals(type, "Region") &&
						template.Boundary) ||
						(String.Equals(type, "Object") &&
						template.Object) ||
						(String.Equals(type, "Marker") &&
						template.Waypoint) ||
						(String.Equals(type, "Road") &&
						template.Road) ||
                        (String.Equals(type, "SpawnGenerator") &&                        
                        template.SpawnGenerator) ||
                        (String.Equals(type, "Asset") &&
                        template.Asset))

					{
						list.Add(Name);
					}
				}
			}
			return list;
		}

		public List<string> NameValuePropertiesList(string templateName)
		{
			List<string> list = new List<string>();
			NameValueTemplate template;
			if (NameValueTemplates.TryGetValue(templateName, out template))
			{
				if (template != null)
				{
					foreach (string propertyName in template.properties.Keys)
					{
						list.Add(propertyName);
					}
				}
			}
			return list;
		}

		public string DefaultValue(string templateName, string propertyName)
		{
			NameValueTemplate template;
			Property prop;
			if (NameValueTemplates.TryGetValue(templateName, out template))
			{
				if ((template != null) && (template.properties.TryGetValue(propertyName, out prop)))
				{
                    if (prop.Default != null)
                    {
                        return prop.Default;
                    }
				}
			}
            return "";

		}

		public string PropertyType(string templateName, string propertyName)
		{
			NameValueTemplate template;
			Property prop;
			if (NameValueTemplates.TryGetValue(templateName, out template))
			{
				if ((template != null) && (template.properties.TryGetValue(propertyName, out prop)))
				{
					return prop.Type;
				}
			}
			return "";
		}

		public List<string> Enum(string typeName, string propertyName)
		{
			NameValueTemplate template;
			Property prop;
			if (NameValueTemplates.TryGetValue(typeName, out template))
			{
				if (template.properties.TryGetValue(propertyName, out prop))
				{
					return prop.Enum;
				}
			}
			return null;
		}

		protected string parseTextNode(XmlTextReader r)
		{
			// read the value
			r.Read();

			if (r.NodeType == XmlNodeType.Whitespace)
			{
				while (r.NodeType == XmlNodeType.Whitespace)
				{
					r.Read();
				}
			}

			if (r.NodeType != XmlNodeType.Text)
			{
				return (null);
			}
			string ret = r.Value;

			// error out if we dont see an end element here
			r.Read();
			if (r.NodeType == XmlNodeType.Whitespace)
			{
				while (r.NodeType == XmlNodeType.Whitespace)
				{
					r.Read();
				}
			}			
			if (r.NodeType != XmlNodeType.EndElement)
			{
				// XXX - should generate an exception here?
				return (null);
			}
			return (ret);
		}


		protected List<string> parseEnum(XmlTextReader r)
		{
			List<string> list = new List<string>();
			while (r.Read())
			{
				if (r.NodeType == XmlNodeType.Whitespace)
				{
					continue;
				}
				if (r.NodeType == XmlNodeType.EndElement)
				{
					break;
				}
				if (r.NodeType == XmlNodeType.Element)
				{
					if (r.Name == "Value")
					{
						list.Add(parseTextNode(r));
					}
				}

				if (r.NodeType != XmlNodeType.EndElement)
				{
					return null;
				}
			}
			return list;
		}


		protected Dictionary<string, Property> parseTypeProperties(XmlTextReader r)
		{
			Dictionary<string, Property> props = new Dictionary<string, Property>();
			while (r.Read())
			{

				if (r.NodeType == XmlNodeType.Whitespace)
				{
					continue;
				}
				if (r.NodeType == XmlNodeType.EndElement)
				{
					break;
				}
				if (r.NodeType == XmlNodeType.Element)
				{
					if (r.Name == "Property")
					{
						r.Read();
						Property prop = parseTypeProperty(r);
						props.Add(prop.Name, prop);
					}
				}
			}
			return props;
		}




		protected Property parseTypeProperty(XmlTextReader r)
		{
			Property prop = new Property();
			while (r.Read())
			{
				if (r.NodeType == XmlNodeType.Whitespace)
				{
					continue;
				}
				if (r.NodeType == XmlNodeType.EndElement)
				{
					break;
				}
				if (r.NodeType == XmlNodeType.Element)
				{
					switch (r.Name)
					{
						case "Name":
							prop.Name = parseTextNode(r);
							break;
						case "Type":
							prop.Type = parseTextNode(r);
							break;
						case "Enums":
							prop.Enum = parseEnum(r);
							break;
						case "Default":
							prop.Default = parseTextNode(r);
							break;
					}
					
				}
			}
			return prop;
		}

		private void parseObjectTypes(XmlTextReader r, NameValueTemplate template)
		{
			while (r.Read())
			{
				if (r.NodeType == XmlNodeType.Whitespace)
				{
					continue;
				}
				if (r.NodeType == XmlNodeType.Element)
				{
					switch (r.Name)
					{
						case "Marker":
							template.Waypoint = true;
							break;
						case "Region":
							template.Boundary = true;
							break;
						case "Road":
							template.Road = true;
							break;
						case "StaticObject":
							template.Object = true;
							break;
                        case "SpawnGenerator":
                            template.SpawnGenerator = true;
                            break;
					}
				}
				if (r.NodeType == XmlNodeType.EndElement)
				{
					break;
				}
			}
		}

		protected Dictionary<string, NameValueTemplate> parseTemplate(XmlTextReader r)
		{
			NameValueTemplate template = new NameValueTemplate();
			while (r.Read())
			{
				if (r.NodeType == XmlNodeType.Whitespace)
				{
					continue;
				}
				if (r.NodeType == XmlNodeType.Element)
				{
					switch (r.Name)
					{
						case "Name":
							template.Name = parseTextNode(r);
							break;
						case "Properties":
							r.Read();
							template.properties = parseTypeProperties(r);
							break;
						case "ObjectTypes":
							r.Read();
							parseObjectTypes(r, template);
							break;
					}
				}
			}
			NameValueTemplates.Add(template.Name, template);
			return NameValueTemplates;
		}

		private void FromXML(TextReader t)
		{
			XmlTextReader r = new XmlTextReader(t);
			while (r.Read())
			{
				if (r.NodeType == XmlNodeType.Whitespace)
				{
					continue;
				}
				// look for the start of the assets list
				if (r.NodeType == XmlNodeType.Element)
				{
					if (r.Name == "NameValueTemplate")
					{
						// we found the list of assets, now parse it
						parseTemplate(r);
					}
				}
			}
		}
	}
}


	





