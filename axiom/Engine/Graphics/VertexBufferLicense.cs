using System;

namespace Axiom.Graphics {
	/// <summary>
	///		Structure holding details of a license to use a temporary shared buffer.
	/// </summary>
	public class VertexBufferLicense {
		#region Fields

		public HardwareVertexBuffer originalBuffer;
		public BufferLicenseRelease licenseType;
		public HardwareVertexBuffer buffer;
		public IHardwareBufferLicensee licensee;
		public int expiredDelay;

		#endregion Fields

		#region Constructor

		/// <summary>
		/// 
		/// </summary>
		public VertexBufferLicense(HardwareVertexBuffer originalBuffer, BufferLicenseRelease licenseType, 
								   int expiredDelay, HardwareVertexBuffer buffer, IHardwareBufferLicensee licensee) {

			this.originalBuffer = originalBuffer;
			this.licenseType = licenseType;
			this.expiredDelay = expiredDelay;
			this.buffer = buffer;
			this.licensee = licensee;
		}

		#endregion Constructor
	}
}
