#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

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
using Axiom;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Scripting;
using Axiom.Utility;
using Axiom.ParticleSystems;
using Axiom.Input;
using Axiom.Graphics;
using Axiom.Collections;

// TODO: Update with infinite far plane and such.

namespace Axiom.SceneManagers.Octree {
    public enum Visibility {
        None,
        Partial,
        Full
    }

    /// <summary>
    ///** Specialized viewpoint from which an Octree can be rendered.
    ///@remarks
    ///This class contains several speciliazations of the Ogre::Camera class. It
    ///implements the getRenderOperation method inorder to return displayable geometry
    ///for debuggin purposes. It also implements a visibility function that is more granular
    ///than the default.
    /// </summary>
    public class OctreeCamera : Camera {
        #region Fields	

        protected bool useIdentityProj;
        protected bool useIdentityView;

        const int PositionBinding = 0;
        const int ColorBinding = 1;

        static long red = 0xFF0000FF;
        short[] indexes = {0, 1, 1, 2, 2, 3, 3, 0,       //back
                              0, 6, 6, 5, 5, 1,             //left
                              3, 7, 7, 4, 4, 2,             //right
                              6, 7, 5, 4 }; 
        long[] colors = {red, red, red, red, red, red, red, red};

        int[] corners = {0, 4, 3, 5, 2, 6, 1, 7};

        #endregion Fields

        public OctreeCamera(string name, SceneManager scene) : base(name, scene) {
            material = MaterialManager.Instance.GetByName("BaseWhite");
        }

        /// <summary>
        ///     Returns the visiblity of the box.
        /// </summary>
        /// <param name="bound"></param>
        /// <returns></returns>
        
        public Visibility GetVisibility(AxisAlignedBox bound) {
            if(bound.IsNull) {
                return Visibility.None;
            }

            UpdateView();

            Vector3[] boxCorners = bound.Corners;

            // For each plane, see if all points are on the negative side
            // If so, object is not visible.
            // If one or more are, it's partial.
            // If all aren't, full

            bool AllInside = true;

            for(int plane = 0; plane < 6; plane++) {
                bool AllOutside = false;
				
                float distance = 0;

                for(int corner=0;corner<8;corner++) {
                    distance = planes[plane].GetDistance(boxCorners[corners[corner]]);
                    AllOutside = AllOutside && (distance < 0);
                    AllInside = AllInside && (distance >= 0);

                    if(!AllOutside && !AllInside) {
                        break;
                    }
                }

                if(AllOutside) {
                    return Visibility.None;
                }
            }

            if(AllInside) {
                return Visibility.Full;
            }
            else {
                return Visibility.Partial;
            }

        }
		
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="op"></param>
//        public override void GetRenderOperation(RenderOperation op) {
//            Vector3[] r = new Vector3[8];
//
//            r = this.Corners;
//
//            r[0] = GetCorner(FrustumPlane.Far,FrustumPlane.Left,FrustumPlane.Bottom);
//            r[1] = GetCorner(FrustumPlane.Far,FrustumPlane.Left,FrustumPlane.Top);
//            r[2] = GetCorner(FrustumPlane.Far,FrustumPlane.Right,FrustumPlane.Top);
//            r[3] = GetCorner(FrustumPlane.Far,FrustumPlane.Right,FrustumPlane.Bottom);
//
//            r[4] = GetCorner(FrustumPlane.Near,FrustumPlane.Right,FrustumPlane.Top);
//            r[5] = GetCorner(FrustumPlane.Near,FrustumPlane.Left,FrustumPlane.Top);
//            r[6] = GetCorner(FrustumPlane.Near,FrustumPlane.Left,FrustumPlane.Bottom);
//            r[7] = GetCorner(FrustumPlane.Near,FrustumPlane.Right,FrustumPlane.Bottom);
//
//            this.Corners = r;
//
//            UpdateView();
//			
//            //TODO: VERTEX BUFFER STUFF
//        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pp1"></param>
        /// <param name="pp2"></param>
        /// <param name="pp3"></param>
        /// <returns></returns>
        private Vector3 GetCorner(FrustumPlane pp1, FrustumPlane pp2, FrustumPlane pp3) {
            Plane p1 = planes[(int)pp1];
            Plane p2 = planes[(int)pp1];
            Plane p3 = planes[(int)pp1];

            Matrix3 mdet;
			
            mdet.m00 = p1.Normal.x;
            mdet.m01 = p1.Normal.y;
            mdet.m02 = p1.Normal.z;
            mdet.m10 = p2.Normal.x;
            mdet.m11 = p2.Normal.y;
            mdet.m12 = p2.Normal.z;
            mdet.m20 = p3.Normal.x;
            mdet.m21 = p3.Normal.y;
            mdet.m22 = p3.Normal.z;

            float det = mdet.Determinant;

            if(det == 0) {
                //TODO: Unsure. The C++ just returned
                return Vector3.Zero; //some planes are parallel.
            }

            Matrix3 mx = new Matrix3(
                -p1.D,
                p1.Normal.y,
                p1.Normal.z,
                -p2.D,
                p2.Normal.y,
                p2.Normal.z,
                -p3.D,
                p3.Normal.y,
                p3.Normal.z);

            float xdet = mx.Determinant;
			
            Matrix3 my = new Matrix3(		
                p1.Normal.x,
                -p1.D,
                p1.Normal.z,
                p2.Normal.x,
                -p2.D,
                p2.Normal.z,
                p3.Normal.x,
                -p3.D,
                p3.Normal.z);

            float ydet = my.Determinant;

            Matrix3 mz = new Matrix3(
                p1.Normal.x,
                p1.Normal.y,
                -p1.D,
                p2.Normal.x,
                p2.Normal.y,
                -p2.D,
                p3.Normal.x,
                p3.Normal.y,
                -p3.D);
	
            float zdet = mz.Determinant;

            Vector3 r = new Vector3();
            r.x	= xdet / det;
            r.y = ydet / det;
            r.z = zdet / det;

            return r;
        }
    }
}
