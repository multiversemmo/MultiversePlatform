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

namespace Axiom.ParticleSystems {
    /// <summary>
    ///		Abstract class defining the interface to be implemented by creators of ParticleAffector subclasses.
    /// </summary>
    /// <remarks>
    ///		Plugins or 3rd party applications can add new types of particle affectors  by creating
    ///		subclasses of the ParticleAffector class. Because multiple instances of these affectors may be
    ///		required, a factory class to manage the instances is also required. 
    ///		<p/>
    ///		ParticleAffectorFactory subclasses must allow the creation and destruction of ParticleAffector
    ///		subclasses. They must also be registered with the ParticleSystemManager. All factories have
    ///		a name which identifies them, examples might be 'ForceVector', 'Attractor', or 'Fader', and these can be 
    ///		also be used from particle system scripts.
    /// </remarks>
    public abstract class ParticleAffectorFactory {
        #region Member variables

        protected AffectorList affectorList = new AffectorList();

        #endregion

        #region Constructors

        /// <summary>
        ///		Default constructor.
        /// </summary>
        public ParticleAffectorFactory() { }

        #endregion

        #region Properties

        /// <summary>
        ///		Returns the name of the factory, which identifies the affector type this factory creates.
        /// </summary>
        public abstract string Name { 	get; 	}

        /// <summary>
        ///		Creates a new affector instance.
        /// </summary>
        /// <remarks>
        ///		Subclasses MUST add a reference to the affectorList.
        /// </remarks>
        /// <returns></returns>
        public abstract ParticleAffector Create();

        /// <summary>
        ///		Destroys the affector referenced by the parameter.
        /// </summary>
        /// <param name="e">The Affector to destroy.</param>
        public virtual void Destroy(ParticleAffector e) {
            // remove the affector from the list
            affectorList.Remove(e);
        }

        #endregion
    }
}
