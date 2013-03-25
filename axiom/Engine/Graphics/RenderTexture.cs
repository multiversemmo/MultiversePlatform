using System;
using Axiom.Core;
using Axiom.Media;

namespace Axiom.Graphics {
	/// <summary>
	///    Custom RenderTarget that allows for rendering a scene to a texture.
	/// </summary>
	public abstract class RenderTexture : RenderTarget {
        #region Fields

        protected HardwarePixelBuffer pixelBuffer;
        protected int zOffset = 0;

        #endregion Fields

        #region Constructors

        public RenderTexture(HardwarePixelBuffer buffer, int zOffset) {
            pixelBuffer = buffer;
            this.zOffset = zOffset;
            priority = RenderTargetPriority.High;
            width = buffer.Width;
            height = buffer.Height;
            colorDepth = PixelUtil.GetNumElemBits(buffer.Format);
        }

        //public RenderTexture(string name, int width, int height) : 
        //    this(name, width, height, TextureType.TwoD) {}

        //public RenderTexture(string name, int width, int height, PixelFormat format) : 
        //    this(name, width, height, TextureType.TwoD, format) {}

        //public RenderTexture(string name, int width, int height, TextureType type) : 
        //    this(name, width, height, TextureType.TwoD, PixelFormat.R8G8B8) {}
		
        //public RenderTexture(string name, int width, int height, TextureType type, PixelFormat format) {
        //    this.name = name;
        //    this.width = width;
        //    this.height = height;
        //    // render textures are high priority
        //    this.priority = RenderTargetPriority.High;
        //    texture = TextureManager.Instance.CreateManual(name, type, width, height, 0, format, TextureUsage.RenderTarget);
        //    TextureManager.Instance.Load(texture, 1);
        //}

        #endregion Constructors

        #region Methods

        /// <summary>
        ///    Ensures texture is destroyed.
        /// </summary>
        public override void Dispose() {
            base.Dispose();

            pixelBuffer.ClearSliceRTT(0);
        }

        #endregion Methods
	
		public PixelFormat Format {
			get { return pixelBuffer.Format; }
		}
				
	}
}
