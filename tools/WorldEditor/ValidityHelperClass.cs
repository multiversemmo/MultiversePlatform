/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Multiverse.Tools.WorldEditor
{
	public class ValidityHelperClass
	{
		public static bool isInt(string number)
		{
			int i;
			bool rv = int.TryParse(number, out i);
			if (false)
			{
				DialogResult result = MessageBox.Show("This box requires an integer numeric value", "Oops" , MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return rv;
		}

		public static bool isUint(string number)
		{
			uint i;
			bool rv = uint.TryParse(number, out i);
			if (false)
			{
				DialogResult result = MessageBox.Show("This box requires an unsigned integer numeric value", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return rv;

		}
		public static bool isFloat(string number)
		{
			float f;
			bool rv = float.TryParse(number, out f);
			if (false)
			{
				DialogResult result = MessageBox.Show("This box requires a floating point numeric value", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return rv;
		}

        public static bool assetExists(string filename)
        {
            string f;
            bool rv = WorldEditor.Instance.CheckAssetFileExists(filename);
            return rv;
        }
	}
}
