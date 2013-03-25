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
using Axiom.Collections;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Graphics {
	/// <summary>
	///		Class which represents the renderable aspects of a set of shadow volume faces.
	/// </summary>
	/// <remarks>
	///		Note that for casters comprised of more than one set of vertex buffers (e.g. SubMeshes each
	///		using their own geometry), it will take more than one <see cref="ShadowRenderable"/> to render the 
	///		shadow volume. Therefore for shadow caster geometry, it is best to stick to one set of
	///		vertex buffers (not necessarily one buffer, but the positions for the entire geometry 
	///		should come from one buffer if possible)
	/// </remarks>
	public abstract class ShadowRenderable : IRenderable {
		#region Fields

		protected Material material;
		protected RenderOperation renderOp = new RenderOperation();
		/// <summary>
		///		Used only if IsLightCapSeparate == true.
		/// </summary>
		protected ShadowRenderable lightCap;
        protected List<Light> dummyLightList = new List<Light>();
		protected Hashtable customParams = new Hashtable();

		#endregion Fields

		#region Properties

		/// <summary>
		///		Does this renderable require a separate light cap?
		/// </summary>
		/// <remarks>
		///		If possible, the light cap (when required) should be contained in the
		///		usual geometry of the shadow renderable. However, if for some reason
		///		the normal depth function (less than) could cause artefacts, then a
		///		separate light cap with a depth function of 'always fail' can be used 
		///		instead. The primary example of this is when there are floating point
		///		inaccuracies caused by calculating the shadow geometry separately from
		///		the real geometry. 
		/// </remarks>
		public bool IsLightCapSeperate {
			get {
				return lightCap != null;
			}
		}

		/// <summary>
		///		Get the light cap version of this renderable.
		/// </summary>
		public ShadowRenderable LightCapRenderable {
			get {
				return lightCap;
			}
		}

		/// <summary>
		///		Should this ShadowRenderable be treated as visible?
		/// </summary>
		public virtual bool IsVisible {
			get {
				return true;
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>
		///		Gets the internal render operation for setup.
		/// </summary>
		/// <returns></returns>
		public RenderOperation GetRenderOperationForUpdate() {
			return renderOp;
		}

		#endregion Methods

        #region IRenderable Members

		public bool CastsShadows {
			get {
				return false;
			}
		}

		/// <summary>
		///		Gets/Sets the material to use for this shadow renderable.
		/// </summary>
		/// <remarks>
		///		Should be set by the caller before adding to a render queue.
		/// </remarks>
        public Material Material {
            get {
                return material;
            }
			set {
				material = value;
			}
        }

        public Technique Technique {
            get {
                return this.Material.GetBestTechnique();
            }
        }

		/// <summary>
		///		Gets the render operation for this shadow renderable.
		/// </summary>
		/// <param name="op"></param>
        public void GetRenderOperation(RenderOperation op) {
			// TODO: Ensure all other places throughout the engine set these properly
            op.indexData = renderOp.indexData;
			op.useIndices = true;
			op.operationType = OperationType.TriangleList;
			op.vertexData = renderOp.vertexData;
        }

        public abstract void GetWorldTransforms(Axiom.MathLib.Matrix4[] matrices);

        public List<Light> Lights {
            get {
                return dummyLightList;
            }
        }

        public bool NormalizeNormals {
            get {
                return false;
            }
        }

        public virtual ushort NumWorldTransforms {
            get {
                return 1;
            }
        }

        public virtual bool UseIdentityProjection {
            get {
                return false;
            }
        }

        public virtual bool UseIdentityView {
            get {
                return false;
            }
        }

        public virtual SceneDetailLevel RenderDetail {
            get {
                return SceneDetailLevel.Solid;
            }
        }

        public abstract Axiom.MathLib.Quaternion WorldOrientation { get; }

        public abstract Axiom.MathLib.Vector3 WorldPosition { get; }

        public virtual float GetSquaredViewDepth(Camera camera) {
            return 0;
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
    }
}
