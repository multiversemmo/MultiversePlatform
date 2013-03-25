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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Multiverse.CollisionLib;

#endregion

namespace Multiverse.CollisionLib
{

    public class RegionVolume
    {
        private long objectOid;
        private string regionName;
        private List<CollisionShape> shapes;

        public RegionVolume(long objectOid, string regionName, List<CollisionShape> shapes)
        {
            this.objectOid = objectOid;
            this.regionName = regionName;
            this.shapes = shapes;
        }

        public bool PointInside(Vector3 p) 
        {
            foreach (CollisionShape shape in shapes) {
                if (shape.PointInside(p))
                    return true;
            }
            return false;
        }
        
        public long ObjectOid 
        {
            get { return objectOid; }
        }
        
        public string RegionName 
        {
            get { return regionName; }
        }
                
    }

    public class RegionVolumes 
    {
        // Maps object oid to dictionary of region name vs region volumes
        private Dictionary<long, Dictionary<string, List<RegionVolume>>> objectOidRegions = new Dictionary<long, Dictionary<string, List<RegionVolume>>>();

        private static RegionVolumes instance = new RegionVolumes();
        
        public void AddRegionShapes(long objNodeOid, string regionName, List<CollisionShape> shapes) 
        {
            Dictionary<string, List<RegionVolume>> objectRegionVolumes;
            if (!objectOidRegions.TryGetValue(objNodeOid, out objectRegionVolumes)) {
                objectRegionVolumes = new Dictionary<string, List<RegionVolume>>();
                objectOidRegions[objNodeOid] = objectRegionVolumes;
            }
            List<RegionVolume> regionVolumes;
            if (!objectRegionVolumes.TryGetValue(regionName, out regionVolumes)) {
                regionVolumes = new List<RegionVolume>();
                objectRegionVolumes[regionName] = regionVolumes;
            }
            regionVolumes.Add(new RegionVolume(objNodeOid, regionName, shapes));
        }

        //
        // Start out with an ugly iteration over all regions.  If it's
        // too slow, I'll hack the sphere tree to do the lookup.
        //
        public List<RegionVolume> RegionsContainingPoint(Vector3 p)
        {
            List<RegionVolume> regionVolumes = new List<RegionVolume>();
            foreach (Dictionary<string, List<RegionVolume>> regionDictionary in objectOidRegions.Values) {
                // Trace.TraceInformation("RegionVolumes.RegionsContainingPoints: regionDictionary.Count " + regionDictionary.Count);
                foreach (List<RegionVolume> volumes in regionDictionary.Values) {
                    foreach (RegionVolume volume in volumes)
                    {
                        if (volume.PointInside(p))
                            regionVolumes.Add(volume);
                    }
                }
            }
            return regionVolumes;
        }
        
        public void RemoveCollisionShapesWithHandle(long objNodeOid) 
        {
            objectOidRegions.Remove(objNodeOid);
        }

        public static string ExtractRegionName(string subMeshName) 
        {
            if (String.Compare(subMeshName.Substring(0, 5), "mvrg_", false) != 0)
                return "";
            string rest = subMeshName.Substring(5);
            int index = rest.IndexOf("_");
            return rest.Substring(0, index);
        }

        public static RegionVolumes Instance
        {
            get { return instance; }
        }
        
    }
}
