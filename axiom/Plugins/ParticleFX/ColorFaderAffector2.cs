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
	/// Summary description for ColorFaderAffector.
	/// </summary>
	public class ColorFaderAffector2 : ParticleAffector {
		#region Private Member Variables
		
		protected float alphaAdjust1;
		protected float redAdjust1;
		protected float greenAdjust1;
		protected float blueAdjust1;

		protected float alphaAdjust2;
		protected float redAdjust2;
		protected float greenAdjust2;
		protected float blueAdjust2;

		protected float stateChangeVal;

		#endregion

		public ColorFaderAffector2() {
			this.type = "ColourFader2";
		}

		#region Public Member Properties

		public float AlphaAdjust1 {
			get { return alphaAdjust1; }
			set { alphaAdjust1 = value; }
		}

		public float RedAdjust1 {
			get { return redAdjust1; }
			set { redAdjust1 = value; }
		}

		public float GreenAdjust1 {
			get { return greenAdjust1; }
			set { greenAdjust1 = value; }
		}

		public float BlueAdjust1 {
			get { return blueAdjust1; }
			set { blueAdjust1 = value; }
		}

		public float AlphaAdjust2 {
			get { return alphaAdjust2; }
			set { alphaAdjust2 = value; }
		}

		public float RedAdjust2 {
			get { return redAdjust2; }
			set { redAdjust2 = value; }
		}

		public float GreenAdjust2 {
			get { return greenAdjust2; }
			set { greenAdjust2 = value; }
		}

		public float BlueAdjust2 {
			get { return blueAdjust2; }
			set { blueAdjust2 = value; }
		}

		public float StateChangeVal {
			get { return stateChangeVal; }
			set { stateChangeVal = value; }
		}

		#endregion

		protected void AdjustWithClamp(ref float component, float adjust) {
			component += adjust;

			// limit to range [0,1]
			if(component < 0.0f)
				component = 0.0f;
			else if(component > 1.0f)
				component = 1.0f;
		}

		public override void AffectParticles(ParticleSystem system, float timeElapsed) {
			float da1, dr1, dg1, db1;
			float da2, dr2, dg2, db2;

			// Scale adjustments by time
			da1 = alphaAdjust1 * timeElapsed;
			dr1 = redAdjust1 * timeElapsed;
			dg1 = greenAdjust1 * timeElapsed;
			db1 = blueAdjust1 * timeElapsed;

			// Scale adjustments by time
			da2 = alphaAdjust2 * timeElapsed;
			dr2 = redAdjust2 * timeElapsed;
			dg2 = greenAdjust2 * timeElapsed;
			db2 = blueAdjust2 * timeElapsed;

			// loop through the particles

			for(int i = 0; i < system.Particles.Count; i++) {
				Particle p = (Particle)system.Particles[i];

				// adjust the values with clamping ([0,1] in this case)
				if( p.timeToLive > StateChangeVal ) {
					AdjustWithClamp(ref p.Color.r, dr1);
					AdjustWithClamp(ref p.Color.g, dg1);
					AdjustWithClamp(ref p.Color.b, db1);
					AdjustWithClamp(ref p.Color.a, da1);
				}
				else {
					AdjustWithClamp(ref p.Color.r, dr2);
					AdjustWithClamp(ref p.Color.g, dg2);
					AdjustWithClamp(ref p.Color.b, db2);
					AdjustWithClamp(ref p.Color.a, da2);
				}
			}
		}

		#region Command definition classes

		[Command("red1", "Initial red.", typeof(ParticleAffector))]
		class Red1Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				return StringConverter.ToString(affector.RedAdjust1);
			}
			public void Set(object target, string val) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				affector.RedAdjust1 = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("red2", "Final red.", typeof(ParticleAffector))]
		class Red2Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				return StringConverter.ToString(affector.RedAdjust2);
			}
			public void Set(object target, string val) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				affector.RedAdjust2 = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("green1", "Initial green.", typeof(ParticleAffector))]
		class Green1Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				return StringConverter.ToString(affector.GreenAdjust1);
			}
			public void Set(object target, string val) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				affector.GreenAdjust1 = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("green2", "Final green.", typeof(ParticleAffector))]
		class Green2Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				return StringConverter.ToString(affector.GreenAdjust2);
			}
			public void Set(object target, string val) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				affector.GreenAdjust2 = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("blue1", "Initial blue.", typeof(ParticleAffector))]
		class Blue1Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				return StringConverter.ToString(affector.BlueAdjust1);
			}
			public void Set(object target, string val) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				affector.BlueAdjust1 = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("blue2", "Final blue.", typeof(ParticleAffector))]
		class Blue2Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				return StringConverter.ToString(affector.BlueAdjust2);
			}
			public void Set(object target, string val) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				affector.BlueAdjust2 = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("alpha1", "Initial alpha.", typeof(ParticleAffector))]
		class Alpha1Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				return StringConverter.ToString(affector.AlphaAdjust1);
			}
			public void Set(object target, string val) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				affector.AlphaAdjust1 = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("alpha2", "Final alpha.", typeof(ParticleAffector))]
		class Alpha2Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				return StringConverter.ToString(affector.AlphaAdjust2);
			}
			public void Set(object target, string val) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				affector.AlphaAdjust2 = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("state_change", "Rate of state changing.", typeof(ParticleAffector))]
		class StateChangeCommand : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				return StringConverter.ToString(affector.StateChangeVal);
			}
			public void Set(object target, string val) {
				ColorFaderAffector2 affector = target as ColorFaderAffector2;
				affector.StateChangeVal = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		#endregion Command definition classes
	}
}
