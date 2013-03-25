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
using Axiom.Collections;

namespace Axiom.ParticleSystems {
    /// <summary>
    ///		Abstract class defining the interface to be implemented by creators of ParticleEmitter subclasses.
    ///	 </summary>
    ///	 <remarks>
    ///		Plugins or 3rd party applications can add new types of particle emitters by creating
    ///		subclasses of the ParticleEmitter class. Because multiple instances of these emitters may be
    ///		required, a factory class to manage the instances is also required. 
    ///		<p/>
    ///		ParticleEmitterFactory subclasses must allow the creation and destruction of ParticleEmitter
    ///		subclasses. They must also be registered with the ParticleSystemManager. All factories have
    ///		a name which identifies them, examples might be 'Point', 'Cone', or 'Box', and these can be 
    ///		also be used from XML particle system scripts.
    /// </summary>
    public abstract class ParticleEmitterFactory {
        #region Member variables

        protected EmitterList emitterList = new EmitterList();
			
        #endregion

        #region Constructors

        /// <summary>
        ///		Default constructor
        /// </summary>
        public ParticleEmitterFactory() { }

        #endregion

        #region Properties

        /// <summary>
        ///		Returns the name of the factory, which identifies which type of emitter this factory creates.
        /// </summary>
        public abstract string Name { get; }

        #endregion

        #region Methods

        /// <summary>
        ///		Creates a new instance of an emitter.
        /// </summary>
        /// <remarks>
        ///		Subclasses must add newly created emitters to the emitterList.
        /// </remarks>
        /// <returns></returns>
        public abstract ParticleEmitter Create();

        /// <summary>
        ///		Destroys the emitter referenced by the parameter.
        /// </summary>
        /// <param name="emitter"></param>
        public virtual void Destroy(ParticleEmitter emitter) {
            // remove the emitter from the list
            emitterList.Remove(emitter);
        }

        #endregion
    }
}
