#region LGPL License

/*

Axiom Game Engine Library

Copyright (C) 2003  Axiom Project Team



The overall design, and a majority of the core engine and rendering code 

contained within this library is a derivative of the open source Object Oriented 

Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  

Many thanks to the OGRE team for maintaining such a high quality project.



This library is free software; you can redistribute it and/or

modify it under the terms of the GNU Lesser General Public

License as published by the Free Software Foundation; either

version 2.1 of the License, or (at your option) any later version.



This library is distributed in the hope that it will be useful,

but WITHOUT ANY WARRANTY; without even the implied warranty of

MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU

Lesser General Public License for more details.



You should have received a copy of the GNU Lesser General Public

License along with this library; if not, write to the Free Software

Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

*/

#endregion LGPL License



#region Using Directives



using System;

using System.Collections;

using System.Diagnostics;



using Axiom.Core;

using Axiom.MathLib;

using Axiom.Collections;

using Axiom.Media;



using Axiom.SceneManagers.PagingLandscape.Collections;

using Axiom.SceneManagers.PagingLandscape.Tile;



#endregion Using Directives



namespace Axiom.SceneManagers.PagingLandscape.Data2D

{

	/// <summary>

	/// Summary description for Data2D.

	/// </summary>

	public abstract class Data2D : IDisposable

	{

		#region Fields

		//  computed Height Data  (scaled)
		protected float[] heightData;

		//  maximum position in Array
		protected long maxArrayPos;

		//  data side maximum size
		protected long size;

		//  image data  maximum size
		protected long max;

		// maximum page/data2d height. (scaled)
		protected float maxheight;

		// if data loaded or not
		protected bool isLoaded;

		

		protected ArrayList newHeight;

		protected bool dynamic;


		#endregion Fields



		public Data2D() 

		{

			dynamic = false;

			isLoaded = false;
			heightData = null;
			newHeight = new ArrayList();
		}



		public bool Dynamic 

		{

			get 

			{

				return dynamic;

			}

		}



		#region IDisposable Members



		public virtual void Dispose()

		{

			if (heightData != null)
				heightData = null;
			newHeight.Clear();
			isLoaded = false;		

		}



		#endregion



		public virtual void Load(  float X,  float Z)
		{
			isLoaded = true;
			load(X, Z);
		}

		public virtual void Load()
		{
			isLoaded = true;
			load();
		}

		public virtual void Load(Image NewHeightmap)

		{
			isLoaded = true;
			load(NewHeightmap);
		}

		public virtual void Unload()
		{
			isLoaded = false;
			unload();
		}

		/**
		*    Method that deform Height Data of the terrain.
		* \param &deformationPoint 
		*       Where modification is, in world coordinates
		* \param &modificationHeight 
		*        What modification do to terrain
		* \param info 
		*        Give some info on tile to 
		*       help coordinate system change
		*/
		public float DeformHeight( Vector3 deformationPoint, float modificationHeight, TileInfo info)
		{
			if ( heightData != null )
			{
				int pSize = (int)Options.Instance.PageSize;

				// adjust x and z to be local to page
				int x = (int) ((deformationPoint.x ) 
						- ((info.PageX - Options.Instance.World_Width * 0.5f) * (pSize)));
				int z = (int) ((deformationPoint.z ) 
						- ((info.PageZ - Options.Instance.World_Height * 0.5f) * (pSize)));

				long arraypos = (long)(z * size + x);

				if (arraypos < maxArrayPos)
				{
					if ((heightData[arraypos] - modificationHeight) > 0.0f)
						heightData[arraypos] -= modificationHeight;
					else
						heightData[arraypos] = 0.0f;
					return heightData[arraypos];
				}
			}
			return 0.0f;
		}

		/**
		*
		*    Method that deform Height Data of the terrain.
		* \param &x 
		*       x Position on 2d height grid
		* \param &z 
		*       z Position on 2d height grid
		* \param &modificationHeight 
		*        What modification do to terrain
		*/
		public float DeformHeight(long x, long z, float modificationHeight)
		{
			if ( heightData != null)
			{
				long arraypos = z * size + x;

				if (arraypos < maxArrayPos)
				{
					if ((heightData[arraypos] - modificationHeight) > 0.0f)
						heightData[arraypos] -= modificationHeight;
					else
						heightData[arraypos] = 0.0f;
					return heightData[arraypos];
				}
			}
			return 0.0f;
		}


		public virtual Vector3 GetNormalAt (float mX, float mZ)

		{

			return Vector3.UnitY;

		}

		public virtual ColorEx GetBase (float mX, float mZ)
		{
			return ColorEx.White;
		}

		public virtual ColorEx GetCoverage (float mX, float mZ)
		{
			return ColorEx.White;
		}

		public float GetHeightAbsolute(float x, float z, TileInfo info)
		{
			if ( heightData != null)
			{
				int pSize = (int)Options.Instance.PageSize;
				Vector3 scale = Options.Instance.Scale;

				// adjust x and z to be local to page
				int i_x = ((int)(x / scale.x) 
					- ((int)(info.PageX - Options.Instance.World_Width * 0.5f) * (pSize)));
				int i_z = ((int)(z / scale.z) 
					- ((int)(info.PageZ - Options.Instance.World_Height * 0.5f) * (pSize)));

				long arraypos = (long)(i_z * size + i_x); 
				if (arraypos < maxArrayPos)
					return heightData[arraypos];
			}
			return 0.0f;
		}

		public virtual float GetHeight( float x, float z )
		{
			if ( heightData != null )
			{
				long Pos = (long) (( z * size )+ x);
				if ( maxArrayPos > Pos )
					return heightData[ Pos ];
			}
			return 0.0f;
		}

		public virtual float GetHeight(  long x, long z )
		{
			if ( heightData != null)
			{
				long Pos = z * size + x;
				if ( maxArrayPos > Pos )
					return heightData[ Pos ];
			}
			return 0.0f;
		}

		public virtual float GetHeight( int x, int z )
		{
			if ( heightData != null )
			{
				int Pos = z * (int)size + x;
				if ( maxArrayPos > (long) Pos )
					return heightData[ Pos ];
			}
			return 0.0f;
		}

		public void SetHeight( long x, long z, float h )
		{
			if ( heightData != null )
			{
				long Pos = ( z * size ) + x;
				if ( maxArrayPos > Pos )
					heightData[ Pos ] = h;
			}
		}
	    
		public float MaxHeight
		{
			get
			{
				return maxheight;
			}
			set
			{
				maxheight = value;
			}
		}


		public float[] HeightData
		{
			get
			{
				return heightData;
			}
		}

		public bool IsLoaded
		{
			get
			{
				return isLoaded;
			}
		}


		public bool AddNewHeight(Sphere NewHeight)
		{
			//std::vector<Sphere>::iterator cur, end = mNewHeight.end();
			//for( cur = mNewHeight.begin(); cur < end; cur++ )
			for (int index= 0; index < newHeight.Count; index ++)
			{
				if( ((Sphere)newHeight[index]).Intersects( NewHeight ) == true )
				{
					// We don´t allow to heights to intersect
					return false;
				}
			}
			newHeight.Add(NewHeight);
			return true;
		}

		public bool RemoveNewHeight(Sphere oldHeight)
		{
			//std::vector<Sphere>::iterator cur, end = mNewHeight.end();
			//for( cur = mNewHeight.begin(); cur < end; cur++ )
			for (int index= 0; index < newHeight.Count; index ++)
			{
				if( ((Sphere)newHeight[index]).Intersects( oldHeight ) == true )
				{
					// Since we don´t allow to heights to intersect we can delete this one
					newHeight.RemoveAt(index);
					return true;
				}
			}
			return false;
		}



		protected abstract void load(float X, float Z);
		protected abstract void load(Image NewHeightmap);
		protected abstract void load();
		protected abstract void unload();



		protected bool checkSize( int s )
		{
			for ( int i = 0; i < 256; i++ ) 
			{
				//printf( "Checking...%d\n", ( 1 << i ) + 1 );
				if ( s == ( 1 << i ) + 1 ) 
				{
					return true;
				}
			}
			return false;
		}

	}

}

