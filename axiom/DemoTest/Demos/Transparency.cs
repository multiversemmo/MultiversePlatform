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
#endregion

using System;
using Axiom.Animating;
using Axiom.Controllers;
using Axiom.Core;

using Axiom.MathLib;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
    /// <summary>
    /// 	Summary description for Transparency.
    /// </summary>
    public class Transparency : DemoBase {
        #region Methods

        protected override void CreateScene() {
            // set some ambient light
            scene.AmbientLight = new ColorEx(1.0f, 0.5f, 0.5f, 0.5f);

            // create a point light (default)
            Light light = scene.CreateLight("MainLight");
            light.Position = new Vector3(20, 80, 50);

            // create a prefab plane
            Entity plane = scene.CreateEntity("Plane", PrefabEntity.Plane);
            // give the plan a texture
            plane.MaterialName = "Transparency/BumpyMetal";

            // create an entity from a model
            Entity knot = scene.CreateEntity("Knot", "knot.mesh");
            knot.MaterialName = "Transparency/Knot";

            // attach the two new entities to the root of the scene
            SceneNode rootNode = scene.RootSceneNode;
            rootNode.AttachObject(plane);
            rootNode.AttachObject(knot);

            Entity clone = null;
            for(int i = 0; i < 10; i++) {
                // create a new node under the root
                SceneNode node = scene.CreateSceneNode();

                // calculate a random position
                Vector3 nodePosition = new Vector3();
                nodePosition.x = MathUtil.SymmetricRandom() * 500.0f;
                nodePosition.y = MathUtil.SymmetricRandom() * 500.0f;
                nodePosition.z = MathUtil.SymmetricRandom() * 500.0f;

                // set the new position
                node.Position = nodePosition;

                // attach this node to the root node
                rootNode.AddChild(node);

                // clone the knot
                string cloneName = string.Format("Knot{0}", i);
                clone = knot.Clone(cloneName);

                // add the cloned knot to the scene
                node.AttachObject(clone);
            }
        }
        #endregion
    }
}