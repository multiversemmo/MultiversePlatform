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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Multiverse.CollisionLib;

#endregion

namespace Multiverse.CollisionLib
{

public class SphereTree {

    public SphereTreeNode root;

    // Flags to control verification and dumping
    private bool VerifyEveryChange = false;
    
	private void MaybeVerifyOrDump ()
	{
		if (VerifyEveryChange)
            Verify();
	}
    
    public int nodeCount = 0;
    public int idCounter = 0;
    public int shapesAdded = 0;
    public int shapesRemoved = 0;
    public int intersectingShapeCount = 0;

    public string SphereTreeCounters()
	{
		return string.Format("nodeCount {0}, shapesAdded {1}, shapesRemoved {2}, intersectingShapeCount {3}",
							 nodeCount, shapesAdded, shapesRemoved, intersectingShapeCount);
	}
	
	public void Initialize()
    {
        root = new SphereTreeNode(this);

        nodeCount = 1;
        shapesAdded = 0;
        shapesRemoved = 0;
        intersectingShapeCount = 0;
    }

    public void Verify()
    {
        root.VerifySphereTree();
    }
        
    public void DumpSphereTree(StreamWriter writer)
    {
        writer.Write(string.Format("nodeCount {0}, shapesAdded {1}, shapesRemoved {2}, intersectingShapeCount {3}\n",
                                   nodeCount, shapesAdded, shapesRemoved, intersectingShapeCount));
        root.DumpSphereTreeInternal(writer, 0);
    }
#if NOT
    public void DumpTree()
    {
        FileStream f = new FileStream("../DumpSphereTree.txt",
                                      FileMode.Create, FileAccess.Write);
        StreamWriter writer = new StreamWriter(f);
        DumpSphereTree(writer);
        writer.Close();
    }
#endif
    
	// private string lastLoggedContainer = "";

    public SphereTreeNode FindSmallestContainer(CollisionShape shape, 
                                                SphereTreeNode lastSphere)
    {
//      if (lastSphere != null) {
//          if (lastSphere.Contains(shape))
//              return lastSphere.FindSubsphereContainer(shape);
//          else {
//              // Move up the parent chain
//              while (lastSphere != this) {
//                  lastSphere = lastSphere.parent;
//                  Debug.Assert(lastSphere != null, "Parent is null!");
//                  Debug.Assert(!lastSphere.leafNode);
//                  if (lastSphere.Contains(shape)) {
//                      if (MO.DoLog)
//                          MO.Log(" Container {0} for shape {1}", lastSphere, shape);
//                      return lastSphere;
//                  }
//              }
//              Debug.Assert(false, "Didn't find container!");
//          }
//      }
        SphereTreeNode container = root.FindSubsphereContainer(shape);
// 		if (MO.DoLog) {
// 			string s = string.Format(" Returning smallest subsphere container {0} for shape {1}",
// 									 container, shape);
// 			if (lastLoggedContainer != s) {
// 				lastLoggedContainer = s;
// 				MO.Log(s);
// 			}
// 		}
        return container;
	}

    public void AddCollisionShape(CollisionShape shape)
    {
        // Called with this equal to the root of the tree
        shapesAdded++;
        SphereTreeNode s = new SphereTreeNode(shape, this);
        if (MO.DoLog)
            MO.Log("Adding shape {0} to leaf {1}", shape, s);
        root.InsertSphereTreeNode(s);
        root.AddToIntersectingShapes(shape);
        Debug.Assert(shapesAdded - shapesRemoved <= nodeCount, 
                     "shapesAdded - shapesRemoved > nodeCount!");
        MaybeVerifyOrDump();
    }
    
    public int RemoveCollisionShapesWithHandle(long handle)
    {
        if (MO.DoLog) {
            MO.Log("Starting removal of shapes with handle {0}", MO.HandleString(handle));
			MO.Log(" Before removal {0}", SphereTreeCounters());
        }
		int removeCount = root.RemoveCollisionShapesWithHandleInternal(handle);
        if (MO.DoLog) {
			MaybeVerifyOrDump();
			MO.Log(" After removal {0}", SphereTreeCounters());
		}
		return removeCount;
    }
    
    public List<CollisionShape> GetCollisionShapesWithHandle(long handle)
    {
        List<CollisionShape> shapes = new List<CollisionShape>();
        if (MO.DoLog) {
            MO.Log("Starting to get shapes with handle {0}", MO.HandleString(handle));
        }
		root.GetCollisionShapesWithHandleInternal(handle, shapes);
		return shapes;
    }
    
	public void ChangeRendering(bool render)
	{
		root.ChangeRendering(render);
	}
	
	public int GetId()
	{
        idCounter++;
        return idCounter;
	}

}

// This class is a node in the sphere hierarchy.
public class SphereTreeNode : BasicSphere {

#region Private Data And Methods

    // For, we'll make it a binary tree
    private const int childCount = 2;

    // Since the address isn't stable . . .
    private int id;
    
	// I look up to my parent.  Used to short-circuit a top-down
    // search.
    private SphereTreeNode parent;
    
    // For leaf nodes, the shape contained in the leaf.  For non-leaf
    // nodes, always null
    private CollisionShape containedShape;

    // The list of render objects created for a leaf when
    // renderedCollisionShapes is true
	private List<RenderedNode> renderedNodes;
	
	// If an obstacle intersects my sphere, but isn't wholly contained
    // in my sphere, it goes in the intersectingShapes list
    private List<CollisionShape> intersectingShapes;

    // The spheres directly contained by this sphere
    private SphereTreeNode[] children;

    private bool leafNode;
    
    private SphereTree sphereTree;
    
    // Create a SphereTreeNode leaf node
    public SphereTreeNode(CollisionShape shape, SphereTree sphereTree)
        : base(shape.center, shape.radius) {
        this.sphereTree = sphereTree;
        this.containedShape = shape;
        parent = null;
        children = new SphereTreeNode[childCount];
        intersectingShapes = new List<CollisionShape>();
        leafNode = true;
        sphereTree.idCounter++;
        id = sphereTree.idCounter;
        sphereTree.nodeCount++;
		RenderedNode.MaybeCreateRenderedNodes(true, shape, id, ref renderedNodes);
	}

    // This constructor for a non-leaf node
    public SphereTreeNode(SphereTree sphereTree) 
        : base(Vector3.Zero, 0) {
        this.sphereTree = sphereTree;
        parent = null;
        children = new SphereTreeNode[childCount];
        containedShape = null;
        intersectingShapes = new List<CollisionShape>();
        leafNode = false;
        sphereTree.idCounter++;
        id = sphereTree.idCounter;
        sphereTree.nodeCount++;
    }

    // This constructor for the top node
    public SphereTreeNode(Vector3 center, float radius, SphereTree sphereTree) 
        : base(center, radius) {
        this.sphereTree = sphereTree;
        parent = null;
        children = new SphereTreeNode[childCount];
        containedShape = null;
        intersectingShapes = new List<CollisionShape>();
        leafNode = false;
        sphereTree.idCounter++;
        id = sphereTree.idCounter;
        sphereTree.nodeCount++;
    }

    public SphereTreeNode FindSubsphereContainer(CollisionShape shape)
    {
        SphereTreeNode container = FindSubsphereContainerInternal(shape);
        if (container == null)
            return sphereTree.root;
        else
            return container;
    }
    
    public SphereTreeNode FindSubsphereContainerInternal(CollisionShape shape)
    {
        // If more than one child contains shape, return this
        int count = 0;
        SphereTreeNode container = null;
        for (int i=0; i<childCount; i++) {
            SphereTreeNode child = children[i];
            if (child != null && child.Contains(shape)) {
                count++;
                container = child;
            }
        }
        if (count > 1)
            return this;
        else if (count == 0)
            return null;
        else {
            container = container.FindSubsphereContainer(shape);
            SphereTreeNode result = (container == null ? this : container);
            return result;
        }
    }

    private void AdjustCenterAndRadius(SphereTreeNode s)
    {
        // Don't adjust the root node
        if (parent == null)
            return;
        if (radius == 0) {
            center = s.center;
            radius = s.radius;
            return;
        }
        // Adjust the center and radius of this sphere do it
        // encompasses s
        Vector3 diff = (s.center - center);
        float sqDist = diff.Dot(diff);
        float rDiff = s.radius - radius;
        if (rDiff * rDiff >= sqDist) {
            // One is contained in the other
            if (s.radius >= radius) {
                center = s.center;
                radius = s.radius;
            }
        }
        else {
            float dist = (float)Math.Sqrt(sqDist);
            float oldRadius = radius;
            radius  = (dist + radius + s.radius) * 0.5f;
            if (dist > Primitives.epsilon)
                center += ((radius - oldRadius) / dist) * diff;
        }
    }
    
    private void RecalculateCenterAndRadius()
    {
        // Don't adjust the root node
        if (parent == null)
            return;
        bool foundOne = false;
        for (int i=0; i<childCount; i++) {
            SphereTreeNode child = children[i];
            if (child != null) {
                if (foundOne) {
                    AdjustCenterAndRadius(child);
                }
                else {
                    foundOne = true;
                    center = child.center;
                    radius = child.radius;
                }
            }
        }
    }
    
    public void InsertSphereTreeNode(SphereTreeNode s) 
    {
        Debug.Assert(s.leafNode);
        if (leafNode) {
            CombineLeafNodes (s);
            parent.RecalculateCenterAndRadius();
            return;
        }
        
        SphereTreeNode child = null;
        for (int i=0; i<childCount; i++) {
            child = children[i];
            if (child != null && child.Contains(s)) {
                child.InsertSphereTreeNode(s);
                RecalculateCenterAndRadius();
                return;
            }
        }

        // Not contained in any child, so create it here
        bool inserted = false;
        for (int i=0; i<childCount; i++) {
            if (children[i] == null) {
                s.parent = this;
                children[i] = s;
                inserted = true;
                if (MO.DoLog)
                    MO.Log(" Inserted sphere {0} in parent {1}", s, this);
                break;
            }
        }
        if (!inserted)
            child = FindChildToDemote(s);
        RecalculateCenterAndRadius();
    }

    private SphereTreeNode CombineLeafNodes(SphereTreeNode s)
    {
        // Remember my own parent, and remove me from that parent
        if (MO.DoLog)
            MO.Log(" Combined {0} with {1}", this, s);
        SphereTreeNode myparent = parent;
        Debug.Assert(myparent != null, "parent was null!");
        parent.RemoveChild(this);
        // Make a new non-leaf node node to hold both
        SphereTreeNode n = new SphereTreeNode(center, radius, sphereTree);
        myparent.AddChild(n);
        n.AddChild(this);
        n.AddChild(s);
        n.RecalculateCenterAndRadius();
        if (MO.DoLog)
            MO.Log(" Made combined node {0} child of {1}", n, myparent);
        return n;
    }
    
    private SphereTreeNode FindChildToDemote(SphereTreeNode s) 
    {
        if (MO.DoLog)
            MO.Log(" Demote this {0} s {1}", this, s);
        Debug.Assert(s.leafNode, "s isn't a leaf node");
        // All children are occupied, and s is not wholly contained in
        // any of them.  Combine s with some child, creating a
        // subordinate.  Choose the child whose center is nearest
        // s.center.  If the child has a larger radius, insert s in
        // the child.  If the child has a smaller radius, s becomes
        // the child, and the child is inserted in s.
        float closest = float.MaxValue;
        int closestChild = -1;
        for (int i=0; i<childCount; i++) {
            SphereTreeNode child = children[i];
            Debug.Assert(child != null, "Null child!");
            float d = Primitives.DistanceSquared(s.center, child.center);
            if (d < closest) {
                closest = d;
                closestChild = i;
            }
        }
        Debug.Assert (closestChild >= 0, "closestChild not found!");
        SphereTreeNode cs = children[closestChild];
        if (cs.leafNode) {
            if (MO.DoLog)
                MO.Log(" Calling CombineLeafNodes to combine {0} with {1}", cs, this);
            SphereTreeNode n = cs.CombineLeafNodes(s);
            children[closestChild] = n;
            return n;
        }
        else if (cs.ChildSlotsFree() > 0) {
            cs.AddChild(s);
            cs.RecalculateCenterAndRadius();
            return cs;
        }
        else {
            SphereTreeNode n = new SphereTreeNode(sphereTree);
            RemoveChild(cs);
            AddChild(n);
            n.AddChild(cs);
            n.AddChild(s);
            n.RecalculateCenterAndRadius();
            if (MO.DoLog)
                MO.Log(" In demote, made new node n {0}", n);
            return n;
        }
    }
    
    private int ChildSlotsFree()
    {
        int count = 0;
        for (int i=0; i<childCount; i++) {
            if (children[i] == null)
                count++;
        }
        return count;
    }
    
    public void AddToIntersectingShapes(CollisionShape shape)
	{
		if (shape == containedShape || !SphereOverlap(shape))
			return;
		for (int i=0; i<childCount; i++) {
			SphereTreeNode child = children[i];
			if (child != null) {
				if (child.Contains(shape)) {
					child.AddToIntersectingShapes(shape);
					return;
				}
			}
		}
		// Not wholly contained in a child so add it to our list
		AddIntersectingShape(shape);
		// Add it to any child that overlaps
		AddToChildIntersectingShapes(shape);
	}
	
	private void AddToChildIntersectingShapes (CollisionShape shape)
    {
        if (shape == containedShape)
			return;
		for (int i=0; i<childCount; i++) {
            SphereTreeNode child = children[i];
            if (child != null && child.SphereOverlap(shape)) {
                child.AddIntersectingShape(shape);
                child.AddToChildIntersectingShapes(shape);
            }
        }
    }
    
    private void AddIntersectingShape (CollisionShape shape)
	{
		if (MO.DoLog)
			MO.Log(" Adding shape {0} to intersecting shapes of {1}",
				   shape, this);
		intersectingShapes.Add(shape);
		sphereTree.intersectingShapeCount++;
	}
	
	private int CountChildren ()
    {
        int count = 0;
        for (int i=0; i<childCount; i++) {
            SphereTreeNode child = children[i];
            if (child != null)
                count++;
        }
        return count;
    }
    
    public int RemoveCollisionShapesWithHandleInternal(long handle)
    {
        // The dumb version: iterate over all containment spheres
        // looking for obstacles with the given handle, returning true
        // if a child changed.  (The smarter version would maintain an
        // index mapping handles to SphereTreeNodes whose
        // containedShape or intersectingShapes have a shape with
        // that handle.)
        //
        // This runs bottom-up, to provide an opportunity to coalesce
        // the sphere tree.

        // intersectingShapes are easy - - just remove if they have a
        // matching handle, since their removal doesn't change the tree
        int removeCount = 0;
        for (int i=0; i<intersectingShapes.Count; i++) {
            CollisionShape obstacle = intersectingShapes[i];
            if (obstacle.handle == handle) {
                if (MO.DoLog)
                    MO.Log(" Removing intersecting shape {0} of {1}", obstacle, this);
                intersectingShapes.RemoveAt(i);
                i--;
                sphereTree.intersectingShapeCount--;
                Debug.Assert(sphereTree.intersectingShapeCount >= 0, "intersectingShapeCount < 0!");
            }
        }
        
        // Now handle the children
        for (int i=0; i<childCount; i++) {
            SphereTreeNode child = children[i];
            if (child != null) {
                if (child.leafNode && child.containedShape.handle == handle) {
                    if (MO.DoLog)
                        MO.Log(" Removing child leaf {0} of {1}", child, this);
                    children[i] = null;
                    child.parent = null;
                    RenderedNode.RemoveNodeRendering(child.id, ref child.renderedNodes);
					sphereTree.shapesRemoved++;
                    sphereTree.nodeCount--;
                    removeCount++;
                }
                else if (!child.leafNode) {
                    removeCount += child.RemoveCollisionShapesWithHandleInternal(handle);
                }
            }
        }
        
        int count = CountChildren();
        if (count == 0 && containedShape == null) {
            // This is now a truly pointless node - - remove from its
            // parent.  Parent will only be null for the root node
            if (parent != null) {
                if (MO.DoLog)
                    MO.Log(" Removing branch with no kids {0} of {1}", this, parent);
                parent.RemoveChild(this);
                sphereTree.nodeCount--;
                Debug.Assert(sphereTree.nodeCount>0, "nodeCount <= 0");
                return removeCount;
            }
        }
        else if (count == 1) {
            if (parent != null) {
                // Make my child the child of my parent
                if (MO.DoLog)
                    MO.Log(" Replacing 1-child node {0} in parent {1}", this, parent);
                parent.ReplaceChild(this, FindFirstChild());
                sphereTree.nodeCount--;
                Debug.Assert(sphereTree.nodeCount>0, "nodeCount <= 0");
                return removeCount;
            }
        }
        if (removeCount > 0)
            RecalculateCenterAndRadius();
        return removeCount;
    }

    private void AddChild(SphereTreeNode s)
    {
        for (int i=0; i<childCount; i++) {
            if (children[i] == null) {
                if (MO.DoLog)
                    MO.Log(" Adding child {0} to parent {1}", s, this);
                children[i] = s;
                s.parent = this;
                return;
            }
        }
        Debug.Assert(false, "No free slot to add child");
    }
    
    private void RemoveChild(SphereTreeNode s)
    {
        ReplaceChild(s, null);
    }
    
    private void ReplaceChild(SphereTreeNode currentChild, SphereTreeNode newChild)
    {
        if (MO.DoLog) {
            if (newChild == null)
                MO.Log(" Replacing child {0} with null", currentChild);
            else
                MO.Log(" Replacing child {0} with child {1}", currentChild, newChild);
        }
        for (int i=0; i<childCount; i++) {
            SphereTreeNode child = children[i];
            if (child == currentChild) {
                children[i] = newChild;
                if (newChild != null)
                    newChild.parent = this;
                currentChild.parent = null;
                return;
            }
        }
        Debug.Assert(false, "Didn't find child");
    }
    
    private SphereTreeNode FindFirstChild ()
    {
        for (int i=0; i<childCount; i++) {
            SphereTreeNode child = children[i];
            if (child != null) {
                return child;
            }
        }
        Debug.Assert(false, "Didn't find child!");
        return null;
    }
    
    private void DoIndentation(StreamWriter writer, int level)
    {
        // do carriage return and indentation
        //stream.Write("\n");
        for (int i=0; i<level; i++) {
            writer.Write("  ");
        }
    }
    
    private string StringIntersectingShapes()
    {
        if (intersectingShapes.Count == 0)
			return "";
		string s = ", intersecting(";
        int i = 0;
        foreach (CollisionShape obstacle in intersectingShapes) {
            if (i > 0)
                s += ", ";
            string h = string.Format("{0:X}", obstacle);
            s += h;
        }
        s += ")";
        return s;
    }
    
    public void GetCollisionShapesWithHandleInternal(long handle, List<CollisionShape> shapes)
    {
        for (int i=0; i<childCount; i++) {
            SphereTreeNode child = children[i];
            if (child != null) {
                if (child.leafNode && child.containedShape.handle == handle)
                    shapes.Add(child.containedShape);
                else if (!child.leafNode)
                    child.GetCollisionShapesWithHandleInternal(handle, shapes);
            }
        }
    }

    public void DumpSphereTreeInternal(StreamWriter writer, int level)
    {
        DoIndentation(writer, level);
        writer.Write(ToString() + "\n");
        for (int i=0; i<childCount; i++) {
            SphereTreeNode child = children[i];
            if (child != null) {
                child.DumpSphereTreeInternal(writer, level+1);
            }
        }
    }
    
    public void VerifySphereTree()
    {
        if (leafNode) {
            Debug.Assert(CountChildren() == 0, "leaf child count non-zero");
            Debug.Assert(containedShape != null, "leaf shape null");
            Debug.Assert(Contains(containedShape), "leaf doesn't contain shape");
        }
        else {
            int count = CountChildren();
            if (parent != null)  // Don't make the test for the root node
                    Debug.Assert(count > 0, "non-leaf with no children");
            for (int i=0; i<childCount; i++) {
                SphereTreeNode child = children[i];
                if (child != null) {
                    Debug.Assert(Contains(child), "non-leaf doesn't contain child");
                    Debug.Assert(child.parent == this, "child doesn't point to parent");
                    if (!child.leafNode && count == 1)
                        Debug.Assert(child.CountChildren() > 1, "this node has only one child, which has only one child");
                    child.VerifySphereTree();
                }
            }
        }
    }

	public void ChangeRendering(bool render)
	{
		if (leafNode)
			RenderedNode.ChangeRendering(render, containedShape, id, ref renderedNodes);
		else {
            for (int i=0; i<childCount; i++) {
                SphereTreeNode child = children[i];
                if (child != null)
				    child.ChangeRendering(render);
			}
		}
	}

#endregion Private Data And Methods

#region Public Methods

    public override string ToString()
    {
        if (leafNode)
            return string.Format("(Leaf {0}:{1}, id {2}, shape {3}{4})",
                                 MO.MeterString(center), MO.MeterString(radius), id,
                                 (containedShape == null ? "null" : containedShape.ToString()),
                                 StringIntersectingShapes());
        else
            return string.Format("(Non-leaf {0}:{1}, id {2}, kids {3}{4})",
                                 MO.MeterString(center), MO.MeterString(radius), id,
                                 CountChildren(),
                                 StringIntersectingShapes());
    }
    
    // Test for collision with any object in this sphere
    public bool TestSphereCollision(CollisionShape part, ulong timeStamp, 
                                    ref int collisionTestCount, CollisionParms parms)
    {
        if (containedShape != null) {
            collisionTestCount++;
            parms.swapped = false;
            if (Primitives.TestCollisionFunctions[(int)part.ShapeType(), 
                                                  (int)containedShape.ShapeType()]
                (part, containedShape, parms)) {
                parms.part = part;
                parms.obstacle = containedShape;
                return true;
            } 
            else {
                containedShape.timeStamp = timeStamp;
            }
        }
        // Iterate over the shapes in this sphere and not wholly
        // contained in a subsphere
		foreach (CollisionShape obstacle in intersectingShapes) {
            if (obstacle.timeStamp == timeStamp)
                continue;
            collisionTestCount++;
            parms.swapped = false;
            if (Primitives.TestCollisionFunctions[(int)part.ShapeType(), (int)obstacle.ShapeType()]
                (part, obstacle, parms)) {
                parms.part = part;
                parms.obstacle = obstacle;
                return true;
            } 
            else {
                obstacle.timeStamp = timeStamp;
            }
        }
        // Now iterate over subspheres
        for (int i=0; i<SphereTreeNode.childCount; i++) {
            SphereTreeNode cs = children[i];
            if (cs == null)
                continue;
            // Skip any sphere that doesn't overlap the part in question
            if (!cs.SphereOverlap(part))
                continue;
            if (cs.TestSphereCollision(part, timeStamp, ref collisionTestCount, parms))
                return true;
        }
        return false;
    }

	// Test for collision with any object in this sphere
    public void FindIntersectingShapes(CollisionShape part,
									   List<CollisionShape> shapes,
									   ulong timeStamp, 
									   ref int collisionTestCount, 
									   CollisionParms parms)
    {
        if (containedShape != null) {
            collisionTestCount++;
            parms.swapped = false;
            if (Primitives.TestCollisionFunctions[(int)part.ShapeType(), 
                                                  (int)containedShape.ShapeType()]
                (part, containedShape, parms)) {
				shapes.Add(containedShape);
			} 
            else {
                containedShape.timeStamp = timeStamp;
            }
        }
        // Iterate over the shapes in this sphere and not wholly
        // contained in a subsphere
		foreach (CollisionShape obstacle in intersectingShapes) {
            if (obstacle.timeStamp == timeStamp)
                continue;
            collisionTestCount++;
            parms.swapped = false;
            if (Primitives.TestCollisionFunctions[(int)part.ShapeType(), (int)obstacle.ShapeType()]
                (part, obstacle, parms)) {
				shapes.Add(obstacle);
			} 
            else {
                obstacle.timeStamp = timeStamp;
            }
        }
        // Now iterate over subspheres
        for (int i=0; i<SphereTreeNode.childCount; i++) {
            SphereTreeNode cs = children[i];
            if (cs == null)
                continue;
            // Skip any sphere that doesn't overlap the part in question
            if (!cs.SphereOverlap(part))
                continue;
            cs.FindIntersectingShapes(part, shapes, timeStamp, ref collisionTestCount, parms);
        }
    }

#endregion Public Methods

}}

