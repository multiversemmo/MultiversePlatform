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
using System.Data;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Collections;
using Axiom.MathLib.Collections;

namespace Axiom.SceneManagers.Bsp
{
 	/// <summary>
	///		Specialization of the SceneManager class to deal with indoor scenes based on a BSP tree.
	///	</summary>
	///	<remarks>
	///		This class refines the behaviour of the default SceneManager to manage
	///		a scene whose bulk of geometry is made up of an indoor environment which
	///		is organised by a Binary Space Partition (BSP) tree. 
	///		<p/>
	///		A BSP tree progressively subdivides the space using planes which are the nodes of the tree.
	///		At some point we stop subdividing and everything in the remaining space is part of a 'leaf' which
	///		contains a number of polygons. Typically we traverse the tree to locate the leaf in which a
	///		point in space is (say the camera origin) and work from there. A second structure, the
	///		Potentially Visible Set, tells us which other leaves can been seen from this
	///		leaf, and we test their bounding boxes against the camera frustum to see which
	///		we need to draw. Leaves are also a good place to start for collision detection since
	///		they divide the level into discrete areas for testing.
	///		<p/>
	///		This BSP and PVS technique has been made famous by engines such as Quake and Unreal. Ogre
	///		provides support for loading Quake3 level files to populate your world through this class,
	///		by calling the BspSceneManager.LoadWorldGeometry. Note that this interface is made
	///		available at the top level of the SceneManager class so you don't have to write your code
	///		specifically for this class - just callRoot.Instance.SceneManagers.SetSceneManager(SceneType.Indoors)
	///		and in the current implementation you will get a BspSceneManager silently disguised as a
	///		standard SceneManager.
	/// </remarks>
	public class BspSceneManager : SceneManager 
	{
		#region Protected members
		protected BspLevel level;
		protected bool[] faceGroupChecked;
		protected RenderOperation renderOp = new RenderOperation();
		protected bool showNodeAABs;
		protected RenderOperation aaBGeometry = new RenderOperation();

		protected Bsp.Collections.Map matFaceGroupMap = new Bsp.Collections.Map();
		protected MovableObjectCollection objectsForRendering = new MovableObjectCollection();
		protected BspGeometry bspGeometry = new BspGeometry();
		protected SpotlightFrustum spotlightFrustum;
		protected Material textureLightMaterial;
		protected Pass textureLightPass;
		protected bool[] lightAddedToFrustum;
		#endregion

		#region Public properties
		public BspLevel Level
		{
			get { return level; }
		}

		public bool ShowNodeBoxes
		{
			get { return showNodeAABs; }
			set { showNodeAABs = value; }
		}
		#endregion

		#region Constructor
		public BspSceneManager()
		{
			// Set features for debugging render
			showNodeAABs = false;

			// No sky by default
			isSkyPlaneEnabled = false;
			isSkyBoxEnabled = false;
			isSkyDomeEnabled = false;

			level = null;
		}
		#endregion

		#region Public methods
		/// <summary>
		///		Specialised from SceneManager to support Quake3 bsp files.
		/// </summary>
		public override void LoadWorldGeometry(string filename)
		{
			if(Path.GetExtension(filename).ToLower() == ".xml")
			{
				DataSet optionData = new DataSet();
				optionData.ReadXml(filename);

				DataTable table = optionData.Tables[0];
				DataRow row = table.Rows[0];

				if(table.Columns["Map"] != null) 
				{
					optionList["Map"] = (string)row["Map"];
				}

				if(table.Columns["SetYAxisUp"] != null) 
				{
					optionList["SetYAxisUp"] = (string.Compare((string)row["SetYAxisUp"], "yes", true)) == 0 ? true : false;
				}

				if(table.Columns["Scale"] != null) 
				{
                    optionList["Scale"] = StringConverter.ParseFloat((string)row["Scale"]);
                }

				Vector3 move = Vector3.Zero;

				if(table.Columns["MoveX"] != null) 
				{
                    move.x = StringConverter.ParseFloat((string)row["MoveX"]);
                }

				if(table.Columns["MoveY"] != null) 
				{
                    move.y = StringConverter.ParseFloat((string)row["MoveY"]);
                }

				if(table.Columns["MoveZ"] != null) 
				{
                    move.z = StringConverter.ParseFloat((string)row["MoveZ"]);
                }

				optionList["Move"] = move;

				if(table.Columns["UseLightmaps"] != null) 
				{
					optionList["UseLightmaps"] = (string.Compare((string)row["UseLightmaps"], "yes", true)) == 0 ? true : false;
				}
				
				if(table.Columns["AmbientEnabled"] != null) 
				{
					optionList["AmbientEnabled"] = (string.Compare((string)row["AmbientEnabled"], "yes", true)) == 0 ? true : false;
				}
				
				if(table.Columns["AmbientRatio"] != null) 
				{
                    optionList["AmbientRatio"] = StringConverter.ParseFloat((string)row["AmbientRatio"]);
                }
			}
			else
			{
				optionList["Map"] = filename;
			}

			LoadWorldGeometry();
		}

		public void LoadWorldGeometry()
		{
			if (!optionList.ContainsKey("Map"))
				throw new AxiomException("Unable to load world geometry. \"Map\" filename option is not set.");

			if(Path.GetExtension(((string)optionList["Map"]).ToLower()) != ".bsp")
				throw new AxiomException("Unable to load world geometry. Invalid extension of map filename option (must be .bsp).");

			if (!optionList.ContainsKey("SetYAxisUp"))
				optionList["SetYAxisUp"] = false;

			if (!optionList.ContainsKey("Scale"))
				optionList["Scale"] = 1f;

			if (!optionList.ContainsKey("Move"))
				optionList["Move"] = Vector3.Zero;
			
			if (!optionList.ContainsKey("UseLightmaps"))
				optionList["UseLightmaps"] = true;
			
			if (!optionList.ContainsKey("AmbientEnabled"))
				optionList["AmbientEnabled"] = false;
			
			if (!optionList.ContainsKey("AmbientRatio"))
				optionList["AmbientRatio"] = 1f;

			InitTextureLighting();

			if (spotlightFrustum == null)
				spotlightFrustum = new SpotlightFrustum();

			// Load using resource manager
			level = BspResourceManager.Instance.Load((string)optionList["Map"]);

			// Init static render operation
			renderOp.vertexData = level.VertexData;
			
			// index data is per-frame
			renderOp.indexData = new IndexData();
			renderOp.indexData.indexStart = 0;
			renderOp.indexData.indexCount = 0;

			// Create enough index space to render whole level
			renderOp.indexData.indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
				IndexType.Size32,
				level.NumIndexes,
				BufferUsage.Dynamic, false
				);
			renderOp.operationType = OperationType.TriangleList;
			renderOp.useIndices = true;
		}

		/// <summary>
		///		Specialised to suggest viewpoints.
		/// </summary>
		public override ViewPoint GetSuggestedViewpoint(bool random)
		{
			if((level == null) || (level.PlayerStarts.Length == 0))
			{
				return base.GetSuggestedViewpoint(random);
			}
			else
			{
				if(random)
					return level.PlayerStarts[(int) (MathUtil.UnitRandom() * level.PlayerStarts.Length)];
				else
					return level.PlayerStarts[0];

			}
		}

		/// <summary>
		///		Overriden from SceneManager.
		/// </summary>
		public override void FindVisibleObjects(Camera camera, bool onlyShadowCasters)
		{
			if (!onlyShadowCasters)
			{
				// Add this renderable to the RenderQueue so that the BspSceneManager gets
				// notified when the geometry needs rendering and with what lights
				GetRenderQueue().AddRenderable(bspGeometry);
			}

			// Clear unique list of movables for this frame
			objectsForRendering.Clear();

			// Walk the tree, tag static geometry, return camera's node (for info only)
			// Movables are now added to the render queue in processVisibleLeaf
			BspNode cameraNode = WalkTree(camera, onlyShadowCasters);
		}

		/// <summary>
		///		Overriden from SceneManager.
		/// </summary>
		/// <param name="position">The position at which to evaluate the list of lights</param>
		/// <param name="radius">The bounding radius to test</param>
		/// <param name="destList">List to be populated with ordered set of lights; will be cleared by this method before population.</param>
		protected override void PopulateLightList(Vector3 position, float radius, LightList destList) 
		{
			BspNode positionNode = level.FindLeaf(position);
			BspNode[] lightNodes = new BspNode[lightList.Count];

			for (int i = 0; i < lightList.Count; i++)
			{
				Light light = lightList[i];
				lightNodes[i] = (BspNode) level.objectToNodeMap.FindFirst(light);
			}

			// Trawl of the lights that are visible from position, then sort
			destList.Clear();
			float squaredRadius = radius * radius;

			// loop through the scene lights an add ones in range and visible from positionNode
			for(int i = 0; i < lightList.Count; i++) 
			{
				TextureLight light = (TextureLight) lightList[i];

				if(light.IsVisible && level.IsLeafVisible(positionNode, lightNodes[i]))
				{
					if(light.Type == LightType.Directional) 
					{
						// no distance
						light.TempSquaredDist = 0.0f;
						destList.Add(light);
					}
					else 
					{
						light.TempSquaredDist = (light.DerivedPosition - position).LengthSquared;
						light.TempSquaredDist -= squaredRadius;
						// only add in-range lights
						float range = light.AttenuationRange;
						if(light.TempSquaredDist <= (range * range)) 
						{
							destList.Add(light);
						}
					}
				} // if
			} // for

			// Sort Destination light list.
			// TODO: Not needed yet since the current LightList is a sorted list under the hood already
			//destList.Sort();
		}

		/// <summary>
		///		Overriden from SceneManager.
		/// </summary>
		protected override void FindLightsAffectingFrustum(Camera camera) 
		{
			lightsAffectingFrustum.Clear();
			lightAddedToFrustum = new bool[lightList.Count];

			if (shadowTechnique == ShadowTechnique.TextureModulative)
			{
				// we must provide the list of lights now, not at WalkTree

				// CHECK: The code here finds the lights faster than at WalkTree
				// but not as accurate. The case where the node of a light is not
				// visible, but the nodes that the light affects are, is not taken
				// into account.

				// Locate the leaf node where the camera is located
				BspNode cameraNode = level.FindLeaf(camera.DerivedPosition);

				for (int i=0; i < lightList.Count; i++)
				{
					TextureLight light = (TextureLight) lightList[i];

					if (!light.IsVisible)
						continue;

					// This is set so that the lights are rendered with ascending
					// Priority order.
					light.TempSquaredDist = light.Priority;

					BspNode lightNode = (BspNode) level.objectToNodeMap.FindFirst(light);
					if (level.IsLeafVisible(cameraNode, lightNode))
					{
						lightsAffectingFrustum.Add(light);
						lightAddedToFrustum[i] = true;
					}
				}
			}

			// Lights are added to the lightsAffectingFrustum in WalkTree for other
			// shadow techniques
		}

		protected override IList FindShadowCastersForLight(Light light, Camera camera)
		{
			// objectsForRendering was filled at ProcessVisibleLeaf which is called
			// during FindVisibleObjects

			IList casters = base.FindShadowCastersForLight (light, camera);

			for (int i = 0; i < casters.Count; i++)
			{
				if(!objectsForRendering.ContainsKey(((MovableObject)casters[i]).Name))
				{
					// this shadow caster is not visible, remove it
					casters.RemoveAt(i);
                    i--;
				}
			}

			return casters;
		}


		/// <summary>
		///		Creates a specialized <see cref="Plugin_BSPSceneManager.BspSceneNode"/>.
		/// </summary>
		public override SceneNode CreateSceneNode()
		{
			BspSceneNode node = new BspSceneNode(this);
			this.sceneNodeList[node.Name] = node;

			return node;
		}

		/// <summary>
		///		Creates a specialized <see cref="Plugin_BSPSceneManager.BspSceneNode"/>.
		/// </summary>
		public override SceneNode CreateSceneNode(string name)
		{
			BspSceneNode node = new BspSceneNode(this, name);
			this.sceneNodeList[node.Name] = node;

			return node;
		}

		/// <summary>
		///		Creates a specialised texture light.
		/// </summary>
		public override Light CreateLight(string name)
		{
			// create a new texture light and add it to our internal list
			TextureLight light = new TextureLight(name, this);
			
			// add the light to the list
			lightList.Add(name, light);

			// add it in the bsp tree
			NotifyObjectMoved(light, light.Position);

			return light;
		}

		public override void RemoveLight(Light light)
		{
			NotifyObjectDetached(light);
			base.RemoveLight (light);
		}

		public override void RemoveAllLights()
		{
			for (int i = 0; i < lightList.Count; i++)
				NotifyObjectDetached(lightList[i]);

			base.RemoveAllLights ();
		}

		public override void RemoveEntity(Entity entity)
		{
			NotifyObjectDetached(entity);
			base.RemoveEntity (entity);
		}

		public override void RemoveEntity(string name)
		{
			Entity entity = entityList[name];
			if(entity != null) 
			{
				this.RemoveEntity(entity);
			}
		}

		public override void RemoveAllEntities()
		{
			for (int i = 0; i < entityList.Count; i++)
				NotifyObjectDetached(entityList[i]);

			base.RemoveAllEntities ();
		}

		/// <summary>
		///		Internal method for tagging <see cref="Plugin_BSPSceneManager.BspNode"/'s with objects which intersect them.
		/// </summary>
		internal void NotifyObjectMoved(MovableObject obj, Vector3 pos)
		{
			level.NotifyObjectMoved(obj, pos);
		}

		/// <summary>
		///		Internal method for notifying the level that an object has been detached from a node.
		/// </summary>
		internal void NotifyObjectDetached(MovableObject obj)
		{
			level.NotifyObjectDetached(obj);
		}

		// TODO: Scene queries.
		/// <summary>
		///		Creates an AxisAlignedBoxSceneQuery for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		for an axis aligned box region. See SceneQuery and AxisAlignedBoxSceneQuery 
		///		for full details.
		///		<p/>
		///		The instance returned from this method must be destroyed by calling
		///		SceneManager.DestroyQuery when it is no longer required.
		/// </remarks>
		/// <param name="box">Details of the box which describes the region for this query.</param>
		/*public virtual AxisAlignedBoxSceneQuery CreateAABBQuery(AxisAlignedBox box)
		{
			return CreateAABBQuery(box, 0xFFFFFFFF);
		}

		/// <summary>
		///		Creates an AxisAlignedBoxSceneQuery for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		for an axis aligned box region. See SceneQuery and AxisAlignedBoxSceneQuery 
		///		for full details.
		///		<p/>
		///		The instance returned from this method must be destroyed by calling
		///		SceneManager.DestroyQuery when it is no longer required.
		/// </remarks>
		/// <param name="box">Details of the box which describes the region for this query.</param>
		/// <param name="mask">The query mask to apply to this query; can be used to filter out certain objects; see SceneQuery for details.</param>
		public virtual AxisAlignedBoxSceneQuery CreateAABBQuery(AxisAlignedBox box, ulong mask)
		{
			// TODO:
			return null;
		}*/

		/// <summary>
		///		Creates a SphereSceneQuery for this scene manager. 
		/// </summary>
		/// <remarks>
		/// 	This method creates a new instance of a query object for this scene manager, 
		///		for a spherical region. See SceneQuery and SphereSceneQuery 
		///		for full details.
		/// </remarks>
		/// <param name="sphere">Details of the sphere which describes the region for this query.</param>
		/// <param name="mask">The query mask to apply to this query; can be used to filter out	certain objects; see SceneQuery for details.</param>
		public override SphereRegionSceneQuery CreateSphereRegionQuery(Sphere sphere, ulong mask)
		{
			BspSphereRegionSceneQuery q = new BspSphereRegionSceneQuery(this);
			q.Sphere = sphere;
			q.QueryMask = mask;

			return q;
		}

		/// <summary>
		///		Creates a RaySceneQuery for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for this scene manager, 
		///		looking for objects which fall along a ray. See SceneQuery and RaySceneQuery 
		///		for full details.
		/// </remarks>
		/// <param name="ray">Details of the ray which describes the region for this query.</param>
		/// <param name="mask">The query mask to apply to this query; can be used to filter out certain objects; see SceneQuery for details.</param>
		public override RaySceneQuery CreateRayQuery(Ray ray, ulong mask)
		{
			BspRaySceneQuery q = new BspRaySceneQuery(this);
			q.Ray = ray;
			q.QueryMask = mask;

			return q;
		}

		/// <summary>
		///		Creates an IntersectionSceneQuery for this scene manager. 
		/// </summary>
		/// <remarks>
		///		This method creates a new instance of a query object for locating
		///		intersecting objects. See SceneQuery and IntersectionSceneQuery
		///		for full details.
		/// </remarks>
		/// <param name="mask">The query mask to apply to this query; can be used to filter out certain objects; see SceneQuery for details.</param>
		public override IntersectionSceneQuery CreateIntersectionQuery(ulong mask)
		{
			BspIntersectionSceneQuery q = new BspIntersectionSceneQuery(this);
			q.QueryMask = mask;

			return q;
		}
		#endregion

		#region Protected methods

		protected void InitTextureLighting()
		{
			Trace.WriteLineIf(targetRenderSystem.Caps.TextureUnitCount < 2, "--WARNING--At least 2 available texture units are required for BSP dynamic lighting!");

			Texture texLight = TextureLight.CreateTexture();

			textureLightMaterial = MaterialManager.Instance.GetByName("Axiom/BspTextureLightMaterial");
			if (textureLightMaterial == null)
			{
				textureLightMaterial = (Material) MaterialManager.Instance.Create("Axiom/BspTextureLightMaterial");
				textureLightPass = textureLightMaterial.GetTechnique(0).GetPass(0);
				// the texture light
				TextureUnitState tex = textureLightPass.CreateTextureUnitState(texLight.Name);
				tex.SetColorOperation(LayerBlendOperation.Modulate);
				tex.ColorBlendMode.source2 = LayerBlendSource.Diffuse;
				tex.SetAlphaOperation(LayerBlendOperationEx.Modulate);
				tex.AlphaBlendMode.source2 = LayerBlendSource.Diffuse;
				tex.TextureCoordSet = 2;
				tex.TextureAddressing = TextureAddressing.Clamp;

				// The geometry texture without lightmap. Use the light texture on this
				// pass, the appropriate texture will be rendered at RenderTextureLighting
				tex = textureLightPass.CreateTextureUnitState(texLight.Name);
				tex.SetColorOperation(LayerBlendOperation.Modulate);
				tex.SetAlphaOperation(LayerBlendOperationEx.Modulate);
				tex.TextureAddressing = TextureAddressing.Wrap;

				textureLightPass.SetSceneBlending(SceneBlendType.TransparentAlpha);

				textureLightMaterial.CullingMode = CullingMode.None;
				textureLightMaterial.Lighting = false;
			}
			else
			{
				textureLightPass = textureLightMaterial.GetTechnique(0).GetPass(0);
			}
		}

		/// <summary>
		///		Walks the BSP tree looking for the node which the camera is in, and tags any geometry 
		///		which is in a visible leaf for later processing.
		/// </summary>
		protected BspNode WalkTree(Camera camera, bool onlyShadowCasters)
		{
			// Locate the leaf node where the camera is located
			BspNode cameraNode = level.FindLeaf(camera.DerivedPosition);

			matFaceGroupMap.Clear();
			faceGroupChecked = new bool[level.FaceGroups.Length];

			TextureLight[] lights = new TextureLight[lightList.Count];
			BspNode[] lightNodes = new BspNode[lightList.Count];
			Sphere[] lightSpheres = new Sphere[lightList.Count];

			// The base SceneManager uses this for shadows.
			// The BspSceneManager uses this for texture lighting as well.
			if (shadowTechnique == ShadowTechnique.None)
			{
				lightsAffectingFrustum.Clear();
				lightAddedToFrustum = new bool[lightList.Count];
			}

			for (int lp=0; lp < lightList.Count; lp++)
			{
				TextureLight light = (TextureLight) lightList[lp];
				lights[lp] = light;
				lightNodes[lp] = (BspNode) level.objectToNodeMap.FindFirst(light);
				if (light.Type != LightType.Directional)
				{
					// treating spotlight as point for simplicity
					lightSpheres[lp] = new Sphere(light.DerivedPosition, light.AttenuationRange);
				}
			}

			// Scan through all the other leaf nodes looking for visibles
			int i = level.NumNodes - level.LeafStart;
			int p = level.LeafStart;
			BspNode node;
			
			while(i-- > 0)
			{
				node = level.Nodes[p];
                
				if(level.IsLeafVisible(cameraNode, node))
				{
					// Visible according to PVS, check bounding box against frustum
					FrustumPlane plane;

					if(camera.IsObjectVisible(node.BoundingBox, out plane))
					{
						if (!onlyShadowCasters)
						{
							for (int lp=0; lp < lights.Length; lp++)
							{
								if (lightAddedToFrustum[lp] || !lights[lp].IsVisible)
									continue;

								if (level.IsLeafVisible(lightNodes[lp], node) &&
                                    (lights[lp].Type == LightType.Directional ||
									lightSpheres[lp].Intersects(node.BoundingBox)))
								{
									// This is set so that the lights are rendered with ascending
									// Priority order.
									lights[lp].TempSquaredDist = lights[lp].Priority;

									lightsAffectingFrustum.Add(lights[lp]);
									lightAddedToFrustum[lp] = true;
								}
							}
						}

						ProcessVisibleLeaf(node, camera, onlyShadowCasters);

						if(showNodeAABs)
							AddBoundingBox(node.BoundingBox, true);
					}
				}

				p++;
			}

			return cameraNode;
		}
		
		/// <summary>
		///		Tags geometry in the leaf specified for later rendering.
		/// </summary>
		protected void ProcessVisibleLeaf(BspNode leaf, Camera camera, bool onlyShadowCasters)
		{
			// Skip world geometry if we're only supposed to process shadow casters
			// World is pre-lit
			if (!onlyShadowCasters)
			{
				// Parse the leaf node's faces, add face groups to material map
				int numGroups = leaf.NumFaceGroups;
				int idx = leaf.FaceGroupStart;

				while(numGroups-- > 0)
				{
					int realIndex = level.LeafFaceGroups[idx++];
				
					// Is it already checked ?
					if (faceGroupChecked[realIndex] == true) continue;

					faceGroupChecked[realIndex] = true;
				
					BspStaticFaceGroup faceGroup = level.FaceGroups[realIndex];
				
					// Get Material reference by handle
					Material mat = GetMaterial(faceGroup.materialHandle);

					// Check normal (manual culling)
					ManualCullingMode cullMode = mat.GetTechnique(0).GetPass(0).ManualCullMode;

					if(cullMode != ManualCullingMode.None)
					{
						float dist = faceGroup.plane.GetDistance(camera.DerivedPosition);
					
						if(((dist < 0) && (cullMode == ManualCullingMode.Back)) ||
							((dist > 0) && (cullMode == ManualCullingMode.Front)))
							continue;
					}

					// Try to insert, will find existing if already there
					matFaceGroupMap.Insert(mat, faceGroup);
				}
			}

			// TODO BspNode.IntersectingObjectSet
			// Add movables to render queue, provided it hasn't been seen already.			
			for(int i = 0; i < leaf.Objects.Count; i++)
			{
				if(!objectsForRendering.ContainsKey(((MovableObject)leaf.Objects[i]).Name))
				{
					MovableObject obj = leaf.Objects[i];

					if(obj.IsVisible && 
						(!onlyShadowCasters || obj.CastShadows) &&
						camera.IsObjectVisible(obj.GetWorldBoundingBox()))
					{
						obj.NotifyCurrentCamera(camera);
						obj.UpdateRenderQueue(this.renderQueue);
						// Check if the bounding box should be shown.
						SceneNode node = (SceneNode)obj.ParentNode;
						if (node.ShowBoundingBox || this.showBoundingBoxes)
						{
							node.AddBoundingBoxToQueue(this.renderQueue);
						}
						objectsForRendering.Add(obj);
					}
				}
			}
		}

		/// <summary>
		///		Caches a face group for imminent rendering.
		/// </summary>
		protected int CacheGeometry(IntPtr indexes, BspStaticFaceGroup faceGroup)
		{
			// Skip sky always
			if(faceGroup.isSky)
				return 0;

			int idxStart = 0;
			int numIdx = 0;
			int vertexStart = 0;

			if(faceGroup.type == FaceGroup.FaceList)
			{
				idxStart = faceGroup.elementStart;
				numIdx = faceGroup.numElements;
				vertexStart = faceGroup.vertexStart;
			}
			else if(faceGroup.type == FaceGroup.Patch)
			{
				idxStart = faceGroup.patchSurf.IndexOffset;
				numIdx = faceGroup.patchSurf.CurrentIndexCount;
				vertexStart = faceGroup.patchSurf.VertexOffset;
			}
			else
			{
				// Unsupported face type
				return 0;
			}

			unsafe
			{
				uint *src = (uint*) level.Indexes.Lock(
					idxStart * sizeof(uint), 
					numIdx * sizeof(uint), 
					BufferLocking.ReadOnly);
				uint *pIndexes = (uint*) indexes;

				// Offset the indexes here
				// we have to do this now rather than up-front because the 
				// indexes are sometimes reused to address different vertex chunks
				for(int i = 0; i < numIdx; i++)
					*pIndexes++ = (uint) (*src++ + vertexStart);

				level.Indexes.Unlock();
			}
			
			// return number of elements
			return numIdx;
		}

		/// <summary>
		///		Caches a face group and calculates texture lighting coordinates.
		/// </summary>
		unsafe protected int CacheLightGeometry(TextureLight light, uint* pIndexes, TextureLightMap* pTexLightMaps, BspVertex* pVertices, BspStaticFaceGroup faceGroup)
		{
			// Skip sky always
			if(faceGroup.isSky)
				return 0;

			int idxStart = 0;
			int numIdx = 0;
			int vertexStart = 0;

			if(faceGroup.type == FaceGroup.FaceList)
			{
				idxStart = faceGroup.elementStart;
				numIdx = faceGroup.numElements;
				vertexStart = faceGroup.vertexStart;
			}
			else if(faceGroup.type == FaceGroup.Patch)
			{
				idxStart = faceGroup.patchSurf.IndexOffset;
				numIdx = faceGroup.patchSurf.CurrentIndexCount;
				vertexStart = faceGroup.patchSurf.VertexOffset;
			}
			else
			{
				// Unsupported face type
				return 0;
			}

			uint *src = (uint*) level.Indexes.Lock(
				idxStart * sizeof(uint), 
				numIdx * sizeof(uint), 
				BufferLocking.ReadOnly);

			int maxIndex = 0;
			for (int i = 0; i <  numIdx; i++)
			{
				int index = (int) *(src + i);
				if (index > maxIndex)
					maxIndex = index;
			}

			Vector3[] vertexPos = new Vector3[maxIndex + 1];
			bool[] vertexIsStored = new bool[maxIndex + 1];

			for (int i = 0; i <  numIdx; i++)
			{
				uint index = *(src + i);
				if (!vertexIsStored[index])
				{
					vertexPos[index] = (*(pVertices + vertexStart + index)).position;
					vertexIsStored[index] = true;
				}
			}

			Vector2[] texCoors;
			ColorEx[] colors;

			bool res = light.CalculateTexCoordsAndColors(faceGroup.plane, vertexPos, out texCoors, out colors);

			if (res)
			{
				for (int i = 0; i <= maxIndex; i++)
				{
					pTexLightMaps[vertexStart + i].color = Root.Instance.RenderSystem.ConvertColor(colors[i]);
					pTexLightMaps[vertexStart + i].textureLightMap = texCoors[i];
				}

				// Offset the indexes here
				// we have to do this now rather than up-front because the 
				// indexes are sometimes reused to address different vertex chunks
				for(int i = 0; i < numIdx; i++)
					*pIndexes++ = (uint) (*src++ + vertexStart);

				level.Indexes.Unlock();
			
				// return number of elements
				return numIdx;
			}
			else
			{
				level.Indexes.Unlock();

				return 0;
			}
		}

		/// <summary>
		///		Adds a bounding box to draw if turned on.
		/// </summary>
		protected void AddBoundingBox(AxisAlignedBox aab, bool visible)
		{
		}

		/// <summary>
		///		Renders the static level geometry tagged in <see cref="Plugin_BSPSceneManager.BspSceneManager.WalkTree"/>.
		/// </summary>
		protected void RenderStaticGeometry()
		{
			// no world transform required
			targetRenderSystem.WorldMatrix = Matrix4.Identity;

			// Set view / proj
			targetRenderSystem.ViewMatrix = camInProgress.ViewMatrix;
			targetRenderSystem.ProjectionMatrix = camInProgress.ProjectionMatrix;

			ColorEx bspAmbient = null;

			if (level.BspOptions.ambientEnabled)
			{
				bspAmbient = new ColorEx(ambientColor.r * level.BspOptions.ambientRatio,
					ambientColor.g * level.BspOptions.ambientRatio,
					ambientColor.b * level.BspOptions.ambientRatio);
			}

			LayerBlendModeEx ambientBlend = new LayerBlendModeEx();
			ambientBlend.blendType = LayerBlendType.Color;
			ambientBlend.operation = LayerBlendOperationEx.Modulate;
			ambientBlend.source1 = LayerBlendSource.Texture;
            ambientBlend.source2 = LayerBlendSource.Manual;
			ambientBlend.colorArg2 = bspAmbient;

			// For each material in turn, cache rendering data & render
			IEnumerator mapEnu = matFaceGroupMap.buckets.Keys.GetEnumerator();

			bool passIsSet = false;

			while(mapEnu.MoveNext())
			{
				// Get Material
				Material thisMaterial = (Material) mapEnu.Current;
				BspStaticFaceGroup[] faceGrp = (BspStaticFaceGroup[]) ((ArrayList) matFaceGroupMap.buckets[thisMaterial]).ToArray(typeof(BspStaticFaceGroup));

				// if one face group is a quake shader then the material is a quake shader
				bool isQuakeShader = faceGrp[0].isQuakeShader;

				// Empty existing cache
				renderOp.indexData.indexCount = 0;
            
				// lock index buffer ready to receive data
				unsafe
				{
					uint *pIdx = (uint *) renderOp.indexData.indexBuffer.Lock(BufferLocking.Discard);

					for(int i = 0; i < faceGrp.Length; i++)
					{
						// Cache each
						int numElems = CacheGeometry((IntPtr) pIdx, faceGrp[i]);
						renderOp.indexData.indexCount += numElems;
						pIdx += numElems;
					}

					// Unlock the buffer
					renderOp.indexData.indexBuffer.Unlock();
				}
            
				// Skip if no faces to process (we're not doing flare types yet)
				if(renderOp.indexData.indexCount == 0)
					continue;
			
				if (isQuakeShader)
				{
					for(int i = 0; i < thisMaterial.GetTechnique(0).NumPasses; i++)
					{
						SetPass(thisMaterial.GetTechnique(0).GetPass(i));
						targetRenderSystem.Render(renderOp);
					}
					passIsSet = false;
				}
				else if (!passIsSet)
				{
					int i;
					for(i = 0; i < thisMaterial.GetTechnique(0).NumPasses; i++)
					{
						SetPass(thisMaterial.GetTechnique(0).GetPass(i));

						// for ambient lighting
						if (i == 0 && level.BspOptions.ambientEnabled)
						{
							targetRenderSystem.SetTextureBlendMode(0, ambientBlend);
						}

						targetRenderSystem.Render(renderOp);
					}

					// if it's only 1 pass then there's no need to set it again
					passIsSet = (i > 1) ? false : true;
				}
				else
				{
					Pass pass = thisMaterial.GetTechnique(0).GetPass(0);
					// Get the plain geometry texture
					TextureUnitState geometryTex = pass.GetTextureUnitState(0);
					targetRenderSystem.SetTexture(0, true, geometryTex.TextureName);

					if (pass.NumTextureUnitStages > 1)
					{
						// Get the lightmap
						TextureUnitState lightmapTex = pass.GetTextureUnitState(1);
						targetRenderSystem.SetTexture(1, true, lightmapTex.TextureName);
					}

					targetRenderSystem.Render(renderOp);
				}
			}

			//if(showNodeAABs)
			//	targetRenderSystem.Render(aaBGeometry);
		}

		/// <summary>
		///		Renders the texture lighting tagged in a light with index lightIndex.
		/// </summary>
		protected void RenderTextureLighting(int lightIndex)
		{
			TextureLight light = (TextureLight) lightList[lightIndex];

			if (!light.IsTextureLight)
				return;

			if (light.Type == LightType.Spotlight)
				spotlightFrustum.Spotlight = light;

			// no world transform required
			targetRenderSystem.WorldMatrix = Matrix4.Identity;

			// Set view / proj
			targetRenderSystem.ViewMatrix = camInProgress.ViewMatrix;
			targetRenderSystem.ProjectionMatrix = camInProgress.ProjectionMatrix;

			TextureUnitState lightTex = textureLightPass.GetTextureUnitState(0);
			TextureUnitState normalTex = textureLightPass.GetTextureUnitState(1);

			switch (light.Intensity)
			{
				case LightIntensity.Normal:
					normalTex.ColorBlendMode.operation = LayerBlendOperationEx.Modulate;
					break;

				case LightIntensity.ModulateX2:
					normalTex.ColorBlendMode.operation = LayerBlendOperationEx.ModulateX2;
					break;
				
				case LightIntensity.ModulateX4:
					normalTex.ColorBlendMode.operation = LayerBlendOperationEx.ModulateX4;
					break;
			}

			if (light.Type == LightType.Spotlight)
			{
				spotlightFrustum.Spotlight = light;
				lightTex.SetProjectiveTexturing(true, spotlightFrustum);
			}
			else
			{
				lightTex.SetProjectiveTexturing(false, null);
			}

			if (light.Type == LightType.Directional)
			{
				// light it using only diffuse color and alpha
				normalTex.ColorBlendMode.source2 = LayerBlendSource.Diffuse;
				normalTex.AlphaBlendMode.source2 = LayerBlendSource.Diffuse;
			}
			else
			{
				// light it using the texture light
				normalTex.ColorBlendMode.source2 = LayerBlendSource.Current;
				normalTex.AlphaBlendMode.source2 = LayerBlendSource.Current;
			}

			SetPass(textureLightPass);

			if (light.Type == LightType.Directional)
			{
				// Disable the light texture
				targetRenderSystem.SetTexture(0, true, lightTex.TextureName);
			}

			// For each material in turn, cache rendering data & render
			IEnumerator mapEnu = matFaceGroupMap.buckets.Keys.GetEnumerator();
            
			while(mapEnu.MoveNext())
			{
				// Get Material
				Material thisMaterial = (Material) mapEnu.Current;
				BspStaticFaceGroup[] faceGrp = (BspStaticFaceGroup[]) ((ArrayList) matFaceGroupMap.buckets[thisMaterial]).ToArray(typeof(BspStaticFaceGroup));

				// if one face group is a quake shader then the material is a quake shader
                if (faceGrp[0].isQuakeShader)
					continue;

				ManualCullingMode cullMode = thisMaterial.GetTechnique(0).GetPass(0).ManualCullMode;

				// Empty existing cache
				renderOp.indexData.indexCount = 0;

				HardwareVertexBuffer bspVertexBuffer = level.VertexData.vertexBufferBinding.GetBuffer(0);
				HardwareVertexBuffer lightTexCoordBuffer = level.VertexData.vertexBufferBinding.GetBuffer(1);

				// lock index buffer ready to receive data
				unsafe
				{
					BspVertex *pVertices = (BspVertex *) bspVertexBuffer.Lock(BufferLocking.ReadOnly);
					TextureLightMap *pTexLightMap = (TextureLightMap *) lightTexCoordBuffer.Lock(BufferLocking.Discard);
					uint *pIdx = (uint *) renderOp.indexData.indexBuffer.Lock(BufferLocking.Discard);

					for(int i = 0; i < faceGrp.Length; i++)
					{
						if (faceGrp[i].type != FaceGroup.Patch &&
							light.AffectsFaceGroup(faceGrp[i], cullMode))
						{
							// Cache each
							int numElems = CacheLightGeometry(light, pIdx, pTexLightMap, pVertices, faceGrp[i]);
							renderOp.indexData.indexCount += numElems;
							pIdx += numElems;
						}
					}

					// Unlock the buffers
					renderOp.indexData.indexBuffer.Unlock();
					lightTexCoordBuffer.Unlock();
					bspVertexBuffer.Unlock();
				}
            
				// Skip if no faces to process
				if(renderOp.indexData.indexCount == 0)
					continue;

				// Get the plain geometry texture
				TextureUnitState geometryTex = thisMaterial.GetTechnique(0).GetPass(0).GetTextureUnitState(0);
				if (geometryTex.IsBlank)
					continue;

				targetRenderSystem.SetTexture(1, true, geometryTex.TextureName);
				// OpenGL requires the addressing mode to be set before every render operation
				targetRenderSystem.SetTextureAddressingMode(0, TextureAddressing.Clamp);
				targetRenderSystem.Render(renderOp);
			}
		}

		/// <summary>
		///		Renders texture shadow on tagged in level geometry.
		/// </summary>
		protected void RenderTextureShadowOnGeometry()
		{
			// no world transform required
			targetRenderSystem.WorldMatrix = Matrix4.Identity;

			// Set view / proj
			targetRenderSystem.ViewMatrix = camInProgress.ViewMatrix;
			targetRenderSystem.ProjectionMatrix = camInProgress.ProjectionMatrix;

			Camera shadowCam = null;
			Vector3 camPos = Vector3.Zero, camDir = Vector3.Zero;
			TextureUnitState shadowTex = shadowReceiverPass.GetTextureUnitState(0);

			for(int i = 0; i < shadowTex.NumEffects; i++) 
			{
				if(shadowTex.GetEffect(i).type == TextureEffectType.ProjectiveTexture)
				{
					shadowCam = (Camera) shadowTex.GetEffect(i).frustum;
					camPos = shadowCam.DerivedPosition;
					camDir = shadowCam.DerivedDirection;
					break;
				}
			}

			CullingMode prevCullMode = shadowReceiverPass.CullMode;
			LayerBlendModeEx colorBlend = shadowTex.ColorBlendMode;
			LayerBlendSource prevSource = colorBlend.source2;
			ColorEx prevColorArg = colorBlend.colorArg2;

			// Quake uses counter-clockwise culling
			shadowReceiverPass.CullMode = CullingMode.CounterClockwise;
			colorBlend.source2 = LayerBlendSource.Manual;
			colorBlend.colorArg2 = ColorEx.White;

			SetPass(shadowReceiverPass);

			shadowReceiverPass.CullMode = prevCullMode;
			colorBlend.source2 = prevSource;
			colorBlend.colorArg2 = prevColorArg;

			// Empty existing cache
			renderOp.indexData.indexCount = 0;
            
			// lock index buffer ready to receive data
			unsafe
			{
				uint *pIdx = (uint *) renderOp.indexData.indexBuffer.Lock(BufferLocking.Discard);

				// For each material in turn, cache rendering data
				IEnumerator mapEnu = matFaceGroupMap.buckets.Keys.GetEnumerator();
            
				while(mapEnu.MoveNext())
				{
					// Get Material
					Material thisMaterial = (Material) mapEnu.Current;
					BspStaticFaceGroup[] faceGrp = (BspStaticFaceGroup[]) ((ArrayList) matFaceGroupMap.buckets[thisMaterial]).ToArray(typeof(BspStaticFaceGroup));

					// if one face group is a quake shader then the material is a quake shader
					if (faceGrp[0].isQuakeShader)
						continue;

					for(int i = 0; i < faceGrp.Length; i++)
					{
						float dist = faceGrp[i].plane.GetDistance(camPos);
						float angle = faceGrp[i].plane.Normal.Dot(camDir);

						if (((dist < 0 && angle > 0) || (dist > 0 && angle < 0)) &&
							MathUtil.Abs(angle) >= MathUtil.Cos(shadowCam.FOV * 0.5f))
						{
							// face is in shadow's frustum

							// Cache each
							int numElems = CacheGeometry((IntPtr) pIdx, faceGrp[i]);
							renderOp.indexData.indexCount += numElems;
							pIdx += numElems;
						}
					}
				}
			}

			// Unlock the buffer
			renderOp.indexData.indexBuffer.Unlock();

			// Skip if no faces to process
			if(renderOp.indexData.indexCount == 0)
				return;
			
			targetRenderSystem.Render(renderOp);
		}

		/// <summary>
		///		Overriden from SceneManager.
		/// </summary>
		protected override void RenderSingleObject(IRenderable renderable, Pass pass, 
			bool doLightIteration, LightList manualLightList)
		{
			if (renderable is BspGeometry)
			{
				// Render static level geometry
				if (doLightIteration)
				{
					// render all geometry without lights first
					RenderStaticGeometry();

					// render geometry affected by each visible light
					for (int i = 0; i < lightsAffectingFrustum.Count; i++)
					{
						int index;

						// find the index of the light
						for (index = 0; index < lightList.Count; index++)
							if (lightList[index] == lightsAffectingFrustum[i]) break;

						if (index < lightList.Count)
						{
							RenderTextureLighting(index);
						}
					}
				}
				else
				{
					if (manualLightList.Count == 0)
					{
						if (illuminationStage == IlluminationRenderStage.RenderModulativePass)
						{
							// texture shadows
							RenderTextureShadowOnGeometry();
						}
						else
						{
							// ambient stencil pass, render geometry without lights
							RenderStaticGeometry();
						}
					}
					else
					{
						// render only geometry affected by the provided light 
						for (int i = 0; i < manualLightList.Count; i++)
						{
							int index;

							// find the index of the light
							for (index = 0; index < lightList.Count; index++)
								if (lightList[index] == manualLightList[i]) break;

							if (index < lightList.Count)
							{
								RenderTextureLighting(index);
							}
						}
					}
				}
			}
			else
			{
				base.RenderSingleObject(renderable, pass, doLightIteration, manualLightList);
			}
		}

		#endregion
	}

	/// <summary>
	///		BSP specialisation of IntersectionSceneQuery.
	/// </summary>
	public class BspIntersectionSceneQuery : DefaultIntersectionSceneQuery
	{
		#region Constructor
		public BspIntersectionSceneQuery(SceneManager creator) : base(creator)
		{
			this.AddWorldFragmentType(WorldFragmentType.PlaneBoundedRegion);
		}
		#endregion	

		#region Public methods
		
		public override void Execute(IIntersectionSceneQueryListener listener)
		{
			//Go through each leaf node in BspLevel and check movables against each other and world
			//Issue: some movable-movable intersections could be reported twice if 2 movables
			//overlap 2 leaves?
			BspLevel lvl = ((BspSceneManager) this.creator).Level;
			int leafPoint = lvl.LeafStart;
			int numLeaves = lvl.NumLeaves;

			Bsp.Collections.Map objIntersections = new Bsp.Collections.Map();
			PlaneBoundedVolume boundedVolume = new PlaneBoundedVolume(PlaneSide.Positive);
        
			while ((numLeaves--) != 0)
			{
				BspNode leaf = lvl.Nodes[leafPoint];
				MovableObjectCollection objects = leaf.Objects;
				int numObjects = objects.Count;

				for(int a = 0; a < numObjects; a++)
				{
					MovableObject aObj = objects[a];
					// Skip this object if collision not enabled
					if((aObj.QueryFlags & queryMask) == 0)
						continue;

					if(a < (numObjects - 1))
					{
						// Check object against others in this node
						int b = a;
						for (++b; b < numObjects; ++b)
						{
							MovableObject bObj = objects[b];
							// Apply mask to b (both must pass)
							if ((bObj.QueryFlags & queryMask) != 0)
							{
								AxisAlignedBox box1 = aObj.GetWorldBoundingBox();
								AxisAlignedBox box2 = bObj.GetWorldBoundingBox();

								if (box1.Intersects(box2))
								{
									//Check if this pair is already reported
									bool alreadyReported = false;
									IList interObjList = objIntersections.FindBucket(aObj);
									if (interObjList != null)
										if (interObjList.Contains(bObj))
											alreadyReported = true;

									if (!alreadyReported)
									{
										objIntersections.Insert(aObj,bObj);
										listener.OnQueryResult(aObj,bObj);
									}
								}
							}
						}
					}
					// Check object against brushes

					/*----This is for bounding sphere-----
					float radius = aObj.BoundingRadius;
					//-------------------------------------------*/

					for (int brushPoint=0; brushPoint < leaf.SolidBrushes.Length; brushPoint++)
					{
						BspBrush brush = leaf.SolidBrushes[brushPoint];

						if (brush == null) continue;

						bool brushIntersect = true; // Assume intersecting for now

						/*----This is for bounding sphere-----
						IEnumerator planes = brush.Planes.GetEnumerator();

						while (planes.MoveNext())
						{
							float dist = ((Plane)planes.Current).GetDistance(pos);
							if (dist > radius)
							{
								// Definitely excluded
								brushIntersect = false;
								break;
							}
						}
						//-------------------------------------------*/

						boundedVolume.planes = brush.Planes;
						//Test object as bounding box
						if (!boundedVolume.Intersects(aObj.GetWorldBoundingBox()))
							brushIntersect = false;

						if (brushIntersect)
						{
							//Check if this pair is already reported
							bool alreadyReported = false;
							IList interObjList = objIntersections.FindBucket(aObj);
							if (interObjList != null)
								if (interObjList.Contains(brush))
									alreadyReported = true;

							if (!alreadyReported)
							{
								objIntersections.Insert(aObj,brush);
								// report this brush as it's WorldFragment
								brush.Fragment.FragmentType = WorldFragmentType.PlaneBoundedRegion;
								listener.OnQueryResult(aObj,brush.Fragment);
							}
						}
					}
				}
				++leafPoint;
			}
		}
		#endregion
	}

	/// <summary>
	///		BSP specialisation of RaySceneQuery.
	/// </summary>
	public class BspRaySceneQuery : DefaultRaySceneQuery
	{
		#region Constructor
		public BspRaySceneQuery(SceneManager creator) : base(creator)
		{
			this.AddWorldFragmentType(WorldFragmentType.PlaneBoundedRegion);
		}
		#endregion	

		#region Protected Members

		protected IRaySceneQueryListener listener;
		protected bool StopRayTracing;

		#endregion

		#region Public methods
		
		public override void Execute(IRaySceneQueryListener listener)
		{
			this.listener = listener;
			this.StopRayTracing = false;
            ProcessNode(((BspSceneManager)creator).Level.RootNode, ray, float.PositiveInfinity, 0);
		}
		#endregion

		#region Protected methods

		protected virtual void ProcessNode(BspNode node, Ray tracingRay, float maxDistance, float traceDistance)
		{
			// check if ray already encountered a solid brush
			if (StopRayTracing) return;

			if (node.IsLeaf)
			{
				ProcessLeaf(node, tracingRay, maxDistance, traceDistance);
				return;
			}

			IntersectResult result = tracingRay.Intersects(node.SplittingPlane);
			if (result.Hit)
			{
				if (result.Distance < maxDistance)
				{
					if (node.GetSide(tracingRay.Origin) == PlaneSide.Negative)
					{
						ProcessNode(node.BackNode, tracingRay, result.Distance, traceDistance);
						Vector3 splitPoint = tracingRay.Origin + tracingRay.Direction * result.Distance;
						ProcessNode(node.FrontNode, new Ray(splitPoint, tracingRay.Direction), maxDistance - result.Distance, traceDistance + result.Distance);
					}
					else
					{
						ProcessNode(node.FrontNode, tracingRay, result.Distance, traceDistance);
						Vector3 splitPoint = tracingRay.Origin + tracingRay.Direction * result.Distance;
						ProcessNode(node.BackNode, new Ray(splitPoint, tracingRay.Direction), maxDistance - result.Distance, traceDistance + result.Distance);
					}
				}
				else
					ProcessNode(node.GetNextNode(tracingRay.Origin), tracingRay, maxDistance, traceDistance);
			}
			else
				ProcessNode(node.GetNextNode(tracingRay.Origin), tracingRay, maxDistance, traceDistance);
		}

		protected virtual void ProcessLeaf(BspNode leaf, Ray tracingRay, float maxDistance, float traceDistance)
		{
			MovableObjectCollection objects = leaf.Objects;
			int numObjects = objects.Count;

			//Check ray against objects
			for(int a = 0; a < numObjects; a++)
			{
				MovableObject obj = objects[a];
				// Skip this object if collision not enabled
				if((obj.QueryFlags & queryMask) == 0)
					continue;

				//Test object as bounding box
				IntersectResult result = tracingRay.Intersects(obj.GetWorldBoundingBox());
				// if the result came back positive and intersection point is inside
				// the node, fire the event handler
				if(result.Hit && result.Distance <= maxDistance) 
				{
					listener.OnQueryResult(obj, result.Distance + traceDistance);
				}
			}

			PlaneBoundedVolume boundedVolume = new PlaneBoundedVolume(PlaneSide.Positive);
			BspBrush intersectBrush = null;
			float intersectBrushDist = float.PositiveInfinity;

			// Check ray against brushes
			for (int brushPoint=0; brushPoint < leaf.SolidBrushes.Length; brushPoint++)
			{
				BspBrush brush = leaf.SolidBrushes[brushPoint];

				if (brush == null) continue;

				boundedVolume.planes = brush.Planes;

				IntersectResult result = tracingRay.Intersects(boundedVolume);
				// if the result came back positive and intersection point is inside
				// the node, check if this brush is closer
				if(result.Hit && result.Distance <= maxDistance) 
				{
					if (result.Distance < intersectBrushDist)
					{
						intersectBrushDist = result.Distance;
						intersectBrush = brush;
					}
				}
			}

			if (intersectBrush != null)
			{
				listener.OnQueryResult(intersectBrush.Fragment, intersectBrushDist + traceDistance);
				StopRayTracing = true;
			}
		}
		#endregion
	}

	/// <summary>
	///		BSP specialisation of SphereRegionSceneQuery.
	/// </summary>
	public class BspSphereRegionSceneQuery : DefaultSphereRegionSceneQuery
	{
		#region Constructor
		public BspSphereRegionSceneQuery(SceneManager creator) : base(creator)
		{
			this.AddWorldFragmentType(WorldFragmentType.PlaneBoundedRegion);
		}
		#endregion	

		#region Protected Members

		protected ISceneQueryListener listener;
		protected ArrayList foundIntersections = new ArrayList();

		#endregion

		#region Public methods
		
		public override void Execute(ISceneQueryListener listener)
		{
			this.listener = listener;
			this.foundIntersections.Clear();
			ProcessNode(((BspSceneManager)creator).Level.RootNode);
		}
		#endregion

		#region Protected methods

		protected virtual void ProcessNode(BspNode node)
		{
			if (node.IsLeaf)
			{
				ProcessLeaf(node);
				return;
			}

			float distance = node.GetDistance(sphere.Center);

			if(MathUtil.Abs(distance) < sphere.Radius)
			{
				// Sphere crosses the plane, do both.
				ProcessNode(node.BackNode);
				ProcessNode(node.FrontNode);
			}
			else if(distance < 0)
			{
				// Do back.
				ProcessNode(node.BackNode);
			}
			else
			{
				// Do front.
				ProcessNode(node.FrontNode);
			}
		}

		protected virtual void ProcessLeaf(BspNode leaf)
		{
			MovableObjectCollection objects = leaf.Objects;
			int numObjects = objects.Count;

			//Check sphere against objects
			for(int a = 0; a < numObjects; a++)
			{
				MovableObject obj = objects[a];
				// Skip this object if collision not enabled
				if((obj.QueryFlags & queryMask) == 0)
					continue;

				//Test object as bounding box
				if(sphere.Intersects(obj.GetWorldBoundingBox())) 
				{
					if (!foundIntersections.Contains(obj))
					{
						listener.OnQueryResult(obj);
						foundIntersections.Add(obj);
					}
				}
			}

			PlaneBoundedVolume boundedVolume = new PlaneBoundedVolume(PlaneSide.Positive);

			// Check ray against brushes
			for (int brushPoint=0; brushPoint < leaf.SolidBrushes.Length; brushPoint++)
			{
				BspBrush brush = leaf.SolidBrushes[brushPoint];
				if (brush == null) continue;

				boundedVolume.planes = brush.Planes;
				if(boundedVolume.Intersects(sphere)) 
				{
					listener.OnQueryResult(brush.Fragment);
				}
			}
		}
		#endregion
	}
}