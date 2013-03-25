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
using System.Drawing;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Core {
    /// <summary>
    ///		A billboard is a primitive which always faces the camera in every frame.
    /// </summary>
    /// <remarks>
    ///		Billboards can be used for special effects or some other trickery which requires the
    ///		triangles to always facing the camera no matter where it is. The engine groups billboards into
    ///		sets for efficiency, so you should never create a billboard on it's own (it's ok to have a
    ///		set of one if you need it).
    ///		<p/>
    ///		Billboards have their geometry generated every frame depending on where the camera is. It is most
    ///		beneficial for all billboards in a set to be identically sized since the engine can take advantage of this and
    ///		save some calculations - useful when you have sets of hundreds of billboards as is possible with special
    ///		effects. You can deviate from this if you wish (example: a smoke effect would probably have smoke puffs
    ///		expanding as they rise, so each billboard will legitimately have it's own size) but be aware the extra
    ///		overhead this brings and try to avoid it if you can.
    ///		<p/>
    ///		Billboards are just the mechanism for rendering a range of effects such as particles. It is other classes
    ///		which use billboards to create their individual effects, so the methods here are quite generic.
    /// </remarks>
    public class Billboard {
        #region Member variables

        protected bool hasOwnDimensions;
        internal float width, height;
        protected bool useTexcoordRect;
        protected short texcoordIndex;
        protected RectangleF texcoordRect;

        // Intentional public access, since having a property for these for 1,000s of billboards
        // could be too costly
        public Vector3 Position = Vector3.Zero;
        public Vector3 Direction = Vector3.Zero;
        public BillboardSet ParentSet;
        public ColorEx Color = ColorEx.White;
		/// <summary>
		///		Needed for particle systems
		/// </summary>
		public float rotationInRadians = 0;

        #endregion

        #region Constructor

        /// <summary>
        ///		Default constructor.
        /// </summary>
        public Billboard() {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="owner"></param>
        public Billboard(Vector3 position, BillboardSet owner) {
            this.Position = position;
            this.ParentSet = owner;
            this.Color = ColorEx.White;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="owner"></param>
        /// <param name="color"></param>
        public Billboard(Vector3 position, BillboardSet owner, ColorEx color) {
            this.Color = color;
            this.Position = position;
            this.ParentSet = owner;
        }

        #endregion

        #region Properties

        /// <summary>
        ///		Width and height of this billboard, if it has it's own.
        /// </summary>
        public float Width {
            get { 
				return width; 
			}
            set {
                hasOwnDimensions = true;
                width = value; 
                ParentSet.NotifyBillboardResized();
            }
        }

        /// <summary>
        ///		Width and height of this billboard, if it has it's own.
        /// </summary>
        public float Height {
            get { 
				return height; 
			}
            set {
                hasOwnDimensions = true;
                height = value; 
                ParentSet.NotifyBillboardResized();
            }
        }

        /// <summary>
        ///		Specifies whether or not this billboard has different dimensions than the rest in the set.
        /// </summary>
        public bool HasOwnDimensions {
            get { 
				return hasOwnDimensions; 
			}
            set {
                hasOwnDimensions = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///		Resets this billboard to use the parent BillboardSet's dimensions instead of it's own.
        /// </summary>
        public virtual void ResetDimensions() {
            hasOwnDimensions = false;
        }

		/// <summary>
		///		Sets the width and height for this billboard.
		/// </summary>
		/// <param name="width">Width of the billboard.</param>
		/// <param name="height">Height of the billboard.</param>
		public virtual void SetDimensions(float width, float height) {
			hasOwnDimensions = true;
			this.width = width;
			this.height = height;
			ParentSet.NotifyBillboardResized();
		}

        /// <summary>
        ///		Internal method for notifying a billboard of it's owner.
        /// </summary>
        /// <param name="owner"></param>
        internal void NotifyOwner(BillboardSet owner) {
            ParentSet = owner;
        }

		/// <summary>
		///		Gets/Sets the rotation in degrees.
		/// </summary>
		public float Rotation {
			get {
				return rotationInRadians * MathUtil.DEGREES_PER_RADIAN;
			}
			set {
				rotationInRadians = value * MathUtil.RADIANS_PER_DEGREE;
				if(rotationInRadians != 0)
					ParentSet.NotifyBillboardRotated();
			}
		}

        public RectangleF TexcoordRect {
            get {
                return texcoordRect;
            }
            set {
                texcoordRect = value;
                useTexcoordRect = true;
            }
        }

        public bool UseTexcoordRect {
            get {
                return useTexcoordRect;
            }
            set {
                useTexcoordRect = value;
            }
        }

        public short TexcoordIndex {
            get {
                return texcoordIndex;
            }
            set {
                texcoordIndex = value;
                useTexcoordRect = false;
            }
        }
		#endregion
    }
}
