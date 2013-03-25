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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Multiverse;
using Multiverse.CollisionLib;
using Multiverse.Config;

namespace Axiom.SceneManagers.Multiverse
{

	/// <summary>
	/// Summary description for TerrainManager.
	/// 
	/// Coordinate systems used here:
	///   World Space - the coordinate system used by the game.  Current multiverse client/server use a millimeter scale.
	///   Sample Space - the heightfield sample space.  One unit per height field sample.  Typically 1 meter.
	///   Page Space - one unit per page.  Typically 256 meters(pageSize).  
	///
    /// Notes:
    ///   Page and Tile objects are always in the same location relative to the camera's page, so they
    ///     are like an overlay that floats over the actual terrain.
    ///   Terrain patch renderables and heightmaps are shifted around as the camera moves from one page to
    ///     another.
	/// </summary>
	public class TerrainManager
	{
		// constants
		public const float oneMeter = 1000.0f;
		
		// The size of the page in "sample space"
		private int pageSize = 256;

		// A subpage is the size of the smallest possible tile.  All tiles must be some
		// power of 2 times this size.
		private int subPageSize;

		// Number of pages from the camera to the horizon
		private int visPageRadius;

        private int pageArraySize;

        private int minMetersPerSample;
        private int maxMetersPerSample;

		// The location of the camera within the world
        private Camera activeCamera;
		private Vector3 cameraLocation;
		private Vector3 cameraDirection;
		private PageCoord cameraPage;
		private PageCoord cameraSubPage;
		private Vector3 lastCameraLocation;

		private Vector3 cameraPageLocation;
		private bool cameraSet;

		private bool cameraTileChange;

		private Page [,] pages;

		private ITerrainGenerator terrainGenerator;
		private ILODSpec lodSpec;

		// has the Initialize() method been called on this object?
		private bool initialized;

		// have the height fields of the tiles been created?
		private bool heightFieldsCreated;

		private SceneNode worldRootSceneNode;
        private SceneNode rootSceneNode;
        private SceneNode oceanSceneNode;

		// private long queueProcessTickThreshold = 50; (unused)

		private bool drawTerrain = true;

		private static readonly TerrainManager instance = new TerrainManager();

		private float time;

		private SceneManager scene;

        private static readonly ILODSpec defaultLODSpec = new DefaultLODSpec();

        private List<Boundary> boundaries;

        private Roads roads;

        private Material waterMaterial;

        private OceanPage ocean;

        private OceanConfig oceanConfig;
        private ITerrainMaterialConfig terrainMaterialConfig;

        private TerrainDecalManager terrainDecalManager;

        private ShadowConfig shadowConfig;

        private bool showOcean;

        public event PageVisibilityEventHandler PageVisibility;

        /// <summary>
        /// This event is fired before the camera location is changed
        /// </summary>
        public event CameraLocationEventHandler SettingCameraLocation;

        /// <summary>
        /// This event is fired after the camera location has changed
        /// </summary>
        public event CameraLocationEventHandler SetCameraLocation;

        private CollisionTileManager collisionTileManager;
		
        private DetailVeg detailVeg;

        private bool renderLeaves = true;

        private bool treeDebug;

        public bool TreeDebug
        {
            get
            {
                return treeDebug;
            }
            set
            {
                treeDebug = value;
            }
        }

	    public bool CameraSet
	    {
	        get
	        {
	            return cameraSet;
	        }
	    }

		/// <summary>
		/// Constructor for the World Manager
		/// </summary>
		private TerrainManager()
		{
		    subPageLODScanZ = 0;
		    ActiveCamera = null;
			ParameterRegistry.RegisterSubsystemHandlers("Ocean", setOceanParameterHandler, 
														getOceanParameterHandler);
		}

		public static TerrainManager Instance 
		{
			get
			{
				return instance;
			}
        }

        #region Initialization and Cleanup

        public void Cleanup()
		{
			if ( initialized ) 
			{
			
				initialized = false;
				heightFieldsCreated = false;
                terrainDecalManager = null;
				cameraSet = false;

                if (pages != null)
                {
                    foreach (Page p in pages)
                    {
                        FreeTerrainPage(p);
                    }

                    pages = null;
                }

                foreach (Boundary b in boundaries)
                {
                    b.Dispose();
                }
                boundaries = null;

                roads.Dispose();
                roads = null;

                worldRootSceneNode.Creator.DestroySceneNode("TerrainRoot");
                worldRootSceneNode.Creator.DestroySceneNode("OceanNode");

                detailVeg.Dispose();
                detailVeg = null;

			}
		}

		/// <summary>
		/// Initialize the world manager
		/// </summary>
        public void Initialize(SceneManager sceneManager, ITerrainGenerator gen, ILODSpec clientLodSpec, SceneNode root)
        {

            Debug.Assert(!initialized, "Attempt to initialize already initialized TerrainManager");
            scene = sceneManager;
            terrainGenerator = gen;
            terrainGenerator.TerrainChanged += TerrainGenerator_OnTerrainChanged;
            lodSpec = clientLodSpec ?? defaultLODSpec;

            pageSize = lodSpec.PageSize;
            visPageRadius = lodSpec.VisiblePageRadius;
            pageArraySize = (visPageRadius * 2) + 1;

            // Compute the size of a "subPage", which is the same as the smalles tile size.
            // We assume that the page containing the camera will have the smallest sized tiles.
            subPageSize = pageSize / TilesPerPage(0);

            minMetersPerSample = lodSpec.MetersPerSample(Vector3.Zero, 0, 0);
            maxMetersPerSample = lodSpec.MetersPerSample(Vector3.Zero, visPageRadius, visPageRadius * pageSize / subPageSize);

            rootSceneNode = root;
            worldRootSceneNode = rootSceneNode.CreateChildSceneNode("TerrainRoot");
            oceanSceneNode = rootSceneNode.CreateChildSceneNode("OceanNode");

            terrainMaterialConfig = new AutoSplatConfig();

            terrainDecalManager = new TerrainDecalManager(pageArraySize);

            shadowConfig = new ShadowConfig(scene);

            oceanConfig = new OceanConfig();
            oceanConfig.ConfigChange += OceanConfigChanged;
            ocean = new OceanPage(oceanConfig);

            oceanSceneNode.AttachObject(ocean);
            //ocean.ShowBoundingBox = true;
            showOcean = true;

            detailVeg = new DetailVeg(65, rootSceneNode);

            // allocate the page array
            pages = new Page[pageArraySize, pageArraySize];

            InitPages();

            heightFieldsCreated = false;

            boundaries = new List<Boundary>();
            roads = new Roads();

            SpeedTreeWrapper.SetDropToBillboard(true);

            // make sure variables based on camera location are up to date
            CameraLocation = cameraLocation;

            // set up material used by water boundaries
            waterMaterial = MaterialManager.Instance.GetByName("MVSMWater");
            if (waterMaterial != null)
                waterMaterial.Load();

            initialized = true;
        }

        private void TerrainGenerator_OnTerrainChanged(ITerrainGenerator generator, int worldXMeters, int worldZMeters, int sizeXMeters, int sizeZMeters)
        {
            int topLeftPageXMeters = (int) (pages[0, 0].Location.x/oneMeter);
            int topLeftPageZMeters = (int) (pages[0, 0].Location.z/oneMeter);
            int pageSizeMeters = pageSize;

            // Find out which pages are affectd by the changed region and recreate them.
            int pageStartX = (worldXMeters - topLeftPageXMeters) / pageSizeMeters;
            int pageStartZ = (worldZMeters - topLeftPageZMeters)/pageSizeMeters;

            int pageEndX = (worldXMeters - topLeftPageXMeters + sizeXMeters) / pageSizeMeters;
            int pageEndZ = (worldZMeters - topLeftPageZMeters + sizeZMeters) / pageSizeMeters;

            // Recreate the modified pages
            for (int z = pageStartZ; z <= pageEndZ; z++)
            {
                for (int x = pageStartX; x <= pageEndX; x++)
                {
                    Page modifiedPage = LookupPage(x, z);
                    if (modifiedPage != null)
                    {
//                        Console.WriteLine("Updating page: [" + x + "," + z + "] at: " + pages[x, z].Location);
                        pages[x, z].TerrainPage.ResetHeightMaps();

                    }
                }
            }
	    }

	    private TerrainPage NewTerrainPage(Page p)
        {
            Debug.Assert(p.TerrainPage == null);

            p.TerrainPage = new TerrainPage(p.Location, p);

            OnPageVisibility(true, p.TerrainPage);

            return p.TerrainPage;
        }

        private void FreeTerrainPage(Page p)
        {
            if (p.TerrainPage != null)
            {
                OnPageVisibility(false, p.TerrainPage);

                p.TerrainPage.Dispose();

                p.TerrainPage = null;
            }
        }

        // create new terrainPages for all the pages
        private void InitTerrainPages()
        {
            foreach (Page p in pages)
            {
                if (p.TerrainPage != null)
                {
                    FreeTerrainPage(p);
                }
                p.TerrainPage = NewTerrainPage(p);
            }
        }

        private void ShiftPageHeightMaps(PageCoord pc)
        {
            ValidatePageArray();

            int dx = pc.X;
            int dz = pc.Z;

            // make sure we are actually moving
            Debug.Assert((dx != 0) || (dz != 0));

            int copyStartX = 0;
            int copyEndX = pageArraySize;
            int copyIncrX = 1;
            int copyStartZ = 0;
            int copyEndZ = pageArraySize;
            int copyIncrZ = 1;

            // keep track of the bounds of the area we keep after shifting
            int clearStartX = 0;
            int clearEndX = 0;
            int keepStartZ = 0;
            int keepEndZ = pageArraySize;

            int maxMove = pageArraySize - 1;

            // set to true if we are moving the camera far enough that all of the current heightmaps
            // move out of view
            bool bigMove = false;

            if ((dx > maxMove) || (dx < -maxMove) || (dz > maxMove) || (dz < -maxMove))
            {
                bigMove = true;    
            }

            if (!bigMove)
            {
                // compute move params for the X axis
                if (dx > 0)
                {
                    copyStartX = pageArraySize - dx - 1;
                    copyEndX = -1;
                    copyIncrX = -1;

                    clearStartX = 0;
                    clearEndX = dx;
                }
                else if (dx < 0)
                {
                    copyStartX = -dx;
                    copyEndX = pageArraySize;
                    copyIncrX = 1;

                    clearStartX = pageArraySize + dx;
                    clearEndX = pageArraySize;
                }

                // compute move params for the Z axis
                if (dz > 0)
                {
                    copyStartZ = pageArraySize - dz - 1;
                    copyEndZ = -1;
                    copyIncrZ = -1;
                    keepStartZ = dz;
                    keepEndZ = pageArraySize;
                }
                else if (dz < 0)
                {
                    copyStartZ = -dz;
                    copyEndZ = pageArraySize;
                    copyIncrZ = 1;
                    keepStartZ = 0;
                    keepEndZ = pageArraySize + dz;
                }

                long startTick = Root.Instance.Timer.Milliseconds;

                // copy all height maps that we are keeping
                for (int z = copyStartZ; z != copyEndZ; z += copyIncrZ)
                {
                    for (int x = copyStartX; x != copyEndX; x += copyIncrX)
                    {
                        // move the heightmap to its new page, and set its page property
                        // dispose of the old terrain page
                        int destX = x + dx;
                        int destZ = z + dz;

                        if (pages[destX, destZ].TerrainPage != null)
                        {
                            pages[destX, destZ].TerrainPage.Dispose();
                        }

                        // move the terrainPage, and set its CurrentPage to the new page
                        pages[destX, destZ].TerrainPage = pages[x, z].TerrainPage;
                        pages[destX, destZ].TerrainPage.CurrentPage = pages[destX, destZ];

                        // clear out the old reference to the TerrainPage we just moved
                        pages[x, z].TerrainPage = null;
                    }
                }

                long middleTick = Root.Instance.Timer.Milliseconds;

                // Perform processing on all pages that are newly revealed after the shift
                // Set locations on the pageHeightMaps so that they get rebuilt at their new
                //  locations.
                // Clear and reinitialize the terrain patches of all these pages
                int newPages = 0;
                for (int z = 0; z < pageArraySize; z++)
                {
                    if ((z < keepStartZ) || (z >= keepEndZ))
                    {
                        // this row does not overlap the keep area, so reainitialize all pages
                        for (int x = 0; x < pageArraySize; x++)
                        {
                            // dispose of the old terrainPage
                            if (pages[x, z].TerrainPage != null)
                            {

                                FreeTerrainPage(pages[x, z]);
                            }
                            pages[x, z].TerrainPage = NewTerrainPage(pages[x, z]);
                            newPages++;
                        }
                    }
                    else  // this row overlaps the keep area
                    {
                        for (int x = clearStartX; x < clearEndX; x++)
                        {
                            // dispose of the old terrainPage
                            if (pages[x, z].TerrainPage != null)
                            {
                                FreeTerrainPage(pages[x, z]);
                            }
                            pages[x, z].TerrainPage = NewTerrainPage(pages[x, z]);
                            newPages++;
                        }
                    }
                }

                long finalTick = Root.Instance.Timer.Milliseconds;


                LogManager.Instance.Write("ShiftPageHeightMaps: Shift: {0}", middleTick - startTick);
                LogManager.Instance.Write("ShiftPageHeightMaps: Clear/Create: {0}", finalTick - middleTick);
                LogManager.Instance.Write("ShiftPageHeightMaps: dx = {0}, dz = {1}, newPages = {2}", dx, dz, newPages);

            }
            else // bigmove
            {
                long startTick = Root.Instance.Timer.Milliseconds;

                InitTerrainPages();

                long finalTick = Root.Instance.Timer.Milliseconds;
                LogManager.Instance.Write("ShiftPageHeightMaps: BigMove: {0}", finalTick - startTick);
            }

            ValidatePageArray();
        }

        private void ValidatePageArray()
        {
            for (int z = 0; z < pageArraySize; z++)
            {
                for (int x = 0; x < pageArraySize; x++)
                {
                    Debug.Assert(pages[x, z] != null);
                    Debug.Assert(pages[x, z].TerrainPage != null);
                    Debug.Assert(pages[x, z].TerrainPage.CurrentPage != null);
                }
            }
        }

        #endregion Initialization and Cleanup

        #region Terrain Material Support

        public ITerrainMaterialConfig TerrainMaterialConfig
        {
            get
            {
                return terrainMaterialConfig;
            }
            set
            {
                terrainMaterialConfig = value;

                foreach (Page p in pages)
                {
                    if (p.TerrainPage != null)
                    {
                        p.TerrainPage.UpdateMaterial();
                    }
                }
            }
        }

        #endregion Terrain Material Support


        #region Decal Support
        public TerrainDecalManager TerrainDecalManager
        {
            get
            {
                return terrainDecalManager;
            }
        }

        #endregion Decal Support

        #region Boundary Support

        public void AddBoundary(Boundary b)
        {
            boundaries.Add(b);
            b.AddToScene(scene);
        }

        public void RemoveBoundary(Boundary b)
        {
            boundaries.Remove(b);

            b.Dispose();
        }

        public void FindObstaclesInBox(AxisAlignedBox box, 
                                       CollisionTileManager.AddTreeObstaclesCallback callback)
        {
            foreach ( Boundary boundary in boundaries ) 
            {
                boundary.FindObstaclesInBox(box, callback);
            }
        }
		
		#endregion Boundary Support

        #region ocean support
        public bool ShowOcean
        {
            get
            {
                return showOcean;
            }
            set
            {
                if (value != showOcean)
                {
                    showOcean = value;
                    if (showOcean)
                    {
                        oceanSceneNode.AttachObject(ocean);
                    }
                    else
                    {
                        oceanSceneNode.DetachObject(ocean);
                    }
                }
            }
        }

        public OceanConfig OceanConfig
        {
            get
            {
                return oceanConfig;
            }
        }

        protected void OceanConfigChanged(object sender, EventArgs e)
        {
            if (oceanConfig.ShowOcean != ShowOcean)
            {
                ShowOcean = oceanConfig.ShowOcean;
            }
        }

		private bool setOceanParameterHandler(string parameterName, string parameterValue)
		{
			string s = parameterValue.ToLower();
			switch(parameterName) {
			case "DisplayOcean":
				oceanConfig.ShowOcean = s == "true";
                ShowOcean = oceanConfig.ShowOcean;
				break;
			case "UseParams":
				oceanConfig.UseParams = s == "true";
				break;
			case "DeepColor":
				oceanConfig.DeepColor = ColorEx.Parse_0_255_String(s);
				break;
			case "ShallowColor":
				oceanConfig.ShallowColor = ColorEx.Parse_0_255_String(s);
				break;
			default:
				// All the rest are floats
				float value;
				try {
					value = float.Parse(parameterValue);
				}
				catch(Exception) {
					return false;
				}
				switch (parameterName) {
				case "SeaLevel":
					oceanConfig.SeaLevel = value;
					break;
				case "WaveHeight":
					oceanConfig.WaveHeight = value;
					break;
				case "BumpScale":
					oceanConfig.BumpScale = value;
					break;
				case "TextureScaleX":
					oceanConfig.TextureScaleX = value;
					break;
				case "TextureScaleZ":
					oceanConfig.TextureScaleZ = value;
					break;
				case "BumpSpeedX":
					oceanConfig.BumpSpeedX = value;
					break;
				case "BumpSpeedZ":
					oceanConfig.BumpSpeedZ = value;
					break;
				default:
					return false;
				}
                break;
			}
            return true;
		}
		
		private bool getOceanParameterHandler(string parameterName, out string parameterValue)
		{
			switch(parameterName) {
			case "DisplayOcean":
				parameterValue = oceanConfig.ShowOcean.ToString();
				break;
			case "UseParams":
				parameterValue = oceanConfig.UseParams.ToString();
				break;
			case "DeepColor":
				parameterValue = oceanConfig.DeepColor.To_0_255_String();
				break;
			case "ShallowColor":
				parameterValue = oceanConfig.ShallowColor.To_0_255_String();
				break;
            case "Help":
                parameterValue = OceanParameterHelp();
                break;
			default:
				// All the rest are floats
				float value;
				switch (parameterName) {
				case "SeaLevel":
					value = oceanConfig.SeaLevel;
					break;
				case "WaveHeight":
					value = oceanConfig.WaveHeight;
					break;
				case "BumpScale":
					value = oceanConfig.BumpScale;
					break;
				case "TextureScaleX":
					value = oceanConfig.TextureScaleX;
					break;
				case "TextureScaleZ":
					value = oceanConfig.TextureScaleZ;
					break;
				case "BumpSpeedX":
					value = oceanConfig.BumpSpeedX;
					break;
				case "BumpSpeedZ":
					value = oceanConfig.BumpSpeedZ;
					break;
				default:
					parameterValue = "";
					return false;
				}
				parameterValue = value.ToString();
                break;
			}
            return true;
		}

		private string OceanParameterHelp()
		{
			return
                "bool DisplayOcean: This property controls whether the client will display the automatically generated ocean; default is true" +
				"\n" +
                "bool UseParams: Set this property to false if you are using your own ocean material that uses different Vertex and Pixel shader parameters than the default Ocean shaders provided by Multiverse.; default is true" +
				"\n" +
                "Color(r,g,b,a) DeepColor: The predominate color when looking directly into the water, used to simulate a deep water effect.; default is (0,0,25,255)" + 
				"\n" +
                "Color(r,g,b,a) ShallowColor:  	 The predominate color when looking at the water from an acute angle, used to simulate a shallow water effect.; default is (0,76,127, 255)" +
				"\n" +
                "float SeaLevel: The SeaLevel property allows you to set the average level of the ocean. The value is specified in millimeters.; default is 10000 (10 meters)" +
				"\n" +
                "float WaveHeight: This property lets you set the amplitude of the ocean waves. The value is specified in millimeters.; default is 1000 (1 meter)" +
				"\n" +
                "float BumpScale: This value is used to scale the normal mapping effect used to simulate smaller waves. Increasing BumpScale will make these smaller waves look taller.; default is 0.5" +
				"\n" +
                "float TextureScaleX: This value controls the scaling of the normal map texture used to generate the small wave bump effect, along the X axis.; default is 0.015625" +
				"\n" +
                "float TextureScaleZ: This value controls the scaling of the normal map texture used to generate the small wave bump effect, along the Z axis.; default is 0.0078125" +
				"\n" +
                "float BumpSpeedX: This value controls the speed at which the small wave bump effect moves along the X axis. Increasing this value will make the smaller waves move faster.; default is -0.005" +
				"\n" +
                "float BumpSpeedZ: This value controls the speed at which the small wave bump effect moves along the Z axis. Increasing this value will make the smaller waves move faster.; default is 0" +
				"\n";
		}

		
		
		#endregion ocean support

        #region shadow config support

        public ShadowConfig ShadowConfig
        {
            get
            {
                return shadowConfig;
            }
        }

        #endregion shadow config support

        #region detail vegetation support

        public void AddDetailVegetationSemantic(VegetationSemantic vegetationSemantic)
		{
			detailVeg.AddVegetationSemantic(vegetationSemantic);
		}

        public void RemoveDetailVegetationSemantic(VegetationSemantic vegetationSemantic)
        {
            detailVeg.RemoveVegetationSemantic(vegetationSemantic);
        }

        public void RefreshVegetation()
        {
            detailVeg.RefreshVegetation();
        }

        public bool ShowDetailVeg
        {
            get
            {
                return detailVeg.Enabled;
            }
            set
            {
                detailVeg.Enabled = value;
            }
        }

        public float DetailVegRoadClearRadius
        {
            get
            {
                return detailVeg.RoadClearRadius;
            }
            set
            {
                detailVeg.RoadClearRadius = value;
            }
        }

        #endregion detail vegetation support

        public bool RenderLeaves
        {
            get
            {
                return renderLeaves;
            }
            set
            {
                renderLeaves = value;
            }
        }

        public Road CreateRoad(String name)
        {
            return roads.CreateRoad(name);
        }

        public void RemoveRoad(Road r)
        {
            roads.RemoveRoad(r);
        }

        internal Roads Roads
        {
            get
            {
                return roads;
            }
        }

        public void DumpLOD()
        {
            LogManager.Instance.Write("DumpLOD start");
            for (int z = 0; z < pageArraySize; z++)
            {
                for (int x = 0; x < pageArraySize; x++)
                {
                    Page page = pages[x, z];

                    page.TerrainPage.DumpLOD();
                }
            }
        }

        public void UpdateCamera(Camera camera)
        {
            if (ActiveCamera == null || ActiveCamera == camera)
            {
                CameraLocation = camera.Position;
                CameraDirection = camera.Direction;

                TreeCamera treeCamera = new TreeCamera();
                treeCamera.position = SpeedTreeUtil.ToSpeedTree(camera.DerivedPosition);
                treeCamera.direction = SpeedTreeUtil.ToSpeedTree(-camera.Direction);
                SpeedTreeWrapper.Camera = treeCamera;

                Instance.PerFrameProcessing(camera);

                // Handle any LOD processing
                Instance.ProcessLODChanges(camera);
            }
        }

		public void PerFrameProcessing(Camera camera)
		{
            foreach (Boundary boundary in boundaries) 
			{
				boundary.PerFrameSemantics(time, camera);
			}

            // update ocean wave time
            ocean.WaveTime = time;
            SetWaterTime();

            detailVeg.PerFrameProcessing(camera);

            //terrainDecalManager.PerFrameProcessing(CameraLocation);
		}

        public void LatePerFrameProcessing(Camera camera)
        {
            if (ActiveCamera == null || camera == ActiveCamera)
            {
                terrainDecalManager.PerFrameProcessing(CameraLocation);
            }
        }

        private void SetWaterTime()
        {
            if (waterMaterial != null)
            {
                Technique t = waterMaterial.GetTechnique(0);
                if (t.IsSupported)
                {
                    t.GetPass(0).VertexProgramParameters.SetNamedConstant("time", new Vector3(time, 0, 0));
                }
            }
        }

		/// <summary>
		/// This method initializes all the pages in the visible area around the camera.  It should only
		/// be called when starting up, or after all the previous pages have been released.
		/// </summary>
		protected void InitPages()
		{

			float startPageOffset = pageSize * oneMeter * visPageRadius;
			Vector3 pv = new Vector3(-startPageOffset, 0, -startPageOffset);
			for ( int x = 0; x < pageArraySize; x++ ) 
			{
				for ( int z = 0; z < pageArraySize; z++ ) 
				{
					pages[x,z] = new Page(pv, x, z);

					pv.z += pageSize * oneMeter;
				}
				pv.x += pageSize * oneMeter;
				pv.z = -startPageOffset;
			}

			// attach the pages
			foreach ( Page p in pages ) 
			{
				p.AttachNeighbors();
			}

			// attach the tiles
			foreach ( Page p in pages ) 
			{
				p.AttachTiles();
			}
		}

		public float Time
		{
			get 
			{
				return time;
			}
			set
			{
				time = value;
			}
		}

        public bool Initialized
        {
            get
            {
                return initialized;
            }
        }

		public int PageArraySize 
		{
			get 
			{
				return pageArraySize;
			}
		}

		public int VisPageRadius 
		{
			get 
			{
				return visPageRadius;
			}
		}

		public Vector3 CameraPageLocation 
		{
			get 
			{
				return cameraPageLocation;
			}
		}

        public PageCoord CameraPage
        {
            get
            {
                return cameraPage;
            }
        }

        public PageCoord MinVisiblePage
        {
            get
            {
                return new PageCoord(cameraPage.X - VisPageRadius, cameraPage.Z - VisPageRadius);
            }
        }

        public PageCoord MaxVisiblePage
        {
            get
            {
                return new PageCoord(cameraPage.X + VisPageRadius, cameraPage.Z + VisPageRadius);
            }
        }

		public Vector3 CameraDirection
		{
			get 
			{
				return cameraDirection;
			}
			set
			{
				cameraDirection = value;
			}
		}

        private void BeforePageShift()
        {
            //foreach (Page p in pages)
            //{
            //    p.Hilight = false;
            //}
        }

        private void AfterPageShift()
        {
            foreach (Boundary b in boundaries)
            {
                b.PageShift();
            }
        }

        public Camera ActiveCamera 
        {
            get
            {
                return activeCamera;
            }
            set
            {
                activeCamera = value;
                cameraSet = false;
            }
        } 

        private void OnSettingCameraLocation(Vector3 oldLoc, Vector3 newLoc)
        {
            CameraLocationEventHandler e = SettingCameraLocation;
            if (e != null)
            {
                e(null, new CameraLocationEventArgs(oldLoc, newLoc));
            }
        }

        private void OnSetCameraLocation(Vector3 oldLoc, Vector3 newLoc)
        {
            CameraLocationEventHandler e = SetCameraLocation;
            if (e != null)
            {
                e(null, new CameraLocationEventArgs(oldLoc, newLoc));
            }
        }

		public Vector3 CameraLocation 
		{
			get 
			{
				return cameraLocation;
			}
			set 
			{
				if ( !cameraSet || ( cameraLocation != value ) )
				{
                    //LogManager.Instance.Write("TerrainManager.CameraLocation: {0}", value.ToString());
                    PageCoord newCameraPage = new PageCoord(value, pageSize);

                    OnSettingCameraLocation(cameraLocation, value);

					cameraSet = true;
					lastCameraLocation = cameraLocation;

					cameraLocation = value;

					cameraPage = newCameraPage;
					cameraSubPage = new PageCoord(cameraLocation, subPageSize);
					cameraPageLocation = cameraPage.WorldLocation(pageSize);

					// update the position of the top level scene node for the terrain
					// worldRootSceneNode.Position = cameraPageLocation;
                    oceanSceneNode.Position = new Vector3(cameraLocation.x, 0, cameraLocation.z);

					if ( !heightFieldsCreated ) 
					{
						// if the height fields havent been populated yet, then do so now
                        InitTerrainPages();

                        heightFieldsCreated = true;
                        AfterPageShift();
					} 
					else 
					{
						PageCoord lastCameraPage = new PageCoord(lastCameraLocation, pageSize);

						if ( lastCameraPage != cameraPage ) 
						{ // camera moved to a different page

							long startTick = Root.Instance.Timer.Milliseconds;

                            BeforePageShift();

							PageCoord pageDelta = lastCameraPage - cameraPage;
                            ShiftPageHeightMaps(pageDelta);

                            LogManager.Instance.Write("Page Shift: {0}", Root.Instance.Timer.Milliseconds - startTick);
							cameraTileChange = true;

                            AfterPageShift();
						} 
						else 
						{
							PageCoord lastCameraSubPage = new PageCoord(lastCameraLocation, subPageSize);
							if ( lastCameraSubPage != cameraSubPage ) 
							{
								// LOD will be adjusted in ProcessLODChanges()
								cameraTileChange = true;
							}
						}
					}

                    OnSetCameraLocation(lastCameraLocation, cameraLocation);
				}
			}
		}


		public Page LookupPage(int pageX, int pageZ)
		{
            if (pageX < 0 || pageX >= pageArraySize || pageZ < 0 || pageZ >= pageArraySize)
            {
                return null;
            }

			return pages[pageX, pageZ];
		}

		public int PageSize 
		{
			get 
			{
				return pageSize;
			}
		}

        public int SubPageSize
        {
            get
            {
                return subPageSize;
            }
        }

        public int MinMetersPerSample
        {
            get
            {
                return minMetersPerSample;
            }
        }

        public int MaxMetersPerSample
        {
            get
            {
                return maxMetersPerSample;
            }
        }


		public ITerrainGenerator TerrainGenerator
		{
			get 
			{
				return terrainGenerator;
			}
		}

		public SceneNode WorldRootSceneNode
		{
			get 
			{
				return worldRootSceneNode;
			}
		}

        public SceneNode RootSceneNode
        {
            get
            {
                return rootSceneNode;
            }
        }

		/// <summary>
		/// Returns the number of tiles (along X and Z axis) for a page at the given distance from the camera.
		/// Actual number of tiles in the page is this value squared.
		/// </summary>
		/// <param name="pagesFromCamera">distance (in pages) from this the page in question to the page containing the camera</param>
		/// <returns></returns>
		public int TilesPerPage(int pagesFromCamera)
		{
			return lodSpec.TilesPerPage(pagesFromCamera);
		}

		// This function computes the level of detail(meters per sample) that a tile should use, based
		// on the tiles location (distance from the camera).  Eventually this might be a delegate supplied
		// by the application to allow for more flexible configuration.
		public int MetersPerSample(Vector3 tileLoc)
		{
			return lodSpec.MetersPerSample(tileLoc, PagesFromCamera(tileLoc), SubPagesFromCamera(tileLoc));
		}

		public int SubPagesFromCamera(Vector3 loc)
		{
			return cameraSubPage.Distance(new PageCoord(loc, subPageSize));
		}

		public int PagesFromCamera(Vector3 loc)
		{
			return cameraPage.Distance(new PageCoord(loc, pageSize));
		}


		/// <summary>
		/// Find the page for a given location
		/// </summary>
		/// <param name="location"></param>
		/// <returns>Returns the page, or null if that page is not currently loaded</returns>
		public Page LookupPage(Vector3 location)
		{
            PageCoord locPage = new PageCoord(location, pageSize);
            return LookupPage(locPage);
        }

        public Page LookupPage(PageCoord locPage)
        {
            Debug.Assert(cameraSet, "Camera not set in LookupPage");
			PageCoord pageOffset = locPage - cameraPage + new PageCoord(visPageRadius, visPageRadius);

			if ( pageOffset.X < 0 || pageOffset.X >= pageArraySize || pageOffset.Z < 0 || pageOffset.Z >= pageArraySize ) 
			{
				return null;
			}

            return pages[pageOffset.X, pageOffset.Z];
        }

        /// <summary>
        /// return the location of the page origin of the page containing the given location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Vector3 PageOrigin(Vector3 location)
        {
            PageCoord locPage = new PageCoord(location, pageSize);
            return locPage.WorldLocation(pageSize);
        }

		/// <summary>
		/// Returns the tile at a specific location in world space.
		/// </summary>
		/// <param name="loc"></param>
		/// <returns>Returns the tile, or null if the location is in a part of the world that is not loaded</returns>
		public Tile LookupTile(Vector3 loc)
		{
			Page p = LookupPage(loc);
			if ( p == null ) 
			{
				return null;
			}

		    return p.LookupTile(loc);
		}

        public SubPageHeightMap LookupSubPage(Vector3 loc)
        {
            Page p = LookupPage(loc);
            if (p == null)
            {
                return null;
            }

            return p.TerrainPage.PageHeightMap.LookupSubPage(loc);
        }

		public Vector3 GetNormalAt(Vector3 loc)
		{
            float x = (float)Math.Round(loc.x / oneMeter) * oneMeter;
            float z = (float)Math.Round(loc.z / oneMeter) * oneMeter;

            float x1 = GetTerrainHeight(new Vector3(x - oneMeter, 0, z), GetHeightMode.Closest, GetHeightLOD.MaxLOD);
            float x2 = GetTerrainHeight(new Vector3(x + oneMeter, 0, z), GetHeightMode.Closest, GetHeightLOD.MaxLOD);
            float z1 = GetTerrainHeight(new Vector3(x, 0, z - oneMeter), GetHeightMode.Closest, GetHeightLOD.MaxLOD);
            float z2 = GetTerrainHeight(new Vector3(x, 0, z + oneMeter), GetHeightMode.Closest, GetHeightLOD.MaxLOD);

            // compute the normal
            Vector3 v = new Vector3(x1 - x2,
                2.0f * oneMeter,
                z1 - z2);
            v.Normalize();

            return v;
		}

		public bool WithinVisibleArea(Vector3 location)
		{
			PageCoord locPage = new PageCoord(location, pageSize);
			PageCoord pageOffset = locPage - cameraPage + new PageCoord(visPageRadius, visPageRadius);

			if ( pageOffset.X < 0 || pageOffset.X >= pageArraySize || pageOffset.Z < 0 || pageOffset.Z >= pageArraySize ) 
			{
				return false;
			}
		
            return true;
		}

        public float GetTerrainHeight(Vector3 loc, GetHeightMode mode, GetHeightLOD lodFlag)
        {
            int metersPerSample;
            float ret;

            Vector3 pageLoc = (new PageCoord(loc, pageSize)).WorldLocation(pageSize);

            // TerrainPage terrainPage = LookupPage(loc).TerrainPage;

            // compute offset of point within the page
            Vector3 offset = loc - pageLoc;

            switch (lodFlag)
            {
                case GetHeightLOD.CurrentLOD:
                    metersPerSample = MetersPerSample(loc);
                    break;
                case GetHeightLOD.MaxLOD:
                default:
                    metersPerSample = minMetersPerSample;
                    break;
            }

            // convert the offset coordinates to meters
            float xPt = offset.x / oneMeter;
            float zPt = offset.z / oneMeter;

            // coordinates of point in sample space(meters) within the page, adjusted to the 
            // nearest sample point based on the mode.
            //  NOTE - Interpolate mode truncates, then interpolates between that point and the
            //  other 3 surrounding the quad containing the original point.
            int xSamp;
            int zSamp;

            // adjust the coordinates to the nearest sample coordinate based on meters per sample
            // and the mode.
            switch (mode)
            {
                case GetHeightMode.Closest:
                    xSamp = ( (int)Math.Round(xPt / metersPerSample) ) * metersPerSample;
                    zSamp = ( (int)Math.Round(zPt / metersPerSample) ) * metersPerSample;
                    break;
                case GetHeightMode.Truncate:
                case GetHeightMode.Interpolate:
                default:
                    xSamp = ( (int)Math.Floor(xPt / metersPerSample) ) * metersPerSample;
                    zSamp = ( (int)Math.Floor(zPt / metersPerSample) ) * metersPerSample;
                    break;
            }

            if (mode == GetHeightMode.Interpolate)
            {
                //bool eastEdge = false;
                //bool northEdge = false;


                float xLoc = pageLoc.x + xSamp * oneMeter;
                float zLoc = pageLoc.z + zSamp * oneMeter;
                float sampleIncr = metersPerSample * oneMeter;

                float nw = Instance.TerrainGenerator.GenerateHeightPointMM(
                    new Vector3(xLoc, 0, zLoc));
                float ne = Instance.TerrainGenerator.GenerateHeightPointMM(
                    new Vector3(xLoc + sampleIncr, 0, zLoc));
                float sw = Instance.TerrainGenerator.GenerateHeightPointMM(
                    new Vector3(xLoc, 0, zLoc + sampleIncr));
                float se = Instance.TerrainGenerator.GenerateHeightPointMM(
                    new Vector3(xLoc + sampleIncr, 0, zLoc + sampleIncr));



                // XXX - for now we just use the above code to compute heights rather than look them up
                // in the height maps
                //sw = terrainPage.PageHeightMap.GetHeight(xSamp, zSamp);

                //// check if we cross subpage boundary
                //// SubPageSize is a power of 2, so it has 1 bit set.  If that bit changes
                //// when the coordinate is incremented, it means we are crossing a subpage boundary.
                //if ( ( ( xSamp & subPageSize ) != ( ( xSamp + 1 ) & subPageSize ) ) ||
                //    ( ( zSamp & subPageSize ) != ( ( zSamp + 1 ) & subPageSize ) ) )
                //{
                //    // if we go off the edge of the page, we generate the other points,
                //    // since it is easier/cleaner than trying to get the data from the adjoining page
                //    float xLoc = terrainPage.Location.x + xSamp * TerrainManager.oneMeter;
                //    float zLoc = terrainPage.Location.z + zSamp * TerrainManager.oneMeter;
                //    float sampleIncr = metersPerSample * TerrainManager.oneMeter;

                //    se = TerrainManager.Instance.TerrainGenerator.GenerateHeightPointMM(
                //        new Vector3(xLoc + sampleIncr, 0, zLoc));
                //    nw = TerrainManager.Instance.TerrainGenerator.GenerateHeightPointMM(
                //        new Vector3(xLoc, 0, zLoc + sampleIncr));
                //    ne = TerrainManager.Instance.TerrainGenerator.GenerateHeightPointMM(
                //        new Vector3(xLoc + sampleIncr, 0, zLoc + sampleIncr));
                //}
                //else
                //{
                //    se = terrainPage.PageHeightMap.GetHeight(xSamp + 1, zSamp);
                //    nw = terrainPage.PageHeightMap.GetHeight(xSamp, zSamp + 1);
                //    ne = terrainPage.PageHeightMap.GetHeight(xSamp + 1, zSamp + 1);
                //}

                float xFrac = (xPt - xSamp) / metersPerSample;
                float zFrac = (zPt - zSamp) / metersPerSample;

                if (xFrac >= zFrac)
                {
                    ret = ((nw + (ne - nw) * xFrac + (se - ne) * zFrac));
                }
                else
                {
                    ret = ((sw + (se - sw) * xFrac + (nw - sw) * (1 - zFrac)));
                }
            }
            else
            {
                Page page = LookupPage(loc);
                if (page == null)
                {
                    ret = 0;
                }
                else
                {
                    TerrainPage terrainPage = page.TerrainPage;
                    ret = terrainPage.PageHeightMap.GetHeight(xSamp, zSamp);
                }
                
            }

            return ret;
        }


        public Vector3 GetClosestVertex(Vector3 loc)
        {
            Vector3 pageLoc = (new PageCoord(loc, pageSize)).WorldLocation(pageSize);

            // TerrainPage terrainPage = LookupPage(loc).TerrainPage;

            // compute offset of point within the page
            Vector3 offset = loc - pageLoc;

            // get metersPerSample at the loc we are trying for
            int metersPerSample = MetersPerSample(loc);

            // convert the offset coordinates to meters
            float xPt = offset.x / oneMeter;
            float zPt = offset.z / oneMeter;

            // find the x and z of the nearest vertex
            int xSamp = ((int)Math.Round(xPt / metersPerSample)) * metersPerSample;
            int zSamp = ((int)Math.Round(zPt / metersPerSample)) * metersPerSample;
            return new Vector3(xSamp, loc.y, zSamp);
        }

        public float GetAreaHeight(Vector3[] points)
        {
            if (!cameraSet)
            {
                return float.MinValue;
            }

            // compute axis aligned bounding box of points
            float fx1 = points[0].x;
            float fx2 = points[0].x;
            float fz1 = points[0].z;
            float fz2 = points[0].z;

            for (int i = 1; i < points.Length; i++)
            {
                if (points[i].x < fx1)
                {
                    fx1 = points[i].x;
                }
                else if (points[i].x > fx2)
                {
                    fx2 = points[i].x;
                }

                if (points[i].z < fz1)
                {
                    fz1 = points[i].z;
                }
                else if (points[i].z > fz2)
                {
                    fz2 = points[i].z;
                }
            }

            PageCoord minPage = new PageCoord(new Vector3(fx1, 0, fz1), pageSize);
            PageCoord maxPage = new PageCoord(new Vector3(fx2, 0, fz2), pageSize);

            PageCoord minPageOffset = minPage - cameraPage + new PageCoord(visPageRadius, visPageRadius);
            PageCoord maxPageOffset = maxPage - cameraPage + new PageCoord(visPageRadius, visPageRadius);

            // if we are outside the visible area around the camera, get out now
            if ((minPageOffset.X < 0) || (minPageOffset.Z < 0) || (maxPageOffset.X > pages.GetUpperBound(0)) || (maxPageOffset.Z > pages.GetUpperBound(1)))
            {
                return float.MinValue;
            }

            float height = float.MinValue;

            for (int pageX = minPageOffset.X; pageX <= maxPageOffset.X; pageX++)
            {
                for (int pageZ = minPageOffset.Z; pageZ <= maxPageOffset.Z; pageZ++)
                {
                    Page p = LookupPage(pageX, pageZ);

                    float pageHeight = p.TerrainPage.PageHeightMap.GetAreaHeight(fx1, fx2, fz1, fz2);

                    if (pageHeight > height)
                    {
                        height = pageHeight;
                    }
                }
            }
            return height;
        }

        // This entrypoint is invoked by the higher-level client to
        // set up the collision tile manager
        public void SetCollisionInterface(CollisionAPI API, float tileSize)
        {
            collisionTileManager = new CollisionTileManager(API, tileSize, FindObstaclesInBox);
        }
        
        // This entrypoint is invoked by the higher-level client, and
        // in turn invokes the collision tile manager operation to
        // change the collision center and the collision horizon, as
        // represented in the radius
        public void SetCollisionArea(Vector3 center, float radius)
        {
            collisionTileManager.SetCollisionArea(center, radius);
        }

		public void RecreateCollisionTiles()
		{
			if (collisionTileManager != null)
				collisionTileManager.RecreateCollisionTiles();
		}
		
		private int pageLODScanX;
        private int pageLODScanZ;
        private int subPageLODScanX;
        private int subPageLODScanZ;

        private bool doLODScan = true;

        private const int maxScanTime = 50;

        // any time the camera crosses a boundary that may change LODs, call
        // this method to force the scan to start over.
        private void ResetLODScan()
        {
            pageLODScanX = 0;
            pageLODScanZ = 0;
            subPageLODScanX = 0;
            subPageLODScanZ = 0;

            doLODScan = true;
        }

        private void UpgradeSubPageLOD()
        {
            // find the current subPage
            SubPageHeightMap subPage = LookupPage(pageLODScanX, pageLODScanZ).TerrainPage.PageHeightMap.LookupSubPage(subPageLODScanX, subPageLODScanZ);

            // get the metersPerSample required for this location
            int mps = MetersPerSample(subPage.Location);

            if (mps > minMetersPerSample)
            {
                // if we are not already at the highest LOD, then request the next higher for this subPage
                subPage.MetersPerSample = ( mps / 2 );
            }
        }

        private void LODPredictionScan()
        {
            if (doLODScan)
            {
                long startTick = Root.Instance.Timer.Milliseconds;
                long currentTick;

                int subPagesPerPage = pageSize / subPageSize;

                int subPageCount = 0;

                do
                {
                    UpgradeSubPageLOD();
                    subPageCount++;

                    // scan to the next subpage, wrapping if necessary
                    subPageLODScanX++;
                    if (subPageLODScanX >= subPagesPerPage)
                    {
                        subPageLODScanX = 0;
                        subPageLODScanZ++;

                        if (subPageLODScanZ >= subPagesPerPage)
                        {
                            subPageLODScanZ = 0;
                            pageLODScanX++;

                            if (pageLODScanX >= pageArraySize)
                            {
                                pageLODScanX = 0;
                                pageLODScanZ++;

                                if (pageLODScanZ >= pageArraySize)
                                {
                                    pageLODScanZ = 0;

                                    // we are done scanning.  don't start again until something
                                    // changes
                                    doLODScan = false;
                                }
                            }
                        }
                    }

                    currentTick = Root.Instance.Timer.Milliseconds;
                } while (doLODScan && ((currentTick - startTick) < maxScanTime));

                LogManager.Instance.Write("Upgraded LOD of {0} subPages", subPageCount);
            }
        }

        /// <summary>
        /// call each TerrainPage to update terrain patches to their correct LOD
        /// </summary>
        private void ValidateLOD()
        {
            long startTick = Root.Instance.Timer.Microseconds;
            foreach (Page p in pages)
            {
                p.TerrainPage.ValidateLOD();
            }
            long endTick = Root.Instance.Timer.Microseconds;

            LogManager.Instance.Write("ValidateLOD: {0}", endTick - startTick);
        }

        /// <summary>
        /// call each TerrainPage to update terrain patch stitches to their correct LOD
        /// </summary>
        private void ValidateStitches()
        {
            long startTick = Root.Instance.Timer.Microseconds;
            foreach (Page p in pages)
            {
                p.TerrainPage.ValidateStitches();
            }
            long endTick = Root.Instance.Timer.Microseconds;

            LogManager.Instance.Write("ValidateStitches: {0}", endTick - startTick);
        }

		public void ProcessLODChanges(Camera camera)
		{
            if (cameraTileChange)
            {
                ValidateLOD();

                ValidateStitches();

                // if the camera changes tile, reset the lod scan, but wait until next frame to
                // start a new scan
                ResetLODScan();
                cameraTileChange = false;
                LogManager.Instance.Write("Starting new LOD Scan");
            }
            else
            {
                LODPredictionScan();
            }
		}

		public bool DrawTerrain
		{
			get 
			{
				return drawTerrain;
			}
			set 
			{
				drawTerrain = value;
			}
		}

		public SceneManager SceneManager
		{
			get 
			{
				return scene;
			}
		}

        public Material WaterMaterial
        {
            get
            {
                return waterMaterial;
            }
        }

        public void ExportBoundaries(XmlTextWriter w)
        {
            if (boundaries.Count > 0)
            {
                w.WriteStartElement("boundaries");
                foreach (Boundary b in boundaries)
                {
                    b.ToXML(w);
                }
                w.WriteEndElement();
            }
        }

        private void ParseBoundary(XmlTextReader r)
        {
            if (r.Name == "boundary")
            {
                Boundary b = new Boundary(r);

                AddBoundary(b);
            }
        }

        public void ImportBoundaries(XmlTextReader r)
        {
            do
            {
                r.Read();
            } while (!((r.NodeType == XmlNodeType.Element) && (r.Name == "boundaries")));

            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    ParseBoundary(r);
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    // if we found an end element, it means we are at the end of the terrain description
                    return;
                }
            }
        }

        /// <summary>
        /// Raise the page visibility event
        /// </summary>
        /// <param name="vis">true if the page is entering visible area, false if the page is leaving visible area</param>
        /// <param name="terrainPage">the page</param>
        private void OnPageVisibility(bool vis, TerrainPage terrainPage)
        {
            PageVisibilityEventHandler pageVis = PageVisibility;
            if (pageVis != null)
            {
                pageVis(terrainPage, new PageVisibilityEventArgs(vis, terrainPage.Location));
            }
        }

		/// <summary>
		///		Nested class that implements IComparer for transparency sorting.
		/// </summary>
		class DistanceSort : IComparer 
		{
		    #region IComparer Members

			public int Compare(object x, object y) 
			{
				if(x == null  || y == null)
					return 0;

				// if they are the same, return 0
				if(x == y)
					return 0;

				Vector3 a = (Vector3)x;
				Vector3 b = (Vector3)y;

				float alen = a.LengthSquared;
				float blen = b.LengthSquared;

				if(alen == blen) 
				{
					return 0;
				}
			
                // sort descending by length, meaning further objects get drawn first
			    if(alen < blen)
			        return 1;

			    return -1;
			}

			#endregion
		}
	}

	public enum Direction 
	{
		North,
		South,
		East,
		West
	}

	public enum Quadrant
	{
		Northwest,
		Northeast,
		Southwest,
		Southeast
	}

    /// <summary>
    /// When getting the height at a location, do we interpolate between samples,
    /// truncate the coordinates to the sample closest to 0, or pick the height
    /// at the closest sample.
    /// </summary>
    public enum GetHeightMode
    {
        Interpolate,
        Truncate,
        Closest
    }

    /// <summary>
    /// When getting the height at a location, this flag indicates whether we want to
    /// force the maximum LOD for the height lookup, or just use the current LOD.
    /// </summary>
    public enum GetHeightLOD
    {
        CurrentLOD,
        MaxLOD
    }

    /// <summary>
    /// This class is used to pass args for the event generated when pages enter
    /// and leave the visible region around the camera.
    /// </summary>
    public class PageVisibilityEventArgs : EventArgs
    {
        public readonly bool visible;
        public readonly Vector3 pageLocation;

        public PageVisibilityEventArgs(bool vis, Vector3 loc)
        {
            visible = vis;
            pageLocation = loc;
        }
    }

    public delegate void PageVisibilityEventHandler(object sender, PageVisibilityEventArgs msg);

    public delegate void CameraLocationEventHandler(object sender, CameraLocationEventArgs msg);

    public class CameraLocationEventArgs : EventArgs
    {
        public Vector3 oldLocation;
        public Vector3 newLocation;

        public CameraLocationEventArgs(Vector3 oldLocation, Vector3 newLocation)
        {
            this.oldLocation = oldLocation;
            this.newLocation = newLocation;
        }
    }
}
