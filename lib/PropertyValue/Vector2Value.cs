using Axiom.MathLib;

namespace Multiverse.Toolbox
{
    public class Vector2Value : PropertyValue
    {
        public Vector2Value(Vector2 val):
            base((object)val)
        {
        }

        override public string ToString()
        {
            return ((Vector2)Value).ToString();
        }

        public Vector2 Parse(string val)
        {
            return Vector2.Parse(val);
        }

        override public int CompareTo(object val)
        {
            float magnitude1 = ((Vector2)Value).x + ((Vector2)Value).y;
            float magnitude2 = ((Vector2)((Vector2Value)val).Value).x + ((Vector2)((Vector2Value)val).Value).y;
            if (magnitude1 == magnitude2)
            {
                if (((Vector2)Value).x != ((Vector2)((Vector2Value)val).Value).x)
                {
                    return (int)(((Vector3)Value).x - ((Vector3)((Vector2Value)val).Value).x);
                }
                if (((Vector2)Value).y != ((Vector2)((Vector2Value)val).Value).y)
                {
                    return (int)(((Vector3)Value).y - ((Vector3)((Vector2Value)val).Value).y);
                }
            }
            else
            {
                return (int)(magnitude1 - magnitude2);
            }
            return 0;
        }

        public Vector2 GetValue()
        {
            return (Vector2)Value;
        }
    }
}
