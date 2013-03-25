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

using System;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
    public class BezierPatch : DemoBase {
        // --- Fields ---
        #region Private Fields
        private VertexDeclaration patchDeclaration;
        private float timeLapse;
        private float factor;
        private bool isWireframe;
        private PatchMesh patch;
        private Entity patchEntity;
        #endregion Private Fields

        #region Private Structs
        private struct PatchVertex {
            public float X, Y, Z;
            public float Nx, Ny, Nz;
            public float U, V;
        }

        #endregion Private Structs

        // --- Protected Override Methods ---
        #region CreateScene()
        // Just override the mandatory create scene method
        protected override void CreateScene() {
            // Set ambient light
            scene.AmbientLight = new ColorEx(0.2f, 0.2f, 0.2f);

            // Create point light
            Light light = scene.CreateLight("MainLight");

            // Accept default settings: point light, white diffuse, just set position.
            // I could attach the light to a SceneNode if I wanted it to move automatically with
            // other objects, but I don't.
            light.Type = LightType.Directional;
            light.Direction = new Vector3(-0.5f, -0.5f, 0);

            // Create patch with positions, normals, and 1 set of texcoords
            patchDeclaration = HardwareBufferManager.Instance.CreateVertexDeclaration();
            patchDeclaration.AddElement(0, 0, VertexElementType.Float3, VertexElementSemantic.Position);
            patchDeclaration.AddElement(0, 12, VertexElementType.Float3, VertexElementSemantic.Normal);
            patchDeclaration.AddElement(0, 24, VertexElementType.Float2, VertexElementSemantic.TexCoords, 0);

            // Patch data
            PatchVertex[] patchVertices = new PatchVertex[9];

            patchVertices[0].X = -500; patchVertices[0].Y = 200; patchVertices[0].Z = -500;
            patchVertices[0].Nx = -0.5f; patchVertices[0].Ny = 0.5f; patchVertices[0].Nz = 0;
            patchVertices[0].U = 0; patchVertices[0].V = 0;

            patchVertices[1].X = 0; patchVertices[1].Y = 500; patchVertices[1].Z = -750;
            patchVertices[1].Nx = 0; patchVertices[1].Ny = 0.5f; patchVertices[1].Nz = 0;
            patchVertices[1].U = 0.5f; patchVertices[1].V = 0;

            patchVertices[2].X = 500; patchVertices[2].Y = 1000; patchVertices[2].Z = -500;
            patchVertices[2].Nx = 0.5f; patchVertices[2].Ny = 0.5f; patchVertices[2].Nz = 0;
            patchVertices[2].U = 1; patchVertices[2].V = 0;

            patchVertices[3].X = -500; patchVertices[3].Y = 0; patchVertices[3].Z = 0;
            patchVertices[3].Nx = -0.5f; patchVertices[3].Ny = 0.5f; patchVertices[3].Nz = 0;
            patchVertices[3].U = 0; patchVertices[3].V = 0.5f;

            patchVertices[4].X = 0; patchVertices[4].Y = 500; patchVertices[4].Z = 0;
            patchVertices[4].Nx = 0; patchVertices[4].Ny = 0.5f; patchVertices[4].Nz = 0;
            patchVertices[4].U = 0.5f; patchVertices[4].V = 0.5f;

            patchVertices[5].X = 500; patchVertices[5].Y = -50; patchVertices[5].Z = 0;
            patchVertices[5].Nx = 0.5f; patchVertices[5].Ny = 0.5f; patchVertices[5].Nz = 0;
            patchVertices[5].U = 1; patchVertices[5].V = 0.5f;

            patchVertices[6].X = -500; patchVertices[6].Y = 0; patchVertices[6].Z = 500;
            patchVertices[6].Nx = -0.5f; patchVertices[6].Ny = 0.5f; patchVertices[6].Nz = 0;
            patchVertices[6].U = 0; patchVertices[6].V = 1;

            patchVertices[7].X = 0; patchVertices[7].Y = 500; patchVertices[7].Z = 500;
            patchVertices[7].Nx = 0; patchVertices[7].Ny = 0.5f; patchVertices[7].Nz = 0;
            patchVertices[7].U = 0.5f; patchVertices[7].V = 1;

            patchVertices[8].X = 500; patchVertices[8].Y = 200; patchVertices[8].Z = 800;
            patchVertices[8].Nx = 0.5f; patchVertices[8].Ny = 0.5f; patchVertices[8].Nz = 0;
            patchVertices[8].U = 1; patchVertices[8].V = 1;

            patch = MeshManager.Instance.CreateBezierPatch("Bezier1", patchVertices, patchDeclaration, 3, 3, 5, 5, VisibleSide.Both, BufferUsage.StaticWriteOnly, BufferUsage.DynamicWriteOnly, true, true);

            // Start patch a 0 detail
            patch.SetSubdivision(0);

            // Create entity based on patch
            patchEntity = scene.CreateEntity("Entity1", "Bezier1");

            Material material = (Material)MaterialManager.Instance.Create("TextMat");
            material.GetTechnique(0).GetPass(0).CreateTextureUnitState("BumpyMetal.jpg");
            patchEntity.MaterialName = "TextMat";

            // Attach the entity to the root of the scene
            scene.RootSceneNode.AttachObject(patchEntity);

            camera.Position = new Vector3(500, 500, 1500);
            camera.LookAt(new Vector3(0, 200, -300));
        }
        #endregion CreateScene()

        // --- Protected Override Event Handlers ---
        #region bool OnFrameStarted(Object source, FrameEventArgs e)

        // Event handler to add ability to alter subdivision
        protected override void OnFrameStarted(Object source, FrameEventArgs e) {
            timeLapse += e.TimeSinceLastFrame;

            // Progressively grow the patch
            if(timeLapse > 1.0f) {
                factor += 0.2f;

                if(factor > 1.0f) {
                    isWireframe = !isWireframe;
                    patchEntity.RenderDetail = (isWireframe ? SceneDetailLevel.Wireframe : SceneDetailLevel.Solid);
                    factor = 0.0f;
                }

                patch.SetSubdivision(factor);
                window.DebugText = "Bezier subdivision factor: " + factor;
                timeLapse = 0.0f;
            }

            // Call default
            base.OnFrameStarted(source, e);
        }

        #endregion bool OnFrameStarted(Object source, FrameEventArgs e)
    }
}
