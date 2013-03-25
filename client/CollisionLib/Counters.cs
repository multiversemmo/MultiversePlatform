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

#define USE_PERFORMANCE_COUNTERS

#region Using directives

using System;
using Vector3 = Axiom.MathLib.Vector3;
using Matrix3 = Axiom.MathLib.Matrix3;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Multiverse.CollisionLib;

#endregion

namespace Multiverse.CollisionLib
{

    public class Counters 
    {
        
#if USE_PERFORMANCE_COUNTERS

// Sphere Tree Counters
public static System.Diagnostics.PerformanceCounter nodeCounter;
public static System.Diagnostics.PerformanceCounter shapesAddedCounter;
public static System.Diagnostics.PerformanceCounter shapesRemovedCounter;
public static System.Diagnostics.PerformanceCounter intersectingShapeCounter;

// Collision API Counters
public static System.Diagnostics.PerformanceCounter topLevelCallsCounter;
public static System.Diagnostics.PerformanceCounter partCallsCounter;
public static System.Diagnostics.PerformanceCounter topLevelCollisionsCounter;
public static System.Diagnostics.PerformanceCounter collisionTestCounter;

protected void SetupPerformanceCategories() {
    PerformanceCounterCategory category = null;

    PerformanceCounterCategory[] categories = PerformanceCounterCategory.GetCategories();
    for (int i = 0; i < categories.Length; ++i) {
        if (categories[i].CategoryName == "Multiverse CollisionLib") {
            category = categories[i];
            break;
        }
    }
    CounterCreationDataCollection ccdc = PrepareCounterCollection();
    // Make sure we have all the appropriate counters
    if (category != null) {
        foreach (CounterCreationData ccd in ccdc) {
            if (!category.CounterExists(ccd.CounterName)) {
                PerformanceCounterCategory.Delete(category.CategoryName);
                category = null;
                break;
            }
        }
    }
    if (category == null) {
        // Create the category.
        PerformanceCounterCategory.Create("Multiverse CollisionLib",
                                          "Monitors the Multiverse CollisionLib.",
                                          PerformanceCounterCategoryType.SingleInstance,
                                          ccdc);
    }
}

public void GrabCounters(CollisionAPI api)
{
    nodeCounter.RawValue = api.SphereTree.nodeCount;
    shapesAddedCounter.RawValue = api.SphereTree.shapesAdded;
    shapesRemovedCounter.RawValue = api.SphereTree.shapesRemoved;
    intersectingShapeCounter.RawValue = api.SphereTree.intersectingShapeCount;
    
    topLevelCallsCounter.RawValue = api.topLevelCalls;
    partCallsCounter.RawValue = api.partCalls;
    topLevelCollisionsCounter.RawValue = api.topLevelCollisions;
    collisionTestCounter.RawValue = api.collisionTestCount;
}
 

protected PerformanceCounter GetCounter(string counterName) {
    return new PerformanceCounter("Multiverse Performance", counterName, false);
}

protected void CreateCounters() {
    // Sphere tree counters
    nodeCounter = GetCounter("node count");
    shapesAddedCounter = GetCounter("nodes added");
    shapesRemovedCounter = GetCounter("nodes removed");
    intersectingShapeCounter = GetCounter("intersecting shapes");

    topLevelCallsCounter = GetCounter("top-level calls");
    partCallsCounter = GetCounter("part calls");
    topLevelCollisionsCounter = GetCounter("collisions");
    collisionTestCounter = GetCounter("collision tests");
}

protected CounterCreationDataCollection PrepareCounterCollection() {
    CounterCreationDataCollection ccdc = new CounterCreationDataCollection();
    CounterCreationData counter;

    ////////////////////////////////////////////////////////////////////////
    // Counters for SphereTreeNodes
    ////////////////////////////////////////////////////////////////////////
    
    // Add the counter for sphere tree node count
    counter = new CounterCreationData();
    counter.CounterType = PerformanceCounterType.NumberOfItems32;
    counter.CounterName = "node count";
    ccdc.Add(counter);

    // Add the counter for shapes added from sphere tree
    counter = new CounterCreationData();
    counter.CounterType = PerformanceCounterType.NumberOfItems32;
    counter.CounterName = "nodes added";
    ccdc.Add(counter);

    // Add the counter for shapes removed from sphere tree
    counter = new CounterCreationData();
    counter.CounterType = PerformanceCounterType.NumberOfItems32;
    counter.CounterName = "nodes removed";
    ccdc.Add(counter);

    // Add the counter for intersecting shapes in the sphere tree
    counter = new CounterCreationData();
    counter.CounterType = PerformanceCounterType.NumberOfItems32;
    counter.CounterName = "intersecting shapes";
    ccdc.Add(counter);

    ////////////////////////////////////////////////////////////////////////
    // Counters for collision tests
    ////////////////////////////////////////////////////////////////////////

    // Add the counter for top-level calls to collision apparatus
    counter = new CounterCreationData();
    counter.CounterType = PerformanceCounterType.NumberOfItems32;
    counter.CounterName = "top-level calls";
    ccdc.Add(counter);

    // Add the counter for per-part calls to the collision apparatus
    counter = new CounterCreationData();
    counter.CounterType = PerformanceCounterType.NumberOfItems32;
    counter.CounterName = "part calls";
    ccdc.Add(counter);

    // Add the counter for top-level collisions
    counter = new CounterCreationData();
    counter.CounterType = PerformanceCounterType.NumberOfItems32;
    counter.CounterName = "collisions";
    ccdc.Add(counter);

    // Add the counter for shape-shape collision tests
    counter = new CounterCreationData();
    counter.CounterType = PerformanceCounterType.NumberOfItems32;
    counter.CounterName = "collision tests";
    ccdc.Add(counter);

    return ccdc;
}
 
#endif

}}


