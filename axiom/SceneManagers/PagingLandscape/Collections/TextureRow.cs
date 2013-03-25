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



using Axiom.Core;

using Axiom.Collections;



// used to alias a type in the code for easy copying and pasting.  Come on generics!!

using T = Axiom.SceneManagers.PagingLandscape.Texture.Texture;

// used to alias a key value in the code for easy copying and pasting.  Come on generics!!

using K = System.String;



namespace Axiom.SceneManagers.PagingLandscape.Collections 

{

	/// <summary>

	///		A strongly-typed collection of <see cref="Page"/> objects.

	/// </summary>

	[Serializable]

	public class TextureRow : UnsortedCollection 

	{

		#region Constructors



		/// <summary>

		///		Default constructor.

		/// </summary>

		public TextureRow() : base() {}

		public TextureRow(int Capacity) : base(Capacity) {}

		public TextureRow(object Parent) : base(Parent) {}

		public TextureRow(object Parent, int Capacity) : base(Parent, Capacity) {}



		#endregion



		#region Strongly typed methods and indexers



		/// <summary>

		///		Get/Set indexer that allows access to the collection by index.

		/// </summary>

		new public T this[int index] 

		{

			get { return (T)base[index]; }

			set { base[index] = value; }

		}



		/// <summary>

		///		Get/Set indexer that allows access to the collection by index.

		/// </summary>

		public T this[long index] 

		{

			get { return (T)base[(int)index]; }

			set { base[(int)index] = value; }

		}



		/// <summary>

		///		Adds an object to the collection.

		/// </summary>

		/// <param name="item"></param>

		public void Add(T item) 

		{

			base.Add(item);

		}



		#endregion

	}

}

