using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multiverse.Toolbox
{
    public class IntValue : PropertyValue
    {
        public IntValue(int val):
            base((object)val)
        {
        }

        override public string ToString()
        {
            return ((int)Value).ToString();
        }

        public int Parse(string val)
        {
            return int.Parse(val);
        }
       
        override public int CompareTo(object val)
        {
            return (int)Value - (int)((IntValue)val).Value;
        }

        public int GetValue()
        {
            return (int)Value;
        }
    }
}
