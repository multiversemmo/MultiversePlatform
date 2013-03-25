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
using Axiom.Animating;
using Axiom.Core;

namespace Axiom.Animating {
    /// <summary>
    /// Summary description for SkeletonManager.
    /// </summary>
    public class SkeletonManager : ResourceManager {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        protected static SkeletonManager instance;

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        protected internal SkeletonManager() {
            if (instance == null) {
                instance = this;
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static SkeletonManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

        #region ResourceManager Implementation

        /// <summary>
        ///    Creates a new skeleton object.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override Resource Create(string name, bool isManual) {
            return new Skeleton(name);
        }

        /// <summary>
        ///    Overloaded method.  Call overload with default of priority 1.
        /// </summary>
        /// <param name="fileName">Name of the skeleton file to load.</param>
        public Skeleton Load(string fileName) {
            return Load(fileName, 1);
        }

        /// <summary>
        ///    Load a skeleton.  Creates one if it doesn't exists, else return the cached version.
        /// </summary>
        /// <remarks>
        ///    Creates one if it doesn't exists, else return the cached version.
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="priority"></param>
        public Skeleton Load(string fileName, int priority) {
            Skeleton skeleton = GetByName(fileName);

            if(skeleton == null) {
                // create and load the skeleton
                skeleton = (Skeleton)Create(fileName);
                base.Load (skeleton, priority);
            }

            return skeleton;
        }

        public new Skeleton GetByName(string name) {
            return (Skeleton)base.GetByName(name);
        }

        #endregion ResourceManager Implementation
    }
}
