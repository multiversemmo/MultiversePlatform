/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Package.Automation;

namespace Microsoft.MultiverseInterfaceStudio
{
    /// <summary>
    /// World of Warcraft project property automation object.
    /// </summary>
    [ComVisible(true)]
    public class OAWowProjectProperty : EnvDTE.Property
    {
        private OAProperties parent;
        private string name = String.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAWowProjectProperty"/> class.
        /// </summary>
        public OAWowProjectProperty()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="OAWowProjectProperty"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="name">The name.</param>
		internal OAWowProjectProperty(OAWowProjectProperties parent, string name)
		{
			this.parent = parent;
			this.name = name;
		}

		public object Application
		{
			get { return null; }
		}
		/// <summary>
		/// Gets the Collection containing the Property object supporting this property.
		/// </summary>
		public EnvDTE.Properties Collection
		{
			get
			{
				return this.parent;
			}
		}
		/// <summary>
		/// Gets the top-level extensibility object.
		/// </summary>
		public EnvDTE.DTE DTE
		{
			get
			{
				return this.parent.DTE;
			}
		}

		/// <summary>
		/// Returns one element of a list. 
		/// </summary>
		/// <param name="Index1">The index of the item to display.</param>
		/// <param name="Index2">The index of the item to display. Reserved for future use.</param>
		/// <param name="Index3">The index of the item to display. Reserved for future use.</param>
		/// <param name="Index4">The index of the item to display. Reserved for future use.</param>
		/// <returns>The value of a property</returns>
		// The message is suppressed to follow the csharp naming conventions instead of the base's naming convention that is using c++
		public object get_IndexedValue(object Index1, object Index2, object Index3, object Index4)
		{
			return null;
		}

		/// <summary>
		/// Setter function to set properties values. 
		/// </summary>
        /// <param name="_return"></param>
        public void let_Value(object _return)
		{
            this.Value = _return;
		}

		/// <summary>
		/// Gets the name of the object.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Gets the number of indices required to access the value.
		/// </summary>
		public short NumIndices
		{
			get { return 0; }
		}

		/// <summary>
		/// Sets or gets the object supporting the Property object.
		/// </summary>
		public object Object
		{
			get
			{
				return this.Value;
			}
			set
			{
				this.Value = value;
			}
		}

		public EnvDTE.Properties Parent
		{
			get { return this.parent; }
		}

		/// <summary>
		/// Sets the value of the property at the specified index.
		/// </summary>
		/// <param name="Index1">The index of the item to set.</param>
		/// <param name="Index2">Reserved for future use.</param>
		/// <param name="Index3">Reserved for future use.</param>
		/// <param name="Index4">Reserved for future use.</param>
		/// <param name="value">The value to set.</param>
		public void set_IndexedValue(object Index1, object Index2, object Index3, object Index4, object Val)
		{
		}

		/// <summary>
		/// Gets or sets the value of the property returned by the Property object.
		/// </summary>
		public object Value
		{
			get
			{
				return this.parent.Target.Node.ProjectMgr.GetProjectProperty(this.name);
			}
			set
			{
				if (value is string)
				{
					this.parent.Target.Node.ProjectMgr.SetProjectProperty(this.name, value.ToString());
				}
				else
				{
					this.parent.Target.Node.ProjectMgr.SetProjectProperty(this.name, value.ToString());
				}
			}
        }
    }
}
