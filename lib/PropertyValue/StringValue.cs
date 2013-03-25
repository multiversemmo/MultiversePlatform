using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multiverse.Toolbox
{
    public class StringValue : PropertyValue
    {
        public StringValue(string val):
            base((object)val)
        {
        }

        override public string ToString()
        {
            return (string)Value;
        }

        public string Parse(string val)
        {
            return (string)val;
        }
       
        override public int CompareTo(object val)
        {
            return String.Compare((string)Value, (string)((StringValue)val).Value);
        }

        public string GetValue()
        {
            return (string)Value;
        }
    }
}
