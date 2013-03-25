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
using System.Drawing;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.Windows.Forms;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	/// <summary>
	/// Extension methods for serialization objects
	/// </summary>
    public static class SerializationObjectExtensions
    {

		public static string ToStringValue(this FRAMEPOINT framePoint)
		{
			string result = framePoint.ToString();
			switch (framePoint)
			{
				case FRAMEPOINT.LEFT:
				case FRAMEPOINT.RIGHT:
					result = "CENTER" + result;
					break;
				case FRAMEPOINT.TOP:
				case FRAMEPOINT.BOTTOM:
					result += "CENTER";
					break;
			}
			return
				result;
		}
    }
}
