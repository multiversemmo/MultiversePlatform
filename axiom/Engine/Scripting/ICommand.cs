using System;

namespace Axiom.Scripting
{
	/// <summary>
	/// 	Summary description for ICommand.
	/// </summary>
    public interface ICommand {
        /// <summary>
        ///    Gets the value for this command from the target object.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        string Get(object target);

        /// <summary>
        ///    Sets the value for this command on the target object.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="val"></param>
        void Set(object target, string val);
    }
}
