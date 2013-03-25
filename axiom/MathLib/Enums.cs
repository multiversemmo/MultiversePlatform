using System;

namespace Axiom.MathLib {
	/// <summary>
	///    Type of intersection detected between 2 object.
	/// </summary>
	public enum Intersection { 
		/// <summary>
		///    The objects are not intersecting.
		/// </summary>
		None,
		/// <summary>
		///    An object is fully contained within another object.
		/// </summary>
		Contained, 
		/// <summary>
		///    An object fully contains another object.
		/// </summary>
		Contains, 
		/// <summary>
		///    The objects are partially intersecting each other.
		/// </summary>
		Partial
	} 

	/// <summary>
	/// The "positive side" of the plane is the half space to which the
	/// plane normal points. The "negative side" is the other half
	/// space. The flag "no side" indicates the plane itself.
	/// </summary>
	public enum PlaneSide {
		None,
		Positive,
		Negative
	}
}
