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



using Axiom.Core;

using Axiom.MathLib;

using Axiom.Collections;

using Axiom.Media;

using Axiom.Graphics;



using Axiom.SceneManagers.PagingLandscape.Collections;

using Axiom.SceneManagers.PagingLandscape.Tile;



#endregion Using Directives



namespace Axiom.SceneManagers.PagingLandscape.Texture

{

	/// <summary>

	/// Summary description for TextureManager.

	/// </summary>

	public class TextureManager: IDisposable

	{


		#region Fields
		protected TexturePages textures;
		#endregion Fields

		#region Singleton Implementation

		

		/// <summary>

		/// Constructor

		/// </summary>

		private TextureManager() 

		{

			long w = Options.Instance.World_Width;
			long h = Options.Instance.World_Height;

			long i, j;
			//Setup the page array
		//    mTexture.reserve (w);	
		//    mTexture.resize (w);	
			textures = new TexturePages();
			for ( i = 0; i < w; i++)
			{
				TextureRow tr = new TextureRow();
				textures.Add( tr );
		       
		//        mTexture[i].reserve (h);	
		//        mTexture[i].resize (h);		
				for (j = 0; j < h; j++ ) tr.Add( null );
			}
			//Populate the page array
			if ( Options.Instance.TextureFormat == "Image" )
			{
				for ( j = 0; j < h; j++ )
				{
					for ( i = 0; i < w; i++ )
					{
                        textures[i][j] = new Texture_Image();

                    }
				}
			} else if ( Options.Instance.TextureFormat == "Splatting5" )
			{
				for ( j = 0; j < h; j++ )
				{
					for ( i = 0; i < w; i++ )
					{
						textures[i][j] = new Texture_Splatting5();

					}
				}
			}

		}





		private static TextureManager instance = null;



		public static TextureManager Instance 

		{

			get 

			{

				if ( instance == null ) instance = new TextureManager();

				return instance;

			}

		}





		#endregion Singleton Implementation



		#region IDisposable Implementation



		public void Dispose()

		{

			if (instance == this) 

			{

				textures.Clear();

				instance = null;

			}

		}



		#endregion IDisposable Implementation


		#region Methods

		public void Load(  long dataX,  long dataZ )
		{
			Texture data = textures[ dataX ][ dataZ ];
			if ( !data.IsLoaded )
			{
				data.Load( dataX, dataZ );
			}
		}

		public void Unload(  long dataX,  long dataZ )
		{
			Texture data = textures[ dataX ][ dataZ ];
			if ( data.IsLoaded )
			{
				data.Unload();
			}
		}

		public bool IsLoaded(  long dataX,  long dataZ )
		{
			return textures[ dataX ][ dataZ ].IsLoaded;
		}

		public Material GetMaterial(  long dataX,  long dataZ )
		{
			return textures[ dataX ][ dataZ ].Material;
		}


		#endregion Methods
	};
}

