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
using System.Runtime.InteropServices;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.RenderSystems.DirectX9 {
    /// <summary>
    /// 	Summary description for D3DHardwareVertexBuffer.
    /// </summary>
    public class D3DHardwareVertexBuffer : HardwareVertexBuffer {
        #region Member variables

        protected D3D.VertexBuffer d3dBuffer;
        protected D3D.Pool d3dPool;

        static Axiom.Utility.TimingMeter vbufferLockTimer = Axiom.Utility.MeterManager.GetMeter("Buffer Lock", "Axiom.RenderSystems.DirectX9");

        #endregion
		
        #region Constructors
		
        public D3DHardwareVertexBuffer(int vertexSize, int numVertices, BufferUsage usage, 
            D3D.Device device, bool useSystemMemory, bool useShadowBuffer) 
            : base(vertexSize, numVertices, usage, useSystemMemory, useShadowBuffer) {
#if !NO_OGRE_D3D_MANAGE_BUFFERS
		    d3dPool = useSystemMemory? Pool.SystemMemory : 
			    // If not system mem, use managed pool UNLESS buffer is discardable
			    // if discardable, keeping the software backing is expensive
                ((usage & BufferUsage.Discardable) != 0) ? Pool.Default : Pool.Managed;
#else
            d3dPool = useSystemMemory ? Pool.SystemMemory : Pool.Default;
#endif
            // Create the d3d vertex buffer
            d3dBuffer = new D3D.VertexBuffer(device, 
                sizeInBytes, 
                D3DHelper.ConvertEnum(usage), 
                VertexFormats.None, 
                d3dPool);
        }

		~D3DHardwareVertexBuffer() {
			if ( d3dBuffer != null )
			{
				d3dBuffer.Dispose();
			}
        }

        #endregion
		
        #region Methods
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="locking"></param>
        /// <returns></returns>
        /// DOC
        protected override IntPtr LockImpl(int offset, int length, BufferLocking locking) {
            D3D.LockFlags d3dLocking = D3DHelper.ConvertEnum(locking, usage);
			Microsoft.DirectX.GraphicsStream s = d3dBuffer.Lock(offset, length, d3dLocking);
			return s.InternalData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// DOC
        public override void UnlockImpl() {
            // unlock the buffer
            d3dBuffer.Unlock();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="dest"></param>
        /// DOC
        public override void ReadData(int offset, int length, IntPtr dest) {
            // lock the buffer for reading
            IntPtr src = this.Lock(offset, length, BufferLocking.ReadOnly);
			
            // copy that data in there
            Memory.Copy(src, dest, length);

            // unlock the buffer
            this.Unlock();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="src"></param>
        /// <param name="discardWholeBuffer"></param>
        /// DOC
        public override void WriteData(int offset, int length, IntPtr src, bool discardWholeBuffer) {
            vbufferLockTimer.Enter();
            // lock the buffer real quick
            IntPtr dest = this.Lock(offset, length, 
                discardWholeBuffer ? BufferLocking.Discard : BufferLocking.Normal);
            vbufferLockTimer.Exit();
            // copy that data in there
            Memory.Copy(src, dest, length);

            // unlock the buffer
            this.Unlock();
        }
        	
        //---------------------------------------------------------------------
	    public bool ReleaseIfDefaultPool()
	    {
            if (d3dPool == Pool.Default) {
                if (d3dBuffer != null) {
                    d3dBuffer.Dispose();
                    d3dBuffer = null;
                }
                return true;
            }
            return false;
	    }

	    //---------------------------------------------------------------------
	    public bool RecreateIfDefaultPool(D3D.Device device)
	    {
		    if (d3dPool == Pool.Default)
		    {
			    // Create the d3d vertex buffer
                d3dBuffer = new D3D.VertexBuffer(
                    typeof(byte), 
                    sizeInBytes, 
                    device,
                    D3DHelper.ConvertEnum(usage), 
                    VertexFormats.None, 
                    d3dPool);
			    return true;
		    }
		    return false;
	    }
        
        public override void Dispose() {
			if (d3dBuffer != null)
				d3dBuffer.Dispose();
			d3dBuffer = null;
		}
        #endregion
		
        #region Properties

        /// <summary>
        ///		Gets the underlying D3D Vertex Buffer object.
        /// </summary>
        public D3D.VertexBuffer D3DVertexBuffer {
            get { 
                return d3dBuffer; 
            }
        }
		
        #endregion
	}
}
