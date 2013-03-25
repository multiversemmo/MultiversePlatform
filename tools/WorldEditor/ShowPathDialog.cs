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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Axiom.MathLib;
using Axiom.Core;
using Axiom.Graphics;

namespace Multiverse.Tools.WorldEditor {
    public partial class ShowPathDialog : Form {
        public ShowPathDialog() {
            InitializeComponent();
        }

		public void Prepare() {
            pathBox.SelectionStart = 0;
            pathBox.SelectionLength = pathBox.Text.Length;
        }
        
        protected List<SceneRod> rods = new List<SceneRod>();
		protected static int nameCounter = 0;
		protected List<Vector3> points;
        protected string terrainString = "";

		public void AddRodsToScene()
		{
			if (points.Count >= 2) {
                Vector3 previousPosition = points[0];
                for (int i=1; i<points.Count; i++) {
                    bool overTerrain = (terrainString.Length > i && (terrainString[i-1] == 'T' || terrainString[i] == 'T'));
                    NewSceneCapsule(previousPosition, points[i], overTerrain);
                    previousPosition = points[i];
                }
			}
		}
		
        protected void RemoveRodsFromScene()
		{
			if (rods.Count > 0) 
			{
				foreach (SceneRod rod in rods) 
				{
					BlastSceneNode(rod.end1);
					BlastSceneNode(rod.end2);
					BlastSceneNode(rod.cylinder);
				}
			}
			rods.Clear();
		}
		
        protected void BlastSceneNode(SceneNode node) {
            node.Creator.DestroySceneNode(node.Name);
        }
		
		private SceneNode UnscaledSceneObject(string meshName, Vector3 position)
		{
			nameCounter++;
            string name = "Rod-" + nameCounter;
			Entity entity = WorldEditor.Instance.Scene.CreateEntity(name, meshName);
			SceneNode node = WorldEditor.Instance.Scene.RootSceneNode.CreateChildSceneNode(name);
			node.AttachObject(entity);
			node.Position = position;
			node.ScaleFactor = Vector3.UnitScale;
			node.Orientation = Quaternion.Identity;
			return node;
		}

		private SceneNode NewSceneObject(string meshName, Vector3 position, float scale)
		{
			SceneNode node = UnscaledSceneObject(meshName, position);
			node.ScaleFactor = Vector3.UnitScale * scale;
			return node;
		}

		private SceneNode NewSceneObject(string meshName, Vector3 position, 
										 Vector3 scale, Quaternion orientation)
		{
			SceneNode node = UnscaledSceneObject(meshName, position);
			node.ScaleFactor = scale;
			node.Orientation = orientation;
			return node;
		}
	
		private SceneNode NewSceneSphere(Vector3 center, float radius)
		{
			return NewSceneObject("unit_sphere.mesh", center, radius);
		}

		protected void NewSceneCapsule(Vector3 from, Vector3 to, bool overTerrain) {
			float rodRadius = 5f / 100f;
			float ballRadius = 10f / 100f;
            // Place the rods so they sit on the triangles
			from.y += rodRadius;
			to.y += rodRadius;
			SceneNode end1 = NewSceneSphere(from, ballRadius);
			SceneNode end2 = NewSceneSphere(to, ballRadius);
			Vector3 seg = (from - to);
			rods.Add(new SceneRod(end1,
								  end2,
                                  NewSceneObject(overTerrain ? "green_unit_cylinder.mesh" : "unit_cylinder.mesh",
                                                 to + seg * 0.5f,
												 new Vector3(rodRadius, seg.Length / 1000f, rodRadius),
												 new Vector3(0f, 1f, 0f).GetRotationTo(seg))));
		}

        private void okButton_Click(object sender, EventArgs e) {
            RemoveRodsFromScene();
            // Parse the pathBox contents into a list of Vector3
            // entities.  Eliminate any #nn phrases, if encountered
            List<Vector3> path = new List<Vector3>();
            string s = pathBox.Text;
            int openParen;
            terrainString = "";
            while ((openParen = s.IndexOf('(')) >= 0) {
                terrainString += (openParen > 0 && s[openParen-1] == 'T' ? 'T' : 'C');
                s = s.Substring(openParen);
                int closeParen = s.IndexOf(')');
                if (closeParen < 0)
                    break;
                string point = s.Substring(0, closeParen + 1);
                s = s.Substring(closeParen + 1);
                try {
                    Vector3 v = Vector3.Parse(point);
                    path.Add(v);
                }
                catch (Exception) {
                }
            }
            if (path.Count > 1) {
                points = path;
                AddRodsToScene();
            }
            DialogResult = DialogResult.OK;
        }

	}

	public class SceneRod {
		public SceneNode end1;
		public SceneNode end2;
		public SceneNode cylinder;

		public SceneRod(SceneNode end1, SceneNode end2, SceneNode cylinder) 
		{
			this.end1 = end1;
			this.end2 = end2;
			this.cylinder = cylinder;
		}

	}

}
