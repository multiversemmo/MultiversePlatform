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

using Axiom.Collections;

using Axiom.MathLib;

using Axiom.Media;



//using Axiom.SceneManagers.IPLSceneManager.Page;

//using Axiom.SceneManagers.IPLSceneManager.Query;



using Axiom.SceneManagers.PagingLandscape.Collections;



#endregion Using Directives



namespace Axiom.SceneManagers.PagingLandscape.Data2D

{

	/// <summary>

	/// A specialized class for loading 2D Data from a HeightField file.

	/// </summary>

	public class Data2D_HeightField: Data2D

	{

		#region Fields



		private Image image;
		private Image coverage;
		private Image baseImg;

		private long bpp;// should be 4 bytes (image is RGBA)

		#endregion Fields

		#region Constructor

		public Data2D_HeightField() : base()

		{

			image = null;
			coverage = null;
			baseImg = null;
			maxheight = 256.0f * Options.Instance.Scale.y;
		}



		#endregion Constructor



		#region IDisposable Members



		public override void Dispose()

		{

			if ( image != null )
				image = null;
			if ( coverage != null )
				coverage= null;
			if ( baseImg != null )
				baseImg = null; 
			base.Dispose();

		}



		#endregion



		public override Vector3 GetNormalAt (float X, float Z)
		{
			if ( image != null )
			{
				long Pos = (long) (( Z * this.size ) * this.bpp + X * this.bpp);//4 bytes (mImage is RGBA)
				if ( this.max > Pos )
				{
					const float normalscale = 1.0f / 127.0f;
					long numVertices = 0;
//					float normalVal = ((float)(image.Data[Pos + 0]) - 128.0f) * normalscale;

					float normalVal = 0;
					if ( Pos - ( (this.size + 1) * this.bpp ) > 0 ) { normalVal += image.Data[Pos - ( (this.size  +1 )* this.bpp )]; numVertices++; }
					if ( Pos - ( this.size * this.bpp ) > 0 ) { normalVal += image.Data[Pos - ( this.size * this.bpp )]; numVertices++; }
					if ( Pos - ( (this.size - 1) * this.bpp ) > 0 ) { normalVal += image.Data[Pos - ( (this.size - 1) * this.bpp )]; numVertices++; }

					if ( Pos - this.bpp > 0) { normalVal += image.Data[Pos - this.bpp]; numVertices++; }
					normalVal += image.Data[Pos]; numVertices ++;
					if ( Pos + this.bpp < image.Size) { normalVal += image.Data[Pos + this.bpp]; numVertices++; }

					if ( Pos + ( (this.size - 1) * this.bpp ) < image.Size ) { normalVal += image.Data[Pos + ( (this.size -1) * this.bpp )]; numVertices++; }
					if ( Pos + ( this.size * this.bpp ) < image.Size ) { normalVal += image.Data[Pos + ( this.size * this.bpp )]; numVertices++; }
					if ( Pos + ( (this.size + 1) * this.bpp ) < image.Size ) { normalVal += image.Data[Pos + ( (this.size +1)* this.bpp )]; numVertices++; }

					normalVal /= numVertices;
					normalVal -= 128.0F;
					normalVal *= normalscale;
					return new Vector3 (normalVal, normalVal, normalVal );
				}
				else
				{
					return Vector3.UnitY;
				}	
			}
			else
			{
				return Vector3.UnitY;
			}	
		}


		public override ColorEx GetBase (float X, float Z)
		{
			if ( baseImg != null )
			{
				long Pos = (long) (( Z * (baseImg.Width) )*4 + X*4);//4 bytes (mImage is RGBA)
				if ( baseImg.Size > Pos )
				{
					float divider = 1.0f / 255.0f;
					return new ColorEx( (float) baseImg.Data[ Pos + 0] * divider,
										(float) baseImg.Data[ Pos + 1] * divider,
										(float) baseImg.Data[ Pos + 2] * divider,
										(float) baseImg.Data[ Pos + 3] * divider);
				}
				else
				{	
					return ColorEx.White;
				}
			}
			else
			{
				return ColorEx.White;
			}
		}


		public override ColorEx GetCoverage (float X, float Z)
		{
			if ( coverage != null )
			{
				long Pos = (long) (( Z * (coverage.Width) )*4 + X*4);//4 bytes (mImage is RGBA)
				if ( coverage.Size > Pos )
				{
					float divider = 1.0f / 255.0f;
					return new ColorEx( (float) coverage.Data[ Pos + 0] * divider,
										(float) coverage.Data[ Pos + 1] * divider,
										(float) coverage.Data[ Pos + 2] * divider,
										(float) coverage.Data[ Pos + 3] * divider);
				}
				else
				{	
					return ColorEx.White;
				}
			}
			else
			{
				return ColorEx.White;
			}
		}


		protected override void load(float X, float Z)
		{
			if ( image == null )
			{
				image = Image.FromFile( Options.Instance.Landscape_Filename + ".HN." + Z.ToString() + "." + 
						X.ToString()  + "." + Options.Instance.Landscape_Extension );
			    
				//check to make sure it's 2^n + 1 size.
				if ( !this.checkSize(image.Height) ||	!this.checkSize( image.Width ) )
				{
				string err = "Error: Invalid heightmap size : " +
					 image.Width.ToString()  +
					"," + image.Height.ToString() +
					". Should be 2^n+1, 2^n+1";

					throw new AxiomException( err );
				}
			    
				this.bpp = (long)Image.GetNumElemBytes( image.Format );
				if ( this.bpp != 4 )
				{
					throw new AxiomException("Error: Image is not a RGBA image.(4 bytes, 32 bits)");
				}
			    
			    
				this.size = Options.Instance.PageSize;
				if ( this.size != image.Width )
				{
					throw new AxiomException("Error: Declared World size <> Height Map Size.");
				}
				this.max = (long)(this.size * image.Height * this.bpp + 1);
			    
				if (Options.Instance.Coverage_Vertex_Color)
				{ 
					//coverage = new Image();
					coverage = Image.FromFile ( Options.Instance.Landscape_Filename + 
							".Coverage." + 
							Z.ToString() + "." + 
							X.ToString() + "." +		
							Options.Instance.Landscape_Extension );
				
				}
				if (Options.Instance.Base_Vertex_Color)
				{
					//baseImg = new Image();
					baseImg = Image.FromFile( Options.Instance.Landscape_Filename + 
						".Base." + 
						Z.ToString() + "." + 
						X.ToString() + "." +		
						Options.Instance.Landscape_Extension );
				}
			    
				maxArrayPos = (long) (this.size * image.Height);
				heightData = new float[maxArrayPos];
				long j = 0;
				float scale = Options.Instance.Scale.y;
				maxheight = 0.0f;
				for (long i = 0; i < this.max - 1;  i += this.bpp )
				{  
				float h =  (float) (image.Data[ i + (this.bpp - 1)]) * scale;
					this.MaxHeight = Math.Max ( h, MaxHeight);
					heightData[j++] = h;
				}
			}
			else
			{
				throw new AxiomException("Error: 2D Data already loaded ");
			}
		}


		protected override void load()
		{
			if ( image == null )
			{
				image = Image.FromFile( Options.Instance.Landscape_Filename + "." + Options.Instance.Landscape_Extension );

				//check to make sure it's 2^n size.
				if ( !this.checkSize(image.Height) ||	!this.checkSize( image.Width ) )
				{
				string err = "Error: Invalid heightmap size : " +
					 image.Width.ToString()  +
					"," + image.Height.ToString() +
					". Should be 2^n+1, 2^n+1";

					throw new AxiomException( err );
				}

				this.bpp = (long)Image.GetNumElemBytes( image.Format );
				if ( this.bpp != 1 )
				{
					throw new AxiomException("Error: Image is not a greyscale image.(1 byte, 8 bits)");
				}

				this.size = (long)image.Width;
				this.max = (long)(this.size * image.Height * this.bpp + 1);    

				maxArrayPos = (long)(this.size * image.Height);
				heightData = new float[maxArrayPos];
				long j = 0;
				float scale = Options.Instance.Scale.y;
				maxheight = 0.0f;
				for (long i = 0; i < this.max - 1;  i += this.bpp )
				{  
					float h =  (float) (image.Data[ i + (this.bpp - 1)]) * scale;
					this.MaxHeight = Math.Max ( h, MaxHeight);
					heightData[j++] = h;
				}
			}
			else
			{
				throw new AxiomException("Error: 2D Data already loaded ");
			}
		}


		protected override void load(Image NewHeightMap )
		{
			if ( image == null )
			{
			 
				image = NewHeightMap;

				//check to make sure it's 2^n + 1 size.
				if ( !this.checkSize(image.Height) ||	!this.checkSize( image.Width ) )
				{
				string err = "Error: Invalid heightmap size : " +
					 image.Width.ToString()  +
					"," + image.Height.ToString() +
					". Should be 2^n+1, 2^n+1";

					throw new AxiomException( err );
				}

				this.bpp = (long)Image.GetNumElemBytes( image.Format );
				if ( this.bpp != 1 )
				{
					throw new AxiomException("Error: Image is not a greyscale image.(1 byte, 8 bits)");
				}

				this.size = (long)image.Width;
				this.max = (long)(this.size * image.Height * this.bpp + 1); 

				maxArrayPos = (long)(this.size * image.Height);
				heightData = new float[maxArrayPos];
				long j = 0;
				float scale = Options.Instance.Scale.y;
				maxheight = 0.0f;
				for (long i = 0; i < this.max - 1;  i += this.bpp )
				{  
					float h =  (float) (image.Data[ i + (this.bpp - 1)]) * scale;
					this.MaxHeight = Math.Max ( h, MaxHeight);
					heightData[j++] = h;
				}
			}
			else
			{
				throw new AxiomException("Error: 2D Data already loaded ");
			}

		}


		protected override void unload()
		{
			this.Dispose();
		}


	}

}

