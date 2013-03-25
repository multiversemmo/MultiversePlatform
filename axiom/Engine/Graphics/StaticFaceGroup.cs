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
using Axiom.MathLib;

namespace Axiom.Graphics {
	/// <summary>
	///     Collects a group of static ie immovable faces together which have common
    ///     properties like the material they use, the plane they lie on.
    /// </summary>
    /// <remarks>
	///     Whilst for discrete geometry (i.e. movable/scene objects) groups of faces are
	///     held in the SubMesh class, for immovable objects like scenery there
	///     needs to ba little more flexibility in the grouping since the group is
	///     likely to be a small part of a huge set of geometry. In addition, because
	///     the faces are unmoving certain optimisations can be performed, e.g.
	///     precalculating a world-coordinate bounding box and normal.
	///     <p/>
	///     Exactly how this class is used depends on the format of the large
	///     static geometry used in the level. An example would be the use of this
	///     class in the BspNode class for indoor levels.
	///     For flexibility and efficiency, it is not assumed that this class holds
	///     details of the vertices itself, or in fact that it holds the vertex indices
	///     itself. Everything is manipulated via pointers so if you want this
	///     class to point into a block of geometry data it can.
	/// </summary>
	public class StaticFaceGroup {
        /// <summary>
        ///     Type of face group.
        /// </summary>
        public FaceGroup type;

        /// <summary>
        ///     Is this a sky surface?
        /// </summary>
        public bool isSky;

        /// <summary>
        ///     Index into a buffer containing vertex definitions. Because we're
        ///     dealing with subsets of large levels this is likely to be part-way
        ///     through a huge vertex buffer.
        /// </summary>
        public int vertexStart;

        /// <summary>
        ///     The range of vertices in the buffer this facegroup references.
        ///     This is really for copying purposes only, so that we know which
        ///     subset of vertices to copy from our large-level buffer into the rendering buffer.
        /// </summary>
        public int numVertices;

        /// <summary>
        ///     Index into a buffer containing vertex indices. This buffer
        ///     may be individual to this group or shared for memory allocation
        ///     efficiency.  The vertex indexes are relative the the vertexStart pointer,
        ///     not to the start of the large-level buffer, allowing simple reindexing
        ///     when copying data into rendering buffers.
        ///     This is only applicable to FaceGroup.FaceList face groups.
        /// </summary>
        public int elementStart;

        /// <summary>
        ///     The number of vertex indices. This is only applicable to FaceGroup.FaceList face group types.
        /// </summary>
        public int numElements;

        /// <summary>
        ///     Handle to material used by this group.
        ///     Note the use of the material handle rather than the material
        ///     name - this is for efficiency since there will be many of these.
        /// </summary>
        public int materialHandle;

        public Plane plane;

        /// <remarks>
        ///     Patch surface (only applicable when type == FaceGroup.Patch)
        /// </remarks>
        public PatchSurface patchSurf;
	}
}
