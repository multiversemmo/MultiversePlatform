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

#region Namespace Declarations
using System;
using Axiom.Core;
using Axiom.ParticleSystems;
using Axiom.Scripting;

#endregion

namespace Axiom.ParticleFX 
{
	/// <summary>
	/// Summary description for ColorFaderAffector.
	/// </summary>
	public class ColorInterpolatorAffector : ParticleAffector {
		protected const int MAX_STAGES = 6;
		
		internal ColorEx[] colorAdj	= new ColorEx[MAX_STAGES];
		internal float[] timeAdj = new float[MAX_STAGES];

		public ColorInterpolatorAffector() {
			this.type = "ColourInterpolator";

			for( int i = 0; i < MAX_STAGES; ++i ) {
				colorAdj[i] = new ColorEx(0.5f, 0.5f, 0.5f, 0.0f);
				timeAdj[i] = 1.0f;
			}
		}

		public override void AffectParticles(ParticleSystem system, float timeElapsed) {
			// loop through the particles
			for(int i = 0; i < system.Particles.Count; i++) {
				Particle p = (Particle)system.Particles[i];

				float lifeTime = p.totalTimeToLive;
				float particleTime = 1.0f - (p.timeToLive / lifeTime); 

				if (particleTime <= timeAdj[0]) {
					p.Color = colorAdj[0];
				} 
				else if (particleTime >= timeAdj[MAX_STAGES - 1]) {
					p.Color = colorAdj[MAX_STAGES-1];
				} 
				else {
					for (int k = 0; k < MAX_STAGES - 1; k++) {
						if (particleTime >= timeAdj[k] && particleTime < timeAdj[k + 1]) {
							particleTime -= timeAdj[k];
							particleTime /= (timeAdj[k+1] - timeAdj[k]);
							p.Color.r = ((colorAdj[k+1].r * particleTime) + (colorAdj[k].r * (1.0f - particleTime)));
							p.Color.g = ((colorAdj[k+1].g * particleTime) + (colorAdj[k].g * (1.0f - particleTime)));
							p.Color.b = ((colorAdj[k+1].b * particleTime) + (colorAdj[k].b * (1.0f - particleTime)));
							p.Color.a = ((colorAdj[k+1].a * particleTime) + (colorAdj[k].a * (1.0f - particleTime)));

							break;
						}
					}
				}
			}
		}

		#region Command definition classes

		[Command("colour0", "Initial 'keyframe' color.", typeof(ParticleAffector))]
		class Color0Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				return StringConverter.ToString(affector.colorAdj[0]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.colorAdj[0] = StringConverter.ParseColor(val);
			}

			#endregion
		}

		[Command("colour1", "1st 'keyframe' color.", typeof(ParticleAffector))]
		class Color1Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				// TODO: Common way for writing color.
				return StringConverter.ToString(affector.colorAdj[1]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.colorAdj[1] = StringConverter.ParseColor(val);
			}

			#endregion
		}

		[Command("colour2", "2nd 'keyframe' color.", typeof(ParticleAffector))]
		class Color2Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				// TODO: Common way for writing color.
				return StringConverter.ToString(affector.colorAdj[2]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.colorAdj[2] = StringConverter.ParseColor(val);
			}

			#endregion
		}

		[Command("colour3", "3rd 'keyframe' color.", typeof(ParticleAffector))]
		class Color3Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				return StringConverter.ToString(affector.colorAdj[3]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.colorAdj[3] = StringConverter.ParseColor(val);
			}

			#endregion
		}

		[Command("colour4", "4th 'keyframe' color.", typeof(ParticleAffector))]
		class Color4Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				return StringConverter.ToString(affector.colorAdj[4]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.colorAdj[4] = StringConverter.ParseColor(val);
			}

			#endregion
		}

		[Command("colour5", "5th 'keyframe' color.", typeof(ParticleAffector))]
		class Color5Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				return StringConverter.ToString(affector.colorAdj[5]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.colorAdj[5] = StringConverter.ParseColor(val);
			}

			#endregion
		}

		[Command("time0", "Initial 'keyframe' time.", typeof(ParticleAffector))]
		class Time0Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				return StringConverter.ToString(affector.timeAdj[0]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.timeAdj[0] = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("time1", "1st 'keyframe' time.", typeof(ParticleAffector))]
		class Time1Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				return StringConverter.ToString(affector.timeAdj[1]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.timeAdj[1] = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("time2", "2nd 'keyframe' time.", typeof(ParticleAffector))]
		class Time2Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				return StringConverter.ToString(affector.timeAdj[2]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.timeAdj[2] = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("time3", "3rd 'keyframe' time.", typeof(ParticleAffector))]
		class Time3Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				return StringConverter.ToString(affector.timeAdj[3]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.timeAdj[3] = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("time4", "4th 'keyframe' time.", typeof(ParticleAffector))]
		class Time4Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				return StringConverter.ToString(affector.timeAdj[4]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.timeAdj[4] = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		[Command("time5", "5th 'keyframe' time.", typeof(ParticleAffector))]
		class Time5Command : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				return StringConverter.ToString(affector.timeAdj[5]);
			}
			public void Set(object target, string val) {
				ColorInterpolatorAffector affector = target as ColorInterpolatorAffector;

				affector.timeAdj[5] = StringConverter.ParseFloat(val);
			}

			#endregion
		}

		#endregion Command definition classes
	}
}
