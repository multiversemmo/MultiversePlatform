using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multiverse.Toolbox
{
    public class DoubleValue : PropertyValue
    {
        public DoubleValue(double val):
            base((object)val)
        {
        }

        override public string ToString()
        {
            return ((double)Value).ToString();
        }

        public double Parse(string val)
        {
            return double.Parse(val);
        }
       
        override public int CompareTo(object val)
        {
            return (int)((double)Value - (double)((DoubleValue)val).Value);
        }

        public double GetValue()
        {
            return (double)Value;
        }
    }
}
