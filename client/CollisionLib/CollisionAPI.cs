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
using Axiom.Core;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Multiverse.Config;
using Multiverse.CollisionLib;

#endregion

namespace Multiverse.CollisionLib
{

    public class CollisionAPI
    {


public int topLevelCalls;
public int partCalls;
public int topLevelCollisions;
public int collisionTestCount;

private SphereTree sphereTree;

#region Advertised CollisionAPI Parameters

// For big moving objects, we step til we're less than this number of
// inches from the obstacle
public static float MinInchesToObstacle = 6.0f;

// For small moving objects, we step til we're within this fraction of
// the smallest part's step size
public static float MinFractionToObstacle = 0.1f;

// If we're within this many meters of the terrain, then we
// assume we can adjust to the terrain level
public static float VerticalTerrainThreshold = .01f;

// We assume we can hop over any obstacle that is less than
// this many meters high without the user jumping
public static float HopOverThreshold = .75f;
		
// The angle of the norm of the obstacle with the desired
// displacement must be within this amount of 90 degrees.  The
// default is 45 degrees
public static float MinSlideAngle = 45f * (float)Math.PI / 180.0f;

// How steep can an avatar climb?  The default is 45 degrees off horizontal
public static float MaxVerticalAngle = (180f - 45f) * (float)Math.PI / 180.0f;

private bool setParameterHandler(string parameterName, string parameterValue)
{
	float value = 0f;
	try {
		value = float.Parse(parameterValue);
	}
	catch(Exception) {
		return false;
	}
	switch (parameterName) {
	case "MinInchesToObstacle":
		MinInchesToObstacle = value;
		break;
	case "MinFractionToObstacle":
		MinFractionToObstacle = value;
		break;
	case "VerticalTerrainThreshold":
		VerticalTerrainThreshold = value;
		break;
	case "HopOverThreshold":
		HopOverThreshold = value;
		break;
	case "MinSlideAngle":
		MinSlideAngle = value;
		break;
	case "MaxVerticalAngle":
		MaxVerticalAngle = value;
		break;
	default:
		return false;
	}
	return true;
}

private bool getParameterHandler(string parameterName, out string parameterValue)
{
	float value = 0f;

	switch (parameterName) {
	case "Help":
		parameterValue = ParameterHelp();
        return true;
	case "MinInchesToObstacle":
		value = MinInchesToObstacle;
		break;
	case "MinFractionToObstacle":
		value = MinFractionToObstacle;
		break;
	case "VerticalTerrainThreshold":
		value = VerticalTerrainThreshold;
		break;
	case "HopOverThreshold":
		value = HopOverThreshold;
		break;
	case "MinSlideAngle":
		value = MinSlideAngle;
		break;
	case "MaxVerticalAngle":
		value = MaxVerticalAngle;
		break;
	default:
        parameterValue = "";
		return false;
	}
	parameterValue = value.ToString();
	return true;
}

private string ParameterHelp()
{
	return
		"float MinInchesToObstacle: For big moving objects, after a collision we step toward " +
		"the obstacle til we're less than this number of inches from the obstacle; default " +
		"is 6.0 inches " +
		"\n" +
		"float MinFractionToObstacle: For moving objects smaller than MinInchesToObstacle, " +
		"we step til we're within this fraction of the smallest part's step size; default " +
		"is .1" +
		"\n" +
		"float VerticalTerrainThreshold: A mob stops falling when it'swithin this many " +
		"meters of the terrain; the default is .01 meters " +
		"\n" +
		"float HopOverThreshold: We assume we can hop over any obstacle that is less than " +
		"this many meters high without the user jumping; the default is .5 meters " +
		"\n" +
		"float MinSlideAngle: The angle of the norm of the obstacle with the desired " +
		"displacement must be within this amount of PI/2 radians.  The default is PI/4 radians " +
		"\n" +
		"float MaxVerticalAngle: How steep can an avatar climb, measured in radians off the horizontal? " +
		"The default is PI/4 radians " +
		"\n";
}

#endregion Advertised CollisionAPI Parameters

// A timestamp, incremented every time we run the collision test
// against a CollisionShape, and stored in the CollisionShape if there
// was no intersection.  This allows us to avoid retesting a
// part/obstacle combination that we've already tested.
private ulong collisionTimeStamp;

// Test all the parts of a moving object to see if they collide with
// any obstacle
public bool CollideAfterAddingDisplacement(MovingObject mo, Vector3 step, CollisionParms parms)
{
	for (int i=0; i<mo.parts.Count; i++) {
		MovingPart movingPart = mo.parts[i];
		collisionTimeStamp++;
		movingPart.AddDisplacement(step);
		SphereTreeNode s = sphereTree.FindSmallestContainer(movingPart.shape, movingPart.sphere);
		//MaybeDumpSphereTree();
		movingPart.sphere = s;
		partCalls++;
		if (s.TestSphereCollision(movingPart.shape, collisionTimeStamp, ref collisionTestCount, parms)) {
			// We hit something, so back out the displacements
			// added to the parts tested so far
			for (int j=0; j<=i; j++) {
				MovingPart displacedPart = mo.parts[j];
				displacedPart.AddDisplacement (-step);
                //MaybeDumpSphereTree();
			}
			return true;
		}
	}
    return false;
}

public bool ShapeCollides(CollisionShape shape, CollisionParms parms)
{
	collisionTimeStamp++;
	SphereTreeNode s = sphereTree.FindSmallestContainer(shape, null);
	return s.TestSphereCollision(shape, collisionTimeStamp, ref collisionTestCount, parms);
}

public bool ShapeCollides(CollisionShape shape)
{
	CollisionParms parms = new CollisionParms();
	parms.genNormals = false;
	return ShapeCollides(shape, parms);
}

public bool PointInCollisionVolume(Vector3 p, long handle) {
    List<CollisionShape> shapes = sphereTree.GetCollisionShapesWithHandle(handle);
    if (shapes.Count > 0) {
        foreach (CollisionShape shape in shapes) {
            if (shape.PointInside(p))
                return true;
        }
    }
    return false;
}

private CollisionShape FindClosestIntersectingShape(List<CollisionShape> shapes,
													Vector3 start, Vector3 end,
													ref Vector3 intersection)
{
	intersection = Vector3.Zero;
	float closestDistance = float.MaxValue;
	CollisionShape closest = null;
	foreach (CollisionShape s in shapes) {
		float thisDistance = s.RayIntersectionDistance(start, end);
		if (thisDistance < closestDistance) {
			closestDistance = thisDistance;
			closest = s;
		}
	}
	if (closest != null)
		intersection = start + (end - start).ToNormalized() * closestDistance;
	return closest;
}

public CollisionShape FindClosestCollision(CollisionShape cap, Vector3 start, Vector3 end, 
                                           ref Vector3 intersection)
{
	collisionTimeStamp++;
	SphereTreeNode s = sphereTree.FindSmallestContainer(cap, null);
	CollisionParms parms = new CollisionParms();
	parms.genNormals = false;
    List<CollisionShape> shapes = new List<CollisionShape>();
	s.FindIntersectingShapes(cap, shapes, collisionTimeStamp, ref collisionTestCount, parms);
	intersection = Vector3.Zero;
	if (shapes.Count == 0)
		return null;
	else {
		collisionTimeStamp++;
		return FindClosestIntersectingShape(shapes, start, end, ref intersection);
	}
}

private bool StepTowardCollision(MovingObject mo, Vector3 displacement, 
								 int nsteps, Vector3 step, CollisionParms parms)
{
    Vector3 accumulatedSteps = Vector3.Zero;
	for (int i=0; i<nsteps; i++) {
		Vector3 nextStep;
		if (i == nsteps - 1)
			nextStep = displacement - accumulatedSteps;
		else
			nextStep = step;
		if (CollideAfterAddingDisplacement(mo, nextStep, parms)) {
			topLevelCollisions++;
			return true;
		}
		accumulatedSteps += nextStep;
	}
    // No collision!
    parms.obstacle = null;
    return false;
}
#if NOT
private void MaybeDumpSphereTree()
{
    sphereTree.DumpTree();
    sphereTree.Verify();
}
#endif
//////////////////////////////////////////////////////////////////////
//
// The external interface to the collision library
//
//////////////////////////////////////////////////////////////////////

public CollisionAPI (bool logCollisions) 
{
    if (logCollisions)
#if NOT
        MO.InitLog(true);
#else
        MO.DoLog = true;
#endif
	sphereTree = new SphereTree();
    sphereTree.Initialize();
    topLevelCalls = 0;     
    partCalls = 0;         
    topLevelCollisions = 0;
    collisionTestCount = 0;
	ParameterRegistry.RegisterSubsystemHandlers("CollisionAPI", setParameterHandler, getParameterHandler);
}
 
public SphereTree SphereTree {
    get { return sphereTree; }
}

~CollisionAPI()
{
	ParameterRegistry.UnregisterSubsystemHandlers("CollisionAPI");
}

public void ToggleRenderCollisionVolumes(SceneManager scene, bool movingObjects)
{
	RenderedNode.ToggleRenderCollisionVolumes(this, scene, movingObjects);
}


// The "global" method to add to the set of obstacles
public void AddCollisionShape(CollisionShape shape, long handle) {
    shape.handle = handle;
    sphereTree.AddCollisionShape(shape);
}

public int RemoveCollisionShapesWithHandle(long handle)
{
    return sphereTree.RemoveCollisionShapesWithHandle(handle);
}


// The "global" method to add to the set of parts in a moving
// object
public void AddPartToMovingObject(MovingObject mo, CollisionShape part) {
    mo.AddPart(part);
} 

// The is the top-level collision detection function.
//
// The MovingObject doesn't intersect anything now; would it
// do so if the vector displacement is applied?
//
// If we do have a collision, we "creep up" on the object we collide
// with until we're within stepsize of the minimum dimension of the
// moving part
//
public bool TestCollision(MovingObject mo, float stepSize, ref Vector3 displacement, CollisionParms parms)

{
    topLevelCalls++;
    parms.Initialize();
    if (mo.parts.Count == 0)
		return false;
	// Remember where the first part started
	Vector3 start = mo.parts[0].shape.center;
	Vector3 originalDisplacement = displacement;
	float len = displacement.Length;
    //MaybeDumpSphereTree();
    int nsteps = (int)Math.Ceiling(len / stepSize);
	Vector3 step = (len <= stepSize ? displacement : displacement * (stepSize/len));
	// Try to step the whole way
	if (!StepTowardCollision(mo, displacement, nsteps, step, parms)) {
		displacement = Vector3.Zero;
		return false;
	}
	// Otherwise, we hit something.  Step toward it
	// The minimum distance
	float smallStepSize = Math.Min(MO.InchesToMeters(MinInchesToObstacle),
								   stepSize * MinFractionToObstacle);
	len = step.Length;
	nsteps = (int)Math.Ceiling(len/smallStepSize);
	Vector3 smallStep = step * (smallStepSize/len);
	bool hit = StepTowardCollision(mo, step, nsteps, smallStep, parms);
	displacement = originalDisplacement - (mo.parts[0].shape.center - start);
	return hit;
}

public bool TestCollision(MovingObject mo, ref Vector3 displacement, CollisionParms parms)
{
	// Find the minimum step size for the assembly of parts
	float stepSize = mo.StepSize(displacement);
    return TestCollision(mo, stepSize, ref displacement, parms);
}

}}
