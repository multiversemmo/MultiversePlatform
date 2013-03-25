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
using System.Runtime.InteropServices;
using Axiom.Core;
using Axiom.Configuration;

namespace Axiom.Graphics {

    /// <summary>
	///     Struct used to hold hardware morph / pose vertex data information
    /// </summary>
	public class HardwareAnimationData {
		protected VertexElement targetVertexElement;
		protected float parametric;

		public VertexElement TargetVertexElement {
			get {
				return targetVertexElement;
			}
			set {
				targetVertexElement = value;
			}
		}
		
		public float Parametric {
			get {
				return parametric;
			}
			set {
				parametric = value;
			}
		}
	}

    /// <summary>
    /// 	Summary class collecting together vertex source information.
    /// </summary>
    public class VertexData {
        #region Fields
		
		/// <summary>
		///		Declaration of the vertex to be used in this operation.
		/// </summary>
        public VertexDeclaration vertexDeclaration;
		/// <summary>
		///		The vertex buffer bindings to be used.
		/// </summary>
        public VertexBufferBinding vertexBufferBinding;
		/// <summary>
		///		The base vertex index to start from, if using unindexed geometry.
		/// </summary>
        public int vertexStart;
		/// <summary>
		///		The number of vertices used in this operation.
		/// </summary>
        public int vertexCount;
		/// <summary>
		///     VertexElements used for hardware morph / pose animation
		/// </summary>	
		public List<HardwareAnimationData> HWAnimationDataList;
		/// <summary>
		///     Number of hardware animation data items used
		/// </summary>	
		public int HWAnimDataItemsUsed = 0;
		
		/// <summary>
		///		Additional shadow volume vertex buffer storage.
		/// </summary>
		/// <remarks>
		///		This additional buffer is only used where we have prepared this VertexData for
		///		use in shadow volume contruction, and where the current render system supports
		///		vertex programs. This buffer contains the 'w' vertex position component which will
		///		be used by that program to differentiate between extruded and non-extruded vertices.
		///		This 'w' component cannot be included in the original position buffer because
		///		DirectX does not allow 4-component positions in the fixed-function pipeline, and the original
		///		position buffer must still be usable for fixed-function rendering.
		///		<p/>
		///		Note that we don't store any vertex declaration or vertex buffer binding here becuase this
		///		can be reused in the shadow algorithm.
		/// </remarks>
		public HardwareVertexBuffer hardwareShadowVolWBuffer;

        #endregion Fields

		#region Constructor

		/// <summary>
		///		Default constructor.  Calls on the current buffer manager to initialize the bindings and declarations.
		/// </summary>
        public VertexData() {
            vertexDeclaration = HardwareBufferManager.Instance.CreateVertexDeclaration();
            vertexBufferBinding = HardwareBufferManager.Instance.CreateVertexBufferBinding();
        }

		#endregion Constructor

        #region Methods

		/// <summary>
		///		Clones this vertex data, potentially including replicating any vertex buffers.
		/// </summary>
		/// <returns>A cloned vertex data object.</returns>
		public VertexData Clone() {
			return Clone(false);
		}

		/// <summary>
		///		Clones this vertex data, potentially including replicating any vertex buffers.
		/// </summary>
		/// <param name="copyData">
		///		If true, makes a copy the vertex buffer in addition to the definition.
		///		If false, the clone will refer to the same vertex buffer this object refers to.
		/// </param>
		/// <returns>A cloned vertex data object.</returns>
		public VertexData Clone(bool copyData) {
			VertexData dest = new VertexData();

			// Copy vertex buffers in turn
			Dictionary<ushort, HardwareVertexBuffer> bindings = vertexBufferBinding.Bindings;

			foreach (ushort source in bindings.Keys) {
                
				HardwareVertexBuffer srcbuf = bindings[source];
				HardwareVertexBuffer dstBuf;

				if (copyData) {
					// create new buffer with the same settings
					dstBuf = 
						HardwareBufferManager.Instance.CreateVertexBuffer(
							srcbuf.VertexSize, srcbuf.VertexCount, srcbuf.Usage,
							srcbuf.HasShadowBuffer);

					// copy data
					dstBuf.CopyData(srcbuf, 0, 0, srcbuf.Size, true);
				}
				else {
					// don't copy, point at existing buffer
					dstBuf = srcbuf;
				}

				// Copy binding
				dest.vertexBufferBinding.SetBinding(source, dstBuf);
			}

			// Basic vertex info
			dest.vertexStart = this.vertexStart;
			dest.vertexCount = this.vertexCount;

			// Copy elements
			for (int i = 0; i < vertexDeclaration.ElementCount; i++) {
				VertexElement element = vertexDeclaration.GetElement(i);

				dest.vertexDeclaration.AddElement(
					element.Source,
					element.Offset,
					element.Type,
					element.Semantic,
					element.Index);
			}

			// Copy hardware shadow buffer if set up
			if (hardwareShadowVolWBuffer != null)
			{
				dest.hardwareShadowVolWBuffer = 
					HardwareBufferManager.Instance.CreateVertexBuffer(
					hardwareShadowVolWBuffer.VertexSize,
					hardwareShadowVolWBuffer.VertexCount, 
					hardwareShadowVolWBuffer.Usage,
					hardwareShadowVolWBuffer.HasShadowBuffer);

				// copy data
				dest.hardwareShadowVolWBuffer.CopyData(
					hardwareShadowVolWBuffer, 0, 0, 
					hardwareShadowVolWBuffer.Size,
					true);
			}

			// copy anim data
			dest.HWAnimationDataList = HWAnimationDataList;
			dest.HWAnimDataItemsUsed = HWAnimDataItemsUsed;

			return dest;
		}

		/// <summary>
		///		Modifies the vertex data to be suitable for use for rendering shadow geometry.
		/// </summary>
		/// <remarks>
		///		<para>
		///			Preparing vertex data to generate a shadow volume involves firstly ensuring that the 
		///			vertex buffer containing the positions is a standalone vertex buffer,
		///			with no other components in it. This method will therefore break apart any existing
		///			vertex buffers if position is sharing a vertex buffer. 
		///			Secondly, it will double the size of this vertex buffer so that there are 2 copies of 
		///			the position data for the mesh. The first half is used for the original, and the second 
		///			half is used for the 'extruded' version. The vertex count used to render will remain 
		///			the same though, so as not to add any overhead to regular rendering of the object.
		///			Both copies of the position are required in one buffer because shadow volumes stretch 
		///			from the original mesh to the extruded version. 
		///		</para>
		///		<para>
		///			It's important to appreciate that this method can fundamentally change the structure of your
		///			vertex buffers, although in reality they will be new buffers. As it happens, if other 
		///			objects are using the original buffers then they will be unaffected because the reference
		///			counting will keep them intact. However, if you have made any assumptions about the 
		///			structure of the vertex data in the buffers of this object, you may have to rethink them.
		///		</para>
		/// </remarks>
		// TODO: Step through and test
		public void PrepareForShadowVolume() {
			/* NOTE
			Sinbad would dearly, dearly love to just use a 4D position buffer in order to 
			store the extra 'w' value I need to differentiate between extruded and 
			non-extruded sections of the buffer, so that vertex programs could use that.
			Hey, it works fine for GL. However, D3D9 in it's infinite stupidity, does not
			support 4d position vertices in the fixed-function pipeline. If you use them, 
			you just see nothing. Since we can't know whether the application is going to use
			fixed function or vertex programs, we have to stick to 3d position vertices and
			store the 'w' in a separate 1D texture coordinate buffer, which is only used
			when rendering the shadow.
			*/

			// Upfront, lets check whether we have vertex program capability
			RenderSystem renderSystem = Root.Instance.RenderSystem;
			bool useVertexPrograms = false;

			if (renderSystem != null && renderSystem.Caps.CheckCap(Capabilities.VertexPrograms)) {
				useVertexPrograms = true;
			}

			// Look for a position element
			VertexElement posElem = 
				vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);

			if(posElem != null) {
				ushort posOldSource = posElem.Source;

				HardwareVertexBuffer vbuf = vertexBufferBinding.GetBuffer(posOldSource);

                bool wasSharedBuffer = false;

                // Are there other elements in the buffer except for the position?
				if (vbuf.VertexSize > posElem.Size) {
					// We need to create another buffer to contain the remaining elements
					// Most drivers don't like gaps in the declaration, and in any case it's waste
					wasSharedBuffer = true;
				}

				HardwareVertexBuffer newPosBuffer = null, newRemainderBuffer = null;

				if (wasSharedBuffer) {
					newRemainderBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
						vbuf.VertexSize - posElem.Size, vbuf.VertexCount, vbuf.Usage,
						vbuf.HasShadowBuffer);
				}

				// Allocate new position buffer, will be FLOAT3 and 2x the size
				int oldVertexCount = vbuf.VertexCount;
				int newVertexCount = oldVertexCount * 2;

				newPosBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
					VertexElement.GetTypeSize(VertexElementType.Float3), 
					newVertexCount, vbuf.Usage, vbuf.HasShadowBuffer);

				// Iterate over the old buffer, copying the appropriate elements and initializing the rest
				IntPtr baseSrcPtr = vbuf.Lock(BufferLocking.ReadOnly);

				// Point first destination pointer at the start of the new position buffer,
				// the other one half way along
				IntPtr destPtr = newPosBuffer.Lock(BufferLocking.Discard);
                // oldVertexCount * 3 * 4, since we are dealing with byte offsets here
				IntPtr dest2Ptr = new IntPtr(destPtr.ToInt32() + (oldVertexCount * 12));

				int prePosVertexSize = 0;
				int postPosVertexSize = 0;
				int postPosVertexOffset = 0;

				if(wasSharedBuffer) {
					// Precalculate any dimensions of vertex areas outside the position
					prePosVertexSize = posElem.Offset;
                    postPosVertexOffset = prePosVertexSize + posElem.Size;
                    postPosVertexSize = vbuf.VertexSize - postPosVertexOffset;

                    // the 2 separate bits together should be the same size as the remainder buffer vertex
					Debug.Assert(newRemainderBuffer.VertexSize == (prePosVertexSize + postPosVertexSize));

                    IntPtr baseDestRemPtr = newRemainderBuffer.Lock(BufferLocking.Discard);

                    int baseSrcOffset = 0;
					int baseDestRemOffset = 0;

					unsafe {
                        float* pDest = (float*)destPtr.ToPointer();
                        float* pDest2 = (float*)dest2Ptr.ToPointer();

                        int destCount = 0, dest2Count = 0;

						// Iterate over the vertices
						for (int v = 0; v < oldVertexCount; v++) {
                            float* pSrc = (float*)((byte*)baseSrcPtr.ToPointer() + posElem.Offset + baseSrcOffset);
                            
                            // Copy position, into both buffers
							pDest[destCount++] = pDest2[dest2Count++] = pSrc[0];
							pDest[destCount++] = pDest2[dest2Count++] = pSrc[1];
							pDest[destCount++] = pDest2[dest2Count++] = pSrc[2];

							// now deal with any other elements 
							// Basically we just memcpy the vertex excluding the position
							if (prePosVertexSize > 0) {
								Memory.Copy(
                                    baseSrcPtr, baseDestRemPtr,
                                    baseSrcOffset, baseDestRemOffset,
                                    prePosVertexSize);
							}

							if (postPosVertexSize > 0) {
								Memory.Copy(
                                    baseSrcPtr, baseDestRemPtr,
									baseSrcOffset + postPosVertexOffset,
                                    baseDestRemOffset + prePosVertexSize,
                                    postPosVertexSize);
							}

							// increment the pointer offsets
							baseDestRemOffset += newRemainderBuffer.VertexSize;
							baseSrcOffset += vbuf.VertexSize;
						} // next vertex
					} // unsafe
				}
				else {
					// copy the data directly
					Memory.Copy(baseSrcPtr, destPtr, vbuf.Size);
					Memory.Copy(baseSrcPtr, dest2Ptr, vbuf.Size);
				}

				vbuf.Unlock();
				newPosBuffer.Unlock();

				if(wasSharedBuffer) {
					newRemainderBuffer.Unlock();
				}

				// At this stage, he original vertex buffer is going to be destroyed
				// So we should force the deallocation of any temporary copies
				HardwareBufferManager.Instance.ForceReleaseBufferCopies(vbuf);

				if (useVertexPrograms) {
					unsafe {
						// Now it's time to set up the w buffer
						hardwareShadowVolWBuffer = 
							HardwareBufferManager.Instance.CreateVertexBuffer(
							sizeof(float), 
							newVertexCount, 
							BufferUsage.StaticWriteOnly, 
							false);

						// Fill the first half with 1.0, second half with 0.0
						IntPtr wPtr = hardwareShadowVolWBuffer.Lock(BufferLocking.Discard);
						float* pDest = (float*)wPtr.ToPointer();
						int destCount = 0;

						for(int v = 0; v < oldVertexCount; v++) {
							pDest[destCount++] = 1.0f;
						}
						for(int v = 0; v < oldVertexCount; v++) {
							pDest[destCount++] = 0.0f;
						}
					} // unsafe

					hardwareShadowVolWBuffer.Unlock();
				} // if vertexPrograms

				ushort newPosBufferSource = 0; 

				if (wasSharedBuffer) {
					// Get the a new buffer binding index
					newPosBufferSource = vertexBufferBinding.NextIndex;

					// Re-bind the old index to the remainder buffer
					vertexBufferBinding.SetBinding(posOldSource, newRemainderBuffer);
				}
				else {
					// We can just re-use the same source idex for the new position buffer
					newPosBufferSource = posOldSource;
				}

				// Bind the new position buffer
				vertexBufferBinding.SetBinding(newPosBufferSource, newPosBuffer);

				// Now, alter the vertex declaration to change the position source
				// and the offsets of elements using the same buffer
				for(int i = 0; i < vertexDeclaration.ElementCount; i++) {
					VertexElement element = vertexDeclaration.GetElement(i);

					if(element.Semantic == VertexElementSemantic.Position) {
						// Modify position to point at new position buffer
						vertexDeclaration.ModifyElement(
							i, 
							newPosBufferSource, // new source buffer
							0, // no offset now
							VertexElementType.Float3, 
							VertexElementSemantic.Position);
					}
					else if(wasSharedBuffer &&
						element.Source == posOldSource &&
						element.Offset > prePosVertexSize) {

						// This element came after position, remove the position's
						// size
						vertexDeclaration.ModifyElement(
							i, 
							posOldSource, // same old source
							element.Offset - posElem.Size, // less offset now
							element.Type, 
							element.Semantic,
							element.Index);
					}
				}

			} // if posElem != null
		}

		/// <summary>
		///     Allocate elements to serve a holder of morph / pose target data 
		///	    for hardware morphing / pose blending.
        /// </summary>
		/// <remarks>
		///		This method will allocate the given number of 3D texture coordinate 
		///		sets for use as a morph target or target pose offset (3D position).
		///		These elements will be saved in hwAnimationDataList.
		///		It will also assume that the source of these new elements will be new
		///		buffers which are not bound at this time, so will start the sources to 
		///		1 higher than the current highest binding source. The caller is
		///		expected to bind these new buffers when appropriate. For morph animation
		///		the original position buffer will be the 'from' keyframe data, whilst
		///		for pose animation it will be the original vertex data.
        /// </remarks>
		public void AllocateHardwareAnimationElements(ushort count) {
			// Find first free texture coord set
			ushort texCoord = 0;
			for(int i = 0; i < vertexDeclaration.ElementCount; i++) {
				VertexElement element = vertexDeclaration.GetElement(i);
				if (element.Semantic == VertexElementSemantic.TexCoords)
					++texCoord;
			}
			Debug.Assert(texCoord <= Config.MaxTextureCoordSets);

			// Increase to correct size
			for (int c = HWAnimationDataList.Count; c < count; ++c) {
				// Create a new 3D texture coordinate set
				HardwareAnimationData data = new HardwareAnimationData();
				data.TargetVertexElement = vertexDeclaration.AddElement(
					vertexBufferBinding.NextIndex, 0, VertexElementType.Float3, VertexElementSemantic.TexCoords, texCoord++);

				HWAnimationDataList.Add(data);
				// Vertex buffer will not be bound yet, we expect this to be done by the
				// caller when it becomes appropriate (e.g. through a VertexAnimationTrack)
			}
		}

        #endregion Methods
	}

	/// <summary>
    /// 	Summary class collecting together index data source information.
    /// </summary>
    public class IndexData {
        #region Fields

		/// <summary>
		///		Reference to the <see cref="HardwareIndexBuffer"/> to use, must be specified if useIndexes = true
		/// </summary>
        public HardwareIndexBuffer indexBuffer;
		/// <summary>
		///		Index in the buffer to start from for this operation.
		/// </summary>
        public int indexStart;
		/// <summary>
		///		The number of indexes to use from the buffer.
		/// </summary>
        public int indexCount;
		
        #endregion

        #region Methods

		/// <summary>
		///		Creates a copy of the index data object, without a copy of the buffer data.
		/// </summary>
		/// <returns>A copy of this IndexData object without the data.</returns>
		public IndexData Clone() {
			return Clone(false);
		}

		/// <summary>
		///		Clones this vertex data, potentially including replicating any index buffers.
		/// </summary>
		/// <param name="copyData">
		///		If true, makes a copy the index buffer in addition to the definition.
		///		If false, the clone will refer to the same index buffer this object refers to.
		/// </param>
		/// <returns>A copy of this IndexData object.</returns>
        public IndexData Clone(bool copyData) {
			IndexData clone = new IndexData();

			if(indexBuffer != null) {
				if(copyData) {
					clone.indexBuffer = 
						HardwareBufferManager.Instance.CreateIndexBuffer(
							indexBuffer.Type,
							indexBuffer.IndexCount,
							indexBuffer.Usage,
							indexBuffer.HasShadowBuffer);

					// copy all the existing buffer data
					clone.indexBuffer.CopyData(indexBuffer, 0, 0, indexBuffer.Size, true);
				}
				else {
					clone.indexBuffer = indexBuffer;
				}
			}

			clone.indexStart = indexStart;
			clone.indexCount = indexCount;

			return clone;
        }

        #endregion Methods
    }
}
