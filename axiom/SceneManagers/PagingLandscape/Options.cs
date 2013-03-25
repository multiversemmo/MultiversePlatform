#region LGPL License

/*

Axiom Game Engine Library

Copyright (C) 2003  Axiom Project Team



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

#endregion



#region Using Statements



using System;

using System.Collections;

using System.Data;



using Axiom.Core;

using Axiom.MathLib;

using Axiom.Media;



#endregion;



#region Versioning Information

/// File								Revision

/// ===============================================

/// OgrePagingLandScapeOptions.h			1.13

/// OgrePagingLandScapeOptions.cpp			1.16

/// 

#endregion



namespace Axiom.SceneManagers.PagingLandscape

{

	/// <summary>

	/// Summary description for IPLOptions.

	/// </summary>

	public sealed class Options : IDisposable

	{

		#region Singleton Implementation

		

		/// <summary>

		/// Constructor

		/// </summary>

		private Options() 

		{

			this.Data2DFormat = "HeightField";
			this.TextureFormat = "Image";

			this.Landscape_Filename = "";
			this.Landscape_Extension = "";

			this.MatColor = new ColorEx[4];
			for (int i = 0; i < 4; i++)
			{
				this.MatColor[i] = new ColorEx();
			}

			this.MatHeight = new float[2];
			this.MatHeight[0] = 0f;
			this.MatHeight[1] = 0f;

			this.MaxValue = 5000;
			this.MinValue = 0;

			this.PageSize = 257;
			this.TileSize = 64;
			this.World_Width = 0;
			this.World_Height = 0;

			this.Change_Factor = 1;
			this.Max_Adjacent_Pages = 1;
			this.Max_Preload_Pages = 2;
			this.Renderable_Factor = 10;

			this.Scale = new Vector3( 1, 1, 1 );

			this.DistanceLOD = 4;
			this.LOD_Factor = 10;

			this.Num_Renderables = 1000;
			this.Num_Renderables_Increment = 16;
			this.Num_Tiles = 1000;
			this.Num_Tiles_Increment = 16;

			this.CameraThreshold = 5;
			this.VisibilityAngle = 50;


			this.Num_Renderables_Loading = 10;

			this.Lit = false;
			this.Colored = false;

			this.Coverage_Vertex_Color = false; 

			this.Base_Vertex_Color = false; 

			this.Vertex_Shadowed = false; 

			this.Vertex_Instant_Colored = false; 

		}





		private static Options instance = null;



		public static Options Instance 

		{

			get 

			{

				if ( instance == null ) instance = new Options();

				return instance;

			}

		}





		#endregion Singleton Implementation



		#region IDisposable Implementation



		public void Dispose()

		{

			if (instance == this) 

			{

				instance = null;

			}

		}



		#endregion IDisposable Implementation



		#region Fields



		/// <summary>

		/// Contain option data loaded during the Load method

		/// </summary>

		private DataSet optionData;

		private DataTable table;

		private DataRow row;



		#endregion Fields



		#region Properties

		public string Data2DFormat;
		public string TextureFormat;

		public string Landscape_Filename;
		public string Landscape_Extension;

		public string Image_Filename;
		public bool ImageNameLoad;

		#region Map Tool Options
		// MAP TOOL OPTIONS
		public string Splat_Filename_0;
		public string Splat_Filename_1;
		public string Splat_Filename_2;
		public string Splat_Filename_3;

		public String OutDirectory;

		public bool Paged;
		public bool PVSMap;
		public bool BaseMap;
		public bool RGBMaps;
		public bool ColorMapSplit;
		public bool ColorMapGenerate;
		public bool LightMap;
		public bool NormalMap;
		public bool HeightMap;
		public bool AlphaMaps;
		public bool ShadowMap;
		public bool HorizonMap;
		public bool LitBaseMap;
		public bool LitColorMapSplit;
		public bool LitColorMapGenerate;
		public bool InfiniteMap;
		public bool CoverageMap;
		public bool ElevationMap;
		public bool HeightNormalMap;
		public bool AlphaSplatRGBAMaps;
		public bool AlphaSplatLightMaps;

		public float HeightMapBlurFactor;
    
		public string ColorMapName;

		public Vector3 Sun;
		public float    Amb;
		public float    Diff;
		public int    Blur;
		// end of MAP TOOL OPTIONS
		#endregion Map Tool Options

		public long MaxValue;						//Compression range for the TC height field
		public long MinValue;

		public long TileSize;
		public long PageSize;						//size of the page.
		public long World_Height;					//world page height, from 0 to height
		public long World_Width;					//world page width, from 0 to width

		public float Change_Factor;				//Determines the value of the change factor for loading/unloading LandScape Pages
		public long Max_Adjacent_Pages;
		public long Max_Preload_Pages;
		public float Visible_Renderables;			//Numbers of visible renderables surrounding the camera
		public float Renderable_Factor;			//Determines the distance of loading and unloading of renderables in renderable numbers

		public Vector3 Scale;

		public float DistanceLOD;					//Distance for the LOD change
		public float LOD_Factor;


		public long Num_Renderables;				//Max number of renderables to use.
		public long Num_Renderables_Increment;		//Number of renderables to add in case we run out of renderables
		public long Num_Tiles;						//Max number of tiles to use.
		public long Num_Tiles_Increment;			//Number of renderables to add in case we run out of renderables

		public float CameraThreshold;				//If the last camera position is >= the the scene is transverse again.
		public float VisibilityAngle;				//Angle to discard renderables
		public long Num_Renderables_Loading;		//Max number of renderable to load in a single Frame.
		public long MaxRenderLevel;
		public ColorEx[] MatColor; //4
		public float[] MatHeight; //2

		public bool Lit;
		public bool Colored;
		public bool Coverage_Vertex_Color;
		public bool Base_Vertex_Color;

		public bool Vertex_Shadowed;

		public bool Vertex_Instant_Colored;



		#endregion // Properties



		public void Load( string filename )

		{

			/* Set up the options */

			//ConfigFile this;

			string val;



			//config.load( filename );

			optionData = new DataSet();

			optionData.ReadXml(filename);

			table = optionData.Tables[0];

			row = table.Rows[0];



			this.Data2DFormat = this.getSetting( "Data2DFormat" );

			this.TextureFormat = this.getSetting( "TextureFormat" );



			val = this.getSetting( "ScaleX" );

			if ( val != string.Empty ) this.Scale.x = float.Parse( val );

		

			val = this.getSetting( "ScaleY" );

			if ( val != string.Empty ) this.Scale.y = float.Parse( val );



			val = this.getSetting( "ScaleZ" );

			if ( val != string.Empty ) this.Scale.z = float.Parse( val );



			if ( Data2DFormat.StartsWith("HeightFieldTC") )

			{

				this.MaxValue = long.Parse( this.getSetting( "MaxValue" )) * (long)this.Scale.y ;

				this.MinValue = long.Parse( this.getSetting( "MinValue" )) * (long)this.Scale.y;

			}

			else

			{

				this.MaxValue = (long)(255 * this.Scale.y);

				this.MinValue = (long)(0 * this.Scale.y);

			}



			this.Landscape_Filename = this.getSetting( "LandScapeFileName" );

			this.Landscape_Extension = this.getSetting( "LandScapeExtension" );



			this.Image_Filename = this.getSetting( "ImageFilename" );
			this.ImageNameLoad = (this.Image_Filename != string.Empty);


			this.Colored = ( this.getSetting( "VertexColors" ) == "yes" );


			this.Coverage_Vertex_Color = (this.getSetting( "CoverageVertexColor" ) == "yes" );
			this.Base_Vertex_Color = (this.getSetting( "BaseVertexColor" ) == "yes" );
			this.Vertex_Shadowed = (this.getSetting( "BaseVertexShadow" ) == "yes" );
			this.Vertex_Instant_Colored = (this.getSetting( "BaseVertexInstantColor" ) == "yes" );

			// Make sure If we are Shadowed then We are Instant Colored
			if (this.Vertex_Shadowed) this.Vertex_Instant_Colored = true;

			if (this.Coverage_Vertex_Color || this.Base_Vertex_Color ||
				this.Vertex_Shadowed || this.Vertex_Instant_Colored)
				this.Colored = true;

			this.Lit = ( this.getSetting( "VertexNormals" ) == "yes" );


			this.Splat_Filename_0 = this.getSetting( "SplatFilename0" );
			this.Splat_Filename_1 = this.getSetting( "SplatFilename1" );
			this.Splat_Filename_2 = this.getSetting( "SplatFilename2" );
			this.Splat_Filename_3 = this.getSetting( "SplatFilename3" );

         // TODO: Why is this commented out??? If those filenames are not present then these should be skipped (unless they are no longer needed)
//			this.MatColor[0] = getAvgColor( this.Splat_Filename_0 );

//			this.MatColor[1] = getAvgColor( this.Splat_Filename_1 );

//			this.MatColor[2] = getAvgColor( this.Splat_Filename_2 );

//			this.MatColor[3] = getAvgColor( this.Splat_Filename_3 );



			float divider = ( MaxValue - MinValue ) / 255.0f;

         // FH 06/17/2005: Got an exception when not supplying these...
         if ((val = this.getSetting( "MaterialHeight1" )) != string.Empty)
            this.MatHeight[0] = float.Parse( val );

			this.MatHeight[0] = this.MatHeight[0] * divider;


         if ((val = this.getSetting( "MaterialHeight2" )) != string.Empty)
            this.MatHeight[1] = float.Parse( val );

			this.MatHeight[1] = this.MatHeight[1] * divider;



			val = this.getSetting( "MaxNumRenderables" );
			if (val != string.Empty )	this.Num_Renderables = long.Parse( val );

			val = this.getSetting( "IncrementRenderables" );
			if (val != string.Empty ) this.Num_Renderables_Increment =  long.Parse( val );

			val = this.getSetting( "MaxNumTiles" );
			if (val != string.Empty ) this.Num_Tiles = long.Parse( val );

			val = this.getSetting( "IncrementTiles" );
			if (val != string.Empty ) this.Num_Tiles_Increment =  long.Parse( val );


			val = this.getSetting( "CameraThreshold" );

			if ( val != string.Empty ) this.CameraThreshold = float.Parse( val );



			// To avoid the use of a square root.

			this.CameraThreshold *= this.CameraThreshold;

			

			val = this.getSetting( "VisibilityAngle" );

			if (val != string.Empty ) this.VisibilityAngle = float.Parse( val );



			val = this.getSetting( "NumRenderablesLoading" );

			if ( val != string.Empty ) this.Num_Renderables_Loading = long.Parse( val );

			

			val = this.getSetting( "MaxAdjacentPages" );

			if ( val != string.Empty ) this.Max_Adjacent_Pages = long.Parse( val );

			

			val = this.getSetting( "MaxPreloadedPages" );

			if ( val != string.Empty ) this.Max_Preload_Pages = long.Parse( val );

			

			val = this.getSetting( "Height" );

			if ( val != string.Empty ) this.World_Height = long.Parse( val );



			val = this.getSetting( "Width" );

			if ( val != string.Empty ) this.World_Width = long.Parse( val );



			val = this.getSetting( "PageSize" );

			if ( val != string.Empty ) this.PageSize = long.Parse( val );



			val = this.getSetting( "TileSize" );

			if ( val != string.Empty ) this.TileSize = long.Parse( val );

			



			val = this.getSetting( "MaxRenderLevel" );

			if (val != string.Empty )	this.MaxRenderLevel = long.Parse( val );
			if (this.MaxRenderLevel == 0)
			{
				while ((long)(1 << (int)this.MaxRenderLevel) < this.TileSize)
					this.MaxRenderLevel++;
			}

			val = this.getSetting( "ChangeFactor" );
			if (val != string.Empty ) this.Change_Factor = float.Parse( val  ) * ( this.PageSize / 9 );

			val = this.getSetting( "VisibleRenderables" );
			if (val != string.Empty ) this.Visible_Renderables = float.Parse( val );
			// compute the actual distance as a square
			this.Renderable_Factor = this.Visible_Renderables * ( this.TileSize * this.Scale.x + this.TileSize * this.Scale.z );
			this.Renderable_Factor *= this.Renderable_Factor;

			val = this.getSetting( "DistanceLOD" );
			if (val != string.Empty ) this.DistanceLOD = float.Parse( val );
			// Compute the actual distance as a square
			this.LOD_Factor = this.DistanceLOD * ( this.TileSize * this.Scale.x + this.TileSize * this.Scale.z );
			this.LOD_Factor *= this.LOD_Factor;


			// MAP TOOL OPTIONS
			this.Paged = (this.getSetting( "Paged" ) == "yes" );

			this.OutDirectory = this.getSetting( "OutDirectory" );
			if ( OutDirectory.StartsWith( "LandScapeFileName") == true )
				this.OutDirectory = this.Landscape_Filename;

			this.PVSMap = (this.getSetting( "PVSMap" ) == "yes" );
			this.BaseMap = (this.getSetting( "BaseMap" ) == "yes" );
			this.RGBMaps = (this.getSetting( "RGBMaps" ) == "yes" );
			this.ColorMapGenerate = (this.getSetting( "ColorMapGenerate" ) == "yes" );
			this.ColorMapSplit = (this.getSetting( "ColorMapSplit" ) == "yes" );
			this.LightMap = (this.getSetting( "LightMap" ) == "yes" );
			this.NormalMap = (this.getSetting( "NormalMap" ) == "yes" );
			this.HeightMap = (this.getSetting( "HeightMap" ) == "yes" );
			this.AlphaMaps = (this.getSetting( "AlphaMaps" ) == "yes" );
			this.ShadowMap = (this.getSetting( "ShadowMap" ) == "yes" );
			this.HorizonMap = (this.getSetting( "HorizonMap" ) == "yes" );  
			this.LitBaseMap = (this.getSetting( "LitBaseMap" ) == "yes" );
			this.InfiniteMap = (this.getSetting( "InfiniteMap" ) == "yes" );
			this.CoverageMap = (this.getSetting( "CoverageMap" ) == "yes" );
			this.LitColorMapGenerate = (this.getSetting( "LitColorMapGenerate" ) == "yes" );
			this.LitColorMapSplit = (this.getSetting( "LitColorMapSplit" ) == "yes" );
			this.ElevationMap= (this.getSetting( "ElevationMap" ) == "yes" ); 
			this.HeightNormalMap = (this.getSetting( "HeightNormalMap" ) == "yes" );
			this.AlphaSplatRGBAMaps =  (this.getSetting( "AlphaSplatRGBAMaps" ) == "yes" );
			this.AlphaSplatLightMaps = (this.getSetting( "AlphaSplatLightMaps" ) == "yes" );

			this.ColorMapName  = this.getSetting( "ColorMapName" );

			val = this.getSetting( "HeightMapBlurFactor" );
			if (val != string.Empty ) HeightMapBlurFactor = float.Parse( val );
			

			Sun = new Vector3();
			val =this.getSetting( "SunX" );
			if (val != string.Empty ) Sun.x = float.Parse( val );
			val =this.getSetting( "SunY" ); 
			if (val != string.Empty ) Sun.y = float.Parse( val );
			val =this.getSetting( "SunZ" );
			if (val != string.Empty ) Sun.z = float.Parse( val );

			val =this.getSetting( "Ambient" );
			if (val != string.Empty ) Amb = float.Parse( val );

			val =this.getSetting( "Diffuse" );
			if (val != string.Empty ) Diff = float.Parse( val );

			val =this.getSetting( "Blur" );
			if (val != string.Empty ) Blur = int.Parse( val );
			
		}



		public bool setOption( string strKey, object pValue )
		{
			if ( strKey == "VisibleRenderables" )
			{
				Visible_Renderables = (int)  pValue ;
				// compute the actual distance as a square
				Renderable_Factor = Visible_Renderables * ( TileSize * Scale.x + TileSize * Scale.z );
				Renderable_Factor *= Renderable_Factor;
				return true;
			}
			if ( strKey == "DistanceLOD" )
			{
				DistanceLOD = (float) ( pValue );
				// Compute the actual distance as a square
				LOD_Factor = DistanceLOD * ( TileSize * Scale.x + TileSize * Scale.z );
				LOD_Factor *= LOD_Factor;
				return true;
			}

			return false;

		}

		
		public bool getOption( string strKey, object pDestValue )
		{
			if ( strKey == "VisibleRenderables" )
			{
				pDestValue = Visible_Renderables;
				return true;
			}
			if ( strKey == "DistanceLOD" )
			{
				pDestValue = DistanceLOD;
				return true;
			}
			if ( strKey == "VisibleDistance" )
			{
				// we need to return the square root of the distance
				pDestValue =  Math.Sqrt (Renderable_Factor);
			}
			if ( strKey == "VisibleLOD" )
			{
				// we need to return the square root of the distance
				pDestValue =  Math.Sqrt (LOD_Factor);
			}
			// Some options proposed by Praetor
			if ( strKey == "Width" )
			{
				pDestValue = World_Width;
				return true;
			}
			if ( strKey == "Height" )
			{
				pDestValue = World_Height;
				return true;
			}
			if ( strKey == "PageSize" )
			{
				pDestValue = PageSize;
				return true;
			}
			if ( strKey == "ScaleX" )
			{
				pDestValue = Scale.x;
				return true;
			}
			if ( strKey == "ScaleY" )
			{
				pDestValue = Scale.y;
				return true;
			}
			if ( strKey == "ScaleZ" )
			{
				pDestValue = Scale.z;
				return true;
			}
			return false;
		}

		public bool hasOption( string strKey )
		{
			if ( strKey == "VisibleRenderables" )
			{
				return true;
			}
			if ( strKey == "DistanceLOD" )
			{
				return true;
			}
			if ( strKey == "VisibleDistance" )
			{
				return true;
			}
			if ( strKey == "VisibleLOD" )
			{
				return true;
			}
			// Some options proposed by Praetor
			if ( strKey == "Width" )
			{
				return true;
			}
			if ( strKey == "Height" )
			{
				return true;
			}
			if ( strKey == "PageSize" )
			{
				return true;
			}
			if ( strKey == "ScaleX" )
			{
				return true;
			}
			if ( strKey == "ScaleY" )
			{
				return true;
			}
			if ( strKey == "ScaleZ" )
			{
				return true;
			}
			return false;
		}

		public bool getOptionValues( string strKey, ArrayList refValueList )
		{
			if ( strKey == "VisibleRenderables" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "DistanceLOD" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "VisibleDistance" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "VisibleLOD" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "Width" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "Height" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "PageSize" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "ScaleX" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "ScaleY" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "ScaleZ" )
			{
				refValueList.Add(new object());
				return true;
			}
			return false;
		}

		public bool getOptionKeys( ArrayList refKeys )	

		{
			refKeys.Add( "VisibleRenderables" );
			refKeys.Add( "DistanceLOD" );
			refKeys.Add( "VisibleDistance" );
			refKeys.Add( "VisibleLOD" );
			// Some options from Praetor
			refKeys.Add( "Width" );
			refKeys.Add( "Height" );
			refKeys.Add( "PageSize" );
			refKeys.Add( "ScaleX" );
			refKeys.Add( "ScaleY" );
			refKeys.Add( "ScaleZ" );
			return true;
		}


		private string getSetting( string setting )

		{

			if(table.Columns[setting] != null) 

			{

				return (string)row[setting];

			}

			return "";

		}



		private ColorEx getAvgColor(string tex)

		{

		Image img = Image.FromFile(tex);

		int bpp = Image.GetNumElemBytes( img.Format );

		byte[] data = img.Data;

		int cr = 0, cg = 0, cb = 0, s = 0;



			for (int i = 0; i < img.Size; i += bpp)

			{

				cr += data[i];

				cg += data[i+1];

				cb += data[i+2];

				s++;

			}

			cr /= s;

			cg /= s;

			cb /= s;

			return new ColorEx(cr / 255.0F, cg / 255.0F, cb / 255.0F, 1);

		}

	}

}

