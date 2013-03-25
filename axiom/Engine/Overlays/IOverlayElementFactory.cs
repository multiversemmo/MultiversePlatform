using System;

namespace Axiom.Overlays {
	/// <summary>
	/// 	Defines the interface which all components wishing to 
	/// 	supply OverlayElement subclasses must implement.
	/// </summary>
	/// <remarks>
	/// 	To allow the OverlayElement types available for inclusion on 
	/// 	overlays to be extended, the engine allows external apps or plugins
	/// 	to register their ability to create custom OverlayElements with
	/// 	the GuiManager, using the AddOverlayElementFactory method. Classes
	/// 	wanting to do this must implement this interface.
	/// 	<p/>
	/// 	Each OverlayElementFactory creates a single type of OverlayElement, 
	/// 	identified by a 'type name' which must be unique.
	/// </summary>
	public interface IOverlayElementFactory {	
		#region Methods

        /// <summary>
        ///    Classes that implement this interface will return an instance of a OverlayElement of their designated
        ///    type.
        /// </summary>
        /// <param name="name">Name of the element to create.</param>
        /// <returns>A new instance of a OverlayElement with the specified name.</returns>
        OverlayElement Create(string name);
		
		#endregion
		
		#region Properties

        /// <summary>
        ///    Classes that implement this interface should return the name of the OverlayElement that it will be
        ///    responsible for creating.
        /// </summary>
        string Type {
            get;
        }
		
		#endregion
	}
}
