using System;

namespace Axiom.Graphics
{
	/// <summary>
	/// 	This class makes the usage of a vertex and fragment programs (low-level or high-level), 
	/// 	with a given set of parameters, explicit.
	/// </summary>
	/// <remarks>
	/// 	Using a vertex or fragment program can get fairly complex; besides the fairly rudimentary
	/// 	process of binding a program to the GPU for rendering, managing usage has few
	/// 	complications, such as:
	/// 	<ul>
	/// 	<li>Programs can be high level (e.g. Cg, GLSlang) or low level (assembler). Using
	/// 	either should be relatively seamless, although high-level programs give you the advantage
	/// 	of being able to use named parameters, instead of just indexed registers</li>
	/// 	<li>Programs and parameters can be shared between multiple usages, in order to save
	/// 	memory</li>
	/// 	<li>When you define a user of a program, such as a material, you often want to be able to
	/// 	set up the definition but not load / compile / assemble the program at that stage, because
	/// 	it is not needed just yet. The program should be loaded when it is first needed, or
	/// 	earlier if specifically requested. The program may not be defined at this time, you
	/// 	may want to have scripts that can set up the definitions independent of the order in which
	/// 	those scripts are loaded.</li>
	/// 	</ul>
	/// 	This class packages up those details so you don't have to worry about them. For example,
	/// 	this class lets you define a high-level program and set up the parameters for it, without
	/// 	having loaded the program (which you normally could not do). When the program is loaded and
	/// 	compiled, this class will then validate the parameters you supplied earlier and turn them
	/// 	into runtime parameters.
	/// 	<p/>
	/// 	Just incase it wasn't clear from the above, this class provides linkage to both 
	/// 	GpuProgram and HighLevelGpuProgram, despite its name.
	/// </remarks>
	public class GpuProgramUsage {
		#region Member variables
		
        /// <summary>
        ///    Type of program (vertex or fragment) this usage is being specified for.
        /// </summary>
        protected GpuProgramType type;
        /// <summary>
        ///    Reference to the program whose usage is being specified within this class.
        /// </summary>
        protected GpuProgram program;
        /// <summary>
        ///    Low level GPU program parameters.
        /// </summary>
        protected GpuProgramParameters parameters;

		#endregion
		
		#region Constructors
		
        /// <summary>
        ///    Default constructor.
        /// </summary>
        /// <param name="type">Type of program to link to.</param>
        public GpuProgramUsage(GpuProgramType type) {
            this.type = type;
        }
		
		#endregion
		
		#region Methods

		/// <summary>
		///		Creates and returns a copy of this GpuProgramUsage object.
		/// </summary>
		/// <returns></returns>
		public GpuProgramUsage Clone() {
			GpuProgramUsage usage = new GpuProgramUsage(type);
			usage.program = program;
			usage.parameters = parameters.Clone();

			return usage;
		}

        /// <summary>
        ///    Load this usage (and ensure program is loaded).
        /// </summary>
        internal void Load() {
            // only load the program if it isn't already loaded
            if(!program.IsLoaded) {
                program.Load(); 
            }
        }

        /// <summary>
        ///    Unload this usage.
        /// </summary>
        internal void Unload() {
            // TODO: Anything needed here?  The program cannot be destroyed since it is shared.
        }
		
		#endregion
		
		#region Properties
		
        /// <summary>
        ///    Gets/Sets the program parameters that should be used; because parameters can be
        ///    shared between multiple usages for efficiency, this method is here for you
        ///    to register externally created parameter objects.
        /// </summary>
        public GpuProgramParameters Params {
            get {
                if(parameters == null) {
                    throw new Exception("A program must be loaded before its parameters can be retreived.");
                }

                return parameters;
            }
            set {
                parameters = value;
            }
        }

        /// <summary>
        ///    Gets the program this usage is linked to; only available after the usage has been
        ///    validated either via enableValidation or by enabling validation on construction.
        /// </summary>
        /// <remarks>
        ///    Note that this will create a fresh set of parameters from the
        ///    new program being linked, so if you had previously set parameters
        ///    you will have to set them again.
        /// </remarks>
        public GpuProgram Program {
            get {
                return program;
            }
            set {
                program = value;

                // create program specific parameters
                parameters = program.CreateParameters();
            }
        }

        /// <summary>
        ///    Gets/Sets the name of the program we're trying to link to.
        /// </summary>
        /// <remarks>
        ///    Note that this will create a fresh set of parameters from the 
        ///    new program being linked, so if you had previously set parameters 
        ///    you will have to set them again. 
        /// </remarks>
        public string ProgramName {
            get {
                return program.Name;
            }
            set {
                // get a reference to the gpu program
                program = GpuProgramManager.Instance.GetByName(value);

                if(program == null) {
                    throw new Exception(string.Format("Unable to locate gpu program named '{0}'", value));
                }

                // create program specific parameters
                parameters = program.CreateParameters();
            }
        }

        /// <summary>
        ///    Gets the type of program we're trying to link to.
        /// </summary>
        public GpuProgramType Type {
            get {
                return type;
            }
        }

		#endregion
	}
}
