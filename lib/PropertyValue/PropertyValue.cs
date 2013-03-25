using System;
using System.Collections.Generic;

namespace Multiverse.Toolbox
{
    public abstract class PropertyValue : IComparable
    {
        public object Value;

        public PropertyValue(object val)
        {
            Value = val;
        }

        public abstract int CompareTo(object val);

        public void SetValue(object val)
        {
            Value = val;
        }
   }
}
