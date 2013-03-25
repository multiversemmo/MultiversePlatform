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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Xml;

using log4net;

using Axiom.MathLib;
using Axiom.Core;
using Axiom.Animating;

using Multiverse.CollisionLib;
using Multiverse.Config;

namespace Multiverse.CollisionLib
{
    abstract public class PathInterpolator {
    
        public PathInterpolator(long oid, long startTime, float speed, String terrainString, List<Vector3> path) {
            this.oid = oid;
            this.startTime = startTime;
            this.speed = speed;
            this.terrainString = terrainString;
            this.path = path;
        }

        // abstract public string ToString();

        abstract public PathLocAndDir Interpolate(long systemTime);

        public Vector3 ZeroYIfOnTerrain(Vector3 loc, int pointIndex) {
            Debug.Assert(pointIndex >= 0 && pointIndex < terrainString.Length - 1);
            // If either the previous point was on terrain or the
            // current point is on terrain, then the path element is a 
            if (terrainString[pointIndex] == 'T' || terrainString[pointIndex + 1] == 'T')
                loc.y = 0f;
            return loc;
        }

        public string StringPath() {
            string s = "";
            for (int i=0; i<path.Count; i++) {
                Vector3 p = path[i];
                if (s.Length > 0)
                    s += ", ";
                s += "#" + i + ": " + terrainString[i] + p;
            }
            return s;
        }
        
        public long Oid {
            get { return oid; } 
        }

        public float Speed {
            get { return speed; } 
        }

        public string TerrainString {
            get { return terrainString; } 
        }

        public long StartTime {
            get { return startTime; } 
        }

        public float TotalTime {
            get { return totalTime; } 
        }

        protected long oid;
        protected float speed;
        protected String terrainString;
        protected List<Vector3> path;
        protected float totalTime;  // In seconds from start time
        protected long startTime;   // In milliseconds - - server system time

    }


    // A PathSpline is a Catmull-Rom spline, which is related to a
    // BSpline.  Paths produced by the pathing code are interpolated as
    // Catmull-Rom splines
    public class PathSpline : PathInterpolator {

        public PathSpline(long oid, long startTime, float speed, String terrainString, List<Vector3> path) :
                base(oid, startTime, speed, terrainString, path) {
            // Add two points before the zeroeth point, and one after the
            // count-1 point, to allow us to access from -2 to +1 points.
            // Add one point before the zeroeth point, and two after the
            // count-1 point, to allow us to access from -1 to +2 points.
            path.Insert(0, path[0]);
            Vector3 last = path[path.Count - 1];
            path.Add(last);
            path.Add(last);
            int count = path.Count;
            timeVector = new float[count];
            timeVector[0] = 0f;
            float t = 0;
            Vector3 curr = path[0];
            for (int i=1; i<count; i++) {
                Vector3 next = path[i];
                Vector3 diff = next - curr;
                float diffTime = diff.Length;
                t = t + diffTime / speed;
                timeVector[i] = t;
                curr = next;
            }
            totalTime = t;
        }

        public override string ToString() {
            return "[PathSpline oid = " + oid + "; speed = " + speed + "; timeVector = " + timeVector + "; path = " + StringPath() + "]";
        }

        public override PathLocAndDir Interpolate(long systemTime) {
            float t = (float)(systemTime - startTime) / 1000f;
            if (t < 0)
                t = 0;
            else if (t >= totalTime)
                return null;
            // Find the point number whose time is less than or equal to t
            int count = path.Count;
            // A bit of trickiness - - the first two points and the last
            // point are dummies, inserted only to ensure that we have -2
            // to +1 points at every real point.
            int pointNumber = -2;
            for (int i=0; i<count; i++) {
                if (timeVector[i] > t) {
                    pointNumber = i - 1;
                    break;
                }
            }
            if (pointNumber == -1) {
                pointNumber = 1;
            }
            Vector3 loc;
            Vector3 dir;
            float timeFraction = 0;
            // If we're beyond the last time, return the last point, and a
            // (0,0,0) direction
            if (pointNumber == -2) {
                loc = path[count - 1];
                dir = new Vector3(0f, 0f, 0f);
            }
            else {
                float timeAtPoint = timeVector[pointNumber];
                float timeSincePoint = t - timeAtPoint;
                timeFraction = timeSincePoint / (timeVector[pointNumber + 1] - timeAtPoint);
                loc = evalPoint(pointNumber, timeFraction);
                dir = evalDirection(loc, pointNumber, timeFraction) * speed;
            }
            // A bit tricky - - if there were n elements in the _original_
            // path, there are n-1 characters in the terrain string.
            // We've added _three_ additional path elements, one before
            // and two after.
            int pathNumber = (pointNumber == -2 ? count - 4 : pointNumber);
            if (terrainString[pathNumber] == 'T' || terrainString[pathNumber + 1] == 'T') {
                loc.y = 0f;
                dir.y = 0f;
            }
            return new PathLocAndDir(loc, dir, speed * (totalTime - t));
        }
        
        // Catmull-Rom spline is just like a B spline, only with a different basis
        protected float basisFactor(int degree, float t) {
            switch (degree) {
            case -1:
                return ((-t + 2f) * t-1f) * t / 2f;
            case 0:
                return (((3f * t-5f) * t) * t + 2f) / 2f;
            case 1:
                return ((-3f * t + 4f) * t + 1f) * t / 2f;
            case 2:
                return ((t-1f) * t * t) / 2f;
            }
            return 0f; //we only get here if an invalid i is specified
        }

        // evaluate a point on the spline.  t is the time since we arrived
        // at point pointNumber.
        protected Vector3 evalPoint(int pointNumber, float t) {
            float px = 0;
            float py = 0;
            float pz = 0;
            for (int degree = -1; degree<=2; degree++) {
                float basis = basisFactor(degree, t);
                Vector3 pathPoint = path[pointNumber + degree];
                px += basis * pathPoint.x;
                py += basis * pathPoint.y;
                pz += basis * pathPoint.z;
            }
            return new Vector3(px, py, pz);
        }

//         // evaluate a point on the spline.  t is the time since we arrived
//         // at point pointNumber.
//         protected Vector3 evalPoint(int pointNumber, float t) {
//             Vector3 p0 = path[pointNumber - 2];
//             Vector3 p1 = path[pointNumber - 1];
//             Vector3 p2 = path[pointNumber];
//             Vector3 p3 = path[pointNumber + 1];
//             Vector3 q = 0.5f * ((p1 * 2f) +
//                                 (p2 - p0) * t +
//                                 (2 * p0 - 5 * p1 + 4 * p2 - p3) * t*t +
//                                 (3 * p1 - 3 * p2 + p3 - p0) * t * t * t);
//             return q;
//         }

        // .1 second
        protected float directionTimeOffset = .01f;

        // evaluate the direction on the spline.  t is the time since we
        // arrived at point pointNumber.
        protected Vector3 evalDirection(Vector3 p, int pointNumber, float t) {
            Vector3 next = evalPoint(pointNumber, t + directionTimeOffset);
            Vector3 n = next - p;
            n.y = 0;
            n.Normalize();
            return n;
        }

        protected float [] timeVector;

    }
    
    // A linear interpolator of a sequence of points
    public class PathLinear : PathInterpolator {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PathLinear));

        public PathLinear(long oid, long startTime, float speed, String terrainString, List<Vector3> path) :
                base(oid, startTime, speed, terrainString, path) {
            float cumm = 0f;
            Vector3 curr = path[0];
            for (int i=1; i<path.Count; i++) {
                Vector3 next = path[i];
                Vector3 diff = next - curr;
                diff.y = 0f;
                float dist = (next - curr).Length;
                float diffTime = dist / speed;
                cumm += diffTime;
            }
            totalTime = cumm;
        }

        // Evaluate the position and direction on the path.  The
        // argument is the millisecond system time
        public override PathLocAndDir Interpolate(long systemTime) {
            float t = (float)(systemTime - startTime) / 1000f;
            if (t < 0)
                t = 0;
            else if (t >= totalTime) {
                log.DebugFormat("PathLinear.Interpolate: oid {0}, time has expired", oid);
                return null;
            }
            float cumm = 0f;
            Vector3 curr = path[0];
            for (int i=1; i<path.Count; i++) {
                Vector3 next = path[i];
                Vector3 diff = next - curr;
                diff.y = 0; // ZeroYIfOnTerrain(diff, i - 1);
                float dist = diff.Length;
                float diffTime = dist / speed;
                if (t <= cumm + diffTime) {
                    float frac = (t - cumm) / diffTime;
                    Vector3 loc = curr + (diff * frac); // ZeroYIfOnTerrain(curr + (diff * frac), i - 1);
                    diff.Normalize();
                    Vector3 dir = diff * speed;
                    log.DebugFormat("PathLinear.Interpolate: oid {0}, next {1}, curr {2}, diff {3}, diffTime {4}, frac {5}, loc {6}, dir {7}",
                                    oid, next, curr, diff, diffTime, frac, loc, dir);
                    return new PathLocAndDir(loc, dir, speed * (totalTime - t));
                }
                cumm += diffTime;
                curr = next;
            }
            // Didn't find the time, so return the last point, and a dir
            // of zero
            return new PathLocAndDir(path[path.Count - 1], new Vector3(0f, 0f, 0f), 0f);
        }

        public override String ToString() {
            return "[PathLinear oid = " + oid + "; speed = " + speed + "; path = " + StringPath() + "]";
        }

    }

    // A class to encapsulate the return values from path
    // interpolators
    public class PathLocAndDir {

        public PathLocAndDir(Vector3 location, Vector3 direction, float lengthLeft) {
            this.location = location;
            this.direction = direction;
            this.lengthLeft = lengthLeft;
        }

        public Vector3 Location {
            get { return location; }
        }

        public Vector3 Direction {
            get { return direction; }
        }

        public float LengthLeft {
            get { return lengthLeft; }
        }

        public override string ToString() {
            return string.Format("[PathLocAndDir loc {0} dir {1} lengthLeft {2}]",
                                 location, direction, lengthLeft);
        }
        
        protected Vector3 location;
        protected Vector3 direction;
        protected float lengthLeft;
    }
    
}
