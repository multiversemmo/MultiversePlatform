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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Axiom.Core;
using Axiom.Collections;
using Axiom.MathLib;
using Axiom.Graphics;

namespace Axiom.SceneManagers.Multiverse {
    public class StitchRenderable : IRenderable, IDisposable {
        #region Fields

        /// <summary>
        ///    Reference to the parent TerrainPatch.
        /// </summary>
        protected TerrainPatch parent;
        /// <summary>
        ///    Detail to be used for rendering this sub entity.
        /// </summary>
        protected SceneDetailLevel renderDetail;
		/// <summary>
		///		Flag indicating whether this sub entity should be rendered or not.
		/// </summary>
		protected bool isVisible;

		/// <summary>
		/// the renderop holds the geometry for the stitch
		/// </summary>
		protected RenderOperation renderOp;

		protected Hashtable customParams;

		//
		// These are used to check if the stitching needs to be redone
		//
		protected int numSamples;
		protected int southMetersPerSample;
		protected int eastMetersPerSample;

        #endregion Fields

        #region Constructor

        /// <summary>
        ///		Internal constructor, only allows creation of StitchRenderables within the scene manager.
        /// </summary>
        internal StitchRenderable(TerrainPatch terrainPatch, VertexData vertexData, IndexData indexData, 
			int numSamples, int southMetersPerSample, int eastMetersPerSample) {

            renderDetail = SceneDetailLevel.Solid;
			parent = terrainPatch;

			renderOp = new RenderOperation();
			renderOp.operationType = OperationType.TriangleList;
			renderOp.useIndices = true;
			renderOp.vertexData = vertexData;
			renderOp.indexData = indexData;

			isVisible = true;
			this.numSamples = numSamples;
			this.southMetersPerSample = southMetersPerSample;
			this.eastMetersPerSample = eastMetersPerSample;
        }

        #endregion

        #region Properties

		/// <summary>
		///		Gets a flag indicating whether or not this sub entity should be rendered or not.
		/// </summary>
		public bool IsVisible {
			get {
				return isVisible;
			}
		}

        /// <summary>
        ///		Gets/Sets the parent terrainPatch.
        /// </summary>
        public TerrainPatch Parent {
            get { 
                return parent; 
            }
        }

        #endregion

		#region Methods

		public bool IsValid(int numSamples, int southMetersPerSample, int eastMetersPerSample)
		{
			return ( numSamples == this.numSamples ) && 
				( southMetersPerSample == this.southMetersPerSample ) &&
				( eastMetersPerSample == this.eastMetersPerSample );
		}

		#endregion Methods

        #region IRenderable Members

		public bool CastsShadows {
			get {
				return parent.CastShadows;;
			}
		}

        public Material Material {
            get { 
                return parent.Material; 
            }
        }

        public bool NormalizeNormals {
            get {
                return false;
            }
        }

        public Technique Technique {
            get {
                return parent.Technique;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        public void GetRenderOperation(RenderOperation op) {
			Debug.Assert(renderOp.vertexData != null, "attempting to render stitch with no vertexData");
			Debug.Assert(renderOp.indexData != null, "attempting to render stitch with no indexData");

			op.useIndices = this.renderOp.useIndices;	
			op.operationType = this.renderOp.operationType;
			op.vertexData = this.renderOp.vertexData;
			op.indexData = this.renderOp.indexData;
        }

        Material IRenderable.Material {
            get { 
                return parent.Material; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrices"></param>
        public void GetWorldTransforms(Matrix4[] matrices) {
            matrices[0] = parent.ParentFullTransform;
        }

        /// <summary>
        /// 
        /// </summary>
        public ushort NumWorldTransforms {
            get {
                return 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool UseIdentityProjection {
            get {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool UseIdentityView {
            get {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public SceneDetailLevel RenderDetail {
            get { 
                return renderDetail;	
            }
            set {
                renderDetail = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public float GetSquaredViewDepth(Camera camera) {
            // get the parent entitie's parent node
            Node node = parent.ParentNode;

            Debug.Assert(node != null);

            return node.GetSquaredViewDepth(camera);
        }

        /// <summary>
        /// 
        /// </summary>
        public Quaternion WorldOrientation {
            get {
                // get the parent entitie's parent node
                Node node = parent.ParentNode;

                Debug.Assert(node != null);

                return parent.ParentNode.DerivedOrientation;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 WorldPosition {
            get {
                // get the parent entitie's parent node
                Node node = parent.ParentNode;

                Debug.Assert(node != null);

                return parent.ParentNode.DerivedPosition;
            }
        }

        /// <summary>
        ///    
        /// </summary>
        public List<Light> Lights {
            get {
                // get the parent entitie's parent node
                Node node = parent.ParentNode;

                Debug.Assert(node != null);

                return parent.ParentNode.Lights;
            }
        }

		public Vector4 GetCustomParameter(int index) {
			if( ( customParams == null ) || ( customParams[index] == null ) ) {
				throw new Exception("A parameter was not found at the given index");
			}
			else {
				return (Vector4)customParams[index];
			}
		}

		public void SetCustomParameter(int index, Vector4 val) {
			if ( customParams == null ) 
			{
				customParams = new Hashtable();
			}
			customParams[index] = val;
		}

		public void UpdateCustomGpuParameter(GpuProgramParameters.AutoConstantEntry entry, GpuProgramParameters gpuParams) {
			if( ( customParams != null ) && ( customParams[entry.data] != null ) ) {
				gpuParams.SetConstant(entry.index, (Vector4)customParams[entry.data]);
			}
		}

        #endregion

		#region IDisposable Members

		public void Dispose()
		{
			renderOp.indexData = null;

			if ( renderOp.vertexData != null ) 
			{
				renderOp.vertexData.vertexBufferBinding.GetBuffer(0).Dispose();
				renderOp.vertexData.vertexBufferBinding.SetBinding(0, null);
				renderOp.vertexData = null;
			}
		}

		#endregion
	}
}
