using System;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Graphics {
	
	public interface IDerivedPlaneProvider {

		/// <summary>
		///		Get the derived plane as transformed by its parent node.
		/// </summary>
		Plane DerivedPlane { get; }
	}

	/// <summary>
	///		Definition of a Plane that may be attached to a node, and the derived
	///		details of it retrieved simply.
	/// </summary>
	/// <remarks>
	///		This plane is not here for rendering purposes, it's to allow you to attach
	///		planes to the scene in order to have them move and follow nodes on their
	///		own, which is useful if you're using the plane for some kind of calculation,
	///		e.g. reflection.
	/// </remarks>
	public class MovablePlane : MovableObject, IDerivedPlaneProvider {
		#region Fields

		/// <summary>
		///		Plane as transformed by it's parent node.
		/// </summary>
		protected Plane derivedPlane = new Plane();
		/// <summary>
		///		Cached translation vector.
		/// </summary>
		protected Vector3 lastTranslate;
		/// <summary>
		///		Cached rotation.
		/// </summary>
		protected Quaternion lastRotate;
		/// <summary>
		///		Bounding box.
		/// </summary>
		protected AxisAlignedBox nullBB = AxisAlignedBox.Null;
		/// <summary>
		///		Flag for whether changes have been made to this planes position/rotation.
		/// </summary>
		protected bool isDirty;

		/// <summary>
		///		Underlying plane representation.
		/// </summary>
		/// <remarks>
		///		Ogre uses multiple inheritance for this purpose - bah! ;)
		/// </remarks>
		protected Plane containedPlane = new Plane();

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="name">Name of this plane.</param>
		public MovablePlane(string name) {
			this.name = name;
			lastTranslate = Vector3.Zero;
			lastRotate = Quaternion.Identity;
			isDirty = true;
		}

		#endregion Constructor

		#region Properties

		
		public Plane Plane { get { return containedPlane; } }

		/// <summary>
		///		The plane's distance from the origin.
		/// </summary>
		public float D {
			get {
				return containedPlane.D;
			}
			set {
				containedPlane.D = value;
			}
		}

		/// <summary>
		///		The direction the plane is facing.
		/// </summary>
		public Vector3 Normal {
			get {
				return containedPlane.Normal;
			}
			set {
				containedPlane.Normal = value;
			}
		}

		/// <summary>
		///		Get the derived plane as transformed by its parent node.
		/// </summary>
		public Plane DerivedPlane {
			get {
				if(parentNode != null) {
					if(isDirty || 
						!(parentNode.DerivedOrientation == lastRotate && 
						parentNode.DerivedPosition == lastTranslate)) {

						// store off parent position/orientation
						lastRotate = parentNode.DerivedOrientation;
						lastTranslate = parentNode.DerivedPosition;

						// rotate normal
						derivedPlane.Normal = lastRotate * containedPlane.Normal;
						
						// d remains the same in rotation, since rotation happens first
						derivedPlane.D = containedPlane.D;

						// add on the effect of the translation (project onto new normal)
						derivedPlane.D -= derivedPlane.Normal.Dot(lastTranslate);

						isDirty = false;
					}
				}
				else {
					return containedPlane;
				}

				return derivedPlane;
			}
		}

		#endregion Properties

		#region SceneObject Members

		public override Axiom.MathLib.AxisAlignedBox BoundingBox {
			get {
				return nullBB;
			}
		}

		public override float BoundingRadius {
			get {
				return float.PositiveInfinity;
			}
		}

		public override void NotifyCurrentCamera(Camera camera) {
			// dont care
		}

		public override void UpdateRenderQueue(RenderQueue queue) {
			// do nothing
		}

		#endregion SceneObject Members
	}
}
