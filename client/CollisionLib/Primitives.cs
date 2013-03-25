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
using System.Text;
using Multiverse.CollisionLib;

#endregion

namespace Multiverse.CollisionLib
{
    
    public class Primitives
    {
    
//////////////////////////////////////////////////////////////////////
//
// Testing primitives
//
//////////////////////////////////////////////////////////////////////

public const float epsilon = 0.00001f;

public static Vector3[] UnitBasisVectors = new Vector3[3]
    {new Vector3(1.0f, 0.0f, 0.0f),
     new Vector3(0.0f, 1.0f, 0.0f),
     new Vector3(0.0f, 0.0f, 1.0f)};

public static float DistanceSquared(Vector3 v1, Vector3 v2) {
    Vector3 diff = v2 - v1;
    return diff.Dot(diff);
}

public static float Distance(Vector3 v1, Vector3 v2) {
    Vector3 diff = v2 - v1;
    return (float)Math.Sqrt(diff.Dot(diff));
}

public static float Square(float x) { return x * x; }

public static float SquareSum (float x, float y, float z) {
    return x * x + y * y + z * z;
}

// Clamp n to lie within the range [min, max]
public static float Clamp(float n, float min, float max) {
    if (n < min) return min;
    if (n > max) return max;
    return n;
}

// Clamp n to lie within the range [min, max]
public static float Clamp01(float n) {
    if (n < 0.0f) return 0.0f;
    if (n > 1.0f) return 1.0f;
    return n;
}

// Given segment ab and point c, computes closest point d on ab.
// Also returns t for the position of d, d(t) = a + t*(b - a)
public static void ClosestPtPointSegment(Vector3 c, Vector3 a, Vector3 b, 
                                         out float t, out Vector3 d) {
    Vector3 ab = b - a;
    // Project c onto ab, computing parameterized position d(t) = a + t*(b - a)
    Vector3 cma = c - a;
    t = cma.Dot(ab) / ab.Dot(ab);
    // If outside segment, clamp t (and therefore d) to the closest endpoint
    t = Clamp01(t);
    // Compute projected position from the clamped t
    d = a + t * ab;
}

// Distance from a segment defined by a and b to a point c
public static float SqDistPointSegment(Vector3 a, Vector3 b, Vector3 c)
{
    Vector3 ab = b - a;
    Vector3 ac = c - a;
    Vector3 bc = c - b;
    float e = ac.Dot(ab);
    // Handle cases where c projects outside ab
    if (e <= 0.0f)
        return ac.Dot(ac);
    float f = ab.Dot(ab);
    if (e >= f)
        return bc.Dot(bc);
    // Handle case where c projects onto ab
    return ac.Dot(ac) - e * e / f;
}

// Computes the square distance between a point p and an AABB b
public static float SqDistPointAABB(Vector3 p, CollisionAABB b) {
    float sqDist = 0.0f;
    for (int i = 0; i < 3; i++) {
        // For each axis count any excess distance outside box extents
        float v = p[i];
        if (v < b.min[i])
            sqDist += (b.min[i] - v) * (b.min[i] - v);
        if (v > b.max[i])
            sqDist += (v - b.max[i]) * (v - b.max[i]);
    }
    return sqDist;
}

// Given point p, return the point q on or in AABB b, that is closest to p
public static void ClosestPtPointAABB(Vector3 p, CollisionAABB b, out Vector3 q)
{
    // For each coordinate axis, if the point coordinate value is
    // outside box, clamp it to the box, else keep it as is
    q = Vector3.Zero;
    for (int i = 0; i < 3; i++) {
        float v = p[i];
        v = Clamp(v, b.min[i], b.max[i]);
        q[i] = v;
    }
}

// Given point p, return point q on the surface of OBB b, closest to p
public static void ClosestPtPointOBB(Vector3 p, CollisionOBB b, out Vector3 q) {
    Vector3 d = p - b.center;
    // Make sure P is on the surface of the OBB
    float pSquared = d.Dot(d);
    float bSquared = b.radius * b.radius;
    if (pSquared < bSquared) {
        p = p * (float)Math.Sqrt(bSquared / pSquared);
    }
    // Start result at center of box; make steps from there
    q = b.center;
    // For each OBB axis...
    for (int i = 0; i < 3; i++) {
        // ...project d onto that axis to get the distance
        // along the axis of d from the box center
        float dist = d.Dot(b.axes[i]);
        // If distance farther than the box extents, clamp to the box
        float e = b.extents[i];
        dist = Clamp(dist, -e, e);
        // Step that distance along the axis to get world coordinate
        q += dist * b.axes[i];
    }
}

// Returns square of the distance from a point to an OBB
public static float SqDistPointOBB(Vector3 p, CollisionOBB b, out Vector3 c)
{
    ClosestPtPointOBB(p, b, out c);
    Vector3 v = p - c;
    return v.Dot(v);
}

// Returns true if sphere s intersects OBB b, false otherwise.
// The point p on the OBB closest to the sphere center is also returned
public static bool TestSphereOBB(CollisionSphere s, CollisionOBB b, out Vector3 p)
{
    return (SqDistPointOBB(s.center, b, out p) < s.radius * s.radius);
}

// Computes closest points C1 and C2 of S1(s)=P1+s*(Q1-P1) and
// S2(t)=P2+t*(Q2-P2), returning s and t. Function result is squared
// distance between between S1(s) and S2(t)
public static float ClosestPtSegmentSegment(Vector3 p1, Vector3 q1, 
                                              Vector3 p2, Vector3 q2,
                                              out float s, out float t,
                                              out Vector3 c1, out Vector3 c2)
{
    Vector3 d1 = q1 - p1; // Direction vector of segment S1
    Vector3 d2 = q2 - p2; // Direction vector of segment S2
    Vector3 r = p1 - p2;
    float a = d1.Dot(d1); // Squared length of segment S1, always nonnegative
    float e = d2.Dot(d2); // Squared length of segment S2, always nonnegative
    float f = d2.Dot(r);

    // Check if either or both segments degenerate into points
    if (a <= epsilon && e <= epsilon) {
        // Both segments degenerate into points
        s = t = 0.0f;
        c1 = p1;
        c2 = p2;
        return DistanceSquared(c1, c2);
    }
    if (a <= epsilon) {
        // First segment degenerates into a point
        s = 0.0f;
        t = f / e; // s = 0 => t = (b*s + f) / e = f / e
        t = Clamp01(t);
    } else {
        float c = d1.Dot(r);
        if (e <= epsilon) {
            // Second segment degenerates into a point
            t = 0.0f;
            s = Clamp01(-c / a); // t = 0 => s = (b*t - c) / a = -c / a
        } else {
            // The general nondegenerate case starts here
            float b = d1.Dot(d2);
            float denom = a*e-b*b; // Always nonnegative

            // If segments not parallel, compute closest point on L1 to L2, and
            // clamp to segment S1. Else pick arbitrary s (here 0)
            if (denom != 0.0f) {
                s = Clamp01((b*f - c*e) / denom);
            } else s = 0.0f;

            // Compute point on L2 closest to S1(s) using
            // t = D2.Dot((P1+D1*s)-P2) / D2.Dot(D2) = (b*s + f) / e
            t = (b*s + f) / e;

            // If t in [0,1] done. Else clamp t, recompute s for the new value
            // of t using s = D1.Dot((P2+D2*t)-P1) / D1.Dot(D1)= (t*b - c) / a
            // and clamp s to [0, 1]
            if (t < 0.0f) {
                t = 0.0f;
                s = Clamp01(-c / a);
            } else if (t > 1.0f) {
                t = 1.0f;
                s = Clamp01((b - c) / a);
            }
        }
    }

    c1 = p1 + d1 * s;
    c2 = p2 + d2 * t;
    return DistanceSquared(c1, c2);
}

public static bool TestCapsuleCapsule(CollisionCapsule c1, CollisionCapsule c2,
                                      CollisionParms parms)                                   
{
    // Compute (squared) distance between the inner structures of the capsules
    float s, t;
    Vector3 p1, p2;
    float d = ClosestPtSegmentSegment(c1.bottomcenter, c1.topcenter,
                                      c2.bottomcenter, c2.topcenter,
                                      out s, out t, out p1, out p2);
    float r = c1.capRadius + c2.capRadius;
    Vector3 n = p2 - p1;
    parms.SetNormPart(n);
    parms.SetNormObstacle(-n);
    return d < r * r;
}

// Test if segment specified by points p0 and p1 intersects AABB b
public static bool TestSegmentAABB(Vector3 p0, Vector3 p1, CollisionAABB b)
{
    Vector3 e = b.max - b.center;       // Box halflength extents
    Vector3 m = (p0 + p1) * 0.5f;       // Segment midpoint
    Vector3 d = p1 - m;                 // Segment halflength vector
    m = m - b.center;                   // Translate box and segment to origin

    // Try world coordinate axes as separating axes
    float adx = Math.Abs(d.x);
    if (Math.Abs(m.x) > e.x + adx)
        return false;
    float ady = Math.Abs(d.y);
    if (Math.Abs(m.y) > e.y + ady)
        return false;
    float adz = Math.Abs(d.z);
    if (Math.Abs(m.z) > e.z + adz)
        return false;

    // Add in an epsilon term to counteract arithmetic errors when segment is
    // (near) parallel to a coordinate axis (see text for detail)
    adx += epsilon;
    ady += epsilon;
    adz += epsilon;

    // Try cross products of segment direction vector with coordinate axes
    if (Math.Abs(m.y * d.z - m.z * d.y) > e.y * adz + e.z * ady)
        return false;
    if (Math.Abs(m.z * d.x - m.x * d.z) > e.x * adz + e.z * adx)
        return false;
    if (Math.Abs(m.x * d.y - m.y * d.x) > e.x * ady + e.y * adx)
        return false;

    // No separating axis found; segment must be overlapping AABB
    return true;
}

public static bool TestAABBAABB(CollisionAABB a, CollisionAABB b)
{
    // Exit with no intersection if separated along an axis
    if (a.max[0] < b.min[0] || a.min[0] > b.max[0])
        return false;
    if (a.max[1] < b.min[1] || a.min[1] > b.max[1])
        return false;
    if (a.max[2] < b.min[2] || a.min[2] > b.max[2])
        return false;
    // Overlapping on all axes means AABBs are intersecting
    return true;
}

public static bool TestOBBOBBInternal(CollisionOBB a, CollisionOBB b)
{
    float ra, rb;
    Matrix3 R = Matrix3.Zero;
    Matrix3 AbsR = Matrix3.Zero;

    // Compute rotation matrix expressing b in a's coordinate frame
    for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
            R[i,j] = a.axes[i].Dot(b.axes[j]);

    // Compute translation vector t
    Vector3 t = b.center - a.center;
    // Bring translation into a's coordinate frame
    t = new Vector3(t.Dot(a.axes[0]), t.Dot(a.axes[1]), t.Dot(a.axes[2]));

    // Compute common subexpressions. Add in an epsilon term to
    // counteract arithmetic errors when two edges are parallel and
    // their cross product is (near) null (see text for details)
    for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
            AbsR[i,j] = Math.Abs(R[i,j]) + epsilon;

    // Test axes L = A0, L = A1, L = A2
    for (int i = 0; i < 3; i++) {
        ra = a.extents[i];
        rb = b.extents[0] * AbsR[i,0] + b.extents[1] * AbsR[i,1] + b.extents[2] * AbsR[i,2];
        if (Math.Abs(t[i]) > ra + rb)
            return false;
    }

    // Test axes L = B0, L = B1, L = B2
    for (int i = 0; i < 3; i++) {
        ra = a.extents[0] * AbsR[0,i] + a.extents[1] * AbsR[1,i] + a.extents[2] * AbsR[2,i];
        rb = b.extents[i];
        if (Math.Abs(t[0] * R[0,i] + t[1] * R[1,i] + t[2] * R[2,i]) > ra + rb)
            return false;
    }

    // Test axis L = A0 x B0
    ra = a.extents[1] * AbsR[2,0] + a.extents[2] * AbsR[1,0];
    rb = b.extents[1] * AbsR[0,2] + b.extents[2] * AbsR[0,1];
    if (Math.Abs(t[2] * R[1,0] - t[1] * R[2,0]) > ra + rb)
        return false;

    // Test axis L = A0 x B1
    ra = a.extents[1] * AbsR[2,1] + a.extents[2] * AbsR[1,1];
    rb = b.extents[0] * AbsR[0,2] + b.extents[2] * AbsR[0,0];
    if (Math.Abs(t[2] * R[1,1] - t[1] * R[2,1]) > ra + rb)
        return false;

    // Test axis L = A0 x B2
    ra = a.extents[1] * AbsR[2,2] + a.extents[2] * AbsR[1,2];
    rb = b.extents[0] * AbsR[0,1] + b.extents[1] * AbsR[0,0];
    if (Math.Abs(t[2] * R[1,2] - t[1] * R[2,2]) > ra + rb)
        return false;

    // Test axis L = A1 x B0
    ra = a.extents[0] * AbsR[2,0] + a.extents[2] * AbsR[0,0];
    rb = b.extents[1] * AbsR[1,2] + b.extents[2] * AbsR[1,1];
    if (Math.Abs(t[0] * R[2,0] - t[2] * R[0,0]) > ra + rb)
        return false;

    // Test axis L = A1 x B1
    ra = a.extents[0] * AbsR[2,1] + a.extents[2] * AbsR[0,1];
    rb = b.extents[0] * AbsR[1,2] + b.extents[2] * AbsR[1,0];
    if (Math.Abs(t[0] * R[2,1] - t[2] * R[0,1]) > ra + rb)
        return false;

    // Test axis L = A1 x B2
    ra = a.extents[0] * AbsR[2,2] + a.extents[2] * AbsR[0,2];
    rb = b.extents[0] * AbsR[1,1] + b.extents[1] * AbsR[1,0];
    if (Math.Abs(t[0] * R[2,2] - t[2] * R[0,2]) > ra + rb)
        return false;

    // Test axis L = A2 x B0
    ra = a.extents[0] * AbsR[1,0] + a.extents[1] * AbsR[0,0];
    rb = b.extents[1] * AbsR[2,2] + b.extents[2] * AbsR[2,1];
    if (Math.Abs(t[1] * R[0,0] - t[0] * R[1,0]) > ra + rb)
        return false;

    // Test axis L = A2 x B1
    ra = a.extents[0] * AbsR[1,1] + a.extents[1] * AbsR[0,1];
    rb = b.extents[0] * AbsR[2,2] + b.extents[2] * AbsR[2,0];
    if (Math.Abs(t[1] * R[0,1] - t[0] * R[1,1]) > ra + rb)
        return false;

    // Test axis L = A2 x B2
    ra = a.extents[0] * AbsR[1,2] + a.extents[1] * AbsR[0,2];
    rb = b.extents[0] * AbsR[2,1] + b.extents[1] * AbsR[2,0];
    if (Math.Abs(t[1] * R[0,2] - t[0] * R[1,2]) > ra + rb)
        return false;

    // Since no separating axis found, the OBBs must be intersecting
    return true;
}

public static bool TestOBBOBB(CollisionOBB a, CollisionOBB b, CollisionParms parms)
{
    if (MO.DoLog)
        MO.Log("TestOBBOBB Entering: a {0} b {1}", a, b);
    if (TestOBBOBBInternal(a, b)) {
        Vector3 n = b.center - a.center;
        // ??? Not right
        parms.SetNormPart(n);
        parms.SetNormObstacle(-n);
		if (MO.DoLog)
			MO.Log("TestOBBOBB Collided: n {0}", n);
        return true;
    }
    else
        return false;
}


public class SegOBBParams
{
    public bool useIt;
    public float pfLParam;
    // pfBVec is a vector relative to the center of a box, but in box
    // coordinates
	public Vector3 pfBVec;
    public SegOBBParams(bool useIt) {
        this.useIt = useIt;
        pfLParam = 0.0f;
        Vector3 pfBVec = new Vector3(0.0f, 0.0f, 0.0f);
    }
}


public static void Face (int i0, int i1, int i2, ref Vector3 rkPnt,
                         Vector3 rkDir, CollisionOBB rkBox, 
                         Vector3 rkPmE, SegOBBParams pf,
                         ref float rfSqrDistance)
{
    Vector3 kPpE = Vector3.Zero;
    float fLSqr, fInv, fTmp, fParam, fT, fDelta;

    kPpE[i1] = rkPnt[i1] + rkBox.extents[i1];
    kPpE[i2] = rkPnt[i2] + rkBox.extents[i2];
    if (rkDir[i0]*kPpE[i1] >= rkDir[i1]*rkPmE[i0]) {
        if (rkDir[i0]*kPpE[i2] >= rkDir[i2]*rkPmE[i0]) {
            // v[i1] >= -e[i1], v[i2] >= -e[i2] (distance = 0)
            if (pf.useIt) {
                rkPnt[i0] = rkBox.extents[i0];
                fInv = 1.0f/rkDir[i0];
                rkPnt[i1] -= rkDir[i1]*rkPmE[i0]*fInv;
                rkPnt[i2] -= rkDir[i2]*rkPmE[i0]*fInv;
                pf.pfLParam = -rkPmE[i0]*fInv;
            }
        } else {
            // v[i1] >= -e[i1], v[i2] < -e[i2]
            fLSqr = rkDir[i0]*rkDir[i0] + rkDir[i2]*rkDir[i2];
            fTmp = fLSqr*kPpE[i1] - rkDir[i1]*(rkDir[i0]*rkPmE[i0] +
                                               rkDir[i2]*kPpE[i2]);
            if (fTmp <= 2.0*fLSqr*rkBox.extents[i1]) {
                fT = fTmp/fLSqr;
                fLSqr += rkDir[i1]*rkDir[i1];
                fTmp = kPpE[i1] - fT;
                fDelta = rkDir[i0]*rkPmE[i0] + rkDir[i1]*fTmp +
                    rkDir[i2]*kPpE[i2];
                fParam = -fDelta/fLSqr;
                rfSqrDistance += rkPmE[i0]*rkPmE[i0] + fTmp*fTmp +
                    kPpE[i2]*kPpE[i2] + fDelta*fParam;

                if (pf.useIt) {
                    pf.pfLParam = fParam;
                    rkPnt[i0] = rkBox.extents[i0];
                    rkPnt[i1] = fT - rkBox.extents[i1];
                    rkPnt[i2] = -rkBox.extents[i2];
                }
            } else {
                fLSqr += rkDir[i1]*rkDir[i1];
                fDelta = rkDir[i0]*rkPmE[i0] + rkDir[i1]*rkPmE[i1] +
                    rkDir[i2]*kPpE[i2];
                fParam = -fDelta/fLSqr;
                rfSqrDistance += rkPmE[i0]*rkPmE[i0] + rkPmE[i1]*rkPmE[i1] +
                    kPpE[i2]*kPpE[i2] + fDelta*fParam;

                if (pf.useIt) {
                    pf.pfLParam = fParam;
                    rkPnt[i0] = rkBox.extents[i0];
                    rkPnt[i1] = rkBox.extents[i1];
                    rkPnt[i2] = -rkBox.extents[i2];
                }
            }
        }
    } else {
        if (rkDir[i0]*kPpE[i2] >= rkDir[i2]*rkPmE[i0]) {
            // v[i1] < -e[i1], v[i2] >= -e[i2]
            fLSqr = rkDir[i0]*rkDir[i0] + rkDir[i1]*rkDir[i1];
            fTmp = fLSqr*kPpE[i2] - rkDir[i2]*(rkDir[i0]*rkPmE[i0] +
                                               rkDir[i1]*kPpE[i1]);
            if (fTmp <= 2.0*fLSqr*rkBox.extents[i2]) {
                fT = fTmp/fLSqr;
                fLSqr += rkDir[i2]*rkDir[i2];
                fTmp = kPpE[i2] - fT;
                fDelta = rkDir[i0]*rkPmE[i0] + rkDir[i1]*kPpE[i1] +
                    rkDir[i2]*fTmp;
                fParam = -fDelta/fLSqr;
                rfSqrDistance += rkPmE[i0]*rkPmE[i0] + kPpE[i1]*kPpE[i1] +
                    fTmp*fTmp + fDelta*fParam;

                if (pf.useIt) {
                    pf.pfLParam = fParam;
                    rkPnt[i0] = rkBox.extents[i0];
                    rkPnt[i1] = -rkBox.extents[i1];
                    rkPnt[i2] = fT - rkBox.extents[i2];
                }
            } else {
                fLSqr += rkDir[i2]*rkDir[i2];
                fDelta = rkDir[i0]*rkPmE[i0] + rkDir[i1]*kPpE[i1] +
                    rkDir[i2]*rkPmE[i2];
                fParam = -fDelta/fLSqr;
                rfSqrDistance += rkPmE[i0]*rkPmE[i0] + kPpE[i1]*kPpE[i1] +
                    rkPmE[i2]*rkPmE[i2] + fDelta*fParam;

                if (pf.useIt) {
                    pf.pfLParam = fParam;
                    rkPnt[i0] = rkBox.extents[i0];
                    rkPnt[i1] = -rkBox.extents[i1];
                    rkPnt[i2] = rkBox.extents[i2];
                }
            }
        } else {
            // v[i1] < -e[i1], v[i2] < -e[i2]
            fLSqr = rkDir[i0]*rkDir[i0]+rkDir[i2]*rkDir[i2];
            fTmp = fLSqr*kPpE[i1] - rkDir[i1]*(rkDir[i0]*rkPmE[i0] +
                                               rkDir[i2]*kPpE[i2]);
            if (fTmp >= 0.0f) {
                // v[i1]-edge is closest
                if (fTmp <= 2.0*fLSqr*rkBox.extents[i1])
                    {
                        fT = fTmp/fLSqr;
                        fLSqr += rkDir[i1]*rkDir[i1];
                        fTmp = kPpE[i1] - fT;
                        fDelta = rkDir[i0]*rkPmE[i0] + rkDir[i1]*fTmp +
                            rkDir[i2]*kPpE[i2];
                        fParam = -fDelta/fLSqr;
                        rfSqrDistance += rkPmE[i0]*rkPmE[i0] + fTmp*fTmp +
                            kPpE[i2]*kPpE[i2] + fDelta*fParam;

                        if (pf.useIt)
                            {
                                pf.pfLParam = fParam;
                                rkPnt[i0] = rkBox.extents[i0];
                                rkPnt[i1] = fT - rkBox.extents[i1];
                                rkPnt[i2] = -rkBox.extents[i2];
                            }
                    } else {
                    fLSqr += rkDir[i1]*rkDir[i1];
                    fDelta = rkDir[i0]*rkPmE[i0] + rkDir[i1]*rkPmE[i1] +
                        rkDir[i2]*kPpE[i2];
                    fParam = -fDelta/fLSqr;
                    rfSqrDistance += rkPmE[i0]*rkPmE[i0] + rkPmE[i1]*rkPmE[i1]
                        + kPpE[i2]*kPpE[i2] + fDelta*fParam;

                    if (pf.useIt) {
                        pf.pfLParam = fParam;
                        rkPnt[i0] = rkBox.extents[i0];
                        rkPnt[i1] = rkBox.extents[i1];
                        rkPnt[i2] = -rkBox.extents[i2];
                    }
                }
                return;
            }

            fLSqr = rkDir[i0]*rkDir[i0] + rkDir[i1]*rkDir[i1];
            fTmp = fLSqr*kPpE[i2] - rkDir[i2]*(rkDir[i0]*rkPmE[i0] +
                                               rkDir[i1]*kPpE[i1]);
            if (fTmp >= 0.0f) {
                // v[i2]-edge is closest
                if (fTmp <= 2.0*fLSqr*rkBox.extents[i2]) {
                    fT = fTmp/fLSqr;
                    fLSqr += rkDir[i2]*rkDir[i2];
                    fTmp = kPpE[i2] - fT;
                    fDelta = rkDir[i0]*rkPmE[i0] + rkDir[i1]*kPpE[i1] +
                        rkDir[i2]*fTmp;
                    fParam = -fDelta/fLSqr;
                    rfSqrDistance += rkPmE[i0]*rkPmE[i0] + kPpE[i1]*kPpE[i1] +
                        fTmp*fTmp + fDelta*fParam;

                    if (pf.useIt) {
                        pf.pfLParam = fParam;
                        rkPnt[i0] = rkBox.extents[i0];
                        rkPnt[i1] = -rkBox.extents[i1];
                        rkPnt[i2] = fT - rkBox.extents[i2];
                    }
                } else {
                    fLSqr += rkDir[i2]*rkDir[i2];
                    fDelta = rkDir[i0]*rkPmE[i0] + rkDir[i1]*kPpE[i1] +
                        rkDir[i2]*rkPmE[i2];
                    fParam = -fDelta/fLSqr;
                    rfSqrDistance += rkPmE[i0]*rkPmE[i0] + kPpE[i1]*kPpE[i1] +
                        rkPmE[i2]*rkPmE[i2] + fDelta*fParam;

                    if (pf.useIt) {
                        pf.pfLParam = fParam;
                        rkPnt[i0] = rkBox.extents[i0];
                        rkPnt[i1] = -rkBox.extents[i1];
                        rkPnt[i2] = rkBox.extents[i2];
                    }
                }
                return;
            }

            // (v[i1],v[i2])-corner is closest
            fLSqr += rkDir[i2]*rkDir[i2];
            fDelta = rkDir[i0]*rkPmE[i0] + rkDir[i1]*kPpE[i1] +
                rkDir[i2]*kPpE[i2];
            fParam = -fDelta/fLSqr;
            rfSqrDistance += rkPmE[i0]*rkPmE[i0] + kPpE[i1]*kPpE[i1] +
                kPpE[i2]*kPpE[i2] + fDelta*fParam;

            if (pf.useIt) {
                pf.pfLParam = fParam;
                rkPnt[i0] = rkBox.extents[i0];
                rkPnt[i1] = -rkBox.extents[i1];
                rkPnt[i2] = -rkBox.extents[i2];
            }
        }
    }
}


public static void CaseNoZeros (ref Vector3 rkPnt, Vector3 rkDir,
                                CollisionOBB rkBox, SegOBBParams pf,
                                ref float rfSqrDistance)
{
    Vector3 kPmE = new Vector3(rkPnt.x - rkBox.extents[0],
                               rkPnt.y - rkBox.extents[1],
                               rkPnt.z - rkBox.extents[2]);

    float fProdDxPy, fProdDyPx, fProdDzPx, fProdDxPz, fProdDzPy, fProdDyPz;

    fProdDxPy = rkDir.x*kPmE.y;
    fProdDyPx = rkDir.y*kPmE.x;
    if (fProdDyPx >= fProdDxPy) {
        fProdDzPx = rkDir.z*kPmE.x;
        fProdDxPz = rkDir.x*kPmE.z;
        if (fProdDzPx >= fProdDxPz) {
            // line intersects x = e0
            Face(0,1,2,ref rkPnt,rkDir,rkBox,kPmE,pf,ref rfSqrDistance);
        } else {
            // line intersects z = e2
            Face(2,0,1,ref rkPnt,rkDir,rkBox,kPmE,pf,ref rfSqrDistance);
        }
    } else {
        fProdDzPy = rkDir.z*kPmE.y;
        fProdDyPz = rkDir.y*kPmE.z;
        if (fProdDzPy >= fProdDyPz)
            {
                // line intersects y = e1
                Face(1,2,0,ref rkPnt,rkDir,rkBox,kPmE,pf,ref rfSqrDistance);
            } else {
            // line intersects z = e2
            Face(2,0,1,ref rkPnt,rkDir,rkBox,kPmE,pf,ref rfSqrDistance);
        }
    }
}


public static void Case0 (int i0, int i1, int i2, ref Vector3 rkPnt,
                            Vector3 rkDir, CollisionOBB rkBox,
                            SegOBBParams pf, ref float rfSqrDistance)
{
    float fPmE0 = rkPnt[i0] - rkBox.extents[i0];
    float fPmE1 = rkPnt[i1] - rkBox.extents[i1];
    float fProd0 = rkDir[i1]*fPmE0;
    float fProd1 = rkDir[i0]*fPmE1;
    float fDelta, fInvLSqr, fInv;

    if (fProd0 >= fProd1) {
        // line intersects P[i0] = e[i0]
        rkPnt[i0] = rkBox.extents[i0];

        float fPpE1 = rkPnt[i1] + rkBox.extents[i1];
        fDelta = fProd0 - rkDir[i0]*fPpE1;
        if (fDelta >= 0.0f)
            {
                fInvLSqr = 1.0f/(rkDir[i0]*rkDir[i0] + rkDir[i1]*rkDir[i1]);
                rfSqrDistance += fDelta*fDelta*fInvLSqr;
                if (pf.useIt) {
                    rkPnt[i1] = -rkBox.extents[i1];
                    pf.pfLParam = -(rkDir[i0]*fPmE0+rkDir[i1]*fPpE1)*fInvLSqr;
                }
            } else {
            if (pf.useIt) {
                fInv = 1.0f/rkDir[i0];
                rkPnt[i1] -= fProd0*fInv;
                pf.pfLParam = -fPmE0*fInv;
            }
        }
    } else {
        // line intersects P[i1] = e[i1]
        rkPnt[i1] = rkBox.extents[i1];

        float fPpE0 = rkPnt[i0] + rkBox.extents[i0];
        fDelta = fProd1 - rkDir[i1]*fPpE0;
        if (fDelta >= 0.0f) {
            fInvLSqr = 1.0f/(rkDir[i0]*rkDir[i0] + rkDir[i1]*rkDir[i1]);
            rfSqrDistance += fDelta*fDelta*fInvLSqr;
            if (pf.useIt) {
                rkPnt[i0] = -rkBox.extents[i0];
                pf.pfLParam = -(rkDir[i0]*fPpE0+rkDir[i1]*fPmE1)*fInvLSqr;
            }
        } else {
            if (pf.useIt) {
                fInv = 1.0f/rkDir[i1];
                rkPnt[i0] -= fProd1*fInv;
                pf.pfLParam = -fPmE1*fInv;
            }
        }
    }

    if (rkPnt[i2] < -rkBox.extents[i2]) {
        fDelta = rkPnt[i2] + rkBox.extents[i2];
        rfSqrDistance += fDelta*fDelta;
        rkPnt[i2] = -rkBox.extents[i2];
    }
    else if (rkPnt[i2] > rkBox.extents[i2]) {
        fDelta = rkPnt[i2] - rkBox.extents[i2];
        rfSqrDistance += fDelta*fDelta;
        rkPnt[i2] = rkBox.extents[i2];
    }
}


public static void Case00 (int i0, int i1, int i2, ref Vector3 rkPnt,
                             Vector3 rkDir, CollisionOBB rkBox,
                             SegOBBParams pf, ref float rfSqrDistance)
{
    float fDelta;

    if (pf.useIt)
        pf.pfLParam = (rkBox.extents[i0] - rkPnt[i0])/rkDir[i0];

    rkPnt[i0] = rkBox.extents[i0];

    if (rkPnt[i1] < -rkBox.extents[i1]) {
        fDelta = rkPnt[i1] + rkBox.extents[i1];
        rfSqrDistance += fDelta*fDelta;
        rkPnt[i1] = -rkBox.extents[i1];
    }
    else if (rkPnt[i1] > rkBox.extents[i1]) {
        fDelta = rkPnt[i1] - rkBox.extents[i1];
        rfSqrDistance += fDelta*fDelta;
        rkPnt[i1] = rkBox.extents[i1];
    }

    if (rkPnt[i2] < -rkBox.extents[i2]) {
        fDelta = rkPnt[i2] + rkBox.extents[i2];
        rfSqrDistance += fDelta*fDelta;
        rkPnt[i2] = -rkBox.extents[i2];
    }
    else if (rkPnt[i2] > rkBox.extents[i2]) {
        fDelta = rkPnt[i2] - rkBox.extents[i2];
        rfSqrDistance += fDelta*fDelta;
        rkPnt[i2] = rkBox.extents[i2];
    }
}


public static void Case000 (ref Vector3 rkPnt, CollisionOBB rkBox,
                            ref float rfSqrDistance)
{
    float fDelta;

    for (int i=0; i<3; i++) {
        
        if (rkPnt[i] < -rkBox.extents[i]) {
            fDelta = rkPnt[i] + rkBox.extents[i];
            rfSqrDistance += fDelta*fDelta;
            rkPnt[i] = -rkBox.extents[i];
        }
        else if (rkPnt[i] > rkBox.extents[i]) {
            fDelta = rkPnt[i] - rkBox.extents[i];
            rfSqrDistance += fDelta*fDelta;
            rkPnt[i] = rkBox.extents[i];
        }
    }
}


public static float SqrDistSegOBBInternal (Segment rkLine, CollisionOBB rkBox,
                                           SegOBBParams pf)
{
    // compute coordinates of line in box coordinate system
    Vector3 kDiff = rkLine.origin - rkBox.center;
    Vector3 kPnt = new Vector3(kDiff.Dot(rkBox.axes[0]),
                               kDiff.Dot(rkBox.axes[1]),
                               kDiff.Dot(rkBox.axes[2]));
    Vector3 kDir = new Vector3(rkLine.direction.Dot(rkBox.axes[0]),
                               rkLine.direction.Dot(rkBox.axes[1]),
                               rkLine.direction.Dot(rkBox.axes[2]));

    // Apply reflections so that direction vector has nonnegative components.
    bool[] bReflect = new bool[3] { false, false, false };
    int i;
    for (i = 0; i < 3; i++) {
        if (kDir[i] < 0.0f) {
            kPnt[i] = -kPnt[i];
            kDir[i] = -kDir[i];
            bReflect[i] = true;
        }
    }

    float fSqrDistance = 0.0f;

    if (kDir.x > 0.0f) {
        if (kDir.y > 0.0f) {
            if (kDir.z > 0.0f) {
                // (+,+,+)
                CaseNoZeros(ref kPnt,kDir,rkBox,pf,ref fSqrDistance);
            } else {
                // (+,+,0)
                Case0(0,1,2,ref kPnt,kDir,rkBox,pf,ref fSqrDistance);
            }
        } else {
            if (kDir.z > 0.0f) {
                // (+,0,+)
                Case0(0,2,1,ref kPnt,kDir,rkBox,pf,ref fSqrDistance);
            } else {
                // (+,0,0)
                Case00(0,1,2,ref kPnt,kDir,rkBox,pf,ref fSqrDistance);
            }
        }
    } else {
        if (kDir.y > 0.0f) {
            if (kDir.z > 0.0f) {
                // (0,+,+)
                Case0(1,2,0,ref kPnt,kDir,rkBox,pf,ref fSqrDistance);
            } else {
                // (0,+,0)
                Case00(1,0,2,ref kPnt,kDir,rkBox,pf,ref fSqrDistance);
            }
        } else {
            if (kDir.z > 0.0f)
                {
                    // (0,0,+)
                    Case00(2,0,1,ref kPnt,kDir,rkBox,pf,ref fSqrDistance);
                } else {
                // (0,0,0)
                Case000(ref kPnt,rkBox,ref fSqrDistance);
                if (pf.useIt)
                    pf.pfLParam = 0.0f;
            }
        }
    }

    if (pf.useIt) {
        // undo reflections
        for (i = 0; i < 3; i++) {
            if (bReflect[i])
                kPnt[i] = -kPnt[i];
        }

        pf.pfBVec = kPnt;
    }

    return fSqrDistance;
}

public static float XSqDistPtOBB (Vector3 rkPoint, CollisionOBB rkBox, 
                                  SegOBBParams pf)
{
    // compute coordinates of point in box coordinate system
    Vector3 kDiff = rkPoint - rkBox.center;
    Vector3 kClosest = new Vector3(kDiff.Dot(rkBox.axes[0]),
                                   kDiff.Dot(rkBox.axes[1]),
                                   kDiff.Dot(rkBox.axes[2]));

    // project test point onto box
    float fSqrDistance = 0.0f;
    float fDelta;

    for (int i=0; i<3; i++) {
        if (kClosest[i] < -rkBox.extents[i]) {
            fDelta = kClosest[i] + rkBox.extents[i];
            fSqrDistance += fDelta*fDelta;
            kClosest[i] = -rkBox.extents[i];
        }
        else if (kClosest[i] > rkBox.extents[i]) {
            fDelta = kClosest[i] - rkBox.extents[i];
            fSqrDistance += fDelta*fDelta;
            kClosest[i] = rkBox.extents[i];
        }
    }
    if (pf.useIt) {
        pf.pfBVec = kClosest;
    }
    return fSqrDistance;
}


public static float SqrDistSegOBB (Segment rkSeg, CollisionOBB rkBox,
                                   SegOBBParams pf)
{
    Segment kLine = new Segment(rkSeg.origin, rkSeg.direction);

    float fSqrDistance = SqrDistSegOBBInternal(kLine,rkBox,pf);
    if (pf.pfLParam >= 0.0f) {
        if (pf.pfLParam <= 1.0f) {
            return fSqrDistance;
        } else {
            fSqrDistance = XSqDistPtOBB(rkSeg.origin + rkSeg.direction,rkBox,pf);

            if (pf.useIt)
                pf.pfLParam = 1.0f;

            return fSqrDistance;
        }
    } else {
        fSqrDistance = XSqDistPtOBB(rkSeg.origin,rkBox,pf);

        if (pf.useIt)
            pf.pfLParam = 0.0f;

        return fSqrDistance;
    }
}

public static bool TestCapsuleAABB(CollisionCapsule a, CollisionAABB b, CollisionParms parms)
{
    // Convert the AABB into an OBB, then run the OBBOBB test.
    // Not the most efficient algorithm but it gets the job
    // done.
    CollisionOBB newB = new CollisionOBB(b.center, 
                                         UnitBasisVectors,
                                         (b.max - b.min) * .5f);
    return TestCapsuleOBB(a, newB, parms);
}

public static bool TestCapsuleOBB(CollisionCapsule a, CollisionOBB b, CollisionParms parms)
{
    SegOBBParams obbParms = new SegOBBParams(true);
    Segment s = Segment.SegmentFromStartAndEnd(a.bottomcenter, a.topcenter);
    if (SqrDistSegOBB(s, b, obbParms) < a.capRadius * a.capRadius) {
        // If parms.pfLParam == 0, closest is bottomcenter; if == 1,
        // closest is topcenter.

		Vector3 d = a.bottomcenter + obbParms.pfLParam * s.direction;
        // pfBVec is relative to the center of the box, but in box
        // coords.
		Vector3 f = b.center;
		for (int i=0; i<3; i++)
			f += obbParms.pfBVec[i] * b.axes[i];
		Vector3 n = f - d;
        parms.SetNormPart(n);
        parms.SetNormObstacle(-n);
		if (MO.DoLog) {
			MO.Log(" TestCapsuleOBB: pfLParam {0}, pfBVec {1}", obbParms.pfLParam, obbParms.pfBVec);
			MO.Log(" TestCapsuleOBB: d {0}, f {1}", f, d);
			MO.Log(" -n {0}", -n);
		}
		return true;
    }
    else
        return false;
}

// Returns true if the part collides with the obstacle.
public delegate bool TestCollisionDelegate(CollisionShape s1, CollisionShape s2, 
                                           CollisionParms parms);

// This function swaps the arguments so we only have to
// implement the intersection function once
public static bool TestCollisionSwapper(CollisionShape s1, CollisionShape s2,
                                        CollisionParms parms) {
    parms.swapped = true;
    return TestCollisionFunctions[(int)s2.ShapeType(), (int)s1.ShapeType()]
        (s2, s1, parms);
}

//////////////////////////////////////////////////////////////////////
//
// The per-shape-pair collision predicates.  Since collisions
// themselves are much less frequent than the tests for
// collisions, all the effort is in making the tests fast, and
// if we have to repeat the same calculations in the event of
// a collision, it's still a net savings if it makes the tests
// cheaper
//
//////////////////////////////////////////////////////////////////////

public static bool TestCollisionSphereSphere(CollisionShape s1, CollisionShape s2,
                                             CollisionParms parms)
{
    CollisionSphere x1 = (CollisionSphere)s1;
    CollisionSphere x2 = (CollisionSphere)s2;
    Vector3 nS2 = s2.center - s1.center;
    if (x1.radius + x2.radius > nS2.Length) {
        Vector3 n = s2.center - s1.center;
        parms.SetNormPart(n);
        parms.SetNormObstacle(-n);
        return true;
    }
    else
        return false;
}

public static bool TestCollisionSphereCapsule(CollisionShape s1, CollisionShape s2,
                                              CollisionParms parms)
{
    CollisionSphere x1 = (CollisionSphere)s1;
    CollisionCapsule x2 = (CollisionCapsule)s2;
    float rSum = x1.radius + x2.capRadius;
    if (SqDistPointSegment(x2.bottomcenter, x2.topcenter, x1.center) < rSum * rSum)
    {
        float t;
        Vector3 d;
        ClosestPtPointSegment(x1.center, x2.bottomcenter, x2.topcenter, out t, out d);
        Vector3 n = d - x1.center;
        parms.SetNormPart(n);
        parms.SetNormObstacle(-n);
        return true;
    }
    else
        return false;
}

public static bool TestCollisionSphereAABB (CollisionShape s1, CollisionShape s2,
                                            CollisionParms parms)
{
    CollisionSphere x1 = (CollisionSphere)s1;
    CollisionAABB x2 = (CollisionAABB)s2;
    float d = SqDistPointAABB(x1.center, x2);
    if (d < Square(x1.radius)) {
        Vector3 n;
        ClosestPtPointAABB(x1.center, x2, out n);
        n -= x1.center;
        parms.SetNormPart(n);
        // ??? This isn't the correct value of the normal
        parms.SetNormObstacle(-n);
        return true;
    }
    else
        return false;
}

public static bool TestCollisionSphereOBB(CollisionShape s1, CollisionShape s2,
                                          CollisionParms parms)
{
    CollisionSphere x1 = (CollisionSphere)s1;
    CollisionOBB x2 = (CollisionOBB)s2;
    Vector3 closest;
    if (TestSphereOBB(x1, x2, out closest)) {
        Vector3 n = s1.center - closest;
        parms.SetNormPart(n);
        // ??? This isn't the correct value of the normal
        parms.SetNormObstacle(-n);
        return true;
    }
    else
        return false;
}

public static bool TestCollisionCapsuleCapsule (CollisionShape s1, CollisionShape s2,
                                                CollisionParms parms)
{
    CollisionCapsule x1 = (CollisionCapsule)s1;
    CollisionCapsule x2 = (CollisionCapsule)s2;
    return TestCapsuleCapsule(x1, x2, parms);
}

public static bool TestCollisionCapsuleAABB (CollisionShape s1, CollisionShape s2,
                                             CollisionParms parms)
{
    CollisionCapsule x1 = (CollisionCapsule)s1;
    CollisionAABB x2 = (CollisionAABB)s2;
    return TestCapsuleAABB(x1, x2, parms);
}

public static bool TestCollisionCapsuleOBB(CollisionShape s1, CollisionShape s2,
                                           CollisionParms parms)
{
    CollisionCapsule x1 = (CollisionCapsule)s1;
    CollisionOBB x2 = (CollisionOBB)s2;
    return TestCapsuleOBB(x1, x2, parms);
}

public static bool TestCollisionAABBAABB (CollisionShape s1, CollisionShape s2,
                                          CollisionParms parms)
{
    CollisionAABB x1 = (CollisionAABB)s1;
    CollisionAABB x2 = (CollisionAABB)s2;
    if (TestAABBAABB(x1, x2)) {
        Vector3 n = x2.center - x1.center;
        parms.SetNormPart(n);
        // ??? This isn't the correct value of the normal
        parms.SetNormObstacle(-n);
        return true;
    }
    else
        return false;
}

public static bool TestCollisionAABBOBB (CollisionShape s1, CollisionShape s2,
                                         CollisionParms parms)
{
    CollisionAABB x1 = (CollisionAABB)s1;
    CollisionOBB x2 = (CollisionOBB)s2;
    // Convert the AABB into an OBB, then run the OBBOBB test.
    // Not the most efficient algorithm but it gets the job
    // done.
    CollisionOBB newx1 = new CollisionOBB(x1.center, 
                                         UnitBasisVectors,
                                         (x1.max - x1.min) * .5f);
    return TestOBBOBB(newx1, x2, parms);
}

public static bool TestCollisionOBBOBB(CollisionShape s1, CollisionShape s2,
                                       CollisionParms parms)
{
    CollisionOBB x1 = (CollisionOBB)s1;
    CollisionOBB x2 = (CollisionOBB)s2;
    return TestOBBOBB(x1, x2, parms);
}

public static TestCollisionDelegate[,] TestCollisionFunctions = 
    { 
        { 
            TestCollisionSphereSphere,
            TestCollisionSphereCapsule,
            TestCollisionSphereAABB,
            TestCollisionSphereOBB
        },
        { 
            TestCollisionSwapper,
            TestCollisionCapsuleCapsule,
            TestCollisionCapsuleAABB,
            TestCollisionCapsuleOBB
        },
        { 
            TestCollisionSwapper, 
            TestCollisionSwapper, 
            TestCollisionAABBAABB,
            TestCollisionAABBOBB
        },
        { 
            TestCollisionSwapper,
            TestCollisionSwapper,
            TestCollisionSwapper,
            TestCollisionOBBOBB
        }
    };


}}
