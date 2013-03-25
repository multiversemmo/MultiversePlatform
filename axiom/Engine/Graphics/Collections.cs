using System;
using System.Collections;
using System.Collections.Generic;

namespace Axiom.Graphics {
	/// <summary>
	///     Generics: List<VertexElement>
	/// </summary>
	public class VertexElementList : ArrayList {}

	/// <summary>
	///     Generics: List<TextureEffect>
	/// </summary>
	public class TextureEffectList : ArrayList {}

	/// <summary>
	///     Generics: List<RenderTexture>
	/// </summary>
	public class RenderTextureList : ArrayList {}

	/// <summary>
	///     Generics: List<Pass>
	/// </summary>
	public class PassList : ArrayList {}

	/// <summary>
	///     Generics: List<Technique>
	/// </summary>
	public class TechniqueList : ArrayList {}

	/// <summary>
	///     Generics: List<TextureUnitState>
	/// </summary>
	public class TextureUnitStateList : ArrayList {}

	/// <summary>
	///     Generics: List<AutoConstantEntry>
	/// </summary>
	public class AutoConstantEntryList : List<GpuProgramParameters.AutoConstantEntry> {}

	/// <summary>
	///     Generics: List<AutoConstantEntry>
	/// </summary>
	public class IntConstantEntryList : List<GpuProgramParameters.IntConstantEntry> {
		public void Resize(int size) {
			while(this.Count < size) {
				Add(new GpuProgramParameters.IntConstantEntry());
			}
		}
	}

	/// <summary>
	///     Generics: List<IRenderable>
	/// </summary>
	public class RenderableList : ArrayList {}

	/// <summary>
	///     Generics: List<EdgeData.Triangle>
	/// </summary>
	public class TriangleList : ArrayList {}

	/// <summary>
	///     Generics: List<EdgeData.Edge>
	/// </summary>
	public class EdgeList : ArrayList {}

	/// <summary>
	///     Generics: List<EdgeGroup>
	/// </summary>
	public class EdgeGroupList : ArrayList {}

	/// <summary>
	///     Generics: List<VertexData>
	/// </summary>
	public class VertexDataList : ArrayList {}

	/// <summary>
	///     Generics: List<IndexData>
	/// </summary>
	public class IndexDataList : ArrayList {}

	/// <summary>
	///     Generics: List<ShadowRenderable>
	/// </summary>
	public class ShadowRenderableList : ArrayList {}

	/// <summary>
	///		Generics: List<RenderOperation>
	/// </summary>
	public class OperationTypeList : ArrayList {
		public void Add(OperationType type) {
			base.Add(type);
		}

		public new OperationType this[int index] {
			get {
				return (OperationType)base[index];
			}
			set {
				base[index] = value;
			}
		}
	}
}
