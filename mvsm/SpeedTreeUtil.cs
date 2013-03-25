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
using System.Diagnostics;
using Axiom.Core;
using Axiom.MathLib;
using Multiverse;

namespace Axiom.SceneManagers.Multiverse
{
	/// <summary>
	/// Summary description for SpeedTreeUtil.
	/// </summary>
	public class SpeedTreeUtil
	{
		private SpeedTreeUtil()
		{
		}

		public static V4 ToSpeedTree(Vector4 v)
		{
			return new V4(v.x, v.y, v.z, v.w);
		}

		public static V3 ToSpeedTree(Vector3 v)
		{
			return new V3(v.x, v.y, v.z);
		}

		public static Color ToSpeedTree(ColorEx c)
		{
			return new Color(c.r, c.g, c.b);
		}

		public static TreeBox ToSpeedTree(AxisAlignedBox box)
		{
			return new TreeBox(ToSpeedTree(box.Minimum), ToSpeedTree(box.Maximum));
		}

		public static float [] ToSpeedTree(Matrix4 m)
		{
			float [] ret = new float[16];

			ret[0] = m.m00;
			ret[1] = m.m10;
			ret[2] = m.m20;
			ret[3] = m.m30;

			ret[4] = m.m01;
			ret[5] = m.m11;
			ret[6] = m.m21;
			ret[7] = m.m31;

			ret[8] = m.m02;
			ret[9] = m.m12;
			ret[10] = m.m22;
			ret[11] = m.m32;

			ret[12] = m.m03;
			ret[13] = m.m13;
			ret[14] = m.m23;
			ret[15] = m.m33;

			return ret;
		}

		public static Vector4 FromSpeedTree(V4 v)
		{
			return new Vector4(v.x, v.y, v.z, v.w);
		}

		public static Vector3 FromSpeedTree(V3 v)
		{
			return new Vector3(v.x, v.y, v.z);
		}

		public static ColorEx FromSpeedTree(Color c)
		{
			return new ColorEx(c.r, c.g, c.b);
		}

		public static Matrix4 FromSpeedTree(float [] m)
		{
			Matrix4 ret;
			Debug.Assert(m.Length == 16, "Matrix4FromSpeedTree: Input array not 16 long");

			ret.m00 = m[0];
			ret.m10 = m[1];
			ret.m20 = m[2];
			ret.m30 = m[3];

			ret.m01 = m[4];
			ret.m11 = m[5];
			ret.m21 = m[6];
			ret.m31 = m[7];

			ret.m02 = m[8];
			ret.m12 = m[9];
			ret.m22 = m[10];
			ret.m32 = m[11];

			ret.m03 = m[12];
			ret.m13 = m[13];
			ret.m23 = m[14];
			ret.m33 = m[15];

			return ret;
		}

		public static AxisAlignedBox FromSpeedTree(TreeBox box)
		{
			return new AxisAlignedBox(FromSpeedTree(box.min), FromSpeedTree(box.max));
		}
	}
}
