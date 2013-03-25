using System;
using System.Diagnostics;
using Axiom.Core;
using Axiom.Graphics;
using Tao.Cg;

namespace Axiom.CgPrograms {
	/// <summary>
	/// 	Specialization of HighLevelGpuProgram to provide support for nVidia's Cg language.
	/// </summary>
	/// <remarks>
	///    Cg can be used to compile common, high-level, C-like code down to assembler
	///    language for both GL and Direct3D, for multiple graphics cards. You must
	///    supply a list of profiles which your program must support using
	///    SetProfiles() before the program is loaded in order for this to work. The
	///    program will then negotiate with the renderer to compile the appropriate program
	///    for the API and graphics card capabilities.
	/// </remarks>
	public class CgProgram : HighLevelGpuProgram {
		#region Fields

        /// <summary>
        ///    Current Cg context id.
        /// </summary>
        protected IntPtr cgContext;
        /// <summary>
        ///    Current Cg program id.
        /// </summary>
        protected IntPtr cgProgram;
        /// <summary>
        ///    Entry point of the Cg program.
        /// </summary>
        protected string entry;
        /// <summary>
        ///    List of requested profiles for this program.
        /// </summary>
        protected string[] profiles;
        /// <summary>
        ///    Chosen profile for this program.
        /// </summary>
        protected string selectedProfile;
        protected int selectedCgProfile;
		
		#endregion Fields
		
		#region Constructors
		
        /// <summary>
        ///    Constructor.
        /// </summary>
        /// <param name="name">Name of this program.</param>
        /// <param name="type">Type of this program, vertex or fragment program.</param>
        /// <param name="language">HLSL language of this program.</param>
        /// <param name="context">CG context id.</param>
		public CgProgram(string name, GpuProgramType type, string language, IntPtr context) : base(name, type, language) {
			this.cgContext = context;
		}
		
		#endregion
		
		#region Methods

        protected void SelectProfile() {
            selectedProfile = "";
            selectedCgProfile = Cg.CG_PROFILE_UNKNOWN;

            for(int i = 0; i < profiles.Length; i++) {
                if(GpuProgramManager.Instance.IsSyntaxSupported(profiles[i])) {
                    selectedProfile = profiles[i];
                    selectedCgProfile = Cg.cgGetProfile(selectedProfile);

                    CgHelper.CheckCgError("Unable to find Cg profile enum for program " + name, cgContext);

                    break;
                }
            }
        }

        /// <summary>
        ///    
        /// </summary>
        protected override void LoadFromSource() {
            SelectProfile();

            string[] args = null;

            // This option causes an error with the CG 1.3 compiler
            if(selectedCgProfile == Cg.CG_PROFILE_VS_1_1) {
                args = new string[] {"-profileopts", "dcls", null};
            }

            // create the Cg program
            cgProgram = Cg.cgCreateProgram(cgContext, Cg.CG_SOURCE, source, selectedCgProfile, entry, args);

            string cgErrStr = CgHelper.FormatCgError("Unable to compile Cg program " + name, cgContext);

            if (cgErrStr != null)
            {
                LogManager.Instance.Write("Unable to compile CG program {0}:\n{1}", name, cgErrStr);
                CgHelper.CheckCgError("Unable to compile Cg program " + name, cgContext);
            }
        }

        /// <summary>
        ///    Create as assembler program from the compiled source supplied by the Cg compiler.
        /// </summary>
        protected override void CreateLowLevelImpl() {  
            // retreive the 
            string lowLevelSource = Cg.cgGetProgramString(cgProgram, Cg.CG_COMPILED_PROGRAM);

            // create a low-level program, with the same name as this one
            assemblerProgram = GpuProgramManager.Instance.CreateProgramFromString(name, lowLevelSource, type, selectedProfile);
        }

        /// <summary>
        ///    
        /// </summary>
        /// <param name="parms"></param>
        protected override void PopulateParameterNames(GpuProgramParameters parms) {
            Debug.Assert(cgProgram != IntPtr.Zero);

            // Note use of 'leaf' format so we only get bottom-level params, not structs
            IntPtr param = Cg.cgGetFirstLeafParameter(cgProgram, Cg.CG_PROGRAM);

            int index = 0;

            // loop through the rest of the params
            while(param != IntPtr.Zero) {

                // get the type of this param up front
                int paramType = Cg.cgGetParameterType(param);

                // Look for uniform parameters only
                // Don't bother enumerating unused parameters, especially since they will
                // be optimized out and therefore not in the indexed versions
                if(Cg.cgIsParameterReferenced(param) != 0
                    && Cg.cgGetParameterVariability(param) == Cg.CG_UNIFORM
                    && Cg.cgGetParameterDirection(param) != Cg.CG_OUT
                    && paramType != Cg.CG_SAMPLER1D
                    && paramType != Cg.CG_SAMPLER2D
                    && paramType != Cg.CG_SAMPLER3D
                    && paramType != Cg.CG_SAMPLERCUBE
                    && paramType != Cg.CG_SAMPLERRECT) {                 

                    // get the name and index of the program param
                    string name = Cg.cgGetParameterName(param);

                    // fp30 uses named rather than indexed params, so Cg returns
                    // 0 for the index. However, we need the index to be unique here
                    // Resource type 3256 doesn't have a define in the Cg header, so I assume
                    // it means an unused or non-indexed params, since it is also returned
                    // for programs that have a param that is not referenced in the program
                    // and ends up getting pruned by the Cg compiler
                    if(selectedCgProfile == Cg.CG_PROFILE_FP30) {
                        // use a fake index just to order the named fp30 params
                        index++;
                    }
                    else {
                        // get the param constant index the normal way 
                        index = Cg.cgGetParameterResourceIndex(param);
                    }

                    // get the underlying resource type of this param
                    // we need a special case for the register combiner
                    // stage constants.
                    int resource = Cg.cgGetParameterResource(param);

                    // Get the parameter resource, so we know what type we're dealing with
                    switch(resource) {
                        case Cg.CG_COMBINER_STAGE_CONST0:
                            // register combiner, const 0
                            // the index relates to the texture stage; store this as (stage * 2) + 0
                            index = index * 2;
                            break;

                        case Cg.CG_COMBINER_STAGE_CONST1:
                            // register combiner, const 1
                            // the index relates to the texture stage; store this as (stage * 2) + 1
                            index = (index * 2) + 1;
                            break;
                    }

                    // map the param to the index
                    parms.MapParamNameToIndex(name, index);
                }

                // get the next param
                param = Cg.cgGetNextLeafParameter(param);
            }
        }

        /// <summary>
        ///    Unloads the Cg program.
        /// </summary>
        protected override void UnloadHighLevelImpl() {
            // destroy this program if it had been loaded
			if (cgProgram != IntPtr.Zero) {
				Cg.cgDestroyProgram(cgProgram);

				CgHelper.CheckCgError(string.Format("Error unloading CgProgram named '{0}'", this.name), cgContext);
                cgProgram = IntPtr.Zero;
			}
        }

		/// <summary>
		///		Only bother with supported programs.
		/// </summary>
		public override void Touch() {
			if(this.IsSupported) {
				base.Touch ();
			}
		}


		#endregion
		
		#region Properties

        /// <summary>
        ///    Returns whether or not this high level gpu program is supported on the current hardware.
        /// </summary>
        public override bool IsSupported {
            get {
				// If skeletal animation is being done, we need support for UBYTE4
				if(this.IsSkeletalAnimationIncluded &&
					!Root.Instance.RenderSystem.Caps.CheckCap(Capabilities.VertexFormatUByte4)) {

					return false;
				}

                // see if any profiles are supported
                for(int i = 0; i < profiles.Length; i++) {
                    if(GpuProgramManager.Instance.IsSyntaxSupported(profiles[i])) {
                        return true;
                    }
                }

                // nope, SOL
                return false;
            }
        }

        public override int SamplerCount
        {
            get
            {
                switch (selectedProfile)
                {
                    case "ps_1_1":
                    case "ps_1_2":
                    case "ps_1_3":
                    case "fp20":
                        return 4;
                    case "ps_1_4":
                        return 6;
                    case "ps_2_0":
                    case "ps_2_x":
                    case "ps_3_0":
                    case "ps_3_x":
                    case "arbfp1":
                    case "fp30":
                    case "fp40":
                        return 16;
                    default:
                        throw new AxiomException("Attempted to query sample count for unknown shader profile({0}).", selectedProfile);
                }
            }
        }
		
		#endregion

        #region IConfigurable Members

        /// <summary>
        ///    Method for passing parameters into the CgProgram.
        /// </summary>
        /// <param name="name">
        ///    Param name.
        /// </param>
        /// <param name="val">
        ///    Param value.
        /// </param>
        public override bool SetParam(string name, string val) {
            bool handled = true;

            switch(name) {
                case "entry_point":
                    entry = val;
                    break;
                
                case "profiles":
                    profiles = val.Split(' ');
                    break;

                default:
                    LogManager.Instance.Write("CgProgram: Unrecognized parameter '{0}'", name);
                    handled = false;
                    break;
            }

            return handled;
        }

        #endregion IConfigurable Members
    }
}
