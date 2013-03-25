namespace Multiverse.Toolbox
{
    public class BooleanValue : PropertyValue
    {
        public BooleanValue(bool val):
            base(val)
        {
        }

        public bool Parse(string val)
        {
            return bool.Parse(val);
        }

        override public string ToString()
        {
            return ((bool)Value).ToString();
        }

        override public int CompareTo(object val)
        {
            if ((bool)Value == (bool)((BooleanValue)val).Value)
            {
                return 0;
            }
            if ((bool)Value)
            {
                return 1;
            }
            return -1;
        }

        public bool GetValue()
        {
            return (bool)Value;
        }
    }
}
