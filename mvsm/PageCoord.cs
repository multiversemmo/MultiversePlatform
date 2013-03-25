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
using Axiom.MathLib;

namespace Axiom.SceneManagers.Multiverse
{
	/// <summary>
	/// Represents a coordinate within page space
	/// </summary>
	public struct PageCoord
	{
		private int x;
		private int z;

		public PageCoord(Vector3 location, int pageSize)
		{
			double pageSizeWorld = pageSize * TerrainManager.oneMeter;
			x = (int)Math.Floor(location.x / pageSizeWorld);
			z = (int)Math.Floor(location.z / pageSizeWorld);
		}

		public PageCoord(int x, int z)
		{
			this.x = x;
			this.z = z;
		}

        public override String ToString()
        {
            return String.Format("({0}, {1})", x, z);
        }

		public int X 
		{
			get 
			{
				return x;
			}
		}

		public int Z
		{
			get 
			{
				return z;
			}
		}

		/// <summary>
		/// Compute the "distance" between two pages.  Returns the axis aligned
		/// distance along X or Z (whichever is greater).
		/// </summary>
		/// <param name="c2">coordinate to compare to this</param>
		/// <returns></returns>
		public int Distance(PageCoord c2)
		{
			int xDiff = c2.X - X;
			int zDiff = c2.Z - Z;

			if ( xDiff < 0 ) 
			{
				xDiff = -xDiff;
			}

			if ( zDiff < 0 ) 
			{
				zDiff = -zDiff;
			}

			if ( xDiff > zDiff ) 
			{
				return xDiff;
			} 
			else 
			{
				return zDiff;
			}
		}

		public Vector3 WorldLocation(int pageSize)
		{
			return new Vector3(x * pageSize * TerrainManager.oneMeter, 0, z * pageSize * TerrainManager.oneMeter);
		}

		/// <summary>
		///		Used to subtract a PageCoord from another PageCoord.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static PageCoord Subtract (PageCoord left, PageCoord right) 
		{
			return left - right;
		}
        
		/// <summary>
		///		Used to subtract a PageCoord from another PageCoord.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static PageCoord operator - (PageCoord left, PageCoord right) 
		{
			return new PageCoord(left.x - right.x, left.z - right.z);
		}


		/// <summary>
		///		Used to negate the elements of a PageCoord.
		/// </summary>
		/// <param name="left"></param>
		/// <returns></returns>
		public static PageCoord Negate (PageCoord left) 
		{
			return -left;
		}
        
		/// <summary>
		///		Used to negate the elements of a PageCoord.
		/// </summary>
		/// <param name="left"></param>
		/// <returns></returns>
		public static PageCoord operator - (PageCoord left) 
		{
			return new PageCoord(-left.x, -left.z);
		}

		/// <summary>
		///		Used when a PageCoord is added to another PageCoord.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static PageCoord Add (PageCoord left, PageCoord right) 
		{
			return left + right;
		}
        
		/// <summary>
		///		Used when a PageCoord is added to another PageCoord.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static PageCoord operator + (PageCoord left, PageCoord right) 
		{
			return new PageCoord(left.x + right.x, left.z + right.z);
		}

		/// <summary>
		///		User to compare two PageCoord instances for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns>true or false</returns>
		public static bool operator == (PageCoord left, PageCoord right) 
		{
			return (left.x == right.x && left.z == right.z);
		}

		/// <summary>
		///		User to compare two PageCoord instances for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns>true or false</returns>
		public static bool operator != (PageCoord left, PageCoord right) 
		{
			return (left.x != right.x || left.z != right.z);
		}

		/// <summary>
		///		Compares this PageCoord to another object.  This should be done because the 
		///		equality operators (==, !=) have been overriden by this class.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj) 
		{
			if(obj is PageCoord)
				return (this == (PageCoord)obj);
			else
				return false;
		}

        /// <summary>
        ///     Gets a unique identifier for this object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (X & 0xffff) | ((Z | 0xffff) << 16);
        }
	}
}
