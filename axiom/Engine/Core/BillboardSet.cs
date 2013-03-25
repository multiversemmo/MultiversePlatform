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
using System.Drawing;
using Axiom.Collections;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;

namespace Axiom.Core {
    /// <summary>
    ///		A collection of billboards (faces which are always facing the camera) with the same (default) dimensions, material
    ///		and which are fairly close proximity to each other.
    ///	 </summary>
    ///	 <remarks>
    ///		Billboards are rectangles made up of 2 tris which are always facing the camera. They are typically used
    ///		for special effects like particles. This class collects together a set of billboards with the same (default) dimensions,
    ///		material and relative locality in order to process them more efficiently. The entire set of billboards will be
    ///		culled as a whole (by default, although this can be changed if you want a large set of billboards
    ///		which are spread out and you want them culled individually), individual Billboards have locations which are relative to the set (which itself derives it's
    ///		position from the SceneNode it is attached to since it is a SceneObject), they will be rendered as a single rendering operation,
    ///		and some calculations will be sped up by the fact that they use the same dimensions so some workings can be reused.
    ///		<p/>
    ///		A BillboardSet can be created using the SceneManager.CreateBillboardSet method. They can also be used internally
    ///		by other classes to create effects.
    /// </remarks>
    public class BillboardSet : MovableObject, IRenderable {
        #region Fields

        /// <summary>Bounds of all billboards in this set</summary>
        protected AxisAlignedBox aab = new AxisAlignedBox();
        /// <summary>Origin of each billboard</summary>
        protected BillboardOrigin originType = BillboardOrigin.Center;
        protected BillboardRotationType rotationType = BillboardRotationType.Texcoord;
        /// <summary>Default width/height of each billboard.</summary>
        protected float defaultParticleWidth = 100;
        protected float defaultParticleHeight = 100;
        /// <summary>Name of the material to use</summary>
        protected string materialName = "BaseWhite";
        /// <summary>Reference to the material to use</summary>
        protected Material material;
        /// <summary></summary>
        protected bool allDefaultSize = true;
        protected bool allDefaultRotation = true;
        /// <summary></summary>
        protected bool autoExtendPool = true;
		/// <summary>True if particles follow the object the
		/// ParticleSystem is attached to.</summary>
        protected bool worldSpace = false;

        // various collections for pooling billboards
        protected List<Billboard> activeBillboards = new List<Billboard>();
        protected List<Billboard> freeBillboards = new List<Billboard>();
        protected List<Billboard> billboardPool = new List<Billboard>();

        // Geometry data.
        protected VertexData vertexData = null;
        protected IndexData indexData = null;

        /// <summary>Indicates whether or not each billboard should be culled individually.</summary>
        protected bool cullIndividual = false;
        /// <summary>Type of billboard to render.</summary>
        protected BillboardType billboardType = BillboardType.Point;
        /// <summary>Common direction for billboard oriented with type Common.</summary>
        protected Vector3 commonDirection = Vector3.UnitZ;
        /// <summary>Common up vector for billboard oriented with type Perpendicular.</summary>
        protected Vector3 commonUpVector = Vector3.UnitY;
        /// <summary>The local bounding radius of this object.</summary>
        protected float boundingRadius;
        /// <summary>The distance particles are offset towards the camera.</summary>
        protected float depthOffset;
		
        protected int numVisibleBillboards;

		/// <summary>
		///		Are tex coords fixed?  If not they have been modified.
		/// </summary>
		protected bool fixedTextureCoords;

        // Temporary matrix for checking billboard visible
        protected Matrix4[] world = new Matrix4[1];
        protected Sphere sphere = new Sphere();

        // used to keep track of current index in GenerateVertices
        protected int posIndex = 0;
        protected int colorIndex = 0;
		protected int texIndex = 0;

        protected bool pointRendering = false;
        protected bool accurateFacing = false;
        protected IntPtr lockPtr = IntPtr.Zero;
        protected int ptrOffset = 0;
        protected Vector3[] vOffset = new Vector3[4];
        protected Camera currentCamera;
        protected float leftOff, rightOff, topOff, bottomOff;
        protected Vector3 camX, camY, camDir;
        protected Quaternion camQ;
        protected Vector3 camPos;

        private bool buffersCreated = false;
        private int poolSize = 0;
        private bool externalData = false;
        List<RectangleF> textureCoords = new List<RectangleF>();

        protected HardwareVertexBuffer mainBuffer;


        protected Hashtable customParams = new Hashtable(20);

		// Template texcoord data
		float[] texData = new float[8] {
										   -0.5f, 0.5f,
										   0.5f, 0.5f,
										   -0.5f,-0.5f,
										   0.5f,-0.5f };

        #endregion Fields

        #region Constructors

        /// <summary>
        ///		Public constructor.  Should not be created manually, must be created using a SceneManager.
        /// </summary>
        internal BillboardSet(string name, int poolSize) {
            this.name = name;
            this.PoolSize = poolSize;

            SetDefaultDimensions(100, 100);
            this.MaterialName = "BaseWhite";
            castShadows = false;
            SetTextureStacksAndSlices(1, 1);
        }
        /// <summary>
        ///		Public constructor.  Should not be created manually, must be created using a SceneManager.
        /// </summary>
        internal BillboardSet(string name, int poolSize, bool externalData) {
            this.name = name;
            this.PoolSize = poolSize;
            this.externalData = externalData;

            SetDefaultDimensions(100, 100);
            this.MaterialName = "BaseWhite";
            castShadows = false;
            SetTextureStacksAndSlices(1, 1);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Generate the vertices for all the billboards relative to the camera
        ///     Also take the opportunity to update the vertex colours
        ///     May as well do it here to save on loops elsewhere
        /// </summary>
        internal void BeginBillboards() {
            // Make sure we aren't calling this more than once
            Debug.Assert(lockPtr == IntPtr.Zero);

            /* NOTE: most engines generate world coordinates for the billboards
               directly, taking the world axes of the camera as offsets to the
               center points. I take a different approach, reverse-transforming
               the camera world axes into local billboard space.
               Why?
               Well, it's actually more efficient this way, because I only have to
               reverse-transform using the billboardset world matrix (inverse)
               once, from then on it's simple additions (assuming identically
               sized billboards). If I transformed every billboard center by it's
               world transform, that's a matrix multiplication per billboard
               instead.
               I leave the final transform to the render pipeline since that can
               use hardware TnL if it is available.
            */

            // create vertex and index buffers if they haven't already been
            if (!buffersCreated)
                CreateBuffers();

		    // Only calculate vertex offets et al if we're not point rendering
		    if (!pointRendering)
		    {
    			// Get offsets for origin type
	    		GetParametricOffsets(out leftOff, out rightOff, out topOff, out bottomOff);

		    	// Generate axes etc up-front if not oriented per-billboard
			    if (billboardType != BillboardType.OrientedSelf &&
				    billboardType != BillboardType.PerpendicularSelf && 
				    !(accurateFacing && billboardType != BillboardType.PerpendicularCommon))
			    {
				    GenerateBillboardAxes(ref camX, ref camY);

				    /* If all billboards are the same size we can precalculate the
				       offsets and just use '+' instead of '*' for each billboard,
				       and it should be faster.
				    */
				    GenerateVertexOffsets(leftOff, rightOff, topOff, bottomOff,
					                      defaultParticleWidth, defaultParticleHeight, 
                                          ref camX, ref camY, vOffset);

			    }
		    }

            // Init num visible
            numVisibleBillboards = 0;

            // Lock the buffer
            lockPtr = mainBuffer.Lock(BufferLocking.Discard);
            ptrOffset = 0;
        }

        internal void InjectBillboard(Billboard bb)
        {
            // Skip if not visible (NB always true if not bounds checking individual billboards)
            if (!IsBillboardVisible(currentCamera, bb))
                return;

            if (!pointRendering && 
                (billboardType == BillboardType.OrientedSelf ||
				 billboardType == BillboardType.PerpendicularSelf ||
				 (accurateFacing && billboardType != BillboardType.PerpendicularCommon)))
            {
                // Have to generate axes & offsets per billboard
                GenerateBillboardAxes(ref camX, ref camY, bb);
            }

		    // If they're all the same size or we're point rendering
            if (allDefaultSize || pointRendering)
            {
                /* No per-billboard checking, just blast through.
                   Saves us an if clause every billboard which may
                   make a difference.
                */

                if (!pointRendering &&
                    (billboardType == BillboardType.OrientedSelf ||
	    			 billboardType == BillboardType.PerpendicularSelf ||
		    		 (accurateFacing && billboardType != BillboardType.PerpendicularCommon)))
                {
                    GenerateVertexOffsets(leftOff, rightOff, topOff, bottomOff,
                                          defaultParticleWidth, defaultParticleHeight, 
                                          ref camX, ref camY, vOffset);
                }
                GenerateVertices(vOffset, bb);
            }
            else // not all default size and not point rendering
            {
                Vector3[] vOwnOffset = new Vector3[4];
                // If it has own dimensions, or self-oriented, gen offsets
                if (billboardType == BillboardType.OrientedSelf ||
	    		    billboardType == BillboardType.PerpendicularSelf ||
                    bb.HasOwnDimensions ||
                    (accurateFacing && billboardType != BillboardType.PerpendicularCommon))
                {
                    // Generate using own dimensions
                    GenerateVertexOffsets(leftOff, rightOff, topOff, bottomOff,
                                          bb.Width, bb.Height, 
                                          ref camX, ref camY, vOwnOffset);
                    // Create vertex data
                    GenerateVertices(vOwnOffset, bb);
                }
                else // Use default dimension, already computed before the loop, for faster creation
                {
                    GenerateVertices(vOffset, bb);
                }
            }
            // Increment visibles
            numVisibleBillboards++;
        }

        internal void EndBillboards() {
            // Make sure we aren't double unlocking
            Debug.Assert(lockPtr != IntPtr.Zero);
            mainBuffer.Unlock();
            lockPtr = IntPtr.Zero;
        }

        protected void SetBounds(AxisAlignedBox box, float radius)
	    {
    		aab = box;
	    	boundingRadius = radius;
	    }

        /// <summary>
        ///		Callback used by Billboards to notify their parent that they have been resized.
        /// </summary>
        protected internal void NotifyBillboardResized() {
            allDefaultSize = false;
        }

        /// <summary>
        ///		Callback used by Billboards to notify their parent that they have been resized.
        /// </summary>
        protected internal void NotifyBillboardRotated() {
            allDefaultRotation = false;
        }

		/// <summary>
		///		Notifies the billboardset that texture coordinates will be modified
		///		for this set.
		/// </summary>
		protected internal void NotifyBillboardTextureCoordsModified() {
			fixedTextureCoords = false;
		}

        /// <summary>
        ///		Internal method for increasing pool size.
        /// </summary>
        /// <param name="size"></param>
        protected virtual void IncreasePool(int size) {
            int oldSize = billboardPool.Count;

            // expand the capacity a bit
            billboardPool.Capacity += size;

            // add fresh Billboard objects to the new slots
            for (int i = oldSize; i < size; ++i)
                billboardPool.Add(new Billboard());
        }

        /// <summary>
        ///		Determines whether the supplied billboard is visible in the camera or not.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="billboard"></param>
        /// <returns></returns>
        protected bool IsBillboardVisible(Camera camera, Billboard billboard) {
            // if not culling each one, return true always
            if(!cullIndividual)
                return true;

            // get the world matrix of this billboard set
            GetWorldTransforms(world);

            // get the center of the bounding sphere
            sphere.Center = world[0] * billboard.Position;

            // calculate the radius of the bounding sphere for the billboard
            if(billboard.HasOwnDimensions) {
                sphere.Radius = MathUtil.Max(billboard.Width, billboard.Height);
            }
            else {
                sphere.Radius = MathUtil.Max(defaultParticleWidth, defaultParticleHeight);
            }

            // finally, see if the sphere is visible in the camera
            return camera.IsObjectVisible(sphere);
        }

        protected void SetTextureStacksAndSlices(int stacks, int slices)
        {
            if (stacks == 0)
                stacks = 1;
            if (slices == 0)
                slices = 1;
            //  clear out any previous allocation
            textureCoords.Clear();
            //  make room
            textureCoords.Capacity = stacks * slices;
            while (textureCoords.Count < stacks * slices)
                textureCoords.Add(new RectangleF());
            ushort coordIndex = 0;
            //  spread the U and V coordinates across the rects
            for (uint v = 0; v < stacks; ++v) {
                //  (float)X / X is guaranteed to be == 1.0f for X up to 8 million, so
                //  our range of 1..256 is quite enough to guarantee perfect coverage.
                float top = (float)v / (float)stacks;
                float bottom = ((float)v + 1) / (float)stacks;
                for (uint u = 0; u < slices; ++u) {
                    RectangleF r = new RectangleF();
                    r.X = (float)u / (float)slices;
                    r.Y = top;
                    r.Width = ((float)u + 1) / (float)slices - r.X;
                    r.Height = bottom - top;
                    textureCoords[coordIndex] = r;
                    ++coordIndex;
                }
            }
            Debug.Assert(coordIndex == stacks * slices);
        }

        /// <summary>
        ///		Overloaded method.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        protected virtual void GenerateBillboardAxes(ref Vector3 x, ref Vector3 y) {
            GenerateBillboardAxes(ref x, ref y, null);
        }

        /// <summary>
        ///		Generates billboard corners.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="billboard"></param>
        /// <remarks>Billboard param only required for type OrientedSelf</remarks>
        protected virtual void GenerateBillboardAxes(ref Vector3 x, ref Vector3 y, Billboard bb) {
            // If we're using accurate facing, recalculate camera direction per BB
            if (accurateFacing && 
                (billboardType == BillboardType.Point || 
                 billboardType == BillboardType.OrientedCommon ||
	    	     billboardType == BillboardType.OrientedSelf))
            {
                // cam -> bb direction
                camDir = bb.Position - camPos;
                camDir.Normalize();
            }


            switch (billboardType)
            {
                case BillboardType.Point:
                    if (accurateFacing) {
                        // Point billboards will have 'up' based on but not equal to cameras
                        y = camQ * Vector3.UnitY;
                        x = camDir.Cross(y);
                        x.Normalize();
                        y = x.Cross(camDir); // both normalised already
                    } else {
                        // Get camera axes for X and Y (depth is irrelevant)
                        x = camQ * Vector3.UnitX;
                        y = camQ * Vector3.UnitY;
                    }
                    break;

                case BillboardType.OrientedCommon:
                    // Y-axis is common direction
                    // X-axis is cross with camera direction
                    y = commonDirection;
                    x = camDir.Cross(y);
                    x.Normalize();
                    break;

                case BillboardType.OrientedSelf:
                    // Y-axis is direction
                    // X-axis is cross with camera direction
                    // Scale direction first
                    y = bb.Direction;
                    x = camDir.Cross(y);
                    x.Normalize();
                    break;

                case BillboardType.PerpendicularCommon:
                    // X-axis is up-vector cross common direction
                    // Y-axis is common direction cross X-axis
                    x = commonUpVector.Cross(commonDirection);
                    y = commonDirection.Cross(x);
                    break;

                case BillboardType.PerpendicularSelf:
                    // X-axis is up-vector cross own direction
                    // Y-axis is own direction cross X-axis
                    x = commonUpVector.Cross(bb.Direction);
                    x.Normalize();
                    y = bb.Direction.Cross(x); // both should be normalised
                    break;
            }


#if NOT
            // Default behavior is that billboards are in local node space
            // so orientation of camera (in world space) must be reverse-transformed 
            // into node space to generate the axes
            Quaternion invTransform = parentNode.DerivedOrientation.Inverse();
            Quaternion camQ = Quaternion.Zero;

            switch (billboardType) {
                case BillboardType.Point:
                    // Get camera world axes for X and Y (depth is irrelevant)
                    camQ = camera.DerivedOrientation;
                    // Convert into billboard local space
                    camQ = invTransform * camQ;
                    x = camQ * Vector3.UnitX;
                    y = camQ * Vector3.UnitY;
                    break;
                case BillboardType.OrientedCommon:
                    // Y-axis is common direction
                    // X-axis is cross with camera direction 
                    y = commonDirection;
                    y.Normalize();
                    // Convert into billboard local space
                    camQ = invTransform * camQ;
                    x = camQ * camera.DerivedDirection.Cross(y);
                    x.Normalize();
                    break;
                case BillboardType.OrientedSelf:
                    // Y-axis is direction
                    // X-axis is cross with camera direction 
                    y = billboard.Direction;
                    // Convert into billboard local space
                    camQ = invTransform * camQ;
                    x = camQ * camera.DerivedDirection.Cross(y);
                    x.Normalize();
                    break;
                case BillboardType.PerpendicularCommon:
                    // X-axis is common direction cross common up vector
                    // Y-axis is coplanar with common direction and common up vector
                    x = commonDirection.Cross(commonUpVector);
                    x.Normalize();
                    y = x.Cross(commonDirection);
                    y.Normalize();
                    break;
                case BillboardType.PerpendicularSelf:
                    // X-axis is direction cross common up vector
                    // Y-axis is coplanar with direction and common up vector
                    x = billboard.Direction.Cross(commonUpVector);
                    x.Normalize();
                    y = x.Cross(billboard.Direction);
                    y.Normalize();
                    break;
            }
#endif
        }

        /// <summary>
        ///		Generate parametric offsets based on the origin.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        protected void GetParametricOffsets(out float left, out float right, out float top, out float bottom) {
            left = 0.0f;
            right = 0.0f;
            top = 0.0f;
            bottom = 0.0f;

            switch(originType) {
                case BillboardOrigin.TopLeft:
                    left = 0.0f;
                    right = 1.0f;
                    top = 0.0f;
                    bottom = -1.0f;
                    break;

                case BillboardOrigin.TopCenter:
                    left = -0.5f;
                    right = 0.5f;
                    top = 0.0f;
                    bottom = 1.0f;
                    break;

                case BillboardOrigin.TopRight:
                    left = -1.0f;
                    right = 0.0f;
                    top = 0.0f;
                    bottom = -1.0f;
                    break;

                case BillboardOrigin.CenterLeft:
                    left = 0.0f;
                    right = 1.0f;
                    top = 0.5f;
                    bottom = -0.5f;
                    break;

                case BillboardOrigin.Center:
                    left = -0.5f;
                    right = 0.5f;
                    top = 0.5f;
                    bottom = -0.5f;
                    break;

                case BillboardOrigin.CenterRight:
                    left = -1.0f;
                    right = 0.0f;
                    top = 0.5f;
                    bottom = -0.5f;
                    break;

                case BillboardOrigin.BottomLeft:
                    left = 0.0f;
                    right = 1.0f;
                    top = 1.0f;
                    bottom = 0.0f;
                    break;

                case BillboardOrigin.BottomCenter:
                    left = -0.5f;
                    right = 0.5f;
                    top = 1.0f;
                    bottom = 0.0f;
                    break;

                case BillboardOrigin.BottomRight:
                    left = -1.0f;
                    right = 0.0f;
                    top = 1.0f;
                    bottom = 0.0f;
                    break;
            }
        }

        protected void GenerateVertices(Vector3[] offsets, Billboard bb) {
            uint color = Root.Instance.ConvertColor(bb.Color);
            // Texcoords
            Debug.Assert(bb.UseTexcoordRect || bb.TexcoordIndex < textureCoords.Count);
            RectangleF r = bb.UseTexcoordRect ? bb.TexcoordRect : textureCoords[bb.TexcoordIndex];

		    if (pointRendering)
		    {
                unsafe {
                    float *posPtr = (float *)lockPtr.ToPointer();
                    uint *colPtr = (uint *)posPtr;

			        // Single vertex per billboard, ignore offsets
			        // position
                    posPtr[ptrOffset++] = bb.Position.x;
                    posPtr[ptrOffset++] = bb.Position.y;
                    posPtr[ptrOffset++] = bb.Position.z;
                    colPtr[ptrOffset++] = color;
                    // No texture coords in point rendering
                }
		    }
		    else if (allDefaultRotation || bb.Rotation == 0)
            {
                unsafe {
                    float *posPtr = (float *)lockPtr.ToPointer();
                    uint *colPtr = (uint *)posPtr;
                    float *texPtr = (float *)posPtr;

                    // Left-top
                    // Positions
                    posPtr[ptrOffset++] = offsets[0].x + bb.Position.x;
                    posPtr[ptrOffset++] = offsets[0].y + bb.Position.y;
                    posPtr[ptrOffset++] = offsets[0].z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = r.Left;
                    texPtr[ptrOffset++] = r.Top;

                    // Right-top
                    // Positions
                    posPtr[ptrOffset++] = offsets[1].x + bb.Position.x;
                    posPtr[ptrOffset++] = offsets[1].y + bb.Position.y;
                    posPtr[ptrOffset++] = offsets[1].z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = r.Right;
                    texPtr[ptrOffset++] = r.Top;

                    // Left-bottom
                    // Positions
                    posPtr[ptrOffset++] = offsets[2].x + bb.Position.x;
                    posPtr[ptrOffset++] = offsets[2].y + bb.Position.y;
                    posPtr[ptrOffset++] = offsets[2].z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = r.Left;
                    texPtr[ptrOffset++] = r.Bottom;
                
                    // Right-bottom
                    // Positions
                    posPtr[ptrOffset++] = offsets[3].x + bb.Position.x;
                    posPtr[ptrOffset++] = offsets[3].y + bb.Position.y;
                    posPtr[ptrOffset++] = offsets[3].z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = r.Right;
                    texPtr[ptrOffset++] = r.Bottom;
                }
            }
            else if (rotationType == BillboardRotationType.Vertex)
            {
                // TODO: Cache axis when billboard type is BillboardType.Point or 
                //       BillboardType.PerpendicularCommon
                Vector3 axis = (offsets[3] - offsets[0]).Cross(offsets[2] - offsets[1]);
                axis.Normalize();

                Quaternion rotation = Quaternion.FromAngleAxis(bb.rotationInRadians, axis);
                Vector3 pt;

                unsafe {
                    float *posPtr = (float *)lockPtr.ToPointer();
                    uint *colPtr = (uint *)posPtr;
                    float *texPtr = (float *)posPtr;

                    // Left-top
                    // Positions
                    pt = rotation * offsets[0];
                    posPtr[ptrOffset++] = offsets[0].x + bb.Position.x;
                    posPtr[ptrOffset++] = offsets[0].y + bb.Position.y;
                    posPtr[ptrOffset++] = offsets[0].z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = r.Left;
                    texPtr[ptrOffset++] = r.Top;

                    // Right-top
                    // Positions
                    pt = rotation * offsets[1];
                    posPtr[ptrOffset++] = pt.x + bb.Position.x;
                    posPtr[ptrOffset++] = pt.y + bb.Position.y;
                    posPtr[ptrOffset++] = pt.z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = r.Right;
                    texPtr[ptrOffset++] = r.Top;

                    // Left-bottom
                    // Positions
                    pt = rotation * offsets[2];
                    posPtr[ptrOffset++] = pt.x + bb.Position.x;
                    posPtr[ptrOffset++] = pt.y + bb.Position.y;
                    posPtr[ptrOffset++] = pt.z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = r.Left;
                    texPtr[ptrOffset++] = r.Bottom;
                
                    // Right-bottom
                    // Positions
                    pt = rotation * offsets[3];
                    posPtr[ptrOffset++] = pt.x + bb.Position.x;
                    posPtr[ptrOffset++] = pt.y + bb.Position.y;
                    posPtr[ptrOffset++] = pt.z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = r.Right;
                    texPtr[ptrOffset++] = r.Bottom;
                }
            }
            else
            {
			    float cos_rot = MathUtil.Cos(bb.rotationInRadians);
			    float sin_rot = MathUtil.Sin(bb.rotationInRadians);

                float width = (r.Right-r.Left)/2;
                float height = (r.Bottom-r.Top)/2;
                float mid_u = r.Left+width;
                float mid_v = r.Top+height;

                float cos_rot_w = cos_rot * width;
                float cos_rot_h = cos_rot * height;
                float sin_rot_w = sin_rot * width;
                float sin_rot_h = sin_rot * height;

                unsafe {
                    float *posPtr = (float *)lockPtr.ToPointer();
                    uint *colPtr = (uint *)posPtr;
                    float *texPtr = (float *)posPtr;

                    // Left-top
                    // Positions
                    posPtr[ptrOffset++] = offsets[0].x + bb.Position.x;
                    posPtr[ptrOffset++] = offsets[0].y + bb.Position.y;
                    posPtr[ptrOffset++] = offsets[0].z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = mid_u - cos_rot_w + sin_rot_h;
                    texPtr[ptrOffset++] = mid_v - sin_rot_w - cos_rot_h;

                    // Right-top
                    // Positions
                    posPtr[ptrOffset++] = offsets[1].x + bb.Position.x;
                    posPtr[ptrOffset++] = offsets[1].y + bb.Position.y;
                    posPtr[ptrOffset++] = offsets[1].z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = mid_u + cos_rot_w + sin_rot_h;
                    texPtr[ptrOffset++] = mid_v + sin_rot_w - cos_rot_h;

                    // Left-bottom
                    // Positions
                    posPtr[ptrOffset++] = offsets[2].x + bb.Position.x;
                    posPtr[ptrOffset++] = offsets[2].y + bb.Position.y;
                    posPtr[ptrOffset++] = offsets[2].z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = mid_u - cos_rot_w - sin_rot_h;
                    texPtr[ptrOffset++] = mid_v - sin_rot_w + cos_rot_h;
                
                    // Right-bottom
                    // Positions
                    posPtr[ptrOffset++] = offsets[3].x + bb.Position.x;
                    posPtr[ptrOffset++] = offsets[3].y + bb.Position.y;
                    posPtr[ptrOffset++] = offsets[3].z + bb.Position.z;
                    // Color
                    colPtr[ptrOffset++] = color;
                    // Texture coords
                    texPtr[ptrOffset++] = mid_u + cos_rot_w - sin_rot_h;
                    texPtr[ptrOffset++] = mid_v + sin_rot_w + cos_rot_h;
                }
            }
        }

        /// <summary>
        ///		Generates vertex offsets.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="destVec"></param>
        /// <remarks>
        ///		Takes in parametric offsets as generated from GetParametericOffsets, width and height values
        ///		and billboard x and y axes as generated from GenerateBillboardAxes. 
        ///		Fills output array of 4 vectors with vector offsets
        ///		from origin for left-top, right-top, left-bottom, right-bottom corners.
        /// </remarks>
        protected void GenerateVertexOffsets(float left, float right, float top, float bottom, float width, float height, ref Vector3 x, ref Vector3 y, Vector3[] destVec) {
            Vector3 vLeftOff, vRightOff, vTopOff, vBottomOff, vDepthOff;
            /* Calculate default offsets. Scale the axes by
               parametric offset and dimensions, ready to be added to
               positions.
            */
            vLeftOff   = x * ( left * width );
            vRightOff  = x * ( right * width );
            vTopOff    = y * ( top * height );
            vBottomOff = y * ( bottom * height );
            vDepthOff = camDir * -depthOffset;

            // Make final offsets to vertex positions
            destVec[0] = vLeftOff  + vTopOff + vDepthOff;
            destVec[1] = vRightOff + vTopOff + vDepthOff;
            destVec[2] = vLeftOff + vBottomOff + vDepthOff;
            destVec[3] = vRightOff + vBottomOff + vDepthOff;
        }

        /// <summary>
        ///		Overloaded method.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Billboard CreateBillboard(Vector3 position) {
            return CreateBillboard(position, ColorEx.White);
        }

        /// <summary>
        ///		Creates a new billboard and adds it to this set.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public Billboard CreateBillboard(Vector3 position, ColorEx color) {
            // see if we need to auto extend the free billboard pool
            if(freeBillboards.Count == 0) {
                if(autoExtendPool)
                    this.PoolSize = this.PoolSize * 2;
                else
                    throw new AxiomException("Could not create a billboard with AutoSize disabled and an empty pool.");
            }

            // get the next free billboard from the queue
            Billboard newBillboard = freeBillboards[0];
            freeBillboards.RemoveAt(0);
            
            // add the billboard to the active list
            activeBillboards.Add(newBillboard);
            
            // initialize the billboard
            newBillboard.Position = position;
            newBillboard.Color = color;
            newBillboard.Direction = Vector3.Zero;
            newBillboard.Rotation = 0;
            // newBillboard.TexCoordIndex = 0;
            newBillboard.ResetDimensions();
            newBillboard.NotifyOwner(this);

            // Merge into bounds
            float adjust = Math.Max(defaultParticleWidth, defaultParticleHeight);
            Vector3 adjustVec = new Vector3(adjust, adjust, adjust);
            Vector3 newMin = position - adjustVec;
            Vector3 newMax = position + adjustVec;
            
            aab.Merge(new AxisAlignedBox(newMin, newMax));

            float sqlen = (float)Math.Max(newMin.LengthSquared, newMax.LengthSquared);
            boundingRadius = (float)Math.Max(boundingRadius, Math.Sqrt(sqlen));

            return newBillboard;
        }

        /// <summary>
        ///     Allocate / reallocate vertex data
        ///     Note that we allocate enough space for ALL the billboards in the pool, but only issue
        ///     rendering operations for the sections relating to the active billboards
        /// </summary>
        private void CreateBuffers() {
            /* Alloc positions   ( 1 or 4 verts per billboard, 3 components )
                     colours     ( 1 x RGBA per vertex )
                     indices     ( 6 per billboard ( 2 tris ) if not point rendering )
                     tex. coords ( 2D coords, 1 or 4 per billboard )
            */

//             LogManager.Instance.Write(string.Format("BillBoardSet.CreateBuffers entered; vertexData {0}, indexData {1}, mainBuffer {2}",
//                     vertexData == null ? "null" : vertexData.ToString(), 
//                     indexData == null ? "null" : indexData.ToString(),
//                     mainBuffer == null ? "null" : mainBuffer.ToString()));
            
            // Warn if user requested an invalid setup
		    // Do it here so it only appears once
		    if (pointRendering && billboardType != BillboardType.Point)
		    {
                LogManager.Instance.Write(
                    "Warning: BillboardSet {0} has point rendering enabled but is using a type " +
				    "other than BillboardType.Point, this may not give you the results you " +
				    "expect.", name);
		    }

            vertexData = new VertexData();
            if (pointRendering)
                vertexData.vertexCount = poolSize;
            else
                vertexData.vertexCount = poolSize * 4;

            vertexData.vertexStart = 0;

            // Vertex declaration
            VertexDeclaration decl = vertexData.vertexDeclaration;
            VertexBufferBinding binding = vertexData.vertexBufferBinding;

            int offset = 0;
            decl.AddElement(0, offset, VertexElementType.Float3, VertexElementSemantic.Position);
            offset += VertexElement.GetTypeSize(VertexElementType.Float3);
            decl.AddElement(0, offset, VertexElementType.Color, VertexElementSemantic.Diffuse);
            offset += VertexElement.GetTypeSize(VertexElementType.Color);
            // Texture coords irrelevant when enabled point rendering (generated
            // in point sprite mode, and unused in standard point mode)
            if (!pointRendering)
            {
                decl.AddElement(0, offset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 0);
            }

            mainBuffer = 
                HardwareBufferManager.Instance.CreateVertexBuffer(
                    decl.GetVertexSize(0),
                    vertexData.vertexCount,
                    BufferUsage.DynamicWriteOnlyDiscardable);

            // bind position and diffuses
            binding.SetBinding(0, mainBuffer);

		    if (!pointRendering)
		    {
                indexData = new IndexData();

                // calc index buffer size
                indexData.indexStart = 0;
                indexData.indexCount = poolSize * 6;

                // create the index buffer
                indexData.indexBuffer = 
                        HardwareBufferManager.Instance.CreateIndexBuffer(
                        IndexType.Size16,
                        indexData.indexCount,
                        BufferUsage.StaticWriteOnly);

			    /* Create indexes (will be the same every frame)
			       Using indexes because it means 1/3 less vertex transforms (4 instead of 6)

			       Billboard layout relative to camera:

				    0-----1
				    |    /|
				    |  /  |
				    |/    |
				    2-----3
			    */

                 // lock the index buffer
                IntPtr idxPtr = indexData.indexBuffer.Lock(BufferLocking.Discard);

                unsafe {
                    ushort* pIdx = (ushort*)idxPtr.ToPointer();

                    for (int idx, idxOffset, bboard = 0; bboard < poolSize; ++bboard) {
			            // Do indexes
                        idx = bboard * 6;
                        idxOffset = bboard * 4;

                        pIdx[idx]   =	(ushort)idxOffset; // + 0;, for clarity
                        pIdx[idx + 1] = (ushort)(idxOffset + 2);
                        pIdx[idx + 2] = (ushort)(idxOffset + 1);
                        pIdx[idx + 3] = (ushort)(idxOffset + 1);
                        pIdx[idx + 4] = (ushort)(idxOffset + 2);
                        pIdx[idx + 5] = (ushort)(idxOffset + 3);
                    } // for
                } // unsafe

                // unlock the buffers
                indexData.indexBuffer.Unlock();
    	    }
            buffersCreated = true;
        }

        private void DestroyBuffers() {
//             LogManager.Instance.Write(string.Format("BillBoardSet.DestroyBuffers entered; vertexData {0}, indexData {1}, mainBuffer {2}",
//                     vertexData == null ? "null" : vertexData.ToString(), 
//                     indexData == null ? "null" : indexData.ToString(),
//                     mainBuffer == null ? "null" : mainBuffer.ToString()));
            vertexData = null;
            indexData = null;
            mainBuffer = null;
            buffersCreated = false;
        }

        // Warn if user requested an invalid setup
        // Do it here so it only appears once

        /// <summary>
        ///		Empties all of the active billboards from this set.
        /// </summary>
        public void Clear() {
            // Move actives to the free list
            freeBillboards.AddRange(activeBillboards);
            activeBillboards.Clear();
        }

        protected Billboard GetBillboard(int index) {
            return activeBillboards[index];
        }

        protected void RemoveBillboard(int index) {
            Billboard tmp = activeBillboards[index];
            activeBillboards.RemoveAt(index);
            freeBillboards.Add(tmp);
        }

        protected void RemoveBillboard(Billboard bill) {
            int index = activeBillboards.IndexOf(bill);
            Debug.Assert(index >= 0, "Billboard is not in the active list");
            RemoveBillboard(index);
        }

        /// <summary>
        ///		Update the bounds of the BillboardSet.
        /// </summary>
        public virtual void UpdateBounds() {
            if (activeBillboards.Count == 0) {
                // no billboards, so the bounding box is null
                aab.IsNull = true;
                boundingRadius = 0.0f;
            } else {
                float maxSqLen = -1.0f;
                Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

                foreach (Billboard billboard in activeBillboards) {
                    Vector3 pos = billboard.Position;
                    min.Floor(pos);
                    max.Ceil(pos);

                    maxSqLen = MathUtil.Max(maxSqLen, pos.LengthSquared);
                }

                // adjust for billboard size
                float adjust = MathUtil.Max(defaultParticleWidth, defaultParticleHeight);
                Vector3 vecAdjust = new Vector3(adjust, adjust, adjust);
                min -= vecAdjust;
                max += vecAdjust;

                // update our local aabb
                aab.SetExtents(min, max);

                boundingRadius = MathUtil.Sqrt(maxSqLen);

            }
            // if we have a parent node, ask it to update us
            if (parentNode != null) {
                parentNode.NeedUpdate();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///		Tells the set whether to allow automatic extension of the pool of billboards.
        ///	 </summary>
        ///	 <remarks>
        ///		A BillboardSet stores a pool of pre-constructed billboards which are used as needed when
        ///		a new billboard is requested. This allows applications to create / remove billboards efficiently
        ///		without incurring construction / destruction costs (a must for sets with lots of billboards like
        ///		particle effects). This method allows you to configure the behaviour when a new billboard is requested
        ///		but the billboard pool has been exhausted.
        ///		<p/>
        ///		The default behaviour is to allow the pool to extend (typically this allocates double the current
        ///		pool of billboards when the pool is expended), equivalent to calling this property to
        ///		true. If you set the property to false however, any attempt to create a new billboard
        ///		when the pool has expired will simply fail silently, returning a null pointer.
        /// </remarks>
        public bool AutoExtend {
            get { 
                return autoExtendPool; 
            }
            set { 
                autoExtendPool = value; 
            }
        }

        /// <summary>
        ///		Adjusts the size of the pool of billboards available in this set.
        ///	 </summary>
        ///	 <remarks>
        ///		See the BillboardSet.AutoExtend property for full details of the billboard pool. This method adjusts
        ///		the preallocated size of the pool. If you try to reduce the size of the pool, the set has the option
        ///		of ignoring you if too many billboards are already in use. Bear in mind that calling this method will
        ///		incur significant construction / destruction calls so should be avoided in time-critical code. The same
        ///		goes for auto-extension, try to avoid it by estimating the pool size correctly up-front.
        /// </remarks>
        public int PoolSize {
            get { 
                return billboardPool.Count; 
            }
            set {
                // If we're driving this from our own data, allocate billboards
                if (!externalData) {
                    int size = value;
                    // Never shrink below Count
                    int currentSize = billboardPool.Count;
                    if (currentSize >= size)
                        return;

                    IncreasePool(size);

                    // add new items to the queue
                    for (int i = currentSize; i < size; ++i)
                        freeBillboards.Add(billboardPool[i]);
                }
                poolSize = value;
                DestroyBuffers();
            }
        }
#if OLD
                    // 4 vertices per billboard, 3 components = 12
                    // 1 int value per vertex
                    // 2 tris, 6 per billboard
                    // 2d coords, 4 per billboard = 8

                    vertexData = new VertexData();
                    indexData = new IndexData();

                    vertexData.vertexCount = size * 4;
                    vertexData.vertexStart = 0;

                    // get references to the declaration and buffer binding
                    VertexDeclaration decl = vertexData.vertexDeclaration;
                    VertexBufferBinding binding = vertexData.vertexBufferBinding;

                    // create the 3 vertex elements we need
                    int offset = 0;
                    decl.AddElement(POSITION, offset, VertexElementType.Float3, VertexElementSemantic.Position);
                    decl.AddElement(COLOR, offset, VertexElementType.Color, VertexElementSemantic.Diffuse);
                    decl.AddElement(TEXCOORD, 0, VertexElementType.Float2, VertexElementSemantic.TexCoords, 0);

                    // create position buffer
                    HardwareVertexBuffer vBuffer = 
                        HardwareBufferManager.Instance.CreateVertexBuffer(
                        decl.GetVertexSize(POSITION),
                        vertexData.vertexCount,
                        BufferUsage.StaticWriteOnly);

                    binding.SetBinding(POSITION, vBuffer);

                    // create color buffer
                    vBuffer = 
                        HardwareBufferManager.Instance.CreateVertexBuffer(
                        decl.GetVertexSize(COLOR),
                        vertexData.vertexCount,
                        BufferUsage.StaticWriteOnly);

                    binding.SetBinding(COLOR, vBuffer);

                    // create texcoord buffer
                    vBuffer = 
                        HardwareBufferManager.Instance.CreateVertexBuffer(
                        decl.GetVertexSize(TEXCOORD),
                        vertexData.vertexCount,
                        BufferUsage.StaticWriteOnly);

                    binding.SetBinding(TEXCOORD, vBuffer);

                    // calc index buffer size
                    indexData.indexStart = 0;
                    indexData.indexCount = size * 6;

                    // create the index buffer
                    indexData.indexBuffer = 
                        HardwareBufferManager.Instance.CreateIndexBuffer(
                        IndexType.Size16,
                        indexData.indexCount,
                        BufferUsage.StaticWriteOnly);

                    /* Create indexes and tex coords (will be the same every frame)
                       Using indexes because it means 1/3 less vertex transforms (4 instead of 6)

                       Billboard layout relative to camera:

                        2-----3
                        |    /|
                        |  /  |
                        |/    |
                        0-----1
                    */

                    float[] texData = new float[] {
                         0.0f, 1.0f,
                         1.0f, 1.0f,
                         0.0f, 0.0f,
                         1.0f, 0.0f };

                    // lock the index buffer
                    IntPtr idxPtr = indexData.indexBuffer.Lock(BufferLocking.Discard);

                    // get the texcoord buffer
                    vBuffer = vertexData.vertexBufferBinding.GetBuffer(TEXCOORD);

                    // lock the texcoord buffer
                    IntPtr texPtr = vBuffer.Lock(BufferLocking.Discard);

                    unsafe {
                        ushort* pIdx = (ushort*)idxPtr.ToPointer();
                        float* pTex = (float*)texPtr.ToPointer();

                        for(int idx, idxOffset, texOffset, bboard = 0; bboard < size; bboard++) {
                            // compute indexes
                            idx = bboard * 6;
                            idxOffset = bboard * 4;
                            texOffset = bboard * 8;

                            pIdx[idx]   =	(ushort)idxOffset; // + 0;, for clarity
                            pIdx[idx + 1] = (ushort)(idxOffset + 1);
                            pIdx[idx + 2] = (ushort)(idxOffset + 3);
                            pIdx[idx + 3] = (ushort)(idxOffset + 0);
                            pIdx[idx + 4] = (ushort)(idxOffset + 3);
                            pIdx[idx + 5] = (ushort)(idxOffset + 2);

                            // Do tex coords
                            pTex[texOffset]   = texData[0];
                            pTex[texOffset+1] = texData[1];
                            pTex[texOffset+2] = texData[2];
                            pTex[texOffset+3] = texData[3];
                            pTex[texOffset+4] = texData[4];
                            pTex[texOffset+5] = texData[5];
                            pTex[texOffset+6] = texData[6];
                            pTex[texOffset+7] = texData[7];
                        } // for
                    } // unsafe

                    // unlock the buffers
                    indexData.indexBuffer.Unlock();
                    vBuffer.Unlock();
                } // if
            } // set
        }
#endif

        /// <summary>
        ///		Gets/Sets the point which acts as the origin point for all billboards in this set.
        ///	 </summary>
        ///	 <remarks>
        ///		This setting controls the fine tuning of where a billboard appears in relation to it's
        ///		position. It could be that a billboard's position represents it's center (e.g. for fireballs),
        ///		it could mean the center of the bottom edge (e.g. a tree which is positioned on the ground),
        /// </remarks>
        public BillboardOrigin BillboardOrigin {
            get { 
                return originType; 
            }
            set  { 
                originType = value; 
            }
        }

        /// <summary>
        ///		Gets/Sets the name of the material to use for this billboard set.
        /// </summary>
        public string MaterialName {
            get { 
                return materialName; 
            }
            set {
                materialName = value;
				
                // find the requested material
                material = MaterialManager.Instance.GetByName(materialName);

                if (material != null) {
                    // make sure it is loaded
                    material.Load();
                }
                else {
                    throw new AxiomException("Material '{0}' could not be found to be set as the material for BillboardSet '{0}'.", materialName, this.name);
                }
            }
        }

        /// <summary>
        ///		Sets whether culling tests billboards in this individually as well as in a group.
        ///	 </summary>
        ///	 <remarks>
        ///		Billboard sets are always culled as a whole group, based on a bounding box which 
        ///		encloses all billboards in the set. For fairly localised sets, this is enough. However, you
        ///		can optionally tell the set to also cull individual billboards in the set, i.e. to test
        ///		each individual billboard before rendering. The default is not to do this.
        ///		<p/>
        ///		This is useful when you have a large, fairly distributed set of billboards, like maybe 
        ///		trees on a landscape. You probably still want to group them into more than one
        ///		set (maybe one set per section of landscape), which will be culled coarsely, but you also
        ///		want to cull the billboards individually because they are spread out. Whilst you could have
        ///		lots of single-tree sets which are culled separately, this would be inefficient to render
        ///		because each tree would be issued as it's own rendering operation.
        ///		<p/>
        ///		By setting this property to true, you can have large billboard sets which 
        ///		are spaced out and so get the benefit of batch rendering and coarse culling, but also have
        ///		fine-grained culling so unnecessary rendering is avoided.
        /// </remarks>
        public bool CullIndividual {
            get { 
                return cullIndividual; 
            }
            set { 
                this.cullIndividual = value;
            }
        }

        /// <summary>
        ///		Gets/Sets the type of billboard to render.
        ///	 </summary>
        ///	 <remarks>
        ///		The default sort of billboard (Point), always has both x and y axes parallel to 
        ///		the camera's local axes. This is fine for 'point' style billboards (e.g. flares,
        ///		smoke, anything which is symmetrical about a central point) but does not look good for
        ///		billboards which have an orientation (e.g. an elongated raindrop). In this case, the
        ///		oriented billboards are more suitable (OrientedCommon or OrientedSelf) since they retain an independant Y axis
        ///		and only the X axis is generated, perpendicular to both the local Y and the camera Z.
        /// </remarks>
        public BillboardType BillboardType {
            get { 
                return billboardType; 
            }
            set { 
                billboardType = value; 
            }
        }

        public BillboardRotationType BillboardRotationType {
            get {
                return rotationType;
            }
            set {
                rotationType = value;
            }
        }

        /// <summary>
        ///		Use this to specify the common direction given to billboards of types OrientedCommon or PerpendicularCommon.
        ///	 </summary>
        ///	 <remarks>
        ///		Use OrientedCommon when you want oriented billboards but you know they are always going to 
        ///		be oriented the same way (e.g. rain in calm weather). It is faster for the system to calculate
        ///		the billboard vertices if they have a common direction.
        /// </remarks>
        public Vector3 CommonDirection {
            get { 
                return commonDirection; 
            }
            set { 
                commonDirection = value; 
            }
        }

        /// <summary>
        ///		Use this to determine the orientation given to billboards of types PerpendicularCommon or PerpendicularSelf.
        ///	 </summary>
        ///	 <remarks>
        ///		Billboards will be oriented with their Y axis coplanar with the up direction vector.
        /// </remarks>
        public Vector3 CommonUpVector
        {
            get
            {
                return commonUpVector;
            }
            set
            {
                commonUpVector = value;
            }
        }

        public bool UseAccurateFacing {
            get {
                return accurateFacing;
            }
            set {
                accurateFacing = value;
            }
        }

        /// <summary>
        ///		Gets the list of active billboards.
        /// </summary>
        public List<Billboard> Billboards {
            get { 
                return activeBillboards; 
            }
        }

        /// <summary>
        ///    Local bounding radius of this billboard set.
        /// </summary>
        public override float BoundingRadius {
            get {
                return boundingRadius;
            }
        }

        /// <summary>
        ///     Distance billboards are offset towards the camera.
        /// </summary>
        public float DepthOffset {
            get {
                return depthOffset;
            }
            set {
                depthOffset = value;
            }
        }

        #endregion

        #region IRenderable Members

		public bool CastsShadows {
			get {
				return false;
			}
		}

        public Material Material {
            get { 
                return material; 
            }
        }

        public Technique Technique {
            get {
                return material.GetBestTechnique();
            }
        }

        public void GetRenderOperation(RenderOperation op) {
            op.vertexData = vertexData;
            op.vertexData.vertexStart = 0;

            if (pointRendering) {
                op.operationType = OperationType.PointList;
                op.useIndices = false;
                op.indexData = null;
                op.vertexData.vertexCount = numVisibleBillboards;
            } else {
                op.operationType = OperationType.TriangleList;
                op.useIndices = true;
                op.vertexData.vertexCount = numVisibleBillboards * 4;
                op.indexData = indexData;
                op.indexData.indexCount = numVisibleBillboards * 6;
                op.indexData.indexStart = 0;
            }
        }		

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrices"></param>
        public virtual void GetWorldTransforms(Matrix4[] matrices) {
            // It's actually more natural to be in local space, which means 
            // that the emitted particles move when the parent object moves.
            // Sometimes you only want the emitter to move though, such as 
            // when you are generating smoke
            if (worldSpace)
                matrices[0] = Matrix4.Identity;
            else
				matrices[0] = parentNode.FullTransform;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ushort NumWorldTransforms {
            get { 
                return 1;	
            }
        }

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
                return SceneDetailLevel.Solid; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public virtual float GetSquaredViewDepth(Camera camera) {
            Debug.Assert(parentNode != null, "BillboardSet must have a parent scene node to get the squared view depth.");

            return parentNode.GetSquaredViewDepth(camera);
        }

        /// <summary>
        /// 
        /// </summary>
        public Quaternion WorldOrientation {
            get {
                return parentNode.DerivedOrientation;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 WorldPosition {
            get {
                return parentNode.DerivedPosition;
            }
        }

        public List<Light> Lights {
            get {
                return parentNode.Lights;
            }
        }

		public Vector4 GetCustomParameter(int index) {
			if(customParams[index] == null) {
				throw new Exception("A parameter was not found at the given index");
			}
			else {
				return (Vector4)customParams[index];
			}
		}

		public void SetCustomParameter(int index, Vector4 val) {
			customParams[index] = val;
		}

		public void UpdateCustomGpuParameter(GpuProgramParameters.AutoConstantEntry entry, GpuProgramParameters gpuParams) {
			if(customParams[entry.data] != null) {
				gpuParams.SetConstant(entry.index, (Vector4)customParams[entry.data]);
			}
		}

        #endregion

        #region Implementation of SceneObject
	
        public override AxisAlignedBox BoundingBox {
            // cloning to prevent direct modification
            get { 
                return (AxisAlignedBox)aab.Clone(); 
            }
        }

        public bool NormalizeNormals {
            get {
                return false;
            }
        }
	
		private static TimingMeter billboardNotifyMeter = MeterManager.GetMeter("Notify Camera", "BillboardSet");
		private static TimingMeter notSelfMeter = MeterManager.GetMeter("Not Self", "BillboardSet");
		private static TimingMeter genVerticesMeter = MeterManager.GetMeter("Gen Vertices", "BillboardSet");
		private static TimingMeter bufferGettingMeter = MeterManager.GetMeter("Get Buffers", "BillboardSet");
		private static TimingMeter posBufferLockingMeter = MeterManager.GetMeter("Lock Pos Buffer", "BillboardSet");
		private static TimingMeter colBufferLockingMeter = MeterManager.GetMeter("Lock Col Buffer", "BillboardSet");
		private static TimingMeter texLockingMeter = MeterManager.GetMeter("Lock Tex", "BillboardSet");
		
		/// <summary>
        ///		Generate the vertices for all the billboards relative to the camera
        /// </summary>
        /// <param name="camera"></param>
        public override void NotifyCurrentCamera(Camera camera) {
			billboardNotifyMeter.Enter();
            // base.NotifyCurrentCamera(camera);
            currentCamera = camera;
            camQ = camera.DerivedOrientation;
            camPos = camera.DerivedPosition;
            if (!worldSpace) {
                // Default behaviour is that billboards are in local node space
                // so orientation of camera (in world space) must be reverse-transformed
                // into node space
                camQ = parentNode.DerivedOrientation.UnitInverse() * camQ;
                camPos = parentNode.DerivedOrientation.UnitInverse() *
                    (camPos - parentNode.DerivedPosition) / parentNode.DerivedScale;
            }
            // Camera direction points down -Z
            camDir = camQ * Vector3.NegativeUnitZ;
#if NOT
            // Take the reverse transform of the camera world axes into billboard space for efficiency

            // parametrics offsets of the origin
            float leftOffset, rightOffset, topOffset, bottomOffset;

            // get offsets for the origin type
            GetParametricOffsets(out leftOffset, out rightOffset, out topOffset, out bottomOffset);

            // Boundary offsets based on origin and camera orientation
            // Final vertex offsets, used where sizes all default to save calcs
            Vector3[] vecOffsets = new Vector3[4];
            Vector3 camX = new Vector3();
            Vector3 camY = new Vector3();

            // generates axes up front if not orient per-billboard
            if((billboardType != BillboardType.OrientedSelf) && (billboardType != BillboardType.PerpendicularSelf)) {
				notSelfMeter.Enter();
                GenerateBillboardAxes(ref camX, ref camY);

                //	if all billboards are the same size we can precalculare the
                // offsets and just use + instead of * for each billboard, which should be faster.
                GenerateVertexOffsets(leftOffset, rightOffset, topOffset, bottomOffset, 
                    defaultParticleWidth, defaultParticleHeight, ref camX, ref camY, vecOffsets);
				notSelfMeter.Exit();
			}

            // reset counter
            numVisibleBillboards = 0;			

            // get a reference to the vertex buffers to update
            bufferGettingMeter.Enter();
			HardwareVertexBuffer posBuffer = vertexData.vertexBufferBinding.GetBuffer(POSITION);
            HardwareVertexBuffer colBuffer = vertexData.vertexBufferBinding.GetBuffer(COLOR);
			HardwareVertexBuffer texBuffer = vertexData.vertexBufferBinding.GetBuffer(TEXCOORD);
            bufferGettingMeter.Exit();

            // lock the buffers
            posBufferLockingMeter.Enter();
			IntPtr posPtr = posBuffer.Lock(BufferLocking.Discard);
            posBufferLockingMeter.Exit();
            colBufferLockingMeter.Enter();
			IntPtr colPtr = colBuffer.Lock(BufferLocking.Discard);
            colBufferLockingMeter.Exit();

			IntPtr texPtr = IntPtr.Zero;

			// do we need to update the tex coords?
			if(!fixedTextureCoords) {
				texLockingMeter.Enter();
				texPtr = texBuffer.Lock(BufferLocking.Discard);
				texLockingMeter.Exit();
			}

            // reset the global index counters
            posIndex = 0;
            colorIndex = 0;
			texIndex = 0;

            // if they are all the same size...
            if(allDefaultSize) {
				genVerticesMeter.Enter();
				for(int i = 0; i < activeBillboards.Count; i++) {
                    Billboard b = (Billboard)activeBillboards[i];
                    // skip if not visible dammit
                    
					if(!IsBillboardVisible(camera, b))
                        continue;

                    if((billboardType == BillboardType.OrientedSelf) || (billboardType == BillboardType.PerpendicularSelf)) {
                        // generate per billboard
						GenerateBillboardAxes(ref camX, ref camY, b);
                        GenerateVertexOffsets(leftOffset, rightOffset, topOffset, bottomOffset,
                                              defaultParticleWidth, defaultParticleHeight, 
                                              ref camX, ref camY, vecOffsets);
					}

                    // generate the billboard vertices
					GenerateVertices(posPtr, colPtr, texPtr, vecOffsets, b);

                    numVisibleBillboards++;
                }
				genVerticesMeter.Exit();
            }
            else {
                // billboards aren't all default size
				genVerticesMeter.Enter();
                for(int i = 0; i < activeBillboards.Count; i++) {
                    Billboard b = (Billboard)activeBillboards[i];
                    // skip if not visible dammit
                    if(!IsBillboardVisible(camera, b))
                        continue;

                    if((billboardType == BillboardType.OrientedSelf) || (billboardType == BillboardType.PerpendicularSelf)) {
                        // generate per billboard
                        GenerateBillboardAxes(ref camX, ref camY, b);
                    }

                    // if it has it's own dimensions. or self oriented, gen offsets
                    if (b.HasOwnDimensions || billboardType == BillboardType.OrientedSelf || billboardType == BillboardType.PerpendicularSelf)
                    {
                        // generate using it's own dimensions
                        GenerateVertexOffsets(leftOffset, rightOffset, topOffset, bottomOffset, b.Width,
                            b.Height, ref camX, ref camY, vecOffsets);
                    }

                    // generate the billboard vertices
                    GenerateVertices(posPtr, colPtr, texPtr, vecOffsets, b);

                    numVisibleBillboards++;
                }
				genVerticesMeter.Exit();
            }

            // unlock the buffers
            posBuffer.Unlock();
            colBuffer.Unlock();

			// unlock this one only if it was updated
			if(!fixedTextureCoords) {
				texBuffer.Unlock();
			}
#endif
			billboardNotifyMeter.Exit();
        }
	
        /// <summary>
        ///		Sets the default dimensions of the billboards in this set.
        ///	 </summary>
        ///	 <remarks>
        ///		All billboards in a set are created with these default dimensions. The set will render most efficiently if
        ///		all the billboards in the set are the default size. It is possible to alter the size of individual
        ///		billboards at the expense of extra calculation. See the Billboard class for more info.
        /// </remarks>
        public void SetDefaultDimensions(float width, float height) {
            defaultParticleWidth = width;
            defaultParticleHeight = height;
        }

        public void SetBillboardsInWorldSpace(bool worldSpace) {
            this.worldSpace = worldSpace;
        }

        public override void UpdateRenderQueue(RenderQueue queue) {
            if (!externalData) {
                // TODO: Implement sorting of billboards
                //if (sortingEnabled)
                //    SortBillboards(currentCamera);

                BeginBillboards();
                foreach (Billboard billboard in activeBillboards) {
                    InjectBillboard(billboard);
                }
                EndBillboards();
            }
            // TODO: Ogre checks mRenderQueueIDSet
            // add ourself to the render queue
            queue.AddRenderable(this, RenderQueue.DEFAULT_PRIORITY, renderQueueID);
        }

        public bool PointRenderingEnabled {
            get {
                return pointRendering;
            }
            set {
                bool enabled = value;
                // Override point rendering if not supported
                if (enabled && !Root.Instance.RenderSystem.Caps.CheckCap(Capabilities.PointSprites)) {
        			enabled = false;
                }
		        if (enabled != pointRendering) {
                    pointRendering = true;
          			// Different buffer structure (1 or 4 verts per billboard)
                    DestroyBuffers();
                }
            }
        }

        #endregion

    }
}
