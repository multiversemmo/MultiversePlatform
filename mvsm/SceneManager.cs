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
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Collections;
using Axiom.Utility;
using Axiom.Animating;
using Multiverse;
using Multiverse.CollisionLib;

namespace Axiom.SceneManagers.Multiverse
{
	/// <summary>
	/// Summary description for SceneManager.
	/// </summary>
	public class SceneManager :  Axiom.Core.SceneManager
	{
		protected ITerrainGenerator terrainGenerator;
		protected ILODSpec lodSpec;
        protected FogConfig fogConfig;
        protected AmbientLightConfig ambientLightConfig;

		public SceneManager() : base("MVSceneManager")
		{
			//
			// TODO: Add constructor logic here
			//

            // set up default shadow parameters
            SetShadowTextureSettings(1024, 1, Axiom.Media.PixelFormat.FLOAT32_R);
            shadowColor = new ColorEx(0.75f, 0.75f, 0.75f);
            shadowFarDistance = 20000;
            shadowDirLightExtrudeDist = 100000;
            ShadowCasterRenderBackFaces = false;

            fogConfig = new FogConfig(this);
            ambientLightConfig = new AmbientLightConfig(this);
		}

		/// <summary>
		/// Loads the LandScape using parameters in the given config file.
		/// </summary>
		/// <param name="filename"></param>
		public override void LoadWorldGeometry( string filename )
		{
            Boundary.parentSceneNode = RootSceneNode;
            TerrainManager.Instance.Cleanup();
			TerrainManager.Instance.Initialize(this, terrainGenerator, lodSpec, RootSceneNode);
		}

        protected void InitShadowParams()
        {
            ShadowTextureCasterMaterial = "MVSMShadowCaster";
            ShadowTextureReceiverMaterial = "MVSMShadowReceiver";
            ShadowTextureSelfShadow = true;
        }

		public void SetWorldParams(ITerrainGenerator terrainGenerator, ILODSpec lodSpec)
		{
            InitShadowParams();

			this.terrainGenerator = terrainGenerator;
			this.lodSpec = lodSpec;
		}

		public override void ClearScene()
		{
			TerrainManager.Instance.Cleanup();

			base.ClearScene();
		}

        public void AddBoundary(Boundary b)
        {
            TerrainManager.Instance.AddBoundary(b);
        }

        public void RemoveBoundary(Boundary b)
        {
            TerrainManager.Instance.RemoveBoundary(b);
        }

        public void ExportBoundaries(XmlTextWriter w)
        {
            TerrainManager.Instance.ExportBoundaries(w);
        }

        public void ImportBoundaries(XmlTextReader r)
        {
            TerrainManager.Instance.ImportBoundaries(r);
        }

        public Road CreateRoad(String name)
        {
            return TerrainManager.Instance.CreateRoad(name);
        }

        public void RemoveRoad(Road r)
        {
            TerrainManager.Instance.RemoveRoad(r);
        }

        public float GetAreaHeight(Vector3[] points)
        {
            return TerrainManager.Instance.GetAreaHeight(points);
        }

		// This entrypoint is invoked by the higher-level client to
		// set up the collision tile manager
		public void SetCollisionInterface(CollisionAPI API, float tileSize)
		{
            TerrainManager.Instance.SetCollisionInterface(API, tileSize);
		}
		
		// This entrypoint is invoked by the higher-level client, and
		// in turn invokes the collision tile manager operation to
		// change the collision center and the collision horizon, as
		// represented in the radius
		public void SetCollisionArea(Vector3 center, float radius)
		{
			TerrainManager.Instance.SetCollisionArea(center, radius);
		}

		/// <summary>
		/// Internal method for updating the scene graph ie the tree of SceneNode instances managed by this class.
		/// </summary>
		/// <param name="cam"></param>
		/// <remarks>
		///	This must be done before issuing objects to the rendering pipeline, since derived transformations from
		///	parent nodes are not updated until required. This SceneManager is a basic implementation which simply
		///	updates all nodes from the root. This ensures the scene is up to date but requires all the nodes
		///	to be updated even if they are not visible. Subclasses could trim this such that only potentially visible
		///	nodes are updated.
		/// </remarks>
		protected override void UpdateSceneGraph( Axiom.Core.Camera cam )
		{
            if (illuminationStage == IlluminationRenderStage.None)
            {
                // Notify the TerrainManager of the camera location.  If a page or tile boundary is crossed
                // this will result in a bunch of shuffling of heightMap data.
                // After the object bounding boxes have been update below, we need to call
                // the TerrainManager again to perform LOD adjustments on all visible tiles, 
                // and queue the non-visible ones.
                TerrainManager.Instance.UpdateCamera(cam);
            }

			// call the base SceneManager to update the transforms and world bounds of all the
			// objects in the scene graph.
			base.UpdateSceneGraph(cam);

            if (illuminationStage == IlluminationRenderStage.None)
            {
                TerrainManager.Instance.LatePerFrameProcessing(cam);
            }

		}

		/// <summary>
		/// Creates a RaySceneQuery for this scene manager.
		/// </summary>
		/// <param name="ray">Details of the ray which describes the region for this query.</param>
		/// <param name="mask">The query mask to apply to this query; can be used to filter out certain objects; see SceneQuery for details.</param>
		/// <returns>
		///	The instance returned from this method must be destroyed by calling
		///	SceneManager::destroyQuery when it is no longer required.
		/// </returns>
		/// <remarks>
		///	This method creates a new instance of a query object for this scene manager, 
		///	looking for objects which fall along a ray. See SceneQuery and RaySceneQuery 
		///	for full details.
		/// </remarks>
		public override Axiom.Core.RaySceneQuery CreateRayQuery(Ray ray, ulong mask)
		{
			Axiom.Core.RaySceneQuery q = new Axiom.SceneManagers.Multiverse.RaySceneQuery(this);
			q.Ray = ray;
			q.QueryMask = mask;
			return q;
		}

		protected const ulong defaultRayQueryFlags = 
			(ulong)RaySceneQueryType.AllTerrain |
			(ulong)RaySceneQueryType.Entities |
			(ulong)RaySceneQueryType.OnexRes;

		public override Axiom.Core.RaySceneQuery CreateRayQuery(Ray ray) 
		{
			return CreateRayQuery(ray, defaultRayQueryFlags);
		}

		internal Pass SetTreeRenderPass(Pass pass, IRenderable renderable)
		{
            targetRenderSystem.BeginProfileEvent(ColorEx.Green, "SetPassTreeRender: Material = " + pass.Parent.Parent.Name);
			Pass usedPass = SetPass(pass);

			autoParamDataSource.Renderable = renderable;
			usedPass.UpdateAutoParamsNoLights(autoParamDataSource);

			// get the world matrices
			renderable.GetWorldTransforms(xform);
			targetRenderSystem.WorldMatrix = xform[0];
			
			// set up light params
			autoParamDataSource.SetCurrentLightList(renderable.Lights);
			usedPass.UpdateAutoParamsLightsOnly(autoParamDataSource);

			// note: parameters must be bound after auto params are updated
			if(usedPass.HasVertexProgram) 
			{
				targetRenderSystem.BindGpuProgramParameters(GpuProgramType.Vertex, usedPass.VertexProgramParameters);
			}
			if(usedPass.HasFragmentProgram) 
			{
				targetRenderSystem.BindGpuProgramParameters(GpuProgramType.Fragment, usedPass.FragmentProgramParameters);
			}

            targetRenderSystem.EndProfileEvent();

            return usedPass;
		}

		private TimingMeter baseRenderSolidMeter = MeterManager.GetMeter("Base Render", "MVSM Render Solid");
		private TimingMeter boundaryRenderSolidMeter = MeterManager.GetMeter("Boundary Render", "MVSM Render Solid");

        protected override void RenderSolidObjects(SortedList list, bool doLightIteration,
            List<Light> manualLightList)
        {
            if (renderingMainGroup)
            {
                if (!(renderingNoShadowQueue || (illuminationStage == IlluminationRenderStage.RenderModulativePass)))
                {
                    TreeGroup.RenderAllTrees(targetRenderSystem);
                }
            }

            baseRenderSolidMeter.Enter();
            base.RenderSolidObjects(list, doLightIteration, manualLightList);
            baseRenderSolidMeter.Exit();

        }

        /// <summary>
        ///		Renders a set of solid objects for the shadow receiver pass.
        ///		Will only render 
        /// </summary>
        /// <param name="list">List of solid objects.</param>
        protected virtual void RenderShadowReceiverObjects(SortedList list, bool doLightIteration,
            List<Light> manualLightList)
        {
            // compute sphere of area around camera that can receive shadows.
            Sphere shadowSphere = new Sphere(autoParamDataSource.CameraPosition, ShadowConfig.ShadowFarDistance);
            List<IRenderable> shadowList = new List<IRenderable>();

            // ----- SOLIDS LOOP -----
            // 			renderSolidObjectsMeter.Enter();
            for (int i = 0; i < list.Count; i++)
            {
                RenderableList renderables = (RenderableList)list.GetByIndex(i);
                
                // bypass if this group is empty
                if (renderables.Count == 0)
                {
                    continue;
                }

                Pass pass = (Pass)list.GetKey(i);

                // Give SM a chance to eliminate this pass
                if (!ValidatePassForRendering(pass))
                    continue;

                shadowList.Clear();

                // special case for a single renderable using the material, so that we can
                // avoid calling SetPass() when there is only one renderable and it doesn't
                // need to be drawn.
                for (int r = 0; r < renderables.Count; r++)
                {
                    bool drawObject = true;
                    IRenderable renderable = (IRenderable)renderables[r];
                    MovableObject movableObject = renderable as MovableObject;
                    if (movableObject != null)
                    {
                        // it is a movableObject, so we can 
                        if (!movableObject.GetWorldBoundingBox().Intersects(shadowSphere))
                        {
                            // objects bounding box doesn't intercect the shadow sphere, so we don't
                            // need to render it in this pass.
                            drawObject = false;
                        }
                    }

                    if (drawObject)
                    {
                        shadowList.Add(renderable);
                    }
                }

                // if nobody is within shadow range, then we don't need to render, and skip the SetPass()
                if (shadowList.Count == 0)
                {
                    continue;
                }

                // For solids, we try to do each pass in turn
                Pass usedPass = SetPass(pass);

                // render each object associated with this rendering pass
                foreach (IRenderable renderable in shadowList)
                {

                    // Give SM a chance to eliminate
                    if (!ValidateRenderableForRendering(usedPass, renderable))
                        continue;

                    // Render a single object, this will set up auto params if required

                    if (MeterManager.Collecting)
                        MeterManager.AddInfoEvent("RenderSingle shadow receiver material {0}, doLight {1}, lightCnt {2}",
                            renderable.Material.Name, doLightIteration, (manualLightList != null ? manualLightList.Count : 0));
                    try
                    {
                        RenderSingleObject(renderable, usedPass, doLightIteration, manualLightList);
                    }
                    catch (Exception e)
                    {
                        LogManager.Instance.WriteException("Invalid call to Axiom.Core.SceneManager.RenderSingleObject: {0}\n{1}", e.Message, e.StackTrace);
                        if (renderable.Material != null)
                            LogManager.Instance.Write("Failed renderable material: {0}", renderable.Material.Name);
                    }

                }
            }
            // 			renderSolidObjectsMeter.Exit();
        }

        /// <summary>
        ///		Render a group rendering only shadow receivers.
        ///		We have our own version here to add some optimizations
        /// </summary>
        /// <param name="group">Render queue group.</param>
        protected override void RenderTextureShadowReceiverQueueGroupObjects(RenderQueueGroup group)
        {
            // Override auto param ambient to force vertex programs to go full-bright
            autoParamDataSource.AmbientLight = ColorEx.White;
            targetRenderSystem.AmbientLight = ColorEx.White;

            // Iterate through priorities
            for (int i = 0; i < group.NumPriorityGroups; i++)
            {
                RenderPriorityGroup priorityGroup = group.GetPriorityGroup(i);

                // Do solids, override light list in case any vertex programs use them
                RenderShadowReceiverObjects(priorityGroup.SolidPasses, false, nullLightList);

                // Don't render transparents or passes which have shadow receipt disabled

            }// for each priority

            // reset ambient
            autoParamDataSource.AmbientLight = ambientColor;
            targetRenderSystem.AmbientLight = ambientColor;
        }

        public override void SetFog(FogMode mode, ColorEx color, float density, float linearStart, float linearEnd)
        {
            // set the scene manager member variables to the new values
            base.SetFog(mode, color, density, linearStart, linearEnd);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="color"></param>
        /// <param name="density"></param>
        public override void SetFog(FogMode mode, ColorEx color, float density)
        {
            SetFog(mode, color, density, 0.0f, 1.0f);
        }

        public ShadowConfig ShadowConfig
        {
            get
            {
                return TerrainManager.Instance.ShadowConfig;
            }
        }

        public OceanConfig OceanConfig
        {
            get
            {
                return TerrainManager.Instance.OceanConfig;
            }
        }

        public AutoParamDataSource AutoParamDataSource
        {
            get
            {
                return autoParamDataSource;
            }
        }

        public FogConfig FogConfig
        {
            get
            {
                return fogConfig;
            }
        }

        public AmbientLightConfig AmbientLightConfig
        {
            get
            {
                return ambientLightConfig;
            }
        }

        public bool DisplayTerrain
        {
            get
            {
                return TerrainManager.Instance.DrawTerrain;
            }
            set
            {
                TerrainManager.Instance.DrawTerrain = value;
            }
        }

        public void SetProfileMarker(ColorEx color, string name)
        {
            targetRenderSystem.SetProfileMarker(color, name);
        }
    }
}
