using System;

namespace Axiom.Core
{
	/// <summary>
	/// 	Describes behaviors required by objects that can be configured, whether through script
	/// 	parameters or programatically.
	/// </summary>
	public interface IConfigurable
	{
        /// <summary>
        ///    Will be called by script parsers that run across extended properties, and will pass them
        ///    along expecting the target object to handle them.
        /// </summary>
        /// <param name="name">
        ///    Name of the parameter.
        /// </param>
        /// <param name="val">
        ///    Value of the parameter.
        /// </param>
        /// <returns>
        ///    False if the param was not dealt with, True if it was.
        /// </returns>
		bool SetParam(string name, string val);
	}
}
