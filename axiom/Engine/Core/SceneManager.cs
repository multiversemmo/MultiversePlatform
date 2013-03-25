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
using System.Collections.Generic;
using System.Diagnostics;
using Axiom.Animating;
using Axiom.Collections;
using Axiom.Configuration;
using Axiom.Controllers;
using Axiom.Overlays;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Utility;
using Axiom.Media;
using Axiom.ParticleSystems;

namespace Axiom.Core {

	#region Delegate declarations
	/// <summary>
	///		Delegate for speicfying the method signature for a render queue event.
	/// </summary>
	public delegate bool RenderQueueEvent(RenderQueueGroupID priority);

    public delegate void AmbientLightChangeHandler(ColorEx newColor);

    public delegate void NamedLightEventHandler(string lightName);

	#endregion

	/// <summary>
	///     Manages the rendering of a 'scene' i.e. a collection of primitives.
	/// </summary>
	/// <remarks>
	///		This class defines the basic behavior of the 'Scene Manager' family. These classes will
	///		organize the objects in the scene and send them to the rendering system, a subclass of
	///		RenderSystem. This basic superclass only does basic bounding box frustum culling.
	///    <p/>
	///		Subclasses may use various techniques to organise the scene depending on how they are
	///		designed (e.g. BSPs, octrees etc). As with other classes, methods marked as internal are 
	///		designed to be called by other classes in the engine, not by user applications.
	///	 </remarks>
	/// TODO: Thoroughly review node removal/cleanup.
	/// TODO: Review of method visibility/virtuality to ensure consistency.
	public class SceneManager {
		#region Fields
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SceneManager));

        /// <summary>Instance name.</summary>
		protected string smName;
        /// <summary>A queue of objects for rendering.</summary>
		protected RenderQueue renderQueue;
		/// <summary>A reference to the current active render system..</summary>
		protected RenderSystem targetRenderSystem;
		/// <summary>Denotes whether or not the camera has been changed.</summary>
		protected bool hasCameraChanged;
		/// <summary>The ambient color, cached from the RenderSystem</summary>
		/// <remarks>Default to a semi-bright white (gray) light to prevent being null</remarks>
		protected ColorEx ambientColor = ColorEx.Gray;
		/// <summary>A list of the valid cameras for this scene for easy lookup.</summary>
		protected Dictionary<string, Camera> cameraList;
		public ICollection<Camera> Cameras { get { return cameraList.Values; } }
		/// <summary>A list of entities in the scene for easy lookup.</summary>
		protected internal Dictionary<string, Dictionary<string, MovableObject>> movableObjectCollectionMap;
        public ICollection<Dictionary<string, MovableObject>> MovableObjectMaps { get { return movableObjectCollectionMap.Values; } }
		/// <summary>A list of scene nodes (includes all in the scene graph).</summary>
		protected Dictionary<string, SceneNode> sceneNodeList;
		public ICollection<SceneNode> SceneNodes { get { return sceneNodeList.Values; } }
		/// <summary>A list of animations for easy lookup.</summary>
		protected Dictionary<string, Animation> animationList;
		public ICollection<Animation> Animations { get { return animationList.Values; } }
		/// <summary>A list of animation states for easy lookup.</summary>
		protected AnimationStateSet animationStateList;
		//public ICollection AnimationStates { get { return animationStateList.Values; } }
        /// <summary>
        ///    A list of the Regions.
        ///    TODO: Is there any point to having this region list?
        /// </summary>
        protected List<Region> regionList;

		protected Matrix4[] xform = new Matrix4[256];

		/// <summary>A reference to the current camera being used for rendering.</summary>
		protected Camera cameraInProgress;
		/// <summary>The root of the scene graph heirarchy.</summary>
		protected SceneNode rootSceneNode;
		/// <summary>
		///    Utility class for updating automatic GPU program params.
		/// </summary>
		protected AutoParamDataSource autoParamDataSource = new AutoParamDataSource();

		protected Entity skyPlaneEntity;
		protected Entity[] skyDomeEntities = new Entity[5];
		protected Entity[] skyBoxEntities = new Entity[6];

		protected SceneNode skyPlaneNode;
		protected SceneNode skyDomeNode;
		protected SceneNode skyBoxNode;
		// Sky plane
		protected bool isSkyPlaneEnabled;
		protected bool isSkyPlaneDrawnFirst;
		protected Plane skyPlane;
		// Sky box
		protected bool isSkyBoxEnabled;
		protected bool isSkyBoxDrawnFirst;
		protected Quaternion skyBoxOrientation;
		// Sky dome
		protected bool isSkyDomeEnabled;
		protected bool isSkyDomeDrawnFirst;
		protected Quaternion skyDomeOrientation;
		// Fog
		protected FogMode fogMode;
		protected ColorEx fogColor;
		protected float fogStart;
		protected float fogEnd;
		protected float fogDensity;

		/// <summary>Flag indicating whether SceneNodes will be rendered as a set of 3 axes.</summary>
		protected bool displayNodes;
		/// <summary>Flag that specifies whether scene nodes will have their bounding boxes rendered as a wire frame.</summary>
		protected bool showBoundingBoxes;

		/// <summary>Hashtable of options that can be used by this or any other scene manager.</summary>
		protected Hashtable optionList = new Hashtable();

		protected bool lastUsedFallback;
		protected static int lastNumTexUnitsUsed = 0;
		protected static RenderOperation op = new RenderOperation();
		/// <summary>
		///    Local light list for use during rendering passes.
		/// </summary>
        protected static List<Light> localLightList = new List<Light>();
		/// <summary>
		///    Whether normals are currently being normalized.
		/// </summary>
		protected static bool normalizeNormals;

// 		protected bool lastViewWasIdentity, lastProjectionWasIdentity;

 		protected bool resetIdentityView = false;
 		protected bool resetIdentityProj = false;
        
		/// <summary>
		///		Active list of nodes tracking other nodes.
		/// </summary>
		protected List<SceneNode> autoTrackingSceneNodes = new List<SceneNode>();

		/// <summary>
		///		Current shadow technique in use in the scene.
		/// </summary>
		protected ShadowTechnique shadowTechnique;
		/// <summary>
		///		If true, the shadow technique is based on texture maps
		/// </summary>
		public bool IsShadowTechniqueStencilBased {
			get { return shadowTechnique == ShadowTechnique.StencilModulative || shadowTechnique == ShadowTechnique.StencilAdditive; }
		}
		/// <summary>
		///		If true, the shadow technique is based on texture maps
		/// </summary>
		public bool IsShadowTechniqueTextureBased {
			get { return shadowTechnique == ShadowTechnique.TextureModulative || shadowTechnique == ShadowTechnique.TextureAdditive; }
		}
		/// <summary>
		///		If true, the shadow technique is additive
		/// </summary>
		public bool IsShadowTechniqueAdditive {
			get { return shadowTechnique == ShadowTechnique.StencilAdditive || shadowTechnique == ShadowTechnique.TextureAdditive; }
		}
		/// <summary>
		///		If true, the shadow technique is modulative
		/// </summary>
		protected bool IsShadowTechniqueModulative {
			get { return shadowTechnique == ShadowTechnique.StencilModulative || shadowTechnique == ShadowTechnique.TextureModulative; }
		}
		/// <summary>
		///		Is the shadow technique is not "None"
		/// </summary>
		public bool IsShadowTechniqueInUse {
			get { return shadowTechnique != ShadowTechnique.None; }
		}

		/// <summary>
		///		If true, shadow volumes will be visible in the scene.
		/// </summary>
		protected bool showDebugShadows;
		/// <summary>
		///		Pass to use for rendering debug shadows.
		/// </summary>
		protected Pass shadowDebugPass;
		/// <summary>
		///		
		/// </summary>
		protected Pass shadowStencilPass;
		/// <summary>
		///		Pass to use while rendering the full screen quad for modulative shadows.
		/// </summary>
		protected Pass shadowModulativePass;
		/// <summary>
		///		A pass designed to let us render shadow colour on white for texture shadows
		/// </summary>
		protected Pass shadowCasterPlainBlackPass;
		/// <summary>
		///		A pass designed to let us render shadow receivers for texture shadows
		/// </summary>
		protected Pass shadowReceiverPass;
		/// <summary>
		///		List of lights in view that could cast shadows.
		/// </summary>
        protected List<Light> lightsAffectingFrustum = new List<Light>();
		/// <summary>
		///		Full screen rectangle to use for rendering stencil shadows.
		/// </summary>
		protected Rectangle2D fullScreenQuad;
		/// <summary>
		///		buffer to use for indexing shadow geometry required for certain techniques.
		/// </summary>
		protected HardwareIndexBuffer shadowIndexBuffer;
		/// <summary>
		///		Current list of shadow casters within the view of the camera.
		/// </summary>
		protected ArrayList shadowCasterList = new ArrayList();
		/// <summary>
		///		Explicit extrusion distance for directional lights.
		/// </summary>
		protected float shadowDirLightExtrudeDist;
		/// <summary>
		///		Sphere region query to find shadow casters within the attenuation range of a point/spot light.
		/// </summary>
		protected SphereRegionSceneQuery shadowCasterSphereQuery;
		/// <summary>
		/// 
		/// </summary>
		protected ColorEx shadowColor;
		/// <summary>
		///		AxisAlignedBox region query to find shadow casters within the attenuation range of a directional light.
		/// </summary>
		protected AxisAlignedBoxRegionSceneQuery shadowCasterAABBQuery;
		/// <summary>
		///		Listener to use when finding shadow casters for a light within a scene.
		/// </summary>
		protected ShadowCasterSceneQueryListener shadowCasterQueryListener = new ShadowCasterSceneQueryListener();
		/// <summary>
		///		Farthest distance from the camera at which to render shadows.
		/// </summary>
		protected float shadowFarDistance;
		/// <summary>
		///		shadowFarDistance ^ 2
		/// </summary>
		protected float shadowFarDistanceSquared;
		/// <summary>
		///		The maximum size of the index buffer used to render shadow primitives.
		/// </summary>
		protected int shadowIndexBufferSize;
		/// <summary>
		///		Current stage of rendering.
		/// </summary>
		protected IlluminationRenderStage illuminationStage;
        /// <summary>
        /// True when calling RenderSolidObjects with the noShadow queue.
        /// </summary>
        protected bool renderingNoShadowQueue;
        /// <summary>
        /// True when the main priority group is rendering.
        /// </summary>
        protected bool renderingMainGroup;
		/// <summary>
		///		Proportion of texture offset in view direction e.g. 0.4
		/// </summary>
		protected float shadowTextureOffset;
		/// <summary>
		///		As a proportion e.g. 0.6
		/// </summary>
		protected float shadowTextureFadeStart;
		/// <summary>
		///		As a proportion e.g. 0.9
		/// </summary>
		protected float shadowTextureFadeEnd;
		/// <summary>
		///     Cached texture size.
		/// </summary>
		protected int shadowTextureSize;
		/// <summary>
		///		A list of shadow texture configurations
		/// </summary>
		protected List<ShadowTextureConfig> shadowTextureConfigList = new List<ShadowTextureConfig>();
		/// <summary>
		///		If true, we need to recreate the shadow textures
		/// </summary>
		protected bool shadowTextureConfigDirty;
		/// <summary>
		///		Current list of shadow textures.
		/// </summary>
        protected List<Texture> shadowTextures = new List<Texture>();
		/// <summary>
		///		The null shadow texture
		/// </summary>
        protected Texture nullShadowTexture = null;
        /// <summary>
        ///     Current list of shadow texture cameras.  There is one camera 
        ///     for each shadow texture.
        /// </summary>
        protected List<Camera> shadowTextureCameras = new List<Camera>();
		/// <summary>
		///		Whether we should override far distance when using stencil volumes
		/// </summary>
		protected bool shadowUseInfiniteFarPlane;
		/// <summary>
		///		Whether shadow casters should be rendered into shadow
        ///     textures using their back faces rather than their front faces.
		/// </summary>
		/// <remarks>
        ///     Rendering back faces rather than front faces into a shadow texture
        ///     can help minimise depth comparison issues, if you're using depth
        ///     shadowmapping. You will probably still need some biasing but you
        ///     won't need as much. For solid objects the result is the same anyway,
        ///     if you have objects with holes you may want to turn this option off.
        ///     The default is to enable this option.
        /// </remarks>
        protected bool shadowCasterRenderBackFaces;
		/// <summary>
		///	    The default implementation of texture shadows uses a fixed-function 
		///    	colour texture projection approach for maximum compatibility, and 
		///     as such cannot support self-shadowing. However, if you decide to 
		///	    implement a more complex shadowing technique using 
		///	    ShadowTextureCasterMaterial and ShadowTextureReceiverMaterial 
		///	    there is a possibility you may be able to support 
		///	    self-shadowing (e.g by implementing a shader-based shadow map). In 
		///	    this case you might want to enable this option.
        /// </summary>
		protected bool shadowTextureSelfShadow;
		/// <summary>
		///		The material file to be use for shadow casters, if any
        /// </summary>
		protected string shadowTextureCasterMaterial;
		/// <summary>
		///		The pass that renders custom texture casters, or null
        /// </summary>
		protected Pass shadowTextureCustomCasterPass;
		/// <summary>
		///		The name of the vertex program that renders custom shadow casters, or null
        /// </summary>
		protected string shadowTextureCustomCasterVertexProgram;
		/// <summary>
		///		The parameters of the vertex program that renders custom shadow casters, or null
        /// </summary>
		protected GpuProgramParameters shadowTextureCustomCasterVPParams;
        /// <summary>
        ///		The name of the pixel program that renders custom shadow casters, or null
        /// </summary>
        protected string shadowTextureCustomCasterFragmentProgram;
        /// <summary>
        ///		The parameters of the pixel program that renders custom shadow casters, or null
        /// </summary>
        protected GpuProgramParameters shadowTextureCustomCasterFPParams;

		/// <summary>
		///		The material file to be use for shadow receivers, if any
        /// </summary>
		protected string shadowTextureReceiverMaterial;
		/// <summary>
		///		The material file to be use for shadow receivers, if any
        /// </summary>
		protected Pass shadowTextureCustomReceiverPass;
		/// <summary>
		///		The name of the vertex program that renders custom shadow receivers, or null
        /// </summary>
		protected string shadowTextureCustomReceiverVertexProgram;
		/// <summary>
		///		The parameters of the vertex program that renders custom shadow casters, or null
        /// </summary>
		protected GpuProgramParameters shadowTextureCustomReceiverVPParams;
		/// <summary>
		///		The name of the pixel program that renders custom shadow receivers, or null
        /// </summary>
		protected string shadowTextureCustomReceiverFragmentProgram;
		/// <summary>
		///		The parameters of the pixel program that renders custom shadow casters, or null
        /// </summary>
		protected GpuProgramParameters shadowTextureCustomReceiverFPParams;
		/// <summary>
		///     Find visible objects?
        /// </summary>
		protected bool findVisibleObjects;
		/// <summary>
		///     Suppress render state changes?
        /// </summary>
		protected bool suppressRenderStateChanges;
		/// <summary>
		///     Suppress shadows?
        /// </summary>
		protected bool suppressShadows;
		/// <summary>
		///     ShadowCamera to light mapping */
        /// </summary>
		protected Dictionary<Camera, Light> shadowCamLightMapping = 
            new Dictionary<Camera, Light>();
		/// <summary>
		///     default shadow camera setup
        /// </summary>
		ShadowCameraSetup defaultShadowCameraSetup;
		/// <summary>
		///		Program parameters for infinite extrusion programs.
		/// </summary>
		protected GpuProgramParameters infiniteExtrusionParams;
		/// <summary>
		///		Program parameters for finite extrusion programs.
		/// </summary>
		protected GpuProgramParameters finiteExtrusionParams;
		/// <summary>
		///		If set, only this scene node (and children) will be rendered.
		///		If null, root node is used.
		/// </summary>
		protected SceneNode defaultRootNode;
		/// <summary>
		///		For the RenderTextureShadowCasterQueueGroupObjects and
		///		RenderTextureShadowReceiverQueueGroupObjects methods.
		/// </summary>
		protected bool shadowMaterialInitDone = false;
        /// <summary>
        ///		Used by compositing layer
        ///</summary>
        protected uint visibilityMask = 0xFFFFFFFF;
        protected List<Light> nullLightList = new List<Light>();
		protected ulong lastFrameNumber;
		protected SceneDetailLevel lastDetailLevel = SceneDetailLevel.Points;
		protected Viewport currentViewport;

        /// <summary>
        ///		Visible objects bounding box list.
        /// </summary>
        /// <remarks>
        ///     Holds an ABB for each camera that contains the physical extends of the visible
        ///     scene elements by each camera. The map is crutial for shadow algorithms which
        ///     have a focus step to limit the shadow sample distribution to only valid visible
        ///     scene elements.
        /// </remarks>
		protected Dictionary<Camera, VisibleObjectsBoundsInfo> camVisibleObjectsMap = 
            new Dictionary<Camera, VisibleObjectsBoundsInfo>();

        /// Cached light information, used to tracking light's changes
        /// <summary>
        ///		The list of static geometry instances maintained by
        ///     the scene manager
        ///</summary>
		protected Dictionary<string, StaticGeometry> staticGeometryList =
            new Dictionary<string, StaticGeometry>();
        
		protected long totalSetPassCalls = 0;
        
        #endregion Fields

		#region Public events
		/// <summary>An event that will fire when a render queue is starting to be rendered.</summary>
		public event RenderQueueEvent QueueStarted;
		/// <summary>An event that will fire when a render queue is finished being rendered.</summary>
		public event RenderQueueEvent QueueEnded;

        /// <summary>
        /// An event that fires when the ambient light changes color
        /// </summary>
        public event AmbientLightChangeHandler AmbientLightChanged;

        /// <summary>
        /// This event fires when a light is added to the scene
        /// </summary>
        public event NamedLightEventHandler LightAdded;

        /// <summary>
        /// This event fires when a light is removed from the scene
        /// </summary>
        public event NamedLightEventHandler LightRemoved;
		#endregion

		#region Constructors

		public SceneManager(string smName) {
			this.smName = smName;
			cameraList = new Dictionary<string, Camera>();
			//lightList = new LightList();
			//entityList = new EntityList();
			movableObjectCollectionMap = new Dictionary<string, Dictionary<string, MovableObject>>();
			sceneNodeList = new Dictionary<string, SceneNode>();
			//billboardSetList = new Dictionary<string, BillboardSet>();
			animationList = new Dictionary<string, Animation>();
			animationStateList = new AnimationStateSet();
            regionList = new List<Region>();

			// create the root scene node
			rootSceneNode = new SceneNode(this, "Root");
			defaultRootNode = rootSceneNode;

			// default to no fog
			fogMode = FogMode.None;

			// no shadows by default
			shadowTechnique = ShadowTechnique.None;

			illuminationStage = IlluminationRenderStage.None;
            renderingNoShadowQueue = false;
            renderingMainGroup = false;
			shadowColor = new ColorEx(0.25f, 0.25f, 0.25f);
			shadowDirLightExtrudeDist = 10000;
			shadowIndexBufferSize = 51200;
			shadowTextureOffset = 0.6f;
			shadowTextureFadeStart = 0.7f;
			shadowTextureFadeEnd = 0.9f;
			shadowTextureSize = 512;
            shadowTextureConfigDirty = true;
            ShadowTextureCount = 1;
			findVisibleObjects = true;
			suppressRenderStateChanges = false;
			suppressShadows = false;
			shadowUseInfiniteFarPlane = true;
			shadowCasterRenderBackFaces = true;
            defaultShadowCameraSetup = new DefaultShadowCameraSetup();
		}

		~SceneManager() {
			ClearScene();
			RemoveAllCameras();
		}

		#endregion

		#region Virtual methods

		/// <summary>
		///		Creates an instance of a SceneNode.
		/// </summary>
		/// <remarks>
		///    Note that this does not add the SceneNode to the scene hierarchy.
		///		This method is for convenience, since it allows an instance to
		///		be created for which the SceneManager is responsible for
		///		allocating and releasing memory, which is convenient in complex
		///		scenes.
		///		<p/>
		///	    To include the returned SceneNode in the scene, use the AddChild
		///		method of the SceneNode which is to be it's parent.
		///		<p/>
		///     Note that this method takes no parameters, and the node created is unnamed (it is
		///     actually given a generated name, which you can retrieve if you want).
		///     If you wish to create a node with a specific name, call the alternative method
		///     which takes a name parameter.
		/// </remarks>
		/// <returns></returns>
		public virtual SceneNode CreateSceneNode() {
			SceneNode node = new SceneNode(this);
			sceneNodeList[node.Name] = node;
			return node;
		}

		/// <summary>
		///		Creates an instance of a SceneNode with a given name.
		/// </summary>
		/// <remarks>
		///		Note that this does not add the SceneNode to the scene hierarchy.
		///		This method is for convenience, since it allows an instance to
		///		be created for which the SceneManager is responsible for
		///		allocating and releasing memory, which is convenient in complex
		///		scenes.
		///		<p/>
		///		To include the returned SceneNode in the scene, use the AddChild
		///		method of the SceneNode which is to be it's parent.
		///		<p/>
		///		Note that this method takes a name parameter, which makes the node easier to
		///		retrieve directly again later.
		/// </remarks>
		/// <param name="pName"></param>
		/// <returns></returns>
		public virtual SceneNode CreateSceneNode(string name) {
			SceneNode node = new SceneNode(this, name);
            sceneNodeList[node.Name] = node;
            return node;
		}

		/// <summary>
		///		Creates an animation which can be used to animate scene nodes.
		/// </summary>
		/// <remarks>
		///		An animation is a collection of 'tracks' which over time change the position / orientation
		///		of Node objects. In this case, the animation will likely have tracks to modify the position
		///		/ orientation of SceneNode objects, e.g. to make objects move along a path.
		///		<p/>
		///		You don't need to use an Animation object to move objects around - you can do it yourself
		///		using the methods of the Node in your application. However, when you need relatively
		///		complex scripted animation, this is the class to use since it will interpolate between
		///		keyframes for you and generally make the whole process easier to manage.
		///		<p/>
		///		A single animation can affect multiple Node objects (each AnimationTrack affects a single Node).
		///		In addition, through animation blending a single Node can be affected by multiple animations,
		///		although this is more useful when performing skeletal animation (see Skeleton.CreateAnimation).
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public virtual Animation CreateAnimation(string name, float length) {
			if(animationList.ContainsKey(name)) {
				throw new AxiomException(string.Format("An animation with the name '{0}' already exists in the scene.", name));
			}

			// create a new animation and record it locally
			Animation anim = new Animation(name, length);
			animationList[anim.Name] = anim;

			return anim;
		}

		/// <summary>
		///		Create an AnimationState object for managing application of animations.
		/// </summary>
		/// <remarks>
		///		You can create Animation objects for animating SceneNode obejcts using the
		///		CreateAnimation method. However, in order to actually apply those animations
		///		you have to call methods on Node and Animation in a particular order (namely
		///		Node.ResetToInitialState and Animation.Apply). To make this easier and to
		///		help track the current time position of animations, the AnimationState object
		///		is provided. 
		///		</p>
		///		So if you don't want to control animation application manually, call this method,
		///		update the returned object as you like every frame and let SceneManager apply 
		///		the animation state for you.
		///		<p/>
		///		Remember, AnimationState objects are disabled by default at creation time. 
		///		Turn them on when you want them using their Enabled property.
		///		<p/>
		///		Note that any SceneNode affected by this automatic animation will have it's state
		///		reset to it's initial position before application of the animation. Unless specifically
		///		modified using Node.SetInitialState the Node assumes it's initial state is at the
		///		origin. If you want the base state of the SceneNode to be elsewhere, make your changes
		///		to the node using the standard transform methods, then call SetInitialState to 
		///		'bake' this reference position into the node.
		/// </remarks>
		/// <param name="animationName"></param>
		/// <returns></returns>
		public virtual AnimationState CreateAnimationState(string animationName) {
			// do we have this already?
			if(animationStateList.HasAnimationState(animationName)) {
				throw new AxiomException("Cannot create, AnimationState already exists: " + animationName);
			}

			if(!animationList.ContainsKey(animationName)) {
				throw new AxiomException(string.Format("The name of a valid animation must be supplied when creating an AnimationState.  Animation '{0}' does not exist.", animationName));
			}

			// get a reference to the specified animation
			Animation anim = animationList[animationName];

            // create and return new animation state
            return animationStateList.CreateAnimationState(animationName, 0, anim.Length);
		}

		/// <summary>
		///		Overloaded method.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual BillboardSet CreateBillboardSet(string name) {
			// return new billboardset with a default pool size of 20
			return CreateBillboardSet(name, 20);
		}

		/// <summary>
		///		Creates a billboard set which can be uses for particles, sprites, etc.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="poolSize"></param>
		/// <returns></returns>
		public virtual BillboardSet CreateBillboardSet(string name, int poolSize) {
            if (GetMovableObjectMap("BillboardSet").ContainsKey(name)) {
				throw new AxiomException(string.Format("An object with the name '{0}' already exists in the scene.", name));
			}

			BillboardSet billboardSet = new BillboardSet(name, poolSize);

            // add it to our local list
            GetMovableObjectMap("BillboardSet")[name] = billboardSet;

			return billboardSet;
		}

 
		/// <summary>
		///		Creates a ribbon trail
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        public virtual RibbonTrail CreateRibbonTrail(string name)
        {
            RibbonTrail ribbonTrail = new RibbonTrail(name);
            // add it to our local list
            GetMovableObjectMap("RibbonTrail")[name] = ribbonTrail;

            return ribbonTrail;
        }

		/// <summary>
		///		Creates a camera to be managed by this scene manager.
		/// </summary>
		/// <remarks>
		///		This camera can be added to the scene at a later time using
		///		the AttachObject method of the SceneNode class.
		///	 </remarks>
		///	 <param name="name"></param>
		/// <returns></returns>
		public virtual Camera CreateCamera(string name) {
			if (cameraList.ContainsKey(name)) {
				throw new AxiomException(string.Format("A camera with the name '{0}' already exists in the scene.", name));
			}

			// create the camera and add it to our local list
			Camera camera = new Camera(name, this);
			cameraList[name] = camera;

            // create visible bounds aabb map entry
            camVisibleObjectsMap[camera] = new VisibleObjectsBoundsInfo();

			return camera;
		}

        /// <summary>
		///		Create an Entity (instance of a discrete mesh).
		/// </summary>
		/// <param name="name">The name to be given to the entity (must be unique).</param>
		/// <param name="meshName">The name of the mesh to load.  Will be loaded if not already.</param>
		/// <returns></returns>
		public virtual Entity CreateEntity(string name, string meshName) {
            if (GetMovableObjectMap("Entity").ContainsKey(name))
				throw new AxiomException(string.Format("An object with the name '{0}' already exists in the scene.", name));

			Mesh mesh = MeshManager.Instance.Load(meshName);

			// create a new entitiy
            Entity entity = new Entity(name, mesh, this);

			// add it to our local list
            GetMovableObjectMap("Entity")[name] = entity;

			return entity;
		}

        /// <summary>
        ///		Create an Entity (instance of a discrete mesh).
        /// </summary>
        /// <param name="name">The name to be given to the entity (must be unique).</param>
        /// <param name="mesh">The mesh to use.</param>
        /// <returns></returns>
        public virtual Entity CreateEntity(string name, Mesh mesh) {
            if (GetMovableObjectMap("Entity").ContainsKey(name))
                throw new AxiomException(string.Format("An object with the name '{0}' already exists in the scene.", name));
            
            // create a new entitiy
            Entity entity = new Entity(name, mesh, this);

            // add it to our local list
            GetMovableObjectMap("Entity")[name] = entity;

            return entity;
        }

		/// <summary>
		///		Create an Entity (instance of a discrete mesh).
		/// </summary>
		/// <param name="name">The name to be given to the entity (must be unique).</param>
		/// <param name="meshName">The name of the mesh to load.  Will be loaded if not already.</param>
		/// <returns></returns>
		public virtual Entity CreateEntity(string name, PrefabEntity prefab) {
			switch(prefab) {
				case PrefabEntity.Plane:
					return CreateEntity(name, "Prefab_Plane");
				default:
					return null;
			}        
		}

        protected void OnLightAdded(string lightName)
        {
            NamedLightEventHandler handler = LightAdded;
            if (handler != null)
            {
                handler(lightName);
            }
        }

		/// <summary>
		///		Creates a light that will be managed by this scene manager.
		/// </summary>
		/// <remarks>
		///		Lights can either be in a fixed position and independent of the
		///		scene graph, or they can be attached to SceneNodes so they derive
		///		their position from the parent node. Either way, they are created
		///		using this method so that the SceneManager manages their
		///		existence.
		/// </remarks>
		/// <param name="name">Name of the light to create.</param>
		/// <returns></returns>
		public virtual Light CreateLight(string name) {
            if (GetMovableObjectMap("Light").ContainsKey(name)) {
				throw new AxiomException(string.Format("An object with the name '{0}' already exists in the scene.", name));
			}

			// create a new light and add it to our internal list
			Light light = new Light(name);
			
			// add the light to the list
            GetMovableObjectMap("Light")[name] = light;

            OnLightAdded(name);

			return light;
		}

		/// <summary>
		///		Creates a new (blank) material with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual Material CreateMaterial(string name) {
			Material material = (Material) MaterialManager.Instance.Create(name);
			return material;
		}

		/// <summary>
		///		Creates a new Overlay.
		/// </summary>
		/// <remarks>
		///		<p>
		///		Overlays can be used to render heads-up-displays (HUDs), menu systems,
		///		cockpits and any other 2D or 3D object you need to appear above the
		///		rest of the scene. See the Overlay class for more information.
		///		</p>
		///		<p>
		///		NOTE: after creation, the Overlay is initially hidden. You can create
		///		as many overlays as you like ready to be displayed whenever. Just call
		///		Overlay.Show to display the overlay.
		///		</p>
		/// </remarks>
		/// <param name="name">The name to give the overlay, must be unique.</param>
		/// <param name="zorder">The zorder of the overlay relative to it's peers, higher zorders appear on top of lower ones.</param>
		public virtual Overlay CreateOverlay(string name, int zorder) {
			Overlay newOverlay = (Overlay) OverlayManager.Instance.Create(name);
			newOverlay.ZOrder = zorder;

			return newOverlay;
		}

		/// <summary>
		///		Empties the entire scene, inluding all SceneNodes, Entities, Lights, 
		///		BillboardSets etc. Cameras are not deleted at this stage since
		///		they are still referenced by viewports, which are not destroyed during
		///		this process.
		/// </summary>
		public virtual void ClearScene() {
			// Delete all SceneNodes, except root that is
			sceneNodeList.Clear();
			autoTrackingSceneNodes.Clear();

			rootSceneNode.RemoveAllChildren();
			rootSceneNode.DetachAllObjects();

			RemoveAllEntities();
			RemoveAllBillboardSets();
            RemoveAllLights();

			// Clear animations
			DestroyAllAnimations();

			// Remove sky nodes since they've been deleted
			skyBoxNode = skyPlaneNode = skyDomeNode = null;
			isSkyBoxEnabled = isSkyPlaneEnabled = isSkyDomeEnabled = false; 
		}

		/// <summary>
		///    Destroys and removes a node from the scene.  This will also 
        ///    remove the contents of the scene node from the scene.
		/// </summary>
		/// <param name="name"></param>
		public virtual void DestroySceneNode(string name) {
			if (!sceneNodeList.ContainsKey(name)) {
				throw new AxiomException("SceneNode named '{0}' not found.", name);
			}

			// grab the node from the list
			SceneNode node = (SceneNode)sceneNodeList[name];

			// Find any scene nodes which are tracking this node, and turn them off.
			for(int i = 0; i < autoTrackingSceneNodes.Count; i++) {
				SceneNode autoNode = autoTrackingSceneNodes[i];

				// Tracking this node
				if(autoNode.AutoTrackTarget == node) {
					// turn off, this will notify SceneManager to remove
					autoNode.SetAutoTracking(false);
				}
				else if(autoNode == node) {
					// node being removed is a tracker
					autoTrackingSceneNodes.Remove(autoNode);
				}
			}

			/* CMH 7/18/2004 */
			// Destroy child nodes
            // Copy the list to a temporary list
            List<Node> nodes = new List<Node>(node.Children);
            foreach (Node child in nodes)
                DestroySceneNode(child.Name);

			foreach (MovableObject obj in node.Objects) {
                if (obj is Camera)
                    cameraList.Remove(obj.Name);
                else if (obj is MovableObject)
                    GetMovableObjectMap(obj).Remove(obj.Name);
			}
			// Remove this node from its parent
            if (node.Parent != null)
			    node.Parent.RemoveChild(node);

			// removes the node from the list
			sceneNodeList.Remove(node.Name);

            // Call the destroy node listener method, if any
            node.OnNodeDestroyed();
		}

		/// <summary>
		///		Destroys an Animation. 
		/// </summary>
		/// <param name="name"></param>
		public virtual void DestroyAnimation(string name) {
			// Also destroy any animation states referencing this animation
            animationStateList.RemoveAnimationState(name);
			Animation animation = animationList[name];
			if (animation == null)
				throw new AxiomException("Animation named '{0}' not found.", name);
			animationList.Remove(animation.Name);
		}

		/// <summary>
		///		Destroys an AnimationState. 
		/// </summary>
		/// <param name="name"></param>
		public virtual void DestroyAnimationState(string name) {
			AnimationState animationState = animationStateList.GetAnimationState(name);
			if (animationState == null)
				throw new AxiomException("AnimationState named '{0}' not found.", name);

			animationStateList.RemoveAnimationState(name);
		}

		/// <summary>
		///		Removes all animations created using this SceneManager.
		/// </summary>
		public virtual void DestroyAllAnimations() {
			// Destroy all states too, since they cannot reference destroyed animations
			DestroyAllAnimationStates();
			animationList.Clear();
		}

		/// <summary>
		///		Removes all AnimationStates created using this SceneManager.
		/// </summary>
		public virtual void DestroyAllAnimationStates() {
			animationStateList.RemoveAllAnimationStates();
		}

		/// <summary>
		///		Destroys all the overlays.
		/// </summary>
		public virtual void DestroyAllOverlays() {
			OverlayManager.Instance.UnloadAndDestroyAll();
		}

		/// <summary>
		///		Destroys the named Overlay.
		/// </summary>
		public virtual void DestroyOverlay(string name) {
			Overlay overlay = OverlayManager.Instance.GetByName(name);

			if (overlay == null)
				throw new AxiomException("An overlay named " + name + " cannot be found to be destroyed.");

			OverlayManager.Instance.Unload(overlay);			
		}

		/// <summary>
		///     Retreives the camera with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual Camera GetCamera(string name) {
			Camera camera = cameraList[name];
			if(camera == null) {
				throw new AxiomException("Camera named '{0}' not found.", name);
			}

			return camera;
		}

		/// <summary>
		///     Retreives the light with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual Light GetLight(string name) {
            return (Light)GetMovableObject(name, "Light");
		}

		/// <summary>
		///     Retreives the BillboardSet with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual BillboardSet GetBillboardSet(string name) {
            return (BillboardSet)GetMovableObject(name, "BillboardSet");
		}

		/// <summary>
		///     Retreives the RibbonTrail with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual RibbonTrail GetRibbonTrail(string name) {
            return (RibbonTrail)GetMovableObject(name, "RibbonTrail");
		}

		/// <summary>
		///     Retreives the animation with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual Animation GetAnimation(string name) {
            Animation animation;
            if (!animationList.TryGetValue(name, out animation))
                // throw new AxiomException("Animation named '{0}' not found.", name);
                return null;
            return animation;
		}

		/// <summary>
		///     Retreives the AnimationState with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual AnimationState GetAnimationState(string name) {
			AnimationState animationState = animationStateList.GetAnimationState(name);
			/*
			if(animationState == null) {
				throw new AxiomException("AnimationState named '{0}' not found.", name);
			}*/
			return animationState;
		}

		/// <summary>
		///		Gets a the named Overlay, previously created using CreateOverlay.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual Overlay GetOverlay(string name) {
			Overlay overlay = OverlayManager.Instance.GetByName(name);
			/*
			if (overlay == null)
				throw new AxiomException("An overlay named " + name + " cannot be found.");
			*/
			return overlay;
		}

		/// <summary>
		///     Returns the material with the specified name.
		/// </summary>
		/// <param name="name">Name of the material to retrieve.</param>
		/// <returns>A reference to a Material.</returns>
		public virtual Material GetMaterial(string name) {
			return MaterialManager.Instance.GetByName(name);
		}

		/// <summary>
		///     Returns the material with the specified handle.
		/// </summary>
		/// <param name="name">Handle of the material to retrieve.</param>
		/// <returns>A reference to a Material.</returns>
		public virtual Material GetMaterial(int handle) {
			return (Material)MaterialManager.Instance.GetByHandle(handle);
		}

		/// <summary>
		///		Retrieves the internal render queue.
		/// </summary>
		/// <returns>
		///		The render queue in use by this scene manager.
		///		Note: The queue is created if it doesn't already exist.
		/// </returns>
		protected virtual RenderQueue GetRenderQueue() {
			if(renderQueue == null) {
				InitRenderQueue();
			}

			return renderQueue;
		}

		/// <summary>
		///		Internal method for initializing the render queue.
		/// </summary>
		/// <remarks>
		///		Subclasses can use this to install their own <see cref="RenderQueue"/> implementation.
		/// </remarks>
		protected virtual void InitRenderQueue() {
			renderQueue = new RenderQueue();

			// init render queues that do not need shadows
			renderQueue.GetQueueGroup(RenderQueueGroupID.Background).ShadowsEnabled = false;
			renderQueue.GetQueueGroup(RenderQueueGroupID.Overlay).ShadowsEnabled = false;
			renderQueue.GetQueueGroup(RenderQueueGroupID.SkiesEarly).ShadowsEnabled = false;
			renderQueue.GetQueueGroup(RenderQueueGroupID.SkiesLate).ShadowsEnabled = false;
		}

		/// <summary>
		///     Retreives the scene node with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns>the scene node associated with the given name, or null if no matching entry is found</returns>
		public SceneNode GetSceneNode(string name) {
            SceneNode node;
            sceneNodeList.TryGetValue(name, out node);
			return node;
		}
		
		/// <summary>
		///     Retreives the scene node with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        //public SceneNode GetSceneNode(int index) 
        //{
        //    int count = sceneNodeList.Count;
        //    if(index > count || index < 0)
        //        throw new ArgumentOutOfRangeException("Scene node index is invalid.");
        //    SceneNode node = sceneNodeList[index];
        //    return node;
        //}


		
		/// <summary>
		///     Retreives the scene node with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        public Entity GetEntity(string name) {
            return (Entity)GetMovableObject(name, "Entity");
        }

        public MovableObject GetMovableObject(string name, string type) {
            return GetMovableObjectMap(type)[name];
        }

        public ICollection<MovableObject> GetMovableObjectCollection(string type) {
            return GetMovableObjectMap(type).Values;
        }

        protected Dictionary<string, MovableObject> GetMovableObjectMap(string type) {
            Dictionary<string, MovableObject> objectMap;
            if (!movableObjectCollectionMap.TryGetValue(type, out objectMap)) {
                objectMap = new Dictionary<string, MovableObject>();
                movableObjectCollectionMap[type] = objectMap;
            }
            return objectMap;
        }
		
		/// <summary>
		///     Retreives the scene node with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        //public Entity GetEntity(int index) 
        //{
        //    int count = entityList.Count;
        //    if(index > count || index < 0)
        //        throw new ArgumentOutOfRangeException("Entity index is invalid.");
        //    return entityList[index] as Entity;
        //}


		/// <summary>
		///     Asks the SceneManager to provide a suggested viewpoint from which the scene should be viewed.
		/// </summary>
		/// <remarks>
		///     Typically this method returns the origin unless a) world geometry has been loaded using
		///     <see cref="SceneManager.LoadWorldGeometry"/> and b) that world geometry has suggested 'start' points.
		///     If there is more than one viewpoint which the scene manager can suggest, it will always suggest
		///     the first one unless the random parameter is true.
		/// </remarks>
		/// <param name="random">
		///     If true, and there is more than one possible suggestion, a random one will be used. If false
		///     the same one will always be suggested.
		/// </param>
		/// <returns>Optimal ViewPoint defined by the scene manager.</returns>
		public virtual ViewPoint GetSuggestedViewpoint(bool random) {
			ViewPoint vp;

			// by default, return the origin.  leave up to subclasses to define
			vp.position = Vector3.Zero;
			vp.orientation = Quaternion.Identity;

			return vp;
		}

		/// <summary>
		///		Loads the source of the 'world' geometry, i.e. the large, mainly static geometry
		///		making up the world e.g. rooms, landscape etc.
		/// </summary>
		/// <remarks>
		///		Depending on the type of SceneManager (subclasses will be specialized
		///		for particular world geometry types) you have requested via the Root or
		///		SceneManagerEnumerator classes, you can pass a filename to this method and it
		///		will attempt to load the world-level geometry for use. If you try to load
		///		an inappropriate type of world data an exception will be thrown. The default
		///		SceneManager cannot handle any sort of world geometry and so will always
		///		throw an exception. However subclasses like BspSceneManager can load
		///		particular types of world geometry e.g. "q3dm1.bsp".
		/// </remarks>
		/// <param name="fileName"></param>
		public virtual void LoadWorldGeometry(string fileName) {
			// This default implementation cannot handle world geometry
			throw new AxiomException("World geometry is not supported by the generic SceneManager.");
		}

		public void ManualRender(RenderOperation op, Pass pass, Viewport vp, 
			Matrix4 worldMatrix, Matrix4 viewMatrix, Matrix4 projMatrix) {

			ManualRender(op, pass, vp, worldMatrix, viewMatrix, projMatrix, false);
		}

		/// <summary>
		///		Manual rendering method, for advanced users only.
		/// </summary>
		/// <remarks>
		///		This method allows you to send rendering commands through the pipeline on
		///		demand, bypassing any normal world processing. You should only use this if you
		///		really know what you're doing; the engine does lots of things for you that you really should
		///		let it do. However, there are times where it may be useful to have this manual interface,
		///		for example overlaying something on top of the scene.
		///		<p/>
		///		Because this is an instant rendering method, timing is important. The best 
		///		time to call it is from a RenderTarget event handler.
		///		<p/>
		///		Don't call this method a lot, it's designed for rare (1 or 2 times per frame) use. 
		///		Calling it regularly per frame will cause frame rate drops!
		/// </remarks>
		/// <param name="op">A RenderOperation object describing the rendering op.</param>
		/// <param name="pass">The Pass to use for this render.</param>
		/// <param name="vp">Reference to the viewport to render to.</param>
		/// <param name="worldMatrix">The transform to apply from object to world space.</param>
		/// <param name="viewMatrix">The transform to apply from object to view space.</param>
		/// <param name="projMatrix">The transform to apply from view to screen space.</param>
		/// <param name="doBeginEndFrame">
		///		If true, BeginFrame() and EndFrame() are called, otherwise not. 
		///		You should leave this as false if you are calling this within the main render loop.
		/// </param>
		public virtual void ManualRender(RenderOperation op, Pass pass, Viewport vp, 
			Matrix4 worldMatrix, Matrix4 viewMatrix, Matrix4 projMatrix, bool doBeginEndFrame) {

			// configure all necessary parameters
			targetRenderSystem.SetViewport(vp);
			targetRenderSystem.WorldMatrix = worldMatrix;
			targetRenderSystem.ViewMatrix = viewMatrix;
			targetRenderSystem.ProjectionMatrix = projMatrix;

			if (doBeginEndFrame) {
				targetRenderSystem.BeginFrame();
			}

			// set the pass and render the object
			SetPass(pass);
			targetRenderSystem.Render(op);

			if (doBeginEndFrame) {
				targetRenderSystem.EndFrame();
			}
		}

		#endregion

		#region Protected methods

		/// <summary>
		///		Internal method for locating a list of lights which could be affecting the frustum.
		/// </summary>
		/// <remarks>
		///		Custom scene managers are encouraged to override this method to make use of their
		///		scene partitioning scheme to more efficiently locate lights, and to eliminate lights
		///		which may be occluded by word geometry.
		/// </remarks>
		/// <param name="camera">Camera to find lights within it's view.</param>
		protected virtual void FindLightsAffectingFrustum(Camera camera) {
			// Basic iteration for this scene manager
			lightsAffectingFrustum.Clear();

			// sphere to use for testing
			Sphere sphere = new Sphere();

            ICollection<MovableObject> lightList = GetMovableObjectCollection("Light");
			foreach (Light light in lightList) {
				if (light.IsVisible) {
					if(light.Type == LightType.Directional) {
						// Always visible
						lightsAffectingFrustum.Add(light);
					}
					else {
						// treating spotlight as point for simplicity
						// Just see if the lights attenuation range is within the frustum
						sphere.Center = light.DerivedPosition;
						sphere.Radius = light.AttenuationRange;

						if (camera.IsObjectVisible(sphere)) {
							lightsAffectingFrustum.Add(light);
						}
					}
				}
			}
		}

		/// <summary>
		///		Internal method for locating a list of shadow casters which 
		///		could be affecting the frustum for a given light. 
		/// </summary>
		/// <remarks>
		///		Custom scene managers are encouraged to override this method to add optimizations, 
		///		and to add their own custom shadow casters (perhaps for world geometry)
		/// </remarks>
		/// <param name="light"></param>
		/// <param name="camera"></param>
		protected virtual IList FindShadowCastersForLight(Light light, Camera camera) {
			shadowCasterList.Clear();

			if (light.Type == LightType.Directional) {
				// Basic AABB query encompassing the frustum and the extrusion of it
				AxisAlignedBox aabb = new AxisAlignedBox();
				Vector3[] corners = camera.WorldSpaceCorners;
				Vector3 min, max;
				Vector3 extrude = light.DerivedDirection * -shadowDirLightExtrudeDist;
				// do first corner
				min = max = corners[0];
				min.Floor(corners[0] + extrude);
				max.Ceil(corners[0] + extrude);
				for (int c = 1; c < 8; ++c) {
					min.Floor(corners[c]);
					max.Ceil(corners[c]);
					min.Floor(corners[c] + extrude);
					max.Ceil(corners[c] + extrude);
				}
				aabb.SetExtents(min, max);

				if (shadowCasterAABBQuery == null)
					shadowCasterAABBQuery = CreateAABBRegionQuery(aabb);
				else
					shadowCasterAABBQuery.Box = aabb;
				// Execute, use callback
				shadowCasterQueryListener.Prepare(false, 
					light.GetFrustumClipVolumes(camera), 
					light, camera, shadowCasterList, shadowFarDistanceSquared);
				shadowCasterAABBQuery.Execute(shadowCasterQueryListener);
			}
			else {
				Sphere s = new Sphere(light.DerivedPosition, light.AttenuationRange);

				// eliminate early if camera cannot see light sphere
				if (camera.IsObjectVisible(s)) {
					// create or init a sphere region query
					if(shadowCasterSphereQuery == null) {
						shadowCasterSphereQuery = CreateSphereRegionQuery(s);
					}
					else {
						shadowCasterSphereQuery.Sphere = s;
					}

					// check if the light is within view of the camera
					bool lightInFrustum = camera.IsObjectVisible(light.DerivedPosition);

					List<PlaneBoundedVolume> volumeList = null;

					// Only worth building an external volume list if
					// light is outside the frustum
					if(!lightInFrustum) {
						volumeList = light.GetFrustumClipVolumes(camera);
					}

					// prepare the query and execute using the callback
					shadowCasterQueryListener.Prepare(
						lightInFrustum, volumeList, 
						light, camera, 
						shadowCasterList, 
						shadowFarDistanceSquared);

					shadowCasterSphereQuery.Execute(shadowCasterQueryListener);
				}
			}

			return shadowCasterList;
		}

		
		protected const string SPOT_SHADOW_FADE_IMAGE = "spot_shadow_fade.png";
		protected const string TEXTURE_SHADOW_RECEIVER_MATERIAL = "Axiom/TextureShadowReceiver";
		protected const string TEXTURE_SHADOW_CASTER_MATERIAL = "Axiom/TextureShadowCaster";
		protected const string STENCIL_SHADOW_MODULATIVE_MATERIAL = "Axiom/StencilShadowModulationPass";
		protected const string STENCIL_SHADOW_VOLUMES_MATERIAL = "Axiom/StencilShadowVolumes";
		protected const string SHADOW_VOLUMES_MATERIAL = "Axiom/Debug/ShadowVolumes";	

		/// <summary>
		///		Internal method for setting up materials for shadows.
		/// </summary>
		protected virtual void InitShadowVolumeMaterials() {
			if (shadowMaterialInitDone) {
				return;
			}

			if (shadowDebugPass == null) {
				InitShadowDebugPass();
			}

			if (shadowStencilPass == null) {
				InitShadowStencilPass();
			}


			if (shadowModulativePass == null) {
				InitShadowModulativePass();
			}

			// Also init full screen quad while we're at it
			if(fullScreenQuad == null) {
				fullScreenQuad = new Rectangle2D();
				fullScreenQuad.SetCorners(-1, 1, 1, -1);
			}

			// Also init shadow caster material for texture shadows
			if (shadowCasterPlainBlackPass == null) {
				InitShadowCasterPass();
			}

			if (shadowReceiverPass == null) {
				InitShadowReceiverPass();
			}

			// InitShadowReceiverPass up spot shadow fade texture (loaded from code data block)
			Texture spotShadowFadeTex = TextureManager.Instance.GetByName(SPOT_SHADOW_FADE_IMAGE);

			if (spotShadowFadeTex == null) {
				// Load the manual buffer into an image
				System.IO.MemoryStream imgStream = new System.IO.MemoryStream(SpotShadowFadePng.SPOT_SHADOW_FADE_PNG);
				Media.Image img = Media.Image.FromStream(imgStream, "png");
				spotShadowFadeTex = 
					TextureManager.Instance.LoadImage(SPOT_SHADOW_FADE_IMAGE, img, TextureType.TwoD);
			}

			shadowMaterialInitDone = true;
		}

		private void InitShadowReceiverPass()
		{
			Material matShadRec = MaterialManager.Instance.GetByName(TEXTURE_SHADOW_RECEIVER_MATERIAL);

			if (matShadRec == null) {
				matShadRec = (Material) MaterialManager.Instance.Create(TEXTURE_SHADOW_RECEIVER_MATERIAL);
				shadowReceiverPass = matShadRec.GetTechnique(0).GetPass(0);
				shadowReceiverPass.SetSceneBlending(SceneBlendFactor.DestColor, SceneBlendFactor.Zero);
				// No lighting, one texture unit 
				// everything else will be bound as needed during the receiver pass
				shadowReceiverPass.LightingEnabled = false;
				TextureUnitState t = shadowReceiverPass.CreateTextureUnitState();
				t.TextureAddressing = TextureAddressing.Clamp;
                shadowReceiverPass.DepthBias = 1;
			}
			else {
				shadowReceiverPass = matShadRec.GetTechnique(0).GetPass(0);
			}
		}

		private void InitShadowCasterPass()
		{
			Material matPlainBlack = MaterialManager.Instance.GetByName(TEXTURE_SHADOW_CASTER_MATERIAL);

			if (matPlainBlack == null) {
				matPlainBlack = (Material) MaterialManager.Instance.Create(TEXTURE_SHADOW_CASTER_MATERIAL);
				shadowCasterPlainBlackPass = matPlainBlack.GetTechnique(0).GetPass(0);
				// Lighting has to be on, because we need shadow coloured objects
				// Note that because we can't predict vertex programs, we'll have to
				// bind light values to those, and so we bind White to ambient
				// reflectance, and we'll set the ambient colour to the shadow colour
				shadowCasterPlainBlackPass.Ambient = ColorEx.White;
				shadowCasterPlainBlackPass.Diffuse = ColorEx.Black;
				shadowCasterPlainBlackPass.Emissive = ColorEx.Black;
				shadowCasterPlainBlackPass.Specular = ColorEx.Black;
				// Override fog
				shadowCasterPlainBlackPass.SetFog(true, FogMode.None);
				// no textures or anything else, we will bind vertex programs
				// every so often though

                //shadowCasterPlainBlackPass.DepthCheck = false;
			}
			else {
				shadowCasterPlainBlackPass = matPlainBlack.GetTechnique(0).GetPass(0);
			}
		}

		private void InitShadowModulativePass()
		{
			Material matModStencil = 
				MaterialManager.Instance.GetByName(STENCIL_SHADOW_MODULATIVE_MATERIAL);

			if(matModStencil == null) {
				// Create
				matModStencil = 
					(Material)MaterialManager.Instance.Create(STENCIL_SHADOW_MODULATIVE_MATERIAL);

				shadowModulativePass = matModStencil.GetTechnique(0).GetPass(0);
				shadowModulativePass.SetSceneBlending(SceneBlendFactor.DestColor, SceneBlendFactor.Zero); 
				shadowModulativePass.LightingEnabled = false;
				shadowModulativePass.DepthWrite = false;
				shadowModulativePass.DepthCheck = false;
				TextureUnitState t = shadowModulativePass.CreateTextureUnitState();
				t.SetColorOperationEx(
					LayerBlendOperationEx.Modulate, 
					LayerBlendSource.Manual, 
					LayerBlendSource.Current, 
					shadowColor);
			}
			else {
				shadowModulativePass = matModStencil.GetTechnique(0).GetPass(0);
			}
		}

		private void InitShadowDebugPass()
		{
			Material matDebug = MaterialManager.Instance.GetByName(SHADOW_VOLUMES_MATERIAL);

			if(matDebug == null) {
				// Create
				matDebug = (Material)MaterialManager.Instance.Create(SHADOW_VOLUMES_MATERIAL);
				shadowDebugPass = matDebug.GetTechnique(0).GetPass(0);
				shadowDebugPass.SetSceneBlending(SceneBlendType.Add); 
				shadowDebugPass.LightingEnabled = false;
				shadowDebugPass.DepthWrite = false;
				TextureUnitState t = shadowDebugPass.CreateTextureUnitState();
				t.SetColorOperationEx(
					LayerBlendOperationEx.Modulate, 
					LayerBlendSource.Manual, 
					LayerBlendSource.Current, 
					new ColorEx(0.7f, 0.0f, 0.2f));

				shadowDebugPass.CullMode = CullingMode.None;

				if(targetRenderSystem.Caps.CheckCap(Capabilities.VertexPrograms)) {
					ShadowVolumeExtrudeProgram.Initialize();

					// Enable the (infinite) point light extruder for now, just to get some params
					shadowDebugPass.SetVertexProgram(
						ShadowVolumeExtrudeProgram.GetProgramName(ShadowVolumeExtrudeProgram.Programs.PointLight));
						
					infiniteExtrusionParams = shadowDebugPass.VertexProgramParameters;
					infiniteExtrusionParams.SetAutoConstant(0, AutoConstants.WorldViewProjMatrix);
					infiniteExtrusionParams.SetAutoConstant(4, AutoConstants.LightPositionObjectSpace);
				}

				matDebug.Compile();
			}
			else {
				shadowDebugPass = matDebug.GetTechnique(0).GetPass(0);
			}
		}

		private void InitShadowStencilPass()
		{
			Material matStencil = MaterialManager.Instance.GetByName(STENCIL_SHADOW_VOLUMES_MATERIAL);

			if(matStencil == null) {
				// Create
				matStencil = (Material)MaterialManager.Instance.Create(STENCIL_SHADOW_VOLUMES_MATERIAL);
				shadowStencilPass = matStencil.GetTechnique(0).GetPass(0);

				if(targetRenderSystem.Caps.CheckCap(Capabilities.VertexPrograms)) {
					// Enable the finite point light extruder for now, just to get some params
					shadowStencilPass.SetVertexProgram(
						ShadowVolumeExtrudeProgram.GetProgramName(ShadowVolumeExtrudeProgram.Programs.PointLightFinite));

					finiteExtrusionParams = shadowStencilPass.VertexProgramParameters;
					finiteExtrusionParams.SetAutoConstant(0, AutoConstants.WorldViewProjMatrix);
					finiteExtrusionParams.SetAutoConstant(4, AutoConstants.LightPositionObjectSpace);
					finiteExtrusionParams.SetAutoConstant(5, AutoConstants.ShadowExtrusionDistance);
				}
				matStencil.Compile();
				// Nothing else, we don't use this like a 'real' pass anyway,
				// it's more of a placeholder
			}
			else {
				shadowStencilPass = matStencil.GetTechnique(0).GetPass(0);
			}
		}


		/// <summary>
		///		Internal method for turning a regular pass into a shadow caster pass.
		/// </summary>
		/// <remarks>
		///		This is only used for texture shadows, basically we're trying to
		///		ensure that objects are rendered solid black.
		///		This method will usually return the standard solid black pass for
		///		all fixed function passes, but will merge in a vertex program
		///		and fudge the AutpoParamDataSource to set black lighting for
		///		passes with vertex programs. 
		/// </remarks>
		/// <param name="pass"></param>
		/// <returns></returns>
		protected virtual Pass DeriveShadowCasterPass(Pass pass) {
            if (IsShadowTechniqueTextureBased)
            {
                Pass retPass = (shadowTextureCustomCasterPass != null ?
                                shadowTextureCustomCasterPass : shadowCasterPlainBlackPass);
                retPass.CullMode = pass.CullMode;
                retPass.ManualCullMode = pass.ManualCullMode;
                // Does incoming pass have a custom shadow caster program?
                if (pass.ShadowCasterVertexProgramName != "")
                {
                    retPass.SetVertexProgram(pass.ShadowCasterVertexProgramName);
                    GpuProgram prg = retPass.VertexProgram;
                    // Load this program if not done already
                    if (!prg.IsLoaded)
                        prg.Load();
                    // Copy params
                    retPass.VertexProgramParameters = pass.ShadowCasterVertexProgramParameters;
                    // Also have to hack the light autoparams, that is done later
                }
                else
                {
                    // reset vp?
                    if (retPass == shadowTextureCustomCasterPass)
                    {
                        if (retPass.VertexProgramName != shadowTextureCustomCasterVertexProgram)
                        {
                            shadowTextureCustomCasterPass.SetVertexProgram(shadowTextureCustomCasterVertexProgram);
                            if (retPass.HasVertexProgram)
                                retPass.VertexProgramParameters = shadowTextureCustomCasterVPParams;
                        }
                    }
                    else
                        // Standard shadow caster pass, reset to no vp
                        retPass.SetVertexProgram("");
                }

                int keepTUCount = 0;

                // Material specifies a caster fragment program
                if (pass.ShadowCasterFragmentProgramName != "")
                {
                    // If the material specifies a fragment program, then we need to copy the
                    // TextureUnitStates from the original pass.
                    int origPassTUCount = pass.NumTextureUnitStages;
                    for (int t = 0; t < origPassTUCount; ++t)
                    {
                        TextureUnitState tex = (retPass.NumTextureUnitStages <= t ?
                                                retPass.CreateTextureUnitState() :
                                                retPass.GetTextureUnitState(t));
                        pass.GetTextureUnitState(t).CopyTo(tex);
                    }
                    keepTUCount = origPassTUCount;

                    // Have to merge the shadow caster vertex program in
                    retPass.SetFragmentProgram(pass.ShadowCasterFragmentProgramName);
                    GpuProgram prg = retPass.FragmentProgram;
                    // Load this program if not done already
                    if (!prg.IsLoaded)
                        prg.Load();
                    // Copy params
                    retPass.FragmentProgramParameters = pass.ShadowCasterFragmentProgramParameters;
                    // Did we bind a shadow vertex program?
                    if (pass.HasVertexProgram && !retPass.HasVertexProgram)
                    {
                        // We didn't bind a caster-specific program, so bind the original
                        retPass.SetVertexProgram(pass.VertexProgramName);
                        prg = retPass.VertexProgram;
                        // Load this program if required
                        if (!prg.IsLoaded)
                            prg.Load();
                        // Copy params
                        retPass.VertexProgramParameters = pass.VertexProgramParameters;
                    }
                }
                else
                {
                    // Reset any merged fragment programs from last time
                    if (retPass == shadowTextureCustomCasterPass)
                    {
                        // reset fp?
                        if (retPass.FragmentProgramName != shadowTextureCustomCasterFragmentProgram)
                        {
                            retPass.SetFragmentProgram(shadowTextureCustomCasterFragmentProgram);
                            if (retPass.HasFragmentProgram)
                                retPass.FragmentProgramParameters = shadowTextureCustomCasterFPParams;
                        }
                    }
                    else
                        // Standard shadow caster pass, reset to no fp
                        retPass.SetFragmentProgram("");
                }

                // Remove any extra texture units
                while (retPass.NumTextureUnitStages > keepTUCount)
                {
                    retPass.RemoveTextureUnitState(keepTUCount);
                }

                // make sure we turn off alpha rejection
                targetRenderSystem.SetAlphaRejectSettings(CompareFunction.AlwaysPass, 0);

                retPass.Load();


                return retPass;
            }
            else
            {
                return pass;
            }
		}

		/// <summary>
		///		Internal method for turning a regular pass into a shadow receiver pass.
		/// </summary>
		/// <remarks>
		///		This is only used for texture shadows, basically we're trying to
		///		ensure that objects are rendered with a projective texture.
		///		This method will usually return a standard single-texture pass for
		///		all fixed function passes, but will merge in a vertex program
		///		for passes with vertex programs. 
		/// </remarks>
		/// <param name="pass"></param>
		/// <returns></returns>
		protected virtual Pass DeriveShadowReceiverPass(Pass pass) {
			if (IsShadowTechniqueTextureBased) {
				Pass retPass = (shadowTextureCustomReceiverPass != null ?
								shadowTextureCustomReceiverPass : shadowReceiverPass);
				// Does incoming pass have a custom shadow receiver program?
				if (pass.ShadowReceiverVertexProgramName != "") {
					retPass.SetVertexProgram(pass.ShadowReceiverVertexProgramName);
					GpuProgram prg = retPass.VertexProgram;
					// Load this program if not done already
					if (!prg.IsLoaded)
						prg.Load();
					// Copy params
					retPass.VertexProgramParameters = pass.ShadowReceiverVertexProgramParameters;
					// Also have to hack the light autoparams, that is done later
				}
				else {
					if (retPass == shadowTextureCustomReceiverPass) {
						if (shadowTextureCustomReceiverPass.VertexProgramName !=
							shadowTextureCustomReceiverVertexProgram) {
							shadowTextureCustomReceiverPass.SetVertexProgram(shadowTextureCustomReceiverVertexProgram);
							if (retPass.HasVertexProgram)
								retPass.VertexProgramParameters = shadowTextureCustomReceiverVPParams;
						}
					}
					else
						retPass.SetVertexProgram("");
				}
				int keepTUCount;
				// If additive, need lighting parameters & standard programs
				if (IsShadowTechniqueAdditive) {
					keepTUCount = 1;
					retPass.LightingEnabled = true;
					retPass.Ambient = pass.Ambient;
					retPass.Diffuse = pass.Diffuse;
					retPass.Specular = pass.Specular;
					retPass.Shininess = pass.Shininess;
					retPass.SetRunNTimesPerLight(pass.RunOncePerLight,
                                                pass.PassIterationCount,
                                                pass.LightsPerIteration,
											    pass.RunOnlyForOneLightType,
											    pass.OnlyLightType);
					int origPassTUCount = pass.NumTextureUnitStages;
					for (int t = 0; t < origPassTUCount; ++t) {
						int targetIndex = t+1;
						TextureUnitState tex = (retPass.NumTextureUnitStages <= targetIndex ?
												retPass.CreateTextureUnitState() :
												retPass.GetTextureUnitState(targetIndex));
						pass.GetTextureUnitState(t).CopyTo(tex);
					}
					keepTUCount = origPassTUCount + 1;
					// Will also need fragment programs since this is a complex light setup
					if (pass.ShadowReceiverFragmentProgramName != "") {
						// Have to merge the shadow receiver vertex program in
						retPass.SetFragmentProgram(pass.ShadowReceiverFragmentProgramName);
						GpuProgram prg = retPass.FragmentProgram;
						// Load this program if not done already
						if (!prg.IsLoaded)
							prg.Load();
						// Copy params
						retPass.FragmentProgramParameters = pass.ShadowReceiverFragmentProgramParameters;
						// Did we bind a shadow vertex program?
						if (pass.HasVertexProgram && !retPass.HasVertexProgram) {
							// We didn't bind a receiver-specific program, so bind the original
							retPass.SetVertexProgram(pass.VertexProgramName);
							prg = retPass.VertexProgram;
							// Load this program if required
							if (!prg.IsLoaded)
								prg.Load();
							// Copy params
							retPass.VertexProgramParameters = pass.VertexProgramParameters;
						}
					}
					else {
						// Reset any merged fragment programs from last time
						if (retPass == shadowTextureCustomReceiverPass) {
							// reset fp?
							if (retPass.FragmentProgramName != shadowTextureCustomReceiverFragmentProgram) {
								retPass.SetFragmentProgram(shadowTextureCustomReceiverFragmentProgram);
								if(retPass.HasFragmentProgram)
									retPass.FragmentProgramParameters = shadowTextureCustomReceiverFPParams;
							}
						}
						else
							// Standard shadow receiver pass, reset to no fp
							retPass.SetFragmentProgram("");
					}
				}
				// additive lighting
				else
					// need to keep spotlight fade etc
					keepTUCount = retPass.NumTextureUnitStages;

				// Remove any extra texture units
				while (retPass.NumTextureUnitStages > keepTUCount)
					retPass.RemoveTextureUnitState(keepTUCount);
				retPass.Load();
				return retPass;
			}
			else
				return pass;
		}

		/// <summary>
		///		Internal method for rendering all the objects for a given light into the stencil buffer.
		/// </summary>
		/// <param name="light">The light source.</param>
		/// <param name="camera">The camera being viewed from.</param>
		protected virtual void RenderShadowVolumesToStencil(Light light, Camera camera) {
			// get the shadow caster list
			IList casters = FindShadowCastersForLight(light, camera);
			if (casters.Count == 0)
				// No casters, just do nothing
				return;
			
			// Set up scissor test (point & spot lights only)
			bool scissored = false;
			if (light.Type != LightType.Directional && 
				targetRenderSystem.Caps.CheckCap(Capabilities.ScissorTest)) {
				// Project the sphere onto the camera
				float left, right, top, bottom;
				Sphere sphere = new Sphere(light.DerivedPosition, light.AttenuationRange);
				if (camera.ProjectSphere(sphere, out left, out top, out right, out bottom)) {
					scissored = true;
					// Turn normalised device coordinates into pixels
					int iLeft, iTop, iWidth, iHeight;
					currentViewport.GetActualDimensions(out iLeft, out iTop, out iWidth, out iHeight);
					int szLeft, szRight, szTop, szBottom;

					szLeft = (int)(iLeft + ((left + 1) * 0.5f * iWidth));
					szRight = (int)(iLeft + ((right + 1) * 0.5f * iWidth));
					szTop = (int)(iTop + ((-top + 1) * 0.5f * iHeight));
					szBottom = (int)(iTop + ((-bottom + 1) * 0.5f * iHeight));

					targetRenderSystem.SetScissorTest(true, szLeft, szTop, szRight, szBottom);
				}
			}

			targetRenderSystem.UnbindGpuProgram(GpuProgramType.Fragment);

			// Can we do a 2-sided stencil?
			bool stencil2sided = false;

			if (targetRenderSystem.Caps.CheckCap(Capabilities.TwoSidedStencil) && 
				targetRenderSystem.Caps.CheckCap(Capabilities.StencilWrap)) {
				// enable
				stencil2sided = true;
			}

			// Do we have access to vertex programs?
			bool extrudeInSoftware = true;

			bool finiteExtrude = !shadowUseInfiniteFarPlane || 
				!targetRenderSystem.Caps.CheckCap(Capabilities.InfiniteFarPlane);

			if (targetRenderSystem.Caps.CheckCap(Capabilities.VertexPrograms)) {
				extrudeInSoftware = false;
				EnableHardwareShadowExtrusion(light, finiteExtrude);
			}
			else {
				targetRenderSystem.UnbindGpuProgram(GpuProgramType.Vertex);
			}

			// Add light to internal list for use in render call
            List<Light> tmpLightList = new List<Light>();
			tmpLightList.Add(light);

			// Turn off color writing and depth writing
			targetRenderSystem.SetColorBufferWriteEnabled(false, false, false, false);
			targetRenderSystem.DepthWrite = false;
			targetRenderSystem.StencilCheckEnabled = true;
			targetRenderSystem.DepthFunction = CompareFunction.Less;

			// Calculate extrusion distance
			float extrudeDistance = 0;
			if (light.Type == LightType.Directional) {
				extrudeDistance = shadowDirLightExtrudeDist;
			}

			// get the near clip volume
			PlaneBoundedVolume nearClipVol = light.GetNearClipVolume(camera);

			// Determine whether zfail is required
			// We need to use zfail for ALL objects if we find a single object which
			// requires it
			bool zfailAlgo = false;

			CheckShadowCasters(casters, nearClipVol, light, extrudeInSoftware, finiteExtrude, zfailAlgo, camera, extrudeDistance, stencil2sided, tmpLightList);
			// revert colour write state
			targetRenderSystem.SetColorBufferWriteEnabled(true, true, true, true);
			// revert depth state
			targetRenderSystem.SetDepthBufferParams();

			targetRenderSystem.StencilCheckEnabled = false;

			targetRenderSystem.UnbindGpuProgram(GpuProgramType.Vertex);

			if (scissored) {
				// disable scissor test
				targetRenderSystem.SetScissorTest(false);
			}
		}

		private void EnableHardwareShadowExtrusion(Light light, bool finiteExtrude)
		{
			// attach the appropriate extrusion vertex program
			// Note we never unset it because support for vertex programs is constant
			shadowStencilPass.SetVertexProgram(
				ShadowVolumeExtrudeProgram.GetProgramName(light.Type, finiteExtrude, false));

			// Set params
			if (finiteExtrude) {
				shadowStencilPass.VertexProgramParameters = finiteExtrusionParams;
			}
			else {
				shadowStencilPass.VertexProgramParameters = infiniteExtrusionParams;
			}

			if(showDebugShadows) {
				shadowDebugPass.SetVertexProgram(
					ShadowVolumeExtrudeProgram.GetProgramName(light.Type, finiteExtrude, true));

				// Set params
				if (finiteExtrude) {
					shadowDebugPass.VertexProgramParameters = finiteExtrusionParams;
				}
				else {
					shadowDebugPass.VertexProgramParameters = infiniteExtrusionParams;
				}
			}

			targetRenderSystem.BindGpuProgram(shadowStencilPass.VertexProgram);
		}

		private void CheckShadowCasters(IList casters, PlaneBoundedVolume nearClipVol, Light light, bool extrudeInSoftware, bool finiteExtrude, bool zfailAlgo, Camera camera, float extrudeDistance, bool stencil2sided, List<Light> tmpLightList)
		{
			int flags;
			for(int i = 0; i < casters.Count; i++) {
				ShadowCaster caster = (ShadowCaster)casters[i];

				if(nearClipVol.Intersects(caster.GetWorldBoundingBox())) {
					// We have a zfail case, we must use zfail for all objects
					zfailAlgo = true;

					break;
				}
			}

			for(int ci = 0; ci < casters.Count; ci++) {
				ShadowCaster caster = (ShadowCaster)casters[ci];
				flags = 0;

				if (light.Type != LightType.Directional) {
					extrudeDistance = caster.GetPointExtrusionDistance(light); 
				}

				if (!extrudeInSoftware && !finiteExtrude) {
					// hardware extrusion, to infinity (and beyond!)
					flags |= (int) ShadowRenderableFlags.ExtrudeToInfinity;
				}

				if(zfailAlgo) {
					// We need to include the light and / or dark cap
					// But only if they will be visible
					if(camera.IsObjectVisible(caster.GetLightCapBounds())) {
						flags |= (int) ShadowRenderableFlags.IncludeLightCap;
					}
				}

				// Dark cap (no dark cap for directional lights using 
				// hardware extrusion to infinity)
				if(!((flags & (int)ShadowRenderableFlags.ExtrudeToInfinity) != 0  && 
				     light.Type == LightType.Directional) &&
				   camera.IsObjectVisible(caster.GetDarkCapBounds(light, extrudeDistance))) {
				   	flags |= (int) ShadowRenderableFlags.IncludeDarkCap;
				   }

				// get shadow renderables
				IEnumerator renderables = caster.GetShadowVolumeRenderableEnumerator(
					shadowTechnique, light, shadowIndexBuffer, extrudeInSoftware, extrudeDistance, flags);

				// If using one-sided stencil, render the first pass of all shadow
				// renderables before all the second passes
				for (int i = 0; i < (stencil2sided ? 1 : 2); i++) {
					if (i == 1)
						renderables = caster.GetLastShadowVolumeRenderableEnumerator();
					
					while(renderables.MoveNext()) {
						ShadowRenderable sr = (ShadowRenderable)renderables.Current;

						// omit hidden renderables
						if (sr.IsVisible) {
							// render volume, including dark and (maybe) light caps
							RenderSingleShadowVolumeToStencil(sr, zfailAlgo, stencil2sided, tmpLightList, (i > 0));

							// optionally render separate light cap
							if (sr.IsLightCapSeperate && ((flags & (int)ShadowRenderableFlags.IncludeLightCap)) > 0) {
								// must always fail depth check
								targetRenderSystem.DepthFunction = CompareFunction.AlwaysFail;

								Debug.Assert(sr.LightCapRenderable != null, "Shadow renderable is missing a separate light cap renderable!");

								RenderSingleShadowVolumeToStencil(sr.LightCapRenderable, zfailAlgo, stencil2sided, tmpLightList, (i > 0));
								// reset depth function
								targetRenderSystem.DepthFunction = CompareFunction.Less;
							}
						}
					}
				}
			}
		}

		/// <summary>
		///		Internal utility method for setting stencil state for rendering shadow volumes.
		/// </summary>
		/// <param name="secondPass">Is this the second pass?</param>
		/// <param name="zfail">Should we be using the zfail method?</param>
		/// <param name="twoSided">Should we use a 2-sided stencil?</param>
		protected virtual void SetShadowVolumeStencilState(bool secondPass, bool zfail, bool twoSided) {
			// First pass, do front faces if zpass
			// Second pass, do back faces if zpass
			// Invert if zfail
			// this is to ensure we always increment before decrement
			if ((secondPass ^ zfail)) {
				targetRenderSystem.CullingMode = twoSided ? CullingMode.None : CullingMode.CounterClockwise;
				targetRenderSystem.SetStencilBufferParams(
					CompareFunction.AlwaysPass, // always pass stencil check
					0, // no ref value (no compare)
					unchecked((int)0xffffffff), // no mask
					StencilOperation.Keep, // stencil test will never fail
					zfail ? (twoSided ? StencilOperation.IncrementWrap : StencilOperation.Increment) : StencilOperation.Keep, // back face depth fail
					zfail ? StencilOperation.Keep : (twoSided ? StencilOperation.DecrementWrap : StencilOperation.Decrement), // back face pass
					twoSided);
			}
			else {
				targetRenderSystem.CullingMode = twoSided ? CullingMode.None : CullingMode.Clockwise;
				targetRenderSystem.SetStencilBufferParams(
					CompareFunction.AlwaysPass, // always pass stencil check
					0, // no ref value (no compare)
					unchecked((int)0xffffffff), // no mask
					StencilOperation.Keep, // stencil test will never fail
					zfail ? (twoSided ? StencilOperation.DecrementWrap : StencilOperation.Decrement) : StencilOperation.Keep, // front face depth fail
					zfail ? StencilOperation.Keep : (twoSided ? StencilOperation.IncrementWrap : StencilOperation.Increment), // front face pass
					twoSided);
			}
		}

		/// <summary>
		///		Render a single shadow volume to the stencil buffer.
		/// </summary>
		/// <param name="sr"></param>
		/// <param name="zfail"></param>
		/// <param name="stencil2sided"></param>
		protected void RenderSingleShadowVolumeToStencil(ShadowRenderable sr, bool zfail, bool stencil2sided, List<Light> manualLightList, bool isSecondPass) {
			// Render a shadow volume here
			//  - if we have 2-sided stencil, one render with no culling
			//  - otherwise, 2 renders, one with each culling method and invert the ops
			if (!isSecondPass) {
				SetShadowVolumeStencilState(false, zfail, stencil2sided);
				RenderSingleObject(sr, shadowStencilPass, false, manualLightList);
			}

			if (!stencil2sided && isSecondPass) {
				// Second pass
				SetShadowVolumeStencilState(true, zfail, false);
				RenderSingleObject(sr, shadowStencilPass, false);
			}

			// Do we need to render a debug shadow marker?
			if(showDebugShadows && (isSecondPass || stencil2sided)) {
				// reset stencil & colour ops
				targetRenderSystem.SetStencilBufferParams();
				SetPass(shadowDebugPass);
				RenderSingleObject(sr, shadowDebugPass, false, manualLightList);
				targetRenderSystem.SetColorBufferWriteEnabled(false, false, false, false);
			}
		}

        private static TimingMeter setPassMeter = MeterManager.GetMeter("Set Pass", "Render Scene");
        private static TimingMeter setPassShadersMeter = MeterManager.GetMeter("Set Pass Shaders", "Render Scene");
        private static TimingMeter setPassLightingFogMeter = MeterManager.GetMeter("Set Pass Lighting and Fog", "Render Scene");
        private static TimingMeter setPassEarlyCommonMeter = MeterManager.GetMeter("Set Pass Early Common", "Render Scene");
        private static TimingMeter setPassTexturesMeter = MeterManager.GetMeter("Set Pass Textures", "Render Scene");
        private static TimingMeter setPassLateCommonMeter = MeterManager.GetMeter("Set Pass Late Common", "Render Scene");

		/// <summary>Internal method for setting up the renderstate for a rendering pass.</summary>
		/// <param name="pass">The Pass details to set.</param>
        /// <param name="evenIfSuppressed">
        ///    Sets the pass details even if render state
        ///    changes are suppressed; if you are using this to manually set state
        ///    when render state changes are suppressed, you should set this to true.
        /// </param>
        /// <param name="shadowDerivation">
        ///    If false, disables the derivation of shadow passes from original passes
        /// </param>
        /// <returns>
		///		A Pass object that was used instead of the one passed in, can
		///		happen when rendering shadow passes
		///	</returns>
   		protected virtual Pass SetPass(Pass pass, bool evenIfSuppressed, bool shadowDerivation)
        {
            setPassMeter.Enter();

            targetRenderSystem.BeginProfileEvent(ColorEx.Green, "SetPass: Material = " + pass.Parent.Parent.Name);
            totalSetPassCalls++;
            if (!suppressRenderStateChanges || evenIfSuppressed) {
				if (illuminationStage == IlluminationRenderStage.RenderToTexture && shadowDerivation) {
						// Derive a special shadow caster pass from this one
					pass = DeriveShadowCasterPass(pass);
    			} else if (illuminationStage == IlluminationRenderStage.RenderModulativePass) {
					pass = DeriveShadowReceiverPass(pass);
				}
                
                // Tell params about current pass
                autoParamDataSource.CurrentPass = pass;

                bool passSurfaceAndLightParams = true;

                //setPassShadersMeter.Enter();
				if (pass.HasVertexProgram) {
					targetRenderSystem.BindGpuProgram(pass.VertexProgram.BindingDelegate);
					// bind parameters later since they can be per-object
					// does the vertex program want surface and light params passed to rendersystem?
					passSurfaceAndLightParams = pass.VertexProgram.PassSurfaceAndLightStates;
				} else {
					// Unbind program?
					if (targetRenderSystem.IsGpuProgramBound(GpuProgramType.Vertex)) {
						targetRenderSystem.UnbindGpuProgram(GpuProgramType.Vertex);
					}
					// Set fixed-function vertex parameters
				}

                // Using a fragment program?
                if (pass.HasFragmentProgram)
                {
                    targetRenderSystem.BindGpuProgram(pass.FragmentProgram.BindingDelegate);
                    // bind parameters later since they can be per-object
                }
                else
                {
                    // Unbind program?
                    if (targetRenderSystem.IsGpuProgramBound(GpuProgramType.Fragment))
                    {
                        targetRenderSystem.UnbindGpuProgram(GpuProgramType.Fragment);
                    }
                }
                //setPassShadersMeter.Exit();

                //setPassLightingFogMeter.Enter();

				if (passSurfaceAndLightParams) {
					// Set surface reflectance properties, only valid if lighting is enabled
					if (pass.LightingEnabled) {
						targetRenderSystem.SetSurfaceParams(pass.Ambient,
															pass.Diffuse,
															pass.Specular,
															pass.Emissive,
															pass.Shininess,
                                                            pass.VertexColorTracking);
// #if NOT
					} else {
						// even with lighting off, we need ambient set to white
						targetRenderSystem.SetSurfaceParams(ColorEx.White,
															ColorEx.Black,
															ColorEx.Black,
															ColorEx.Black,
                                                            0,
                                                            TrackVertexColor.None);
// #endif
                    }
                    // Dynamic lighting enabled?
					targetRenderSystem.LightingEnabled = pass.LightingEnabled;
				}

				// Set fixed-function fragment settings

				// Fog (assumes we want pixel fog which is the usual)
				// New fog params can either be from scene or from material

				// jsw - set the fog for both fixed function and fragment programs
				ColorEx newFogColor;
				FogMode newFogMode;
				float newFogDensity, newFogStart, newFogEnd;

				// does the pass want to override the fog mode?
				if (pass.FogOverride) {
					// New fog params from material
				    newFogMode = pass.FogMode;
					newFogColor = pass.FogColor;
					newFogDensity = pass.FogDensity;
					newFogStart = pass.FogStart;
					newFogEnd = pass.FogEnd;
				} else {
					// New fog params from scene
					newFogMode = fogMode;
					newFogColor = fogColor;
					newFogDensity = fogDensity;
					newFogStart = fogStart;
					newFogEnd = fogEnd;
				}

                // set fog params
                float fogScale = 1f;
                if (newFogMode == FogMode.None)
                    fogScale = 0f;

				// set fog using the render system
				targetRenderSystem.SetFog(newFogMode, newFogColor, newFogDensity, newFogStart, newFogEnd);

                // Tell params about ORIGINAL fog Need to be able to
                // override fixed function fog, but still have
                // original fog parameters available to a shader than
                // chooses to use
                autoParamDataSource.SetFog(fogMode, fogColor, fogStart, fogEnd, fogScale);

                //setPassLightingFogMeter.Exit();

                //setPassEarlyCommonMeter.Enter();

				// The rest of the settings are the same no matter whether we use programs or not

				// Set scene blending
				targetRenderSystem.SetSceneBlending(pass.SourceBlendFactor, pass.DestBlendFactor);

                // Set point parameters
                targetRenderSystem.SetPointParameters(pass.PointSize,
                                                      pass.PointAttenuationEnabled, 
                                                      pass.PointAttenuationConstant, 
                                                      pass.PointAttenuationLinear, 
                                                      pass.PointAttenuationQuadratic, 
                                                      pass.PointMinSize, 
                                                      pass.PointMaxSize);

                targetRenderSystem.PointSpritesEnabled = pass.PointSpritesEnabled;

                //setPassEarlyCommonMeter.Exit();
                // Texture unit settings

                //setPassTexturesMeter.Enter();
				// set all required texture units for this pass, and disable ones not being used
				int numTextureUnits;
				if (pass.HasFragmentProgram) {
					numTextureUnits = pass.FragmentProgram.SamplerCount;
				} else {
					numTextureUnits = targetRenderSystem.Caps.TextureUnitCount;
				}

                int shadowTexIndex = pass.StartLight;
				for (int i = 0; i < numTextureUnits; i++) {
                    if (i < pass.NumTextureUnitStages)
                    {
                        TextureUnitState texUnit = pass.GetTextureUnitState(i);

                        // If we couldn't load the texture, skip it
                        if (texUnit.Blank)
                            continue;

                        if (pass.RunOncePerLight &&
                            IsShadowTechniqueTextureBased &&
                            texUnit.ContentType == TextureContentType.Shadow)
                        {
                            // Need to bind the correct shadow texture, based on the start light
                            // Even though the light list can change per object, our restrictions
                            // say that when texture shadows are enabled, the lights up to the
                            // number of texture shadows will be fixed for all objects
                            // to match the shadow textures that have been generated
                            // see ShadowListener::sortLightsAffectingFrustum and
                            // MovableObject::Listener::objectQueryLights
                            // Note that light iteration throws the indexes out so we don't bind here
                            // if that's the case, we have to bind when lights are iterated
                            // in renderSingleObject
                            Texture shadowTex;
                            if (shadowTexIndex < shadowTextures.Count)
                            {
                                shadowTex = shadowTextures[shadowTexIndex];
                                // Hook up projection frustum
                                Camera cam = shadowTex.GetBuffer().GetRenderTarget().GetViewport(0).Camera;
                                // Only set fixed-function projection if no vertex program
                                if (!pass.HasVertexProgram)
                                    texUnit.SetProjectiveTexturing(true, cam);
                                autoParamDataSource.SetTextureProjector(cam, shadowTexIndex);
                            }
                            else
                            {
                                // Use fallback 'null' shadow texture
                                // no projection since all uniform colour anyway
                                shadowTex = nullShadowTexture;
                                texUnit.SetProjectiveTexturing(false, null);
                                autoParamDataSource.SetTextureProjector(null, shadowTexIndex);

                            }
                            texUnit.SetTexturePtr(shadowTex);
                            shadowTexIndex++;
                        }
                        targetRenderSystem.SetTextureUnit(i, texUnit, !pass.HasFragmentProgram);
                    }
                    else
                    {
                        if (pass.HasFragmentProgram)
                        {

                            targetRenderSystem.SetTextureCoordSet(i, i);
                        }
                        else
                        {
                            // Disable the ones that the pass doesn't use.
                            targetRenderSystem.DisableTextureUnit(i);
                        }
                    }
    			}

                //setPassTexturesMeter.Exit();

                //setPassLateCommonMeter.Enter();
                // Set up non-texture related material settings
				// Depth Settings
				targetRenderSystem.DepthFunction = pass.DepthFunction;
				targetRenderSystem.DepthCheck = pass.DepthCheck;
				targetRenderSystem.DepthWrite = pass.DepthWrite;
				targetRenderSystem.SetDepthBias(pass.DepthBiasConstant, pass.DepthBiasSlopeScale);

                // Alpha-reject settings
                targetRenderSystem.SetAlphaRejectSettings(pass.AlphaRejectFunction, (byte)pass.AlphaRejectValue);

				// Color Write
				// right now only using on/off, not per channel
				bool colWrite = pass.ColorWrite;
				targetRenderSystem.SetColorBufferWriteEnabled(colWrite, colWrite, colWrite, colWrite);

				// Culling Mode
                if (IsShadowTechniqueTextureBased &&
                    illuminationStage == IlluminationRenderStage.RenderToTexture &&
                    shadowCasterRenderBackFaces &&
                    pass.CullMode == CullingMode.Clockwise) {
                    // render back faces into shadow caster, can help with depth comparison
                    targetRenderSystem.CullingMode = CullingMode.CounterClockwise;
                }
                else
                    targetRenderSystem.CullingMode = pass.CullMode;

				// Shading mode
				targetRenderSystem.ShadingMode = pass.ShadingMode;

				// Polygon mode
                targetRenderSystem.RasterizationMode = pass.SceneDetail;

                //setPassLateCommonMeter.Exit();

                // set pass number
				autoParamDataSource.PassNumber = pass.Index;
			}

            targetRenderSystem.EndProfileEvent();
            setPassMeter.Exit();
            return pass;
        }

        /// <summary>
        ///		If only the first parameter is supplied
        /// </summary>
        protected virtual Pass SetPass(Pass pass) {
            return SetPass(pass, false, true);
        }

        /// <summary>
        ///		If only the first two parameters are supplied
        /// </summary>
        protected virtual Pass SetPass(Pass pass, bool evenIfSuppressed) {
            return SetPass(pass, evenIfSuppressed, true);
        }
		
		/// <summary>
		///		Utility method for creating the planes of a skybox.
		/// </summary>
		/// <param name="plane"></param>
		/// <param name="distance"></param>
		/// <param name="orientation"></param>
		/// <returns></returns>
		protected Mesh CreateSkyboxPlane(BoxPlane plane, float distance, Quaternion orientation) {
			Plane p = new Plane();
			string meshName = "SkyboxPlane_";
			Vector3 up = Vector3.Zero;

			// set the distance of the plane
			p.D = distance;

			switch(plane) {
				case BoxPlane.Front:
					p.Normal = Vector3.UnitZ;
					up = Vector3.UnitY;
					meshName += "Front";
					break;
				case BoxPlane.Back:
					p.Normal = -Vector3.UnitZ;
					up = Vector3.UnitY;
					meshName += "Back";
					break;
				case BoxPlane.Left:
					p.Normal = Vector3.UnitX;
					up = Vector3.UnitY;
					meshName += "Left";
					break;
				case BoxPlane.Right:
					p.Normal = -Vector3.UnitX;
					up = Vector3.UnitY;
					meshName += "Right";
					break;
				case BoxPlane.Up:
					p.Normal = -Vector3.UnitY;
					up = Vector3.UnitZ;
					meshName += "Up";
					break;
				case BoxPlane.Down:
					p.Normal = Vector3.UnitY;
					up = -Vector3.UnitZ;
					meshName += "Down";
					break;
			}

			// modify by orientation
			p.Normal = orientation * p.Normal;
			up = orientation * up;

			MeshManager modelMgr = MeshManager.Instance;

			// see if this mesh exists
			Mesh planeModel = modelMgr.GetByName(meshName);

			// trash it if it already exists
			if(planeModel != null)
				modelMgr.Unload(planeModel);

			float planeSize = distance * 2;

			// create and return the plane mesh
			return modelMgr.CreatePlane(meshName, p, planeSize, planeSize, 1, 1, false, 1, 1, 1, up);
		}

		/// <summary>
		///    
		/// </summary>
		/// <param name="plane"></param>
		/// <param name="curvature"></param>
		/// <param name="tiling"></param>
		/// <param name="distance"></param>
		/// <param name="orientation"></param>
		/// <returns></returns>
		protected Mesh CreateSkyDomePlane(BoxPlane plane, float curvature, float tiling, float distance, Quaternion orientation) {
			Plane p = new Plane();
			Vector3 up = Vector3.Zero;
			string meshName = "SkyDomePlane_";

			// set up plane equation
			p.D = distance;

			switch(plane) {
				case BoxPlane.Front:
					p.Normal = Vector3.UnitZ;
					up = Vector3.UnitY;
					meshName += "Front";
					break;
				case BoxPlane.Back:
					p.Normal = -Vector3.UnitZ;
					up = Vector3.UnitY;
					meshName += "Back";
					break;
				case BoxPlane.Left:
					p.Normal = Vector3.UnitX;
					up = Vector3.UnitY;
					meshName += "Left";
					break;
				case BoxPlane.Right:
					p.Normal = -Vector3.UnitX;
					up = Vector3.UnitY;
					meshName += "Right";
					break;
				case BoxPlane.Up:
					p.Normal = -Vector3.UnitY;
					up = Vector3.UnitZ;
					meshName += "Up";
					break;
				case BoxPlane.Down:
					return null;
			}

			// modify orientation
			p.Normal = orientation * p.Normal;
			up = orientation * up;

			// check to see if mesh exists
			MeshManager meshManager = MeshManager.Instance;
			Mesh planeMesh = meshManager.GetByName(meshName);

			// destroy existing
			if(planeMesh != null) {
				meshManager.Unload(planeMesh);
				planeMesh.Dispose();
			}

			// create new
			float planeSize = distance * 2;
			int segments = 16;
			planeMesh = 
				meshManager.CreateCurvedIllusionPlane(
				meshName, p, planeSize, planeSize, curvature, segments, segments, 
				false, 1, tiling, tiling, up, orientation, 
				BufferUsage.DynamicWriteOnly, BufferUsage.StaticWriteOnly, true, true);

			return planeMesh;
		}

		/// <summary>
		///		Protected method used by RenderVisibleObjects to deal with renderables
		///		which override the camera's own view / projection materices.
		/// </summary>
		/// <param name="renderable"></param>
#if NOT
		protected void UseRenderableViewProjection(IRenderable renderable) {
			bool useIdentityView = renderable.UseIdentityView;
			bool useIdentityProj = renderable.UseIdentityProjection;

			// View
			if(useIdentityView && (hasCameraChanged || !lastViewWasIdentity)) {
				// using identity view now, so change it
				targetRenderSystem.ViewMatrix = Matrix4.Identity;
				lastViewWasIdentity = true;
			}
			else if (!useIdentityView && (hasCameraChanged || lastViewWasIdentity)){
				targetRenderSystem.ViewMatrix = cameraInProgress.ViewMatrix;
				lastViewWasIdentity = false;
			}

			// Projection
			if(useIdentityProj && (hasCameraChanged || !lastProjectionWasIdentity)) {
				// using identity view now, so change it
				targetRenderSystem.ProjectionMatrix = Matrix4.Identity;
				lastProjectionWasIdentity = true;
			}
			else if(!useIdentityProj && (hasCameraChanged || lastProjectionWasIdentity)) {
				targetRenderSystem.ProjectionMatrix = cameraInProgress.ProjectionMatrix;
				lastProjectionWasIdentity = false;
			}

			// reset this flag so the view/proj wont be updated again this frame
			hasCameraChanged = false;
		}
#else
		protected void UseRenderableViewProjection(IRenderable renderable) {
			// View
            bool useIdentityView = renderable.UseIdentityView;
            if (useIdentityView) {
				// Using identity view now, change it
				targetRenderSystem.ViewMatrix = Matrix4.Identity;
// 				lastViewWasIdentity = true;
                resetIdentityView = true;
			}

			// Projection
            bool useIdentityProj = renderable.UseIdentityProjection;
            if (useIdentityProj) {
				// Use identity projection matrix, still need to take RS depth into account
                Matrix4 mat = targetRenderSystem.ConvertProjectionMatrix(Matrix4.Identity);
                targetRenderSystem.ProjectionMatrix = mat;
// 				lastProjectionWasIdentity = true;
                resetIdentityProj = true;
			}
		}

#endif

        protected void ResetViewProjMode() {
            if (resetIdentityView) {
                // Coming back to normal from identity view
                targetRenderSystem.ViewMatrix = cameraInProgress.GetViewMatrix(true);
                resetIdentityView = false;
            }

            if (resetIdentityProj) {
                // Coming back from flat projection
                targetRenderSystem.ProjectionMatrix = cameraInProgress.ProjectionMatrixRS;
                resetIdentityProj = false;
            }
        }

        /// <summary>
		///		Used to first the QueueStarted event.  
		/// </summary>
		/// <param name="group"></param>
		/// <returns>True if the queue should be skipped.</returns>
		protected bool OnRenderQueueStarted(RenderQueueGroupID group) {
			if(QueueStarted != null) {
				return QueueStarted(group);
			}

			return false;
		}

		/// <summary>
		///		Used to first the QueueEnded event.  
		/// </summary>
		/// <param name="group"></param>
		/// <returns>True if the queue should be repeated.</returns>
		protected bool OnRenderQueueEnded(RenderQueueGroupID group) {
			if(QueueEnded != null) {
				return QueueEnded(group);
			}

			return false;
		}

		#endregion

		#region Public methods
		
		public void RekeySceneNode(string oldName, SceneNode node) 
		{
			if(this.sceneNodeList[oldName] == node)
			{
				this.sceneNodeList.Remove(oldName);
				this.sceneNodeList[node.Name] = node;
			}
		}


		/// <summary>
		///		Creates a <see cref="AxisAlignedBoxRegionSceneQuery"/> for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		for querying for objects within a AxisAlignedBox region.
		/// </remarks>
		/// <returns>A specialized implementation of AxisAlignedBoxRegionSceneQuery for this scene manager.</returns>
		public AxisAlignedBoxRegionSceneQuery CreateAABBRegionQuery() {
			return CreateAABBRegionQuery(new AxisAlignedBox(), 0xffffffff);
		}

		/// <summary>
		///		Creates a <see cref="AxisAlignedBoxRegionSceneQuery"/> for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		for querying for objects within a AxisAlignedBox region.
		/// </remarks>
		/// <param name="box">AxisAlignedBox to use for the region query.</param>
		/// <returns>A specialized implementation of AxisAlignedBoxRegionSceneQuery for this scene manager.</returns>
		public AxisAlignedBoxRegionSceneQuery CreateAABBRegionQuery(AxisAlignedBox box) {
			return CreateAABBRegionQuery(box, 0xffffffff);
		}

		/// <summary>
		///		Creates a <see cref="AxisAlignedBoxRegionSceneQuery"/> for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		for querying for objects within a AxisAlignedBox region.
		/// </remarks>
		/// <param name="box">AxisAlignedBox to use for the region query.</param>
		/// <param name="mask">Custom user defined flags to use for the query.</param>
		/// <returns>A specialized implementation of AxisAlignedBoxRegionSceneQuery for this scene manager.</returns>
		public virtual AxisAlignedBoxRegionSceneQuery CreateAABBRegionQuery(AxisAlignedBox box, ulong mask) {
			DefaultAxisAlignedBoxRegionSceneQuery query = new DefaultAxisAlignedBoxRegionSceneQuery(this);
			query.Box = box;
			query.QueryMask = mask;

			return query;
		}

		/// <summary>
		///    Creates a query to return objects found along the ray.
		/// </summary>
		/// <returns>A specialized implementation of RaySceneQuery for this scene manager.</returns>
		public virtual RaySceneQuery CreateRayQuery() {
			return CreateRayQuery(new Ray(), 0xffffffff);
		}

		/// <summary>
		///    Creates a query to return objects found along the ray.
		/// </summary>
		/// <param name="ray">Ray to use for the intersection query.</param>
		/// <returns>A specialized implementation of RaySceneQuery for this scene manager.</returns>
		public virtual RaySceneQuery CreateRayQuery(Ray ray) {
			return CreateRayQuery(ray, 0xffffffff);
		}

		/// <summary>
		///    Creates a query to return objects found along the ray. 
		/// </summary>
		/// <param name="ray">Ray to use for the intersection query.</param>
		/// <returns>A specialized implementation of RaySceneQuery for this scene manager.</returns>
		public virtual RaySceneQuery CreateRayQuery(Ray ray, ulong mask) {
			DefaultRaySceneQuery query = new DefaultRaySceneQuery(this);
			query.Ray = ray;
			query.QueryMask = mask;
			return query;
		}

		/// <summary>
		///		Creates a <see cref="SphereRegionSceneQuery"/> for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		for querying for objects within a spherical region.
		/// </remarks>
		/// <returns>A specialized implementation of SphereRegionSceneQuery for this scene manager.</returns>
		public SphereRegionSceneQuery CreateSphereRegionQuery() {
			return CreateSphereRegionQuery(new Sphere(), 0xffffffff);
		}

		/// <summary>
		///		Creates a <see cref="SphereRegionSceneQuery"/> for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		for querying for objects within a spherical region.
		/// </remarks>
		/// <param name="sphere">Sphere to use for the region query.</param>
		/// <returns>A specialized implementation of SphereRegionSceneQuery for this scene manager.</returns>
		public SphereRegionSceneQuery CreateSphereRegionQuery(Sphere sphere) {
			return CreateSphereRegionQuery(sphere, 0xffffffff);
		}

		/// <summary>
		///		Creates a <see cref="SphereRegionSceneQuery"/> for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		for querying for objects within a spherical region.
		/// </remarks>
		/// <param name="sphere">Sphere to use for the region query.</param>
		/// <param name="mask">Custom user defined flags to use for the query.</param>
		/// <returns>A specialized implementation of SphereRegionSceneQuery for this scene manager.</returns>
		public virtual SphereRegionSceneQuery CreateSphereRegionQuery(Sphere sphere, ulong mask) {
			DefaultSphereRegionSceneQuery query = new DefaultSphereRegionSceneQuery(this);
			query.Sphere = sphere;
			query.QueryMask = mask;

			return query;
		}

		/// <summary>
		///		Creates a <see cref="PlaneBoundedVolumeListSceneQuery"/> for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		for querying for objects within a PlaneBoundedVolumes region.
		/// </remarks>
		/// <returns>A specialized implementation of PlaneBoundedVolumeListSceneQuery for this scene manager.</returns>
		public PlaneBoundedVolumeListSceneQuery CreatePlaneBoundedVolumeQuery() {
			return CreatePlaneBoundedVolumeQuery(new List<PlaneBoundedVolume>(), 0xffffffff);
		}

		/// <summary>
		///		Creates a <see cref="PlaneBoundedVolumeListSceneQuery"/> for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		for querying for objects within a PlaneBoundedVolumes region.
		/// </remarks>
		/// <param name="volumes">PlaneBoundedVolumeList to use for the region query.</param>
		/// <returns>A specialized implementation of PlaneBoundedVolumeListSceneQuery for this scene manager.</returns>
		public PlaneBoundedVolumeListSceneQuery CreatePlaneBoundedVolumeQuery(List<PlaneBoundedVolume> volumes) {
			return CreatePlaneBoundedVolumeQuery(volumes, 0xffffffff);
		}

		/// <summary>
		///		Creates a <see cref="PlaneBoundedVolumeListSceneQuery"/> for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		for querying for objects within a PlaneBoundedVolumes region.
		/// </remarks>
		/// <param name="volumes">PlaneBoundedVolumeList to use for the region query.</param>
		/// <param name="mask">Custom user defined flags to use for the query.</param>
		/// <returns>A specialized implementation of PlaneBoundedVolumeListSceneQuery for this scene manager.</returns>
		public virtual PlaneBoundedVolumeListSceneQuery CreatePlaneBoundedVolumeQuery(List<PlaneBoundedVolume> volumes, ulong mask) {
			DefaultPlaneBoundedVolumeListSceneQuery query = new DefaultPlaneBoundedVolumeListSceneQuery(this);
			query.Volumes = volumes;
			query.QueryMask = mask;

			return query;
		}

		/// <summary>
		///    Creates an IntersectionSceneQuery for this scene manager.
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for locating
		///		intersecting objects. See SceneQuery and IntersectionSceneQuery
		///		for full details.
		/// </remarks>
		/// <returns>A specialized implementation of IntersectionSceneQuery for this scene manager.</returns>
		public IntersectionSceneQuery CreateIntersectionQuery() {
			return CreateIntersectionQuery(0xffffffff);
		}

		/// <summary>
		///    Creates an IntersectionSceneQuery for this scene manager.
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for locating
		///		intersecting objects. See SceneQuery and IntersectionSceneQuery
		///		for full details.
		/// </remarks>
		/// <param name="mask">The query mask to apply to this query; can be used to filter out certain objects.</param>
		/// <returns>A specialized implementation of IntersectionSceneQuery for this scene manager.</returns>
		public virtual IntersectionSceneQuery CreateIntersectionQuery(ulong mask) {
			DefaultIntersectionSceneQuery query = new DefaultIntersectionSceneQuery(this);
			query.QueryMask = mask;
			return query;
		}

		/// <summary>
		///		Removes all cameras from the scene.
		/// </summary>
		public virtual void RemoveAllCameras() {
			// notify the render system of each camera being removed
			foreach (Camera camera in cameraList.Values) {
                targetRenderSystem.NotifyCameraRemoved(camera);
			}

			// clear the list
			cameraList.Clear();
		}

		/// <summary>
		///		Removes all lights from the scene.
		/// </summary>
		public virtual void RemoveAllLights() {
			// clear the list
            GetMovableObjectMap("Light").Clear();
		}

		/// <summary>
		///		Removes all entities from the scene.
		/// </summary>
		public virtual void RemoveAllEntities() {
			// clear the list
            GetMovableObjectMap("Entity").Clear();
		}

		/// <summary>
		///		Removes all billboardsets from the scene.
		/// </summary>
		public virtual void RemoveAllBillboardSets() {
			// clear the list
			GetMovableObjectMap("BillboardSet").Clear();
		}

		/// <summary>
		///		Removes all ribbon trails from the scene.
		/// </summary>
		public virtual void RemoveAllRibbonTrails() {
			// clear the list
			GetMovableObjectMap("RibbonTrail").Clear();
		}

		/// <summary>
		///		Removes the specified camera from the scene.
		/// </summary>
		/// <remarks>
		///		This method removes a previously added camera from the scene.
		/// </remarks>
		/// <param name="camera">Reference to the camera to remove.</param>
		public virtual void RemoveCamera(Camera camera) {
			cameraList.Remove(camera.Name);

			// notify all render targets
			targetRenderSystem.NotifyCameraRemoved(camera);
		}

		/// <summary>
		///		Removes a camera from the scene with the specified name.
		/// </summary>
		/// <remarks>
		///		This method removes a previously added camera from the scene.
		/// </remarks>
		/// <param name="name">Name of the camera to remove.</param>
		public virtual void RemoveCamera(string name) {
			Debug.Assert(cameraList.ContainsKey(name), string.Format("Camera '{0}' does not exist in the scene.", name));

			RemoveCamera(cameraList[name]);
		}

        protected void OnLightRemoved(string lightName)
        {
            NamedLightEventHandler handler = LightRemoved;
            if (handler != null)
            {
                handler(lightName);
            }
        }

		/// <summary>
		///		Removes the specified light from the scene.
		/// </summary>
		/// <remarks>
		///		This method removes a previously added light from the scene.
		/// </remarks>
		/// <param name="camera">Reference to the light to remove.</param>
        public virtual void RemoveLight(Light light) {
            ExtractMovableObject(light);
            OnLightRemoved(light.Name);
        }

		/// <summary>
		///		Removes a light from the scene with the specified name.
		/// </summary>
		/// <remarks>
		///		This method removes a previously added light from the scene.
		/// </remarks>
		/// <param name="name">Name of the light to remove.</param>
        public virtual void RemoveLight(string name) {
            ExtractMovableObject(name, "Light");
            OnLightRemoved(name);
        }
		/// <summary>
		///		Removes the specified BillboardSet from the scene.
		/// </summary>
		/// <remarks>
		///		This method removes a previously added BillboardSet from the scene.
		/// </remarks>
        /// <param name="billboardSet">Reference to the BillboardSet to remove.</param>
        public virtual void RemoveBillboardSet(BillboardSet billboardSet) {
            ExtractMovableObject(billboardSet);
        }
		/// <summary>
		///		Removes a BillboardSet from the scene with the specified name.
		/// </summary>
		/// <remarks>
		///		This method removes a previously added BillboardSet from the scene.
		/// </remarks>
		/// <param name="name">Name of the BillboardSet to remove.</param>
        public virtual void RemoveBillboardSet(string name) {
            ExtractMovableObject(name, "BillboardSet");
        }
        /// <summary>
        ///		Removes the specified ParticleSystem from the scene.
        /// </summary>
        /// <remarks>
        ///		This method removes a previously added ParticleSystem from the scene.
        /// </remarks>
        /// <param name="particleSystem">Reference to the ParticleSystem to remove.</param>
        public virtual void RemoveParticleSystem(ParticleSystem particleSystem) {
            ExtractMovableObject(particleSystem);
        }
        /// <summary>
        ///		Removes a ParticleSystem from the scene with the specified name.
        /// </summary>
        /// <remarks>
        ///		This method removes a previously added ParticleSystem from the scene.
        /// </remarks>
        /// <param name="name">Name of the ParticleSystem to remove.</param>
        public virtual void RemoveParticleSystem(string name) {
            ExtractMovableObject(name, "ParticleSystem");
        }
		/// <summary>
		///		Removes the specified RibbonTrail from the scene.
		/// </summary>
		/// <remarks>
		///		This method removes a previously added RibbonTrail from the scene.
		/// </remarks>
        /// <param name="ribbonTrail">Reference to the RibbonTrail to remove.</param>
        public virtual void RemoveRibbonTrail(RibbonTrail ribbonTrail) {
            ExtractMovableObject(ribbonTrail);
        }
		/// <summary>
		///		Removes a RibbonTrail from the scene with the specified name.
		/// </summary>
		/// <remarks>
		///		This method removes a previously added RibbonTrail from the scene.
		/// </remarks>
		/// <param name="name">Name of the RibbonTrail to remove.</param>
        public virtual void RemoveRibbonTrail(string name) {
            ExtractMovableObject(name, "RibbonTrail");
        }
		/// <summary>
		///    Removes the specified entity from the scene.
		/// </summary>
		/// <param name="entity">Entity to remove from the scene.</param>
		public virtual void RemoveEntity(Entity entity) {
            ExtractMovableObject(entity);
		}
        /// <summary>
        ///    Removes the entity with the specified name from the scene.
        /// </summary>
        /// <param name="name">name of the entity to remove from the scene.</param>
        public virtual void RemoveEntity(string name) {
            ExtractMovableObject(name, "Entity");
        }

        /// <summary>
        ///    Removes the specified movable object from the scene.
        /// </summary>
        /// <param name="obj">MovableObject to remove from the scene.</param>
        protected void ExtractMovableObject(MovableObject obj) {
            GetMovableObjectMap(obj).Remove(obj.Name);
        }

        /// <summary>
        ///    Removes the movable object with the specified name and type from the scene.
        /// </summary>
        /// <param name="name">name of the movable object to remove from the scene.</param>
        protected void ExtractMovableObject(string name, string type) {
            GetMovableObjectMap(type).Remove(name);
        }

        /// <summary>
        ///    Removes the specified movable object from the scene.
        /// </summary>
        /// <param name="obj">MovableObject to remove from the scene.</param>
        public virtual void RemoveMovableObject(MovableObject obj) {
            ExtractMovableObject(obj);
        }
        /// <summary>
        ///   Removes the movable object with the specified name and type from the scene.
        /// </summary>
        /// <param name="name">name of the movable object to remove from the scene.</param>
        /// <param name="type">type of the movable object to remove from the scene.</param>
        public virtual void RemoveMovableObject(string name, string type) {
            ExtractMovableObject(name, type);
        }

        private Dictionary<string, MovableObject> GetMovableObjectMap(MovableObject obj) {
            if (obj is Entity)
                return GetMovableObjectMap("Entity");
            else if (obj is Light)
                return GetMovableObjectMap("Light");
            else if (obj is BillboardSet)
                return GetMovableObjectMap("BillboardSet");
            else if (obj is ParticleSystem)
                return GetMovableObjectMap("ParticleSystem");
            else if (obj is RibbonTrail)
                return GetMovableObjectMap("RibbonTrail");
            else if (obj is SimpleRenderable)
                return GetMovableObjectMap("SimpleRenderable");
            else if (obj is Region)
                return GetMovableObjectMap("StaticGeomRegion");
            throw new AxiomException("Unknown object type: " + obj);
        }

        /// <summary>
        ///    Adds the specified movable object to the scene.
        /// </summary>
        /// <param name="obj">the movable object to add to the scene.</param>
        public virtual void AddMovableObject(MovableObject obj) {
            InjectMovableObject(obj);
        }

        /// <summary>
        ///    Adds the specified movable object to the scene.
        /// </summary>
        /// <param name="obj">the movable object to add to the scene.</param>
        protected void InjectMovableObject(MovableObject obj) {
            GetMovableObjectMap(obj)[obj.Name] = obj;
        }

		/// <summary>
		///		Sets the fogging mode applied to the scene.
		/// </summary>
		/// <remarks>
		///		This method sets up the scene-wide fogging effect. These settings
		///		apply to all geometry rendered, UNLESS the material with which it
		///		is rendered has it's own fog settings (see Material.SetFog).
		/// </remarks>
		/// <param name="mode">Set up the mode of fog as described in the FogMode
		///		enum, or set to FogMode.None to turn off.</param>
		/// <param name="color">The color of the fog. Either set this to the same
		///		as your viewport background color, or to blend in with a skydome or skybox.</param>
		/// <param name="density">The density of the fog in Exp or Exp2.
		///		mode, as a value between 0 and 1. The default is 0.001. </param>
		/// <param name="linearStart">Distance in world units at which linear fog starts to
		///		encroach. Only applicable if mode is</param>
		/// <param name="linearEnd">Distance in world units at which linear fog becomes completely
		///		opaque. Only applicable if mode is</param>
		public virtual void SetFog(FogMode mode, ColorEx color, float density, float linearStart, float linearEnd) {
			// set all the fog information
			fogMode = mode;
			fogColor = color;
			fogDensity = density;
			fogStart = linearStart;
			fogEnd = linearEnd;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="color"></param>
		/// <param name="density"></param>
		public virtual void SetFog(FogMode mode, ColorEx color, float density) {
			// set all the fog information
			fogMode = mode;
			fogColor = color;
			fogDensity = density;
			fogStart = 0.0f;
			fogEnd = 1.0f;
		}

		/// <summary>
		///		Overloaded method.
		/// </summary>
		/// <param name="enable"></param>
		/// <param name="materialName"></param>
		/// <param name="distance"></param>
		public void SetSkyBox(bool enable, string materialName, float distance) {
			SetSkyBox(enable, materialName, distance, true, Quaternion.Identity);
		}

		/// <summary>
		///		Enables / disables a 'sky box' i.e. a 6-sided box at constant
		///		distance from the camera representing the sky.
		/// </summary>
		/// <remarks>
		///		You could create a sky box yourself using the standard mesh and
		///		entity methods, but this creates a plane which the camera can
		///		never get closer or further away from - it moves with the camera.
		///		(you could create this effect by creating a world box which
		///		was attached to the same SceneNode as the Camera too, but this
		///		would only apply to a single camera whereas this skybox applies
		///		to any camera using this scene manager).
		///		<p/>
		///		The material you use for the skybox can either contain layers
		///		which are single textures, or they can be cubic textures, i.e.
		///		made up of 6 images, one for each plane of the cube. See the
		///		TextureLayer class for more information.
		/// </remarks>
		/// <param name="enable">True to enable the skybox, false to disable it</param>
		/// <param name="materialName">The name of the material the box will use.</param>
		/// <param name="distance">Distance in world coordinates from the camera to each plane of the box. </param>
		/// <param name="drawFirst">
		///		If true, the box is drawn before all other
		///		geometry in the scene, without updating the depth buffer.
		///		This is the safest rendering method since all other objects
		///		will always appear in front of the sky. However this is not
		///		the most efficient way if most of the sky is often occluded
		///		by other objects. If this is the case, you can set this
		///		parameter to false meaning it draws <em>after</em> all other
		///		geometry which can be an optimisation - however you must
		///		ensure that the distance value is large enough that no
		///		objects will 'poke through' the sky box when it is rendered.
		/// </param>
		/// <param name="orientation">
		///		Specifies the orientation of the box. By default the 'top' of the box is deemed to be
		///		in the +y direction, and the 'front' at the -z direction.
		///		You can use this parameter to rotate the sky if you want.
		/// </param>
		public void SetSkyBox(bool enable, string materialName, float distance, bool drawFirst, Quaternion orientation) {
			if(enable) {
				Material m = MaterialManager.Instance.GetByName(materialName);

                if (m == null) {
                    isSkyBoxEnabled = false;
                    throw new AxiomException(string.Format("Could not find skybox material '{0}'", materialName));
                }
				// Make sure the material doesn't update the depth buffer
				m.DepthWrite = false;
				// Ensure loaded
				m.Load();

				// ensure texture clamping to reduce fuzzy edges when using filtering
                if (m.GetTechnique(0).GetPass(0).NumTextureUnitStages > 0)
				    m.GetTechnique(0).GetPass(0).GetTextureUnitState(0).TextureAddressing = TextureAddressing.Clamp;
	
				isSkyBoxDrawnFirst = drawFirst;

				if(skyBoxNode == null) {
					skyBoxNode = CreateSceneNode("SkyBoxNode");
				}
				else {
					skyBoxNode.DetachAllObjects();
				}

				// need to create 6 plane entities for each side of the skybox
				for(int i = 0; i < 6; i++) {
					Mesh planeModel = CreateSkyboxPlane((BoxPlane)i, distance, orientation);
					string entityName = "SkyBoxPlane" + i;

					if(skyBoxEntities[i] != null) {
						RemoveEntity(skyBoxEntities[i]);
					}

					// create an entity for this plane
					skyBoxEntities[i] = CreateEntity(entityName, planeModel.Name);

					// skyboxes need not cast shadows
					skyBoxEntities[i].CastShadows = false;

					// Have to create 6 materials, one for each frame
					// Used to use combined material but now we're using queue we can't split to change frame
					// This doesn't use much memory because textures aren't duplicated
					Material boxMaterial = MaterialManager.Instance.GetByName(entityName);

					if(boxMaterial == null) {
						// Create new by clone
						boxMaterial = m.Clone(entityName);
						boxMaterial.Load();
					}
					else {
						// Copy over existing
						//must not copy over the name, otherwise the sky box entity will be set to use the origional m material instead of the copy for each entity which each uses a different frame
						m.CopyTo(boxMaterial, false);
						boxMaterial.Load();
					}

					// set the current frame
                    TechniqueList techniques = boxMaterial.SupportedTechniques;
                    for (int j=0; j<techniques.Count; j++) {
                        Technique technique = (Technique)techniques[j];
                        technique.GetPass(0).GetTextureUnitState(0).CurrentFrame = i;
                    }
                    
					skyBoxEntities[i].MaterialName = boxMaterial.Name;

					// Attach to node
					skyBoxNode.AttachObject(skyBoxEntities[i]);
				} // for each plane
			}
            isSkyBoxEnabled = enable;
        }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="isEnabled"></param>
		/// <param name="materialName"></param>
		/// <param name="curvature"></param>
		/// <param name="tiling"></param>
		public void SetSkyDome(bool isEnabled, string materialName, float curvature, float tiling) {
			SetSkyDome(isEnabled, materialName, curvature, tiling, 4000, true, Quaternion.Identity);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="isEnabled"></param>
		/// <param name="materialName"></param>
		/// <param name="curvature"></param>
		/// <param name="tiling"></param>
		/// <param name="distance"></param>
		/// <param name="drawFirst"></param>
		/// <param name="orientation"></param>
		public void SetSkyDome(bool isEnabled, string materialName, float curvature, float tiling, float distance, bool drawFirst, Quaternion orientation) {
			if(isEnabled) {
				Material material = MaterialManager.Instance.GetByName(materialName);

				if(material == null) {
					throw new AxiomException(string.Format("Could not find skydome material '{0}'", materialName));
				}

				// make sure the material doesn't update the depth buffer
				material.DepthWrite = false;
				// ensure loading
				material.Load();

				isSkyDomeDrawnFirst = drawFirst;

				// create node
				if(skyDomeNode == null) {
					skyDomeNode = CreateSceneNode("SkyDomeNode");
				}
				else {
					skyDomeNode.DetachAllObjects();
				}

				// set up the dome (5 planes)
				for(int i = 0; i < 5; ++i) {
					Mesh planeMesh = CreateSkyDomePlane((BoxPlane) i, curvature, tiling, distance, orientation);
					string entityName = String.Format("SkyDomePlane{0}", i);

					// create entity
					if(skyDomeEntities[i] != null) {
						RemoveEntity(skyDomeEntities[i]);
					}

					skyDomeEntities[i] = CreateEntity(entityName, planeMesh.Name);
					skyDomeEntities[i].MaterialName = material.Name;
					// Sky entities need not cast shadows
					skyDomeEntities[i].CastShadows = false;

					// attach to node
					skyDomeNode.AttachObject(skyDomeEntities[i]);
				} // for each plane
			}
			isSkyDomeEnabled = isEnabled;
		}

		/// <summary>
		///		Sets the size and count of textures used in texture-based shadows. 
		/// </summary>
		/// <remarks>
		///		See ShadowTextureSize and ShadowTextureCount for details, this
		///		method just allows you to change both at once, which can save on 
		///		reallocation if the textures have already been created.
		/// </remarks>
		/// <param name="size"></param>
		/// <param name="count"></param>
		public virtual void SetShadowTextureSettings(ushort size, ushort count, PixelFormat fmt) {
            ShadowTextureCount = count;
            foreach (ShadowTextureConfig config in shadowTextureConfigList) {
                if (config.width != size || config.height != size || config.format != fmt) {
                    config.width = config.height = size;
                    config.format = fmt;
                    shadowTextureConfigDirty = true;
                }
            }
		}
		
		#endregion

		#region Properties

		/// <summary>
        ///      Gets the name of the scene manager
        /// </summary>
		public string SMName {
			get { 
				return smName; 
			}
        }
        
        /// <summary>
		/// Gets/Sets the target render system that this scene manager should be using.
		/// </summary>
		public RenderSystem TargetRenderSystem {
			get { 
				return targetRenderSystem; 
			}
			set { 
				targetRenderSystem = value; 
			}
		}

		/// <summary>
		///		Gets the SceneNode at the root of the scene hierarchy.
		/// </summary>
		/// <remarks>
		///		The entire scene is held as a hierarchy of nodes, which
		///		allows things like relative transforms, general changes in
		///		rendering state etc (See the SceneNode class for more info).
		///		In this basic SceneManager class, you are free to
		///		structure this hierarchy however you like, since 
		///		it has no real significance apart from making transforms
		///		relative to each node (more specialised subclasses will
		///		provide utility methods for building specific node structures
		///		e.g. loading a BSP tree).
		/// </remarks>
		public SceneNode RootSceneNode {
			get { 
				return rootSceneNode; 
			}
		}

        public void OnAmbientLightChanged()
        {
            AmbientLightChangeHandler handler = AmbientLightChanged;
            if (handler != null)
            {
                handler(ambientColor);
            }
        }

		/// <summary>
		///		Gets/Sets the ambient light level to be used for the scene.
		/// </summary>
		/// <remarks>
		///		This sets the color and intensity of the ambient light in the scene, i.e. the
		///		light which is 'sourceless' and illuminates all objects equally.
		///		The color of an object is affected by a combination of the light in the scene,
		///		and the amount of light that object reflects (in this case based on the Material.Ambient
		///		property).
		///		<p/>
		///		By default the ambient light in the scene is Black, i.e. no ambient light. This
		///		means that any objects rendered with a Material which has lighting enabled 
		///		(see Material.LightingEnabled) will not be visible unless you have some dynamic lights in your scene.
		/// </remarks>
		public ColorEx AmbientLight {
			get { 
				return ambientColor; 
			}
			set { 
				if(value == null)
					throw new ArgumentException("Cannot set the scene ambient light color to null");
				//it will cause the GpuProgramParameters.SetConstant() color overload to throw a null reference exception otherwise
				ambientColor = value; 
				// change ambient color of current render system
				targetRenderSystem.AmbientLight = ambientColor;

                OnAmbientLightChanged();
			}
		}

		/// <summary>
		///		Method for setting a specific option of the Scene Manager. These options are usually
		///		specific for a certain implementation of the Scene Manager class, and may (and probably
		///		will) not exist across different implementations.
		/// </summary>
		public Hashtable Options {
			get { 
				return optionList; 
			}
		}

		/// <summary>
		///		Gets/Sets the color used to modulate areas in shadow.
		/// </summary>
		/// <remarks>
		///		This is only applicable for shadow techniques which involve 
		///		darkening the area in shadow, as opposed to masking out the light. 
		///		This color provided is used as a modulative value to darken the
		///		areas.
		/// </remarks>
		public ColorEx ShadowColor {
			get {
				return shadowColor;
			}
			set {
				shadowColor = value;

				if(shadowModulativePass == null && shadowCasterPlainBlackPass == null) {
					InitShadowVolumeMaterials();
				}

				shadowModulativePass.GetTextureUnitState(0).SetColorOperationEx(
					LayerBlendOperationEx.Modulate, 
					LayerBlendSource.Manual, 
					LayerBlendSource.Current, 
					value);
			}
		}

		/// <summary>
		///		Sets the distance a shadow volume is extruded for a directional light.
		/// </summary>
		/// <remarks>
		///		Although directional lights are essentially infinite, there are many
		///		reasons to limit the shadow extrusion distance to a finite number, 
		///		not least of which is compatibility with older cards (which do not
		///		support infinite positions), and shadow caster elimination.
		///		<p/>
		///		The default value is 10,000 world units. This does not apply to
		///		point lights or spotlights, since they extrude up to their 
		///		attenuation range.
		/// </remarks>
		public float ShadowDirectionalLightExtrusionDistance {
			get {
				return shadowDirLightExtrudeDist;
			}
			set {
				shadowDirLightExtrudeDist = value;
			}
		}

		/// <summary>
		///		Gets the proportional distance which a texture shadow which is generated from a
		///		directional light will be offset into the camera view to make best use of texture space.
		/// </summary>
        public float ShadowDirLightTextureOffset {
            get {
                return shadowTextureOffset;
            }
			set {
				shadowTextureOffset = value;
			}
        }

		/// <summary>
		///		Sets the maximum distance away from the camera that shadows will be visible.
		/// </summary>
		/// <remarks>
		///		Shadow techniques can be expensive, therefore it is a good idea
		///		to limit them to being rendered close to the camera if possible,
		///		and to skip the expense of rendering shadows for distance objects.
		///		This method allows you to set the distance at which shadows will no
		///		longer be rendered.
		///		Note:
		///		Each shadow technique can interpret this subtely differently.
		///		For example, one technique may use this to eliminate casters,
		///		another might use it to attenuate the shadows themselves.
		///		You should tweak this value to suit your chosen shadow technique
		///		and scene setup.
		/// </remarks>
		public float ShadowFarDistance {
			get {
				return shadowFarDistance;
			}
			set {
				shadowFarDistance = value;
				shadowFarDistanceSquared = shadowFarDistance * shadowFarDistance;
			}
		}

		/// <summary>
		///		Sets the maximum size of the index buffer used to render shadow primitives.
		/// </summary>
		/// <remarks>
		///		This property allows you to tweak the size of the index buffer used
		///		to render shadow primitives (including stencil shadow volumes). The
		///		default size is 51,200 entries, which is 100k of GPU memory, or
		///		enough to render approximately 17,000 triangles. You can reduce this
		///		as long as you do not have any models / world geometry chunks which 
		///		could require more than the amount you set.
		///		<p/>
		///		The maximum number of triangles required to render a single shadow 
		///		volume (including light and dark caps when needed) will be 3x the 
		///		number of edges on the light silhouette, plus the number of 
		///		light-facing triangles.	On average, half the 
		///		triangles will be facing toward the light, but the number of 
		///		triangles in the silhouette entirely depends on the mesh - 
		///		angular meshes will have a higher silhouette tris/mesh tris
		///		ratio than a smooth mesh. You can estimate the requirements for
		///		your particular mesh by rendering it alone in a scene with shadows
		///		enabled and a single light - rotate it or the light and make a note
		///		of how high the triangle count goes (remembering to subtract the 
		///		mesh triangle count)
		/// </remarks>
		public int ShadowIndexBufferSize {
			get {
				return shadowIndexBufferSize;
			}
			set {
				if(shadowIndexBuffer != null || value != shadowIndexBufferSize) {

					// create an shadow index buffer
					shadowIndexBuffer = 
						HardwareBufferManager.Instance.CreateIndexBuffer(
						IndexType.Size16, shadowIndexBufferSize,
						BufferUsage.DynamicWriteOnly, false);
				}

				shadowIndexBufferSize = value;
			}
		}

		/// <summary>
		///		Set the size of the texture used for texture-based shadows.
		/// </summary>
		/// <remarks>
		///		The larger the shadow texture, the better the detail on 
		///		texture based shadows, but obviously this takes more memory.
		///		The default size is 512. Sizes must be a power of 2.
		///	</remarks>
		public int ShadowTextureSize {
            get {
                return shadowTextureSize;
            }
			set {
                // default all current
                shadowTextureSize = value;
                foreach (ShadowTextureConfig config in  shadowTextureConfigList) {
                    if (config.width != value || config.height != value) {
                        config.width = config.height = value;
                        shadowTextureConfigDirty = true;
                    }
                }
			}
		}

		/// <summary>
		///		Get/Set the number of textures allocated for texture-based shadows.
		/// </summary>
        /// <remarks>
        ///     The default number of textures assigned to deal with texture based
        ///     shadows is 1; however this means you can only have one light casting
        ///     shadows at the same time. You can increase this number in order to 
        ///     make this more flexible, but be aware of the texture memory it will use.
        /// </remarks>
		public int ShadowTextureCount {
			get {
				return shadowTextureConfigList.Count;
			}
			set {
				if (shadowTextureConfigList.Count != value) {
                    int count = shadowTextureConfigList.Count - value;
                    if (count > 0)
                        shadowTextureConfigList.RemoveRange(value, count);
                    else {
                        for (int i=0; i<-count; i++)
                            shadowTextureConfigList.Add(new ShadowTextureConfig());
                    }
                }
            }
		}

		/// <summary>
		///		Set the pixel format of the textures used for texture-based shadows.
		/// </summary>
		/// <remarks>
        ///     By default, a colour texture is used (PF_X8R8G8B8) for texture shadows,
        ///     but if you want to use more advanced texture shadow types you can 
        ///     alter this. If you do, you will have to also call
        ///     setShadowTextureCasterMaterial and setShadowTextureReceiverMaterial
        ///     to provide shader-based materials to use these customised shadow
        ///     texture formats.
		/// </remarks>
        /// <note>
        ///     This is the simple form, see SetShadowTextureConfig for the more 
        ///     complex form.
        /// </note>
        public PixelFormat ShadowTextureFormat {
            set {
                foreach(ShadowTextureConfig config in shadowTextureConfigList) {
                    if (config.format != value) {
                        config.format = value;
                        shadowTextureConfigDirty = true;
                    }
                }
            }
        }

		/// <summary>
		///		Determines whether we're supporting self-shadowing
		/// </summary>
		/// <remarks>
		///		The default number of textures assigned to deal with texture based
		///		shadows is 1; however this means you can only have one light casting
		///		shadows at the same time. You can increase this number in order to 
		///		make this more flexible, but be aware of the texture memory it will use.
		///	</remarks>
		public bool ShadowTextureSelfShadow {
			get{ return shadowTextureSelfShadow; }
			set {
                shadowTextureSelfShadow = value;
				if (IsShadowTechniqueTextureBased)
                    if(renderQueue != null) 
                        renderQueue.ShadowCastersCannotBeReceivers = !value;
			}
		}

		/// <summary>
		///		Sets the general shadow technique to be used in this scene.
		/// </summary>
		/// <remarks>
		///		There are multiple ways to generate shadows in a scene, and each has 
		///		strengths and weaknesses. 
		///		<ul><li>Stencil-based approaches can be used to 
		///		draw very long, extreme shadows without loss of precision and the 'additive'
		///		version can correctly show the shadowing of complex effects like bump mapping
		///		because they physically exclude the light from those areas. However, the edges
		///		are very sharp and stencils cannot handle transparency, and they involve a 
		///		fair amount of CPU work in order to calculate the shadow volumes, especially
		///		when animated objects are involved.</li>
		///		<li>Texture-based approaches are good for handling transparency (they can, for
		///		example, correctly shadow a mesh which uses alpha to represent holes), and they
		///		require little CPU overhead, and can happily shadow geometry which is deformed
		///		by a vertex program, unlike stencil shadows. However, they have a fixed precision 
		///		which can introduce 'jaggies' at long range and have fillrate issues of their own.</li>
		///		</ul>
		///		<p/>
		///		We support 2 kinds of stencil shadows, and 2 kinds of texture-based shadows, and one
		///		simple decal approach. The 2 stencil approaches differ in the amount of multipass work 
		///		that is required - the modulative approach simply 'darkens' areas in shadow after the 
		///		main render, which is the least expensive, whilst the additive approach has to perform 
		///		a render per light and adds the cumulative effect, whcih is more expensive but more 
		///		accurate. The texture based shadows both work in roughly the same way, the only difference is
		///		that the shadowmap approach is slightly more accurate, but requires a more recent
		///		graphics card.
		///		<p/>
		///		Note that because mixing many shadow techniques can cause problems, only one technique
		///		is supported at once. Also, you should call this method at the start of the 
		///		scene setup. 
		/// </remarks>
		public ShadowTechnique ShadowTechnique {
			get {
				return shadowTechnique;
			}
			set {
				shadowTechnique = value;

				// do initial setup for stencil shadows if needed
				if(IsShadowTechniqueStencilBased) {

					// Firstly check that we have a stencil. Otherwise, forget it!
					if(!targetRenderSystem.Caps.CheckCap(Capabilities.StencilBuffer)) {
                        log.Warn("Stencil shadows were requested, but the current hardware does not support them.  Disabling.");

                        shadowTechnique = ShadowTechnique.None;
					}
					else if(shadowIndexBuffer == null) {
						// create an shadow index buffer
						shadowIndexBuffer = 
							HardwareBufferManager.Instance.CreateIndexBuffer(
							IndexType.Size16, shadowIndexBufferSize,
							BufferUsage.DynamicWriteOnly, false);

						// tell all the meshes to prepare shadow volumes
						MeshManager.Instance.PrepareAllMeshesForShadowVolumes = true;
					}
				}

                if (!IsShadowTechniqueTextureBased)
                    // Destroy shadow textures to optimise resource usage
                    DestroyShadowTextures();
                else {
                    // assure no custom shadow matrix is used accidentally in case we switch
                    // from a custom shadow mapping type to a non-custom (uniform shadow mapping)
                    foreach (Camera texCam in shadowTextureCameras) {
                        texCam.CustomViewMatrix = false;
                        texCam.CustomProjectionMatrix = false;
                    }
                }
			}
		}

		public string ShadowTextureCasterMaterial {
			get {
				return shadowTextureCasterMaterial;
			}
			set {
                // When rendering with a material that includes its own custom shadow caster
                // vertex program, the code that sets up the pass will replace the vertex program
                // in this material with the one from the object's own material.
                // We need to set it back before we switch materials, in case we need to use this
                // material again.
                if (shadowTextureCustomCasterPass != null)
                {
                    if (shadowTextureCustomCasterPass.VertexProgramName !=
                                shadowTextureCustomCasterVertexProgram)
                    {
                        shadowTextureCustomCasterPass.SetVertexProgram(shadowTextureCustomCasterVertexProgram);
                        if (shadowTextureCustomCasterPass.HasVertexProgram)
                            shadowTextureCustomCasterPass.VertexProgramParameters = shadowTextureCustomCasterVPParams;
                    }
                }

				shadowTextureCasterMaterial = value;
                if (value == "")
                {
                    shadowTextureCustomCasterPass = null;
                    shadowTextureCustomCasterFragmentProgram = "";
                    shadowTextureCustomCasterVertexProgram = "";
                }
                else
                {
                    Material material = MaterialManager.Instance.GetByName(value);
                    if (material == null)
                        log.WarnFormat("Cannot use material '{0}' as the ShadowTextureCasterMaterial because the material doesn't exist.",
                                       value);
                    else
                    {
                        material.Load();
                        Technique technique = material.GetBestTechnique();
                        if (technique == null)
                        {
                            log.WarnFormat("Cannot use material '{0}' as the ShadowTextureCasterMaterial because there is no supported technique",
                                           value);
                        }
                        else
                        {
                            shadowTextureCustomCasterPass = material.GetBestTechnique().GetPass(0);
                            if (shadowTextureCustomCasterPass.HasVertexProgram)
                            {
                                // Save vertex program and params in case we have to swap them out
                                shadowTextureCustomCasterVertexProgram = shadowTextureCustomCasterPass.VertexProgramName;
                                shadowTextureCustomCasterVPParams = shadowTextureCustomCasterPass.VertexProgramParameters;

                            }
                            else
                            {
                                shadowTextureCustomCasterVertexProgram = "";
                            }
                            if (shadowTextureCustomCasterPass.HasFragmentProgram)
                            {
                                // Save fragment program and params in case we have to swap them out
                                shadowTextureCustomCasterFragmentProgram = shadowTextureCustomCasterPass.FragmentProgramName;
                                shadowTextureCustomCasterFPParams = shadowTextureCustomCasterPass.FragmentProgramParameters;
                            }
                            else
                            {
                                shadowTextureCustomCasterFragmentProgram = "";
                            }
                        }
                    }
                }
			}
		}

		public string ShadowTextureReceiverMaterial {
			get {
				return shadowTextureReceiverMaterial;
			}
			set {
                // When rendering with a material that includes its own custom shadow receiver
                // vertex program, the code that sets up the pass will replace the vertex program
                // in this material with the one from the object's own material.
                // We need to set it back before we switch materials, in case we need to use this
                // material again.
                if (shadowTextureCustomReceiverPass != null)
                {
                    if (shadowTextureCustomReceiverPass.VertexProgramName !=
                                shadowTextureCustomReceiverVertexProgram)
                    {
                        shadowTextureCustomReceiverPass.SetVertexProgram(shadowTextureCustomReceiverVertexProgram);
                        if (shadowTextureCustomReceiverPass.HasVertexProgram)
                            shadowTextureCustomReceiverPass.VertexProgramParameters = shadowTextureCustomReceiverVPParams;
                    }
                }

				shadowTextureReceiverMaterial = value;
                if (value == "")
                {
                    shadowTextureCustomReceiverPass = null;
                    shadowTextureCustomReceiverVertexProgram = "";
                    shadowTextureCustomReceiverFragmentProgram = "";
                }
                else
                {
                    Material material = MaterialManager.Instance.GetByName(value);
                    if (material == null)
                        log.WarnFormat("Cannot use material '{0}' as the ShadowTextureReceiverMaterial because the material doesn't exist.",
                                       value);
                    else
                    {
                        material.Load();
                        Technique technique = material.GetBestTechnique();
                        if (technique == null)
                        {
                            log.WarnFormat("Cannot use material '{0}' as the ShadowTextureReceiverMaterial because there is no supported technique",
                                           value);
                        }
                        else
                        {
                            shadowTextureCustomReceiverPass = technique.GetPass(0);
                            if (shadowTextureCustomReceiverPass.HasVertexProgram)
                            {
                                // Save vertex program and params in case we have to swap them out
                                shadowTextureCustomReceiverVertexProgram = shadowTextureCustomReceiverPass.VertexProgramName;
                                shadowTextureCustomReceiverVPParams = shadowTextureCustomReceiverPass.VertexProgramParameters;
                            }
                            else
                                shadowTextureCustomReceiverVertexProgram = "";
                            if (shadowTextureCustomReceiverPass.HasFragmentProgram)
                            {
                                // Save fragment program and params in case we have to swap them out
                                shadowTextureCustomReceiverFragmentProgram = shadowTextureCustomReceiverPass.FragmentProgramName;
                                shadowTextureCustomReceiverFPParams = shadowTextureCustomReceiverPass.FragmentProgramParameters;
                            }
                            else
                                shadowTextureCustomReceiverFragmentProgram = "";
                        }
                    }
                }
			}
		}

		/// <summary>
		///		Sets whether we should use an inifinite camera far plane
		///		when rendering stencil shadows.
		/// </summary>
		/// <remarks>
		///		Stencil shadow coherency is very reliant on the shadow volume
		///		not being clipped by the far plane. If this clipping happens, you
		///		get a kind of 'negative' shadow effect. The best way to achieve
		///		coherency is to move the far plane of the camera out to infinity,
		///		thus preventing the far plane from clipping the shadow volumes.
		///		When combined with vertex program extrusion of the volume to 
		///		infinity, which	Axiom does when available, this results in very
		///		robust shadow volumes. For this reason, when you enable stencil 
		///		shadows, Ogre automatically changes your camera settings to 
		///		project to infinity if the card supports it. You can disable this
		///		behavior if you like by setting this property; although you can 
		///		never enable infinite projection if the card does not support it.
		///		<p/>
		///		If you disable infinite projection, or it is not available, 
		///		you need to be far more careful with your light attenuation /
		///		directional light extrusion distances to avoid clipping artefacts
		///		at the far plane.
		///		<p/>
		///		Recent cards will generally support infinite far plane projection.
		///		However, we have found some cases where they do not, especially
		///		on Direct3D. There is no standard capability we can check to 
		///		validate this, so we use some heuristics based on experience:
		///		<UL>
		///		<LI>OpenGL always seems to support it no matter what the card</LI>
		///		<LI>Direct3D on non-vertex program capable systems (including 
		///		vertex program capable cards on Direct3D7) does not
		///		support it</LI>
		///		<LI>Direct3D on GeForce3 and GeForce4 Ti does not seem to support
		///		infinite projection<LI>
		///		</UL>
		///		Therefore in the RenderSystem implementation, we may veto the use
		///		of an infinite far plane based on these heuristics. 
		/// </remarks>
		public bool ShadowUseInfiniteFarPlane {
			get {
				return shadowUseInfiniteFarPlane;
			}
			set {
				shadowUseInfiniteFarPlane = value;
			}
		}

		/// <summary>
		///		Whether shadow casters should be rendered into shadow
        ///     textures using their back faces rather than their front faces.
		/// </summary>
		public bool ShadowCasterRenderBackFaces {
			get {
				return shadowCasterRenderBackFaces;
			}
			set {
				shadowCasterRenderBackFaces = value;
			}
		}

        public bool SuppressRenderStateChanges {
            get {
                return suppressRenderStateChanges;
            }
            set {
                suppressRenderStateChanges = value;
            }
        }

		/// <summary>
		///		Gets/Sets a value that forces all nodes to render their bounding boxes.
		/// </summary>
		public bool ShowBoundingBoxes {
			get { 
				return showBoundingBoxes; 
			}
			set { 
				showBoundingBoxes = value; 
			}
		}

		/// <summary>
		///		Gets/Sets a flag that indicates whether debug shadow info (i.e. visible volumes)
		///		will be displayed.
		/// </summary>
		public virtual bool ShowDebugShadows {
			get {
				return showDebugShadows;
			}
			set {
				showDebugShadows = value;
			}
		}

		/// <summary>
		///		Gets/Sets whether or not to display the nodes themselves in addition to their objects.
		/// </summary>
		/// <remarks>
		///		What will be displayed is the local axes of the node (for debugging mainly).
		/// </remarks>
		public bool DisplayNodes {
			get { 
				return displayNodes; 
			}
			set { 
				displayNodes = value; 
			}
		}

		/// <summary>
		///		Gets the fog mode that was set during the last call to SetFog.
		/// </summary>
        public FogMode FogMode
        {
            get
            {
                return fogMode;
            }
            set
            {
                fogMode = value;
            }
        }

		/// <summary>
		///		Gets the fog starting point that was set during the last call to SetFog.
		/// </summary>
        public float FogStart
        {
            get
            {
                return fogStart;
            }
            set
            {
                fogStart = value;
            }
        }

		/// <summary>
		///		Gets the fog ending point that was set during the last call to SetFog.
		/// </summary>
        public float FogEnd
        {
            get
            {
                return fogEnd;
            }
            set
            {
                fogEnd = value;
            }
        }

		/// <summary>
		///		Gets the fog density that was set during the last call to SetFog.
		/// </summary>
        public float FogDensity
        {
            get
            {
                return fogDensity;
            }
            set
            {
                fogDensity = value;
            }
        }

		/// <summary>
		///		Gets the fog color that was set during the last call to SetFog.
		/// </summary>
        public virtual ColorEx FogColor
        {
            get
            {
                return fogColor;
            }
            set
            {
                fogColor = value;
            }
        }

		/// <summary>
		///		Returns a pointer to the default Material settings.
		/// </summary>
		///	<remarks>
		///		<p>
		///		Axiom comes configured with a set of defaults for newly created
		///		materials. If you wish to have a different set of defaults,
		///		simply call this method and change the returned Material's
		///		settings. All materials created from then on will be configured
		///		with the new defaults you have specified.
		///		</p>
		///		<p>
		///		The default settings begin as a single Technique with a single, non-programmable Pass:
		///     <ul>
		///     <li>ambient = ColourEx.White</li>
		///     <li>diffuse = ColourEx.White</li>
		///     <li>specular = ColourEx.Black</li>
		///     <li>emmissive = ColourEx.Black</li>
		///     <li>shininess = 0</li>
		///     <li>No texture unit settings (& hence no textures)</li>
		///     <li>SourceBlendFactor = SBF_ONE</li>
		///     <li>DestBlendFactor = SBF_ZERO (no blend, replace with new colour)</li>
		///     <li>Depth buffer checking on</li>
		///     <li>Depth buffer writing on</li>
		///     <li>Depth buffer comparison function = CMPF_LESS_EQUAL</li>
		///     <li>Colour buffer writing on for all channels</li>
		///     <li>Culling mode = CULL_CLOCKWISE</li>
		///     <li>Ambient lighting = ColourValue(0.5, 0.5, 0.5) (mid-grey)</li>
		///     <li>Dynamic lighting enabled</li>
		///     <li>Gourad shading mode</li>
		///     <li>Bilinear texture filtering</li>
		///     </ul>
		///		</p>
		///	</remarks>
		public Material DefaultMaterialSettings {
			get {
				return Material.defaultSettings;
			}
		}

        /// <summary>
        ///		Gets the current viewport - - needed by compositor
        /// </summary>
        public Viewport CurrentViewport {
            get {
                return currentViewport;
            }
        }

        /// <summary>
        ///		Gets and sets the object visibility mask
        /// </summary>
        public uint VisibilityMask {
            get {
                return visibilityMask;
            }
            set {
                visibilityMask = value;
            }
        }

        /// <summary>
        ///		Gets and sets the object visibility mask
        /// </summary>
        public bool FindVisibleObjectsBool {
            get {
                return findVisibleObjects;
            }
            set {
                findVisibleObjects = value;
            }
        }

		/// <summary>
		///		Get the count of entities in this scene
		/// </summary>
		public int NumEntities {
            get { return GetMovableObjectMap("Entity").Values.Count; }
		}
		
		/// <summary>
		///		Get the regions in use
		///     TODO: Is there any point to having this region list?
        /// </summary>
		public List<Region> RegionList {
			get { return regionList; }
		}

        public IlluminationRenderStage IlluminationStage
        {
            get
            {
                return illuminationStage;
            }
        }

		public long TotalSetPassCalls {
			get { return totalSetPassCalls; }
		}
        
        /// <summary>
        ///		Gets the camera that the SceneManager is using for rendering
        /// </summary>
        public Camera CameraInProgress {
            get {
                return cameraInProgress;
            }
        }

        #endregion

		#region Internal methods

		private static TimingMeter renderSceneMeter = MeterManager.GetMeter("Render Scene", "Render Scene");
		private static TimingMeter animationsMeter = MeterManager.GetMeter("Apply Animations", "Render Scene");
		private static TimingMeter renderVisibleMeter = MeterManager.GetMeter("Render Objects", "Render Scene");
		private static TimingMeter findVisibleMeter = MeterManager.GetMeter("Find Visible", "Render Scene");
		
		/// <summary>
		///		Prompts the class to send its contents to the renderer.
		/// </summary>
		/// <remarks>
		///		This method prompts the scene manager to send the
		///		contents of the scene it manages to the rendering
		///		pipeline, possibly preceded by some sorting, culling
		///		or other scene management tasks. Note that this method is not normally called
		///		directly by the user application; it is called automatically
		///		by the engine's rendering loop.
		/// </remarks>
		/// <param name="camera">Pointer to a camera from whose viewpoint the scene is to be rendered.</param>
		/// <param name="viewport">The target viewport</param>
		/// <param name="showOverlays">Whether or not any overlay objects should be rendered</param>
		internal void RenderScene(Camera camera, Viewport viewport, bool showOverlays) {
			
			renderSceneMeter.Enter();
			
			// let the engine know this is the current scene manager
			Root.Instance.SceneManager = this;

			if (IsShadowTechniqueInUse) {
				// initialize shadow volume materials
				InitShadowVolumeMaterials();
			}

			// Perform a quick pre-check to see whether we should override far distance
			// When using stencil volumes we have to use infinite far distance
			// to prevent dark caps getting clipped
			if (IsShadowTechniqueStencilBased &&
				camera.Far != 0 && 
				targetRenderSystem.Caps.CheckCap(Capabilities.InfiniteFarPlane) &&
				shadowUseInfiniteFarPlane) {

				// infinite far distance
				camera.Far = 0.0f;
			}

			cameraInProgress = camera;
			hasCameraChanged = true;

			// Update the scene, only do this once per frame
			ulong thisFrameNumber = Root.Instance.CurrentFrameCount;
			if (thisFrameNumber != lastFrameNumber) {
				// Update animations
				animationsMeter.Enter();
				ApplySceneAnimations();
				animationsMeter.Exit();
				// Update controllers 
				ControllerManager.Instance.UpdateAll();
				lastFrameNumber = thisFrameNumber;
			}

			// Update scene graph for this camera (can happen multiple times per frame)
			UpdateSceneGraph(camera);

			// Auto-track nodes
			for(int i = 0; i < autoTrackingSceneNodes.Count; i++) {
				autoTrackingSceneNodes[i].AutoTrack();
			}

			// ask the camera to auto track if it has a target
			camera.AutoTrack();

			// Are we using any shadows at all?
			if (IsShadowTechniqueInUse && illuminationStage != IlluminationRenderStage.RenderToTexture &&
				viewport.ShadowsEnabled && findVisibleObjects) {
				// Locate any lights which could be affecting the frustum
				FindLightsAffectingFrustum(camera);

				if (IsShadowTechniqueTextureBased) {
					// *******
					// WARNING
					// *******
					// This call will result in re-entrant calls to this method
					// therefore anything which comes before this is NOT 
					// guaranteed persistent. Make sure that anything which 
					// MUST be specific to this camera / target is done 
					// AFTER THIS POINT
					PrepareShadowTextures(camera, viewport);
					// reset the cameras because of the re-entrant call
					cameraInProgress = camera;
					hasCameraChanged = true;
				}
			}

			// Invert vertex winding?
			targetRenderSystem.InvertVertexWinding = camera.IsReflected;

            // Tell params about viewport
            autoParamDataSource.Viewport = viewport;
			// Set the viewport
			SetViewport(viewport);

			// set the current camera for use in the auto GPU program params
			autoParamDataSource.Camera = camera;

			// Set autoparams for finite dir light extrusion
			autoParamDataSource.SetShadowDirLightExtrusionDistance(shadowDirLightExtrudeDist);

			// sets the current ambient light color for use in auto GPU program params
			autoParamDataSource.AmbientLight = ambientColor;
            // Tell rendersystem
            targetRenderSystem.AmbientLight = ambientColor;

			// Tell params about render target
			autoParamDataSource.RenderTarget = viewport.Target;

            // set fog params
            float fogScale = 1f;
            if (fogMode == FogMode.None)
            {
                fogScale = 0f;
            }
            autoParamDataSource.FogParams = new Vector4(fogStart, fogEnd, fogScale, 0);

            // set the time in the auto param data source
            // autoParamDataSource.Time = ((float)Root.Instance.Timer.Milliseconds) / 1000f;

			// Set camera window clipping planes (if any)
			if (targetRenderSystem.Caps.CheckCap(Capabilities.UserClipPlanes)) {
				// TODO: Add ClipPlanes to RenderSystem.cs
				/*
				if (camera.IsWindowSet)  
				{
					PlaneList planeList = camera.WindowPlanes;
					for (ushort i = 0; i < 4; ++i)
					{
						targetRenderSystem.EnableClipPlane(i, true);
						targetRenderSystem.SetClipPlane(i, planeList[i]);
					}
				}
				else
				{
					for (ushort i = 0; i < 4; ++i)
					{
						targetRenderSystem.EnableClipPlane(i, false);
					}
				}
				*/
			}

			// Prepare render queue for receiving new objects
			PrepareRenderQueue();

			// Parse the scene and tag visibles
			if (findVisibleObjects) {
                VisibleObjectsBoundsInfo bounds;
                if (!camVisibleObjectsMap.TryGetValue(camera, out bounds))
                    Debug.Assert(false, "Should never fail to find a visible object bound for a camera, " +
                        "did you override SceneManager.CreateCamera or something?");
                else {
                    bounds.Reset();
                    findVisibleMeter.Enter();
                    FindVisibleObjects(camera, bounds, illuminationStage == IlluminationRenderStage.RenderToTexture);
                    findVisibleMeter.Exit();
                    autoParamDataSource.MainCamBoundsInfo = bounds;
                }
            }

			// Add overlays, if viewport deems it
			if(viewport.OverlaysEnabled && illuminationStage != IlluminationRenderStage.RenderToTexture) {
				// Queue overlays for rendering
				OverlayManager.Instance.QueueOverlaysForRendering(camera, GetRenderQueue(), viewport);
			}

			// queue overlays and skyboxes for rendering
			if (viewport.SkiesEnabled && findVisibleObjects &&
				illuminationStage != IlluminationRenderStage.RenderToTexture)
			    QueueSkiesForRendering(camera);

			// begin frame geometry count
			targetRenderSystem.BeginGeometryCount();
			
            // Clear the viewport if required
            if (currentViewport.ClearEveryFrame)
                targetRenderSystem.ClearFrameBuffer(currentViewport.ClearBuffers, currentViewport.BackgroundColor);

            // being a frame of animation
			targetRenderSystem.BeginFrame();

			// use the camera's current scene detail level
			targetRenderSystem.RasterizationMode = camera.SceneDetail;

            // Set initial camera state
            targetRenderSystem.ProjectionMatrix = camera.ProjectionMatrixRS;
            targetRenderSystem.ViewMatrix = camera.ViewMatrix;

			// render all visible objects
			renderVisibleMeter.Enter();
			RenderVisibleObjects();
			renderVisibleMeter.Exit();

			// end the current frame
			targetRenderSystem.EndFrame();

			// Notify camera of the number of rendered faces
			camera.NotifyRenderedFaces(targetRenderSystem.FacesRendered);
		
			renderSceneMeter.Exit();
		}

		void PrepareRenderQueue() {
						
			// Clear the render queue
			GetRenderQueue().Clear();

			// Global split options
			UpdateRenderQueueSplitOptions();
		}

		void UpdateRenderQueueSplitOptions() {
			RenderQueue q = GetRenderQueue();
			if (IsShadowTechniqueStencilBased)
				// Casters can always be receivers
				q.ShadowCastersCannotBeReceivers = false;
			else // texture based
				q.ShadowCastersCannotBeReceivers = !shadowTextureSelfShadow;

			if (IsShadowTechniqueAdditive && currentViewport.ShadowsEnabled)
				// Additive lighting, we need to split everything by illumination stage
				q.SplitPassesByLightingType = true;
			else
				q.SplitPassesByLightingType = false;

			if (IsShadowTechniqueInUse && currentViewport.ShadowsEnabled)
				// Tell render queue to split off non-shadowable materials
				q.SplitNoShadowPasses = true;
			else
				q.SplitNoShadowPasses = false;
		}

		void UpdateRenderQueueGroupSplitOptions(RenderQueueGroup group, 
												bool suppressShadows,
												bool suppressRenderState) {
			if (IsShadowTechniqueStencilBased)
				// Casters can always be receivers
				group.ShadowCastersCannotBeReceivers = false;
			else if (IsShadowTechniqueTextureBased) 
				group.ShadowCastersCannotBeReceivers = !shadowTextureSelfShadow;

			if (!suppressShadows && currentViewport.ShadowsEnabled && IsShadowTechniqueAdditive)
				// Additive lighting, we need to split everything by illumination stage
				group.SplitPassesByLightingType = true;
			else
				group.SplitPassesByLightingType = false;

			if (!suppressShadows && currentViewport.ShadowsEnabled && IsShadowTechniqueInUse)
				// Tell render queue to split off non-shadowable materials
				group.SplitNoShadowPasses = true;
			else
				group.SplitNoShadowPasses = false;
		}

		/// <summary>
		///		Internal method for updating the scene graph ie the tree of SceneNode instances managed by this class.
		/// </summary>
		/// <remarks>
		///		This must be done before issuing objects to the rendering pipeline, since derived transformations from
		///		parent nodes are not updated until required. This SceneManager is a basic implementation which simply
		///		updates all nodes from the root. This ensures the scene is up to date but requires all the nodes
		///		to be updated even if they are not visible. Subclasses could trim this such that only potentially visible
		///		nodes are updated.
		/// </remarks>
		/// <param name="camera"></param>
		protected internal virtual void UpdateSceneGraph(Camera camera) {
            // Process queued needUpdate calls 
            Node.ProcessQueuedUpdates();

			// Cascade down the graph updating transforms & world bounds
			// In this implementation, just update from the root
			// Smarter SceneManager subclasses may choose to update only
			// certain scene graph branches based on space partioning info.
			rootSceneNode.Update(true, false);
		}

		/// <summary>
		///		Internal method which parses the scene to find visible objects to render.
		/// </summary>
		/// <remarks>
		///		If you're implementing a custom scene manager, this is the most important method to
		///		override since it's here you can apply your custom world partitioning scheme. Once you
		///		have added the appropriate objects to the render queue, you can let the default
		///		SceneManager objects RenderVisibleObjects handle the actual rendering of the objects
		///		you pick.
		///		<p/>
		///		Any visible objects will be added to a rendering queue, which is indexed by material in order
		///		to ensure objects with the same material are rendered together to minimise render state changes.
		/// </remarks>
		/// <param name="camera"></param>
		public virtual void FindVisibleObjects(Camera camera, VisibleObjectsBoundsInfo visibleBounds, bool onlyShadowCasters) {
			// ask the root node to iterate through and find visible objects in the scene
			rootSceneNode.FindVisibleObjects(camera, GetRenderQueue(), visibleBounds, true, displayNodes, onlyShadowCasters);
		}

		/// <summary>
		///		Internal method for applying animations to scene nodes.
		/// </summary>
		/// <remarks>
		///		Uses the internally stored AnimationState objects to apply animation to SceneNodes.
		/// </remarks>
		internal virtual void ApplySceneAnimations() {
			foreach(AnimationState animState in animationStateList.Values) {
				// get this states animation
				Animation anim = animationList[animState.Name];

				// loop through all node tracks and reset their nodes initial state
				foreach(NodeAnimationTrack nodeTrack in anim.NodeTracks.Values) {
					Node node = nodeTrack.TargetNode;
					node.ResetToInitialState();
				}

				// loop through all node tracks and reset their nodes initial state
				foreach(NumericAnimationTrack numericTrack in anim.NumericTracks.Values) {
					AnimableValue animable = numericTrack.TargetAnimable;
					animable.ResetToBaseValue();
				}

				// apply the animation
				anim.Apply(animState.Time, animState.Weight, false, 1.0f);
			}
		}

        protected internal void DestroyShadowTextures() {

            for (int i = 0; i < shadowTextures.Count; ++i) {
				Texture shadowTex = shadowTextures[i];
                // TODO: It would be useful to have a reference count for 
                // these textures.  They should only be removed from the
                // resource manager if nobody else is using them.

                // Material names are global to SM, make specific
                string matName = shadowTex.Name + "Mat" + smName;

                Material shadowMat = MaterialManager.Instance.GetByName(matName);
                shadowMat.GetTechnique(0).GetPass(0).RemoveAllTextureUnitStates();
                MaterialManager.Instance.Remove(matName);

                DestroyCamera(shadowTextureCameras[i]);
			}
			shadowTextures.Clear();
            shadowTextureCameras.Clear();

            // XXX - should call ClearUnused(), but we don't have real refcounting yet.
            ShadowTextureManager.Instance.ClearAll();

            shadowTextureConfigDirty = true;
        }
        protected internal void DestroyCamera(Camera camera) {
            cameraList.Remove(camera.Name);
            if (camVisibleObjectsMap.ContainsKey(camera))
                camVisibleObjectsMap.Remove(camera);
            targetRenderSystem.NotifyCameraRemoved(camera);
        }

        public void EnsureShadowTexturesCreated() {
            if (shadowTextureConfigDirty) {
                DestroyShadowTextures();
                ShadowTextureManager.Instance.GetShadowTextures(shadowTextureConfigList, shadowTextures);

                // clear shadow cam - light mapping
                shadowCamLightMapping.Clear();


                // Recreate shadow textures
                foreach (Texture shadowTex in shadowTextures) {

                    // Camera names are local to SM 
                    string camName = shadowTex.Name + "Cam";
                    // Material names are global to SM, make specific
                    string matName = shadowTex.Name + "Mat" + smName;

                    RenderTexture shadowRTT = shadowTex.GetBuffer().GetRenderTarget();

                    // Create camera for this texture, but note that we have to rebind
                    // in prepareShadowTextures to coexist with multiple SMs
                    Camera cam = CreateCamera(camName);
                    cam.AspectRatio = (float)shadowTex.Width / (float)shadowTex.Height;
                    // Don't use rendering distance for light cameras; we don't want shadows
                    // for visible objects disappearing, especially for directional lights
                    cam.UseRenderingDistance = false;
                    shadowTextureCameras.Add(cam);

                    // Create a viewport, if not there already
                    if (shadowRTT.NumViewports == 0) {
                        // Note camera assignment is transient when multiple SMs
                        Viewport view = shadowRTT.AddViewport(cam);
                        view.ClearEveryFrame = true;
                        // remove overlays
                        view.OverlaysEnabled = false;
                    }

                    // Don't update automatically - we'll do it when required
                    shadowRTT.IsAutoUpdated = false;

                    // Also create corresponding Material used for rendering this shadow
                    Material mat = (Material)MaterialManager.Instance.GetByName(matName);
                    if (mat == null) {
                        mat = (Material)MaterialManager.Instance.Create(matName);
                    Pass p = mat.GetTechnique(0).GetPass(0);
                    if (p.NumTextureUnitStages != 1 ||
                        p.GetTextureUnitState(0).GetTexturePtr(0) != shadowTex) {
                        mat.GetTechnique(0).GetPass(0).RemoveAllTextureUnitStates();
                        // create texture unit referring to render target texture
                        TextureUnitState texUnit = p.CreateTextureUnitState(shadowTex.Name);
                        // set projective based on camera
                        texUnit.SetProjectiveTexturing(!p.HasVertexProgram, cam);
                        // clamp to border colour
                        texUnit.TextureAddressing = TextureAddressing.Border;
                        texUnit.TextureBorderColor = ColorEx.White;
                        mat.Touch();
                    }

                    // insert dummy camera-light combination
                    shadowCamLightMapping[cam] = null;

                    // Get null shadow texture
                    if (shadowTextureConfigList.Count == 0)
                        nullShadowTexture = null;
                    else
                        nullShadowTexture = ShadowTextureManager.Instance.GetNullShadowTexture(
                            shadowTextureConfigList[0].format);
                    }
                }
                shadowTextureConfigDirty = false;
            }
        }

		private static TimingMeter prepareShadowTexturesMeter = MeterManager.GetMeter("Prepare Shadow Textures", "Prepare Shadow Textures");

		/// <summary>
		/// Internal method for preparing shadow textures ready for use in a regular render
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="viewPort"></param>
		protected internal virtual void PrepareShadowTextures(Camera camera, Viewport viewPort) {
			prepareShadowTexturesMeter.Enter();
			
            // create shadow textures if needed
            EnsureShadowTexturesCreated();
            
            // Set the illumination stage, prevents recursive calls
			IlluminationRenderStage savedStage = illuminationStage;
			illuminationStage = IlluminationRenderStage.RenderToTexture;

			// Determine far shadow distance
			float shadowDist = shadowFarDistance;
			if (shadowDist == 0.0f) {
				// need a shadow distance, make one up
				shadowDist = camera.Near * 300;
			}
			// set fogging to hide the shadow edge
			float shadowOffset = shadowDist * shadowTextureOffset;
            // Precalculate fading info
			float shadowEnd = shadowDist + shadowOffset;
            float fadeStart = shadowEnd * shadowTextureFadeStart;
            float fadeEnd = shadowEnd * shadowTextureFadeEnd;

            // set auto param for the start and end of the shadow fade region
            autoParamDataSource.ShadowFadeParams = new Vector4(fadeStart, fadeEnd, 0, 0);

			// Additive lighting should not use fogging, since it will overbrighten; use border clamp
            if (!IsShadowTechniqueAdditive) {
                shadowReceiverPass.SetFog(true, FogMode.Linear, ColorEx.White,
                                          0, fadeStart, fadeEnd);
                // if we have a custom receiver material, then give it the fog params too
                if (shadowTextureCustomReceiverPass != null)
                    shadowTextureCustomReceiverPass.SetFog(true, FogMode.Linear, ColorEx.White,
                                                           0, fadeStart, fadeEnd);
            } else {
                // disable fogging explicitly
                shadowReceiverPass.SetFog(true, FogMode.None);
                // if we have a custom receiver material, then give it the fog params too
                if (shadowTextureCustomReceiverPass != null)
                    shadowTextureCustomReceiverPass.SetFog(true, FogMode.None);
            }

			// Iterate over the lights we've found, max out at the limit of light textures
			for (int i = 0, sti = 0;
				i < lightsAffectingFrustum.Count && sti < shadowTextures.Count; i++) {
				Light light = lightsAffectingFrustum[i];
				// Skip non-shadowing lights
				if (!light.CastShadows)
					continue;

				Texture shadowTex = shadowTextures[sti];
                RenderTarget shadowRTT = shadowTex.GetBuffer().GetRenderTarget();
				Viewport shadowView = shadowRTT.GetViewport(0);
				Camera texCam = shadowTextureCameras[sti];
				// rebind camera, incase another SM in use which has switched to its cam
				// This seems pretty silly - - is there any reason to do it?
				shadowView.Camera = texCam;

				// update shadow cam - light mapping
                shadowCamLightMapping[texCam] = light;

                if (light.CustomShadowCameraSetup == null)
                    defaultShadowCameraSetup.GetShadowCamera(this, camera, shadowView, light, texCam);
                else
                    light.CustomShadowCameraSetup.GetShadowCamera(this, camera, shadowView, light, texCam);
                
                // Setup background colour
                shadowView.BackgroundColor = ColorEx.White;

                // Fire shadow caster update, callee can alter camera settings
                // fireShadowTexturesPreCaster(light, texCam);

                // Update target
                shadowRTT.Update();
                
                sti++;
                
            }
            illuminationStage = savedStage;
            
            //fireShadowTexturesUpdated(Math.Min(lightsAffectingFrustum.Count, shadowTextures.Count));

            ShadowTextureManager.Instance.ClearUnused();

			prepareShadowTexturesMeter.Exit();
        }

		/// <summary>
		///		Internal method for setting the destination viewport for the next render.
		/// </summary>
		/// <param name="viewport"></param>
		protected virtual void SetViewport(Viewport viewport) {
			currentViewport = viewport;

			// Set viewport in render system
			targetRenderSystem.SetViewport(viewport);

            // Set the active material scheme for this viewport
            if (MaterialManager.Instance.ActiveScheme != viewport.MaterialScheme)
            {
                MaterialManager.Instance.ActiveScheme = viewport.MaterialScheme;

                // Any entities in the scene that have skeletons need to determine if they
                // need to enable or disable software skinning, since the new scheme may
                // change whether they are using a shader with hardware vertex processing.
                ICollection<MovableObject> entityList = GetMovableObjectCollection("Entity");
                foreach (MovableObject obj in entityList)
                {
                    Entity entity = obj as Entity;
                    Debug.Assert(entity != null);
                    if (entity != null && entity.HasSkeleton)
                    {
                        entity.ReevaluateVertexProcessing();
                    }
                }
            }
		}

		protected void RenderSingleObject(IRenderable renderable, Pass pass, bool doLightIteration) {
			RenderSingleObject(renderable, pass, doLightIteration, null);
		}

		private static TimingMeter doLightMeter = MeterManager.GetMeter("Do Light", "Render Single");
		private static TimingMeter updateAutoParmsMeter = MeterManager.GetMeter("Update Auto Parms", "Render Single");
		private static TimingMeter setGpuParmsMeter = MeterManager.GetMeter("Set Gpu Parms", "Render Single");
		private static TimingMeter callRenderSingleMeter = MeterManager.GetMeter("Call RenderSingle", "Render Single");
		private static TimingMeter useLightsMeter = MeterManager.GetMeter("Use Lights", "Render Single");
		private static TimingMeter renderOpMeter = MeterManager.GetMeter("Render Op", "Render Single");
		private static TimingMeter renderSolidObjectsMeter = MeterManager.GetMeter("Render Solid Objects", "Render Solid Objects");
		
		/// <summary>
		///		Internal utility method for rendering a single object.
		/// </summary>
		/// <param name="renderable">The renderable to issue to the pipeline.</param>
		/// <param name="pass">The pass which is being used.</param>
		/// <param name="doLightIteration">If true, this method will issue the renderable to
		/// the pipeline possibly multiple times, if the pass indicates it should be
		/// done once per light.</param>
		/// <param name="manualLightList">Only applicable if 'doLightIteration' is false, this
		/// method allows you to pass in a previously determined set of lights
		/// which will be used for a single render of this object.</param>
		protected virtual void RenderSingleObject(IRenderable renderable, Pass pass, 
			bool doLightIteration, List<Light> manualLightList) {

            targetRenderSystem.BeginProfileEvent(ColorEx.Red, "RenderSingleObject: Material = " + renderable.Material.Name);

			ushort numMatrices = 0;

			// grab the current scene detail level
			SceneDetailLevel camDetailLevel = cameraInProgress.SceneDetail;
        
// 			// update auto params if this is a programmable pass
// 			if(pass.IsProgrammable) {
// 				autoParamDataSource.Renderable = renderable;
// 				pass.UpdateAutoParamsNoLights(autoParamDataSource);
// 			}

			// get the world matrices and the count
			renderable.GetWorldTransforms(xform);
			numMatrices = renderable.NumWorldTransforms;

			// set the world matrices in the render system
			if(numMatrices > 1) {
				targetRenderSystem.SetWorldMatrices(xform, numMatrices);
			}
			else {
				targetRenderSystem.WorldMatrix = xform[0];
			}

			// issue view/projection changes (if any)
			UseRenderableViewProjection(renderable);

			if (!suppressRenderStateChanges) {
				bool passSurfaceAndLightParams = true;
				if (pass.IsProgrammable) {
					// Tell auto params object about the renderable change
					autoParamDataSource.Renderable = renderable;
                    // Tell auto params object about the world matrices, eliminated query from renderable again
                    autoParamDataSource.SetWorldMatrices(xform, numMatrices);
					pass.UpdateAutoParamsNoLights(autoParamDataSource);
					if (pass.HasVertexProgram)
						passSurfaceAndLightParams = pass.VertexProgram.PassSurfaceAndLightStates;
				}

				// issue texture units that depend on updated view matrix
				// reflective env mapping is one case
				for(int i = 0; i < pass.NumTextureUnitStages; i++) {
					TextureUnitState texUnit = pass.GetTextureUnitState(i);

					if(texUnit.HasViewRelativeTexCoordGen) {
						targetRenderSystem.SetTextureUnit(i, texUnit, !pass.HasFragmentProgram);
					}
				}

				// Normalize normals
				bool thisNormalize = renderable.NormalizeNormals;

				if(thisNormalize != normalizeNormals) {
					targetRenderSystem.NormalizeNormals = thisNormalize;
					normalizeNormals = thisNormalize;
				}

				// Set up the solid / wireframe override
				SceneDetailLevel requestedDetail = renderable.RenderDetail;
				if (requestedDetail != lastDetailLevel || requestedDetail != camDetailLevel) {
					if (requestedDetail > camDetailLevel) {
						// only downgrade detail; if cam says wireframe we don't go up to solid
						requestedDetail = camDetailLevel;
					}
					targetRenderSystem.RasterizationMode = requestedDetail;
					lastDetailLevel = requestedDetail;

				}

				// TODO: Add ClipPlanes to RenderSystem.cs
				//targetRenderSystem.ClipPlanes = renderable.ClipPlanes;

				// get the renderables render operation
				renderable.GetRenderOperation(op);
				// TODO: Add srcRenderable to RenderOperation.cs
				//op.srcRenderable = renderable;
			
				if(doLightIteration) {
					// Here's where we issue the rendering operation to the render system
					// Note that we may do this once per light, therefore it's in a loop
					// and the light parameters are updated once per traversal through the
					// loop
					doLightMeter.Enter();
					if (MeterManager.Collecting) {
						string s = string.Format("Rendering material '{0}'", renderable.Material.Name);
                        if (renderable is SubEntity) {
                            SubEntity subEntity = (SubEntity)renderable;
                            s += string.Format(", SubMesh '{0}', Entity '{1}'", subEntity.SubMesh.Name, subEntity.Parent.Name);
                        }
                        MeterManager.AddInfoEvent(s);
                    }

                    List<Light> rendLightList = renderable.Lights;
					bool iteratePerLight = pass.RunOncePerLight;
                    int startLight = pass.StartLight;
                    int lightsLeft = iteratePerLight ? rendLightList.Count - startLight : 1;
                    List<Light> lightListToUse = null;
					int lightIndex = startLight;

                    while (lightsLeft > 0) {
						// determine light list to use
						if(iteratePerLight) {
							localLightList.Clear();
                            int lightsPerIteration = pass.LightsPerIteration;
                            int numShadowTextureLights = 0;
                            int lightsConsidered = 0;
                            for (int i=0; i<lightsPerIteration && lightIndex < rendLightList.Count; i++,lightIndex++,lightsLeft--) {
                                // check whether we need to filter this one out
                                lightsConsidered++;
                                if(pass.RunOnlyForOneLightType && pass.OnlyLightType != rendLightList[lightIndex].Type)
                                    // skip this one
                                    continue;
                                localLightList.Add(rendLightList[lightIndex]);
                                // potentially need to update content_type shadow texunit
                                // corresponding to this light
                                if (IsShadowTechniqueTextureBased && lightIndex < shadowTextures.Count) {
                                    // link the numShadowTextureLights'th shadow texture unit
                                    int tuindex = pass.GetTextureUnitWithContentTypeIndex(
                                        TextureContentType.Shadow, numShadowTextureLights);
                                    if (tuindex < pass.NumTextureUnitStages) {
                                        TextureUnitState tu = pass.GetTextureUnitState(tuindex);
                                        tu.SetTexturePtr(shadowTextures[lightIndex]);
                                        Camera cam = shadowTextures[lightIndex].GetBuffer().GetRenderTarget().GetViewport(0).Camera;
                                        if (!pass.HasVertexProgram)
                                            tu.SetProjectiveTexturing(true, cam);
                                        autoParamDataSource.SetTextureProjector(cam, numShadowTextureLights);
                                        ++numShadowTextureLights;
                                        // Have to set TU on rendersystem right now, although
                                        // autoparams will be set later
                                        targetRenderSystem.SetTextureUnit(tuindex, tu, !pass.HasFragmentProgram);
                                    }
                                }
                            }
                            // Did we run out of lights before slots? e.g. 5 lights, 2 per iteration
                            if (lightsPerIteration != lightsConsidered)
                                lightsLeft = 0;
                            lightListToUse = localLightList;
						}
						else {
							if (startLight != 0) {
                                if (startLight >= rendLightList.Count) {
                                    lightsLeft = 0;
                                    break;
                                }
                                else {
                                    localLightList.Clear();
                                    for (int i=startLight; i<rendLightList.Count; i++)
                                        localLightList.Add(rendLightList[i]);
                                    lightListToUse = localLightList;
                                }
                            }
                            else
                                // use complete light list
                                lightListToUse = rendLightList;
                            lightsLeft = 0;
						}

						if(pass.IsProgrammable) {
							// Update any automatic gpu params for lights
							// Other bits of information will have to be looked up
							updateAutoParmsMeter.Enter();
							autoParamDataSource.SetCurrentLightList(lightListToUse);
							pass.UpdateAutoParamsLightsOnly(autoParamDataSource);
							updateAutoParmsMeter.Exit();

							// note: parameters must be bound after auto params are updated
							setGpuParmsMeter.Enter();
							if(pass.HasVertexProgram) {
                                if (MeterManager.Collecting)
                                    MeterManager.AddInfoEvent("Vertex Program " + pass.VertexProgramName);
								targetRenderSystem.BindGpuProgramParameters(GpuProgramType.Vertex, pass.VertexProgramParameters);
							}
							if(pass.HasFragmentProgram) {
                                if (MeterManager.Collecting)
                                    MeterManager.AddInfoEvent("Fragment Program " + pass.FragmentProgramName);
								targetRenderSystem.BindGpuProgramParameters(GpuProgramType.Fragment, pass.FragmentProgramParameters);
							}
							setGpuParmsMeter.Exit();
						}

						// Do we need to update light states? 
						// Only do this if fixed-function vertex lighting applies
						if(pass.LightingEnabled && passSurfaceAndLightParams) {
							//useLightsMeter.Enter();
							targetRenderSystem.UseLights(lightListToUse, pass.MaxLights);
							//useLightsMeter.Exit();
						}
						// issue the render op		
						renderOpMeter.Enter();
						targetRenderSystem.Render(op);
						renderOpMeter.Exit();
					} // iterate per light
					doLightMeter.Exit();
				}
				else {
					// do we need to update GPU program parameters?
					if(pass.IsProgrammable) {
						// do we have a manual light list
						if(manualLightList != null) {
							// Update any automatic gpu params for lights
							// Other bits of information will have to be looked up
							autoParamDataSource.SetCurrentLightList(manualLightList);
							pass.UpdateAutoParamsLightsOnly(autoParamDataSource);
						}

						// note: parameters must be bound after auto params are updated
						if(pass.HasVertexProgram) {
							targetRenderSystem.BindGpuProgramParameters(GpuProgramType.Vertex, pass.VertexProgramParameters);
						}

						if(pass.HasFragmentProgram) {
							 targetRenderSystem.BindGpuProgramParameters(GpuProgramType.Fragment, pass.FragmentProgramParameters);
						}
					}

				
					// Use manual lights if present, and not using vertex programs
					if(manualLightList != null && pass.LightingEnabled && passSurfaceAndLightParams) {
						targetRenderSystem.UseLights(manualLightList, pass.MaxLights);
					}

					// issue the render op		
					renderOpMeter.Enter();
					targetRenderSystem.Render(op);
					renderOpMeter.Exit();
				}
			}
			else { // suppressRenderStateChanges
				// Just render
                renderOpMeter.Enter();
				targetRenderSystem.Render(op);
				renderOpMeter.Exit();
			}

            // Reset view / projection changes if any
            ResetViewProjMode();

            targetRenderSystem.EndProfileEvent();
		}

		/// <summary>
		///		Renders a set of solid objects.
		/// </summary>
		/// <param name="list">List of solid objects.</param>
		protected virtual void RenderSolidObjects(SortedList list, bool doLightIteration, 
			List<Light> manualLightList) {
			// ----- SOLIDS LOOP -----
// 			renderSolidObjectsMeter.Enter();
			for(int i = 0; i < list.Count; i++) {
				RenderableList renderables = (RenderableList)list.GetByIndex(i);

				// bypass if this group is empty
				if(renderables.Count == 0) {
					continue;
				}

				Pass pass = (Pass)list.GetKey(i);

				// Give SM a chance to eliminate this pass
				if (!ValidatePassForRendering(pass))
					continue;

				// For solids, we try to do each pass in turn
				Pass usedPass = SetPass(pass);

				// render each object associated with this rendering pass
				for(int r = 0; r < renderables.Count; r++) {

					IRenderable renderable = (IRenderable)renderables[r];

					// Give SM a chance to eliminate
					if (!ValidateRenderableForRendering(usedPass, renderable))
						continue;
					
					// Render a single object, this will set up auto params if required
					callRenderSingleMeter.Enter();
// 					if (MeterManager.Collecting)
// 						MeterManager.AddInfoEvent("RenderSingle material " + renderable.Material.Name +
//                             ", doLight " + doLightIteration + ", lightCnt " + (manualLightList != null ? manualLightList.Count : 0));
                    try {
                        RenderSingleObject(renderable, usedPass, doLightIteration, manualLightList);
                    } catch (Exception e) {
                        LogManager.Instance.WriteException("Invalid call to Axiom.Core.SceneManager.RenderSingleObject: {0}\n{1}", e.Message, e.StackTrace);
                        if (renderable.Material != null)
                            log.InfoFormat("Failed renderable material: {0}", renderable.Material.Name);
                    }
					callRenderSingleMeter.Exit();
				}
			}
// 			renderSolidObjectsMeter.Exit();
		}

		protected void RenderSolidObjects(SortedList list, bool doLightIteration) {
			RenderSolidObjects(list, doLightIteration, null);
		}

		/// <summary>
		///		Renders a set of transparent objects.
		/// </summary>
		/// <param name="list"></param>
		protected virtual void RenderTransparentObjects(ArrayList list, bool doLightIteration,
            List<Light> manualLightList) {
			// ----- TRANSPARENT LOOP -----
			// This time we render by Z, not by material
			// The transparent objects set needs to be ordered first
			for(int i = 0; i < list.Count; i++) {
				RenderablePass rp = (RenderablePass)list[i];

				// set the pass first
				SetPass(rp.pass);

				// render the transparent object
				RenderSingleObject(rp.renderable, rp.pass, doLightIteration,
					manualLightList);
			}
		}

		protected void RenderTransparentObjects(ArrayList list, bool doLightIteration) {
			RenderTransparentObjects(list, doLightIteration, null);
		}

		/// <summary>
		///		Render a group with the added complexity of additive stencil shadows.
		/// </summary>
		/// <param name="group">Render queue group.</param>
		protected virtual void RenderAdditiveStencilShadowedQueueGroupObjects(RenderQueueGroup group) {
            List<Light> tempLightList = new List<Light>();

			for(int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// sort the group first
				priorityGroup.Sort(cameraInProgress);

				// Clear light list
				tempLightList.Clear();

				// Render all the ambient passes first, no light iteration, no lights
				illuminationStage = IlluminationRenderStage.Ambient;
				RenderSolidObjects(priorityGroup.solidPasses, false, tempLightList);
				// Also render any objects which have receive shadows disabled
                renderingNoShadowQueue = true;
				RenderSolidObjects(priorityGroup.solidPassesNoShadow, true);
                renderingNoShadowQueue = false;

				// Now iterate per light
				illuminationStage = IlluminationRenderStage.PerLight;

				for (int li = 0; li < lightsAffectingFrustum.Count; li++) {
					Light light = lightsAffectingFrustum[li];
					// Set light state

					if (light.CastShadows) {
						// Clear stencil
						targetRenderSystem.ClearFrameBuffer(FrameBuffer.Stencil);
						RenderShadowVolumesToStencil(light, cameraInProgress);
						// turn stencil check on
						targetRenderSystem.StencilCheckEnabled = true;
						// NB we render where the stencil is equal to zero to render lit areas
						targetRenderSystem.SetStencilBufferParams(CompareFunction.Equal, 0);
					}

					// render lighting passes for this light
					if (tempLightList.Count == 0) {
						tempLightList.Add(light);
					}
					else {
						tempLightList[0] = light;
					}

					RenderSolidObjects(priorityGroup.solidPassesDiffuseSpecular, false, tempLightList);

					// Reset stencil params
					targetRenderSystem.SetStencilBufferParams();
					targetRenderSystem.StencilCheckEnabled = false;
					targetRenderSystem.SetDepthBufferParams();

				}// for each light


				// Now render decal passes, no need to set lights as lighting will be disabled
				illuminationStage = IlluminationRenderStage.Decal;
				RenderSolidObjects(priorityGroup.solidPassesDecal, false);


			}// for each priority

			// reset lighting stage
			illuminationStage = IlluminationRenderStage.None;

			// Iterate again
			for(int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// Do transparents
				RenderTransparentObjects(priorityGroup.transparentPasses, true);

			}// for each priority
		}

		/// <summary>
		///		Render a group with the added complexity of modulative stencil shadows.
		/// </summary>
		/// <param name="group">Render queue group.</param>
		protected virtual void RenderModulativeStencilShadowedQueueGroupObjects(RenderQueueGroup group) {
			/* For each light, we need to render all the solids from each group, 
			then do the modulative shadows, then render the transparents from
			each group.
			Now, this means we are going to reorder things more, but that it required
			if the shadows are to look correct. The overall order is preserved anyway,
			it's just that all the transparents are at the end instead of them being
			interleaved as in the normal rendering loop. 
			*/
			for(int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// sort the group first
				priorityGroup.Sort(cameraInProgress);

				// do solids
				RenderSolidObjects(priorityGroup.solidPasses, true);
			}

			// iterate over lights, rendering all volumes to the stencil buffer
			for(int i = 0; i < lightsAffectingFrustum.Count; i++) {
				Light light = lightsAffectingFrustum[i];

				if(light.CastShadows) {
					// clear the stencil buffer
					targetRenderSystem.ClearFrameBuffer(FrameBuffer.Stencil);
					RenderShadowVolumesToStencil(light, cameraInProgress);

					// render full-screen shadow modulator for all lights
					SetPass(shadowModulativePass);

					// turn the stencil check on
					targetRenderSystem.StencilCheckEnabled = true;

					// we render where the stencil is not equal to zero to render shadows, not lit areas
					targetRenderSystem.SetStencilBufferParams(CompareFunction.NotEqual, 0);
					RenderSingleObject(fullScreenQuad, shadowModulativePass, false);

					// reset stencil buffer params
					targetRenderSystem.SetStencilBufferParams();
					targetRenderSystem.StencilCheckEnabled = false;
					targetRenderSystem.SetDepthBufferParams();
				}
			}// for each light

			for(int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// Do non-shadowable solids
                renderingNoShadowQueue = true;
				RenderSolidObjects(priorityGroup.solidPassesNoShadow, true);
                renderingNoShadowQueue = false;
			}// for each priority

			for(int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// Do transparents
				RenderTransparentObjects(priorityGroup.transparentPasses, true);

			} // for each priority
		}

		/// <summary>
		///		Render a group rendering only shadow casters.
		/// </summary>
		/// <param name="group">Render queue group.</param>
		protected virtual void RenderTextureShadowCasterQueueGroupObjects(RenderQueueGroup group) {
			// This is like the basic group render, except we skip all transparents
			// and we also render any non-shadowed objects
			// Note that non-shadow casters will have already been eliminated during
			// FindVisibleObjects

			// Override auto param ambient to force vertex programs and fixed function to 
			// use shadow colour
			if (IsShadowTechniqueAdditive) {
				autoParamDataSource.AmbientLight = ColorEx.Black;
				targetRenderSystem.AmbientLight = ColorEx.Black;
			}
			else {
				autoParamDataSource.AmbientLight = shadowColor;
				targetRenderSystem.AmbientLight = shadowColor;
			}
			
			// Iterate through priorities
			for(int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// Sort the queue first
				priorityGroup.Sort(cameraInProgress);

				// Do solids, override light list in case any vertex programs use them
				RenderSolidObjects(priorityGroup.solidPasses, false, nullLightList);
                renderingNoShadowQueue = true;
				RenderSolidObjects(priorityGroup.solidPassesNoShadow, false, nullLightList);
                renderingNoShadowQueue = false;

				// Do transparents that cast shadows
				RenderTransparentShadowCasterObjects(priorityGroup.transparentPasses, false, nullLightList);
			}// for each priority

			// reset ambient light
			autoParamDataSource.AmbientLight = ambientColor;
			targetRenderSystem.AmbientLight = ambientColor;
		}

        private static TimingMeter renderModulativeGroupsMeter = MeterManager.GetMeter("Render Groups", "Render Modulative");
        private static TimingMeter renderModulativeLightsMeter = MeterManager.GetMeter("Render Lights", "Render Modulative");
        
        /// <summary>
		///		Render a group with the added complexity of modulative texture shadows.
		/// </summary>
		/// <param name="group">Render queue group.</param>
		protected virtual void RenderModulativeTextureShadowedQueueGroupObjects(RenderQueueGroup group) {
			/* For each light, we need to render all the solids from each group, 
			then do the modulative shadows, then render the transparents from
			each group.
			Now, this means we are going to reorder things more, but that is required
			if the shadows are to look correct. The overall order is preserved anyway,
			it's just that all the transparents are at the end instead of them being
			interleaved as in the normal rendering loop. 
			*/
			// Iterate through priorities
			renderModulativeGroupsMeter.Enter();
            for(int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// Sort the queue first
				priorityGroup.Sort(cameraInProgress);

				// Do solids
				RenderSolidObjects(priorityGroup.solidPasses, true);
                renderingNoShadowQueue = true;
				RenderSolidObjects(priorityGroup.solidPassesNoShadow, true);
                renderingNoShadowQueue = false;
			}
			renderModulativeGroupsMeter.Exit();

			renderModulativeLightsMeter.Enter();
            // Iterate over lights, render received shadows
			// only perform this if we're in the 'normal' render stage, to avoid
			// doing it during the render to texture
			if (illuminationStage == IlluminationRenderStage.None) {
				illuminationStage = IlluminationRenderStage.RenderModulativePass;

				for (int i = 0, sti = 0;
					i < lightsAffectingFrustum.Count && sti < shadowTextures.Count; i++) {
					Light light = lightsAffectingFrustum[i];

					if (!light.CastShadows)
						continue;

					Texture shadowTex = shadowTextures[sti];
                    Camera cam = shadowTex.GetBuffer().GetRenderTarget().GetViewport(0).Camera;
					
                    // Hook up receiver texture
					Pass targetPass = shadowTextureCustomReceiverPass != null ?
						shadowTextureCustomReceiverPass : shadowReceiverPass;
					targetPass.GetTextureUnitState(0).SetTextureName(shadowTex.Name);
					// Hook up projection frustum
					targetPass.GetTextureUnitState(0).SetProjectiveTexturing(true, cam);
					autoParamDataSource.TextureProjector = cam;

					// if this light is a spotlight, we need to add the spot fader layer
					if (light.Type == LightType.Spotlight) {
						// remove all TUs except 0 & 1 
						// (only an issue if additive shadows have been used)
						while (targetPass.NumTextureUnitStages > 2)
							targetPass.RemoveTextureUnitState(2);

						// Add spot fader if not present already
						if (targetPass.NumTextureUnitStages == 2 && 
							targetPass.GetTextureUnitState(1).TextureName == "spot_shadow_fade.png") {
							// Just set 
                            TextureUnitState tex = 
                                targetPass.GetTextureUnitState(1);
							tex.SetProjectiveTexturing(true, cam);
						}
						else {
							// Remove any non-conforming spot layers
							while (targetPass.NumTextureUnitStages > 1)
								targetPass.RemoveTextureUnitState(1);

                            TextureUnitState tex = 
                                targetPass.CreateTextureUnitState("spot_shadow_fade.png");
							tex.SetProjectiveTexturing(true, cam);
							tex.SetColorOperation(LayerBlendOperation.Add);
							tex.TextureAddressing = TextureAddressing.Clamp;
						}
					}
					else {
						// remove all TUs except 0 including spot
						while (targetPass.NumTextureUnitStages > 1)
							targetPass.RemoveTextureUnitState(1);
					}
					
					// Set lighting / blending modes
					targetPass.SetSceneBlending(SceneBlendFactor.DestColor, SceneBlendFactor.Zero);
					targetPass.LightingEnabled = false;

					targetPass.Load();

                    // Fire pre-reciever event
                    // fireShadowTexturesPreReceiver(light, cam);

					RenderTextureShadowReceiverQueueGroupObjects(group);
					++sti;

				} // for each light

				illuminationStage = IlluminationRenderStage.None;
			}
            renderModulativeLightsMeter.Exit();

			// Iterate again
			for (int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// Do transparents
				RenderTransparentObjects(priorityGroup.transparentPasses, true);
			} // for each priority
		}

		/// <summary>
		///		Render a group with the added complexity of additive texture shadows.
		/// </summary>
		/// <param name="group">Render queue group.</param>
		void RenderAdditiveTextureShadowedQueueGroupObjects(RenderQueueGroup group) {
			List<Light> tempLightList = new List<Light>();
			for(int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// Sort the queue first
				priorityGroup.Sort(cameraInProgress);

				// Clear light list
				tempLightList.Clear();

				// Render all the ambient passes first, no light iteration, no lights
				RenderSolidObjects(priorityGroup.solidPasses, false, tempLightList);
				// Also render any objects which have receive shadows disabled
                renderingNoShadowQueue = true;
				RenderSolidObjects(priorityGroup.solidPassesNoShadow, true);
                renderingNoShadowQueue = false;

				// only perform this next part if we're in the 'normal' render stage, to avoid
				// doing it during the render to texture
				if (illuminationStage == IlluminationRenderStage.None) {
                    
                    ICollection<MovableObject> lightList = GetMovableObjectCollection("Light");
                    List<Light> lightList2 = new List<Light>();
                    foreach (Light l in lightList)
                        lightList2.Add(l);
                    lightList2.Sort();

					// Iterate over lights, render masked
					for (int li = 0, sti = 0; li < lightsAffectingFrustum.Count; li++) {
						Light light = lightsAffectingFrustum[li];
						// Set light state
						if (light.CastShadows && sti < shadowTextures.Count) {
							Texture shadowTex = shadowTextures[sti];
							// Get camera for current shadow texture
							Camera camera = shadowTex.GetBuffer().GetRenderTarget().GetViewport(0).Camera;
							// Hook up receiver texture
							Pass targetPass = shadowTextureCustomReceiverPass != null ?
								shadowTextureCustomReceiverPass : shadowReceiverPass;
							targetPass.GetTextureUnitState(0).SetTextureName(shadowTex.Name);
							// Hook up projection frustum
							targetPass.GetTextureUnitState(0).SetProjectiveTexturing(true, camera);
							autoParamDataSource.TextureProjector = camera;
							// Remove any spot fader layer
							if (targetPass.NumTextureUnitStages > 1 && 
								targetPass.GetTextureUnitState(1).TextureName == "spot_shadow_fade.png") {
								// remove spot fader layer (should only be there if
								// we previously used modulative shadows)
								targetPass.RemoveTextureUnitState(1);
							}
							// Set lighting / blending modes
							targetPass.SetSceneBlending(SceneBlendFactor.One, SceneBlendFactor.One);
							targetPass.LightingEnabled = true;
							targetPass.Load();
							// increment shadow texture since used
							++sti;
							illuminationStage = IlluminationRenderStage.RenderModulativePass;
						}
						else {
							illuminationStage = IlluminationRenderStage.None;
						}

						// render lighting passes for this light
						if (tempLightList.Count == 0)
							tempLightList.Add(light);
						else
							tempLightList[0] = light;

                        RenderSolidObjects(priorityGroup.solidPassesDiffuseSpecular, false, lightList2);

					}// for each light
					illuminationStage = IlluminationRenderStage.None;

					// Now render decal passes, no need to set lights as lighting will be disabled
					RenderSolidObjects(priorityGroup.solidPassesDecal, false);
				}

			}// for each priority

			for(int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// Do transparents
				RenderTransparentObjects(priorityGroup.transparentPasses, true);
			}// for each priority
		}

		/// <summary>
		///		Render a group rendering only shadow receivers.
		/// </summary>
		/// <param name="group">Render queue group.</param>
		protected virtual void RenderTextureShadowReceiverQueueGroupObjects(RenderQueueGroup group) {
			// Override auto param ambient to force vertex programs to go full-bright
			autoParamDataSource.AmbientLight = ColorEx.White;
			targetRenderSystem.AmbientLight = ColorEx.White;

			// Iterate through priorities
			for(int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// Do solids, override light list in case any vertex programs use them
				RenderSolidObjects(priorityGroup.solidPasses, false, nullLightList);

				// Don't render transparents or passes which have shadow receipt disabled

			}// for each priority

			// reset ambient
			autoParamDataSource.AmbientLight = ambientColor;
			targetRenderSystem.AmbientLight = ambientColor;
		}

		/// <summary>
		///		Internal method to validate whether a Pass should be allowed to render.
		/// </summary>
		/// <remarks>
		///		Called just before a pass is about to be used for rendering a group to
		///		allow the SceneManager to omit it if required. A return value of false
		///		skips this pass. 
		/// </remarks>
		protected virtual bool ValidatePassForRendering(Pass pass) {
			// Bypass if we're doing a texture shadow render and 
			// this pass is after the first (only 1 pass needed for shadow texture)
			if (!suppressShadows && currentViewport.ShadowsEnabled &&
				((IsShadowTechniqueModulative && illuminationStage == IlluminationRenderStage.RenderModulativePass) ||
				 illuminationStage == IlluminationRenderStage.RenderToTexture || suppressRenderStateChanges) &&
				 pass.Index > 0) {
				return false;
			}

			return true;
		}

		/// <summary>
		///		Internal method to validate whether a Renderable should be allowed to render.
		/// </summary>
		/// <remarks>
		///		Called just before a pass is about to be used for rendering a Renderable to
		///		allow the SceneManager to omit it if required. A return value of false
		///		skips it. 
		/// </remarks>
		protected virtual bool ValidateRenderableForRendering(Pass pass, IRenderable renderable) {
			// Skip this renderable if we're doing texture shadows, it casts shadows			
			// and we're doing the render receivers pass
			if (!suppressShadows && currentViewport.ShadowsEnabled && IsShadowTechniqueTextureBased) {
				if (illuminationStage == IlluminationRenderStage.RenderModulativePass && 
					renderable.CastsShadows && !shadowTextureSelfShadow)
					return false;
				// Some duplication here with validatePassForRendering, for transparents
				if (((IsShadowTechniqueModulative && illuminationStage == IlluminationRenderStage.RenderModulativePass)
					 || illuminationStage == IlluminationRenderStage.RenderToTexture || suppressRenderStateChanges) && 
					pass.Index > 0)
					return false;
			}
			return true;
		}

		private static TimingMeter renderQueueGroupMeter = MeterManager.GetMeter("Render Queue Group", "Render Queue Group");
		private static TimingMeter renderAdditiveGroupMeter = MeterManager.GetMeter("Render Additive Group", "Render Queue Group");
		private static TimingMeter renderModulativeGroupMeter = MeterManager.GetMeter("Render Modulative Group", "Render Queue Group");
		private static TimingMeter renderTextureCasterGroupMeter = MeterManager.GetMeter("Render Texture Caster Group", "Render Queue Group");
		private static TimingMeter renderTextureShadowedGroupMeter = MeterManager.GetMeter("Render Texture Shadowed Group", "Render Queue Group");

		/// <summary>
		///		Render the objects in a given queue group.
		/// </summary>
		/// <param name="group">Group containing the objects to render.</param>
		protected virtual void RenderQueueGroupObjects(RenderQueueGroup group) {
			// Redirect to alternate versions if stencil shadows in use
			bool doShadows = group.ShadowsEnabled && currentViewport.ShadowsEnabled && 
				!suppressShadows && !suppressRenderStateChanges;
			renderQueueGroupMeter.Enter();
			if(doShadows && shadowTechnique == ShadowTechnique.StencilAdditive) {
				renderAdditiveGroupMeter.Enter();
				RenderAdditiveStencilShadowedQueueGroupObjects(group);
				renderAdditiveGroupMeter.Exit();
			}
			else if(doShadows && shadowTechnique == ShadowTechnique.StencilModulative) {
				renderModulativeGroupMeter.Enter();
				RenderModulativeStencilShadowedQueueGroupObjects(group);
				renderModulativeGroupMeter.Exit();
			}
			else if (IsShadowTechniqueTextureBased) {
				// Modulative texture shadows in use
				if (illuminationStage == IlluminationRenderStage.RenderToTexture) {
					// Shadow caster pass
					if (currentViewport.ShadowsEnabled && !suppressShadows && !suppressRenderStateChanges) {
						renderTextureCasterGroupMeter.Enter();
						RenderTextureShadowCasterQueueGroupObjects(group);
						renderTextureCasterGroupMeter.Exit();
					}
				}
				else {
					// Ordinary + receiver pass
					if (doShadows) {
						renderTextureShadowedGroupMeter.Enter();
						if (IsShadowTechniqueAdditive)
							RenderAdditiveTextureShadowedQueueGroupObjects(group);
						else
							RenderModulativeTextureShadowedQueueGroupObjects(group);
						renderTextureShadowedGroupMeter.Exit();
					}
					else
						RenderBasicQueueGroupObjects(group);
				}
			}
			else
				// No shadows, ordinary pass
				RenderBasicQueueGroupObjects(group);

			renderQueueGroupMeter.Exit();
		}

		/// <summary>
		///		Render a group in the ordinary way
		/// </summary>
		/// <param name="group">Group containing the objects to render.</param>
		protected virtual void RenderBasicQueueGroupObjects(RenderQueueGroup group) {
			// Basic render loop
			// Iterate through priorities
			for(int i = 0; i < group.NumPriorityGroups; i++) {
				RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

				// Sort the queue first
				priorityGroup.Sort(cameraInProgress);

				// Do solids
				RenderSolidObjects(priorityGroup.solidPasses, true);

				// Do transparents
				RenderTransparentObjects(priorityGroup.transparentPasses, true);
			}// for each priority
		}

		/// <summary>
		///		Render those objects in the transparent pass list which have shadow casting forced on
		/// </summary>
		/// <remarks>
		///		This function is intended to be used to render the shadows of transparent objects which have
		///		transparency_casts_shadows set to 'on' in their material
		/// </remarks>
		/// <param name="list"></param>
		/// <param name="doLightIteration"></param>
		/// <param name="manualLightList"></param>
		protected virtual void RenderTransparentShadowCasterObjects(ArrayList list,
            bool doLightIteration, List<Light> manualLightList) {
			// ----- TRANSPARENT LOOP as in RenderTransparentObjects, but changed a bit -----
			for(int i = 0; i < list.Count; i++) {
				RenderablePass rp = (RenderablePass)list[i];

				// only render this pass if it's being forced to cast shadows
				if (rp.pass.Parent.Parent.TransparencyCastsShadows) {
					SetPass(rp.pass);
					RenderSingleObject(rp.renderable, rp.pass, doLightIteration, manualLightList);
				}
			}
		}

		private static TimingMeter renderGroupMeter = MeterManager.GetMeter("Render Group Objects", "Render Visible");
		
		/// <summary>
		///		Sends visible objects found in <see cref="FindVisibleObjects"/> to the rendering engine.
		/// </summary>
		protected internal virtual void RenderVisibleObjects() {
			// loop through each main render group ( which is already sorted)
			for(int i = 0; i < GetRenderQueue().NumRenderQueueGroups; i++) {
				RenderQueueGroupID queueID = GetRenderQueue().GetRenderQueueGroupID(i);
				RenderQueueGroup queueGroup = GetRenderQueue().GetQueueGroupByIndex(i);

                if (queueID == RenderQueueGroupID.Main)
                {
                    renderingMainGroup = true;
                }
				bool repeatQueue = false;

				// repeat
				do { 
					if(OnRenderQueueStarted(queueID)) {
						// someone requested we skip this queue
						continue;
					}

					if(queueGroup.NumPriorityGroups > 0) {
						// render objects in all groups
						renderGroupMeter.Enter();
						RenderQueueGroupObjects(queueGroup);
						renderGroupMeter.Exit();
					}

					// true if someone requested that we repeat this queue
					repeatQueue = OnRenderQueueEnded(queueID);

				} while(repeatQueue);

                renderingMainGroup = false;
			} // for each queue group
		}

		/// <summary>
		///		Internal method for queueing the sky objects with the params as 
		///		previously set through SetSkyBox, SetSkyPlane and SetSkyDome.
		/// </summary>
		/// <param name="camera"></param>
		internal virtual void QueueSkiesForRendering(Camera camera) {
			// translate the skybox by cam position
			if(skyPlaneNode != null) {
				skyPlaneNode.Position = camera.DerivedPosition;
			}

			if(skyBoxNode != null) {
				skyBoxNode.Position = camera.DerivedPosition;
			}

			if(skyDomeNode != null) {
				skyDomeNode.Position = camera.DerivedPosition;
			}

			RenderQueueGroupID qid;

			// if the skyplane is enabled, queue up the single plane
			if(isSkyPlaneEnabled) {
				qid = isSkyPlaneDrawnFirst ? RenderQueueGroupID.SkiesEarly : RenderQueueGroupID.SkiesLate;
				GetRenderQueue().AddRenderable(skyPlaneEntity.GetSubEntity(0), 1, qid);
			}

			// if the skybox is enabled, queue up all the planes
			if(isSkyBoxEnabled) {
				qid = isSkyBoxDrawnFirst ? RenderQueueGroupID.SkiesEarly : RenderQueueGroupID.SkiesLate;

				for(int plane = 0; plane < 6; plane++) {
					GetRenderQueue().AddRenderable(skyBoxEntities[plane].GetSubEntity(0), 1, qid);
				}
			}

			// if the skydome is enabled, queue up all the planes
			if(isSkyDomeEnabled) {
				qid = isSkyDomeDrawnFirst ? RenderQueueGroupID.SkiesEarly : RenderQueueGroupID.SkiesLate;

				for(int plane = 0; plane < 5; ++plane) {
					GetRenderQueue().AddRenderable(skyDomeEntities[plane].GetSubEntity(0), 1, qid);
				}
			}
		}

		/// <summary>
		///		Populate a light list with an ordered set of the lights which are closest 
		/// </summary>
		/// <remarks>
		///		<p>
		///		Note that since directional lights have no position, they are always considered
		///		closer than any point lights and as such will always take precedence.
		///		</p>
		///		<p>
		///		Subclasses of the default SceneManager may wish to take into account other issues
		///		such as possible visibility of the light if that information is included in their
		///		data structures. This basic scenemanager simply orders by distance, eliminating
		///		those lights which are out of range.
		///		</p>
		///		<p>
		///		The number of items in the list max exceed the maximum number of lights supported
		///		by the renderer, but the extraneous ones will never be used. In fact the limit will
		///		be imposed by Pass::getMaxSimultaneousLights.
		///		</p>
		/// </remarks>
		/// <param name="position">The position at which to evaluate the list of lights</param>
		/// <param name="radius">The bounding radius to test</param>
		/// <param name="destList">List to be populated with ordered set of lights; will be cleared by this method before population.</param>
        protected internal virtual void PopulateLightList(Vector3 position, float radius, List<Light> destList) {
			// Really basic trawl of the lights, then sort
			// Subclasses could do something smarter
			destList.Clear();
			float squaredRadius = radius * radius;

			// loop through the scene lights an add ones in range
            ICollection<MovableObject> lightList = GetMovableObjectCollection("Light");
			foreach (Light light in lightList) {
				if(light.IsVisible) {
					if(light.Type == LightType.Directional) {
						// no distance
						light.tempSquaredDist = 0.0f;
						destList.Add(light);
					}
					else {
						light.tempSquaredDist = (light.DerivedPosition - position).LengthSquared;
						light.tempSquaredDist -= squaredRadius;
						// only add in-range lights
						float range = light.AttenuationRange;
						if(light.tempSquaredDist <= (range * range)) {
							destList.Add(light);
						}
					}
				} // if
			} // for

			// Sort Destination light list.
			destList.Sort();
		}
            
		/// <summary>
		///		Enables / disables a 'sky plane' i.e. a plane at constant
		///		distance from the camera representing the sky.
		/// </summary>
		/// <param name="enable">True to enable the plane, false to disable it.</param>
		/// <param name="plane">Details of the plane, i.e. it's normal and it's distance from the camera.</param>
		/// <param name="materialName">The name of the material the plane will use.</param>
		/// <param name="scale">The scaling applied to the sky plane - higher values mean a bigger sky plane.</param>
		/// <param name="tiling">How many times to tile the texture across the sky.</param>
		/// <param name="drawFirst">
		///		If true, the plane is drawn before all other geometry in the scene, without updating the depth buffer.
		///		This is the safest rendering method since all other objects
		///		will always appear in front of the sky. However this is not
		///		the most efficient way if most of the sky is often occluded
		///		by other objects. If this is the case, you can set this
		///		parameter to false meaning it draws <em>after</em> all other
		///		geometry which can be an optimisation - however you must
		///		ensure that the plane.d value is large enough that no objects
		///		will 'poke through' the sky plane when it is rendered.
		///	 </param>
		/// <param name="bow">
		///		If above zero, the plane will be curved, allowing
		///		the sky to appear below camera level.  Curved sky planes are 
		///		simular to skydomes, but are more compatable with fog.
		/// </param>
		public virtual void SetSkyPlane(bool enable, Plane plane, string materialName, float scale, float tiling, bool drawFirst, float bow) {
			isSkyPlaneEnabled = enable;

			if(enable) {
				string meshName = "SkyPlane";
				skyPlane = plane;

				Material m = MaterialManager.Instance.GetByName(materialName);

				if(m == null)
					throw new AxiomException(string.Format("Skyplane material '{0}' not found.", materialName));

				// make sure the material doesn't update the depth buffer
				m.DepthWrite = false;
				m.Load();

				isSkyPlaneDrawnFirst = drawFirst;

				// set up the place
				Mesh planeMesh = MeshManager.Instance.GetByName(meshName);

				// unload the old one if it exists
				if(planeMesh != null)
					MeshManager.Instance.Unload(planeMesh);

				// create up vector
				Vector3 up = plane.Normal.Cross(Vector3.UnitX);
				if(up == Vector3.Zero)
					up = plane.Normal.Cross(-Vector3.UnitZ);

				if(bow > 0) {
					planeMesh = MeshManager.Instance.CreateCurvedIllusionPlane(
						meshName, 
						plane, 
						scale * 100, 
						scale * 100, 
						scale * bow * 100,
						6, 6, false, 1, tiling, tiling, up);
				}
				else {
					planeMesh = MeshManager.Instance.CreatePlane(meshName, plane, scale * 100, scale * 100, 1, 1, false, 1, tiling, tiling, up);
				}

				if(skyPlaneEntity != null) {
					ExtractMovableObject(skyPlaneEntity);
				}

				// create entity for the plane, using the mesh name
				skyPlaneEntity = CreateEntity(meshName, meshName);
				skyPlaneEntity.MaterialName = materialName;
				// sky entities need not cast shadows
				skyPlaneEntity.CastShadows = false;

				if(skyPlaneNode == null)
					skyPlaneNode = CreateSceneNode(meshName + "Node");
				else
					skyPlaneNode.DetachAllObjects();

				// attach the skyplane to the new node
				skyPlaneNode.AttachObject(skyPlaneEntity);
			}
		}

		/// <summary>
		///		Overload.
		/// </summary>
		/// <param name="enable"></param>
		/// <param name="plane"></param>
		public virtual void SetSkyPlane(bool enable, Plane plane, string materialName) {
			// call the overloaded method
			SetSkyPlane(enable, plane, materialName, 1000.0f, 10.0f, true, 0);
		}

		/// <summary>
		///		Internal method for notifying the manager that a SceneNode is autotracking.
		/// </summary>
		/// <param name="node">Scene node that is auto tracking another scene node.</param>
		/// <param name="autoTrack">True if tracking, false if it is stopping tracking.</param>
		internal void NotifyAutoTrackingSceneNode(SceneNode node, bool autoTrack) {
			if(autoTrack) {
				autoTrackingSceneNodes.Add(node);
			}
			else {
				autoTrackingSceneNodes.Remove(node);
			}
		}

		/// <summary>
		/// If set, only the selected node is rendered.
		/// To render all nodes, set to null.
		/// </summary>
		/// 
		public void OverrideRootSceneNode(SceneNode node) {
			rootSceneNode = node;
		}
		 
		public void RestoreRootSceneNode() {
			rootSceneNode = defaultRootNode;
		}

   		/// <summary>
		///     Render something as if it came from the current queue.
		/// </summary>
		///<param name="pass">Material pass to use for setting up this quad.</param>
		///<param name="rend">Renderable to render</param>
		///<param name="shadowDerivation">Whether passes should be replaced with shadow caster / receiver passes</param>
		public virtual void InjectRenderWithPass(Pass pass, IRenderable rend, bool shadowDerivation) {
			// render something as if it came from the current queue
			Pass usedPass = SetPass(pass, false, shadowDerivation);
			RenderSingleObject(rend, usedPass, false);
		}

		public virtual void InjectRenderWithPass(Pass pass, IRenderable rend) {
			InjectRenderWithPass(pass, rend, true);
		}

   		/// <summary>
		///     Creates a StaticGeometry instance suitable for use with this
        ///     SceneManager.
		/// </summary>
		/// <remarks>
        ///     StaticGeometry is a way of batching up geometry into a more 
        ///     efficient form at the expense of being able to move it. Please 
        ///     read the StaticGeometry class documentation for full information.
		/// </remarks>
        ///<param name="name">The name to give the new object</param>
        ///<returns>The new StaticGeometry instance</returns>
        public StaticGeometry CreateStaticGeometry(string name) {
            // Check not existing
            if (staticGeometryList.ContainsKey(name))
                throw new Exception("StaticGeometry with name '" + name + "' already exists!  In " +
                    "SceneManager.CreateStaticGeometry");
            StaticGeometry geometry = new StaticGeometry(this, name);
            staticGeometryList[name] = geometry;
            return geometry;
        }
   		/// <summary>
		///     Retrieve a previously created StaticGeometry instance. 
		/// </summary>
   		/// <note>
		///     Throws an exception if the named instance does not exist
		/// </note>
        public StaticGeometry GetStaticGeometry(string name) {
            StaticGeometry geometry;
            if (!staticGeometryList.TryGetValue(name, out geometry))
                throw new Exception("StaticGeometry with name '" + name + "' not found!  In " +
                    "SceneManager.CreateStaticGeometry");
            else
                return geometry;
        }

   		/// <summary>
		///     Returns whether a static geometry instance with the given name exists. */
		/// </summary>
        public bool HasStaticGeometry(string name) {
            return staticGeometryList.ContainsKey(name);
        }

   		/// <summary>
		///     Remove & destroy a StaticGeometry instance.
		/// </summary>
        public void DestroyStaticGeometry(StaticGeometry geom) {
            DestroyStaticGeometry(geom.Name);
        }

   		/// <summary>
		///     Remove & destroy a StaticGeometry instance.
		/// </summary>
        public void DestroyStaticGeometry(string name) {
            StaticGeometry geometry;
            if (!staticGeometryList.TryGetValue(name, out geometry))
                throw new Exception("StaticGeometry with name '" + name + "' not found!  In " +
                    "SceneManager.DestroyStaticGeometry");
            else {
                staticGeometryList.Remove(name);
                geometry.Destroy();
            }
        }

   		/// <summary>
		///     Destroy all StaticGeometry instances.
		/// </summary>
        public void DestroyAllStaticGeometry() {
            foreach(StaticGeometry geometry in staticGeometryList.Values)
                geometry.Destroy();
            staticGeometryList.Clear();
        }

   		/// <summary>
		///     Returns a visibility boundary box for a specific camera.
		/// </summary>
        public VisibleObjectsBoundsInfo GetVisibleObjectsBoundsInfo(Camera camera) {
            VisibleObjectsBoundsInfo bounds;
            if (camVisibleObjectsMap.TryGetValue(camera, out bounds))
                return bounds;
            else
                return new VisibleObjectsBoundsInfo();
        }

   		/// <summary>
		///     Returns the shadow caster AAB for a specific light-camera combination
		/// </summary>
        public VisibleObjectsBoundsInfo GetShadowCasterBoundsInfo(Light light) {
            foreach(KeyValuePair<Camera, Light> pair in shadowCamLightMapping) {
                if (pair.Value == light) 
                    return GetVisibleObjectsBoundsInfo(pair.Key);
            }
            return new VisibleObjectsBoundsInfo();
        }

		#endregion

		#region ShadowCasterSceneQueryListener Class
		/// <summary>
		///		Nested class to use as a callback for shadow caster scene query.
		/// </summary>
		protected class ShadowCasterSceneQueryListener : ISceneQueryListener {
			#region Fields

			protected ArrayList casterList = new ArrayList();
			protected bool isLightInFrustum;
			protected List<PlaneBoundedVolume> lightClipVolumeList = new List<PlaneBoundedVolume>();
			protected Camera camera;
			protected Light light;
			protected float farDistSquared;

			#endregion Fields

			#region Methods

			/// <summary>
			///		Prepare the listener for use with a set of parameters.
			/// </summary>
			/// <param name="lightInFrustum"></param>
			/// <param name="lightClipVolumes"></param>
			/// <param name="light"></param>
			/// <param name="camera"></param>
			/// <param name="shadowCasterList"></param>
			/// <param name="farDistSquared"></param>
			public void Prepare(bool lightInFrustum, List<PlaneBoundedVolume> lightClipVolumes, Light light, 
				Camera camera, ArrayList shadowCasterList, float farDistSquared) {

				this.casterList = shadowCasterList;
				this.isLightInFrustum = lightInFrustum;
				this.lightClipVolumeList = lightClipVolumes;
				this.camera = camera;
				this.light = light;
				this.farDistSquared = farDistSquared;
			}

			#endregion Methods

			#region ISceneQueryListener Members

			public bool OnQueryResult(MovableObject sceneObject) {
				if (sceneObject.CastShadows && sceneObject.IsVisible) {
					if (farDistSquared > 0) {
						// Check object is within the shadow far distance
						Vector3 toObj = sceneObject.ParentNode.DerivedPosition - camera.DerivedPosition;
						float radius = sceneObject.GetWorldBoundingSphere().Radius;
						float dist = toObj.LengthSquared;

						if (dist - (radius * radius) > farDistSquared) {
							// skip, beyond max range
							return true;
						}
					}

					// If the object is in the frustum, we can always see the shadow
					if (camera.IsObjectVisible(sceneObject.GetWorldBoundingBox())) {
						casterList.Add(sceneObject);
						return true;
					}

					// Otherwise, object can only be casting a shadow into our view if
					// the light is outside the frustum (or it's a directional light, 
					// which are always outside), and the object is intersecting
					// on of the volumes formed between the edges of the frustum and the
					// light
					if (!isLightInFrustum || light.Type == LightType.Directional) {
						// Iterate over volumes
						for (int i = 0; i < lightClipVolumeList.Count; i++) {
							PlaneBoundedVolume pbv = (PlaneBoundedVolume)lightClipVolumeList[i];

							if (pbv.Intersects(sceneObject.GetWorldBoundingBox())) {
								casterList.Add(sceneObject);
								return true;
							}
						}
					}
				}

				return true;
			}

			public bool OnQueryResult(SceneQuery.WorldFragment fragment) {
				// don't deal with world fragments by default
				return true;
			}

			#endregion

		}

		#endregion
	}

	public class VisibleObjectsBoundsInfo
	{
		/// The axis-aligned bounds of the visible objects
		public AxisAlignedBox aabb;
		/// The closest a visible object is to the camera
		public float minDistance;
		/// The farthest a visible objects is from the camera
		public float maxDistance;

		public VisibleObjectsBoundsInfo() {
            Reset();
		}

		public void Reset() {
			aabb = AxisAlignedBox.Null;
			minDistance = float.MaxValue;
			maxDistance = 0;
		}

		public void Merge(AxisAlignedBox boxBounds, Sphere sphereBounds, Camera cam) {
			aabb.Merge(boxBounds);
			float camDistToCenter = (cam.DerivedPosition - sphereBounds.Center).Length;
			minDistance = Math.Min(minDistance, Math.Max(0f, camDistToCenter - sphereBounds.Radius));
			maxDistance = Math.Max(maxDistance, camDistToCenter + sphereBounds.Radius);
		}
	}


	#region Default SceneQuery Implementations


	/// <summary>
	///		Default implementation of a AxisAlignedBoxRegionSceneQuery.
	/// </summary>
	public class DefaultAxisAlignedBoxRegionSceneQuery : AxisAlignedBoxRegionSceneQuery {
		internal protected DefaultAxisAlignedBoxRegionSceneQuery(SceneManager creator) : base(creator) {}

		public override void Execute(ISceneQueryListener listener) {
			// TODO: BillboardSets? Will need per-billboard collision most likely
			// Entities only for now
            foreach (Dictionary<string, MovableObject> objectMap in creator.MovableObjectMaps) {
                foreach (MovableObject obj in objectMap.Values) {
                    // skip if unattached or filtered out by query flags
                    if (!obj.IsAttached || (obj.QueryFlags & queryMask) == 0) {
                        continue;
                    }

                    if (box.Intersects(obj.GetWorldBoundingBox())) {
                        listener.OnQueryResult(obj);
                    }
                }
            }
		}
	}

	/// <summary>
	///    Default implementation of RaySceneQuery.
	/// </summary>
	public class DefaultRaySceneQuery : RaySceneQuery {
		internal protected DefaultRaySceneQuery(SceneManager creator) : base(creator) {}

		public override void Execute(IRaySceneQueryListener listener) {
			// Note that becuase we have no scene partitioning, we actually
			// perform a complete scene search even if restricted results are
			// requested; smarter scene manager queries can utilise the paritioning 
			// of the scene in order to reduce the number of intersection tests 
			// required to fulfil the query

			// TODO: BillboardSets? Will need per-billboard collision most likely
			// Entities only for now
            foreach (Dictionary<string, MovableObject> objectMap in creator.MovableObjectMaps) {
                foreach (MovableObject obj in objectMap.Values) {
                    // skip if unattached or filtered out by query flags
                    if (!obj.IsAttached || (obj.QueryFlags & queryMask) == 0) {
                        continue;
                    }

                    // test the intersection against the world bounding box of the entity
                    IntersectResult results = MathUtil.Intersects(ray, obj.GetWorldBoundingBox());

                    // if the results came back positive, fire the event handler
                    if (results.Hit == true) {
                        listener.OnQueryResult(obj, results.Distance);
                    }
                }
            }
		}
	}

	/// <summary>
	///		Default implementation of a SphereRegionSceneQuery.
	/// </summary>
	public class DefaultSphereRegionSceneQuery : SphereRegionSceneQuery {
		internal protected DefaultSphereRegionSceneQuery(SceneManager creator) : base(creator) {}

		public override void Execute(ISceneQueryListener listener) {
			// TODO: BillboardSets? Will need per-billboard collision most likely
			// Entities only for now
			Sphere testSphere = new Sphere();

            foreach (Dictionary<string, MovableObject> objectMap in creator.MovableObjectMaps) {
                foreach (MovableObject obj in objectMap.Values) {
                    // skip if unattached or filtered out by query flags
                    if (!obj.IsAttached || (obj.QueryFlags & queryMask) == 0) {
                        continue;
                    }

                    testSphere.Center = obj.ParentNode.DerivedPosition;
                    testSphere.Radius = obj.BoundingRadius;

                    // if the results came back positive, fire the event handler
                    if (sphere.Intersects(testSphere)) {
                        listener.OnQueryResult(obj);
                    }
                }
            }
		}
	}

	/// <summary>
	///		Default implementation of a PlaneBoundedVolumeListSceneQuery.
	/// </summary>
	public class DefaultPlaneBoundedVolumeListSceneQuery : PlaneBoundedVolumeListSceneQuery {
		internal protected DefaultPlaneBoundedVolumeListSceneQuery(SceneManager creator) : base(creator) {}

		public override void Execute(ISceneQueryListener listener) {
            foreach (Dictionary<string, MovableObject> objectMap in creator.MovableObjectMaps) {
                foreach (MovableObject obj in objectMap.Values) {
                    // skip if unattached or filtered out by query flags
                    if (!obj.IsAttached || (obj.QueryFlags & queryMask) == 0) {
                        continue;
                    }

                    for (int v = 0; v < volumes.Count; v++) {
                        PlaneBoundedVolume volume = (PlaneBoundedVolume)volumes[v];
                        // Do AABB / plane volume test
                        if ((obj.QueryFlags & queryMask) != 0 && volume.Intersects(obj.GetWorldBoundingBox())) {
                            listener.OnQueryResult(obj);
                            break;
                        }
                    }
                }
            }
		}
	}

	/// <summary>
	///    Default implementation of IntersectionSceneQuery.
	/// </summary>
	public class DefaultIntersectionSceneQuery : IntersectionSceneQuery {
		internal protected DefaultIntersectionSceneQuery(SceneManager creator) : base(creator) {
			// No world geometry results supported
			this.AddWorldFragmentType(WorldFragmentType.None);
		}

		public override void Execute(IIntersectionSceneQueryListener listener) {
            foreach (Dictionary<string, MovableObject> objectMap in creator.MovableObjectMaps) {
                ICollection<MovableObject> movableObjects = objectMap.Values;
                int numEntries = movableObjects.Count;
                MovableObject[] movableObjectArray = new MovableObject[numEntries];
                movableObjects.CopyTo(movableObjectArray, 0);
                for (int a = 0; a < (numEntries - 1); a++) {
                    MovableObject aobj = movableObjectArray[a];

                    // skip if unattached or filtered out by query flags
                    if (!aobj.IsAttached || (aobj.QueryFlags & queryMask) == 0) {
                        continue;
                    }

                    // Loop b from a+1 to last
                    int b = a;
                    for (++b; b != (numEntries - 1); ++b) {
                        MovableObject bobj = movableObjectArray[b];

                        // skip if unattached or filtered out by query flags
                        if (!bobj.IsAttached || (bobj.QueryFlags & queryMask) == 0) {
                            continue;
                        }

                        // Apply mask to b (both must pass)
                        if ((bobj.QueryFlags & this.queryMask) != 0) {
                            AxisAlignedBox box1 = aobj.GetWorldBoundingBox();
                            AxisAlignedBox box2 = bobj.GetWorldBoundingBox();

                            if (box1.Intersects(box2)) {
                                listener.OnQueryResult(aobj, bobj);
                            }
                        }

                    }
                }
            }
		}
	}

	#endregion Default SceneQuery Implementations

	/// <summary>
	///     Structure for holding a position & orientation pair.
	/// </summary>
	public struct ViewPoint {
		public Vector3 position;
		public Quaternion orientation;
	}
}

