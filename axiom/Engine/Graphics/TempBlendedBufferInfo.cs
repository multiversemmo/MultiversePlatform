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
using System.Diagnostics;

namespace Axiom.Graphics {
	/// <summary>
	///     Records the use of temporary blend buffers.
	/// </summary>
	public class TempBlendedBufferInfo : IHardwareBufferLicensee {
        #region Fields

        /// <summary>
        ///     Pre-blended position buffer.
        /// </summary>
        public HardwareVertexBuffer srcPositionBuffer;
        /// <summary>
        ///     Pre-blended normal buffer.
        /// </summary>
        public HardwareVertexBuffer srcNormalBuffer;
        /// <summary>
        ///     Pre-blended tangent buffer.
        /// </summary>
        public HardwareVertexBuffer srcTangentBuffer;
        /// <summary>
        ///     Pre-blended binormal buffer.
        /// </summary>
        public HardwareVertexBuffer srcBinormalBuffer;
        /// <summary>
        ///     Post-blended position buffer.
        /// </summary>
        public HardwareVertexBuffer destPositionBuffer;
        /// <summary>
        ///     Post-blended normal buffer.
        /// </summary>
        public HardwareVertexBuffer destNormalBuffer;
        /// <summary>
        ///     Post-blended tangent buffer.
        /// </summary>
        public HardwareVertexBuffer destTangentBuffer;
        /// <summary>
        ///     Post-blended binormal buffer.
        /// </summary>
        public HardwareVertexBuffer destBinormalBuffer;
        /// <summary>
        ///     Both positions and normals are contained in the same buffer
        /// </summary>
		public bool posNormalShareBuffer;
        /// <summary>
        ///     Index at which the positions are bound in the buffer.
        /// </summary>
        public ushort posBindIndex;
        /// <summary>
        ///     Index at which the normals are bound in the buffer.
        /// </summary>
        public ushort normBindIndex;
        /// <summary>
        ///     Index at which the tangents are bound in the buffer.
        /// </summary>
        public ushort tanBindIndex;
        /// <summary>
        ///     Index at which the binormals are bound in the buffer.
        /// </summary>
        public ushort binormBindIndex;
        /// <summary>
		///		Should we bind the position buffer
		/// </summary>
		public bool bindPositions;
        /// <summary>
        ///		Should we bind the normals buffer
        /// </summary>
        public bool bindNormals;
        /// <summary>
        ///		Should we bind the tangents buffer
        /// </summary>
        public bool bindTangents;
        /// <summary>
        ///		Should we bind the binormals buffer
        /// </summary>
        public bool bindBinormals;

        #endregion Fields

        #region Methods

		
        /// <summary>
        ///		Utility method, extract info from the given VertexData
        /// </summary>
		public void ExtractFrom(VertexData sourceData) {
			// Release old buffer copies first
			HardwareBufferManager mgr = HardwareBufferManager.Instance;
			if (destPositionBuffer != null) {
				mgr.ReleaseVertexBufferCopy(destPositionBuffer);
				Debug.Assert(destPositionBuffer == null);
			}
			if (destNormalBuffer != null) {
				mgr.ReleaseVertexBufferCopy(destNormalBuffer);
				Debug.Assert(destNormalBuffer == null);
			}

			VertexDeclaration decl = sourceData.vertexDeclaration;
			VertexBufferBinding bind = sourceData.vertexBufferBinding;
			VertexElement posElem = decl.FindElementBySemantic(VertexElementSemantic.Position);
			VertexElement normElem = decl.FindElementBySemantic(VertexElementSemantic.Normal);
            VertexElement tanElem = decl.FindElementBySemantic(VertexElementSemantic.Tangent);
            VertexElement binormElem = decl.FindElementBySemantic(VertexElementSemantic.Binormal);

			Debug.Assert(posElem != null, "Positions are required");

			posBindIndex = posElem.Source;
			srcPositionBuffer = bind.GetBuffer(posBindIndex);

			if (normElem == null) {
				posNormalShareBuffer = false;
				srcNormalBuffer = null;
			}
			else {
				normBindIndex = normElem.Source;
				if (normBindIndex == posBindIndex) {
					posNormalShareBuffer = true;
					srcNormalBuffer = null;
				}
				else {
					posNormalShareBuffer = false;
					srcNormalBuffer = bind.GetBuffer(normBindIndex);
				}
			}
            if (tanElem == null)
                srcTangentBuffer = null;
		    else {
                tanBindIndex = tanElem.Source;
                srcTangentBuffer = bind.GetBuffer(tanBindIndex);
            }

            if (binormElem == null)
                srcBinormalBuffer = null;
			else {
                binormBindIndex = binormElem.Source;
                srcBinormalBuffer = bind.GetBuffer(binormBindIndex);
            }
		}

		/// <summary>
		///     Utility method, checks out temporary copies of src into dest.
		/// </summary>
		public void CheckoutTempCopies(bool positions, bool normals, bool tangents, bool binormals) {
			bindPositions = positions;
			bindNormals = normals;
            bindTangents = tangents;
            bindBinormals = binormals;

			if(bindPositions && destPositionBuffer == null) {
				destPositionBuffer = 
					HardwareBufferManager.Instance.AllocateVertexBufferCopy(
					srcPositionBuffer,
					BufferLicenseRelease.Automatic,
					this);
			}

            if (bindNormals && !posNormalShareBuffer && 
				srcNormalBuffer != null && destNormalBuffer == null) {
				destNormalBuffer =
					HardwareBufferManager.Instance.AllocateVertexBufferCopy(
					srcNormalBuffer,
					BufferLicenseRelease.Automatic,
					this);
            }

            if (bindTangents && srcTangentBuffer != null) {
                if (this.tanBindIndex != this.posBindIndex &&
                    this.tanBindIndex != this.normBindIndex) {
                    destTangentBuffer =
                           HardwareBufferManager.Instance.AllocateVertexBufferCopy(
                           srcTangentBuffer,
                           BufferLicenseRelease.Automatic,
                           this);
                }
            }

            if (bindNormals && srcBinormalBuffer != null) {
                if (this.binormBindIndex != this.posBindIndex &&
                    this.binormBindIndex != this.normBindIndex &&
                    this.binormBindIndex != this.tanBindIndex) {
                    destBinormalBuffer =
                        HardwareBufferManager.Instance.AllocateVertexBufferCopy(
                        srcBinormalBuffer,
                        BufferLicenseRelease.Automatic,
                        this);
                }
            }
        }

		public void CheckoutTempCopies() {
			CheckoutTempCopies(true, true, true, true);
		}

        /// <summary>
        ///     Detect currently have buffer copies checked out and touch it
        /// </summary>
		public bool BuffersCheckedOut(bool positions, bool normals) {
			if (positions || (normals && posNormalShareBuffer)) {
				if (destPositionBuffer == null)
					return false;
				HardwareBufferManager.Instance.TouchVertexBufferCopy(destPositionBuffer);
			}
			if (normals && !posNormalShareBuffer) {
				if (destNormalBuffer == null)
					return false;
				HardwareBufferManager.Instance.TouchVertexBufferCopy(destNormalBuffer);
			}
			return true;
		}

        /// <summary>
        ///     Utility method, binds dest copies into a given VertexData.
        /// </summary>
        /// <param name="targetData">VertexData object to bind the temp buffers into.</param>
        /// <param name="suppressHardwareUpload"></param>
        public void BindTempCopies(VertexData targetData, bool suppressHardwareUpload) {
            destPositionBuffer.SuppressHardwareUpdate(suppressHardwareUpload);
			targetData.vertexBufferBinding.SetBinding(posBindIndex, destPositionBuffer);

            if (bindNormals && destNormalBuffer != null) {
                if (normBindIndex != posBindIndex) {
                    destNormalBuffer.SuppressHardwareUpdate(suppressHardwareUpload);
                    targetData.vertexBufferBinding.SetBinding(normBindIndex, destNormalBuffer);
                }
            }
            if (bindTangents && destTangentBuffer != null) {
                if (tanBindIndex != posBindIndex &&
                    tanBindIndex != normBindIndex) {
                    destTangentBuffer.SuppressHardwareUpdate(suppressHardwareUpload);
                    targetData.vertexBufferBinding.SetBinding(tanBindIndex, destTangentBuffer);
                }
            }
            if (bindBinormals && destBinormalBuffer != null) {
                if (binormBindIndex != posBindIndex &&
                    binormBindIndex != normBindIndex &&
                    binormBindIndex != tanBindIndex) {
                    destBinormalBuffer.SuppressHardwareUpdate(suppressHardwareUpload);
                    targetData.vertexBufferBinding.SetBinding(binormBindIndex, destBinormalBuffer);
                }
            }
        }

        #endregion Methods

        #region IHardwareBufferLicensee Members

        /// <summary>
        ///     Implementation of LicenseExpired.
        /// </summary>
        /// <param name="buffer"></param>
        public void LicenseExpired(HardwareBuffer buffer) {
			if(buffer == destPositionBuffer) {
				destPositionBuffer = null;
			}
			if(buffer == destNormalBuffer) {
				destNormalBuffer = null;
			}
        }

        #endregion
    }
}
