using System;
using System.Collections;
using System.Collections.Specialized;
using Axiom.Core;

namespace Axiom.FileSystem {
	/// <summary>
    ///     ResourceManager specialization to handle Archive plug-ins.
    /// </summary>
	public sealed class ArchiveManager : IDisposable {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static ArchiveManager instance = new ArchiveManager();

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        internal ArchiveManager() {
            if (instance == null) {
                instance = this;

                // add zip and folder factories by default
                instance.AddArchiveFactory(new ZipArchiveFactory());
                instance.AddArchiveFactory(new FolderFactory());
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static ArchiveManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

		#region Fields

		/// <summary>
		/// The list of factories
		/// </summary>
        private Hashtable factories = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();

		#endregion

		#region Methods

		/// <summary>
		/// Add an archive factory to the list
		/// </summary>
		/// <param name="type">The type of the factory (zip, file, etc.)</param>
		/// <param name="factory">The factory itself</param>
		public void AddArchiveFactory(IArchiveFactory factory) {
			if (factories[factory.Type] != null) {
                throw new AxiomException("Attempted to add the {0} factory to ArchiveManager more than once.", factory.Type);
            }

			factories.Add(factory.Type, factory);
		}

		/// <summary>
		/// Get the archive factory
		/// </summary>
		/// <param name="type">The type of factory to get</param>
		/// <returns>The corresponding factory, or null if no factory</returns>
		public IArchiveFactory GetArchiveFactory(string type) {
			return (IArchiveFactory)factories[type];
		}

		#endregion Methods

        #region IDisposable Implementation

        /// <summary>
        ///     Called when the engine is shutting down.
        /// </summary>
        public void Dispose() {
            factories.Clear();

            instance = null;
        }

        #endregion IDisposable Implementation
    }
}
