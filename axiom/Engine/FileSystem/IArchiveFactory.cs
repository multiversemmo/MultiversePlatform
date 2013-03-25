using System;
using System.Collections.Generic;

namespace Axiom.FileSystem {
	
	/// <summary>
	/// 	Interface for plugin developers to override to create new types of archive to load
	/// 	resources from.
	/// </summary>
	public interface IArchiveFactory 
	{
		#region Methods
		/// <summary>
		/// 	Create an archive object based on the name.
		/// </summary>
		/// <param name="name">the name identifying the archive to the factory
		/// (usually a zip or directory name)</param>
		/// <returns>An Archive matching the name</returns>
		Archive CreateArchive(string name);
        
		#endregion Methods
        
		#region Properties
        
		/// <summary>
		///		Name of the archive supported by this factory.
		/// </summary>
		string Type { get; }
        
		#endregion Properties
	}
}