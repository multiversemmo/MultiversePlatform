using System;
using System.Collections.Generic;
using System.Diagnostics;
using Axiom.Core;

namespace Axiom.FileSystem {
	/// <summary>
    ///     Specialization of IArchiveFactory for Zip files.
    /// </summary>
	public class ZipArchiveFactory : IArchiveFactory {
		#region IArchiveFactory Implementation

        /// <summary>
        ///     Creates a new zip file archive.
        /// </summary>
        /// <param name="name">Name of the archive to create.</param>
        /// <returns>A new isntance of ZipArchive.</returns>
        public Archive CreateArchive(string name) {
            return new Zip(name);
        }

        /// <summary>
        ///     Type of archive this factory creates.
        /// </summary>
        /// <value></value>
		public string Type {
			get {
				return ResourceManager.ZipFileResourceType;
			}
        }

        #endregion IArchiveFactory Implementation
    }
}
