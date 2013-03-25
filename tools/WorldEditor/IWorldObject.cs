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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Axiom.MathLib;


namespace Multiverse.Tools.WorldEditor
{

    public enum UpdateTypes { All, AlphaSplatTerrainDisplay, AmbientLight, Collection, DirectionalLight, Fog, Forest, GlobalAmbientLight, GlobalDirectionalLight, GlobalFog, Grass, Markers, Object, Ocean, ParticleEffect, PathObjectTypeContainer, PathObjectTypeNode, Plant, Point, PointLight, Points, Regions, Road, Skybox, Sound, SpawnGenerator, TerrainDecal, Tree, Water, World, WorldTerrain };
    public enum UpdateHint { Display, Position, TerrainUpdate, DisplayMarker, DisplayDecal, DisplayLight, DisplayPointLightCircles };

    public interface IWorldObject : IDisposable
    {
        void AddToTree(WorldTreeNode parentNode);

        void RemoveFromTree();

        void AddToScene();

        void RemoveFromScene();

        void CheckAssets();

		void ToXml(XmlWriter w);

        void ToManifest(StreamWriter w);

        void UpdateScene(UpdateTypes type, UpdateHint hint);

        void Clone(IWorldContainer parent);

        Vector3 FocusLocation
        {
            get;
        }

        string ObjectType
        {
            get;
        }

        bool Highlight
        {
            get;
            set;
        }

		WorldTreeNode Node
		{
			get;
		}

        List<ToolStripButton> ButtonBar
        {
            get;
        }

        string ObjectAsString
        {
            get;
        }

        bool IsGlobal
        {
            get;
        }

        bool IsTopLevel
        {
            get;
        }

        bool AcceptObjectPlacement
        {
            get;
            set;
        }

        bool WorldViewSelectable
        {
            get;
            set;
        }
    }
}
