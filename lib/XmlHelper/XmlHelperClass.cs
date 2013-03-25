using System;
using System.Collections.Generic;
using System.Text;
using Axiom.MathLib;
using Axiom.Core;
using System.Xml;

namespace Multiverse.ToolBox
{
	public class XmlHelperClass
	{
		public static Vector3 ParseVectorAttributes(XmlReader r)
		{
			float x = 0;
			float y = 0;
			float z = 0;

			for (int i = 0; i < r.AttributeCount; i++)
			{
				r.MoveToAttribute(i);

				// set the field in this object based on the element we just read
				switch (r.Name)
				{
					case "x":
						x = float.Parse(r.Value);
						break;
					case "y":
						y = float.Parse(r.Value);
						break;
					case "z":
						z = float.Parse(r.Value);
						break;
				}
			}
			r.MoveToElement(); //Moves the reader back to the element node.

			return new Vector3(x, y, z);
		}

        public static void WriteVectorElement(XmlWriter w, string elementName, Vector3 v)
        {
            w.WriteStartElement(elementName);
            w.WriteAttributeString("x", v.x.ToString());
            w.WriteAttributeString("y", v.y.ToString());
            w.WriteAttributeString("z", v.z.ToString());
            w.WriteEndElement();
        }

        public static ColorEx ParseColorAttributes(XmlReader r)
        {
            float R = 0;
            float G = 0;
            float B = 0;

            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "R":
                        R = float.Parse(r.Value);
                        break;
                    case "G":
                        G = float.Parse(r.Value);
                        break;
                    case "B":
                        B = float.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.

            r.MoveToElement(); //Moves the reader back to the element node.
            if (R > 1 || G > 1 || B > 1)
            {
                R = R / 255;
                G = G / 255;
                B = B / 255;
            }
            return new ColorEx(R, G, B);
        }

		public static string ParseTextNode(XmlReader r)
		{
			// read the value
			r.Read();
			if (r.NodeType != XmlNodeType.Text)
			{
				return (null);
			}
			string ret = r.Value;

			// error out if we dont see an end element here
			r.Read();
			if (r.NodeType != XmlNodeType.EndElement)
			{
				// XXX - should generate an exception here?
				return (null);
			}

			return (ret);
		}

		public static Quaternion ParseQuaternion(XmlReader r)
		{
			float w = 0;
			float x = 0;
			float y = 0;
			float z = 0;

			for (int i = 0; i < r.AttributeCount; i++)
			{
				r.MoveToAttribute(i);

				// set the field in this object based on the element we just read
				switch (r.Name)
				{

					case "w":
						w = float.Parse(r.Value);
						break;
					case "x":
						x = float.Parse(r.Value);
						break;
					case "y":
						y = float.Parse(r.Value);
						break;
					case "z":
						z = float.Parse(r.Value);
						break;
				}
			}
			r.MoveToElement(); //Moves the reader back to the element node.

			return new Quaternion(w, x, y, z);
		}
	}
}
