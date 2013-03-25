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
using System.Runtime.InteropServices;

namespace Axiom.Graphics {
    /// <summary>
    /// 	This class declares the usage of a single vertex buffer as a component
    /// 	of a complete <see cref="VertexDeclaration"/>. 
    /// </summary>
    public class VertexElement : ICloneable {
        #region Fields
		
        /// <summary>
        ///     The source vertex buffer, as bound to an index using <see cref="VertexBufferBinding"/>.
        /// </summary>
        protected ushort source;
        /// <summary>
        ///     The offset in the buffer that this element starts at.
        /// </summary>
        protected int offset;
        /// <summary>
        ///     The type of element.
        /// </summary>
        protected VertexElementType type;
        /// <summary>
        ///     The meaning of the element.
        /// </summary>
        protected VertexElementSemantic semantic;
        /// <summary>
        ///     Index of the item, only applicable for some elements like texture coords.
        /// </summary>
        protected int index;

        #endregion Fields
		
        #region Constructors
		
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="source">The source vertex buffer, as bound to an index using <see cref="VertexBufferBinding"/>.</param>
        /// <param name="offset">The offset in the buffer that this element starts at.</param>
        /// <param name="type">The type of element.</param>
        /// <param name="semantic">The meaning of the element.</param>
        public VertexElement(ushort source, int offset, VertexElementType type, VertexElementSemantic semantic) 
            : this(source, offset, type, semantic, 0) {}

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="source">The source vertex buffer, as bound to an index using <see cref="VertexBufferBinding"/>.</param>
        /// <param name="offset">The offset in the buffer that this element starts at.</param>
        /// <param name="type">The type of element.</param>
        /// <param name="semantic">The meaning of the element.</param>
        /// <param name="index">Index of the item, only applicable for some elements like texture coords.</param>
        public VertexElement(ushort source, int offset, VertexElementType type, VertexElementSemantic semantic, int index) {
            this.source = source;
            this.offset = offset;
            this.type = type;
            this.semantic = semantic;
            this.index = index;			
        }
		
        #endregion
		
        #region Methods
		
        /// <summary>
        ///     Utility method for helping to calculate offsets.
        /// </summary>
        public static int GetTypeSize(VertexElementType type) {

            switch(type) {
                case VertexElementType.Color:
                    return Marshal.SizeOf(typeof(int));

                case VertexElementType.Float1:
                    return Marshal.SizeOf(typeof(float));						

                case VertexElementType.Float2:
                    return Marshal.SizeOf(typeof(float)) * 2;

                case VertexElementType.Float3:
                    return Marshal.SizeOf(typeof(float)) * 3;

                case VertexElementType.Float4:
                    return Marshal.SizeOf(typeof(float)) * 4;

                case VertexElementType.Short1:
                    return Marshal.SizeOf(typeof(short));

                case VertexElementType.Short2:
                    return Marshal.SizeOf(typeof(short)) * 2;

                case VertexElementType.Short3:
                    return Marshal.SizeOf(typeof(short)) * 3;

                case VertexElementType.Short4:
                    return Marshal.SizeOf(typeof(short)) * 4;

                case VertexElementType.UByte4:
                    return Marshal.SizeOf(typeof(byte)) * 4;
            } // end switch

            // keep the compiler happy
            return 0;
        }

        /// <summary>
        ///     Utility method which returns the count of values in a given type.
        /// </summary>
        public static int GetTypeCount(VertexElementType type) {
            switch(type) {
                case VertexElementType.Color:
                    return 1;

                case VertexElementType.Float1:
                    return 1;						

                case VertexElementType.Float2:
                    return 2;

                case VertexElementType.Float3:
                    return 3;

                case VertexElementType.Float4:
                    return 4;

                case VertexElementType.Short1:
                    return 1;

                case VertexElementType.Short2:
                    return 2;

                case VertexElementType.Short3:
                    return 3;

                case VertexElementType.Short4:
                    return 4;

                case VertexElementType.UByte4:
                    return 4;
            } // end switch			

            // keep the compiler happy
            return 0;
        }

        /// <summary>
        ///		Returns proper enum for a base type multiplied by a value.  This is helpful
        ///		when working with tex coords especially since you might not know the number
        ///		of texture dimensions at runtime, and when creating the VertexBuffer you will
        ///		have to get a VertexElementType based on that amount to creating the VertexElement.
        /// </summary>
        /// <param name="type">Data type.</param>
        /// <param name="count">Multiplier.</param>
        /// <returns>
        ///     A <see cref="VertexElementType"/> that represents the requested type and count.
        /// </returns>
        /// <example>
        ///     MultiplyTypeCount(VertexElementType.Float1, 3) returns VertexElementType.Float3.
        /// </example>
        public static VertexElementType MultiplyTypeCount(VertexElementType type, int count) {
            switch(type) {
                case VertexElementType.Float1:
					switch(count) {
						case 1:
							return VertexElementType.Float1;
						case 2:
							return VertexElementType.Float2;
						case 3:
							return VertexElementType.Float3;
						case 4:
							return VertexElementType.Float4;
					}
					break;

                case VertexElementType.Short1:
					switch(count) {
						case 1:
							return VertexElementType.Short1;
						case 2:
							return VertexElementType.Short2;
						case 3:
							return VertexElementType.Short3;
						case 4:
							return VertexElementType.Short4;
					}
					break;
            }

            throw new Exception("Cannot multiply base vertex element type: " + type.ToString());
        }

        #endregion
		
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public ushort Source {
            get { 
                return source; 
            }
        }

        /// <summary>
        ///     Gets the offset into the buffer where this element starts.
        /// </summary>
        public int Offset {
            get { 
                return offset; 
            }
        }

        /// <summary>
        ///     Gets the data format of this element.
        /// </summary>
        public VertexElementType Type {
            get { 
                return type; 
            }
        }

        /// <summary>
        ///     Gets the meaning of this element.
        /// </summary>
        public VertexElementSemantic Semantic {
            get { 
                return semantic; 
            }
        }

        /// <summary>
        ///     Gets the index of this element, only applicable for repeating elements (like texcoords).
        /// </summary>
        public int Index {
            get { 
                return index; 
            }
        }

        /// <summary>
        ///     Gets the size of this element in bytes.
        /// </summary>
        public int Size {
            get { 
                return GetTypeSize(type); 
            }
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        ///     Simple memberwise clone since all local fields are value types.
        /// </summary>
        /// <returns></returns>
        public object Clone() {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
