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
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Multiverse;
using Axiom.MathLib;
using Axiom.Core;
using Axiom.Graphics;
using Multiverse.CollisionLib;

namespace Axiom.SceneManagers.Multiverse
{
    public class VegetationSemantic : IBoundarySemantic, IDisposable
    {
        private String name;
        private Boundary boundary = null;
        private List<PlantType> plantTypes;
        // private AxisAlignedBox bounds = null;

        public VegetationSemantic(String name, Boundary boundary)
        {
            this.name = name;
            this.boundary = boundary;
			plantTypes = new List<PlantType>();
        }

        public VegetationSemantic(XmlTextReader r)
        {
            plantTypes = new List<PlantType>();
            FromXML(r);
        }

        public string Type
        {
            get
            {
                return "VegetationSemantic";
            }
        }

        public void AddToBoundary(Boundary boundary)
        {
            this.boundary = boundary;
            TerrainManager.Instance.AddDetailVegetationSemantic(this);
        }

        public void RemoveFromBoundary()
        {
            TerrainManager.Instance.RemoveDetailVegetationSemantic(this);
            boundary = null;
        }

        /// <summary>
        /// Add a new type of grass to the boundary.
        /// </summary>
        public void AddPlantType(uint numInstances, string imageName, 
								 float atlasStartX, float atlasWidthX,
								 float atlasStartY, float atlasWidthY,
                                 float scaleWidthLow, float scaleWidthHi,
                                 float scaleHeightLow, float scaleHeightHi, float windMagnitude)
        {
            AddPlantType(numInstances, imageName, 
						 atlasStartX, atlasWidthX, atlasStartY, atlasWidthY, 
						 scaleWidthLow, scaleWidthHi, scaleHeightLow, scaleHeightHi,
						 ColorEx.White, 1, 1, windMagnitude);
        }

        public void AddPlantType(uint numInstances, string imageName, 
								 float atlasStartX, float atlasWidthX,
								 float atlasStartY, float atlasWidthY,
                                 float scaleWidthLow, float scaleWidthHi,
                                 float scaleHeightLow, float scaleHeightHi,
                                 ColorEx color, float colorMultLow, float colorMultHi, float windMagnitude)
        {
            PlantType plantType = new PlantType(numInstances, imageName,
												atlasStartX, atlasWidthX, atlasStartY, atlasWidthY,
                                                scaleWidthLow, scaleWidthHi, scaleHeightLow, scaleHeightHi,
                                                color, colorMultLow, colorMultHi, windMagnitude);
            AddPlantType(plantType);
        }

        public void AddPlantType(uint numInstances, string imageName, 
                                 float scaleWidthLow, float scaleWidthHi,
                                 float scaleHeightLow, float scaleHeightHi,
                                 ColorEx color, float colorMultLow, float colorMultHi,
								 float windMagnitude)
        {
            PlantType plantType = new PlantType(numInstances, imageName,
                                                scaleWidthLow, scaleWidthHi, scaleHeightLow, scaleHeightHi,
                                                color, colorMultLow, colorMultHi, windMagnitude);
            AddPlantType(plantType);
        }

        public void AddPlantType(PlantType plantType)
		{
            plantTypes.Add(plantType);
            TerrainManager.Instance.RefreshVegetation();
		}

        public void RemovePlantType(PlantType plantType)
        {
            plantTypes.Remove(plantType);
            TerrainManager.Instance.RefreshVegetation();
        }
		
		public void ToXML(XmlTextWriter w)
        {
            w.WriteStartElement("boundarySemantic");
            w.WriteAttributeString("type", "VegetationSemantic");
            w.WriteElementString("name", name);

            foreach (PlantType t in plantTypes)
            {
                w.WriteStartElement("PlantType");
                w.WriteAttributeString("numInstances", t.NumInstances.ToString());
                w.WriteAttributeString("imageName", t.ImageName);
				w.WriteAttributeString("atlasStartX", t.AtlasStartX.ToString());
                w.WriteAttributeString("atlasEndX", t.AtlasEndX.ToString());
                w.WriteAttributeString("scaleWidthLow", t.ScaleWidthLow.ToString());
                w.WriteAttributeString("scaleWidthHi", t.ScaleWidthHi.ToString());
                w.WriteAttributeString("scaleHeightLow", t.ScaleHeightLow.ToString());
                w.WriteAttributeString("scaleHeightHi", t.ScaleHeightHi.ToString());
                w.WriteAttributeString("colorMultLow", t.ColorMultLow.ToString());
                w.WriteAttributeString("colorMultHi", t.ColorMultHi.ToString());
                w.WriteAttributeString("windMagnitude", t.WindMagnitude.ToString());
				w.WriteStartElement("color");
				w.WriteAttributeString("a", t.Color.a.ToString());
				w.WriteAttributeString("r", t.Color.r.ToString());
				w.WriteAttributeString("g", t.Color.g.ToString());
				w.WriteAttributeString("b", t.Color.b.ToString());
				w.WriteEndElement();
				w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        protected void ParseElement(XmlTextReader r)
        {
            bool readEnd = true;

            // set the field in this object based on the element we just read
            switch (r.Name)
			{
			case "name":
				// read the value
				r.Read();
				if (r.NodeType != XmlNodeType.Text)
					return;
				name = r.Value;

				break;

			case "PlantType":
				uint numInstances = 0;
				string imageName = "";
				float atlasStartX = 0f;
				float atlasEndX = 1f;
				float atlasStartY = 0f;
				float atlasEndY = 1f;
				float scaleWidthLow = 0f;
				float scaleWidthHi = 0f;
				float scaleHeightLow = 0f;
				float scaleHeightHi = 0f;
				ColorEx color = new ColorEx();
				float colorMultLow = 0f;
				float colorMultHi = 0f;
				float windMagnitude = 0f;
				for (int i = 0; i < r.AttributeCount; i++)
				{
					r.MoveToAttribute(i);

					// set the field in this object based on the element we just read
					switch (r.Name)
					{
					case "numInstances":
						numInstances = uint.Parse(r.Value);
						break;
					case "imageName":
						imageName = r.Value;
						break;
					case "atlasStartX":
						atlasStartX = float.Parse(r.Value);
						break;
					case "atlasEndX":
						atlasEndX = float.Parse(r.Value);
						break;
					case "atlasStartY":
						atlasStartY = float.Parse(r.Value);
						break;
					case "atlasEndY":
						atlasEndY = float.Parse(r.Value);
						break;
					case "scaleWidthLow":
						scaleWidthLow = float.Parse(r.Value);
						break;
					case "scaleWidthHi":
						scaleWidthHi = float.Parse(r.Value);
						break;
					case "scaleHeightLow":
						scaleHeightLow = float.Parse(r.Value);
						break;
					case "scaleHeightHi":
						scaleHeightHi = float.Parse(r.Value);
						break;
					case "colorMultLow":
						colorMultLow = float.Parse(r.Value);
						break;
					case "colorMultHi":
						colorMultHi = float.Parse(r.Value);
						break;
					case "windMagnitude":
						windMagnitude = float.Parse(r.Value);
						break;
					}
				}
				r.MoveToElement();
				while (r.Read())
				{
					// look for the start of an element
					if (r.NodeType == XmlNodeType.Element)
					{
						// parse that element
						switch (r.Name)
						{
						case "color":
							float alpha = 0f;
							float red = 0f;
							float blue = 0f;
							float green = 0f;
							for (int j = 0; j < r.AttributeCount; j++)
							{
								r.MoveToAttribute(j);
								switch(r.Name)
								{
								case "a":
									alpha = float.Parse(r.Value);
									break;
								case "r":
									red = float.Parse(r.Value);
									break;
								case "g":
									green = float.Parse(r.Value);
									break;
								case "b":
									blue = float.Parse(r.Value);
									break;
								}
							}
							color = new ColorEx(alpha, red, green, blue);
							break;
						}
					}
					else
						break;
				}
				r.MoveToElement(); //Moves the reader back to the element node.
				AddPlantType(numInstances, imageName, 
							 atlasStartX, atlasEndX, atlasStartY, atlasEndY,
							 scaleWidthLow, scaleWidthHi, scaleHeightLow, scaleHeightHi,
							 color, colorMultLow, colorMultHi, windMagnitude);
				readEnd = false;
				break;
			}
            if (readEnd)
            {
                // error out if we dont see an end element here
                r.Read();
                if (r.NodeType != XmlNodeType.EndElement)
                {
                    return;
                }
            }
		}

        private void FromXML(XmlTextReader r)
        {
            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    ParseElement(r);
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    // if we found an end element, it means we are at the end of the terrain description
                    return;
                }
            }
        }

        public void BoundaryChange()
        {
            TerrainManager.Instance.RefreshVegetation();
        }

        public void Dispose()
        {
        }

        public void PerFrameProcessing(float time, Camera camera)
        {
        }

        public void PageShift()
        {
        }

        public List<PlantType> PlantTypes
        {
            get
            {
                return plantTypes;
            }

        }
    
		public Boundary BoundaryObject
		{
			get
			{
				return boundary;
			}
		}
			
	}
}

