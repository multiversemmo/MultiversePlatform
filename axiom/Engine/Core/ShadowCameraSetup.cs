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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Axiom.MathLib;
using Axiom.Graphics;

namespace Axiom.Core {

    /// <summary>
    ///     This class allows you to plug in new ways to define the camera setup when
    ///     rendering and projecting shadow textures.
    /// </summary>
    /// <remarks>
    ///     The default projection used when rendering shadow textures is a uniform
    ///     frustum. This is pretty straight forward but doesn't make the best use of 
    ///     the space in the shadow map since texels closer to the camera will be larger, 
    ///     resulting in 'jaggies'. There are several ways to distribute the texels
    ///     in the shadow texture differently, and this class allows you to override
    ///     that. 
    ///     
    ///     Ogre is provided with several alternative shadow camera setups, including
    ///     LiSPSM (LiSPSMShadowCameraSetup) and Plane Optimal (PlaneOptimalShadowCameraSetup).
    ///     Others can of course be written to incorporate other algorithms. All you 
    ///     have to do is instantiate one of these classes and enable it using 
    ///     SceneManager::setShadowCameraSetup (global) or Light::setCustomShadowCameraSetup
    ///     (per light). In both cases the instance is wrapped in a SharedPtr which means
    ///     it will  be deleted automatically when no more references to it exist.
    ///     
    ///     Shadow map matrices, being projective matrices, have 15 degrees of freedom.
    ///     3 of these degrees of freedom are fixed by the light's position.  4 are used to
    ///     affinely affect z values.  6 affinely affect u,v sampling.  2 are projective
    ///     degrees of freedom.  This class is meant to allow custom methods for 
    ///     handling optimization.
    /// </remarks>
	public abstract class ShadowCameraSetup {
        
        /// <summary>
        ///     Function to implement -- must set the shadow camera properties
        /// </summary>
        public abstract void GetShadowCamera(SceneManager sm, Camera cam, Viewport vp,  Light light, Camera texCam);
    }
    
    /// <summary>
    ///     Implements default shadow camera setup
    /// </summary>
    /// <remarks>
    ///     This implements the default shadow camera setup algorithm.  This is what might
    ///     be referred to as "normal" shadow mapping.  
    /// </remarks>
	public class DefaultShadowCameraSetup : ShadowCameraSetup {
        
        /// <summary>
        ///     Function to implement -- must set the shadow camera properties
        /// </summary>
        public override void GetShadowCamera(SceneManager sm, Camera cam, Viewport vp,  Light light, Camera texCam) {
            Vector3 pos, dir;

            // reset custom view / projection matrix in case already set
            texCam.CustomViewMatrix = false;
            texCam.CustomProjectionMatrix = false;

            // get the shadow frustum's far distance
            float shadowDist = sm.ShadowFarDistance;
            if (shadowDist == 0f)
                // need a shadow distance, make one up
                shadowDist = cam.Near * 300f;
            float shadowOffset = shadowDist * sm.ShadowDirLightTextureOffset;

            // Directional lights 
            if (light.Type == LightType.Directional) {

                // set up the shadow texture
                // Set ortho projection
                texCam.ProjectionType = Projection.Orthographic;
                // set easy FOV and near dist so that texture covers far dist
                texCam.FOVy = 90;
                texCam.Near = shadowDist;
                // TODO: Ogre doesn't include this line
                texCam.Far = sm.ShadowDirectionalLightExtrusionDistance * 3;

                // Set size of projection

                // Calculate look at position
                // We want to look at a spot shadowOffset away from near plane
                // 0.5 is a litle too close for angles
                Vector3 target = cam.DerivedPosition + (cam.DerivedDirection * shadowOffset);

                // Calculate orientation
                dir = - light.DerivedDirection; // backwards since point down -z
                dir.Normalize();

                // Calculate position
                // We want to be in the -ve direction of the light direction
                // far enough to project for the dir light extrusion distance
                pos = target + dir * sm.ShadowDirectionalLightExtrusionDistance;

                // Round local x/y position based on a world-space texel; this helps to reduce
                // jittering caused by the projection moving with the camera
                // Viewport is 2 * near clip distance across (90 degree fov)
                float worldTexelSize = (texCam.Near * 20f) / vp.ActualWidth;
                pos.x -= pos.x % worldTexelSize;
                pos.y -= pos.y % worldTexelSize;
                pos.z -= pos.z % worldTexelSize;

                //LogManager.Instance.Write("Light Camera Pos: {0}", pos.ToString());
                //Vector3 nearPos = pos - shadowDist * dir;
                //Vector3 farPos = pos - shadowDirLightExtrudeDist * 3 * dir;
                //LogManager.Instance.Write("Light Camera Near: {0}", nearPos.ToString());
                //LogManager.Instance.Write("Light Camera Far: {0}", farPos.ToString());
                //LogManager.Instance.Write("Light Camera Target: {0}", target.ToString());
            }
            // Spotlight
            else if (light.Type == LightType.Spotlight) {

                // Set perspective projection
                texCam.ProjectionType = Projection.Perspective;
                // set FOV slightly larger than the spotlight range to ensure coverage
                texCam.FOVy = light.SpotlightOuterAngle * 1.2f;
                // set near clip the same as main camera, since they are likely
                // to both reflect the nature of the scene
                texCam.Near = cam.Near;

                pos = light.DerivedPosition;
                dir = - light.DerivedDirection; // backwards since point down -z
                dir.Normalize();
            }
            // Point light
            else {
                // Set perspective projection
                texCam.ProjectionType = Projection.Perspective;
                // Use 120 degree FOV for point light to ensure coverage more area
                texCam.FOVy = 120;
                // set near clip the same as main camera, since they are likely
                // to both reflect the nature of the scene
                texCam.Near = cam.Near;

                // Calculate look at position
                // We want to look at a spot shadowOffset away from near plane
                // 0.5 is a litle too close for angles
                Vector3 target = cam.DerivedPosition + (cam.DerivedDirection * shadowOffset);

                // Calculate position, which same as point light position
                pos = light.DerivedPosition;

                dir = (pos - target); // backwards since point down -z
                dir.Normalize();
            }

            // Finally set position
            texCam.Position = pos;

            /*
            // Next section (camera oriented shadow map) abandoned
            // Always point in the same direction, if we don't do this then
            // we get 'shadow swimming' as camera rotates
            // As it is, we get swimming on moving but this is less noticeable

            // calculate up vector, we want it aligned with cam direction
            Vector3 up = cam->getDerivedDirection();
            // Check it's not coincident with dir
            if (up.dotProduct(dir) >= 1.0f)
            {
            // Use camera up
            up = cam->getUp();
            }
            */
            Vector3 up = Vector3.UnitY;
            // Check it's not coincident with dir
            if (Math.Abs(up.Dot(dir)) >= 1.0f) {
                // Use camera up
                up = Vector3.UnitZ;
            }
            // cross twice to rederive, only direction is unaltered
            Vector3 left = dir.Cross(up);
            left.Normalize();
            up = dir.Cross(left);
            up.Normalize();
            // Derive quaternion from axes
            Quaternion q = Quaternion.Zero;
            q.FromAxes(left, up, dir);
            texCam.Orientation = q;
        }
        
    }
    
}
