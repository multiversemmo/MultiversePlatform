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
using Axiom.Core;
using Axiom.ParticleSystems;
using Axiom.Scripting;

namespace Axiom.ParticleFX {
	/// <summary>
	/// Summary description for ScaleAffector.
	/// </summary>
	public class ScaleAffector : ParticleAffector {
		protected float scaleAdjust;

		public ScaleAffector() {
			this.type	= "Scaler";
			scaleAdjust	= 0;
		}

		public float ScaleAdjust {
			get { 
				return scaleAdjust; 
			}
			set { 
				scaleAdjust = value; 
			}
		}

		public override void AffectParticles(ParticleSystem system, float timeElapsed) {
			float ds;

			// Scale adjustments by time
			ds = scaleAdjust * timeElapsed;

			float newWide, newHigh;

			// loop through the particles

			for(int i = 0; i < system.Particles.Count; i++) {
				Particle p = (Particle)system.Particles[i];

				if( p.HasOwnDimensions == false ) {
					p.Height = system.DefaultHeight;
					p.Width = system.DefaultWidth;
				}
				else {
					newWide	= p.Width + ds;
					newHigh	= p.Height + ds;
					p.Width	= newWide;
					p.Height= newHigh;
				}
			}
		}

		#region Command definition classes

		[Command("rate", "Rate of particle scaling.", typeof(ParticleAffector))]
		class RateCommand : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ScaleAffector affector = target as ScaleAffector;
				return StringConverter.ToString(affector.ScaleAdjust);
			}
			public void Set(object target, string val) {
				ScaleAffector affector = target as ScaleAffector;
				affector.ScaleAdjust = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		#endregion Command definition classes
	}
}
