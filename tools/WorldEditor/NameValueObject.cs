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
using System.Text;
using System.Xml;


namespace Multiverse.Tools.WorldEditor
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
			nameValuePairs = new Dictionary<string, valueItem>();
			foreach (string name in this.NameValuePairKeyList())
			{
				valueItem value = new valueItem();
				nvo.nameValuePairs.TryGetValue(name, out value);
				this.AddNameValuePair(name, value.value, value.enumList);
			}
			return;
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
					case "Enum":
						value.enumList.Add(r.Value);
						break;
				}
			}
			this.AddNameValuePair(valName, value);
		}

		protected void parseNameValuePairs(XmlReader r)
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
						case "NameValuePair":
							parseNameValuePair(r);
							break;
					}
				}
				else if (r.NodeType == XmlNodeType.EndElement)
				{
					break;
				}
			}
		}

		public void AddNameValuePair(string name, string value, List<string> enumList)
		{
			valueItem item = new valueItem();
			item.type = "enum";
			item.enumList = enumList;
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

		public void EditNameValuePair(string oldName, string newName, string newValue, string type)
		{
			this.nameValuePairs.Remove(oldName);
			valueItem value = new valueItem();
			value.value = newValue;
			value.type = type;
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
                    if (String.Equals(value.type, "enum") && value.enumList != null && value.enumList.Count != 0)
                    {
                        foreach (string enumElement in value.enumList)
                        {
                            w.WriteAttributeString("Enum", enumElement);
                        }
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
