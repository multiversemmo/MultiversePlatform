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

namespace Axiom.Collections {

    /// <summary>
    ///		Serves as a basis for strongly typed collections in the engine.
    /// </summary>
    /// <remarks>
    ///		Can't wait for Generics in .Net Framework 2.0!   
    /// </remarks>
    public abstract class AxiomCollection : ICollection, IEnumerable {
        /// <summary></summary>
        protected SortedList objectList;
        /// <summary></summary>
        protected Object parent;
        static protected int nextUniqueKeyCounter;
		
        const int INITIAL_CAPACITY = 60;

        #region Constructors

        /// <summary>
        ///		
        /// </summary>
        public AxiomCollection() {
            this.parent = null;
            objectList = new SortedList(INITIAL_CAPACITY);
        }

        /// <summary>
        ///		
        /// </summary>
        /// <param name="parent"></param>
        public AxiomCollection(Object parent) {
            this.parent = parent;
            objectList = new SortedList(INITIAL_CAPACITY);
        }

        #endregion

        /// <summary>
        ///		
        /// </summary>
        public object this[int index] { 
            get {  
                return objectList.GetByIndex(index); 
            } 
            set { 
                objectList.SetByIndex(index, value); 
            }
        }

		public ICollection Values { get{ return objectList.Values; } }

		public ICollection Keys { get{ return objectList.Keys; } }

        /// <summary>
        ///		
        /// </summary>
        protected object this[object key] { 
            get { return objectList[key]; } 
            set { objectList[key] = value; }
        }

        /// <summary>
        ///		Accepts an unnamed object and names it manually.
        /// </summary>
        /// <param name="item"></param>
        protected void Add(object item) {
            objectList.Add("Object" + (nextUniqueKeyCounter++), item);
        }

        /// <summary>
        ///		Adds a named object to the collection.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        protected void Add(object key, object item) {
            objectList.Add(key, item);
        }

        /// <summary>
        ///		Clears all objects from the collection.
        /// </summary>
        public void Clear() {
            objectList.Clear();
        }

        /// <summary>
        ///		Removes the item from the collection.
        /// </summary>
        /// <param name="item"></param>
        public virtual void Remove(object item) {
            int index = objectList.IndexOfValue(item);

            if(index != -1)
                objectList.RemoveAt(index);
        }

		
		/// <summary>
		///		Removes the item from the collection.
		/// </summary>
		/// <param name="item"></param>
		public virtual void RemoveByKey(object item) 
		{
			int index = objectList.IndexOfKey(item);

			if(index != -1)
				objectList.RemoveAt(index);
		}


		/// <summary>
		///		Removes an item at the specified index.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index) {
			objectList.RemoveAt(index);
		}

        /// <summary>
        ///		Tests if there is a dupe entry in here.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(object key) {
            return objectList.ContainsKey(key);
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
            return objectList.Values.GetEnumerator();
        }

        #endregion

        #region Implementation of IEnumerator

        public class Enumerator : IEnumerator {
            private int position = -1;
            private AxiomCollection list;

            public Enumerator(AxiomCollection list) {
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
                    return list.objectList.GetByIndex(position); 
                }
            }
        }
        #endregion

    }
}
