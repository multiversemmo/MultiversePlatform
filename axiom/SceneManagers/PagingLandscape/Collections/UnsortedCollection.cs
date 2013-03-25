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



namespace Axiom.SceneManagers.PagingLandscape.Collections {



    /// <summary>

    ///		Serves as a basis for strongly typed collections in the engine.

    /// </summary>

    /// <remarks>

    ///		Can't wait for Generics in .Net Framework 2.0!   

    /// </remarks>

    public abstract class UnsortedCollection : ICollection, IEnumerable {

        /// <summary></summary>

        protected ArrayList objectList;

        /// <summary></summary>

        protected Object parent;

        static protected int nextUniqueKeyCounter;

		

        const int INITIAL_CAPACITY = 60;



        #region Constructors



        /// <summary>

        ///		

        /// </summary>

        public UnsortedCollection(): this(null,INITIAL_CAPACITY) {}



		/// <summary>

		///		

		/// </summary>

		public UnsortedCollection(int Capacity) : this(null, Capacity ) {}



        /// <summary>

        ///		

        /// </summary>

        /// <param name="parent"></param>

        public UnsortedCollection(Object parent) : this(parent, INITIAL_CAPACITY) {}



		/// <summary>

		///		

		/// </summary>

		/// <param name="parent"></param>

		public UnsortedCollection(Object parent, int Capacity) 

		{

			this.parent = parent;

			objectList = new ArrayList(Capacity);

		}



        #endregion



        /// <summary>

        ///		

        /// </summary>

        public object this[int index] 

		{ 

            get {  

                return objectList[index]; 

            } 

            set { 

                objectList[index] = value; 

            }

        }





        /// <summary>

        ///		Accepts an unnamed object and names it manually.

        /// </summary>

        /// <param name="item"></param>

        protected void Add(object item) 

		{

            objectList.Add(item);

        }



        /// <summary>

        ///		Clears all objects from the collection.

        /// </summary>

        public void Clear() 

		{

            objectList.Clear();

        }



		/// <summary>

		///		Removes an item at the specified index.

		/// </summary>

		/// <param name="index"></param>

		public void RemoveAt(int index) {

			objectList.RemoveAt(index);

		}



        #region Implementation of ICollection



        public void CopyTo(System.Array array, int index) {

            objectList.CopyTo(array, index);

        }



        public bool IsSynchronized {

            get {

                return objectList.IsSynchronized;

            }

        }



        public int Count {

            get {

                return objectList.Count;

            }

        }



        public object SyncRoot {

            get {

                return objectList.SyncRoot;

            }

        }



        #endregion



        #region Implementation of IEnumerable



        public System.Collections.IEnumerator GetEnumerator() {

            return new Enumerator(this);

        }



        #endregion



        #region Implementation of IEnumerator



        public class Enumerator : IEnumerator {

            private int position = -1;

            private UnsortedCollection list;



            public Enumerator(UnsortedCollection list) {

                this.list = list;

            }



            /// <summary>

            ///		Resets the in progress enumerator.

            /// </summary>

            public void Reset() {

                // reset the enumerator position

                position = -1;

            }



            /// <summary>

            ///		Moves to the next item in the enumeration if there is one.

            /// </summary>

            /// <returns></returns>

            public bool MoveNext() {

                position += 1;



                if(position >= list.objectList.Count) {

                    return false;

                }

                else {

                    return true;

                }

            }



            /// <summary>

            ///		Returns the current object in the enumeration.

            /// </summary>

            public object Current {

                get { 

                    return list.objectList[position]; 

                }

            }

        }

        #endregion



    }

}

