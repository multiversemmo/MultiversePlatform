
namespace Multiverse.Toolbox
{
    public class FloatValue : PropertyValue
    {
        public FloatValue(float val):
            base((object)val)
        {
        }

        override public string ToString()
        {
            return ((float)Value).ToString();
        }

        public float Parse(string val)
        {
            return float.Parse(val);
        }
       
        override public int CompareTo(object val)
        {
            return (int)((float)Value -(float)((FloatValue)val).Value);
        }

        public float GetValue()
        {
            return (float)Value;
        }
    }
}
