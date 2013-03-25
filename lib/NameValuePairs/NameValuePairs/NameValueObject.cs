using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;


namespace Multiverse.ToolBox
{
	public class valueItem
	{
		public string value = "";
		public string type = "";
		public List<string> enumList = new List<string>();
	}

	public class NameValueObject : IEnumerable<valueItem>
	{
		public Dictionary<string, valueItem> nameValuePairs;

		public NameValueObject()
		{
			nameValuePairs = new Dictionary<string, valueItem>();
		}

		public NameValueObject(XmlReader r)
		{
			nameValuePairs = new Dictionary<string, valueItem>();
			parseNameValuePairs(r);
			return;
		}

		public NameValueObject(NameValueObject nvo)
		{
			this.nameValuePairs = new Dictionary<string, valueItem>();
			foreach (string name in nvo.NameValuePairKeyList())
			{
				valueItem value = new valueItem();
				nvo.nameValuePairs.TryGetValue(name, out value);
                if (value.type == "Enum" && value.enumList.Count > 0)
                {
                    this.AddNameValuePair(name, value.value, value.enumList);
                }
                else
                {
                    this.AddNameValuePair(name, value.value, value.type);
                }
			}
		}
	

		public int Count
		{
			get
			{
				return nameValuePairs.Count;
			}
		}


        protected void parseNameValuePair(XmlReader r)
        {
            string valName = "";
            valueItem value = new valueItem();
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                switch (r.Name)
                {
                    case "Name":
                        valName = r.Value;
                        break;
                    case "Value":
                        value.value = r.Value;
                        break;
                    case "Type":
                        value.type = r.Value;
                        break;
                }
            }
            if (String.Equals(value.type, "Enum"))
            {
                r.MoveToElement();
                value.enumList = parseEnumList(r);
            }
            this.AddNameValuePair(valName, value);
            r.MoveToElement();
        }

        protected List<string> parseEnumList(XmlReader r)
        {
            List<string> enumList = null;
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
                        case "Enum":
                            enumList = parseEnumItems(r);
                            break;
                    }
                }
            }
            return enumList;
        }

        protected List<string> parseEnumItems(XmlReader r)
        {
            List<string> enumList = new List<string>();
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
                        case "EnumItem":
                            string item = parseEnumItem(r);
                            enumList.Add(item);
                            break;
                    }
                }
            }
            return enumList;
        }


        protected string parseEnumItem(XmlReader r)
        {
            string enumItem = "";

            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                if (r.NodeType == XmlNodeType.Text)
                {
                    enumItem = r.Value;
                }
                if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }

            return enumItem;
        }

		protected void parseNameValuePairs(XmlReader r)
		{

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
                        case "NameValuePair":
                            parseNameValuePair(r);
                            break;
                    }
                }
            }
            
		}

		public void AddNameValuePair(string name, string value, List<string> enumList)
		{
			valueItem item = new valueItem();
			item.type = "Enum";
            foreach (string eitem in enumList)
            {
                item.enumList.Add(eitem);
            }
			item.value = value;
			this.nameValuePairs.Add(name, item);
		}


		public void AddNameValuePair(string name, string value, string type)
		{
			valueItem item = new valueItem();
			item.type = type;
			item.value = value;
			this.nameValuePairs.Add(name, item);
		}

		public void AddNameValuePair(string name, string value)
		{
			valueItem item = new valueItem();
			item.type = "String";
			item.value = value;
			this.nameValuePairs.Add(name, item);
		}

		public void AddNameValuePair(string name, valueItem value)
		{
			this.nameValuePairs.Add(name, value);
		}

		public valueItem LookUp(string name)
        {
            valueItem value = new valueItem();
            if (this.nameValuePairs.TryGetValue(name, out value))
            {
                return value;
            }
            return null;
        }

		public Dictionary<string, valueItem> CopyNameValueObject()
		{
			NameValueObject dest = new NameValueObject();
			foreach(string name in this.NameValuePairKeyList())
			{
				valueItem value = new valueItem();
				this.nameValuePairs.TryGetValue(name, out value);
				dest.AddNameValuePair(name, value.value, value.enumList);
			}
			return dest.nameValuePairs;
		}

		public void SetNameValueObject(Dictionary<string,valueItem> source)
		{
			this.nameValuePairs.Clear();
			foreach (string name in source.Keys)
			{
				valueItem value = new valueItem();
				source.TryGetValue(name, out value);
				this.nameValuePairs.Add(name, value);
			}
			return;
		}

		public void EditNameValuePair(string oldName, string newName, string newValue, string type, List<string> enumList)
		{
			this.nameValuePairs.Remove(oldName);
			valueItem value = new valueItem();
			value.value = newValue;
			value.type = type;
            if (enumList != null)
            {
                value.enumList = new List<string>(enumList);
            }
			this.nameValuePairs.Add(newName, value);
		}

		public void RemoveNameValuePair(string delName)
		{
			this.nameValuePairs.Remove(delName);
		}

		public void ToXml(XmlWriter w)
		{
            if (nameValuePairs.Count > 0)
            {
                w.WriteStartElement("NameValuePairs");
                foreach (string namekey in nameValuePairs.Keys)
                {
                    w.WriteStartElement("NameValuePair");
                    w.WriteAttributeString("Name", namekey);
                    valueItem value = new valueItem();
                    nameValuePairs.TryGetValue(namekey, out value);
                    w.WriteAttributeString("Value", value.value);
                    w.WriteAttributeString("Type", value.type);
                    if (String.Equals(value.type, "Enum") && value.enumList != null && value.enumList.Count != 0)
                    {
                        w.WriteStartElement("Enum");
                        foreach (string enumElement in value.enumList)
                        {
                            w.WriteElementString("EnumItem", enumElement);
                        }
                        w.WriteEndElement();
                    }
                    w.WriteEndElement();
                }
                w.WriteEndElement();
            }
		}

		public List<string> NameValuePairKeyList()
		{
			List<string> list = new List<string>();
			if (this.nameValuePairs != null)
			{
				foreach (string key in this.nameValuePairs.Keys)
				{
					{
						list.Add(key);
					}
				}
			}
			return list;
		}

		#region IEnumerable<valueItem> Members

		public IEnumerator<valueItem> GetEnumerator()
		{
			return nameValuePairs.Values.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.nameValuePairs.Values.GetEnumerator();
		}
		#endregion
	}
}
