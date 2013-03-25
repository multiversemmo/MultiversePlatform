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
using System.Collections;
using System.Diagnostics;
using Axiom.Collections;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Graphics {
	/// <summary>
	///     Abstract base class for classes that iterate over the triangles in a mesh
	/// </summary>
	public class AnyBuilder {
        #region Fields

		/// <summary>
		///		List of objects that will provide index data to the build process.
		/// </summary>
        protected IndexDataList indexDataList = new IndexDataList();
		/// <summary>
		///		Mapping of index data sets to vertex data sets.
		/// </summary>
        protected IntList indexDataVertexDataSetList = new IntList();
		/// <summary>
		///		List of vertex data objects.
		/// </summary>
        protected VertexDataList vertexDataList = new VertexDataList();
		/// <summary>
		///		Mappings of operation type to vertex data.
		/// </summary>
		protected OperationTypeList operationTypes = new OperationTypeList();

        #endregion Fields

        #region Methods

        /// <summary>
        ///     Add a set of vertex geometry data to the edge builder.
        /// </summary>
        /// <remarks>
        ///     You must add at least one set of vertex data to the builder before invoking the
        ///     <see cref="Build"/> method.
        /// </remarks>
        /// <param name="vertexData">Vertex data to consider for edge detection.</param>
        public void AddVertexData(VertexData vertexData) {
            vertexDataList.Add(vertexData);
        }

        /// <summary>
        ///     Add a set of index geometry data to the edge builder.
        /// </summary>
        /// <remarks>
        ///     You must add at least one set of index data to the builder before invoking the
        ///     <see cref="Build"/> method.
        /// </remarks>
        /// <param name="indexData">The index information which describes the triangles.</param>
        public void AddIndexData(IndexData indexData) {
            AddIndexData(indexData, 0, OperationType.TriangleList);
        }

        public void AddIndexData(IndexData indexData, int vertexSet) {
            AddIndexData(indexData, vertexSet, OperationType.TriangleList);
        }

        /// <summary>
        ///     Add a set of index geometry data to the edge builder.
        /// </summary>
        /// <remarks>
        ///     You must add at least one set of index data to the builder before invoking the
        ///     <see cref="Build"/> method.
        /// </remarks>
        /// <param name="indexData">The index information which describes the triangles.</param>
        /// <param name="vertexSet">
        ///     The vertex data set this index data refers to; you only need to alter this
        ///     if you have added multiple sets of vertices.
        /// </param>
        public void AddIndexData(IndexData indexData, int vertexSet, OperationType opType) {
            indexDataList.Add(indexData);
            indexDataVertexDataSetList.Add(vertexSet);
			operationTypes.Add(opType);
        }

        #endregion Methods
		
	}
}

