using System;
using System.Collections.Generic;
using Axiom.Core;

namespace Axiom.FileSystem {
	/// <summary>
    ///     Specialization of IArchiveFactory for file system folders.
    /// </summary>
	public class FolderFactory : IArchiveFactory {
		#region IArchiveFactory Implementation

        /// <summary>
        ///     Creates a new Folder archive.
        /// </summary>
        /// <param name="name">Name of the archive to create.</param>
        /// <returns>A new instance of a folder archive.</returns>
        public Archive CreateArchive(string name) {
            return new Folder(name);
        }

        /// <summary>
        ///     Type of archive this factory creates.
        /// </summary>
        /// <value></value>
		public string Type {
			get {
				return ResourceManager.FolderResourceType;
			}
        }

        #endregion IArchiveFactory Implementation
    }
}
