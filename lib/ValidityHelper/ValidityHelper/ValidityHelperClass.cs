using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Axiom.Core;

namespace Multiverse.ToolBox
{
	public class ValidityHelperClass
	{
		public static bool isInt(string number)
		{
			int i;
			bool rv = int.TryParse(number, out i);
			if (false)
			{
				// DialogResult result = MessageBox.Show("This box requires an integer numeric value", "Oops" , MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return rv;
		}

		public static bool isUint(string number)
		{
			uint i;
			bool rv = uint.TryParse(number, out i);
			if (false)
			{
				// DialogResult result = MessageBox.Show("This box requires an unsigned integer numeric value", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return rv;

		}
		public static bool isFloat(string number)
		{
			float f;
			bool rv = float.TryParse(number, out f);
			if (false)
			{
				// DialogResult result = MessageBox.Show("This box requires a floating point numeric value", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return rv;
		}

        public static bool isBoolean(string boolean)
        {
            bool b;
            bool rv = bool.TryParse(boolean, out b);
            return rv;
        }

        public static bool assetExists(string filename)
        {
            return ResourceManager.HasCommonResourceData(filename);
        }
	}
}
