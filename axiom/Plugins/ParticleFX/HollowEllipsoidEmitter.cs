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
using Axiom.MathLib;
using Axiom.Scripting;
using System.Diagnostics;

namespace Axiom.ParticleFX {
	/// <summary>
	/// Summary description for HollowEllipsidEmitter.
	/// </summary>
	public class HollowEllipsoidEmitter : AreaEmitter {
		#region Fields

		protected float innerX;
		protected float innerY;
		protected float innerZ;

		#endregion Fields

		#region Properties

		public float InnerX { 
			get { 
				return innerX;	
			}
			set { 
				Debug.Assert( value > 0.0f && value < 1.0f ); 
				innerX = value;	
			}
		}

		public float InnerY { 
			get { 
				return innerY;	
			}
			set { 
				Debug.Assert( value > 0.0f && value < 1.0f ); 
				innerY = value;	
			}
		}

		public float InnerZ { 
			get { 
				return innerZ;	
			}
			set { 
				Debug.Assert( value > 0.0f && value < 1.0f ); 
				innerZ = value;	
			}
		}

		#endregion Properties

		public HollowEllipsoidEmitter() : base() {
			InitDefaults("HollowEllipsoid");
			innerX = 0.5f;
			innerY = 0.5f;
			innerZ = 0.5f;
		}

		public override void InitParticle(Particle particle) {
			float alpha, beta, a, b, c, x, y, z;

			particle.ResetDimensions();

			// create two random angles alpha and beta
			// with these two angles, we are able to select any point on an
			// ellipsoid's surface
			MathUtil.RangeRandom(durationMin, durationMax);

			alpha = MathUtil.RangeRandom(0,MathUtil.TWO_PI);
			beta  = MathUtil.RangeRandom(0,MathUtil.PI);

			// create three random radius values that are bigger than the inner
			// size, but smaller/equal than/to the outer size 1.0 (inner size is
			// between 0 and 1)
			a = MathUtil.RangeRandom(InnerX,1.0f);
			b = MathUtil.RangeRandom(InnerY,1.0f);
			c = MathUtil.RangeRandom(InnerZ,1.0f);

			// with a,b,c we have defined a random ellipsoid between the inner
			// ellipsoid and the outer sphere (radius 1.0)
			// with alpha and beta we select on point on this random ellipsoid
			// and calculate the 3D coordinates of this point
			x = a * MathUtil.Cos(alpha) * MathUtil.Sin(beta);
			y = b * MathUtil.Sin(alpha) * MathUtil.Sin(beta);
			z = c * MathUtil.Cos(beta);

			// scale the found point to the ellipsoid's size and move it
			// relatively to the center of the emitter point

			particle.Position = position + x * xRange + y * yRange * z * zRange;

			// Generate complex data by reference
			GenerateEmissionColor(particle.Color);
			GenerateEmissionDirection(ref particle.Direction);
			GenerateEmissionVelocity(ref particle.Direction);

			// Generate simpler data
			particle.timeToLive = GenerateEmissionTTL();
		}

		#region Command definition classes

		/// <summary>
		///    
		/// </summary>
		[Command("width", "Width of the hollow ellipsoidal emitter.", typeof(ParticleEmitter))]
			class WidthCommand: ICommand {
			public void Set(object target, string val) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				emitter.Width = StringConverter.ParseFloat(val);
			}
			public string Get(object target) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				return StringConverter.ToString(emitter.Width);
			}
		}

		/// <summary>
		///    
		/// </summary>
		[Command("height", "Height of the hollow ellipsoidal emitter.", typeof(ParticleEmitter))]
			class HeightCommand: ICommand {
			public void Set(object target, string val) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				emitter.Height = StringConverter.ParseFloat(val);
			}
			public string Get(object target) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				return StringConverter.ToString(emitter.Height);
			}
		}

		/// <summary>
		///    
		/// </summary>
		[Command("depth", "Depth of the hollow ellipsoidal emitter.", typeof(ParticleEmitter))]
			class DepthCommand: ICommand {
			public void Set(object target, string val) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				emitter.Depth = StringConverter.ParseFloat(val);
			}
			public string Get(object target) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				return StringConverter.ToString(emitter.Depth);
			}
		}

		/// <summary>
		///    
		/// </summary>
		[Command("inner_width", "Parametric value describing the proportion of the shape which is hollow.", typeof(ParticleEmitter))]
			class InnerWidthCommand: ICommand {
			public void Set(object target, string val) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				emitter.InnerX = StringConverter.ParseFloat(val);
			}
			public string Get(object target) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				return StringConverter.ToString(emitter.InnerX);
			}
		}

		/// <summary>
		///    
		/// </summary>
		[Command("inner_height", "Parametric value describing the proportion of the shape which is hollow.", typeof(ParticleEmitter))]
			class InnerHeightCommand: ICommand {
			public void Set(object target, string val) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				emitter.InnerY = StringConverter.ParseFloat(val);
			}
			public string Get(object target) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				return StringConverter.ToString(emitter.InnerY);
			}
		}

		/// <summary>
		///    
		/// </summary>
		[Command("inner_depth", "Parametric value describing the proportion of the shape which is hollow.", typeof(ParticleEmitter))]
			class InnerDepthCommand: ICommand {
			public void Set(object target, string val) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				emitter.InnerZ = StringConverter.ParseFloat(val);
			}
			public string Get(object target) {
				HollowEllipsoidEmitter emitter = target as HollowEllipsoidEmitter;
				return StringConverter.ToString(emitter.InnerZ);
			}
		}
		#endregion Command definition classes
	}
}
