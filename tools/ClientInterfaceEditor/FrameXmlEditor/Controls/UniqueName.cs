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
using System.Linq;
using System.Text;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	public class UniqueName
	{
        private const string defaultNameSeed = "Object";

        private static bool IsNameUnique(IEnumerable<string> controlNames, string controlName)
        {
            return !controlNames.Contains(controlName);
        }

		public static string GetUniqueName(string desiredName, string nameSeed, IEnumerable<string> controlNames)
		{
			const int numberingSeed = 1;

			string name = desiredName;

			if (string.IsNullOrEmpty(name))
			{
				name = nameSeed + numberingSeed.ToString();
			}

			if (IsNameUnique(controlNames, name))
			{
				return name;
			}

			string numberPart = string.Empty;
			char actChar = default(char);
			for (int index = name.Length - 1; ((index >= 0) && char.IsNumber(actChar = name[index])); index--)
			{
				numberPart = actChar.ToString() + numberPart;
			}

			string textPart = name.Substring(0, name.Length - numberPart.Length);
			int number = (numberPart.Length > 0) ? int.Parse(numberPart) : numberingSeed;

			while (!IsNameUnique(controlNames, name))
			{
				name = textPart + (number++).ToString();
			}

			return name;

		}
	}
}
