using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axiom.MathLib;

namespace Multiverse.Toolbox
{
    public class QuaternionValue : PropertyValue
    {
        public QuaternionValue(Quaternion val):
            base((object)val)
        {
        }
        
        override public string ToString()
        {
            return ((Quaternion)Value).ToString();
        }

        public Quaternion Parse(string val)
        {
            return Quaternion.Parse(val);
        }

        override public int CompareTo(object val)
        {
            float magnitude1 = ((Quaternion)Value).x + ((Quaternion)Value).y + ((Quaternion)Value).z + ((Quaternion)Value).w;
            float magnitude2 = ((Quaternion)(((QuaternionValue)val).Value)).x + ((Quaternion)((QuaternionValue)val).Value).y + ((Quaternion)((QuaternionValue)val).Value).z + ((Quaternion)((QuaternionValue)val).Value).w;
            if(magnitude1 == magnitude2)
            {
                if (((Quaternion)Value).x != ((Quaternion)((QuaternionValue)val).Value).x)
                {
                    return (int)(((Quaternion)Value).x - ((Quaternion)((QuaternionValue)val).Value).x);
                }
                if (((Quaternion)Value).y != ((Quaternion)((QuaternionValue)val).Value).y)
                {
                    return (int)(((Quaternion)Value).y - ((Quaternion)((QuaternionValue)val).Value).y);
                }
                if (((Quaternion)Value).z != ((Quaternion)((QuaternionValue)val).Value).z)
                {
                    return (int)(((Quaternion)Value).z - ((Quaternion)((QuaternionValue)val).Value).z);
                }
                if (((Quaternion)Value).w != ((Quaternion)((QuaternionValue)val).Value).w)
                {
                    return (int)(((Quaternion)Value).w - ((Quaternion)((QuaternionValue)val).Value).w);
                }
            }
            else
            {
                return (int)(magnitude1 - magnitude2);
            }
            return 0;
        }

        public Quaternion GetValue()
        {
            return (Quaternion)Value;
        }
    }
}
