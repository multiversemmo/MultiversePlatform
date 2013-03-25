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
using Axiom.ParticleSystems;
using Axiom.Scripting;
using Axiom.Media;

namespace Axiom.ParticleFX {
	/// <summary>
	/// Summary description for ColorFaderAffector.
	/// </summary>
	public class ColorImageAffector : ParticleAffector {
		protected Axiom.Media.Image colorImage;
		protected String colorImageName;

		public ColorImageAffector() {
			this.type = "ColourImage";
		}

		public String ColorImageName {
			get { 
				return colorImageName; 
			}
			set { 
				colorImageName = value;
				colorImage = Axiom.Media.Image.FromFile(value);

				PixelFormat	format = colorImage.Format;

				if ( format != PixelFormat.A8R8G8B8 ) {
					throw new NotSupportedException( "Error: Image is not a rgba image.  ColorImageAffector.ColorImageName property set" );
				}
			}
		}

		public override void InitParticle(ref Particle particle) {
			const float div_255 = 1.0f / 255.0f;
		
			particle.Color.r = colorImage.Data[0] * div_255;
			particle.Color.g = colorImage.Data[1] * div_255;
			particle.Color.b = colorImage.Data[2] * div_255;
			particle.Color.a = colorImage.Data[3] * div_255;
		}

		public override void AffectParticles(ParticleSystem system, float timeElapsed) {
			float width = colorImage.Width  - 1;
			float height = colorImage.Height - 1;
			const float div_255 = 1.0f / 255.0f;

			// loop through the particles
			for( int i = 0; i < system.Particles.Count; i++) {
				Particle p = (Particle)system.Particles[i];

				// life_time, float_index, index and position are CONST in OGRE, but errors here

				// We do not have the concept of a total time to live!
				float life_time = p.totalTimeToLive;
				float particle_time	= 1.0f - (p.timeToLive / life_time); 

				if (particle_time > 1.0f) {
					particle_time = 1.0f;
				}
				if (particle_time < 0.0f) {
					particle_time = 0.0f;
				}

				float float_index = particle_time * width;
				int index = (int)float_index;
				int position = index * 4;
				
				if (index <= 0 || index >= width) {
					p.Color.r = (colorImage.Data[position + 0] * div_255);
					p.Color.g = (colorImage.Data[position + 1] * div_255);
					p.Color.b = (colorImage.Data[position + 2] * div_255);
					p.Color.a = (colorImage.Data[position + 3] * div_255);
				} 
				else {
					// fract, to_color and from_color are CONST in OGRE, but errors here
					float fract = float_index - (float)index;
					float toColor = fract * div_255;
					float fromColor = (div_255 - toColor);

					p.Color.r = (colorImage.Data[position + 0] * fromColor) + (colorImage.Data[position + 4] * toColor);
					p.Color.g = (colorImage.Data[position + 1] * fromColor) + (colorImage.Data[position + 5] * toColor);
					p.Color.b = (colorImage.Data[position + 2] * fromColor) + (colorImage.Data[position + 6] * toColor);
					p.Color.a = (colorImage.Data[position + 3] * fromColor) + (colorImage.Data[position + 7] * toColor);
				}
			}
		}

		#region Command definition classes

		[Command("image", "Image for color alterations.", typeof(ParticleAffector))]
		class ImageCommand : ICommand {
			#region ICommand Members

			public string Get(object target) {
				ColorImageAffector affector = target as ColorImageAffector;
				return affector.ColorImageName;
			}
			public void Set(object target, string val) {
				ColorImageAffector affector = target as ColorImageAffector;
				affector.ColorImageName = val;
			}

			#endregion
		}

		#endregion Command definition classes
	}
}
