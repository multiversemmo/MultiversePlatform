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
using Axiom.Core;

namespace Axiom.Graphics {
	/// <summary>
	/// 	This ResourceManager manages high-level vertex and fragment programs. 
	/// </summary>
	/// <remarks>
	///    High-level vertex and fragment programs can be used instead of assembler programs
    ///    as managed by <see cref="GpuProgramManager"/>; however they typically result in a 
    ///    <see cref="GpuProgram"/> being created as a derivative of the high-level program. 
    ///    High-level programs are easier to write, and can often be API-independent, 
    ///    unlike assembler programs. 
	///    <p/>
	///    This class not only manages the programs themselves, it also manages the factory
	///    classes which allow the creation of high-level programs using a variety of high-level
	///    syntaxes. Plugins can be created which register themselves as high-level program
	///    factories and as such the engine can be extended to accept virtually any kind of
	///    program provided a plugin is written.
	/// </remarks>
	public class HighLevelGpuProgramManager : ResourceManager {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static HighLevelGpuProgramManager instance;

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        internal HighLevelGpuProgramManager() {
            if (instance == null) {
                instance = this;
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static HighLevelGpuProgramManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

		#region Fields

		/// <summary>
		///    Lookup table for list of registered factories.
		/// </summary>
		protected Hashtable factories = new Hashtable();
		
		#endregion Fields
		
		#region Methods
		
		/// <summary>
		///    Add a new factory object for high-level programs of a given language.
		/// </summary>
		/// <param name="factory">
		///    The factory instance to register.
		/// </param>
		public void AddFactory(IHighLevelGpuProgramFactory factory) {
			factories.Add(factory.Language, factory);
		}

		/// <summary>
		///    Creates a new, unloaded HighLevelGpuProgram instance.
		/// </summary>
		/// <remarks>
		///    This method creates a new program of the type specified as the second and third parameters.
		///    You will have to call further methods on the returned program in order to 
		///    define the program fully before you can load it.
		/// </remarks>
		/// <param name="name">Name of the program to create.</param>
		/// <param name="language">HLSL language to use.</param>
		/// <param name="type">Type of program, i.e. vertex or fragment.</param>
		/// <returns>An unloaded instance of HighLevelGpuProgram.</returns>
		public HighLevelGpuProgram CreateProgram(string name, string language, GpuProgramType type) {
			// lookup the factory for the requested program language
			IHighLevelGpuProgramFactory factory = GetFactory(language);

			if(factory == null) {
				throw new Exception(string.Format("Could not find HighLevelGpuProgramManager that can compile programs of type '{0}'", language));
			}

			// create the high level program using the factory
			HighLevelGpuProgram program = factory.Create(name, type);
			Add(program);
			return program;
		}

		/// <summary>
		///    Retreives a factory instance capable of producing HighLevelGpuPrograms of the
		///    specified language.
		/// </summary>
		/// <param name="language">HLSL language.</param>
		/// <returns>A factory capable of creating a HighLevelGpuProgram of the specified language.</returns>
		public IHighLevelGpuProgramFactory GetFactory(string language) {
			if(factories.ContainsKey(language)) {
				return (IHighLevelGpuProgramFactory)factories[language];
			}
            
			// wasn't found, so return null
			return null;
		}

		#endregion Methods
		
		#region Properties

		public string[] ProgramNames {
			get {
				string[] sl = new string[resourceList.Count];
				int count = 0;
				foreach(string s in resourceList.Keys) {
					sl[count++] = s;
				}
				return sl;
			}
		}
		
		#endregion Properties

		#region ResourceManager Implementation

		/// <summary>
		///    Overridden to throw an exception since this Create method isn't sufficient enough
		///    for creating HighLevelGpuPrograms, since more info is required.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public override Resource Create(string name, bool isManual) {
			throw new AxiomException("The more specific method, CreateProgram should be used.");
		}

        /// <summary>
        ///     Gets a HighLevelGpuProgram with the specified name.
        /// </summary>
        /// <param name="name">Name of the program to retrieve.</param>
        /// <returns>The high level gpu program with the specified name.</returns>
		public new HighLevelGpuProgram GetByName(string name) {
			return (HighLevelGpuProgram)base.GetByName(name);
		}

        /// <summary>
        ///     Called when the engine is shutting down.
        /// </summary>
        public override void Dispose() {
            base.Dispose();

            instance = null;
        }

        #endregion ResourceManager Implementation
    }

    /// <summary>
	///    Interface definition for factories that create instances of HighLevelGpuProgram.
	/// </summary>
	public interface IHighLevelGpuProgramFactory {
		#region Methods

		/// <summary>
		///    Create method which needs to be implemented to return an
		///    instance of a HighLevelGpuProgram.
		/// </summary>
		/// <param name="name">
		///    Name of the program to create.
		/// </param>
		/// <param name="type">
		///    Type of program to create, i.e. vertex or fragment.
		/// </param>
		/// <returns>
		///    A newly created instance of HighLevelGpuProgram.
		/// </returns>
        HighLevelGpuProgram Create(string name, GpuProgramType type);

		#endregion Methods

		#region Properties

		/// <summary>
		///    Gets the name of the HLSL language that this factory creates programs for.
		/// </summary>
		string Language {
			get;
		}

		#endregion Properties
	}
}
