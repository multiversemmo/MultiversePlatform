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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.ParticleSystems {
    /// <summary>
    ///     Particle system renderer attribute method definition.
    /// </summary>
    /// <param name="values">Attribute values.</param>
    /// <param name="renderer">Target particle system renderer.</param>
    delegate void ParticleSystemRendererAttributeParser(string[] values, ParticleSystemRenderer renderer);

    public abstract class ParticleSystemRenderer {
        /// Constructor
        public ParticleSystemRenderer() {
        }

        /** Gets the type of this renderer - must be implemented by subclasses */
        public abstract string Type {
            get;
        }

        /** Delegated to by ParticleSystem::_updateRenderQueue
        @remarks
            The subclass must update the render queue using whichever Renderable
            instance(s) it wishes.
        */
        public abstract void UpdateRenderQueue(RenderQueue queue, List<Particle> currentParticles, bool cullIndividually);

        /** Sets the material this renderer must use; called by ParticleSystem. */
        public abstract void SetMaterial(Material mat);
        /** Delegated to by ParticleSystem::_notifyCurrentCamera */
        public abstract void NotifyCurrentCamera(Camera cam);
        /** Delegated to by ParticleSystem::_notifyAttached */
        public abstract void NotifyAttached(Node parent, bool isTagPoint);
        /** Optional callback notified when particles are rotated */
        public virtual void NotifyParticleRotated() {
        }
        /** Optional callback notified when particles are resized individually */
        public virtual void NotifyParticleResized() {
        }
        /** Tells the renderer that the particle quota has changed */
        public abstract void NotifyParticleQuota(int quota);
        /** Tells the renderer that the particle default size has changed */
        public abstract void NotifyDefaultDimensions(float width, float height);
        /** Create a new ParticleVisualData instance for attachment to a particle.
        @remarks
            If this renderer needs additional data in each particle, then this should
            be held in an instance of a subclass of ParticleVisualData, and this method
            should be overridden to return a new instance of it. The default
            behaviour is to return null.
        */
        public virtual ParticleVisualData CreateVisualData() {
            return null;
        }
        /** Destroy a ParticleVisualData instance.
        @remarks
            If this renderer needs additional data in each particle, then this should
            be held in an instance of a subclass of ParticleVisualData, and this method
            should be overridden to destroy an instance of it. The default
            behaviour is to do nothing.
        */
        public virtual void DestroyVisualData(ParticleVisualData vis) {
            Debug.Assert(vis == null);
        }

        /** Sets which render queue group this renderer should target with it's
            output.
        */
        public abstract void SetRenderQueueGroup(RenderQueueGroupID queueID);

        /** Setting carried over from ParticleSystem.
        */
        public abstract void SetKeepParticlesInLocalSpace(bool keepLocal);

        /** Gets the desired particles sort mode of this renderer */
        //virtual SortMode _getSortMode(void) const = 0;

        public abstract void CopyParametersTo(ParticleSystemRenderer other);

        public abstract bool SetParameter(string attr, string val);
    }

    public abstract class ParticleSystemRendererFactory : FactoryObj<ParticleSystemRenderer> {
    }
}
