using System;

namespace Axiom.Scripting
{
	/// <summary>
	/// 	Summary description for CommandAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class CommandAttribute : Attribute {

        #region Fields

        /// <summary>
        ///    Name of the command the target class will be registered to handle.
        /// </summary>
        private string name;
        /// <summary>
        ///    Description of what this command does.
        /// </summary>
        private string description;
        /// <summary>
        ///    Target type this class is meant to handle commands for.
        /// </summary>
        private Type target;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///    Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="target"></param>
        public CommandAttribute(string name, string description, Type target) {
            this.name = name;
            this.description = description;
            this.target = target;
        }

        /// <summary>
        ///    Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public CommandAttribute(string name, string description) {
            this.name = name;
            this.description = description;
        }

        /// <summary>
        ///    Constructor.
        /// </summary>
        /// <param name="name"></param>
        public CommandAttribute(string name) {
            this.name = name;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///    Name of this command.
        /// </summary>
        public string Name {
            get {
                return name;
            }
        }

        /// <summary>
        ///    Optional description of what this command does.
        /// </summary>
        public string Description {
            get {
                return description;
            }
        }

        /// <summary>
        ///    Optional target to specify what object type this command affects.
        /// </summary>
        public Type Target {
            get {
                return target;
            }
        }

        #endregion
	}
}
