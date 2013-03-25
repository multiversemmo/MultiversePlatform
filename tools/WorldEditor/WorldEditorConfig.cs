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
using System.Collections.Generic;
using System.Text;
using Axiom.MathLib;
using Axiom.Core;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    public class WorldEditorConfig
    {
        protected bool displayMarkerPoints = true;
        protected string markerPointMeshName = "directional_marker.mesh";
        protected string markerPointMaterial = "directional_marker.orange";
        protected float markerPointScale = 1f;
        protected string markerCustomMaterial = "directional_marker.custom";

        protected bool displayRoadPoints = true;
        protected string roadPointMeshName = "directional_marker.mesh";
        protected string roadPointMaterial = "directional_marker.yellow";

        protected bool displayRegionPoints = true;
        protected bool displayRegionHighlights = true;
        protected string regionPointMeshName = "directional_marker.mesh";
        protected string regionPointMaterial = "directional_marker.red";
        protected int defaultBoundaryPriority = 50;

		protected string forestSpeedWindFileDefault = "demoWind.ini";
		protected int forestSeedDefault = 0;
		protected float forestWindSpeedDefault = 0.3f;
		protected Vector3 forestWindDirectionDefault = new Vector3(1, 0, 0);
        protected string speedTreeLicenseLink = "http://update.multiverse.net/wiki/index.php/SpeedTree_Licensing";

		protected float treeScaleDefault = 10000f;
		protected float treeScaleVarianceDefault = 1000f;
        protected int defaultTreeInstances = 50;

		protected ColorEx fogColorDefault = new ColorEx(0.5f, 0.578125f, 0.76953125f);
		protected float fogNearDefault = 500000f;
		protected float fogFarDefault = 1000000f;

        protected float waterHeightDefault = 20000f;

		protected uint instancesDefault = 100;
		protected float scaleWidthLowDefault = 1000f;
		protected float scaleWidthHiDefault = 1000f;
		protected float scaleHeightLowDefault = 1000f;
		protected float scaleHeightHiDefault = 1000f;
		protected ColorEx plantColorDefault = ColorEx.White;
		protected float colorMultLowDefault = 1.0f;
		protected float colorMultHiDefault = 1.0f;
		protected float windMagnitudeDefault = 100f;

        protected float oneMeter = 1000f;

        protected Vector3 cameraFocusOffset;

        protected int xmlSaveFileVersion = 2;

        protected bool showTypeLabelsInTreeView = true;

        protected float particleFXVelocityScaleDefault = 1;
        protected float particleFXPositionScaleDefault = 1;
        protected bool displayParticleEffects = true;

		protected uint mobNumSpawnDefault = 1;
		protected int mobRespawnTimeDefault = 0;
		protected float mobSpawnRadiusDefault = 0;

        protected int errorDisplayTimeDefault = 60000;

        protected string helpBaseURL = "http://update.multiverse.net/wiki/index.php/Using_World_Editor_Version_1.5";
        protected string feedbackBaseURL = "http://update.multiverse.net/custportal/login.php";
        protected string relaseNotesURL = "http://update.multiverse.net/wiki/index.php/Tools_Version_1.5_Release_Notes";

        protected int defaultErrorDisplayLength = 15000;

        protected bool defaultDisplayOcean = true;
        protected float defaultSeaLevel = 10000;
        protected float defaultWaveHeight = 1000;

        protected ColorEx defaultOceanDeepColor = new ColorEx(1, 0, 0, 0.1f);
        protected ColorEx defaultOceanShallowColor = new ColorEx(1, 0, 0.3f, 0.5f);

        protected float defaultOceanBumpScale = 0.5f;
        protected float defaultOceanTextureScaleX = 0.015625f;
        protected float defaultOceanTextureScaleZ = 0.0078125f;
        protected float defaultOceanBumpSpeedX = -0.005f;
        protected float defaultOceanBumpSpeedZ = 0;
        protected bool defaultOceanUseParams = true;

        protected string pointLightMeshName = "Point Light Marker";
        protected string pointLightCircleMeshName = "world_editor_light_ring";
        protected float defaultPointLightHeight = 2000f;
        protected ColorEx defaultPointLightSpecular = ColorEx.White;
        protected ColorEx defaultPointLightdiffuse = ColorEx.White;
        protected ColorEx defaultAmbientLightColor = new ColorEx(0.8f, 0.8f, 0.8f);
        protected float defaultDirectionalLightAzimuth = 0;
        protected float defaultDirectionalLightZenith = 40;
        protected Vector3 defaultDirectionalLightDirection = new Vector3(0f, -0.6427876f, -0.7660445f);
        protected ColorEx defaultGlobalDirectionalLightSpecular = new ColorEx(0.875f, 0.953125f, 0.875f);
        protected ColorEx defaultGlobalDirectionalLightDiffuse = new ColorEx(.78125f, .7890625f, .5859375f);
        protected ColorEx defaultBoundaryDirectionalLightDiffuse = new ColorEx(.78125f, .7890625f, .5859375f);
        protected ColorEx defaultBoundaryDirectionalLightSpecular = new ColorEx(0.875f, 0.953125f, 0.875f);
        protected string directionalLightMeshName = "Directional Light Marker";

        protected float terrainDecalDefaultSize = 1000f;
        protected int terrainDecalDefaultPriority = 1;

        protected float defaultSoundGain = 1f;
        protected bool defaultSoundLooping = true;
        protected float defaultSoundMinAttenuationDistance = 1000f;
        protected float defaultSoundMaxAttenuationDistance = 1000000f;

        protected static string worldEditorBaseRegistryKey = "HKEY_CURRENT_USER\\Software\\Multiverse\\WorldEditor";
        protected string assetRepositoryBaseRegistryKey = "HKEY_CURRENT_USER\\Software\\Multiverse\\AssetRepository";
        protected string recentFileListBaseRegistryKey = worldEditorBaseRegistryKey + "\\RecentFiles";

        protected string commandBindingsFilePath = ".\\KeyBindings.xml";
        protected string altCommandBindingsFilePath = ".\\MyBindings.xml";
        protected string commandBindingEventsFilePath = ".\\CommandEvents.xml";
        protected static string[] contextarray = { "world view", "global", "treeView" };
        protected static string[] excludedKey = { "F", "E", "H", "V" };
        protected static string[] excludedKeyModifiers = { "Alt", "Alt", "Alt", "Alt" };
        protected static string[] mouseCapableContexts = { "world view" };
        protected List<string> context = new List<string>(contextarray);

        #region camera control defaults
        protected float defaultCamSpeed = 7000f;
        protected float defaultCamSpeedIncrement = 2000f;
        protected float defaultPresetCamSpeed1 = 7000f;
        protected float defaultPresetCamSpeed2 = 15000f;
        protected float defaultPresetCamSpeed3 = 20000f;
        protected float defaultPresetCamSpeed4 = 30000f;
        protected bool defaultAccelerateCamera = true;
        protected float defaultCamAccelRate = 2000f;
        protected float defaultCamAccelRateIncrement = 1000f;
        protected float defaultPresetCamAccel1 = 2000f;
        protected float defaultPresetCamAccel2 = 5000f;
        protected float defaultPresetCamAccel3 = 7000f;
        protected float defaultPresetCamAccel4 = 10000f;
        protected float defaultCameraTurnRate = 10f;
        protected float defaultMouseWheelMultiplier = 0.1f;
        protected float defaultPresetMWM1 = .001f;
        protected float defaultPresetMWM2 = .01f;
        protected float defaultPresetMWM3 = .1f;
        protected float defaultPresetMWM4 = 1f;

        #endregion camera control defaults


        public WorldEditorConfig()
        {
            cameraFocusOffset = new Vector3(0, 100 * oneMeter, 50 * oneMeter);
        }

        #region Marker Point Properties

        public bool DisplayMarkerPoints
        {
            get
            {
                return displayMarkerPoints;
            }
            set
            {
                displayMarkerPoints = value;
            }
        }

        public string MarkerPointMeshName
        {
            get
            {
                return markerPointMeshName;
            }
            set
            {
                markerPointMeshName = value;
            }
        }   

        public string MarkerPointMaterial
        {
            get
            {
                return markerPointMaterial;
            }
            set
            {
                markerPointMaterial = value;
            }
        }

        public string MarkerPointCustomMaterial
        {
            get
            {
                return markerCustomMaterial;
            }
        }

        public float MarkerPointScale
        {
            get
            {
                return markerPointScale;
            }
            set
            {
                markerPointScale = value;
            }
        }
        #endregion Marker Point Properties

        #region Road Properties

        public bool DisplayRoadPoints
        {
            get
            {
                return displayRoadPoints;
            }
            set
            {
                displayRoadPoints = value;
            }
        }

        public string RoadPointMeshName
        {
            get
            {
                return roadPointMeshName;
            }
            set
            {
                roadPointMeshName = value;
            }
        }

        public string RoadPointMaterial
        {
            get
            {
                return roadPointMaterial;
            }
            set
            {
                roadPointMaterial = value;
            }
        }
        #endregion Road Properties

        #region Region Properties

        public bool DisplayRegionPoints
        {
            get
            {
                return displayRegionPoints;
            }
            set
            {
                displayRegionPoints = value;
            }
        }

        public bool DisplayRegionHighlights
        {
            get
            {
                return displayRegionHighlights;
            }
            set
            {
                displayRegionHighlights = value;
            }
        }

        public string RegionPointMeshName
        {
            get
            {
                return regionPointMeshName;
            }
            set
            {
                regionPointMeshName = value;
            }
        }

        public string RegionPointMaterial
        {
            get
            {
                return regionPointMaterial;
            }
            set
            {
                regionPointMaterial = value;
            }
        }

        public int DefaultBoundaryPriority
        {
            get
            {
                return defaultBoundaryPriority;
            }
            set
            {
                defaultBoundaryPriority = value;
            }
        }

        #endregion Region Properties

        #region Mob Properties

        public uint MobNumSpawnDefault
        {
            get
            {
        
                return mobNumSpawnDefault;
            }
            set
            {
                mobNumSpawnDefault = value;
            }
        }

        public int MobRespawnTimeDefault
        {
            get
            {
                return mobRespawnTimeDefault;
            }
            set
            {
                mobRespawnTimeDefault = value;
            }
        }

        public float MobSpawnRadiusDefault
        {
            get
            {
                return mobSpawnRadiusDefault;
            }
            set
            {
                mobSpawnRadiusDefault = value;
            }
        }
        #endregion

        #region Forest Properties
        public string ForestSpeedWindFileDefault
		{
			get
			{
				return forestSpeedWindFileDefault;
			}
			set
			{
				forestSpeedWindFileDefault = value;
			}
		}

		public int ForestSeedDefault
		{
			get
			{
				return forestSeedDefault;
			}
			set
			{
				forestSeedDefault = value;
			}
		}

		public float ForestWindSpeedDefault
		{
			get
			{
				return forestWindSpeedDefault;
			}
			set
			{
				forestWindSpeedDefault = value;
			}
		}

		public Vector3 ForestWindDirectionDefault
		{
			get
			{
				return forestWindDirectionDefault;
			}
			set
			{
				forestWindDirectionDefault = value;
			}
		}

        public string SpeedTreeLicenseLink
        {
            get
            {
                return speedTreeLicenseLink;
            }
        }

		#endregion Forest Properties

		#region Tree Properties

		public float TreeScaleDefault
		{
			get
			{
				return treeScaleDefault;
			}
			set
			{
				treeScaleDefault = value;
			}
		}

		public float TreeScaleVarianceDefault
		{
			get
			{
				return treeScaleVarianceDefault;
			}
			set
			{
				treeScaleVarianceDefault = value;
			}
		}

        public int DefaultTreeInstances
        {
            get
            {
                return defaultTreeInstances;
            }
        }

		#endregion end Tree Properties

		#region Fog Properties

		public ColorEx FogColorDefault
		{
			get
			{
				return fogColorDefault;
			}
			set
			{
				fogColorDefault = value;
			}
		}

		public float FogNearDefault
		{
			get
			{
				return fogNearDefault;
			}
			set
			{
				fogNearDefault = value;
			}	
		}

		public float FogFarDefault
		{
			get
			{
				return fogFarDefault;
			}
			set
			{
				fogFarDefault = value;
			}	
		}

		#endregion end Fog Properties

		#region PlantType Properties

		public uint InstancesDefault
		{
			get
			{
				return instancesDefault;
			}
			set
			{
				instancesDefault = value;
			}
		}

		public float ScaleWidthLowDefault
		{
			get
			{
				return scaleWidthLowDefault;
			}
			set
			{
				scaleWidthLowDefault = value;
			}
		}

		public float ScaleWidthHiDefault
		{
			get
			{
				return scaleWidthHiDefault;
			}
			set
			{
				scaleWidthHiDefault = value;
			}
		}

		public float ScaleHeightLowDefault
		{
			get
			{
				return scaleHeightLowDefault;
			}
			set
			{
				scaleHeightLowDefault = value;
			}
		}

		public float ScaleHeightHiDefault
		{
			get
			{
				return scaleHeightHiDefault;
			}
			set
			{
				scaleHeightHiDefault = value;
			}
		}

		public ColorEx PlantColorDefault
		{
			get
			{
				return plantColorDefault;
			}
			set
			{
				plantColorDefault = value;
			}
		}
		public float ColorMultLowDefault
		{
			get
			{
				return colorMultLowDefault;
			}
			set
			{
				colorMultLowDefault = value;
			}
		}
		public float ColorMultHiDefault
		{
			get
			{
				return colorMultHiDefault;
			}
			set
			{
				colorMultHiDefault = value;
			}
		}
		public float WindMagnitudeDefault
		{
			get
			{
				return windMagnitudeDefault;
			}
			set
			{
				windMagnitudeDefault = value;
			}
		}


		#endregion Plant Type Properties

		#region Camera Related Properties

		public Vector3 CameraFocusOffset
        {
            get
            {
                return cameraFocusOffset;
            }
            set
            {
                cameraFocusOffset = value;
            }
        }

        #endregion Camera Related Properties

        #region Point Light Properties

        public string PointLightMeshName
        {
            get
            {
                return pointLightMeshName;
            }
        }

        public string PointLightCircleMeshName
        {
            get
            {
                return pointLightCircleMeshName;
            }
        }

        public float DefaultPointLightHeight
        {
            get
            {
                return defaultPointLightHeight;
            }
            set
            {
                defaultPointLightHeight = value;
            }
        }

        public ColorEx DefaultPointLightSpecular
        {
            get
            {
                return defaultPointLightSpecular;
            }
            set
            {
                defaultPointLightSpecular = value;
            }
        }

        public ColorEx DefaultPointLightDiffuse
        {
            get
            {
                return defaultPointLightdiffuse;
            }
            set
            {
                defaultPointLightdiffuse = value;
            }
        }

        #endregion Point Light Properties


        #region Directional Light Properties

        public ColorEx DefaultGlobalDirectionalLightSpecular
        {
            get
            {
                return defaultGlobalDirectionalLightSpecular;
            }
            set
            {
                defaultGlobalDirectionalLightSpecular = value;
            }
        }

        public Vector3 DefaultDirectionalLightDirection
        {
            get
            {
                return defaultDirectionalLightDirection;
            }
        }


        public ColorEx DefaultGlobalDirectionalLightDiffuse
        {
            get
            {
                return defaultGlobalDirectionalLightDiffuse;
            }
            set
            {
                defaultGlobalDirectionalLightDiffuse = value;
            }
        }
        public ColorEx DefaultBoundaryDirectionalLightDiffuse
        {
            get
            {
                return defaultBoundaryDirectionalLightDiffuse;
            }
            set
            {
                defaultBoundaryDirectionalLightDiffuse = value;
            }
        }
        public ColorEx DefaultBoundaryDirectionalLightSpecular
        {
            get
            {
                return defaultBoundaryDirectionalLightSpecular;
            }
            set
            {
                defaultBoundaryDirectionalLightSpecular = value;
            }
        }


        public string DirectionalLightMeshName
        {
            get
            {
                return directionalLightMeshName;
            }
            set
            {
                directionalLightMeshName = value;
            }
        }

        public float DefaultDirectionalLightAzimuth
        {
            get
            {
                return defaultDirectionalLightAzimuth;
            }
        }

        public float DefaultDirectionalLightZenith
        {
            get
            {
                return defaultDirectionalLightZenith;
            }
        }

        #endregion Directional Light Properties


        #region AmbientLight Properties

        public ColorEx DefaultAmbientLightColor
        {
            get
            {
                return defaultAmbientLightColor;
            }
            set
            {
                defaultAmbientLightColor = value;
            }
        }
        #endregion AmbientLight Properties


        #region Sound Properties
        public float DefaultSoundGain
        {
            get
            {
                return defaultSoundGain;
            }
        }

        public bool DefaultSoundLooping
        {
            get
            {
                return defaultSoundLooping;
            }
        }

        public float DefaultSoundMinAttenuationDistance
        {
            get
            {
                return defaultSoundMinAttenuationDistance;
            }
        }

        public float DefaultSoundMaxAttenuationDistance
        {
            get
            {
                return defaultSoundMaxAttenuationDistance;
            }
        }

        #endregion Sound Properties

        #region Water Properties

        public float WaterHeightDefault
        {
            get
            {
                return waterHeightDefault;
            }
        }

        #endregion Water Properties

        #region TerrainDecal Properties
        public float TerrainDecalDefaultSize
        {
            get
            {
                return terrainDecalDefaultSize;
            }
        }

        public int TerrainDecalDefaultPriority
        {
            get
            {
                return terrainDecalDefaultPriority;
            }
        }

        #endregion TerrainDecal Properties

        #region XML File Properties

        public int XmlSaveFileVersion
        {
            get
            {
                return xmlSaveFileVersion;
            }
            set
            {
                xmlSaveFileVersion = value;
            }
        }

        #endregion // XML File Properties

        public bool ShowTypeLabelsInTreeView
        {
            get
            {
                return showTypeLabelsInTreeView;
            }
            set
            {
                showTypeLabelsInTreeView = value;
            }
        }

        #region Particle Effects

        public float ParticleFXVelocityScaleDefault
        {
            get
            {
                return particleFXVelocityScaleDefault;
            }
            set
            {
                particleFXVelocityScaleDefault = value;
            }
        }

        public float ParticleFXPositionScaleDefault
        {
            get
            {
                return particleFXPositionScaleDefault;
            }
            set
            {
                particleFXPositionScaleDefault = value;
            }
        }

        public bool DisplayParticleEffects
        {
            get
            {
                return displayParticleEffects;
            }
            set
            {
                displayParticleEffects = value;
            }
        }

        #endregion Particle Effects


        #region Help
        public string HelpBaseURL
        {
            get
            {
                return helpBaseURL;
            }
        }

		public string FeedbackBaseURL
		{
			get
			{
				return feedbackBaseURL;
			}
		}

		public string ReleaseNotesURL
		{
			get
			{
				return relaseNotesURL;
			}
		}

        #endregion Help

        #region Error Reporting

        public int ErrorDisplayTimeDefault
        {
            get
            {
                return errorDisplayTimeDefault;
            }
            set
            {
                errorDisplayTimeDefault = value;
            }
        }

        #endregion Error Reporting

        #region Ocean Defaults
        public bool DefaultDisplayOcean
        {
            get
            {
                return defaultDisplayOcean;
            }
            set
            {
                defaultDisplayOcean = value;
            }
        }

        public float DefaultSeaLevel
        {
            get
            {
                return defaultSeaLevel;
            }
            set
            {
                defaultSeaLevel = value;
            }
        }

        public float DefaultWaveHeight
        {
            get
            {
                return defaultWaveHeight;
            }
            set
            {
                defaultWaveHeight = value;
            }
        }

        public ColorEx DefaultOceanDeepColor
        {
            get
            {
                return defaultOceanDeepColor;
            }
            set
            {
                defaultOceanDeepColor = value;
            }
        }

        public ColorEx DefaultOceanShallowColor
        {
            get
            {
                return defaultOceanShallowColor;
            }
            set
            {
                defaultOceanShallowColor = value;
            }
        }

        public float DefaultOceanBumpScale
        {
            get
            {
                return defaultOceanBumpScale;
            }
            set
            {
                DefaultOceanBumpScale = value;
            }
        }

        public float DefaultOceanTextureScaleX
        {
            get
            {
                return defaultOceanTextureScaleX;
            }
            set
            {
                defaultOceanTextureScaleX = value;
            }
        }

        public float DefaultOceanTextureScaleZ
        {
            get
            {
                return defaultOceanTextureScaleZ;
            }
            set
            {
                defaultOceanTextureScaleZ = value;
            }
        }

        public float DefaultOceanBumpSpeedX
        {
            get
            {
                return defaultOceanBumpSpeedX;
            }
            set
            {
                defaultOceanBumpSpeedX = value;
            }
        }

        public float DefaultOceanBumpSpeedZ
        {
            get
            {
                return defaultOceanBumpSpeedZ;
            }
            set
            {
                defaultOceanBumpSpeedZ = value;
            }
        }

        public bool DefaultOceanUseParams
        {
            get
            {
                return defaultOceanUseParams;
            }
            set
            {
                defaultOceanUseParams = value;
            }
        }

        #endregion Ocean Defaults

        #region Registry Defaults

        public string WorldEditorBaseRegistryKey
        {
            get
            {
                return worldEditorBaseRegistryKey;
            }
        }

        public string AssetRepositoryBaseRegistryKey
        {
            get
            {
                return assetRepositoryBaseRegistryKey;
            }
        }

        public string RecentFileListBaseRegistryKey
        {
            get
            {
                return recentFileListBaseRegistryKey;
            }
        }

        #endregion Registry Defaults

        #region Command Bindings
        public string CommandBindingsFilePath
        {
            get
            {
                return commandBindingsFilePath;
            }
        }

        public string AltCommandBindingsFilePath
        {
            get
            {
                return altCommandBindingsFilePath;
            }
        }



        public List<string> Context
        {
            get
            {
                return context;
            }
        }

        public List<ExcludedKey> ExcludedKeys
        {
            get
            {
                List<ExcludedKey> excludedKeys = new List<ExcludedKey>();
                int i = 0;
                foreach (string key in excludedKey)
                {
                    ExcludedKey exKey = new ExcludedKey(key, excludedKeyModifiers[i]);
                    excludedKeys.Add(exKey);
                    i++;
                }
                return excludedKeys;
            }
        }

        public List<string> MouseCapableContexts
        {
            get
            {
                return new List<string>(mouseCapableContexts);
            }
        }

        public string CommandBindingEventsFilePath
        {
            get
            {
                return commandBindingEventsFilePath;
            }
        }

        #endregion CommandBingings 

        #region Camera Control Defaults

        public float DefaultCamAccelRate
        {
            get
            {
                return defaultCamAccelRate;
            }
        }

        public float DefaultCamSpeed
        {
            get
            {
                return defaultCamSpeed;
            }
        }

        public float DefaultCamSpeedIncrement
        {
            get
            {
                return defaultCamSpeedIncrement;
            }
        }

        public float DefaultCamAccelRateIncrement
        {
            get
            {
                return defaultCamAccelRateIncrement;
            }
        }

        public float DefaultPresetCamSpeed1
        {
            get
            {
                return defaultPresetCamSpeed1;
            }
        }

        public float DefaultPresetCamSpeed2
        {
            get
            {
                return defaultPresetCamSpeed2;
            }
        }

        public float DefaultPresetCamSpeed3
        {
            get
            {
                return defaultPresetCamSpeed3;
            }
        }

        public float DefaultPresetCamSpeed4
        {
            get
            {
                return defaultPresetCamSpeed4;
            }
        }

        public float DefaultPresetCamAccel1
        {
            get
            {
                return defaultPresetCamAccel1;
            }
        }

        public float DefaultPresetCamAccel2
        {
            get
            {
                return defaultPresetCamAccel2;
            }
        }

        public float DefaultPresetCamAccel3
        {
            get
            {
                return defaultPresetCamAccel3;
            }
        }

        public float DefaultPresetCamAccel4
        {
            get
            {
                return defaultPresetCamAccel4;
            }
        }

        public bool DefaultAccelerateCamera
        {
            get
            {
                return defaultAccelerateCamera;
            }
        }

        public float DefaultCameraTurnRate
        {
            get
            {
                return defaultCameraTurnRate;
            }
        }

        public float DefaultMouseWheelMultiplier
        {
            get
            {
                return defaultMouseWheelMultiplier;
            }
        }

        public float DefaultPresetMWM1
        {
            get
            {
                return defaultPresetMWM1;
            }
        }

        public float DefaultPresetMWM2
        {
            get
            {
                return defaultPresetMWM2;
            }
        }

        public float DefaultPresetMWM3
        {
            get
            {
                return defaultPresetMWM3;
            }
        }

        public float DefaultPresetMWM4
        {
            get
            {
                return defaultPresetMWM4;
            }
        }

        #endregion Camera Control Defaults
    }
}
