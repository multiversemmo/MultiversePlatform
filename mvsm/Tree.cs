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
using System.Diagnostics;
using Axiom.MathLib;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Utility;
using Multiverse;
using Multiverse.CollisionLib;
using System.Security.Cryptography;

namespace Axiom.SceneManagers.Multiverse
{
    public class TreeRenderArgs 
    {
        public int AlphaTestValue;
        public int LOD;
        public bool Active;

        public TreeRenderArgs()
        {
        }

    }

	/// <summary>
	/// Summary description for Tree.
	/// </summary>
	public class Tree : IDisposable
	{
		private Vector3 location;

        protected float[] billboard0;
        protected float[] billboard1;

		private SpeedTreeWrapper speedTree;

		private bool renderFlag = false;

        private AxisAlignedBox bounds;

        private TreeGroup group;

        private bool disposed = false;

        // These values are returned by speedtree.GetGeometry().  They only change when
        //  the camera moves.
        protected TreeRenderArgs branchRenderArgs = new TreeRenderArgs();
        protected TreeRenderArgs frondRenderArgs = new TreeRenderArgs();
        protected TreeRenderArgs leaf0RenderArgs = new TreeRenderArgs();
        protected TreeRenderArgs leaf1RenderArgs = new TreeRenderArgs();
        protected TreeRenderArgs billboard0RenderArgs = new TreeRenderArgs();
        protected TreeRenderArgs billboard1RenderArgs = new TreeRenderArgs();

		public Tree(TreeGroup group, SpeedTreeWrapper speedTree, Vector3 location)
		{
            this.group = group;
			this.location = location;

			this.speedTree = speedTree;

            billboard0 = new float[20];
            billboard1 = new float[20];

			// set location for the instance
			speedTree.TreePosition = SpeedTreeUtil.ToSpeedTree(location);

			// set bounding box
			AxisAlignedBox stbox = SpeedTreeUtil.FromSpeedTree(speedTree.BoundingBox);

			this.bounds = new AxisAlignedBox(stbox.Minimum  + location, stbox.Maximum + location);
		}

        /// <summary>
        /// This method must be called whenever the camera changes.  It gets the new
        ///  camera dependent rendering arguments from SpeedTree.
        /// </summary>
        protected void UpdateRenderArgs()
        {
            // let speedtree compute the LOD for this tree
            speedTree.ComputeLodLevel();

            // update geometry for all parts
            TreeGeometry geometry = group.Geometry;
            speedTree.GetGeometry(geometry, SpeedTreeWrapper.GeometryFlags.AllGeometry, -1, -1, -1);

            // copy out branch args
            branchRenderArgs.AlphaTestValue = (int)geometry.BranchAlphaTestValue;
            branchRenderArgs.LOD = geometry.Branches.DiscreteLodLevel;
            branchRenderArgs.Active = branchRenderArgs.AlphaTestValue < 255;

            // copy out frond args
            frondRenderArgs.AlphaTestValue = (int)geometry.FrondAlphaTestValue;
            frondRenderArgs.LOD = geometry.Fronds.DiscreteLodLevel;
            frondRenderArgs.Active = frondRenderArgs.AlphaTestValue < 255;

            // copy out leaf args
            leaf0RenderArgs.AlphaTestValue = (int)geometry.Leaves0.AlphaTestValue;
            leaf0RenderArgs.LOD = geometry.Leaves0.DiscreteLodLevel;
            leaf0RenderArgs.Active = geometry.Leaves0.IsActive;
            leaf1RenderArgs.AlphaTestValue = (int)geometry.Leaves1.AlphaTestValue;
            leaf1RenderArgs.LOD = geometry.Leaves1.DiscreteLodLevel;
            leaf1RenderArgs.Active = geometry.Leaves1.IsActive;

            // copy out billboard args
            // NOTE - billboards dont have LOD
            billboard0RenderArgs.AlphaTestValue = (int)geometry.Billboard0.AlphaTestValue;
            billboard0RenderArgs.Active = geometry.Billboard0.IsActive;
            billboard0RenderArgs.LOD = 0;
            billboard1RenderArgs.AlphaTestValue = (int)geometry.Billboard1.AlphaTestValue;
            billboard1RenderArgs.Active = geometry.Billboard1.IsActive;
            billboard1RenderArgs.LOD = 0;

            if (billboard0RenderArgs.Active)
            {
                FillBillboardBuffer(billboard0, geometry.Billboard0);
            }
            if (billboard1RenderArgs.Active)
            {
                FillBillboardBuffer(billboard1, geometry.Billboard1);
            }

            group.AddVisible(this, branchRenderArgs.Active, frondRenderArgs.Active, leaf0RenderArgs.Active || leaf1RenderArgs.Active, billboard0RenderArgs.Active || billboard1RenderArgs.Active);
        }

		public RenderOperation CreateBillboardBuffer()
		{
            RenderOperation renderOp = new RenderOperation();
            renderOp.operationType = OperationType.TriangleFan;
            renderOp.useIndices = false;

			VertexData vertexData = new VertexData();

			vertexData.vertexCount = 4;
			vertexData.vertexStart = 0;

            // free the original vertex declaration to avoid a leak
            HardwareBufferManager.Instance.DestroyVertexDeclaration(vertexData.vertexDeclaration);

            // use common vertex declaration
            vertexData.vertexDeclaration = TreeGroup.BillboardVertexDeclaration;

			// create the hardware vertex buffer and set up the buffer binding
			HardwareVertexBuffer hvBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
				vertexData.vertexDeclaration.GetVertexSize(0), vertexData.vertexCount,	
				BufferUsage.DynamicWriteOnly, false);

			vertexData.vertexBufferBinding.SetBinding(0, hvBuffer);

			renderOp.vertexData = vertexData;

            return renderOp;
		}

		public void FillBillboardBuffer(float [] buffer, TreeGeometry.Billboard billboard)
		{
			unsafe
			{
				float * srcCoord = billboard.Coords;
				float * srcTexCoord = billboard.TexCoords;
			
				// Position
				buffer[0] = srcCoord[0] + location.x;
				buffer[1] = srcCoord[1] + location.y;
				buffer[2] = srcCoord[2] + location.z;
	
				// Texture
				buffer[3] = srcTexCoord[0];
				buffer[4] = srcTexCoord[1];


				// Position
				buffer[5] = srcCoord[3] + location.x;
				buffer[6] = srcCoord[4] + location.y;
				buffer[7] = srcCoord[5] + location.z;
	
				// Texture
				buffer[8] = srcTexCoord[2];
				buffer[9] = srcTexCoord[3];


				// Position
				buffer[10] = srcCoord[6] + location.x;
				buffer[11] = srcCoord[7] + location.y;
				buffer[12] = srcCoord[8] + location.z;
	
				// Texture
				buffer[13] = srcTexCoord[4];
				buffer[14] = srcTexCoord[5];


				// Position
				buffer[15] = srcCoord[9] + location.x;
				buffer[16] = srcCoord[10] + location.y;
				buffer[17] = srcCoord[11] + location.z;

				// Texture
				buffer[18] = srcTexCoord[6];
				buffer[19] = srcTexCoord[7];

			}

			return;
		}

        // formerly UpdateVisibility(Camera camera)
        public void CameraChange(Camera camera)
        {
            // we need to draw the tree if it intersects with the camera frustrum
            renderFlag = camera.IsObjectVisible(bounds);
            if (renderFlag)
            {
                // if the tree is still visible, update the camera dependent rendering args
                UpdateRenderArgs();
            }
        }

		public bool RenderFlag 
		{
			get 
			{
				return renderFlag;
			}
		}

        public AxisAlignedBox Bounds
        {
            get
            {
                return bounds;
            }
        }

		public void FindObstaclesInBox(AxisAlignedBox box,
									   CollisionTileManager.AddTreeObstaclesCallback callback)
		{
			if (box.Intersects(location))
				callback(speedTree);
		}

        public TreeRenderArgs BranchRenderArgs
        {
            get
            {
                return branchRenderArgs;
            }
        }

        public TreeRenderArgs FrondRenderArgs
        {
            get
            {
                return frondRenderArgs;
            }
        }

        public TreeRenderArgs Leaf0RenderArgs
        {
            get
            {
                return leaf0RenderArgs;
            }
        }

        public TreeRenderArgs Leaf1RenderArgs
        {
            get
            {
                return leaf1RenderArgs;
            }
        }

        public TreeRenderArgs Billboard0RenderArgs
        {
            get
            {
                return billboard0RenderArgs;
            }
        }

        public TreeRenderArgs Billboard1RenderArgs
        {
            get
            {
                return billboard1RenderArgs;
            }
        }

        public Vector3 Location
        {
            get
            {
                return location;
            }
        }

        public SpeedTreeWrapper SpeedTree
        {
            get
            {
                return speedTree;
            }
        }

        public float[] Billboard0
        {
            get
            {
                return billboard0;
            }
        }

        public float[] Billboard1
        {
            get
            {
                return billboard1;
            }
        }

		#region IDisposable Members

		public void Dispose()
		{
            Debug.Assert(!disposed);
            disposed = true;
		}

		#endregion

	}
}
