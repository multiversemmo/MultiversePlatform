using System;
using Axiom.Animating;
using Axiom.Collections;
using Axiom.Controllers;
using Axiom.FileSystem;
using Axiom.Fonts;
using Axiom.Media;
using Axiom.Overlays;
using Axiom.Input;
using Axiom.ParticleSystems;
using Axiom.Graphics;

namespace Axiom.Core
{
	/// <summary>
	/// Static singleton aggregator utility class
	/// </summary>
	public sealed class Ax
	{
		/// <summary>
		/// Private constructor to prevent instantiation
		/// </summary>
		private Ax() 
		{
		}

		#region Static Fields and Properties
		public static RenderSystem Renderer { get { return Root.RenderSystem; } }
		public static SceneManager Scene { get { return Root.SceneManager; } }
		public static Root Root { get { return Root.Instance; } }
		public static LogManager Log { get { return LogManager.Instance; } }
		public static MaterialManager Materials { get { return MaterialManager.Instance; } }
		public static ArchiveManager Archives { get { return ArchiveManager.Instance; } }
		public static MeshManager Meshes { get { return MeshManager.Instance; } }
		public static SkeletonManager Skeletons { get { return SkeletonManager.Instance; } }
		public static ParticleSystemManager Particles { get { return ParticleSystemManager.Instance; } }
		public static IPlatformManager Platform { get { return PlatformManager.Instance; } }
		public static OverlayManager Overlays { get { return OverlayManager.Instance; } }
		public static OverlayElementManager OverlayElements { get { return OverlayElementManager.Instance; } }
		public static FontManager Fonts { get { return FontManager.Instance; } }
		public static CodecManager Codecs { get { return CodecManager.Instance; } }
		public static HighLevelGpuProgramManager GpuPrograms { get { return HighLevelGpuProgramManager.Instance; } }
		public static PluginManager Plugins { get { return PluginManager.Instance; } }
		public static TextureManager Textures { get { return TextureManager.Instance; } }
		public static ControllerManager Controllers { get { return ControllerManager.Instance; } }
		public static HardwareBufferManager Buffers { get { return HardwareBufferManager.Instance; } }
		#endregion
	}
}
