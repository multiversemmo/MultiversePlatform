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
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using log4net;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Collections;
using Axiom.Graphics;
using Multiverse.Network;
using Multiverse.Lib.LogUtil;

using TimeTool = Multiverse.Utility.TimeTool;

#endregion

namespace Multiverse.Base
{
    /// <summary>
    ///     Big nodes have perceptionRadii; little nodes don't
    ///     generation fo static geometry
    /// </summary>
    public enum StaticGeometryKind {
        BigNode = 1,
        LittleNode,
        BigOrLittleNode
    }

    /// <summary>
    ///     This class maintains the records necessary to drive the
    ///     generation fo static geometry
    /// </summary>
    public class StaticGeometryHelper {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(StaticGeometryHelper));

        protected WorldManager worldMgr;
        protected StaticGeometryKind kind;
        protected string name;
        protected StaticGeometry objectGeometry;
        protected int nodesAddedSinceLastRebuild;
        protected int nodesRemovedSinceLastRebuild;
        protected int lastNodesAdded;
        protected long lastRebuildCheckTime;
        protected long timeOfLastRebuild;
        protected bool enabled;
        protected bool force;
        // If enough nodes have changed in this many milliseconds,
        // we rebuild
        protected int rebuildTimeThreshold;
        // If this many nodes have been added, and the time
        // threshold has expired, we rebuild the geometry
        protected int nodesAddedThreshold;
        // If this many nodes have been added, and the time
        // threshold has expired, we rebuild the geometry
        protected int nodesRemovedThreshold;
        // We won't rebuild unless the nodes added in the last
        // second drops below this number
        protected int nodesAddedInLastSecondThreshold;
        // The set of static nodes from last time
        protected Dictionary<ObjectNode, int> lastStaticNodes = null;

        public StaticGeometryHelper(WorldManager worldMgr, StaticGeometryKind kind, int rebuildTimeThreshold, 
                                    int nodesAddedThreshold, int nodesRemovedThreshold, 
                                    int nodesAddedInLastSecondThreshold) {
            this.worldMgr = worldMgr;
            this.kind = kind;
            this.name = (kind == StaticGeometryKind.BigOrLittleNode ? "StaticGeom" : 
                (kind == StaticGeometryKind.BigNode ? "BigNodes" : "LittleNodes"));
            this.objectGeometry = null;
            this.rebuildTimeThreshold = rebuildTimeThreshold;
            this.nodesAddedThreshold = nodesAddedThreshold;
            this.nodesRemovedThreshold = nodesRemovedThreshold;
            this.nodesAddedInLastSecondThreshold = nodesAddedInLastSecondThreshold;
            this.nodesAddedSinceLastRebuild = 0;
            this.nodesRemovedSinceLastRebuild = 0;
            this.lastNodesAdded = 0;
            this.timeOfLastRebuild = 0;
            this.lastRebuildCheckTime = 0;
            this.enabled = false;
            this.force = false;
        }

        public void RebuildIfFinishedLoading(ExtensionMessage msg, bool loadingState) {
            if (!loadingState) {
                log.DebugFormat("StaticGeometryHelper.RebuildIfFinishedLoading: Rebuilding because loadingState is {0}", loadingState);
                Rebuild();
            }
        }
        
        public void NodeAdded(ObjectNode node) {
            if (RightKind(node))
                nodesAddedSinceLastRebuild++;
        }

        public void NodeRemoved(ObjectNode node) {
            if (RightKind(node))
                nodesRemovedSinceLastRebuild++;
        }

        public void RebuildIfNecessary(SceneManager mgr, Dictionary<long, WorldEntity> nodeDictionary) {
            if (TimeToRebuild())
                Rebuild();
        }

        public bool Enabled {
            set {
                enabled = value;
            }
        }

        public bool Force {
            set {
                force = value;
            }
        }

        protected bool RightKind(ObjectNode node) {
            return (kind == StaticGeometryKind.BigOrLittleNode ||
                    (node.PerceptionRadius == 0) == (kind == StaticGeometryKind.LittleNode)) 
                && (node.Entity == null || node.Entity.Mesh.Skeleton == null);
        }

        // Is it time to rebuild the static geometry?
        protected bool TimeToRebuild() {
            long thisTick = TimeTool.CurrentTime;
            // Only check every second, because this is how we get
            // the nodesAddedInLastSecondThreshold mechanism to work
            int timeSinceLastCheck = (int)(thisTick - lastRebuildCheckTime);
            if (timeSinceLastCheck < 1000)
                return false;
            lastRebuildCheckTime = thisTick;
            // If more nodes were added in the last second than
            // nodesAddedInLastSecondThreshold, don't rebuild yet.
            int recentNodesAdded = nodesAddedSinceLastRebuild - lastNodesAdded;
            lastNodesAdded = nodesAddedSinceLastRebuild;
            if (recentNodesAdded > nodesAddedInLastSecondThreshold)
                return false;
            return force ||
                   (enabled &&
                       ((nodesAddedSinceLastRebuild >= nodesAddedThreshold) || 
                        (nodesRemovedSinceLastRebuild >= nodesRemovedThreshold)) &&
                       (int)(thisTick - timeOfLastRebuild) >= rebuildTimeThreshold);
        }

        public void Rebuild() {
            Rebuild(worldMgr.SceneManager, worldMgr.NodeDictionary);
        }
        
        public class MaterialAndNodeCounts {
            public int materialUseCount = 0;
            public Dictionary<ObjectNode, int> submeshUseCounts = new Dictionary<ObjectNode, int>();
        }
        
        // Lock the node dictionary, and rebuild the static
        // geometry for objects of this kind
        protected void Rebuild(SceneManager mgr, Dictionary<long, WorldEntity> nodeDictionary) {
            log.DebugFormat("Entering StaticGeometryHelper.Rebuild for geometry '{0}'", name);
            long tickStart = TimeTool.CurrentTime;
            try {
                nodesAddedSinceLastRebuild = 0;
                nodesRemovedSinceLastRebuild = 0;
                force = false;
                Monitor.Enter(mgr);
                if (objectGeometry != null)
                    objectGeometry.Reset();
                else
                    objectGeometry = new StaticGeometry(mgr, name);
                // Dictionary mapping Material into a list of
                // ObjectNodes in which some submesh uses the material
                Dictionary<Material, MaterialAndNodeCounts> materialsUsedMap = new Dictionary<Material, MaterialAndNodeCounts>();
                lock(nodeDictionary) {
                    foreach (WorldEntity entity in nodeDictionary.Values) {
                        if (entity is ObjectNode) {
                            ObjectNode node = (ObjectNode)entity;
                            // For now, we only consider "props" that have an associated SceneNode
                            // and are direct descendants of the root scene node, and are of the right
                            // kind, i.e., don't have a perception radius if this static geometry is for
                            // little nodes, and vice versa.
//                             log.DebugFormat("StaticGeometry.Rebuild: Examining node {0}, oid {1}, type {2}, sceneNode {3}, InStaticGeometry {4}, top-level {5}", 
//                                 node.Name, node.Oid, node.ObjectType, node.SceneNode, node.InStaticGeometry, node.SceneNode.Parent == mgr.RootSceneNode);
                            if (node.ObjectType == ObjectNodeType.Prop &&
                                (node.InStaticGeometry || (node.SceneNode != null && node.SceneNode.Parent == mgr.RootSceneNode)) &&
                                RightKind(node)) {
                                foreach (Material m in node.Entity.SubEntityMaterials) {
                                    MaterialAndNodeCounts nodesUsingMaterial;
                                    if (!materialsUsedMap.TryGetValue(m, out nodesUsingMaterial)) {
                                        nodesUsingMaterial = new MaterialAndNodeCounts();
                                        materialsUsedMap[m] = nodesUsingMaterial;
                                    }
                                    nodesUsingMaterial.materialUseCount++;
                                    int subMeshUseCount;
                                    Dictionary<ObjectNode, int> submeshUseCounts = nodesUsingMaterial.submeshUseCounts;
                                    if (!submeshUseCounts.TryGetValue(node, out subMeshUseCount))
                                        submeshUseCounts[node] = 1;
                                    else
                                        submeshUseCounts[node] = subMeshUseCount + 1;
                                }
                            }
                        }
                    }
                }
                    
                // Now we have a count of uses of each material, and
                // for each node, the number of subentities that use the
                // material.  Now we need to calculate the number of 
                // instance of sharings for each object node
                Dictionary<ObjectNode, bool> candidateNodes = new Dictionary<ObjectNode, bool>();
                foreach (MaterialAndNodeCounts counts in materialsUsedMap.Values) {
                    if (counts.materialUseCount > 1) {
                        foreach (KeyValuePair<ObjectNode, int> pair in counts.submeshUseCounts)
                            candidateNodes[pair.Key] = true;
                    }
                }
                Dictionary<ObjectNode, int> staticNodes = new Dictionary<ObjectNode, int>();
                foreach (KeyValuePair<ObjectNode, bool> pair in candidateNodes) {
                    ObjectNode candidate = pair.Key;
                    bool useIt = pair.Value;
                    if (useIt)
                        staticNodes[candidate] = 0;
                }
                if (staticNodes.Count == 0)
                    log.InfoFormat("StaticGeometryHelper.Rebuild: Didn't build static geometry {0} because object count was zero", name);
                else {
                    log.InfoFormat("StaticGeometryHelper.Rebuild: {0} ObjectNodes", staticNodes.Count);
                    foreach(ObjectNode staticNode in staticNodes.Keys) {
                        SceneNode sc = staticNode.SceneNode;
                        if (!staticNode.InStaticGeometry) {
                            sc.RemoveFromParent();
                            staticNode.InStaticGeometry = true;
                        }
                        log.DebugFormat("StaticGeometryHelper.Rebuild: Add node {0} with name {1} to static geometry",
                            staticNode.Oid, staticNode.Name);
                        objectGeometry.AddSceneNode(sc);
                    }
                }
                if (lastStaticNodes != null) {
                    foreach(ObjectNode node in lastStaticNodes.Keys) {
                        if (!staticNodes.ContainsKey(node)) {
                            // Only 1 instance of the mesh, so make sure that if in a former build it was in
                            // static geometry, that we add it back to the scene graph.
                            if (node.InStaticGeometry) {
                                SceneNode sn = node.SceneNode;
                                if (sn != null)
                                    mgr.RootSceneNode.AddChild(sn);
                                node.InStaticGeometry = false;
                            }
                        }
                    }
                }
                if (staticNodes.Count > 0)
                    objectGeometry.Build();
                lastStaticNodes = staticNodes;
                timeOfLastRebuild = TimeTool.CurrentTime;
            }
            finally {
                Monitor.Exit(mgr);
            }
            log.InfoFormat("StaticGeometryHelper.Rebuild: Rebuild of geometry '{0}' took {1} ms", 
                name, TimeTool.CurrentTime - tickStart);
        }

    }
    
}
