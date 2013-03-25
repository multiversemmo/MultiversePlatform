using System;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;

namespace Axiom.Graphics
{
	/// <summary>
	///		Allows the rendering of a simple 2D rectangle
	///		This class renders a simple 2D rectangle; this rectangle has no depth and
	///		therefore is best used with specific render queue and depth settings,
	///		like <see cref="RenderQueueGroupID.Background"/> and 'depth_write off' for backdrops, and 
	///		<see cref="RenderQueueGroupID.Overlay"/> and 'depth_check off' for fullscreen quads.
	/// </summary>
	public class Rectangle2D : SimpleRenderable {

		const int POSITION = 0;
		const int TEXCOORD = 1;

		static float[] texCoords = new float[] {0, 0, 0, 1, 1, 0, 1, 1};

		public Rectangle2D() : this(false) {}

		public Rectangle2D(bool includeTextureCoordinates) {
			vertexData = new VertexData();
			vertexData.vertexStart = 0;
			vertexData.vertexCount = 4;

			VertexDeclaration decl = vertexData.vertexDeclaration;
			VertexBufferBinding binding = vertexData.vertexBufferBinding;

			decl.AddElement(POSITION, 0, VertexElementType.Float3, VertexElementSemantic.Position);

			HardwareVertexBuffer buffer =
				HardwareBufferManager.Instance.CreateVertexBuffer(
					decl.GetVertexSize(POSITION),
					vertexData.vertexCount,
					BufferUsage.StaticWriteOnly);

			binding.SetBinding(POSITION, buffer);

			if(includeTextureCoordinates) {
				decl.AddElement(TEXCOORD, 0, VertexElementType.Float2, VertexElementSemantic.TexCoords);

				buffer =
					HardwareBufferManager.Instance.CreateVertexBuffer(
					decl.GetVertexSize(TEXCOORD),
					vertexData.vertexCount,
					BufferUsage.StaticWriteOnly);

				binding.SetBinding(TEXCOORD, buffer);

				buffer.WriteData(0, buffer.Size, texCoords, true);	
			}

			// TODO: Fix
			material = MaterialManager.Instance.GetByName("BaseWhite");
			material.Lighting = false;
		}

		#region SimpleRenderable Members

		public override float BoundingRadius {
			get {
				return 0;
			}
		}

		public override float GetSquaredViewDepth(Camera camera) {
			return 0;
		}

		public override void GetRenderOperation(RenderOperation op) {
			op.vertexData = vertexData;
			op.useIndices = false;
			op.operationType = OperationType.TriangleStrip;
		}

		public override void GetWorldTransforms(Axiom.MathLib.Matrix4[] matrices) {
			// return identity matrix to prevent parent transforms
			matrices[0] = Matrix4.Identity;
		}


		public override bool UseIdentityProjection {
			get {
				return true;
			}
		}

		public override bool UseIdentityView {
			get {
				return true;
			}
		}

		public override Quaternion WorldOrientation {
			get {
				return Quaternion.Identity;
			}
		}

		public override Vector3 WorldPosition {
			get {
				return Vector3.Zero;
			}
		}


		#endregion SimpleRenderable Members

		#region Methods

		/// <summary>
		///		Sets the corners of the rectangle, in relative coordinates.
		/// </summary>
		/// <param name="left">Left position in screen relative coordinates, -1 = left edge, 1.0 = right edge.</param>
		/// <param name="top">Top position in screen relative coordinates, 1 = top edge, -1 = bottom edge.</param>
		/// <param name="right">Position in screen relative coordinates.</param>
		/// <param name="bottom">Position in screen relative coordinates.</param>
		public void SetCorners(float left, float top, float right, float bottom) {
			float[] data = new float[] {
				left, top, -1,
				left, bottom, -1,
				right, top, -1,
				right, bottom, -1
			};

			HardwareVertexBuffer buffer = 
				vertexData.vertexBufferBinding.GetBuffer(POSITION);

			buffer.WriteData(0, buffer.Size, data, true);

			box = new AxisAlignedBox();
			box.SetExtents(new Vector3(left, top, 0), new Vector3(right, bottom, 0));
		}

		#endregion Methods
	}
}
