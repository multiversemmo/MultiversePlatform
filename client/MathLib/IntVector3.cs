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

#region Using directives

using System;
using System.Text;

using Axiom.MathLib;

#endregion

namespace Multiverse.MathLib
{
    public class IntVector3
    {
        public int x;
        public int y;
        public int z;

        public IntVector3() {
        }

        public IntVector3(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public IntVector3(Vector3 vec) {
            this.x = (int)vec.x;
            this.y = (int)vec.y;
            this.z = (int)vec.z;
		}

		public Vector3 ToVector3() {
			return new Vector3(x, y, z);
		}

        public override string ToString() {
            return string.Format("IntVector3({0}, {1}, {2})", x, y, z);
        }
    }
}
