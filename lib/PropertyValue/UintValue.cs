using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multiverse.Toolbox
{
    public class UintValue : PropertyValue
    {
        public UintValue(uint val):
            base((object)val)
        {
        }

        override public string ToString()
        {
            return ((uint)Value).ToString();
        }

        public uint Parse(string val)
        {
            return uint.Parse(val);
        }
       
        override public int CompareTo(object val)
        {
            return (int)((int)Value - (int)((UintValue)val).Value);
        }

        public uint GetValue()
        {
            return (uint)Value;
        }
    }
}
