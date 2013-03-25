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
using System.Collections;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Multiverse.AssetRepository;

namespace Multiverse.Tools.WorldEditor
{
	/// <summary>
	/// Summary description for AssetCollection.
	/// </summary>
	public class AssetCollection
	{
		protected Dictionary<String, AssetDesc> assets;

		public AssetCollection(string filename)
		{
            assets = new Dictionary<String, AssetDesc>();

            AddAssets(filename);
		}

		public AssetCollection()
		{
            assets = new Dictionary<String, AssetDesc>();
			ImportAssetDefinitions();
		}

        public void AddAssets(string filename)
        {
            try
            {
                XmlReader r = XmlReader.Create(filename);
                FromXML(r);
                r.Close();
            }
            catch (Exception)
            {
            }
        }

		public void FromXML(XmlReader r)
		{
			while ( r.Read() ) 
			{
				// look for the start of the assets list
				if ( r.NodeType == XmlNodeType.Element ) 
				{
					if ( r.Name == "Assets" ) 
					{
						// we found the list of assets, now parse it
						ParseAssets(r);
					}
				}
			}
		}

		protected void ParseAssets(XmlReader r)
		{
			while ( r.Read() ) 
			{
				// look for the start of an element
				if ( r.NodeType == XmlNodeType.Element ) 
				{
					// parse that element
                    if (r.Name == "Model")
                    {
                        ParseAssetElement(r, "Model");
                    }
                    else if (r.Name == "Skybox")
                    {
                        ParseAssetElement(r, "Skybox");
                    }
                    else if (r.Name == "TerrainMaterial")
                    {
                        ParseAssetElement(r, "TerrainMaterial");
                    }
                    else if (r.Name == "Sound")
                    {
                        ParseAssetElement(r, "Sound");
                    }
                    else if (r.Name == "SpeedTree")
                    {
                        ParseAssetElement(r, "SpeedTree");
                    }
                    else if (r.Name == "SpeedWind")
                    {
                        ParseAssetElement(r, "SpeedWind");
                    }
                    else if (r.Name == "Vegitation")
                    {
                        ParseAssetElement(r, "Vegitation");
                    }
                    else if (r.Name == "ParticleFX")
                    {
                        ParseAssetElement(r, "ParticleFX");
                    }
                    else if (r.Name == "Movie")
                    {
                        ParseAssetElement(r, "Movie");
                    }
				} 
				else if ( r.NodeType == XmlNodeType.EndElement ) 
				{
					// if we found an end element, it means we are at the end of the assets list
					return;
				}
			}

			return;
		}

		// Import asset definitions using the asset repository
		protected void ImportAssetDefinitions()
		{
			Dictionary<string, AssetDefinition>.ValueCollection defs = RepositoryClass.Instance.AssetDefinitions;
			foreach (AssetDefinition def in defs) {
				string type = "";
                string subType ="";
				switch (def.TypeEnum) {
				case AssetTypeEnum.Mesh:
					type = "Model";
					break;
				case AssetTypeEnum.Sound:
					type = "Sound";
					break;
				case AssetTypeEnum.SpeedTree:
					type = "SpeedTree";
					break;
				case AssetTypeEnum.ParticleScript:
					type = "ParticleFX";
					break;
				case AssetTypeEnum.Material:
					if (def.Category == "Skybox")
						type = "Skybox";
					break;
				case AssetTypeEnum.SpeedWind:			
					type = "SpeedWind";
                    break;
				case AssetTypeEnum.PlantType:
					type = "Vegitation";
					break;
                case AssetTypeEnum.Movie:
                    type = "Movie";
                    break;
                case AssetTypeEnum.Texture:
                    foreach (AssetProperty prop in def.Properties)
                    {
                        if (String.Equals(prop.Name, "TerrainTexture") && String.Equals(prop.Value.ToLower(), "true"))
                        {
                            subType = "TerrainTexture";
                        }
                        if (String.Equals(prop.Name, "TerrainDecal") && String.Equals(prop.Value.ToLower(), "true"))
                        {
                            subType = "TerrainDecal";
                        }
                    }
                    type = "Texture";
                    break;
				default:
					break;
				}
				if (type != "") {
					AssetDesc asset = new AssetDesc();
					asset.Name = (def.Description == "" ? def.Name : def.Description);

                    string assetName = null;

                    // look for asset name property
                    foreach (AssetProperty prop in def.Properties)
                    {
                        if (prop.Name == "AssetName")
                        {
                            assetName = prop.Value;
                            break;
                        }
                    }

                    // if no AssetName property was found, then use the filename
                    if (assetName == null)
                    {
                        assetName = Path.GetFileName(def.Files[0].TargetFile);
                    }

                    asset.AssetName = assetName;
                    asset.Type = type;
                    if (String.Equals(subType, ""))
                    {
                        asset.SubType = def.Category;
                    }
                    else
                    {
                        asset.SubType = subType;
                    }
					assets[asset.Name] = asset;
				}
			}
		}
		
		protected void ParseAssetElement(XmlReader r, string assetType)
		{
			AssetDesc asset = new AssetDesc();
			asset.Type = assetType;

			for (int i = 0; i < r.AttributeCount; i++)
			{
				r.MoveToAttribute(i);
				
				// set the field in this object based on the element we just read
				switch ( r.Name ) 
				{
					case "assetName":
						asset.AssetName = r.Value;
						break;
					case "name":
						asset.Name = r.Value;
						break;
					case "subType":
						asset.SubType = r.Value;
						break;
				}
			}
			r.MoveToElement(); //Moves the reader back to the element node.

			assets[asset.Name] = asset;
			//models.Add(model);
			return;
		}

		public int Count 
		{
			get 
			{
				return assets.Count;
			}
		}

		public System.Collections.IEnumerator GetEnumerator() 
		{
			return assets.GetEnumerator();
		}

//		new public ModelDesc this[int index] 
//		{
//			get { return (ModelDesc)models[index]; }
//			set { models[index] = value; }
//		}

		public AssetDesc this[string name]
		{
			get 
			{
				return assets[name];
			}
		}

		// return a list of all assets of the specified type
		public List<AssetDesc> Select(string type)
		{
			List<AssetDesc> ret = new List<AssetDesc>();
			foreach ( KeyValuePair<String, AssetDesc> kvp in assets )
			{
				AssetDesc asset = kvp.Value;
				if ( asset.Type == type ) 
				{
					ret.Add(asset);
				}
			}

			return ret;
		}

		// return a list of all assets of the specified type and subtype
		public List<AssetDesc> Select(string type, string subType)
		{
			List<AssetDesc> ret = new List<AssetDesc>();
			foreach ( KeyValuePair<String, AssetDesc> kvp in assets )
			{
				AssetDesc asset = kvp.Value;
				if ( ( asset.Type == type ) && ( asset.SubType == subType ) )
				{
					ret.Add(asset);
				}
			}

			return ret;
		}

		// return a list of subtypes available for the given type
		public List<String> SubTypes(string type)
		{
			List<String> ret = new List<String>();
			foreach ( KeyValuePair<String, AssetDesc> kvp in assets )
			{
				AssetDesc asset = kvp.Value;
				if ( asset.Type == type )
				{
					if ( ( asset.SubType != null ) && ( asset.SubType.Length != 0 ) )
					{
						if ( ! ret.Contains(asset.SubType) ) 
						{
							ret.Add(asset.SubType);
						}
					}
				}
			}

			return ret;
		}

		
		public AssetDesc assetFromAssetName(string assetName)
		{
			AssetDesc asset = null;
			foreach ( KeyValuePair<String, AssetDesc> kvp in assets )
			{
				asset = kvp.Value;
				if (asset.AssetName == assetName)
				{
					break;
				}
			}
			return asset;
		}




		public AssetDesc assetFromName(string name)
		{
			AssetDesc asset = null;
			foreach (KeyValuePair<String, AssetDesc> kvp in assets)
			{
				asset = kvp.Value;
				if (asset.Name == name)
				{
					break;
				}
			}
			return asset;
		}
	}
}
