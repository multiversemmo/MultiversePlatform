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

	/// Summary description for Texture_Image.

	/// </summary>

	public class Texture_Image: Texture

	{



		public Texture_Image(): base()

		{

			//

			// TODO: Add constructor logic here

			//

		}

		protected override void loadMaterial()
		{
			if ( material == null )
			{
			
				if (Options.Instance.ImageNameLoad)
				{
					// JEFF - all material settings configured through material script
					//material = (Material)(MaterialManager.Instance.GetByName("PagingLandScape.0.0"));

					string commonName = dataZ.ToString() + "." + dataX.ToString();
					string matname = "Image." + commonName;
					//material = material.Clone(matname);
					material = CreateMaterial(matname);
					//if (material.GetTechnique(0).GetPass(0).NumTextureUnitStages == 0 )
					//{
					//	material.GetTechnique(0).GetPass(0).AddTextureUnitState(material.GetTechnique(0).GetPass(0).CreateTextureUnitState("gcanyon_texture_4k2k.0.0.png",0));
					//	material.GetTechnique(0).GetPass(0).GetTextureUnitState(0).TextureAddressing = TextureAddressing.Clamp;
					//	material.GetTechnique(0).GetPass(0).AddTextureUnitState(material.GetTechnique(0).GetPass(0).CreateTextureUnitState("gcanyon_texture_4k2k.0.0.png",0));
					//	material.GetTechnique(0).GetPass(0).GetTextureUnitState(1).SetTextureName("detail3.jpg");
					//	material.GetTechnique(0).GetPass(0).GetTextureUnitState(1).SetTextureScaleU(0.03F);
					//	material.GetTechnique(0).GetPass(0).GetTextureUnitState(1).SetTextureScaleV(0.03F);
					//	
					//}

					string texname = Options.Instance.Image_Filename + "." +
						commonName + "." + Options.Instance.Landscape_Extension;       
					// assign this texture to the material

					material.GetTechnique(0).GetPass(0).CreateTextureUnitState(texname,0);
					material.GetTechnique(0).GetPass(0).GetTextureUnitState(0).TextureAddressing = TextureAddressing.Clamp;
				}
				else
				{
					// JEFF - all material settings configured through material script
					material = (Material)(MaterialManager.Instance.GetByName("PagingLandScape." +  dataX.ToString()+ "." + dataZ.ToString()));
				}
		           
				material.Load(); 
				material.Lighting = Options.Instance.Lit;
			}
		}

		protected override void unloadMaterial()
		{
			if ( material != null )
			{
				material.Unload();
			}

		}

		/// <summary>

		///		Creates a new (blank) material with the specified name.

		/// </summary>

		/// <param name="name"></param>

		/// <returns></returns>

		public virtual Material CreateMaterial(string name) 

		{

			Material material = (Material) MaterialManager.Instance.Create(name);

			material.CreateTechnique().CreatePass();

			return material;

		}



	}

}

