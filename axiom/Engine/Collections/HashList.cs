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
    /// 	Summary description for HashList.
    /// </summary>
    public class HashList {
        SortedList itemList = new SortedList();

        #region Member variables
		
        #endregion
		
        #region Constructors
		
        public HashList() {
            //
            // TODO: Add constructor logic here
            //
        }
		
        #endregion
		
        #region Methods
		
        public void Add(object key, object item) {
            itemList.Add(key, item);
        }

        public object GetByKey(object key) {
            return this[key];
        }

        public bool ContainsKey(object key) {
            return itemList.ContainsKey(key);
        }

        public void Remove(object key) {
            itemList.Remove(key);
        }

        public void Clear() {
            itemList.Clear();
        }

        #endregion
		
        #region Properties
		
        public int Count {
            get { return itemList.Count; }
        }

        #endregion

        #region Operators

        public object this[int index] {
            get { 
				return itemList.GetByIndex(index); 
			}
        }

        public object this[object key] {
            get { 
				return itemList[key];
			}
        }

        #endregion

    }
}
