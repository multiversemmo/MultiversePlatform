#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

The math library included in this project, in addition to being a derivative of
the works of Ogre, also include derivative work of the free portion of the 
Wild Magic mathematics source code that is distributed with the excellent
book Game Engine Design.
http://www.wild-magic.com/

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Axiom.MathLib {
    /// <summary>
    ///		A Catmull-Rom spline that can be used for interpolating translation movements.
    /// </summary>
    /// <remarks>
    ///		A Catmull-Rom spline is a derivitive of the Hermite spline.  The difference is that the Hermite spline
    ///		allows you to specifiy 2 endpoints and 2 tangents, then the spline is generated.  A Catmull-Rom spline
    ///		allows you to just supply 1-n number of points and the tangents will be automatically calculated.
    ///		<p/>
    ///		Derivation of the hermite polynomial can be found here: 
    ///		<a href="http://www.cs.unc.edu/~hoff/projects/comp236/curves/papers/hermite.html">Hermite splines.</a>
    /// </remarks>
    public sealed class PositionalSpline {
        #region Member variables

        readonly private Matrix4 hermitePoly = new Matrix4(	
			2, -2,  1,  1,
            -3,  3, -2, -1,
            0,  0,  1,  0,
            1,  0,  0,  0);

        /// <summary>Collection of control points.</summary>
        private List<Vector3> pointList;
        /// <summary>Collection of generated tangents for the spline controls points.</summary>
        private List<Vector3> tangentList;
        /// <summary>Specifies whether or not to recalculate tangents as each control point is added.</summary>
        private bool autoCalculateTangents;

        #endregion

        #region Constructors

        /// <summary>
        ///		Creates a new Positional Spline.
        /// </summary>
        public PositionalSpline() {
            // intialize the vector collections
            pointList = new List<Vector3>();
            tangentList = new List<Vector3>();

            // do not auto calculate tangents by default
            autoCalculateTangents = false;
        }

        #endregion

        #region Public properties

        /// <summary>
        ///		Specifies whether or not to recalculate tangents as each control point is added.
        /// </summary>
        public bool AutoCalculate {
            get { 
                return autoCalculateTangents; 
            }
            set { 
                autoCalculateTangents = value; 
            }
        }

        /// <summary>
        ///    Gets the number of control points in this spline.
        /// </summary>
        public int PointCount {
            get {
                return pointList.Count;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        ///    Adds a new control point to the end of this spline.
        /// </summary>
        /// <param name="point"></param>
        public void AddPoint(Vector3 point) {
            pointList.Add(point);

            // recalc tangents if necessary
            if(autoCalculateTangents)
                RecalculateTangents();
        }

        /// <summary>
        ///    Removes all current control points from this spline.
        /// </summary>
        public void Clear() {
            pointList.Clear();
            tangentList.Clear();
        }

        /// <summary>
        ///     Returns the point at the specified index.
        /// </summary>
        /// <param name="index">Index at which to retreive a point.</param>
        /// <returns>Vector3 containing the point data.</returns>
        public Vector3 GetPoint(int index) {
            Debug.Assert(index < pointList.Count);

            return pointList[index];
        }

        /// <summary>
        ///		Returns an interpolated point based on a parametric value over the whole series.
        /// </summary>
        /// <remarks>
        ///		Given a t value between 0 and 1 representing the parametric distance along the
        ///		whole length of the spline, this method returns an interpolated point.
        /// </remarks>
        /// <param name="t">Parametric value.</param>
        /// <returns>An interpolated point along the spline.</returns>
        public Vector3 Interpolate(float t) {
            // This does not take into account that points may not be evenly spaced.
            // This will cause a change in velocity for interpolation.

            // What segment this is in?
            float segment = t * pointList.Count;
            int segIndex = (int)segment;

            // apportion t
            t = segment - segIndex;

            // call the overloaded method
            return Interpolate(segIndex, t);
        }

        /// <summary>
        ///		Interpolates a single segment of the spline given a parametric value.
        /// </summary>
        /// <param name="index">The point index to treat as t=0. index + 1 is deemed to be t=1</param>
        /// <param name="t">Parametric value</param>
        /// <returns>An interpolated point along the spline.</returns>
        public Vector3 Interpolate(int index, float t) {
            Debug.Assert(index >= 0 && index < pointList.Count, "Spline point index overrun.");

            if((index + 1) == pointList.Count) {
                // cant interpolate past the end of the list, just return the last point
                return pointList[index];
            }

            // quick special cases
            if(t == 0.0f)
                return pointList[index];
            else if(t == 1.0f)
                return pointList[index + 1];

            // Time for real interpolation
            // Construct a Vector4 of powers of 2
            float t2, t3;
            // t^2
            t2 = t * t; 
            // t^3
            t3 = t2 * t;

            Vector4 powers = new Vector4(t3, t2, t, 1);

            // Algorithm is result = powers * hermitePoly * Matrix4(point1, point2, tangent1, tangent2)
            Vector3 point1 = pointList[index];
            Vector3 point2 = pointList[index + 1];
            Vector3 tangent1 = tangentList[index];
            Vector3 tangent2 = tangentList[index + 1];
            Matrix4 point = new Matrix4();

            // create the matrix 4 with the 2 point and tangent values
            point.m00 = point1.x;
            point.m01 = point1.y;
            point.m02 = point1.z;
            point.m03 = 1.0f;
            point.m10 = point2.x;
            point.m11 = point2.y;
            point.m12 = point2.z;
            point.m13 = 1.0f;
            point.m20 = tangent1.x;
            point.m21 = tangent1.y;
            point.m22 = tangent1.z;
            point.m23 = 1.0f;
            point.m30 = tangent2.x;
            point.m31 = tangent2.y;
            point.m32 = tangent2.z;
            point.m33 = 1.0f;

            // get the final result in a Vector4
            Vector4 result = powers * hermitePoly * point;

            // return the final result
            return new Vector3(result.x, result.y, result.z);
        }

        /// <summary>
        ///		Recalculates the tangents associated with this spline. 
        /// </summary>
        /// <remarks>
        ///		If you tell the spline not to update on demand by setting AutoCalculate to false,
        ///		then you must call this after completing your updates to the spline points.
        /// </remarks>
        public void RecalculateTangents() {
            // Catmull-Rom approach
            // tangent[i] = 0.5 * (point[i+1] - point[i-1])

            // TODO: Resize tangent list and use existing elements rather than clear/readd every time.
 
            tangentList.Clear();

            int i, numPoints;
            bool isClosed;

            numPoints = pointList.Count;

            // if there arent at least 2 points, there is nothing to inerpolate
            if(numPoints < 2)
                return;

            // closed or open?
            if(pointList[0] == pointList[numPoints - 1])
                isClosed = true;
            else
                isClosed = false;

            // loop through the points and generate the tangents
            for(i = 0; i < numPoints; i++) {
                // special cases for first and last point in list
                if(i ==0) {
                    if(isClosed) {
                        // Use numPoints-2 since numPoints-1 is the last point and == [0]
                        tangentList.Add(0.5f * (pointList[1] - pointList[numPoints - 2]));
                    }
                    else
                        tangentList.Add(0.5f * (pointList[1] - pointList[0]));
                }
                else if(i == numPoints - 1) {
                    if(isClosed) {
                        // Use same tangent as already calculated for [0]
                        tangentList.Add(tangentList[0]);
                    }
                    else
                        tangentList.Add(0.5f * (pointList[i] - pointList[i - 1]));
                }
                else
                    tangentList.Add(0.5f * (pointList[i + 1] - pointList[i - 1]));
            }
        }

        #endregion
    }
}
