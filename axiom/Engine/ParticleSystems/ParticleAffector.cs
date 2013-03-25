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
using System.Reflection;
using Axiom.Scripting;

namespace Axiom.ParticleSystems {
    /// <summary>
    ///		Abstract class defining the interface to be implemented by particle affectors.
    /// </summary>
    /// <remarks>
    ///		Particle affectors modify particles in a particle system over their lifetime. They can be
    ///		grouped into types, e.g. 'vector force' affectors, 'fader' affectors etc; each type will 
    ///		modify particles in a different way, using different parameters.
    ///		<p/>
    ///		Because there are so many types of affectors you could use, the engine chooses not to dictate
    ///		the available types. It comes with some in-built, but allows plugins or applications to extend the affector types available.
    ///		This is done by subclassing ParticleAffector to have the appropriate emission behavior you want,
    ///		and also creating a subclass of ParticleAffectorFactory which is responsible for creating instances 
    ///		of your new affector type. You register this factory with the ParticleSystemManager using
    ///		AddAffectorFactory, and from then on affectors of this type can be created either from code or through
    ///		.particle scripts by naming the type.
    ///		<p/>
    ///		This same approach is used for ParticleEmitters (which are the source of particles in a system).
    ///		This means that the engine is particularly flexible when it comes to creating particle system effects,
    ///		with literally infinite combinations of affector and affector types, and parameters within those
    ///		types.
    /// </summary>
    public abstract class ParticleAffector {
        #region Fields

        /// <summary>Name of the affector type.  Must be initialized by subclasses.</summary>
        protected string type;
        
		protected Hashtable commandTable = new Hashtable();

        #endregion Fields

        #region Constructors

        /// <summary>
        ///		Default constructor
        /// </summary>
        public ParticleAffector() {
			RegisterCommands();
        }

        #endregion

        #region Properties

        /// <summary>
        ///		Gets the type name of this affector.
        /// </summary>
        public string Type {
            get { return type; }
            set { type = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///		Method called to allow the affector to 'do it's stuff' on all active particles in the system.
        /// </summary>
        /// <remarks>
        ///		This is where the affector gets the chance to apply it's effects to the particles of a system.
        ///		The affector is expected to apply it's effect to some or all of the particles in the system
        ///		passed to it, depending on the affector's approach.
        /// </remarks>
        /// <param name="system">Reference to a ParticleSystem to affect.</param>
        /// <param name="timeElapsed">The number of seconds which have elapsed since the last call.</param>
        public abstract void AffectParticles(ParticleSystem system, float timeElapsed);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="emitter"></param>
		public virtual void CopyTo(ParticleAffector affector) {
			// loop through all registered commands and copy from this instance to the target instance
			foreach(DictionaryEntry entry in commandTable) {
				string name = (string)entry.Key;

				// get the value of the param from this instance
				string val = ((ICommand)entry.Value).Get(this);

				// set the param on the target instance
				affector.SetParam(name, val);
			}
		}

		/// <summary>
		///		Method called to allow the affector to 'do it's stuff' on all active particles in the system.
		/// </summary>
		/// <remarks>
		///		This is where the affector gets the chance to apply it's effects to the particles of a system.
		///		The affector is expected to apply it's effect to some or all of the particles in the system
		///		passed to it, depending on the affector's approach.
		/// </remarks>
		/// <param name="system">Reference to a ParticleSystem to affect.</param>
		/// <param name="timeElapsed">The number of seconds which have elapsed since the last call.</param>
		public virtual void InitParticle(ref Particle particle ) {
			// do nothing by default
		}
		
		#endregion
		
		#region Script parser methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public bool SetParam(string name, string val) {
			if(commandTable.ContainsKey(name)) {
				ICommand command = (ICommand)commandTable[name];

				command.Set(this, val);
 
				return true;
			}

			return false;
		}

		/// <summary>
		///		Registers all attribute names with their respective parser.
		/// </summary>
		/// <remarks>
		///		Methods meant to serve as attribute parsers should use a method attribute to 
		/// </remarks>
		protected void RegisterCommands() {
			Type baseType = GetType();

			do {
				Type[] types = baseType.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
			
				// loop through all methods and look for ones marked with attributes
				for(int i = 0; i < types.Length; i++) {
					// get the current method in the loop
					Type type = types[i];
				
					// get as many command attributes as there are on this type
					CommandAttribute[] commandAtts = 
						(CommandAttribute[])type.GetCustomAttributes(typeof(CommandAttribute), true);

					// loop through each one we found and register its command
					for(int j = 0; j < commandAtts.Length; j++) {
						CommandAttribute commandAtt = commandAtts[j];

						commandTable.Add(commandAtt.Name, Activator.CreateInstance(type));
					} // for
				} // for

				// get the base type of the current type
				baseType = baseType.BaseType;

			} while(baseType != typeof(object));
		}

		#endregion Script parser methods
    }
}
