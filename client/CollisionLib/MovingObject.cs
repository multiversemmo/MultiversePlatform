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
using Matrix4 = Axiom.MathLib.Matrix4;
using Quaternion = Axiom.MathLib.Quaternion;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Multiverse.CollisionLib;

#endregion

namespace Multiverse.CollisionLib
{

    public class MovingPart
	{
		public CollisionShape shape;
		public CollisionShape constantShape;
		public int id;
		public CollisionAPI api;
        List<RenderedNode> renderedNodes;
        // The last containment sphere in which this moving object
        // was located
        public SphereTreeNode sphere;

		public MovingPart(CollisionAPI api, CollisionShape shape)
		{
			this.shape = shape;
			this.api = api;
            constantShape = shape;
            id = api.SphereTree.GetId();
            renderedNodes = null;
			RenderedNode.MaybeCreateRenderedNodes(false, shape, id, ref renderedNodes);
			sphere = null;
		}

		public Vector3 Center() 
        {
            return shape.center;
        }
        
        public void ChangeRendering(bool render)
		{
			RenderedNode.ChangeRendering(render, shape, id, ref renderedNodes);
		}

		public void RemoveNodeRendering()
		{
			RenderedNode.RemoveNodeRendering(id, ref renderedNodes);
		}
		
		public void AddDisplacement(Vector3 displacement)
		{
			shape.AddDisplacement(displacement);
			RenderedNode.AddDisplacement(displacement, ref renderedNodes);
		}

        public void Transform(Vector3 scale, Quaternion rotate, Vector3 translate) {
            // TODO: This is a temporary solution, and terribly inefficient.
            // I need to update the transforms on the node properly, without
            // the remove/add.
            RenderedNode.RemoveNodeRendering(id, ref renderedNodes);
            shape = constantShape.Clone();
            shape.Transform(scale, rotate, translate);
            id = api.SphereTree.GetId();
            renderedNodes = null;
            RenderedNode.MaybeCreateRenderedNodes(false, shape, id, ref renderedNodes);
            sphere = null;
        }
	}
	
	// This class provides the record of the state of a moving
    // object, i.e., an avatar or vehicle or the like, maintained
    // between checks on collisions.  Any information that the
    // collision algorithm wants to cache is cached here.
    public class MovingObject
    {
        // The parts that make up the moving object.  In many cases, there
        // will be just one shape; a capsule
        public List<MovingPart> parts;
        // How much can the moving object change its Z coord without jumping?
        public float zChange;
        // How high can the shape jump?  (Does this belong here?)
        public float jumpHeight;
		// The list of rendered nodes for these parts
		public List<RenderedNode> renderedNodes;

        public CollisionAPI api;
		
        public MovingObject(CollisionAPI api)
        {
            this.api = api;
            parts = new List<MovingPart>();
			renderedNodes = null;
			movingObjects.Add(this);
		}

		public void AddPart(CollisionShape part)
		{
			parts.Add(new MovingPart(api, part));
		}
		
		public int PartCount {
            get
            {
                return parts.Count;
            }
        }
        
        public void AddDisplacementToRenderedNodes(Vector3 displacement)
		{
			RenderedNode.AddDisplacement(displacement, ref renderedNodes);
		}
		
		public void AddDisplacement(Vector3 displacement)
		{
			foreach(MovingPart part in parts)
				part.AddDisplacement(displacement);
		}
		
		public float StepSize(Vector3 displacement)
		{
			float step = float.MaxValue;
			foreach (MovingPart part in parts)
				step = Math.Min(step, part.shape.StepSize(displacement));
			return step;
		}
		
		public void Dispose()
		{
			foreach(MovingObject mo in movingObjects) {
				foreach(MovingPart part in mo.parts) {
					part.RemoveNodeRendering();
                }
            }
		}

        public void Transform(Vector3 scale, Quaternion rotate, Vector3 translate) {
    		foreach(MovingPart part in parts)
                part.Transform(scale, rotate, translate);
        }

        public Vector3 Center() 
        {
            // Return the center of the first part
            foreach(MovingPart part in parts)
                return part.Center();
            return Vector3.Zero;
        }

		public static List<MovingObject> movingObjects = new List<MovingObject>();


		public static void ChangeRendering(bool render)
		{
			foreach(MovingObject mo in movingObjects) {
				foreach(MovingPart part in mo.parts) {
					part.ChangeRendering(render);
				}
			}
		}
    }
	
}

