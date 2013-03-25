#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Media;
using Axiom.Graphics;

namespace Axiom.SceneManagers.Bsp
{
	public enum Quake3LumpType
	{
		Entities = 0,
		Shaders,
		Planes,
		Nodes,
		Leaves,
		LeafFaces,
		LeafBrushes,
		Models,
		Brushes,
		BrushSides,
		Vertices,
		Elements,
		Fog,
		Faces,
		Lightmaps,
		LightVolumes,
		Visibility,
		NumLumps
	}

	/// <summary>
	///		Support for loading and extracting data from a Quake3 level file.
	///	</summary>
	///	<remarks>
	///		This class implements the required methods for opening Quake3 level files
    ///		and extracting the pertinent data within. Ogre supports BSP based levels
    ///		through it's own BspLevel class, which is not specific to any file format,
    ///		so this class is here to source that data from the Quake3 format.
    ///		</p>
    ///		Quake3 levels include far more than just data for rendering - typically the
    ///		<strong>leaves</strong> of the tree are used for rendering, and <strong>brushes,</strong>
    ///		are used to define convex hulls made of planes for collision detection. There are also
    ///		<strong>entities</strong> which define non-visual elements like player start
    ///		points, triggers etc and <strong>models</strong> which are used for movable
    ///		scenery like doors and platforms. <strong>Shaders</strong> meanwhile are textures
    ///		with extra effects and 'content flags' indicating special properties like
    ///		water or lava.
    ///		<p/>
    ///		I will try to support as much of this as I can in Ogre, but I won't duplicate
    ///		the structure or necesarily use the same terminology. Quake3 is designed for a very specific
    ///		purpose and code structure, whereas Ogre is designed to be more flexible,
    ///		so for example I'm likely to separate game-related properties like surface flags
    ///		from the generics of materials in my implementation.</p>
    ///		This is a utility class only - a single call to loadFromChunk should be
    ///		enough. You should not expect the state of this object to be consistent
    ///		between calls, since it uses pointers to memory which may no longer
    ///		be valid after the original call. This is why it has no accessor methods
    ///		for reading it's internal state.
    ///	</remarks>
	public class Quake3Level 
	{
		#region Internal storage
		// This is ALL temporary. Don't rely on it being static
		// NB no brushes, fog or local lightvolumes yet
		private Stream chunk;
		private InternalBspHeader header;
		
		private int[] elements;
		private string entities;
		private int[] leafBrushes;
		private int[] leafFaces;

		private InternalBspModel[] models;
		private InternalBspNode[] nodes;
		private InternalBspLeaf[] leaves;
		private InternalBspPlane[] planes;
		private InternalBspFace[] faces;
		private InternalBspVertex[] vertices;
		private InternalBspShader[] shaders;
		private InternalBspVis visData;
		private InternalBspBrush[] brushes;
		private InternalBspBrushSide[] brushSides;

		protected BspOptions options;
		#endregion

		#region Properties
		public int NumVertices { get { return vertices.Length; } }
		public int NumFaces	{ get { return faces.Length; } }
		public int NumLeafFaces { get { return leafFaces.Length; } }
		public int NumElements { get { return elements.Length; } }
		public int NumNodes { get { return nodes.Length; } }
		public int NumLeaves { get { return leaves.Length; } }
		public int NumBrushes { get { return brushes.Length; } }
		public BspOptions Options { get { return options; } }

		public int[] LeafFaces { get { return leafFaces; } }
		public int[] LeafBrushes { get { return leafBrushes; } }
		public int[] Elements { get { return elements; } }
		public string Entities { get { return entities; } }
		public InternalBspVertex[] Vertices { get { return vertices; } }
		public InternalBspFace[] Faces { get { return faces; } }
		public InternalBspShader[] Shaders { get { return shaders; } }
		public InternalBspNode[] Nodes { get { return nodes; } }
		public InternalBspPlane[] Planes { get { return planes; } }
		public InternalBspBrush[] Brushes { get { return brushes; } }
		public InternalBspBrushSide[] BrushSides { get { return brushSides; } }
		public InternalBspVis VisData { get { return visData; } }
		public InternalBspLeaf[] Leaves { get { return leaves; } }
		#endregion

		#region Constructor
		public Quake3Level(BspOptions options)
		{
			this.options = options;
		}
		#endregion

		#region Methods
		/// <summary>
		///		Utility function read the header.
		/// </summary>
		public void Initialize()
		{
			BinaryReader reader = new BinaryReader(chunk);

			header = new InternalBspHeader();
			header.magic = System.Text.Encoding.ASCII.GetChars(reader.ReadBytes(4));
			header.version = reader.ReadInt32();
			header.lumps = new InternalBspLump[(int) Quake3LumpType.NumLumps];
			
			for(int i = 0; i < (int) Quake3LumpType.NumLumps; i++)
			{
				header.lumps[i] = new InternalBspLump();
				header.lumps[i].offset = reader.ReadInt32();
				header.lumps[i].size = reader.ReadInt32();
			}

			ReadEntities(header.lumps[(int) Quake3LumpType.Entities], reader);
			ReadElements(header.lumps[(int) Quake3LumpType.Elements], reader);
			ReadFaces(header.lumps[(int) Quake3LumpType.Faces], reader);
			ReadLeafFaces(header.lumps[(int) Quake3LumpType.LeafFaces], reader);
			ReadLeaves(header.lumps[(int) Quake3LumpType.Leaves], reader);
			ReadModels(header.lumps[(int) Quake3LumpType.Models], reader);
			ReadNodes(header.lumps[(int) Quake3LumpType.Nodes], reader);
			ReadPlanes(header.lumps[(int) Quake3LumpType.Planes], reader);
			ReadShaders(header.lumps[(int) Quake3LumpType.Shaders], reader);
			ReadVisData(header.lumps[(int) Quake3LumpType.Visibility], reader);
			ReadVertices(header.lumps[(int) Quake3LumpType.Vertices], reader);
			ReadLeafBrushes(header.lumps[(int) Quake3LumpType.LeafBrushes], reader);
			ReadBrushes(header.lumps[(int) Quake3LumpType.Brushes], reader);
			ReadBrushSides(header.lumps[(int) Quake3LumpType.BrushSides], reader);
		}

		/// <summary>
		///		Reads Quake3 bsp data from a chunk of memory as read from the file.
		///	</summary>
		///	<remarks>
		///		Since ResourceManagers generally locate data in a variety of
		///		places they typically manipulate them as a chunk of data, rather than
		///		a file pointer since this is unsupported through compressed archives.
		///		<p/>
		///		Quake3 files are made up of a header (which contains version info and
		///		a table of the contents) and 17 'lumps' i.e. sections of data,
		///		the offsets to which are kept in the table of contents. The 17 types
		///		are predefined.
		/// </remarks>
		/// <param name="chunk">Input stream containing Quake3 data.</param>
		public void LoadFromStream(Stream inChunk)
		{
			chunk = inChunk;

			Initialize();
			DumpContents();
		}

		/// <summary>
		///		Extracts the embedded lightmap texture data and loads them as textures.
		/// </summary>
		/// <remarks>
		///		Calling this method makes the lightmap texture data embedded in
		///		the .bsp file available to the renderer. Lightmaps are extracted
		///		and loaded as Texture objects (subclass specific to RenderSystem
		///		subclass) and are named "@lightmap1", "@lightmap2" etc.
		/// </remarks>
		public void ExtractLightmaps()
		{
			chunk.Seek(header.lumps[(int) Quake3LumpType.Lightmaps].offset, SeekOrigin.Begin);
			int numLightmaps = header.lumps[(int) Quake3LumpType.Lightmaps].size / BspLevel.LightmapSize;

			// Lightmaps are always 128x128x24 (RGB).
			for(int i = 0; i < numLightmaps; i++)
			{
				string name = String.Format("@lightmap{0}", i);
				byte[] buffer = new byte[BspLevel.LightmapSize];
				chunk.Read(buffer, 0, BspLevel.LightmapSize);

				// Load, no mipmaps, brighten by factor 4
				// Set gamma explicitly, OpenGL doesn't apply it
				// CHECK: Make OpenGL apply gamma at LoadImage
				Image.ApplyGamma(buffer, 4, buffer.Length, 24);
				MemoryStream stream = new MemoryStream(buffer);		
				Image img = Image.FromRawStream(stream, 128, 128, PixelFormat.R8G8B8);
				TextureManager.Instance.LoadImage(name, img, TextureType.TwoD, -1, 1, 1);				
			}
		}

		/// <summary>
		///		Debug method.
		/// </summary>
		public void DumpContents()
		{
			LogManager.Instance.Write("Quake3 level statistics");
			LogManager.Instance.Write("-----------------------");
			LogManager.Instance.Write("Entities		: " + entities.Length.ToString());
			LogManager.Instance.Write("Faces			: " + faces.Length.ToString());
			LogManager.Instance.Write("Leaf Faces		: " + leafFaces.Length.ToString());
			LogManager.Instance.Write("Leaves			: " + leaves.Length.ToString());
			LogManager.Instance.Write("Lightmaps		: " + header.lumps[(int) Quake3LumpType.Lightmaps].size / BspLevel.LightmapSize);
			LogManager.Instance.Write("Elements		: " + elements.Length.ToString());
			LogManager.Instance.Write("Models			: " + models.Length.ToString());
			LogManager.Instance.Write("Nodes			: " + nodes.Length.ToString());
			LogManager.Instance.Write("Planes			: " + planes.Length.ToString());
			LogManager.Instance.Write("Shaders		: " + shaders.Length.ToString());
			LogManager.Instance.Write("Vertices		: " + vertices.Length.ToString());
			LogManager.Instance.Write("Vis Clusters	: " + visData.clusterCount.ToString());
			LogManager.Instance.Write("");
			LogManager.Instance.Write("-= Shaders =-");

			for(int i = 0; i < shaders.Length; i++)
				LogManager.Instance.Write(String.Format("Shader {0}: {1:x}", i, shaders[i].name));

			LogManager.Instance.Write("");
			LogManager.Instance.Write("-= Entities =-");

			string[] ents = entities.Split('\0');

			for(int i = 0; i < ents.Length; i++)
				LogManager.Instance.Write(ents[i]);
		}

		private void ReadEntities(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			entities = Encoding.ASCII.GetString(reader.ReadBytes(lump.size));
		}

		private void ReadElements(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			elements = new int[lump.size / Marshal.SizeOf(typeof(int))];

			for(int i = 0; i < elements.Length; i++)
				elements[i] = reader.ReadInt32();
		}

		private void ReadFaces(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			faces = new InternalBspFace[lump.size / Marshal.SizeOf(typeof(InternalBspFace))];

			for(int i = 0; i < faces.Length; i++)
			{

				faces[i] = new InternalBspFace();
				faces[i].shader = reader.ReadInt32();
				faces[i].unknown = reader.ReadInt32();
				faces[i].type = (BspFaceType) Enum.Parse(typeof(BspFaceType), reader.ReadInt32().ToString());
				faces[i].vertStart = reader.ReadInt32();
				faces[i].vertCount = reader.ReadInt32();
				faces[i].elemStart = reader.ReadInt32();
				faces[i].elemCount = reader.ReadInt32();
				faces[i].lmTexture = reader.ReadInt32();

				faces[i].lmOffset = new int[] { reader.ReadInt32(), reader.ReadInt32() };
				faces[i].lmSize = new int[] { reader.ReadInt32(), reader.ReadInt32() };
				faces[i].org = new float[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
				
				faces[i].bbox = new float[6];

				for(int j = 0; j < faces[i].bbox.Length; j++)
					faces[i].bbox[j] = reader.ReadSingle();

				faces[i].normal = new float[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
				faces[i].meshCtrl = new int[] { reader.ReadInt32(), reader.ReadInt32() };

				TransformBoundingBox(faces[i].bbox);
				TransformVector(faces[i].org);
				TransformVector(faces[i].normal, true);
			}
		}

		private void ReadLeafFaces(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			leafFaces = new int[lump.size / Marshal.SizeOf(typeof(int))];

			for(int i = 0; i < leafFaces.Length; i++)
				leafFaces[i] = reader.ReadInt32();
		}

		private void ReadLeaves(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			leaves = new InternalBspLeaf[lump.size / Marshal.SizeOf(typeof(InternalBspLeaf))];

			for(int i = 0; i < leaves.Length; i++)
			{
				leaves[i] = new InternalBspLeaf();
				leaves[i].cluster = reader.ReadInt32();
				leaves[i].area = reader.ReadInt32();
				
				leaves[i].bbox = new int[6];

				for(int j = 0; j < leaves[i].bbox.Length; j++)
					leaves[i].bbox[j] = reader.ReadInt32();

				leaves[i].faceStart = reader.ReadInt32();
				leaves[i].faceCount = reader.ReadInt32();
				leaves[i].brushStart = reader.ReadInt32();
				leaves[i].brushCount = reader.ReadInt32();

				TransformBoundingBox(leaves[i].bbox);
			}
		}

		private void ReadModels(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			models = new InternalBspModel[lump.size / Marshal.SizeOf(typeof(InternalBspModel))];

			for(int i = 0; i < models.Length; i++)
			{
				models[i] = new InternalBspModel();
				models[i].bbox = new float[6];

				for(int j = 0; j < models[i].bbox.Length; j++)
					models[i].bbox[j] = reader.ReadSingle();

				models[i].faceStart = reader.ReadInt32();
				models[i].faceCount = reader.ReadInt32();
				models[i].brushStart = reader.ReadInt32();
				models[i].brushCount = reader.ReadInt32();

				TransformBoundingBox(models[i].bbox);
			}
		}

		private void ReadNodes(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			nodes = new InternalBspNode[lump.size / Marshal.SizeOf(typeof(InternalBspNode))];

			for(int i = 0; i < nodes.Length; i++)
			{
				nodes[i] = new InternalBspNode();
				nodes[i].plane = reader.ReadInt32();
				nodes[i].front = reader.ReadInt32();
				nodes[i].back = reader.ReadInt32();
				nodes[i].bbox = new int[6];

				for(int j = 0; j < nodes[i].bbox.Length; j++)
					nodes[i].bbox[j] = reader.ReadInt32();

				TransformBoundingBox(nodes[i].bbox);
			}
		}

		private void ReadPlanes(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			planes = new InternalBspPlane[lump.size / Marshal.SizeOf(typeof(InternalBspPlane))];

			for(int i = 0; i < planes.Length; i++)
			{
				planes[i] = new InternalBspPlane();
				planes[i].normal = new float[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
				planes[i].distance = reader.ReadSingle();

				TransformPlane(planes[i].normal, ref planes[i].distance);
			}
		}

		private void ReadShaders(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			shaders = new InternalBspShader[lump.size / Marshal.SizeOf(typeof(InternalBspShader))];

			for(int i = 0; i < shaders.Length; i++)
			{
				char[] name = Encoding.ASCII.GetChars(reader.ReadBytes(64));

				shaders[i] = new InternalBspShader();
				shaders[i].surfaceFlags = (SurfaceFlags) Enum.Parse(typeof(SurfaceFlags), reader.ReadInt32().ToString());
				shaders[i].contentFlags = (ContentFlags) Enum.Parse(typeof(ContentFlags), reader.ReadInt32().ToString());

				foreach(char c in name)
				{
					if(c == '\0')
						break;

					shaders[i].name += c;
				}
			}
		}

		private void ReadVisData(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			
			visData = new InternalBspVis();
			visData.clusterCount = reader.ReadInt32();
			visData.rowSize = reader.ReadInt32();
			visData.data = reader.ReadBytes(lump.offset - (Marshal.SizeOf(typeof(int)) * 2));
		}

		private void ReadVertices(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			vertices = new InternalBspVertex[lump.size / Marshal.SizeOf(typeof(InternalBspVertex))];

			for(int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = new InternalBspVertex();
				vertices[i].point = new float[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
				vertices[i].texture = new float[] { reader.ReadSingle(), reader.ReadSingle() };
				vertices[i].lightMap = new float[] { reader.ReadSingle(), reader.ReadSingle() };
				vertices[i].normal = new float[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
				vertices[i].color = reader.ReadInt32();

				TransformVector(vertices[i].point);
				TransformVector(vertices[i].normal, true);
			}
		}

		private void ReadLeafBrushes(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			leafBrushes = new int[lump.size / Marshal.SizeOf(typeof(int))];

			for(int i = 0; i < leafBrushes.Length; i++)
				leafBrushes[i] = reader.ReadInt32();
		}

		private void ReadBrushes(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			brushes = new InternalBspBrush[lump.size / Marshal.SizeOf(typeof(InternalBspBrush))];

			for(int i = 0; i < brushes.Length; i++)
			{
				brushes[i] = new InternalBspBrush();
				brushes[i].firstSide = reader.ReadInt32();
				brushes[i].numSides = reader.ReadInt32();
				brushes[i].shaderIndex = reader.ReadInt32();
			}
		}

		private void ReadBrushSides(InternalBspLump lump, BinaryReader reader)
		{
			reader.BaseStream.Seek(lump.offset, SeekOrigin.Begin);
			brushSides = new InternalBspBrushSide[lump.size / Marshal.SizeOf(typeof(InternalBspBrushSide))];

			for(int i = 0; i < brushSides.Length; i++)
			{
				brushSides[i] = new InternalBspBrushSide();
				brushSides[i].planeNum = reader.ReadInt32();
				brushSides[i].content = reader.ReadInt32();
			}
		}

		internal void TransformVector(float[] v, bool isNormal, int pos)
		{
			if (options.setYAxisUp)
			{
				Swap(ref v[pos + 1], ref v[pos + 2]);
				v[pos + 2] = -v[pos + 2];
			}
            
			if (!isNormal)
			{
				for (int i = pos; i < pos + 3; i++)
					v[i] *= options.scale;

				Vector3 move = options.move;
				v[pos] += options.move.x;
				v[pos + 1] += options.move.y;
				v[pos + 2] += options.move.z;
			}
		}

		internal void TransformVector(float[] v, bool isNormal)
		{
			TransformVector(v, isNormal, 0);
		}

		internal void TransformVector(float[] v, int pos)
		{
			TransformVector(v, false, pos);
		}

		internal void TransformVector(float[] v)
		{
			TransformVector(v, false, 0);
		}

		internal void TransformPlane(float[] norm, ref float dist)
		{
			TransformVector(norm, true);
			dist *= options.scale;
			Vector3 normal = new Vector3(norm[0], norm[1], norm[2]);
			Vector3 point = normal * dist;
            point += options.move;
			dist = normal.Dot(point);
		}

		internal void TransformBoundingBox(float[] bb)
		{
			TransformVector(bb, 0);
			TransformVector(bb, 3);
			if (options.setYAxisUp)
				Swap(ref bb[2], ref bb[5]);
		}

		internal void TransformBoundingBox(int[] bb)
		{
			float[] floatbb = new float[6];
			for (int i = 0; i < 6; i++)
				floatbb[i] = (float)bb[i];

			TransformBoundingBox(floatbb);

			for (int i = 0; i < 6; i++)
				bb[i] = Convert.ToInt32(floatbb[i]);
		}

		private void Swap(ref float num1, ref float num2)
		{
			float tmp = num1;
			num1 = num2;
			num2 = tmp;
		}

		private void Swap(ref int num1, ref int num2)
		{
			int tmp = num1;
			num1 = num2;
			num2 = tmp;
		}
		#endregion
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InternalBspPlane
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
		public float[] normal;
		public float distance;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InternalBspModel
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
		public float[] bbox;
		public int faceStart;
		public int faceCount;
		public int brushStart;
		public int brushCount;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InternalBspNode
	{
		public int plane;          // dividing plane
		//int children[2];    // left and right nodes,
		// negative are leaves
		public int front;
		public int back;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
		public int[] bbox;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InternalBspLeaf
	{
		public int cluster;    // visibility cluster number
		public int area;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
		public int[] bbox;
		public int faceStart;
		public int faceCount;
		public int brushStart;
		public int brushCount;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InternalBspFace
	{
		public int shader;         // shader ref
		public int unknown;
		public BspFaceType type;           // face type
		public int vertStart;
		public int vertCount;
		public int elemStart;
		public int elemCount;
		public int lmTexture;     // lightmap
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
		public int[] lmOffset;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
		public int[] lmSize;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
		public float[] org;       // facetype_normal only
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
		public float[] bbox;      // facetype_patch only
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
		public float[] normal;    // facetype_normal only
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
		public int[] meshCtrl;     // patch control point dims
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct InternalBspShader
	{
		[FieldOffset(0)]
		[MarshalAs(UnmanagedType.LPStr)]
		public string name;
		[FieldOffset(64)]
		public SurfaceFlags surfaceFlags;
		[FieldOffset(68)]
		public ContentFlags contentFlags;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InternalBspVertex
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
		public float[] point;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
		public float[] texture;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
		public float[] lightMap;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
		public float[] normal;
		public int color;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InternalBspVis
	{
		public int clusterCount;
		public int rowSize;
		public byte[] data;
	}

	
	[StructLayout(LayoutKind.Sequential)]
	public struct InternalBspLump
	{
		public int offset;
		public int size;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InternalBspHeader
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
		public char[] magic;
		public int version;
		
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=17)]
		public InternalBspLump[] lumps;
	}


	[StructLayout(LayoutKind.Sequential)]
	public struct InternalBspBrushSide
	{
		public int planeNum;
		public int content;			// ¿?shader¿?
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InternalBspBrush
	{
		public int firstSide;
		public int numSides;
		public int shaderIndex;
	}

	[Flags]
	public enum ContentFlags : uint
	{
		/// <summary>An eye is never valid in a solid.</summary>
		Solid = 1,
		Lava = 8,
		Slime = 16,
		Water = 32,
		Fog = 64,
		
		AreaPortal = 0x8000,
		PlayerClip = 0x10000,
		MonsterClip = 0x20000,
			
		/// <summary>Bot specific.</summary>
		Teleporter = 0x40000,
		/// <summary>Bot specific.</summary>
		JumpPad = 0x80000,
		/// <summary>Bot specific.</summary>
		ClusterPortal = 0x100000,
		/// <summary>Bot specific.</summary>
		DoNotEnter = 0x200000,

		/// <summary>Removed before bsping an entity.</summary>
		Origin = 0x1000000,

		/// <summary>Should never be on a brush, only in game.</summary>
		Body = 0x2000000,
		Corpse = 0x4000000,
		/// <summary>Brushes not used for the bsp.</summary>
		Detail = 0x8000000,
		/// <summary>Brushes used for the bsp.</summary>
		Structural = 0x10000000,
		/// <summary>Don't consume surface fragments inside.</summary>
		Translucent = 0x20000000,
		Trigger = 0x40000000,
		/// <summary>Don't leave bodies or items (death fog, lava).</summary>
		NoDrop = 0x80000000
	}

	[Flags]
	public enum SurfaceFlags
	{
		/// <summary>Never give falling damage.</summary>
		NoDamage = 0x1,
		/// <summary>Effects game physics.</summary>
		Slick = 0x2,
		/// <summary>Lighting from environment map.</summary>
		Sky = 0x4,
		Ladder = 0x8,
		/// <summary>Don't make missile explosions.</summary>
		NoImpact = 0x10,
		/// <summary>Don't leave missile marks.</summary>
		NoMarks = 0x20,
		/// <summary>Make flesh sounds and effects.</summary>
		Flesh = 0x40,
		/// <summary>Don't generate a drawsurface at all.</summary>
		NoDraw = 0x80,
		/// <summary>Make a primary bsp splitter.</summary>
		Hint = 0x100,
		/// <summary>Completely ignore, allowing non-closed brushes.</summary>
		Skip = 0x200,
		/// <summary>Surface doesn't need a lightmap.</summary>
		NoLightmap = 0x400,
		/// <summary>Generate lighting info at vertexes.</summary>
		PointLight = 0x800,
		/// <summary>Clanking footsteps.</summary>
		MetalSteps = 0x1000,
		/// <summary>No footstep sounds.</summary>
		NoSteps = 0x2000,
		/// <summary>Don't collide against curves with this set.</summary>
		NonSolid = 0x4000,
		/// <summary>Act as a light filter during q3map -light.</summary>
		LightFilter = 0x8000,
		/// <summary>Do per-pixel light shadow casting in q3map.</summary>
		AlphaShadow = 0x10000,
		/// <summary>Don't dlight even if solid (solid lava, skies).</summary>
		NoDLight = 0x20000
	}

	public enum BspFaceType
	{
		Normal = 1,
		Patch = 2,
		Mesh = 3,
		Flare = 4
	}
}