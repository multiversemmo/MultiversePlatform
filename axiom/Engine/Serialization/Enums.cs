using System;

namespace Axiom.Serialization {
	/// <summary>
	///		Values that mark data chunks in the .mesh file.
	/// </summary>
	public enum MeshChunkID : ushort {
		/// <summary>
		///		string vesion;
		/// </summary>
		Header						= 0x1000,
		/// <summary>
		///		bool skeletallyAnimated: Important flag which affects h/w buffer policies
		///		Optional Geometry chunk.
		/// </summary>
		Mesh						= 0x3000,
		/// <summary>
		///		string materialName;
		///		bool useSharedVertices;
		///		uint indexCount;
		///		bool indexes32Bit;
		///		uint[indexCount]/ushort[indexCount] faceVertexIndices;
		///		M_GEOMETRY chunk (Optional: present only if useSharedVertices = false)
		/// </summary>
		SubMesh                     = 0x4000,
		/// <summary>
		///		ushort operationType: Optional - TriangleList assumed if missing.
		/// </summary>
		SubMeshOperation            = 0x4010,
		/// <summary>
		///		Optional bone weights (repeating section)
		///		uint vertexIndex;
		///		ushort boneIndex;
		///		float weight;
		/// </summary>
		SubMeshBoneAssignment		= 0x4100,
		/// <summary>
		///		string aliasName;
		///		string textureName;
        ///     Optional chunk that matches a texture name to an alias (repeating section)
        ///     a texture alias is sent to the submesh material to use this texture name
        ///     instead of the one in the texture unit with a matching alias name
		/// </summary>
        SubMeshTextureAlias         = 0x4200,
		/// <summary>
		///		This chunk is embedded within Mesh and SubMesh.
		///		uint vertexCount;
		/// </summary>
        Geometry                    = 0x5000,
		/// <summary>
		///		Beginning of a vertex delcaraion section.
		/// </summary>
		GeometryVertexDeclaration	= 0x5100,
		/// <summary>
		///		Optional (pre 1.30).
		/// </summary>
		GeometryNormals             = 0x5100,
		/// <summary>
		///		Repeating section.
		///		ushort source:		buffer bind source
		//		ushort type:		VertexElementType
		//		ushort semantic:	VertexElementSemantic
		//		ushort offset:		start offset in buffer in bytes
		//		ushort index:		index of the semantic (for colours and texture coords)
		/// </summary>
		GeometryVertexElement		= 0x5110,
		/// <summary>
		///		Repeating section.
		///		ushort bindIndex:	Index to bind this buffer to
		//		ushort vertexSize:	Per-vertex size, must agree with declaration at this index
		/// </summary>
		GeometryVertexBuffer		= 0x5200,
		/// <summary>
		///		Optional (pre 1.30).
		/// </summary>
		GeometryColors              = 0x5200,
		/// <summary>
		///		Raw buffer data.
		/// </summary>
		GeometryVertexBufferData	= 0x5210,
		/// <summary>
		///		Optional, REPEATABLE, each one adds an extra set.  (pre 1.30).
		/// </summary>
		GeometryTexCoords           = 0x5300,
		/// <summary>
		///		Optional link to skeleton.
		///		string skeletonName:	name of .skeleton to use
		/// </summary>
		MeshSkeletonLink            = 0x6000,
		/// <summary>
		///		Optional bone weights (repeating section)
		//		uint vertexIndex;
		//		ushort boneIndex;
		//		float weight;
		/// </summary>
		MeshBoneAssignment          = 0x7000,
		/// <summary>
		///		Optional LOD information
		//		ushort numLevels;
		//		bool manual;  (true for manual alternate meshes, false for generated)
		/// </summary>
		MeshLOD                     = 0x8000,
		/// <summary>
		///		Repeating section, ordered in increasing depth
		//		LOD 0 (full detail from 0 depth) is omitted
		//		float fromSquaredDepth;
		/// </summary>
		MeshLODUsage                = 0x8100,
		/// <summary>
		///		Required if MeshLOD section manual = true
		//		string manualMeshName;
		/// </summary>
		MeshLODManual               = 0x8110,
		/// <summary>
		///		Required if MeshLOD section manual = false
		//		Repeating section (1 per submesh)
		//		uint indexCount;
		//		bool indexes32Bit
		//		ushort[indexCount]/uint[indexCount] faceIndexes;
		/// </summary>
		MeshLODGenerated            = 0x8120,
		/// <summary>
		///		float minx, miny, minz;
		//		float maxx, maxy, maxz;
		//		float radius;
		/// </summary>
		MeshBounds                  = 0x9000,
		/// <summary>
		///		Added By DrEvil
		//		Optional chunk that contains a table of submesh indexes and the names of
		//		the sub-meshes.
		/// </summary>
		SubMeshNameTable			= 0xA000,
		/// <summary>
		///		short index;
		//		string name;
		/// </summary>
		SubMeshNameTableElement		= 0xA100,
		/// <summary>
		///		Optional chunk which stores precomputed edge data.
		/// </summary>
		EdgeLists					= 0xB000,
		/// <summary>
		///		Each LOD has a seperate edge list.
		///		ushort		lodIndex;
		//		bool		isManual:	If manual, no edge data here, loaded from manual mesh
		//		ulong		numTriangles
		//		ulong		numEdgeGroups
		//		Triangle	triangleList[numTriangles];
		//		ulong		indexSet
		//		ulong		vertexSet
		//		ulong		vertIndex[3]
		//		ulong		sharedVertIndex[3] 
		//		float		normal[4]
		/// </summary>
		EdgeListLOD					= 0xB100,
		/// <summary>
		///		ulong	vertexSet;
		//		ulong	numEdges;
		//		Edge	edgeList[numEdges];
		//		ulong	triIndex[2];
		//		ulong	vertIndex[2];
		//		ulong	sharedVertIndex[2];
		//		bool	degenerate;
		/// </summary>
		EdgeListGroup				= 0xB110,

        /// <summary>
		///		Optional poses section, referred to by pose keyframes
		/// </summary>
		Poses                       = 0xC000,
        /// <summary>
		///		string name;
		///     ushort target - 0 for shared geometry; 1+ for submesh index + 1
		/// </summary>
		Pose                        = 0xC100,
        /// <summary>
		///		ulong vertexIndex
		///     float xoffset, yoffset, zoffset
		/// </summary>
		PoseVertex                  = 0xC111,
        /// <summary>
		///		Optional vertex animation chunk
		/// </summary>
        Animations                  = 0xD000,
        /// <summary>
		///		string name;
		///     float length
		/// </summary>
        Animation                   = 0xD100,
        /// <summary>
		///		ushort type			// 1 == morph, 2 == posestring name;
		///     ushort target		// 0 for shared geometry; 1+ for submesh index + 1
		/// </summary>
        AnimationTrack              = 0xD110,
        /// <summary>
		///		float time
		///     float x, y, z       // repeat by number of vertices in original geometry
		/// </summary>
        AnimationMorphKeyframe      = 0xD111,
        /// <summary>
		///		float time
		/// </summary>
        AnimationPoseKeyframe       = 0xD112,
        /// <summary>
		///		repeat for number of referenced poses:
		///		ushort poseIndex
		///		float influence
		/// </summary>
        AnimationPoseRef            = 0xD113,

        // Multiverse Additions
        AttachmentPoint             = 0xE000,
        DependencyInfo              = 0xE100,
        MeshDependency              = 0xE101,
        SkeletonDependency          = 0xE102,
        MaterialDependency          = 0xE103,

	};
}
