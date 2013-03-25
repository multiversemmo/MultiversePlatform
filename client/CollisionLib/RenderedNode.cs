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

#region Using directives

using System;
using Vector3 = Axiom.MathLib.Vector3;
using Matrix3 = Axiom.MathLib.Matrix3;
using Quaternion = Axiom.MathLib.Quaternion;
using Axiom.Core;
using Axiom.Graphics;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Multiverse.CollisionLib;

#endregion

namespace Multiverse.CollisionLib
{

public struct RenderedNode
{
	public Entity entity;
	public SceneNode node;

	public enum RenderState { None, All, Obstacles };
	
	public RenderedNode(Entity entity, SceneNode node) 
	{
		this.entity = entity;
		this.node = node;
	}

    // A flag to control whether collision shapes are rendered
	public static RenderState renderState = RenderState.None;

	// The scene manager used to render collision shapes
	private static SceneManager scene = null;
	
	private static string RenderedNodeName(int id, int index)
	{
		return string.Format("STL_{0}_{1}", id, index);
	}
	
	private static RenderedNode UnscaledRenderedObject(string meshName, int id,
													   int index, Vector3 position)
	{
		string name = RenderedNodeName(id, index);
		if (MO.DoLog)
			MO.Log(string.Format(" Creating rendering for node {0}", name));
		Entity entity = scene.CreateEntity(name, meshName);
		SceneNode node = scene.RootSceneNode.CreateChildSceneNode(name);
		node.AttachObject(entity);
		node.Position = position;
		node.ScaleFactor = Vector3.UnitScale;
		node.Orientation = Quaternion.Identity;
		RenderedNode n = new RenderedNode(entity, node);
		return n;
	}
	
	private static RenderedNode NewRenderedObject(string meshName, int id, int index,
												  Vector3 position, float scale)
	{
		RenderedNode n = UnscaledRenderedObject(meshName, id, index, position);
		n.node.ScaleFactor = Vector3.UnitScale * scale;
		return n;
	}
	
	private static RenderedNode NewRenderedObject(string meshName, int id, int index, Vector3 position, 
												  float scale, Quaternion orientation)
	{
		RenderedNode n = NewRenderedObject(meshName, id, index, position, scale);
		n.node.Orientation = orientation;
		return n;
	}
	
	private static RenderedNode NewRenderedObject(string meshName, int id, int index, Vector3 position, 
												  Vector3 scale, Quaternion orientation)
	{
		RenderedNode n = UnscaledRenderedObject(meshName, id, index, position);
		n.node.ScaleFactor = scale;
		n.node.Orientation = orientation;
		return n;
	}
	
	private static RenderedNode NewRenderedSphere(int id, int index, Vector3 center, float radius)
	{
		return NewRenderedObject("unit_sphere.mesh", id, index, center, radius);
	}
	
	private static RenderedNode NewRenderedBox(int id, int index, CollisionOBB box)
	{
		RenderedNode n = UnscaledRenderedObject("unit_box.mesh", id, index, box.center);
		n.node.ScaleFactor = 2 * box.extents * MO.DivMeter;
		Quaternion q = Quaternion.Identity;
		q.FromAxes(box.axes[0], box.axes[1], box.axes[2]);
		n.node.Orientation = q;
		return n;
	}
	
	// if obstacle is true, it's an obstacle.  If obstacle is false,
	// it's a moving object part.
	public static void MaybeCreateRenderedNodes(bool obstacle, CollisionShape shape, int id,
												ref List<RenderedNode> renderedNodes)
	{
		bool create = (obstacle ? renderState != RenderState.None :
					   renderState == RenderState.All);
		if (create)
			CreateRenderedNodes(shape, id, ref renderedNodes);
	}
	
	
	public static void CreateRenderedNodes(CollisionShape shape, int id, ref List<RenderedNode> renderedNodes)
	{
		// To keep all the rendering stuff internal to this file, we
		// reach inside of collision objects to create the rendered
		// shapes
		if (renderedNodes == null)
			renderedNodes = new List<RenderedNode>();
		switch(shape.ShapeType()) {
		case ShapeEnum.ShapeSphere:
			renderedNodes.Add(NewRenderedSphere(id, 0, shape.center, shape.radius * MO.DivMeter));
			break;
		case ShapeEnum.ShapeCapsule:
			CollisionCapsule c = (CollisionCapsule)shape;
			float r = c.capRadius * MO.DivMeter;
 			renderedNodes.Add(NewRenderedSphere(id, 0, c.bottomcenter, r));
 			renderedNodes.Add(NewRenderedSphere(id, 1, c.topcenter, r));
			Vector3 seg = (c.topcenter - c.bottomcenter);
			renderedNodes.Add(NewRenderedObject("unit_cylinder.mesh",
												id, 2, c.center,
												new Vector3(r, seg.Length * MO.DivMeter, r),
												new Vector3(0f, 1f, 0f).GetRotationTo(seg)));
			break;
		case ShapeEnum.ShapeAABB:
			renderedNodes.Add(NewRenderedBox(id, 0, ((CollisionAABB)shape).OBB()));
			break;
		case ShapeEnum.ShapeOBB:
			renderedNodes.Add(NewRenderedBox(id, 0, (CollisionOBB)shape));
			break;
		}
	}

	private static bool RenderObstacles(RenderState state)
	{
		return state == RenderState.All || state == RenderState.Obstacles;
	}
	
	private static bool RenderMovingObjects(RenderState state)
	{
		return state == RenderState.All;
	}
	
	public static SceneManager Scene {
        set {
            scene = value;
        }
    }

    public static RenderState ToggleRenderCollisionVolumes(CollisionAPI api, SceneManager sceneManager, bool movingObjects)
	{
		if (scene == null)
			scene = sceneManager;
		RenderState previousState = renderState;
		if (movingObjects)
			renderState = (renderState == RenderState.None ? RenderState.All :
						   (renderState == RenderState.All ? RenderState.Obstacles : RenderState.None));
		else
			renderState = (renderState == RenderState.None ? RenderState.Obstacles : RenderState.None);
		bool renderObstacles = RenderObstacles(renderState);
		if (renderObstacles != RenderObstacles(previousState))
			api.SphereTree.ChangeRendering(renderObstacles);
		bool renderMOs = RenderMovingObjects(renderState);
		if (renderMOs != RenderMovingObjects(previousState))
			MovingObject.ChangeRendering(renderMOs);
		return previousState;
	}

	public static void RemoveNodeRendering(int id, ref List<RenderedNode> renderedNodes)
	{
		if (renderedNodes != null) {
			for(int i=0; i<renderedNodes.Count; i++) {
				string name = RenderedNodeName(id, i);
				if (MO.DoLog)
					MO.Log(string.Format(" Removing rendering for node {0}", name));
				RenderedNode n = renderedNodes[i];
				n.node.Creator.DestroySceneNode(name);
				// remove the entity from the scene
				scene.RemoveEntity(n.entity);
				// clean up any unmanaged resources
				n.entity.Dispose();
			}
			renderedNodes.Clear();
		}
	}
	
	public static void ChangeRendering(bool render, CollisionShape shape, int id, 
									   ref List<RenderedNode> renderedNodes)
	{
		if (!render)
			RenderedNode.RemoveNodeRendering(id, ref renderedNodes);
		else
			RenderedNode.CreateRenderedNodes(shape, id, ref renderedNodes);
	}

	public static void AddDisplacement(Vector3 displacement, ref List<RenderedNode> renderedNodes)
	{
		if (renderedNodes != null) {
			for(int i=0; i<renderedNodes.Count; i++) {
				RenderedNode n = renderedNodes[i];
				n.node.Position += displacement;
			}
		}
	}

}
}

	

