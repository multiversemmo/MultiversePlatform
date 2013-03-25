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
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Animating {
	/// <summary>
    ///     Defines an object property which is animable, ie may be keyframed.
	/// </summary>
    /// <remarks>
	///     Animable properties are those which can be altered over time by a 
	///     predefined keyframe sequence. They may be set directly, or they may
	///     be modified from their existing state (common if multiple animations
	///     are expected to apply at once). Implementors of this interface are
	///     expected to override the 'setValue', 'setCurrentStateAsBaseValue' and 
	///     'ApplyDeltaValue' methods appropriate to the type in question, and to 
	///     initialise the type.
	///     
	///     AnimableValue instances are accessible through any class which extends
	///     AnimableObject in order to expose it's animable properties.
	///     
	///     This class is an instance of the Adapter pattern, since it generalises
	///     access to a particular property. Whilst it could have been templated
	///     such that the type which was being referenced was compiled in, this would
	///     make it more difficult to aggregated generically, and since animations
	///     are often comprised of multiple properties it helps to be able to deal
	///     with all values through a single class.
	///</remarks>

	public abstract class AnimableValue {
		protected AnimableType type;
		protected Object valueObject;

		public AnimableValue(AnimableType type) {
			this.type = type;

			valueObject = null;
		}
		
		public AnimableType Type {
			get { return type; }
			set { type = value; }
		}
				
		public Object ValueObject {
			get { return ValueObject; }
			set { ValueObject = value; }
		}
				
		/// Internal method to set a value as base
		public virtual void SetAsBaseValue(int val) { valueObject = val; }
		/// Internal method to set a value as base
		public virtual void SetAsBaseValue(float val) { valueObject = val; }
		/// Internal method to set a value as base
		public virtual void SetAsBaseValue(Vector2 val) { valueObject = val; }
		/// Internal method to set a value as base
		public virtual void SetAsBaseValue(Vector3 val) { valueObject = val; }
		/// Internal method to set a value as base
		public virtual void SetAsBaseValue(Vector4 val) { valueObject = val; }
		/// Internal method to set a value as base
		public virtual void SetAsBaseValue(Quaternion val) { valueObject = val; }
		/// Internal method to set a value as base
		public virtual void SetAsBaseValue(ColorEx val) { valueObject = new ColorEx((ColorEx)val); }
		
		void SetAsBaseValue(Object val) {
			switch(type) {
			case AnimableType.Int:
				SetAsBaseValue((int)val);
				break;
			case AnimableType.Float:
				SetAsBaseValue((float)val);
				break;
			case AnimableType.Vector2:
				SetAsBaseValue((Vector2)val);
				break;
			case AnimableType.Vector3:
				SetAsBaseValue((Vector3)val);
				break;
			case AnimableType.Vector4:
				SetAsBaseValue((Vector4)val);
				break;
			case AnimableType.Quaternion:
				SetAsBaseValue((Quaternion)val);
				break;
			case AnimableType.ColorEx:
				SetAsBaseValue((ColorEx)val);
				break;
			}
		}

		public void ResetToBaseValue() {
			switch(type) {
			case AnimableType.Int:
				SetValue((int)valueObject);
				break;
			case AnimableType.Float:
				SetValue((float)valueObject);
				break;
			case AnimableType.Vector2:
				SetValue((Vector2)valueObject);
				break;
			case AnimableType.Vector3:
				SetValue((Vector3)valueObject);
				break;
			case AnimableType.Vector4:
				SetValue((Vector4)valueObject);
				break;
			case AnimableType.Quaternion:
				SetValue((Quaternion)valueObject);
				break;
			case AnimableType.ColorEx:
				SetValue((ColorEx)valueObject);
				break;
			}
		}

		/// Set value 
		public virtual void SetValue(int val) {
			throw (new Exception("Animable SetValue to int not implemented"));
		}
		/// Set value 
		public virtual void SetValue(float val) {
			throw (new Exception("Animable SetValue to float not implemented"));
		}
		/// Set value 
		public virtual void SetValue(Vector2 val) {
			throw (new Exception("Animable SetValue to Vector2 not implemented"));
		}
		/// Set value 
		public virtual void SetValue(Vector3 val) {
			throw (new Exception("Animable SetValue to Vector3 not implemented"));
		}
		/// Set value 
		public virtual void SetValue(Vector4 val) {
			throw (new Exception("Animable SetValue to Vector4 not implemented"));
		}
		/// Set value 
		public virtual void SetValue(Quaternion val) {
			throw (new Exception("Animable SetValue to Quaternion not implemented"));
		}
		/// Set value 
		public virtual void SetValue(ColorEx val) {
			throw (new Exception("Animable SetValue to ColorEx not implemented"));
		}
		/// Set value 
		public virtual void SetValue(Object val) {
			switch(type) {
			case AnimableType.Int:
				SetValue((int)val);
				break;
			case AnimableType.Float:
				SetValue((float)val);
				break;
			case AnimableType.Vector2:
				SetValue((Vector2)val);
				break;
			case AnimableType.Vector3:
				SetValue((Vector3)val);
				break;
			case AnimableType.Vector4:
				SetValue((Vector4)val);
				break;
			case AnimableType.Quaternion:
				SetValue((Quaternion)val);
				break;
			case AnimableType.ColorEx:
				SetValue((ColorEx)val);
				break;
			}
		}

 		/// Apply the specified delta 
		public virtual void ApplyDeltaValue(int val) {
			throw (new Exception("Animable ApplyDeltaValue to int not implemented"));
		}
		/// Apply the specified delta 
		public virtual void ApplyDeltaValue(float val) {
			throw (new Exception("Animable ApplyDeltaValue to float not implemented"));
		}
		/// Apply the specified delta 
		public virtual void ApplyDeltaValue(Vector2 val) {
			throw (new Exception("Animable ApplyDeltaValue to Vector2 not implemented"));
		}
		/// Apply the specified delta 
		public virtual void ApplyDeltaValue(Vector3 val) {
			throw (new Exception("Animable ApplyDeltaValue to Vector3 not implemented"));
		}
		/// Apply the specified delta 
		public virtual void ApplyDeltaValue(Vector4 val) {
			throw (new Exception("Animable ApplyDeltaValue to Vector4 not implemented"));
		}
		/// Apply the specified delta 
		public virtual void ApplyDeltaValue(Quaternion val) {
			throw (new Exception("Animable ApplyDeltaValue to Quaternion not implemented"));
		}
		/// Apply the specified delta 
		public virtual void ApplyDeltaValue(ColorEx val) {
			throw (new Exception("Animable ApplyDeltaValue to ColorEx not implemented"));
		}

		/// Apply the specified delta 
		public virtual void ApplyDeltaValue(Object val) {
			switch(type) {
			case AnimableType.Int:
				ApplyDeltaValue((int)val);
				break;
			case AnimableType.Float:
				ApplyDeltaValue((float)val);
				break;
			case AnimableType.Vector2:
				ApplyDeltaValue((Vector2)val);
				break;
			case AnimableType.Vector3:
				ApplyDeltaValue((Vector3)val);
				break;
			case AnimableType.Vector4:
				ApplyDeltaValue((Vector4)val);
				break;
			case AnimableType.Quaternion:
				ApplyDeltaValue((Quaternion)val);
				break;
			case AnimableType.ColorEx:
				ApplyDeltaValue((ColorEx)val);
				break;
			}
		}
		
		public static Object InterpolateValues(float time, AnimableType type, Object k1, Object k2) {
			switch(type) {
			case AnimableType.Int:
				int i1 = (int)k1;
				int i2 = (int)k2;
				return (Object)(int)(i1 + (i2 - i1) * time);
			case AnimableType.Float:
				float f1 = (float)k1;
				float f2 = (float)k2;
				return (Object)(f1 + (f2 - f1) * time);
			case AnimableType.Vector2:
				Vector2 v21 = (Vector2)k1;
				Vector2 v22 = (Vector2)k2;
				return (Object)(v21 + (v22 - v21) * time);
			case AnimableType.Vector3:
				Vector3 v31 = (Vector3)k1;
				Vector3 v32 = (Vector3)k2;
				return (Object)(v31 + (v32 - v31) * time);
			case AnimableType.Vector4:
				Vector4 v41 = (Vector4)k1;
				Vector4 v42 = (Vector4)k2;
				return (Object)(v41 + (v42 - v41) * time);
			case AnimableType.Quaternion:
				Quaternion q1 = (Quaternion)k1;
				Quaternion q2 = (Quaternion)k2;
                return Quaternion.Slerp(time, q1, q2);
				// return (Object)(q1 + (q2 + (-1 * q1)) * time);
			case AnimableType.ColorEx:
				ColorEx c1 = (ColorEx)k1;
				ColorEx c2 = (ColorEx)k2;
				return (Object)(new ColorEx(c1.a + (c2.a - c1.a) * time,
											c1.r + (c2.r - c1.r) * time,
											c1.g + (c2.g - c1.g) * time,
											c1.b + (c2.b - c1.b) * time));
			}
			throw new Exception(string.Format("In AnimableValue.InterpolateValues, unknown type {0}", type));
		}

		public static Object MultiplyFloat(AnimableType type, float v, Object k) {
			switch(type) {
			case AnimableType.Int:
				return (Object)(int)(((int)k) * v);
			case AnimableType.Float:
				float f = (float)k;
				return (Object)(f * v);
			case AnimableType.Vector2:
				Vector2 v2 = (Vector2)k;
				return (Object)(v2 * v);
			case AnimableType.Vector3:
				Vector3 v3 = (Vector3)k;
				return (Object)(v3 * v);
			case AnimableType.Vector4:
				Vector4 v4 = (Vector4)k;
				return (Object)(v4 * v);
			case AnimableType.Quaternion:
				Quaternion q = (Quaternion)k;
				return (Object)(q * v);
			case AnimableType.ColorEx:
				ColorEx c = (ColorEx)k;
				return (Object)(new ColorEx(c.a * v, c.r * v, c.g * v, c.b * v));
			}
			throw new Exception(string.Format("In AnimableValue.MultiplyFloat, unknown type {0}", type));
		}

		/// Sets the current state as the 'base' value; used for delta animation
		/// Any instantiated derived class must implement this guy
		public abstract void SetCurrentStateAsBaseValue();

	}

	public interface IAnimableObject 
	{
		#region Methods
		
		/// <summary>
		///		Create an AnimableValue for the attribute with the given name, or 
		///     throws an exception if this object doesn't support creating them.
		/// </summary>
		AnimableValue CreateAnimableValue(string valueName);

		#endregion Methods
	
		#region Properties

		/// <summary>
		///		Return the names of all the AnimableValue names supported by this object.
		///     This can return the null list if there are none.
		/// </summary>
		string[] AnimableValueNames { get; }
        
		#endregion Properties
	}

}



