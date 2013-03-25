/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

using System;
using System.IO;
using System.Diagnostics;
using Axiom.MathLib;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Utility;
using Multiverse;
using Multiverse.CollisionLib;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace Axiom.SceneManagers.Multiverse
{
	/// <summary>
	/// Summary description for Tree.
	/// </summary>
	public class TreeGroup : IDisposable
	{
		private RenderOperation branchRenderOp;
		private RenderOperation frondRenderOp;
		private RenderOperation leaf0RenderOp;
		private RenderOperation leaf1RenderOp;

        private VertexData[] branchVertexBuffers;
        private IndexData[] branchIndexBuffers;

		private VertexData[] frondVertexBuffers;
		private IndexData[] frondIndexBuffers;

		private VertexData[] leafVertexBuffers;
		private IndexData[] leafIndexBuffers;

		private SpeedTreeWrapper speedTree;
		private TreeTextures treeTextures;
		private TreeGeometry geometry;
		
		private SpeedWindWrapper speedWind;

        private Material branchMaterial;
		private Material frondMaterial;
		private Material leafMaterial;

		private static int treeNum = 0;

		private static String [] windMatrixConstantNames;
		private static String [] leafAngleMatrixConstantNames;
		private static String [] leafClusterConstantNames;

        // axis aligned bounding box of the area covered by all the trees in the group
        private AxisAlignedBox bounds;

        // radius of the sphere the contains bounds.  used for quick distance checks
        protected float boundingRadius;

        private Forest forest;

        private bool disposed = false;

        protected bool visible = false;

        private static bool initialized = false;
        private static bool normalMapped = false;
        private static int branchTechnique = 1;

        protected static VertexDeclaration indexedVertexDeclaration;
        protected static VertexDeclaration indexedNormalMapVertexDeclaration;
        protected static VertexDeclaration leafVertexDeclaration;
        protected static VertexDeclaration billboardVertexDeclaration;

        protected List<Tree> trees = new List<Tree>();

        protected GpuProgramParameters positionParameters = new GpuProgramParameters();

        private static List<TreeGroup> allGroups;

        private static readonly float nearLOD = 50000.0f;
        private static readonly float farLOD = 300000.0f;

        private static bool billboardsDirty = false;

        protected string name;

        Dictionary<int, List<Tree>>[] branchBuckets;
        Dictionary<int, List<Tree>>[] frondBuckets;
        Dictionary<int, List<Tree>>[] leafBuckets;
        Dictionary<int, List<Tree>>[] billboardBucket;

        protected List<Tree> visibleBranches = new List<Tree>();
        protected List<Tree> visibleFronds = new List<Tree>();
        protected List<Tree> visibleLeaves = new List<Tree>();
        protected List<Tree> visibleBillboards = new List<Tree>();

        #region Static Methods and Properties
        private static void Initialize()
        {
            allGroups = new List<TreeGroup>();

            Material material = MaterialManager.Instance.GetByName("SpeedTree/Branch");
            material.Load();

            // if the shader technique is supported, then use shaders
            normalMapped = material.GetTechnique(0).IsSupported;
            if (normalMapped)
            {
                branchTechnique = 0;
            }
            initialized = true;

            InitGPUConstantNames();

            BuildVertexDeclarations();
        }

        private static void BuildVertexDeclarations()
        {
            int vDecOffset;

            //
            // Set up vertex declaration for fronds or branches that don't use normal mapping
            //
            indexedVertexDeclaration = HardwareBufferManager.Instance.CreateVertexDeclaration();
            // set up the vertex declaration
            vDecOffset = 0;
            indexedVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            indexedVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            // xy are normal tex coords
            // zw are self shadowed tex coords
            indexedVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float4, VertexElementSemantic.TexCoords, 0);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float4);

            // wind params
            indexedVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 1);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

            //
            // Set up vertex declaration for fronds or branches that use normal mapping
            //
            indexedNormalMapVertexDeclaration = HardwareBufferManager.Instance.CreateVertexDeclaration();
            // set up the vertex declaration
            vDecOffset = 0;
            indexedNormalMapVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            indexedNormalMapVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            // xy are normal tex coords
            // zw are self shadowed tex coords
            indexedNormalMapVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float4, VertexElementSemantic.TexCoords, 0);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float4);

            // wind params
            indexedNormalMapVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 1);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

            // binormal
            indexedNormalMapVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.TexCoords, 2);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            // tangent
            indexedNormalMapVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.TexCoords, 3);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            //
            // Set up vertex declaration for leaves
            //
            leafVertexDeclaration = HardwareBufferManager.Instance.CreateVertexDeclaration();
            // set up the vertex declaration
            vDecOffset = 0;

            leafVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            leafVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            leafVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 0);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

            leafVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 1);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

            leafVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.TexCoords, 2);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            //
            // Set up vertex declaration for billboards
            //
            billboardVertexDeclaration = HardwareBufferManager.Instance.CreateVertexDeclaration();
            // set up the vertex declaration
            vDecOffset = 0;

            billboardVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            billboardVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

        }

        private static void InitGPUConstantNames()
        {
            windMatrixConstantNames = new String[4];
            leafAngleMatrixConstantNames = new String[4];
            leafClusterConstantNames = new String[48];

            for (int i = 0; i < 4; i++)
            {
                windMatrixConstantNames[i] = String.Format("g_amWindMatrices[{0}]", i);
                leafAngleMatrixConstantNames[i] = String.Format("g_amLeafAngleMatrices[{0}]", i);
            }

            for (int i = 0; i < 48; i++)
            {
                leafClusterConstantNames[i] = String.Format("g_avLeafClusters[{0}]", i);
            }
        }

        public static VertexDeclaration IndexedVertexDeclaration
        {
            get
            {
                return indexedVertexDeclaration;
            }
        }

        public static VertexDeclaration IndexedNormalMapVertexDeclaration
        {
            get
            {
                return indexedNormalMapVertexDeclaration;
            }
        }

        public static VertexDeclaration LeafVertexDeclaration
        {
            get
            {
                return leafVertexDeclaration;
            }
        }

        public static VertexDeclaration BillboardVertexDeclaration
        {
            get
            {
                return billboardVertexDeclaration;
            }
        }

        public static List<TreeGroup> AllGroups
        {
            get
            {
                return allGroups;
            }
        }

        #endregion Static Methods and Properties


		public TreeGroup(String filename, float size, float sizeVariance, SpeedWindWrapper speedWind, Forest forest, List<Vector3> locations)
		{
            if (!initialized)
            {
                Initialize();
            }

            name = String.Format("Forest: {0} File: {1} Instances: {2}", forest.Name, filename, locations.Count);
            this.forest = forest;

			this.speedWind = speedWind;

			speedTree = new SpeedTreeWrapper();

            speedTree.TextureFlip = true;
			LoadTree(filename);
			speedTree.BranchWindMethod = WindMethod.WindGPU;
			speedTree.FrondWindMethod = WindMethod.WindGPU;
			speedTree.LeafWindMethod = WindMethod.WindGPU;

            float originalSize = 0f;
            float variance = 0f;
            speedTree.GetTreeSize(ref originalSize, ref variance);
            speedTree.OriginalSize = originalSize;
			speedTree.SetTreeSize(size, sizeVariance);
			
			treeTextures = speedTree.Textures;

            // make sure the tree doesn't have too many leaf texture groups
            Debug.Assert(treeTextures.LeafTextureFilenames.Length <= 3);

            // for trees with 3 leaf textures, reduce the number of rocking groups to 2
            // (from the default of 3), so that we don't overflow the number of available shader
            // param registers
            if (treeTextures.LeafTextureFilenames.Length == 3)
            {
                speedTree.NumLeafRockingGroups = 2;
            }

			speedTree.Compute(SpeedTreeUtil.ToSpeedTree(Matrix4.Identity), 1, true);

			speedTree.TreePosition = SpeedTreeUtil.ToSpeedTree(Vector3.Zero);

			// set lod limits
			speedTree.SetLodLimits(nearLOD, farLOD);

			// create the geometry object
			geometry = new TreeGeometry();

			//
			// Setup branches
			//

			// create the render operation
			branchRenderOp = new RenderOperation();
			branchRenderOp.operationType = OperationType.TriangleStrip;
			branchRenderOp.useIndices = true;

			// set up the material.
            branchMaterial = SetupTreeMaterial("SpeedTree/Branch", treeTextures.BranchTextureFilename, treeTextures.SelfShadowFilename, GenerateNormalMapTextureName(treeTextures.BranchTextureFilename), speedTree.BranchMaterial, !normalMapped, branchTechnique);

            // get number of branch LODs
            uint nBranchLODs = speedTree.NumBranchLodLevels;

            // allocate branch buffers
            branchVertexBuffers = new VertexData[nBranchLODs];
            branchIndexBuffers = new IndexData[nBranchLODs];

            for (short i = 0; i < nBranchLODs; i++)
            {
                // set up the vertex and index buffers for the branch geometry
                speedTree.GetGeometry(geometry, SpeedTreeWrapper.GeometryFlags.BranchGeometry, i, -1, -1);
                BuildIndexedBuffer(geometry.Branches, true, out branchVertexBuffers[i], out branchIndexBuffers[i]);
            }
			//
			// Setup fronds
			//

			// create the render operation
			frondRenderOp = new RenderOperation();
			frondRenderOp.operationType = OperationType.TriangleStrip;
			frondRenderOp.useIndices = true;

			// set up the material
			frondMaterial = SetupTreeMaterial("SpeedTree/Frond", treeTextures.CompositeFilename, treeTextures.SelfShadowFilename, null, speedTree.FrondMaterial, true, 0);

			uint nFrondLODs = speedTree.NumFrondLodLevels;

			// allocate frond buffer arrays
			frondVertexBuffers = new VertexData[nFrondLODs];
			frondIndexBuffers = new IndexData[nFrondLODs];

			for ( short i = 0; i < nFrondLODs; i++ ) 
			{
				// build the frond geometry for each LOD
				speedTree.GetGeometry(geometry, SpeedTreeWrapper.GeometryFlags.FrondGeometry, -1, i, -1);
				BuildIndexedBuffer(geometry.Fronds, false, out frondVertexBuffers[i], out frondIndexBuffers[i]);
			}

			//
			// Setup Leaves
			//

            TreeCamera saveCam = SpeedTreeWrapper.Camera;

			TreeCamera treeCamera = new TreeCamera();
			treeCamera.position = SpeedTreeUtil.ToSpeedTree(Vector3.Zero);
			treeCamera.direction = SpeedTreeUtil.ToSpeedTree(new Vector3(1,0,0));
			SpeedTreeWrapper.Camera = treeCamera;

			// set up render ops
			leaf0RenderOp = new RenderOperation();
			leaf0RenderOp.operationType = OperationType.TriangleList;
			leaf0RenderOp.useIndices = true;

			leaf1RenderOp = new RenderOperation();
			leaf1RenderOp.operationType = OperationType.TriangleList;
			leaf1RenderOp.useIndices = true;

			// set up the material
			leafMaterial = SetupTreeMaterial("SpeedTree/Leaf", treeTextures.CompositeFilename, null, null, speedTree.LeafMaterial, true, 0);

			uint nLeafLODs = speedTree.NumLeafLodLevels;

			// allocate leaf buffer arrays
			leafVertexBuffers = new VertexData[nLeafLODs];
			leafIndexBuffers = new IndexData[nLeafLODs];

			float [] lodLeafAdjust = speedTree.LeafLodSizeAdjustments;

			for ( short i = 0; i < nLeafLODs; i++ ) 
			{
				// build the leaf geometry for each LOD
				speedTree.GetGeometry(geometry, SpeedTreeWrapper.GeometryFlags.LeafGeometry, -1, -1, i);
				BuildLeafBuffer(geometry.Leaves0, lodLeafAdjust[i], out leafVertexBuffers[i], out leafIndexBuffers[i]);
			}

            // restore the camera afte getting leaf buffers
            SpeedTreeWrapper.Camera = saveCam;

            bounds = new AxisAlignedBox();

            // build all the trees and accumulate bounds
            foreach (Vector3 loc in locations)
            {
                SpeedTreeWrapper treeInstance = speedTree.MakeInstance();
                treeInstance.OriginalSize = originalSize;
                Tree t = new Tree(this, treeInstance, loc);

                bounds.Merge(t.Bounds);

                trees.Add(t);
            }

            boundingRadius = (bounds.Maximum - bounds.Minimum).Length / 2;

            // create the buckets
            branchBuckets = new Dictionary<int, List<Tree>>[nBranchLODs];
            frondBuckets = new Dictionary<int, List<Tree>>[nFrondLODs];
            leafBuckets = new Dictionary<int, List<Tree>>[nLeafLODs];
            billboardBucket = new Dictionary<int, List<Tree>>[1];

            // initialize the bucket dictionaries
            for (int i = 0; i < nBranchLODs; i++)
            {
                branchBuckets[i] = new Dictionary<int, List<Tree>>();
            }
            for (int i = 0; i < nFrondLODs; i++)
            {
                frondBuckets[i] = new Dictionary<int, List<Tree>>();
            }
            for (int i = 0; i < nLeafLODs; i++)
            {
                leafBuckets[i] = new Dictionary<int, List<Tree>>();
            }
            billboardBucket[0] = new Dictionary<int, List<Tree>>();
            allGroups.Add(this);
        }

        #region Tree Loading Methods
        static string[] freeTrees = { 
            "americanboxwoodcluster_rt",
            "americanboxwood_rt",
            "azaleapatch_rt",
            "azaleapatch_rt_pink",
            "azalea_rt",
            "azalea_rt_pink",
            "beech_rt",
            "beech_rt_fall",
            "beech_rt_winter",
            "curlypalmcluster_rt",
            "curlypalm_rt",
            "fraserfircluster_rt",
            "fraserfircluster_rt_snow",
            "fraserfir_rt",
            "fraserfir_rt_snow",
            "rdapple_rt",
            "rdapple_rt_apples",
            "rdapple_rt_spring",
            "rdapple_rt_winter",
            "sugarpine_rt",
            "sugarpine_rt_winter",
            "umbrellathorn_rt",
            "umbrellathorn_rt_dead",
            "umbrellathorn_rt_flowers",
            "venustree_rt",
            "weepingwillow_rt",
            "weepingwillow_rt_fall",
            "weepingwillow_rt_winter"
        };

        private void GenKey(string baseName, out byte[] desKey, out byte[] desIV)
        {
            byte[] secretBytes = { 6, 29, 66, 6, 2, 68, 4, 7, 70 };

            byte[] baseNameBytes = new System.Text.ASCIIEncoding().GetBytes(baseName);

            byte[] hashBytes = new byte[secretBytes.Length + baseNameBytes.Length];

            // copy secret byte to start of hash array
            for (int i = 0; i < secretBytes.Length; i++)
            {
                hashBytes[i] = secretBytes[i];
            }

            // copy filename byte to end of hash array
            for (int i = 0; i < baseNameBytes.Length; i++)
            {
                hashBytes[i + secretBytes.Length] = baseNameBytes[i];
            }

            SHA1Managed sha = new SHA1Managed();

            // run the sha1 hash
            byte[] hashResult = sha.ComputeHash(hashBytes);

            desKey = new byte[8];
            desIV = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                desKey[i] = hashResult[i];
                desIV[i] = hashResult[8 + i];
            }
        }

        private void LoadTree(String filename)
        {
            bool found = false;

            int extOffset = filename.LastIndexOf('.');
            string baseName = filename.Substring(0, extOffset);
            
            foreach (string treeName in freeTrees)
            {
                if (baseName.ToLower() == treeName)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                filename = "FraserFir_RT.tre";
            }

            int len;
            byte[] buffer;
            Stream s;
            bool encrypted = true;

            // try the encrypted file first, and if it isn't there, then try the
            // unencrypted one.
            try
            {
                s = ResourceManager.FindCommonResourceData(string.Format("{0}.tre", baseName));
            }
            catch ( AxiomException )
            {
                encrypted = false;
                s = ResourceManager.FindCommonResourceData(filename);
            }

            len = (int)s.Length;
            buffer = new byte[len];

            if (encrypted)
            {
                byte[] desKey;
                byte[] desIV;

                GenKey(baseName, out desKey, out desIV);

                DES des = new DESCryptoServiceProvider();
                CryptoStream encStream = new CryptoStream(s, des.CreateDecryptor(desKey, desIV), CryptoStreamMode.Read);

                len = encStream.Read(buffer, 0, len);

                encStream.Close();
                s.Close();
            }
            else
            {
                s.Read(buffer, 0, len);
                s.Close();
            }

            speedTree.LoadTree(buffer, (uint)len);
        }

        #endregion Tree Loading Methods

        private string ConvertTextureName(string texName)
        {
            if (texName.EndsWith(".tga") || texName.EndsWith(".TGA"))
            {
                texName = texName.Substring(0, texName.Length - 4) + ".dds";
            }

            return texName;
        }

        private string GenerateNormalMapTextureName(string texName)
        {
            int dotindex = texName.LastIndexOf('.');
            string ret;

            // if the texture name is blank.dds, then we aren't actually going to draw with
            // this material.  Return the same texture name, so that we don't fail out looking
            // for "blankNormal.dds"
            string lowerName = texName.Substring(0,dotindex).ToLower();
            if ( lowerName == "blank")
            {
                ret = texName;
            }
            else
            {
                ret = string.Format("{0}Normals{1}", texName.Substring(0, dotindex), texName.Substring(dotindex));
            }
            return ret;
        }

		private Material SetupTreeMaterial(String baseMaterialName, String texture1, String texture2, String texture3, TreeMaterial treeMaterial, bool setupGPUParams, int tech )
		{
			Material baseMaterial = MaterialManager.Instance.GetByName(baseMaterialName);
			String newMatName = String.Format("{0}/{1}", baseMaterialName, treeNum++);
			
			Material material = baseMaterial.Clone(newMatName);

			if ( setupGPUParams ) 
			{
				// material lighting properties
				material.GetTechnique(tech).GetPass(0).VertexProgramParameters.SetNamedConstant( "g_vMaterialDiffuse", SpeedTreeUtil.FromSpeedTree(treeMaterial.Diffuse));
                material.GetTechnique(tech).GetPass(0).VertexProgramParameters.SetNamedConstant( "g_vMaterialAmbient", SpeedTreeUtil.FromSpeedTree(treeMaterial.Ambient));
			}

			material.GetTechnique(tech).GetPass(0).GetTextureUnitState(0).SetTextureName(ConvertTextureName(texture1));
            if (texture2 != null)
            {
                // deal with lack of shadow map
                if (texture2 == "")
                {
                    texture2 = "White.dds";
                }
                // self shadow map
                material.GetTechnique(tech).GetPass(0).GetTextureUnitState(1).SetTextureName(ConvertTextureName(texture2));

                if ((texture3 != null) && normalMapped)
                {
                    // normal map
                    material.GetTechnique(tech).GetPass(0).GetTextureUnitState(2).SetTextureName(ConvertTextureName(texture3));

                    // material lighting properties for normal mapping
                    material.GetTechnique(tech).GetPass(0).FragmentProgramParameters.SetNamedConstant("g_vMaterialDiffuse", SpeedTreeUtil.FromSpeedTree(treeMaterial.Diffuse));
                    material.GetTechnique(tech).GetPass(0).FragmentProgramParameters.SetNamedConstant("g_vMaterialAmbient", SpeedTreeUtil.FromSpeedTree(treeMaterial.Ambient));

                }
            }

			material.Load(); 
			material.Lighting = true;

			return material;
		}

		public void BuildIndexedBuffer(TreeGeometry.Indexed indexed, bool normalMapped, out VertexData vertexData, out IndexData indexData)
		{

			//
			// Build the vertex buffer
			//

            if (indexed.VertexCount == 0)
            {
                vertexData = null;
                indexData = null;
            }
            else
            {

                vertexData = new VertexData();

                vertexData.vertexCount = indexed.VertexCount;
                vertexData.vertexStart = 0;

                // free the vertex declaration to avoid a leak.
                HardwareBufferManager.Instance.DestroyVertexDeclaration(vertexData.vertexDeclaration);

                // pick from the common vertex declarations based on whether we are normal mapped or not
                if (normalMapped)
                {
                    vertexData.vertexDeclaration = indexedNormalMapVertexDeclaration;
                }
                else
                {
                    vertexData.vertexDeclaration = indexedVertexDeclaration;
                }

                // create the hardware vertex buffer and set up the buffer binding
                HardwareVertexBuffer hvBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                    vertexData.vertexDeclaration.GetVertexSize(0), vertexData.vertexCount,
                    BufferUsage.StaticWriteOnly, false);

                vertexData.vertexBufferBinding.SetBinding(0, hvBuffer);

                // lock the vertex buffer
                IntPtr ipBuf = hvBuffer.Lock(BufferLocking.Discard);

                int bufferOff = 0;

                unsafe
                {
                    float* buffer = (float*)ipBuf.ToPointer();
                    float* srcPosition = indexed.Coords;
                    float* srcNormal = indexed.Normals;
                    float* srcTexCoord = indexed.TexCoords0;
                    float* srcShadowTexCoord = indexed.TexCoords1;
                    byte* srcWindIndices = indexed.WindMatrixIndices;
                    float* srcWindWeights = indexed.WindWeights;
                    float* srcBinormal = indexed.Binormals;
                    float* srcTangent = indexed.Tangents;
                    

                    for (int v = 0; v < vertexData.vertexCount; v++)
                    {

                        // Position
                        buffer[bufferOff++] = srcPosition[0];
                        buffer[bufferOff++] = srcPosition[1];
                        buffer[bufferOff++] = srcPosition[2];
                        srcPosition += 3;

                        // normals
                        buffer[bufferOff++] = *srcNormal++;
                        buffer[bufferOff++] = *srcNormal++;
                        buffer[bufferOff++] = *srcNormal++;

                        // Texture
                        buffer[bufferOff++] = *srcTexCoord++;
                        buffer[bufferOff++] = *srcTexCoord++;

                        // Self Shadow Texture
                        buffer[bufferOff++] = *srcShadowTexCoord++;
                        buffer[bufferOff++] = *srcShadowTexCoord++;

                        // wind params
                        buffer[bufferOff++] = *srcWindIndices++;
                        buffer[bufferOff++] = *srcWindWeights++;

                        if (normalMapped)
                        {
                            // Binormal
                            buffer[bufferOff++] = *srcBinormal++;
                            buffer[bufferOff++] = *srcBinormal++;
                            buffer[bufferOff++] = *srcBinormal++;

                            // Tangent
                            buffer[bufferOff++] = *srcTangent++;
                            buffer[bufferOff++] = *srcTangent++;
                            buffer[bufferOff++] = *srcTangent++;
                        }
                    }
                }
                hvBuffer.Unlock();

                //
                // build the index buffer
                //
                if (indexed.NumStrips == 0)
                {
                    indexData = null;
                }
                else
                {
                    indexData = new IndexData();
                    int numIndices;

                    unsafe
                    {
                        numIndices = indexed.StripLengths[0];
                    }

                    indexData.indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
                        IndexType.Size16, numIndices, BufferUsage.StaticWriteOnly);

                    IntPtr indexBufferPtr = indexData.indexBuffer.Lock(0, indexData.indexBuffer.Size, BufferLocking.Discard);

                    unsafe
                    {
                        ushort* strip = null;
                        if (numIndices != 0)
                        {
                            strip = indexed.Strips[0];
                        }

                        ushort* indexBuffer = (ushort*)indexBufferPtr.ToPointer();
                        for (int i = 0; i < numIndices; i++)
                        {
                            indexBuffer[i] = strip[i];
                        }
                    }

                    indexData.indexBuffer.Unlock();

                    indexData.indexCount = numIndices;
                    indexData.indexStart = 0;
                }

                return;
            }
		}

		public void BuildLeafBuffer(TreeGeometry.Leaf leaf, float leafAdjust, out VertexData vertexData, out IndexData indexData)
		{

            if (leaf.LeafCount == 0)
            {
                vertexData = null;
                indexData = null;
            }
            else
            {
                //
                // Build the vertex buffer
                //
                vertexData = new VertexData();

                vertexData.vertexCount = leaf.LeafCount * 4;
                vertexData.vertexStart = 0;

                // destroy the original vertex declaration, and use the common one for leaves
                HardwareBufferManager.Instance.DestroyVertexDeclaration(vertexData.vertexDeclaration);
                vertexData.vertexDeclaration = leafVertexDeclaration;

                // create the hardware vertex buffer and set up the buffer binding
                HardwareVertexBuffer hvBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                    vertexData.vertexDeclaration.GetVertexSize(0), vertexData.vertexCount,
                    BufferUsage.StaticWriteOnly, false);

                vertexData.vertexBufferBinding.SetBinding(0, hvBuffer);

                // lock the vertex buffer
                IntPtr ipBuf = hvBuffer.Lock(BufferLocking.Discard);

                int bufferOff = 0;

                int maxindex = 0;

                unsafe
                {
                    float* buffer = (float*)ipBuf.ToPointer();
                    float* srcCenter = leaf.CenterCoords;
                    float* srcNormal = leaf.Normals;
                    byte* srcWindIndices = leaf.WindMatrixIndices;
                    float* srcWindWeights = leaf.WindWeights;
                    byte* srcLeafClusterIndices = leaf.LeafClusterIndices;


                    for (int l = 0; l < leaf.LeafCount; l++)
                    {
                        float* srcOffset = leaf.LeafMapCoords[l];
                        float* srcTexCoord = leaf.LeafMapTexCoords[l];
                        int leafClusterIndex = *srcLeafClusterIndices;

                        //if (leafClusterIndex % 4 == 0)
                        //{
                        //    leafClusterIndex++;
                        //}

                        if (leafClusterIndex > maxindex)
                        {
                            maxindex = leafClusterIndex;
                        }

                        // Position
                        buffer[bufferOff++] = srcCenter[0];
                        buffer[bufferOff++] = srcCenter[1];
                        buffer[bufferOff++] = srcCenter[2];

                        // normals
                        buffer[bufferOff++] = srcNormal[0];
                        buffer[bufferOff++] = srcNormal[1];
                        buffer[bufferOff++] = srcNormal[2];

                        // Texture
                        buffer[bufferOff++] = srcTexCoord[0];
                        buffer[bufferOff++] = srcTexCoord[1];

                        // Wind Attributes
                        buffer[bufferOff++] = *srcWindIndices;
                        buffer[bufferOff++] = *srcWindWeights;

                        // Leaf Placement Data
                        buffer[bufferOff++] = leafClusterIndex * 4;
                        buffer[bufferOff++] = leafAdjust;
                        buffer[bufferOff++] = (float)(leafClusterIndex % speedWind.NumWindMatrices);


                        // Position
                        buffer[bufferOff++] = srcCenter[0];
                        buffer[bufferOff++] = srcCenter[1];
                        buffer[bufferOff++] = srcCenter[2];

                        // normals
                        buffer[bufferOff++] = srcNormal[0];
                        buffer[bufferOff++] = srcNormal[1];
                        buffer[bufferOff++] = srcNormal[2];

                        // Texture
                        buffer[bufferOff++] = srcTexCoord[2];
                        buffer[bufferOff++] = srcTexCoord[3];

                        // Wind Attributes
                        buffer[bufferOff++] = *srcWindIndices;
                        buffer[bufferOff++] = *srcWindWeights;

                        // Leaf Placement Data
                        buffer[bufferOff++] = leafClusterIndex * 4 + 1;
                        buffer[bufferOff++] = leafAdjust;
                        buffer[bufferOff++] = (float)(leafClusterIndex % speedWind.NumWindMatrices);


                        // Position
                        buffer[bufferOff++] = srcCenter[0];
                        buffer[bufferOff++] = srcCenter[1];
                        buffer[bufferOff++] = srcCenter[2];

                        // normals
                        buffer[bufferOff++] = srcNormal[0];
                        buffer[bufferOff++] = srcNormal[1];
                        buffer[bufferOff++] = srcNormal[2];

                        // Texture
                        buffer[bufferOff++] = srcTexCoord[4];
                        buffer[bufferOff++] = srcTexCoord[5];

                        // Wind Attributes
                        buffer[bufferOff++] = *srcWindIndices;
                        buffer[bufferOff++] = *srcWindWeights;

                        // Leaf Placement Data
                        buffer[bufferOff++] = leafClusterIndex * 4 + 2;
                        buffer[bufferOff++] = leafAdjust;
                        buffer[bufferOff++] = (float)(leafClusterIndex % speedWind.NumWindMatrices);


                        // Position
                        buffer[bufferOff++] = srcCenter[0];
                        buffer[bufferOff++] = srcCenter[1];
                        buffer[bufferOff++] = srcCenter[2];

                        // normals
                        buffer[bufferOff++] = srcNormal[0];
                        buffer[bufferOff++] = srcNormal[1];
                        buffer[bufferOff++] = srcNormal[2];

                        // Texture
                        buffer[bufferOff++] = srcTexCoord[6];
                        buffer[bufferOff++] = srcTexCoord[7];

                        // Wind Attributes
                        buffer[bufferOff++] = *srcWindIndices++;
                        buffer[bufferOff++] = *srcWindWeights++;

                        // Leaf Placement Data
                        buffer[bufferOff++] = leafClusterIndex * 4 + 3;
                        buffer[bufferOff++] = leafAdjust;
                        buffer[bufferOff++] = (float)(leafClusterIndex % speedWind.NumWindMatrices);

                        srcCenter += 3;
                        srcNormal += 3;
                        srcLeafClusterIndices++;
                    }
                }
                hvBuffer.Unlock();

                // Turned off this output because it kills performance when editing terrain
//                Console.WriteLine("max leaf cluster index: {0}", maxindex);

                //
                // build the index buffer
                //
                indexData = new IndexData();
                int numIndices;

                numIndices = leaf.LeafCount * 6;

                indexData.indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
                    IndexType.Size16, numIndices, BufferUsage.StaticWriteOnly);

                IntPtr indexBufferPtr = indexData.indexBuffer.Lock(0, indexData.indexBuffer.Size, BufferLocking.Discard);

                unsafe
                {
                    ushort* indexBuffer = (ushort*)indexBufferPtr.ToPointer();
                    int bufOff = 0;
                    int vert = 0;
                    for (int i = 0; i < leaf.LeafCount; i++)
                    {
                        indexBuffer[bufOff++] = (ushort)vert;
                        indexBuffer[bufOff++] = (ushort)(vert + 1);
                        indexBuffer[bufOff++] = (ushort)(vert + 2);

                        indexBuffer[bufOff++] = (ushort)vert;
                        indexBuffer[bufOff++] = (ushort)(vert + 2);
                        indexBuffer[bufOff++] = (ushort)(vert + 3);

                        vert += 4;
                    }
                }

                indexData.indexBuffer.Unlock();
                indexData.indexCount = numIndices;
                indexData.indexStart = 0;
            }

			return;
		}

        public void CameraChange(Camera camera)
        {
            visible = camera.IsObjectVisible(bounds);

            // mark branches, fronds and leaves as not visible.  If any of the trees have them visible,
            //  the tree will set them to true in their own CameraChange() method.
            visibleBranches.Clear();
            visibleFronds.Clear();
            visibleLeaves.Clear();
            visibleBillboards.Clear();

            // force rebuilding of billboards before next render
            billboardsDirty = true;

            if (visible)
            {
                foreach (Tree t in trees)
                {
                    t.CameraChange(camera);
                }
                //SortTrees();
            }
            else
            {
                //ClearBuckets();
            }
        }

        public void AddVisible(Tree t, bool branchesVisible, bool frondsVisible, bool leavesVisible, bool billboardsVisible)
        {
            if (branchesVisible)
            {
                visibleBranches.Add(t);
            }
            if (frondsVisible)
            {
                visibleFronds.Add(t);
            }
            if (leavesVisible)
            {
                visibleLeaves.Add(t);
            }
            if (billboardsVisible)
            {
                visibleBillboards.Add(t);
            }
        }

        protected void AddToBucket(Dictionary<int, List<Tree>>[] buckets, TreeRenderArgs args, Tree t)
        {
            if (args.Active)
            {
                int lod = args.LOD;
                int alpha = args.AlphaTestValue;

                if (!buckets[lod].ContainsKey(alpha))
                {
                    buckets[lod][alpha] = new List<Tree>();
                }
                buckets[lod][alpha].Add(t);
            }
        }

        protected int DumpBuckets(Dictionary<int, List<Tree>>[] buckets, string partName)
        {
            int count = 0;
            LogManager.Instance.Write("Dumping sort buckets for: {0} : {1}", partName, name);
            for (int i = 0; i < buckets.Length; i++)
            {
                LogManager.Instance.Write("  LOD: {0}", i);
                foreach (int alpha in buckets[i].Keys)
                {
                    count += buckets[i][alpha].Count;
                    LogManager.Instance.Write("    Alpha : {0} : {1} instance", alpha, buckets[i][alpha].Count);
                }
            }
            return count;
        }

        static int branchCount;
        static int frondCount;
        static int leafCount;
        static int billboardCount;

        protected void DumpAllBuckets()
        {
            branchCount += DumpBuckets(branchBuckets, "Branches");
            frondCount += DumpBuckets(frondBuckets, "Fronds");
            leafCount += DumpBuckets(leafBuckets, "Leaves");
            billboardCount += DumpBuckets(billboardBucket, "Billboards");
        }

        public static void DumpAllTrees()
        {
            branchCount = 0;
            frondCount = 0;
            leafCount = 0;
            billboardCount = 0;

            foreach (TreeGroup group in allGroups)
            {
                group.DumpAllBuckets();
            }

            LogManager.Instance.Write("BranchCount = {0}, FrondCount = {1}, LeafCount = {2}, billboardCount = {3}", branchCount, frondCount, leafCount, billboardCount);
        }

        protected void ClearBuckets()
        {
            foreach (Dictionary<int, List<Tree>> b in branchBuckets)
            {
                b.Clear();
            }
            foreach (Dictionary<int, List<Tree>> b in frondBuckets)
            {
                b.Clear();
            }
            foreach (Dictionary<int, List<Tree>> b in leafBuckets)
            {
                b.Clear();
            }
            billboardBucket[0].Clear();
        }

        protected void SortTrees()
        {
            foreach (Tree t in trees)
            {
                AddToBucket(branchBuckets, t.BranchRenderArgs, t);
                AddToBucket(frondBuckets, t.FrondRenderArgs, t);
                AddToBucket(leafBuckets, t.Leaf0RenderArgs, t);
                AddToBucket(leafBuckets, t.Leaf1RenderArgs, t);
                AddToBucket(billboardBucket, t.Billboard0RenderArgs, t);
                AddToBucket(billboardBucket, t.Billboard1RenderArgs, t);
            }
        }

        public void UpdateMaterials()
		{
			int numMat = speedWind.NumWindMatrices;
            Debug.Assert(numMat == 4);

            Pass branchPass = branchMaterial.GetTechnique(branchTechnique).GetPass(0);
            Pass frondPass = frondMaterial.GetTechnique(0).GetPass(0);
            Pass leafPass = leafMaterial.GetTechnique(0).GetPass(0);

            // set wind matrices in branch, frond and leaf passes
			for ( uint i = 0; i < numMat; i++ ) 
			{
				Matrix4 mat = SpeedTreeUtil.FromSpeedTree(speedWind.get_WindMatrix(i));

				branchPass.VertexProgramParameters.SetNamedConstant(windMatrixConstantNames[i], mat );
                branchPass.ShadowCasterVertexProgramParameters.SetNamedConstant(windMatrixConstantNames[i], mat);

				frondPass.VertexProgramParameters.SetNamedConstant(windMatrixConstantNames[i], mat );
                frondPass.ShadowCasterVertexProgramParameters.SetNamedConstant(windMatrixConstantNames[i], mat);
				leafPass.VertexProgramParameters.SetNamedConstant(windMatrixConstantNames[i], mat );
                leafPass.ShadowCasterVertexProgramParameters.SetNamedConstant(windMatrixConstantNames[i], mat);
			}

            // set camera direction
            if (!normalMapped)
            {
                branchPass.VertexProgramParameters.SetNamedConstant("g_vCameraDir", TerrainManager.Instance.CameraDirection);
            }
			frondPass.VertexProgramParameters.SetNamedConstant("g_vCameraDir", TerrainManager.Instance.CameraDirection);
			leafPass.VertexProgramParameters.SetNamedConstant("g_vCameraDir", TerrainManager.Instance.CameraDirection);

			// leaf matrices
			speedWind.BuildLeafAngleMatrices(SpeedTreeUtil.ToSpeedTree(TerrainManager.Instance.CameraDirection));
			numMat = speedWind.NumLeafAngles;
            Debug.Assert(numMat == 4);
			for ( uint i = 0; i < numMat; i++ ) 
			{
				Matrix4 mat = SpeedTreeUtil.FromSpeedTree(speedWind.get_LeafAngleMatrix(i));

				leafPass.VertexProgramParameters.SetNamedConstant(leafAngleMatrixConstantNames[i], mat );
                leafPass.ShadowCasterVertexProgramParameters.SetNamedConstant(leafAngleMatrixConstantNames[i], mat);
			}

			// get leaf billboard table

			// since we are doing GPU wind and leaf billboarding, set the camera direction to the positive X axis
			TreeCamera treeCamera = new TreeCamera();
			treeCamera.position = SpeedTreeUtil.ToSpeedTree(TerrainManager.Instance.CameraLocation);
			treeCamera.direction = SpeedTreeUtil.ToSpeedTree(new Vector3(1, 0, 0));
			SpeedTreeWrapper.Camera = treeCamera;

			speedTree.GetGeometry(geometry, SpeedTreeWrapper.GeometryFlags.LeafGeometry, -1, -1, 0);
			V4[] leafBillboards = speedTree.LeafBillboardTable;

            // set leaf billboard constants
            for ( uint i = 0; i < leafBillboards.Length; i++ ) 
            {
                leafPass.VertexProgramParameters.SetNamedConstant(leafClusterConstantNames[i], SpeedTreeUtil.FromSpeedTree(leafBillboards[i]) );
                leafPass.ShadowCasterVertexProgramParameters.SetNamedConstant(leafClusterConstantNames[i], SpeedTreeUtil.FromSpeedTree(leafBillboards[i]));
			
            }
            for (uint i = (uint)leafBillboards.Length; i < 48; i++)
            {
                leafPass.VertexProgramParameters.SetNamedConstant(leafClusterConstantNames[i], new Vector4(0,0,0,0));
                leafPass.ShadowCasterVertexProgramParameters.SetNamedConstant(leafClusterConstantNames[i], new Vector4(0, 0, 0, 0));
            }

			// set camera direction back
            treeCamera.position = SpeedTreeUtil.ToSpeedTree(TerrainManager.Instance.CameraLocation);
			treeCamera.direction = SpeedTreeUtil.ToSpeedTree(-TerrainManager.Instance.CameraDirection);
			SpeedTreeWrapper.Camera = treeCamera;

		}

		public bool Visible 
		{
			get 
			{
				return visible;
			}
		}

        public AxisAlignedBox Bounds
        {
            get
            {
                return bounds;
            }
        }

        private static TimingMeter loadMaterialMeter = MeterManager.GetMeter("SetupRenderMaterial", "Tree");
		/// <summary>
		/// Set up the rendering for a tree type.  Should be called once per tree type(master trees).
		/// </summary>
		public int SetupRenderMaterial(Material mat, bool setPosition)
		{
            int index = 0;
            if (meterRenderingOfTrees)
            {
                loadMaterialMeter.Enter();
            }
            // mvsm.SetTreeRenderPass() is a special version of SetPass() that just sets the stuff we need for tree rendering
			Pass usedPass = ((Axiom.SceneManagers.Multiverse.SceneManager)TerrainManager.Instance.SceneManager).SetTreeRenderPass(mat.GetBestTechnique(0).GetPass(0), forest);
			if ( setPosition && usedPass.HasVertexProgram ) 
			{
				index =  usedPass.VertexProgramParameters.GetParamIndex("g_vTreePosition");
			}
            if (meterRenderingOfTrees)
            {
                loadMaterialMeter.Exit();
            }
            return index;
		}

		public int SetupBranchRenderMaterial()
		{
			return SetupRenderMaterial(branchMaterial, true);
		}

		public int SetupFrondRenderMaterial()
		{
			return SetupRenderMaterial(frondMaterial, true);
		}

		public int SetupLeafRenderMaterial()
		{
			return SetupRenderMaterial(leafMaterial, true);
		}

		private TimingMeter renderBranchMeter = MeterManager.GetMeter("RenderBranch", "Tree");
		private TimingMeter renderFrondMeter = MeterManager.GetMeter("RenderFrond", "Tree");
		private TimingMeter renderLeavesMeter = MeterManager.GetMeter("RenderLeaves", "Tree");
		private TimingMeter renderBillboardsMeter = MeterManager.GetMeter("RenderBillboards", "Tree");
        private static bool meterRenderingOfTrees = false;

        public void RenderBranch(RenderSystem targetRenderSystem, int positionParamIndex, Tree t)
        {
            if (meterRenderingOfTrees)
            {
                renderBranchMeter.Enter();
            }

            int lod = t.BranchRenderArgs.LOD;

            if ((branchVertexBuffers[lod] != null) && (branchIndexBuffers[lod] != null))
            {

                targetRenderSystem.SetAlphaRejectSettings(CompareFunction.Greater, (byte)t.BranchRenderArgs.AlphaTestValue);

                // set the position register in the hardware
                positionParameters.SetConstant(positionParamIndex, t.Location.x, t.Location.y, t.Location.z, 0);
                targetRenderSystem.BindGpuProgramParameters(GpuProgramType.Vertex, positionParameters);

                branchRenderOp.vertexData = branchVertexBuffers[lod];
                branchRenderOp.indexData = branchIndexBuffers[lod];
                targetRenderSystem.Render(branchRenderOp);
            }
            if (meterRenderingOfTrees)
            {
                renderBranchMeter.Exit();
            }
        }

        public void RenderFrond(RenderSystem targetRenderSystem, int positionParamIndex, Tree t)
        {
            if (meterRenderingOfTrees)
            {
                renderFrondMeter.Enter();
            }

            int lod = t.FrondRenderArgs.LOD;

            if (frondVertexBuffers[lod] != null)
            {
                targetRenderSystem.SetAlphaRejectSettings(CompareFunction.Greater, (byte)t.FrondRenderArgs.AlphaTestValue);

                // set the position register in the hardware
                positionParameters.SetConstant(positionParamIndex, t.Location.x, t.Location.y, t.Location.z, 0);
                targetRenderSystem.BindGpuProgramParameters(GpuProgramType.Vertex, positionParameters);

                frondRenderOp.vertexData = frondVertexBuffers[lod];
                frondRenderOp.indexData = frondIndexBuffers[lod];
                if (frondRenderOp.indexData != null)
                {
                    targetRenderSystem.Render(frondRenderOp);
                }
            }

            if (meterRenderingOfTrees)
            {
                renderFrondMeter.Exit();
            }
        }

        public void RenderLeaves(RenderSystem targetRenderSystem, int positionParamIndex, Tree t)
        {
            if (meterRenderingOfTrees)
            {
                renderLeavesMeter.Enter();
            }

            // set the position register in the hardware
            positionParameters.SetConstant(positionParamIndex, t.Location.x, t.Location.y, t.Location.z, 0);
            targetRenderSystem.BindGpuProgramParameters(GpuProgramType.Vertex, positionParameters);

            if (t.Leaf0RenderArgs.Active)
            {
                targetRenderSystem.SetAlphaRejectSettings(CompareFunction.Greater, (byte)t.Leaf0RenderArgs.AlphaTestValue);
                int lod = t.Leaf0RenderArgs.LOD;
                leaf0RenderOp.vertexData = leafVertexBuffers[lod];
                leaf0RenderOp.indexData = leafIndexBuffers[lod];

                if (leaf0RenderOp.vertexData != null)
                {
                    targetRenderSystem.Render(leaf0RenderOp);
                }
            }
            if (t.Leaf1RenderArgs.Active)
            {
                targetRenderSystem.SetAlphaRejectSettings(CompareFunction.Greater, (byte)t.Leaf1RenderArgs.AlphaTestValue);

                int lod = t.Leaf1RenderArgs.LOD;
                leaf1RenderOp.vertexData = leafVertexBuffers[lod];
                leaf1RenderOp.indexData = leafIndexBuffers[lod];
                if (leaf1RenderOp.vertexData != null)
                {
                    targetRenderSystem.Render(leaf1RenderOp);
                }
            }
            if (meterRenderingOfTrees)
            {
                renderLeavesMeter.Exit();
            }
        }

        private static TimingMeter branchesMeter = MeterManager.GetMeter("RenderAllBranches", "Tree");
        private static TimingMeter frondsMeter = MeterManager.GetMeter("RenderAllFronds", "Tree");
        private static TimingMeter leavesMeter = MeterManager.GetMeter("RenderAllLeaves", "Tree");
        private static TimingMeter billboardsMeter = MeterManager.GetMeter("RenderAllBillboards", "Tree");
        private static TimingMeter rebuildBillboardsMeter = MeterManager.GetMeter("RebuildBillboards", "Tree");
        private static TimingMeter allTreeRenderMeter = MeterManager.GetMeter("RenderAllTrees", "Tree");

        public static void RenderAllTrees(RenderSystem targetRenderSystem)
        {
            int positionParamIndex;
            if (!initialized)
            {
                return;
            }

            allTreeRenderMeter.Enter();
            if (TerrainManager.Instance.RenderLeaves)
            {
                if (meterRenderingOfTrees)
                {
                    leavesMeter.Enter();
                }

                foreach (TreeGroup group in allGroups)
                {
                    if (group.visibleLeaves.Count != 0)
                    {
                        targetRenderSystem.BeginProfileEvent(ColorEx.White, "Render Leaves : " + group.name);

                        positionParamIndex = group.SetupLeafRenderMaterial();

                        foreach (Tree t in group.visibleLeaves)
                        {
                            group.RenderLeaves(targetRenderSystem, positionParamIndex, t);
                        }
                        // clear the constant from the previous material
                        group.positionParameters.ClearFloatConstant(positionParamIndex);
                    }
                    targetRenderSystem.EndProfileEvent();
                }
                if (meterRenderingOfTrees)
                {
                    leavesMeter.Exit();
                }
            }

            //
            // Render all fronds
            //
            if (meterRenderingOfTrees)
            {
                frondsMeter.Enter();
            }
            foreach (TreeGroup group in allGroups)
            {
                if (group.visibleFronds.Count != 0)
                {

                    targetRenderSystem.BeginProfileEvent(ColorEx.White, "Render Fronds : " + group.name);
                    positionParamIndex = group.SetupFrondRenderMaterial();

                    foreach (Tree t in group.visibleFronds)
                    {
                        group.RenderFrond(targetRenderSystem, positionParamIndex, t);
                    }
                    // clear the constant from the previous material
                    group.positionParameters.ClearFloatConstant(positionParamIndex);

                    targetRenderSystem.EndProfileEvent();
                }
            }
            if (meterRenderingOfTrees)
            {
                frondsMeter.Exit();
            }

            //
            // Render all branches
            //
            if (meterRenderingOfTrees)
            {
                branchesMeter.Enter();
            }
            foreach (TreeGroup group in allGroups)
            {
                if (group.visibleBranches.Count != 0)
                {
                    targetRenderSystem.BeginProfileEvent(ColorEx.White, "Render Branches : " + group.name);
                    positionParamIndex = group.SetupBranchRenderMaterial();
                    foreach (Tree t in group.visibleBranches)
                    {
                        group.RenderBranch(targetRenderSystem, positionParamIndex, t);
                    }
                    // clear the constant from the previous material
                    group.positionParameters.ClearFloatConstant(positionParamIndex);

                    targetRenderSystem.EndProfileEvent();
                }
            }
            if (meterRenderingOfTrees)
            {
                branchesMeter.Exit();
            }

            //
            // now deal with billboards
            //

            if (billboardsDirty)
            {
                if (meterRenderingOfTrees)
                {
                    rebuildBillboardsMeter.Enter();
                }
                // we need to rebuild the billboards
                TreeBillboardRenderer.Instance.StartRebuild();
                foreach (TreeGroup group in allGroups)
                {
                    foreach (Tree t in group.visibleBillboards)
                    {
                        if (t.Billboard0RenderArgs.Active)
                        {
                            TreeBillboardRenderer.Instance.AddBillboard(group.BillboardTextureName, t.Billboard0RenderArgs.AlphaTestValue, t.Billboard0);
                        }
                        if (t.Billboard1RenderArgs.Active)
                        {
                            TreeBillboardRenderer.Instance.AddBillboard(group.BillboardTextureName, t.Billboard1RenderArgs.AlphaTestValue, t.Billboard1);
                        }
                    }
                }
                TreeBillboardRenderer.Instance.FinishRebuild();
                if (meterRenderingOfTrees)
                {
                    rebuildBillboardsMeter.Exit();
                }
                billboardsDirty = false;
            }

            if (meterRenderingOfTrees)
            {
                billboardsMeter.Enter();
            }
            TreeBillboardRenderer.Instance.Render(targetRenderSystem);

            if (meterRenderingOfTrees)
            {
                billboardsMeter.Exit();
            }

            allTreeRenderMeter.Exit();
        }

        private static TimingMeter renderGroupMeter = MeterManager.GetMeter("Render TreeGroup", "Tree");
        public void Render(RenderSystem targetRenderSystem)
        {
            // exit early if the entire group is not visible
            if (!visible)
            {
                return;
            }
            int positionParamIndex;

            if (meterRenderingOfTrees)
            {
                renderGroupMeter.Enter();
            }
            targetRenderSystem.BeginProfileEvent(ColorEx.White, "Render Branches : " + name);
            positionParamIndex = SetupBranchRenderMaterial();
            foreach (Tree t in trees)
            {
                if (t.RenderFlag && t.BranchRenderArgs.Active)
                {
                    RenderBranch(targetRenderSystem, positionParamIndex, t);
                }
            }
            // clear the constant from the previous material
            positionParameters.ClearFloatConstant(positionParamIndex);

            targetRenderSystem.EndProfileEvent();

            targetRenderSystem.BeginProfileEvent(ColorEx.White, "Render Fronds : " + name);
            positionParamIndex = SetupFrondRenderMaterial();

            foreach (Tree t in trees)
            {
                if (t.RenderFlag && t.FrondRenderArgs.Active)
                {
                    RenderFrond(targetRenderSystem, positionParamIndex, t);
                }
            }
            // clear the constant from the previous material
            positionParameters.ClearFloatConstant(positionParamIndex);

            targetRenderSystem.EndProfileEvent();

            targetRenderSystem.BeginProfileEvent(ColorEx.White, "Render Leaves : " + name);
            if (TerrainManager.Instance.RenderLeaves)
            {

                positionParamIndex = SetupLeafRenderMaterial();

                foreach (Tree t in trees)
                {
                    if (t.RenderFlag && (t.Leaf0RenderArgs.Active || t.Leaf1RenderArgs.Active))
                    {
                        RenderLeaves(targetRenderSystem, positionParamIndex, t);
                    }
                }
                // clear the constant from the previous material
                positionParameters.ClearFloatConstant(positionParamIndex);
            }
            targetRenderSystem.EndProfileEvent();

            if (meterRenderingOfTrees)
            {
                renderGroupMeter.Exit();
            }
        }

		public float WindStrength 
		{
			set
			{
				float [] zeroangles = new float[6];

				for ( int i = 0; i < 6; i++ ) 
				{
					zeroangles[i] = 0;
				}

				speedTree.SetWindStrengthAndLeafAngles(value, zeroangles, zeroangles);
                foreach (Tree t in trees)
                {
                    t.SpeedTree.SetWindStrengthAndLeafAngles(value, zeroangles, zeroangles);
                }
			}
		}

		public void FindObstaclesInBox(AxisAlignedBox box,
									   CollisionTileManager.AddTreeObstaclesCallback callback)
		{
            if (box.Intersects(bounds))
            {
                foreach (Tree t in trees)
                {
                    t.FindObstaclesInBox(box, callback);
                }
            }
		}

        public TreeGeometry Geometry
        {
            get
            {
                return geometry;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string BillboardTextureName
        {
            get
            {
                return ConvertTextureName(treeTextures.CompositeFilename);
            }
        }

		#region IDisposable Members

		private void DisposeBuffers()
		{
            foreach (VertexData v in branchVertexBuffers)
            {
                if (v != null)
                {
                    HardwareVertexBuffer vb = v.vertexBufferBinding.GetBuffer(0);
                    if (vb != null)
                    {
                        vb.Dispose();
                    }
                }
            }
            foreach (IndexData i in branchIndexBuffers)
            {
                if (i != null)
                {
                    HardwareIndexBuffer ib = i.indexBuffer;
                    if (ib != null)
                    {
                        ib.Dispose();
                    }
                }
            }

            foreach (VertexData v in frondVertexBuffers)
            {
                if (v != null)
                {
                    HardwareVertexBuffer vb = v.vertexBufferBinding.GetBuffer(0);
                    if (vb != null)
                    {
                        vb.Dispose();
                    }
                }
            }
            foreach (IndexData i in frondIndexBuffers)
            {
                if (i != null)
                {
                    HardwareIndexBuffer ib = i.indexBuffer;
                    if (ib != null)
                    {
                        ib.Dispose();
                    }
                }
            }
            foreach (VertexData v in leafVertexBuffers)
            {
                if (v != null)
                {
                    HardwareVertexBuffer vb = v.vertexBufferBinding.GetBuffer(0);
                    if (vb != null)
                    {
                        vb.Dispose();
                    }
                }
            }
            foreach (IndexData i in leafIndexBuffers)
            {
                if (i != null)
                {
                    HardwareIndexBuffer ib = i.indexBuffer;
                    if (ib != null)
                    {
                        ib.Dispose();
                    }
                }
            }
		}

		public void Dispose()
		{
            foreach (Tree t in trees)
            {
                t.Dispose();
            }
            Debug.Assert(!disposed);
			DisposeBuffers();
            disposed = true;
            allGroups.Remove(this);
            billboardsDirty = true;
		}

		#endregion

	}
}
