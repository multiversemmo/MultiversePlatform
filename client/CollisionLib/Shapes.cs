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
using AxisAlignedBox = Axiom.MathLib.AxisAlignedBox;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Multiverse.CollisionLib;

#endregion

namespace Multiverse.CollisionLib
{

    public class Segment
    {
        public Vector3 origin;
        public Vector3 direction;
        public Segment(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }
        static public Segment SegmentFromStartAndEnd(Vector3 start, Vector3 end)
        {
            return new Segment(start, end - start);
        }
    }

    // These are the fundamental kinds of shapes we support.  They are
    // enumerated because we need to be able to compute the
    // intersection of any pair of them, and the intersection code
    // depends on the details of each of the two shapes.
    public enum ShapeEnum { ShapeSphere, ShapeCapsule, ShapeAABB, ShapeOBB };

    // The base class for everything that tests containment:
    // CollisionShapes; SphereTreeNodes, etc.
    public abstract class BasicSphere
    {
        public Vector3 center;
        public float radius;

        public BasicSphere(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public BasicSphere()
        {
            this.center = Vector3.Zero;
            this.radius = 0;
        }

        public bool Contains(BasicSphere s)
        {
            float sqSep = Primitives.DistanceSquared(s.center, center);
            return (sqSep + s.radius * s.radius) <= radius * radius;
        }

        public bool PointInsideSphere(Vector3 p) 
        {
            return Primitives.DistanceSquared(p, center) <= radius * radius;
        }
        
        public bool SphereOverlap(BasicSphere S)
        {
            return Primitives.DistanceSquared(center, S.center) <
                Primitives.Square(radius + S.radius);
        }
    }

    // The base class for all shapes that can collide with each other.
    // The key exports are the "center" of the shape and the radius of
    // the containment sphere for the shape.
    public abstract class CollisionShape : BasicSphere
    {
        public long handle;
        public abstract ShapeEnum ShapeType();
        public abstract void AddDisplacement(Vector3 displacement);
        // The maximum distance for a single step in the direction of the
        // displacement vector such that we're sure we won't "punch
        // through" a narrow obstacle
        protected const float PunchThroughFraction = 0.6f;
        // Value to check scale transforms to make sure they are close
        protected const float ScaleEpsilon = 0.0001f;
        public abstract float StepSize(Vector3 displacement);
		public ulong timeStamp;
        public CollisionShape() { timeStamp = 0; handle = 0; }
        public CollisionShape(Vector3 center, float radius)
            : base(center, radius)
        {
            timeStamp = 0;
            handle = 0;
        }
        public abstract void Transform(Vector3 scale, Quaternion rotate, Vector3 translate);

        public abstract CollisionShape Clone();
        
        public abstract bool PointInside(Vector3 p);
        
        protected static bool IsUniformScale(Vector3 scale) {
            if (Math.Abs(scale.x - scale.y) > ScaleEpsilon || 
                Math.Abs(scale.y - scale.z) > ScaleEpsilon)
                return false;
            return true;
        }

		protected float PickShortestDistance(int roots, float []afT)
		{
			if (roots == 0)
				return float.MaxValue;
			else if (roots == 1)
				return afT[0];
			else 
				return Math.Min(afT[0], afT[1]);
		}
		
		public abstract float RayIntersectionDistance(Vector3 start, Vector3 end);

        public abstract AxisAlignedBox BoundingBox();
    }

    public class CollisionSphere : CollisionShape
    {
        public CollisionSphere(Vector3 center, float radius) : base(center, radius) { }
        public CollisionSphere() { }
        public override ShapeEnum ShapeType() { return ShapeEnum.ShapeSphere; }
        public override void AddDisplacement(Vector3 displacement) { center += displacement; }

        public override float StepSize(Vector3 displacement) 
		{
			return radius * 2.0f * PunchThroughFraction;
		}

        public override bool PointInside(Vector3 p)
        {
            return PointInsideSphere(p);
        }
        
        public bool BelowPoint(Vector3 p, ref float distance)
		{
			float xdiff = p.x - center.x;
			float zdiff = p.z - center.z;
			float xzdiffsq = xdiff * xdiff + zdiff * zdiff;
			float rsq = radius * radius;
			if (xzdiffsq > rsq)
				return false;
			float ydiff = p.y - center.y;
			float c = xzdiffsq + ydiff * ydiff;
			float b = 2.0f * ydiff;
			float disc = b * b - 4 * c;
			Debug.Assert(disc >= 0, string.Format("Sphere {0} not below point {1}", this, p.ToString()));
			float rootDisc = (float)Math.Sqrt((double)disc);
			float t1 = (- b + rootDisc) * 0.5f;
			float t2 = (b + rootDisc) * 0.5f;
			distance = (Math.Min(t1, t2));
			return true;
		}

        public override string ToString()
        {
            return string.Format("Sphere(handle {0}, center{1}, radius {2})",
                                 MO.HandleString(handle), MO.MeterString(center), MO.MeterString(radius));
        }

        public override void Transform(Vector3 scale, Quaternion rotate, Vector3 translate) {
            if (!IsUniformScale(scale))
                throw new Exception("Unsupported non-uniform scale on Sphere");
            // Update radius and center to take the scale into account
            radius *= scale.x;
            center *= scale.x;
            // We can ignore the rotate for a sphere
            // Update the center to take the translate into account
            center += translate;
        }
        public override CollisionShape Clone() {
            CollisionSphere rv = new CollisionSphere(center, radius);
            rv.handle = this.handle;
            rv.timeStamp = this.timeStamp;
            return rv;
        }

		public override float RayIntersectionDistance(Vector3 start, Vector3 end)
		{	
			// set up quadratic Q(t) = a*t^2 + 2*b*t + c
			Vector3 kDiff = start - center;
			Vector3 direction = end - start;
			float fA = direction.LengthSquared;
			float fB = kDiff.Dot(direction);
			float fC = kDiff.LengthSquared - radius * radius;

			float afT0, afT1;
			float fDiscr = fB*fB - fA*fC;
			if (fDiscr < 0.0) {
				return float.MaxValue;
			}
			else if (fDiscr > 0.0) {
				float fRoot = (float)Math.Sqrt(fDiscr);
				float fInvA = 1.0f/fA;
				afT0 = (-fB - fRoot)*fInvA;
				afT1 = (-fB + fRoot)*fInvA;
                if (afT1 >= 0.0)
                    return Math.Min((afT0 * direction).Length, (afT1 * direction).Length);
                else if (afT1 >= 0.0)
                    return ((start + afT1 * direction) - start).Length;
                else
                    return float.MaxValue;
			}
			else {
				afT0 = -fB/fA;
				if ( afT0 >= 0.0 )
					return afT0 * direction.Length;
				else
					return float.MaxValue;
			}
		}

        public override AxisAlignedBox BoundingBox() 
        {
            return new AxisAlignedBox(
                new Vector3(center.x - radius, center.y - radius, center.z - radius),
                new Vector3(center.x + radius, center.y + radius, center.z + radius));
        }
    }

    public class CollisionCapsule : CollisionShape
    {
        public Vector3 bottomcenter;
        public Vector3 topcenter;
        public float height;
        public float capRadius;

        public CollisionCapsule(Vector3 bottomcenter, Vector3 topcenter, float capRadius)
        {
            this.bottomcenter = bottomcenter;
            this.topcenter = topcenter;
            this.capRadius = capRadius;
            this.height = (topcenter - bottomcenter).Length;
            this.center = (topcenter + bottomcenter) / 2;
            this.radius = height + capRadius;
        }

        public override ShapeEnum ShapeType() { return ShapeEnum.ShapeCapsule; }

        public override void Transform(Vector3 scale, Quaternion rotate, Vector3 translate)
        {
            if (!IsUniformScale(scale))
                throw new Exception("Unsupported non-uniform scale on Capsule");
            Matrix4 transform = Matrix4.Identity;
            transform.Scale = scale;
            transform = rotate.ToRotationMatrix() * transform;
            transform.Translation = translate;
            bottomcenter = transform * bottomcenter;
            topcenter = transform * topcenter;
            capRadius *= scale.x;
            this.height = (topcenter - bottomcenter).Length;
            this.center = (topcenter + bottomcenter) * 0.5f;
            this.radius = height * 0.5f + capRadius;
        }

        public override void AddDisplacement(Vector3 displacement)
        {
            center += displacement;
            bottomcenter += displacement;
            topcenter += displacement;
        }

        public override float StepSize(Vector3 displacement) { return capRadius * 2.0f * PunchThroughFraction; }

        public override string ToString()
        {
            return string.Format("Capsule(handle {0}, bottom{1}, top{2}, capRadius {3})",
                                 MO.HandleString(handle), MO.MeterString(bottomcenter), MO.MeterString(topcenter), 
                                 MO.MeterFractionString(capRadius));
        }
        public override CollisionShape Clone() {
            CollisionCapsule rv = new CollisionCapsule(bottomcenter, topcenter, capRadius);
            rv.handle = this.handle;
            rv.timeStamp = this.timeStamp;
            return rv;
        }

        public override bool PointInside(Vector3 p)
        {
            return PointInsideSphere(p) && 
                   Primitives.SqDistPointSegment(bottomcenter, topcenter, p) <= capRadius * capRadius;
        }
        
		void GenerateOrthonormalBasis (out Vector3 rkU, out Vector3 rkV, ref Vector3 rkW)
		{
			rkU = Vector3.Zero;
			if ( Math.Abs(rkW.x) >= Math.Abs(rkW.y)
				 && Math.Abs(rkW.x) >= Math.Abs(rkW.z) )
				{
					rkU.x = -rkW.y;
					rkU.y = +rkW.x;
					rkU.z = 0.0f;
				}
			else
				{
					rkU.x = 0.0f;
					rkU.y = +rkW.z;
					rkU.z = -rkW.y;
				}

			rkU.ToNormalized();
			rkV = rkW.Cross(rkU);
		}

		public override float RayIntersectionDistance(Vector3 start, Vector3 end)
		{
			float []afT;
			int roots = RayIntersectionDistanceInternal(start, end, out afT);
			if (roots == 0)
				return float.MaxValue;
			else if (roots == 1)
				return afT[0];
			else 
				return Math.Min(afT[0], afT[1]) * (end - start).Length;
		}
		
		private int RayIntersectionDistanceInternal(Vector3 start, Vector3 end,
													out float []afT)
		{
			Vector3 direction = end - start;
			Vector3 capDirection = topcenter - bottomcenter;
			// set up quadratic Q(t) = a*t^2 + 2*b*t + c
			Vector3 kU;
			Vector3 kV;
			Vector3 kW = capDirection;
			float fWLength = kW.Normalize();
			float fInvWLength = 1.0f / fWLength;
			GenerateOrthonormalBasis(out kU, out kV, ref kW);
			Vector3 kD = new Vector3(kU.Dot(direction), kV.Dot(direction), kW.Dot(direction));
			float fDLength = kD.Normalize();
			float fInvDLength = 1.0f / fDLength;
			Vector3 kDiff = start - bottomcenter;
			Vector3 kP = new Vector3(kU.Dot(kDiff), kV.Dot(kDiff), kW.Dot(kDiff));
			float fRadiusSqr = capRadius * capRadius;
			afT = new float[] {0f, 0f};
			float fInv, fA, fB, fC, fDiscr, fRoot, fT, fTmp;

			if ( Math.Abs(kD.z) >= 1.0 - ScaleEpsilon) {
				// line is parallel to capsule axis
				fDiscr = fRadiusSqr - kP.x*kP.x - kP.y*kP.y;
				if (fDiscr >= 0.0) {
					fRoot = (float)Math.Sqrt(fDiscr);
					afT[0] = -(kP.z + fRoot)*fInvDLength;
					afT[1] = (fWLength - kP.z + fRoot)*fInvDLength;
					return 2;
				}
				else
					{
						return 0;
					}
			}

			// test intersection with infinite cylinder
			fA = kD.x*kD.x + kD.y*kD.y;
			fB = kP.x*kD.x + kP.y*kD.y;
			fC = kP.x*kP.x + kP.y*kP.y - fRadiusSqr;
			fDiscr = fB*fB - fA*fC;
			if (fDiscr < 0.0)
				return 0;

			int iQuantity = 0;

			if (fDiscr > 0.0) {
				// line intersects infinite cylinder in two places
				fRoot = (float)Math.Sqrt(fDiscr);
				fInv = 1.0f / fA;
				fT = (-fB - fRoot)*fInv;
				fTmp = kP.z + fT*kD.z;
				if (0.0f <= fTmp && fTmp <= fWLength)
					afT[iQuantity++] = fT*fInvDLength;

				fT = (-fB + fRoot)*fInv;
				fTmp = kP.z + fT*kD.z;
				if (0.0f <= fTmp && fTmp <= fWLength)
					afT[iQuantity++] = fT*fInvDLength;

				if (iQuantity == 2)
					return 2;
			}
			else {
				// line is tangent to infinite cylinder
				fT = -fB/fA;
				fTmp = kP.z + fT*kD.z;
				if (0.0 <= fTmp && fTmp <= fWLength) {
					afT[0] = fT*fInvDLength;
					return 1;
				}
			}

			// test intersection with bottom hemisphere
			// fA = 1
			fB += kP.z*kD.z;
			fC += kP.z*kP.z;
			fDiscr = fB*fB - fC;
			if (fDiscr > 0.0) {
				fRoot = (float)Math.Sqrt(fDiscr);
				fT = -fB - fRoot;
				fTmp = kP.z + fT*kD.z;
				if (fTmp <= 0.0f) {
					afT[iQuantity++] = fT*fInvDLength;
					if (iQuantity == 2)
						return 2;
				}

				fT = -fB + fRoot;
				fTmp = kP.z + fT*kD.z;
				if (fTmp <= 0.0) {
					afT[iQuantity++] = fT*fInvDLength;
					if (iQuantity == 2)
						return 2;
				}
			}
			else if (fDiscr == 0.0) {
				fT = -fB;
				fTmp = kP.z + fT*kD.z;
				if (fTmp <= 0.0) {
					afT[iQuantity++] = fT*fInvDLength;
					if ( iQuantity == 2 )
						return 2;
				}
			}

			// test intersection with top hemisphere
			// fA = 1
			fB -= kD.z*fWLength;
			fC += fWLength*(fWLength - 2.0f*kP.z);

			fDiscr = fB*fB - fC;
			if (fDiscr > 0.0f) {
				fRoot = (float)Math.Sqrt(fDiscr);
				fT = -fB - fRoot;
				fTmp = kP.z + fT*kD.z;
				if (fTmp >= fWLength) {
					afT[iQuantity++] = fT*fInvDLength;
					if (iQuantity == 2)
						return 2;
				}

				fT = -fB + fRoot;
				fTmp = kP.z + fT*kD.z;
				if (fTmp >= fWLength) {
					afT[iQuantity++] = fT*fInvDLength;
					if (iQuantity == 2)
						return 2;
				}
			}
			else if (fDiscr == 0.0) {
				fT = -fB;
				fTmp = kP.z + fT*kD.z;
				if (fTmp >= fWLength) {
					afT[iQuantity++] = fT*fInvDLength;
					if (iQuantity == 2)
						return 2;
				}
			}
			return iQuantity;
		}

        public override AxisAlignedBox BoundingBox() 
        {
            return new AxisAlignedBox(
                new Vector3(
                    Math.Min(topcenter.x - radius, bottomcenter.x - radius),
                    Math.Min(topcenter.y - radius, bottomcenter.y - radius),
                    Math.Min(topcenter.z - radius, bottomcenter.z - radius)),
                new Vector3(
                    Math.Min(topcenter.x + radius, bottomcenter.x + radius),
                    Math.Min(topcenter.y + radius, bottomcenter.y + radius),
                    Math.Min(topcenter.z + radius, bottomcenter.z + radius)));
        }

    }

    // Either an AABB or an OBB
    public abstract class CollisionBox : CollisionShape
    {
        // cornerNumber is 0 - 7
        public abstract Vector3 Corner(int cornerNumber);

        // A 2-dimensional array, indexed by corner number (0-7) and face
        // number (0-2), giving the two other corners that form the ends
        // of vectors with originate with the indexing corner, such that
        // when we form the cross product of the two vectors in order, we
        // get the "outward facing" normal to the face
        public static int[, ,] cornerFaces = { 
        ///          1-----2
        ///         /|    /|
        ///       /  |   / |
        ///     5-----4|/  |
        ///     |   0--|---3
        ///     |  /   |  /
        ///     |/     |/
        ///     6-----7

        // Corner 0
        { {1,3}, {6,1}, {3,6} },
        // Corner 1
        { {2,0}, {5,2}, {0,5} },
        // Corner 2
        { {1,4}, {4,3}, {3,1} },
        // Corner 3
        { {0,2}, {2,7}, {7,0} },
        // Corner 4
        { {5,7}, {2,5}, {7,2} },
        // Corner 5
        { {4,1}, {6,4}, {1,6} },
        // Corner 6
        { {7,6}, {5,0}, {0,7} },
        // Corner 7
        { {6,3}, {4,6}, {3,4} }
    };

        // Returns a (not-necessaryly unit) normal vector
        public Vector3 NormalVector(Vector3 p, Vector3 displacement)
        {
            const float cornerEpsilon = 0.001f;
            // First find out if we're at a corner
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = Corner(i);
                Vector3 d = (p - corner);
                if (d.LengthSquared < cornerEpsilon)
                {
                    // This is the corner, so figure out which face to
                    // return.  Robin says that the best face is the one
                    // most in line with the displacement vector, which is
                    // the face that has the smallest cross product with
                    // the displacement vector
                    float minCross = float.MaxValue;
                    Vector3 norm = Vector3.Zero;
                    for (int j = 0; j < 3; j++)
                    {
                        Vector3 tc1 = Corner(cornerFaces[i, j, 0]) - corner;
                        Vector3 tc2 = Corner(cornerFaces[i, j, 1]) - corner;
                        // I wish we didn't have to normalize the bastard;
                        // sqrt is expensive!
                        Vector3 cross = tc1.Cross(tc2).ToNormalized();
                        float cd = cross.Dot(displacement);
                        if (cd < minCross)
                        {
                            minCross = cd;
                            norm = cross;
                        }
                    }
                    return norm;
                }
            }
            // It's not at a corner, so return the norm of the face
            // containing point p

            // ??? Need to finish
            return Vector3.Zero;
        }
        public override void Transform(Vector3 scale, Quaternion rotate, Vector3 translate) {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class CollisionAABB : CollisionBox
    {
        public Vector3 min;
        public Vector3 max;

        public CollisionAABB(Vector3 min, Vector3 max)
        {
            float s = 0.0f;
            this.min = min;
            this.max = max;
            for (int i = 0; i < 3; i++)
            {
                this.center[i] = (min[i] + max[i]) / 2;
                s += ((max[i] - min[i]) * (max[i] - min[i]));
            }
            this.radius = (float)Math.Sqrt((double)(s / 4));
        }

        public override ShapeEnum ShapeType() { return ShapeEnum.ShapeAABB; }

        public override void AddDisplacement(Vector3 displacement)
        {
            center += displacement;
            min += displacement;
            max += displacement;
        }

        public override float StepSize(Vector3 displacement)
        {
            float s = float.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                float d = max[i] - min[i];
                s = Math.Min(s, d);
            }
            return s * PunchThroughFraction;
        }

        public override Vector3 Corner(int cornerNumber)
        {
            switch (cornerNumber)
            {
                case 0:
                    return min;
                case 1:
                    return new Vector3(min.x, max.y, min.z);
                case 2:
                    return new Vector3(max.x, max.y, min.z);
                case 3:
                    return new Vector3(max.x, min.y, min.z);
                case 4:
                    return max;
                case 5:
                    return new Vector3(min.x, max.y, max.z);
                case 6:
                    return new Vector3(min.x, min.y, max.z);
                case 7:
                    return new Vector3(max.x, min.y, max.z);
                default:
                    Debug.Assert(false, "cornerNumber not 0-7!");
                    return Vector3.Zero;
            }
        }
        public override string ToString()
        {
            return string.Format("AABB(handle {0}, min{1}, max{2})",
                                 MO.HandleString(handle), MO.MeterFractionString(min), MO.MeterFractionString(max));
        }
        public override CollisionShape Clone() {
            CollisionAABB rv = new CollisionAABB(min, max);
            rv.handle = this.handle;
            rv.timeStamp = this.timeStamp;
            return rv;
        }
        public override void Transform(Vector3 scale, Quaternion rotate, Vector3 translate) {
            if (rotate != Quaternion.Identity)
                throw new Exception("Unsupported non-identity rotation on AABB");
            Matrix4 transform = Matrix4.Identity;
            transform.Scale = scale;
            transform = rotate.ToRotationMatrix() * transform;
            transform.Translation = translate;
            float s = 0.0f;
            this.min = transform * min;
            this.max = transform * max;
            for (int i = 0; i < 3; i++) {
                this.center[i] = (min[i] + max[i]) / 2;
                s += ((max[i] - min[i]) * (max[i] - min[i]));
            }
            this.radius = (float)Math.Sqrt((double)(s / 4));
        }

        public override bool PointInside(Vector3 p)
        {
            return PointInsideSphere(p) && 
                   Primitives.SqDistPointAABB(p, this) < ScaleEpsilon;
        }
        
		public CollisionOBB OBB()
		{
			return new CollisionOBB(center,
									Primitives.UnitBasisVectors,
									(max - min) * .5f);
		}

		public override float RayIntersectionDistance(Vector3 start, Vector3 end)
		{
			return OBB().RayIntersectionDistance(start, end);
		}

        public override AxisAlignedBox BoundingBox() 
        {
            return new AxisAlignedBox(min, max);
        }
        
    }

    public class CollisionOBB : CollisionBox
    {
        public Vector3[] axes;
        public Vector3 extents;

        public CollisionOBB(Vector3 center, Vector3[] axes, Vector3 extents)
        {
            this.center = center;
            this.axes = axes;
            this.extents = extents;
            this.radius = extents.Length;
        }

        public override ShapeEnum ShapeType() { return ShapeEnum.ShapeOBB; }

        public override void AddDisplacement(Vector3 displacement)
        {
            center += displacement;
        }

        public override float StepSize(Vector3 displacement)
        {
            float s = float.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                s = Math.Min(s, extents[i]);
            }
            return s * 2 * PunchThroughFraction;
        }

        public override Vector3 Corner(int cornerNumber)
        {
            Vector3 cornerSum = center;
            for (int i=0; i<3; i++) {
                float multiplier = ((1<<i) & cornerNumber) == 0 ? 1f : -1f;
                cornerSum += multiplier * extents[i] * axes[i];
            }
            return cornerSum;
        }

        public override string ToString()
        {
            return string.Format("OBB(handle {0}, center{1}, axis1{2}, axis2{3}, axis3{4}, extents{5})",
                                 MO.HandleString(handle), MO.MeterFractionString(center),
                                 MO.AxisString(axes[0]), MO.AxisString(axes[1]), MO.AxisString(axes[2]),
                                 MO.MeterFractionString(extents));
        }

        public override CollisionShape Clone() {
            Vector3[] newAxes = new Vector3[3];
            newAxes[0] = axes[0];
            newAxes[1] = axes[1];
            newAxes[2] = axes[2];
            CollisionOBB rv = new CollisionOBB(center, newAxes, extents);
            rv.handle = this.handle;
            rv.timeStamp = this.timeStamp;
            return rv;
        }
        public override void Transform(Vector3 scale, Quaternion rotate, Vector3 translate) {
            if (!IsUniformScale(scale))
                throw new Exception("Unsupported non-uniform scale on OBB");
            Matrix4 transform = Matrix4.Identity;
            transform.Scale = scale;
            transform = rotate.ToRotationMatrix() * transform;
            transform.Translation = translate; 
            this.center = transform * center;
            for (int i = 0; i < this.axes.Length; ++i)
                this.axes[i] = rotate * axes[i];
            this.extents = scale.x * extents;
            this.radius = extents.Length;
        }

		public override float RayIntersectionDistance(Vector3 start, Vector3 end)
		{
			Vector3 []afT;
			int roots = RayIntersectionDistanceInternal(start, end, out afT);
			if (roots == 0)
				return float.MaxValue;
			else if (roots == 1)
				return (start - afT[0]).Length;
			else 
				return Math.Min((start - afT[0]).Length, (start - afT[1]).Length);
		}
		
        public override bool PointInside(Vector3 p)
        {
            Vector3 junk;
            return PointInsideSphere(p) && 
                   Primitives.SqDistPointOBB(p, this, out junk) < ScaleEpsilon;
        }
        
		private int RayIntersectionDistanceInternal(Vector3 start, Vector3 end,
													out Vector3 []afT)
		{
			int riQuantity;
			// convert ray to box coordinates
			Vector3 direction = end - start;
			Vector3 kDiff = start - center;
			Vector3 kOrigin = new Vector3(kDiff.Dot(axes[0]), kDiff.Dot(axes[1]), kDiff.Dot(axes[2]));
			Vector3 kDirection = new Vector3(direction.Dot(axes[0]), direction.Dot(axes[1]), direction.Dot(axes[2]));
			float fT0 = 0.0f;
			float fT1 = float.MaxValue;
			afT = new Vector3[] {Vector3.Zero, Vector3.Zero};
			bool bIntersects = FindIntersection(kOrigin, kDirection, ref fT0, ref fT1);

			if (bIntersects) {
				if (fT0 > 0.0f) {
					if (fT1 <= 1.0f) {
						riQuantity = 2;
						afT[0] = start + fT0*direction;
						afT[1] = start + fT1*direction;
					}
					else {
						riQuantity = 1;
						afT[0] = start + fT0*direction;
					}
				}
				else {  // fT0 == 0.0
					if (fT1 <= 1.0f) {
						riQuantity = 1;
						afT[0] = start + fT1*direction;
					}
					else { // fT1 == INFINITY
						// assert:  should not get here
						riQuantity = 0;
					}
				}
			}
			else
				riQuantity = 0;
			
			return riQuantity;
		}

		bool FindIntersection (Vector3 start, Vector3 direction, ref float rfT0, ref float rfT1)
		{
			float fSaveT0 = rfT0;
			float fSaveT1 = rfT1;

			bool bNotEntirelyClipped =
				Clip(+ direction.x, - start.x - extents[0], ref rfT0, ref rfT1) &&
				Clip(- direction.x, + start.x - extents[0], ref rfT0, ref rfT1) &&
				Clip(+ direction.y, - start.y - extents[1], ref rfT0, ref rfT1) &&
				Clip(- direction.y, + start.y - extents[1], ref rfT0, ref rfT1) &&
				Clip(+ direction.z, - start.z - extents[2], ref rfT0, ref rfT1) &&
				Clip(- direction.z, + start.z - extents[2], ref rfT0, ref rfT1);

			return bNotEntirelyClipped && (rfT0 != fSaveT0 || rfT1 != fSaveT1);
		}

		bool Clip (float fDenom, float fNumer, ref float rfT0, ref float rfT1)
		{
			// Return value is 'true' if line segment intersects the current test
			// plane.  Otherwise 'false' is returned in which case the line segment
			// is entirely clipped.

			if (fDenom > 0.0) {
				if (fNumer > fDenom*rfT1)
					return false;
				if (fNumer > fDenom*rfT0)
					rfT0 = fNumer/fDenom;
				return true;
			}
			else if (fDenom < 0.0) {
				if (fNumer > fDenom*rfT0)
					return false;
				if (fNumer > fDenom*rfT1)
					rfT1 = fNumer/fDenom;
				return true;
			}
			else
				return fNumer <= 0.0;
		}

        public override AxisAlignedBox BoundingBox() 
        {
            Vector3 min = center;
            Vector3 max = center;
            for(int i=0; i<8; i++) {
                Vector3 point = Corner(i);
                min.x = Math.Min(min.x, point.x);
                min.y = Math.Min(min.y, point.y);
                min.z = Math.Min(min.z, point.z);
                max.x = Math.Max(max.x, point.x);
                max.y = Math.Max(max.y, point.y);
                max.z = Math.Max(max.z, point.z);
            }
            return new AxisAlignedBox(min, max);
        }
        
    }

    // These are passed in by the API caller of TestCollision, and contain
    // the return values: the colliding part and collision shape, and the
    // normal vectors.  Generation of the normal vectors is controlled by
    // a boolean.
    public class CollisionParms
    {
        public bool genNormals;
        public CollisionShape part;
        public CollisionShape obstacle;
        public bool swapped;   // Are the normals swapped
        public Vector3 normPart;
        public Vector3 normObstacle;

        public CollisionParms()
        {
            genNormals = true;
            Initialize();
        }

        public void Initialize()
        {
            part = null;
            obstacle = null;
            swapped = false;
            normPart = Vector3.Zero;
            normObstacle = Vector3.Zero;
        }

        public void SetNormPart(Vector3 norm)
        {
            if (swapped)
                normObstacle = norm;
            else
                normPart = norm;
        }

        public void SetNormObstacle(Vector3 norm)
        {
            if (swapped)
                normPart = norm;
            else
                normObstacle = norm;
        }
    }

}
