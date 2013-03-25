using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axiom.Core;

namespace Multiverse.Toolbox
{
    public class ColorValue : PropertyValue
    {
        public ColorValue(ColorEx val) :
            base((object)val)
        {
        }

                
        override public string ToString()
        {
            return ((ColorEx)Value).ToString();
        }

        public ColorEx Parse(string val)
        {
            string commaSeparatedList = val.Substring(1, val.Length - 2);
            int startIndexR = 0;
            int rLength = commaSeparatedList.IndexOf(',');
            int startIndexG = rLength + 1;
            int gLength = commaSeparatedList.IndexOf(',', startIndexG) - startIndexG;
            int startIndexB = startIndexG + gLength + 1;
            int bLength = commaSeparatedList.IndexOf(',', startIndexB) - startIndexB;
            int startIndexA = startIndexB + bLength + 1;
            string r = commaSeparatedList.Substring(startIndexR, rLength);
            string g = commaSeparatedList.Substring(startIndexG, gLength);
            string b = commaSeparatedList.Substring(startIndexB, bLength);
            string a = commaSeparatedList.Substring(startIndexA);
            return new ColorEx(float.Parse(a), float.Parse(r), float.Parse(g), float.Parse(b));
        }

        override public int CompareTo(object val)
        {
            float magnitude1 = ((ColorEx)Value).r + ((ColorEx)Value).g + ((ColorEx)Value).b + ((ColorEx)Value).a;
            float magnitude2 = ((ColorEx)((ColorValue)val).Value).r + ((ColorEx)((ColorValue)val).Value).g + ((ColorEx)((ColorValue)val).Value).b + ((ColorEx)((ColorValue)val).Value).a;
            if(magnitude1 == magnitude2)
            {
                if (((ColorEx)Value).r != ((ColorEx)((ColorValue)val).Value).r)
                {
                    return (int)(((ColorEx)Value).r - ((ColorEx)((ColorValue)val).Value).r);
                }
                if (((ColorEx)Value).g != ((ColorEx)((ColorValue)val).Value).g)
                {
                    return (int)(((ColorEx)Value).g - ((ColorEx)((ColorValue)val).Value).g);
                }
                if (((ColorEx)Value).b != ((ColorEx)((ColorValue)val).Value).b)
                {
                    return (int)(((ColorEx)Value).b - ((ColorEx)((ColorValue)val).Value).b);
                }
                if (((ColorEx)Value).a != ((ColorEx)((ColorValue)val).Value).a)
                {
                    return (int)(((ColorEx)Value).a - ((ColorEx)((ColorValue)val).Value).a);
                }
            }
            else
            {
                return (int)(magnitude1 - magnitude2);
            }
            return 0;
        }

        public ColorEx GetValue()
        {
            return (ColorEx)Value;
        }
    }
}
