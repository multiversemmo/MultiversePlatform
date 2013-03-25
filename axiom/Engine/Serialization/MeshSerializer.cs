using System;
using System.Collections;
using System.IO;
using Axiom.Core;

namespace Axiom.Serialization {
	/// <summary>
	///		Class for serialising mesh data to/from an OGRE .mesh file.
	/// </summary>
	/// <remarks>
	///		This class allows exporters to write OGRE .mesh files easily, and allows the
	///		OGRE engine to import .mesh files into instatiated OGRE Meshes.
	///		<p/>
	///		It's important to realize that this exporter uses OGRE terminology. In this context,
	///		'Mesh' means a top-level mesh structure which can actually contain many SubMeshes, each
	///		of which has only one Material. Modelling packages may refer to these differently, for
	///		example in Milkshape, it says 'Model' instead of 'Mesh' and 'Mesh' instead of 'SubMesh', 
	///		but the theory is the same.
	/// </remarks>
	public sealed class MeshSerializer : Serializer {
		#region Fields

		/// <summary>
		///		Lookup table holding the various mesh serializer versions.
		/// </summary>
		private Hashtable implementations = new Hashtable();

		/// <summary>
		///		Current version string.
		/// </summary>
		private static string currentVersion = "[MeshSerializer_v1.30]";

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Default constructor.
		/// </summary>
		public MeshSerializer() {
			// add the supported .mesh versions
			implementations.Add("[MeshSerializer_v1.10]", new MeshSerializerImplv11());
			implementations.Add("[MeshSerializer_v1.20]", new MeshSerializerImplv12());
			implementations.Add(currentVersion, new MeshSerializerImpl());
		}

		#endregion Constructor

		#region Methods

		/// <summary>
		///		Exports a mesh to the file specified.
		/// </summary>
		/// <param name="mesh">Reference to the mesh to export.</param>
		/// <param name="fileName">The destination filename.</param>
		public void ExportMesh(Mesh mesh, string fileName) {
            // call implementation
            MeshSerializerImpl serializer = (MeshSerializerImpl)implementations[currentVersion];
            serializer.ExportMesh(mesh, fileName);
		}

		/// <summary>
		///		Imports mesh data from a .mesh file.
		/// </summary>
		/// <param name="stream">The stream holding the .mesh data. Must be initialised (pos at the start of the buffer).</param>
		/// <param name="mesh">Reference to the Mesh object which will receive the data. Should be blank already.</param>
		public void ImportMesh(Stream stream, Mesh mesh) {
			BinaryMemoryReader reader = new BinaryMemoryReader(stream);

			// read the header ID
			ushort headerID = ReadUShort(reader);

			if(headerID != (ushort)MeshChunkID.Header) {
				throw new AxiomException("File header not found.");
			}

			// read version
			string fileVersion = ReadString(reader);

			// set jump back to the start of the reader
			Seek(reader, 0, SeekOrigin.Begin);

			// barf if there specified version is not supported
			if(!implementations.ContainsKey(fileVersion)) {
				throw new AxiomException("Cannot find serializer implementation for version '{0}'.", fileVersion);
			}

            LogManager.Instance.Write("Mesh: Loading '{0}'...", mesh.Name);

            // call implementation
			MeshSerializerImpl serializer = (MeshSerializerImpl)implementations[fileVersion];
			serializer.ImportMesh(reader, mesh);

			// warn on old version of mesh
			if(fileVersion != currentVersion) {
				LogManager.Instance.Write("WARNING: {0} is an older format ({1}); you should upgrade it as soon as possible using the OgreMeshUpdate tool.", mesh.Name, fileVersion);
			}

            LogManager.Instance.Write("Mesh: Finished loading '{0}'", mesh.Name);
		}

        public DependencyInfo GetDependencyInfo(Stream stream, Mesh mesh) {
            BinaryMemoryReader reader = new BinaryMemoryReader(stream);
            
            // read the header ID
            ushort headerID = ReadUShort(reader);

            if (headerID != (ushort)MeshChunkID.Header) {
                throw new AxiomException("File header not found.");
            }

            // read version
            string fileVersion = ReadString(reader);

            // set jump back to the start of the reader
            Seek(reader, 0, SeekOrigin.Begin);

            // barf if there specified version is not supported
            if (!implementations.ContainsKey(fileVersion)) {
                throw new AxiomException("Cannot find serializer implementation for version '{0}'.", fileVersion);
            }

            LogManager.Instance.Write("Mesh: Fetching dependency info '{0}'...", mesh.Name);

            // call implementation
            MeshSerializerImpl serializer = (MeshSerializerImpl)implementations[fileVersion];
            DependencyInfo rv = serializer.GetDependencyInfo(stream, mesh);

            // warn on old version of mesh
            if (fileVersion != currentVersion) {
                LogManager.Instance.Write("WARNING: {0} is an older format ({1}); you should upgrade it as soon as possible using the OgreMeshUpdate tool.", mesh.Name, fileVersion);
            }
            return rv;
        }

		#endregion Methods
	}
}
