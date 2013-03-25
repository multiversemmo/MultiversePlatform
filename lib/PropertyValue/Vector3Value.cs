using Axiom.MathLib;

namespace Multiverse.Toolbox
{
    public class Vector3Value : PropertyValue
    {
        public Vector3Value(Vector3 val):
            base((object)val)
        {
        }

        override public string ToString()
        {
            return ((Vector3)Value).ToString();
        }

        public Vector3 Parse(string val)
        {
            return Vector3.Parse(val);
        }

        override public int CompareTo(object val)
        {
            float magnitude1 = ((Vector3)Value).x + ((Vector3)Value).y + ((Vector3)Value).z;
            float magnitude2 = ((Vector3)((Vector3Value)val).Value).x + ((Vector3)((Vector3Value)val).Value).y + ((Vector3)((Vector3Value)val).Value).z;
            if(magnitude1 == magnitude2)
            {
                if (((Vector3)Value).x != ((Vector3)((Vector3Value)val).Value).x)
                {
                    return (int)(((Vector3)Value).x - ((Vector3)((Vector3Value)val).Value).x);
                }
                if (((Vector3)Value).y != ((Vector3)((Vector3Value)val).Value).y)
                {
                    return (int)(((Vector3)Value).y - ((Vector3)((Vector3Value)val).Value).y);
                }
                if (((Vector3)Value).z != ((Vector3)((Vector3Value)val).Value).z)
                {
                    return (int)(((Vector3)Value).z - ((Vector3)((Vector3Value)val).Value).z);
                }
            }
            else
            {
                return (int)(magnitude1 - magnitude2);
            }
            return 0;
        }

        public Vector3 GetValue()
        {
            return (Vector3)Value;
        }
    }
}
