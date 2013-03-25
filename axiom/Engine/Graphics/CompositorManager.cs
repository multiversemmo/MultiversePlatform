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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Drawing;
using Axiom.Core;
using Axiom.Configuration;
using Axiom.Scripting;

namespace Axiom.Graphics {

	///<summary>
	///    Class for managing Compositor settings for Ogre. Compositors provide the means
	///    to flexibly "composite" the final rendering result from multiple scene renders
	///    and intermediate operations like rendering fullscreen quads. This makes
	///    it possible to apply postfilter effects, HDRI postprocessing, and shadow
	///    effects to a Viewport.
	///    
	///    When loaded from a script, a Compositor is in an 'unloaded' state and only stores the settings
	///    required. It does not at that stage load any textures. This is because the material settings may be
	///    loaded 'en masse' from bulk material script files, but only a subset will actually be required.
	///
	///    Because this is a subclass of ResourceManager, any files loaded will be searched for in any path or
	///    archive added to the resource paths/archives. See ResourceManager for details.
	///</summary>
	public class CompositorManager : ResourceManager {

        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static CompositorManager instance;

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        /// <remarks>
        ///     Protected internal because this singleton will actually hold the instance of a subclass
        ///     created by a render system plugin.
        /// </remarks>
        protected internal CompositorManager() {
            if (instance == null) {
                instance = this;
            }
			rectangle = null;
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static CompositorManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

		#region Fields

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CompositorManager));

		///<summary>
		///    Mapping from viewport to compositor chain
		///</summary>
        protected Dictionary<Viewport, CompositorChain> chains;
		///<summary>
		///    Serializer
		///</summary>
        //protected CompositorSerializer serializer;

		///<summary>
		///</summary>
		protected Rectangle2D rectangle;

		#endregion Fields


		#region Methods

		///<summary>
		///    Required by the ResourceManager base class
		///</summary>
		public override Resource Create(string name, bool isManual) {
			Compositor ret = new Compositor(name);
            Add(ret);
            return ret;
		}

		///<summary>
		///    Intialises the Compositor manager, which also triggers it to
		///    parse all available .compositor scripts.
		///</summary>
		public void Initialize() {
			Compositor scene = (Compositor)Create("Ogre/Scene");
			CompositionTechnique t = scene.CreateTechnique();
			CompositionTargetPass tp = t.OutputTarget;
			tp.VisibilityMask = 0xFFFFFFFF;
			CompositionPass pass = tp.CreatePass();
			pass.Type = CompositorPassType.Clear;
			CompositionPass nextPass = tp.CreatePass();
			nextPass.Type = CompositorPassType.RenderScene;
			/// Render everything, including skies
			pass.FirstRenderQueue = RenderQueueGroupID.SkiesEarly;
			pass.LastRenderQueue = RenderQueueGroupID.SkiesLate;

            chains = new Dictionary<Viewport, CompositorChain>();

			// parse all compositing scripts
			ParseAllSources();
		}

        /// <summary>
        ///		Parses all compositing script files in resource folders and archives.
        /// </summary>
        private void ParseAllSources() {
            StringCollection compositingFiles = ResourceManager.GetAllCommonNamesLike("", ".compositor");

            foreach(string file in compositingFiles) {
                Stream data = ResourceManager.FindCommonResourceData(file);
                try {
                    ParseScript(data, file);
                } catch (Exception e) {
                    LogManager.Instance.WriteException("Unable to parse compositing script '{0}': {1}", file, e.Message);
                }
            }
        }

		public void ParseScript(string script) {
			ParseScript(new StringReader(script), "");
		}

		public void ParseScript(Stream data, string file) {
			ParseScript(new StreamReader(data, System.Text.Encoding.ASCII), file);
		}

		protected static void LogError(CompositorScriptContext context, string error,
											params object[] substitutions) {
			StringBuilder errorBuilder = new StringBuilder();

			// log compositor name only if filename not specified
			if(context.filename == null && context.compositor != null) {
				errorBuilder.Append("Error in compositor ");
				errorBuilder.Append(context.compositor.Name);
				errorBuilder.Append(" : ");
				errorBuilder.AppendFormat("At line # {0}: '{1}'", context.lineNo, context.line);
				errorBuilder.AppendFormat(error, substitutions);
			}
			else {
				if(context.compositor != null) {
					errorBuilder.Append("Error in compositor ");
					errorBuilder.Append(context.compositor.Name);
					errorBuilder.AppendFormat(" at line # {0}: '{1}'", context.lineNo, context.line);
					errorBuilder.AppendFormat(" of {0}: ", context.filename);
					errorBuilder.AppendFormat(error, substitutions);
				}
				else {
					errorBuilder.AppendFormat("Error at line # {0}: '{1}'", context.lineNo, context.line);
					errorBuilder.AppendFormat(" of {0}: ", context.filename);
					errorBuilder.AppendFormat(error, substitutions);
				}
			}

            LogManager.Instance.Write(errorBuilder.ToString());
        }

        protected string[] SplitByWhitespace(string line, int count) {
			return line.Split(new char[] {' ', '\t'}, count);
		}
		
		protected string[] SplitArgs(string args) {
			return args.Split(new char[] {' ', '\t'});
		}

		protected string RemoveQuotes(string token) {
			if (token.Length >= 2 && token[0] == '\"')
				token = token.Substring(1);
			if (token[token.Length - 1] == '\"')
				token = token.Substring(0, token.Length - 1);
			return token;
		}
		
		protected bool OptionCount (CompositorScriptContext context, string introducer,
									int expectedCount, int count) {
			if (expectedCount < count) {
				LogError(context, "The '{0}' phrase requires {1} arguments", introducer, expectedCount);
				return false;
			}
			else
				return true;
		}
		
		protected bool OnOffArg(CompositorScriptContext context, string introducer, string []args) {
			if (OptionCount(context, introducer, 1, args.Length)) {
				string arg = args[0];
				if (arg == "on")
					return true;
				else if (arg == "off")
					return false;
				else {
					LogError(context, "Illegal '{0}' arg '{1}'; should be 'on' or 'off'", introducer, arg);
				}
			}
            return false;
		}
		
		protected int ParseInt(CompositorScriptContext context, string s) {
			string n = s.Trim();
			try {
				return int.Parse(n);
			}
			catch (Exception e) {
				LogError(context, "Error converting string '{0}' to integer; error message is '{1}'",
						 n, e.Message);
				return 0;
			}
		}

		protected uint ParseUint(CompositorScriptContext context, string s) {
			string n = s.Trim();
			try {
				return uint.Parse(n);
			}
			catch (Exception e) {
				LogError(context, "Error converting string '{0}' to unsigned integer; error message is '{1}'",
						 n, e.Message);
				return 0;
			}
		}

		protected float ParseFloat(CompositorScriptContext context, string s) {
			string n = s.Trim();
			try {
				return float.Parse(n);
			}
			catch (Exception e) {
				LogError(context, "Error converting string '{0}' to float; error message is '{1}'",
						 n, e.Message);
				return 0.0f;
			}
		}

		protected ColorEx ParseClearColor(CompositorScriptContext context, string [] args) {
            if (args.Length != 4) {
                LogError(context, "A color value must consist of 4 floating point numbers");
                return ColorEx.Black;
            }
            else {
                float r = ParseFloat(context, args[0]);
                float g = ParseFloat(context, args[0]);
                float b = ParseFloat(context, args[0]);
                float a = ParseFloat(context, args[0]);
				
				return new ColorEx(a, r, g, b);
            }
		}
		
		protected void LogIllegal(CompositorScriptContext context, string category, string token) {
			LogError(context, "Illegal {0} attribute '{1}'", category, token);
		}

		/// <summary>
        ///		Starts parsing an individual script file.
        /// </summary>
        /// <param name="data">Stream containing the script data.</param>
		public void ParseScript(TextReader script, string file) {

            string line = "";
			CompositorScriptContext context = new CompositorScriptContext();
			context.filename = file;
			context.lineNo = 0;

            // parse through the data to the end
            while((line = ParseHelper.ReadLine(script)) != null) {
				context.lineNo++;
				string[] splitCmd;
				string[] args;
                string arg;
				// ignore blank lines and comments
                if(!(line.Length == 0 || line.StartsWith("//"))) {
					context.line = line;
					splitCmd = SplitByWhitespace(line, 2);
                    string token = splitCmd[0];
					args = SplitArgs(splitCmd.Length == 2 ? splitCmd[1] : "");
					arg = (args.Length > 0 ? args[0] : "");
					if(context.section == CompositorScriptSection.None) {
						if (token != "compositor") {
							LogError(context, "First token is not 'compositor'!");
							break; // Give up
						}
                        string compositorName = RemoveQuotes(splitCmd[1].Trim());
                        context.compositor = (Compositor)CompositorManager.Instance.Create(compositorName);
						context.section = CompositorScriptSection.Compositor;
						context.seenOpen = false;
						continue; // next line
					}
					else {
						if (!context.seenOpen) {
							if (token == "{")
								context.seenOpen = true;
							else
								LogError(context, "Expected open brace '{'; instead got {0}", token);
							continue; // next line
						}
						switch(context.section) {
						case CompositorScriptSection.Compositor:
							switch (token) {
							case "technique":
								context.section = CompositorScriptSection.Technique;
                                context.technique = context.compositor.CreateTechnique();
								context.seenOpen = false;
								continue; // next line
							case "}":
								context.section = CompositorScriptSection.None;
								context.seenOpen = false;
								if (context.technique == null) {
									LogError(context, "No 'technique' section in compositor");
									continue;
								}
								break;
							default:
								LogError(context, 
										 "After opening brace '{' of compositor definition, expected 'technique', but got '{0}'",
										 token);
								continue; // next line
							}
							break;
						case CompositorScriptSection.Technique:
							switch (token) {
							case "texture":
								ParseTextureLine(context, args);
								break;
							case "target":
								context.section = CompositorScriptSection.Target;
								context.target = context.technique.CreateTargetPass();
								context.target.OutputName = arg.Trim();
								context.seenOpen = false;
								break;
							case "target_output":
								context.section = CompositorScriptSection.Target;
								context.target = context.technique.OutputTarget;
								context.seenOpen = false;
								break;
							case "}":
								context.section = CompositorScriptSection.Compositor;
								context.seenOpen = true;
								break;
							default:
								LogIllegal(context, "technique", token);
								break;
							}
							break;
						case CompositorScriptSection.Target:
							switch (token) {
							case "input":
								if (OptionCount(context, token, 1, args.Length)) {
									arg = args[0];
									if (arg == "previous")
										context.target.InputMode = CompositorInputMode.Previous;
									else if (arg == "none")
										context.target.InputMode = CompositorInputMode.None;
									else
										LogError(context, "Illegal 'input' arg '{0}'", arg);
								}
								break;
							case "only_initial":
								context.target.OnlyInitial = OnOffArg(context, token, args);
								break;
							case "visibility_mask":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.target.VisibilityMask = ParseUint(context, arg);
								break;
							case "lod_bias":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.target.LodBias = ParseInt(context, arg);
								break;
							case "material_scheme":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.target.MaterialScheme = arg.Trim();
								break;
							case "pass":
								context.section = CompositorScriptSection.Pass;
								context.pass = context.target.CreatePass();
								context.seenOpen = false;
								if (!OptionCount(context, token, 1, args.Length))
									break;
								arg = arg.Trim();
								switch(arg) {
								case "render_quad":
									context.pass.Type = CompositorPassType.RenderQuad;
									break;
								case "clear":
									context.pass.Type = CompositorPassType.Clear;
									break;
								case "stencil":
									context.pass.Type = CompositorPassType.Stencil;
									break;
								case "render_scene":
									context.pass.Type = CompositorPassType.RenderScene;
									break;
								default:
									LogError(context, "In line '{0}', unrecognized compositor pass type '{1}'", arg);
									break;
								}
								break;
							case "}":
								context.section = CompositorScriptSection.Technique;
								context.seenOpen = true;
								break;
							default:
								LogIllegal(context, "target", token);
								break;
							}
                            break;
						case CompositorScriptSection.Pass:
							switch (token) {
							case "first_render_queue":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.FirstRenderQueue = (RenderQueueGroupID)ParseInt(context, args[0]);
								break;
							case "last_render_queue":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.LastRenderQueue = (RenderQueueGroupID)ParseInt(context, args[0]);
								break;
							case "identifier":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.Identifier = ParseUint(context, args[0]);
								break;
							case "material":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.MaterialName = args[0].Trim();
								break;
							case "input":
								if (!OptionCount(context, token, 2, args.Length))
									break;
								context.pass.SetInput(ParseInt(context, args[0]), args[1].Trim());
								break;
							case "clear":
								context.section = CompositorScriptSection.Clear;
								context.seenOpen = false;
                                break;
							case "stencil":
								context.section = CompositorScriptSection.Clear;
								context.seenOpen = false;
                                break;
							case "}":
								context.section = CompositorScriptSection.Target;
								context.seenOpen = true;
								break;
							default:
								LogIllegal(context, "pass", token);
								break;
							}
                            break;
						case CompositorScriptSection.Clear:
							switch (token) {
							case "buffers":
								FrameBuffer fb = (FrameBuffer)0;
								foreach (string cb in args) {
									switch (cb) {
									case "colour":
										fb |= FrameBuffer.Color;
										break;
									case "color":
										fb |= FrameBuffer.Color;
										break;
									case "depth":
										fb |= FrameBuffer.Depth;
										break;
									case "stencil":
										fb |= FrameBuffer.Stencil;
										break;
									default:
										LogError(context, "When parsing pass clear buffers options, illegal option '{0}'", cb);
										break;
									}
								}
								break;
							case "colour":
								context.pass.ClearColor = ParseClearColor(context, args);
								break;
							case "color":
								context.pass.ClearColor = ParseClearColor(context, args);
								break;
							case "depth_value":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.ClearDepth = ParseFloat(context, args[0]);
								break;
							case "stencil_value":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.ClearDepth = ParseInt(context, args[0]);
								break;
							case "}":
								context.section = CompositorScriptSection.Pass;
								context.seenOpen = true;
								break;
							default:
								LogIllegal(context, "clear", token);
								break;
							}
                            break;
						case CompositorScriptSection.Stencil:
							switch (token) {
							case "check":
								context.pass.StencilCheck = OnOffArg(context, token, args);
								break;
							case "compare_func":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.StencilFunc = ParseCompareFunc(context, arg);
								break;
							case "ref_value":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.StencilRefValue = ParseInt(context, arg);
								break;
							case "mask":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.StencilMask = ParseInt(context, arg);
								break;
							case "fail_op":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.StencilFailOp = ParseStencilOperation(context, arg);
								break;
							case "depth_fail_op":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.StencilDepthFailOp = ParseStencilOperation(context, arg);
								break;
							case "pass_op":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.StencilPassOp = ParseStencilOperation(context, arg);
								break;
							case "two_sided":
								if (!OptionCount(context, token, 1, args.Length))
									break;
								context.pass.StencilTwoSidedOperation = OnOffArg(context, token, args);
								break;
							case "}":
								context.section = CompositorScriptSection.Pass;
								context.seenOpen = true;
								break;
							default:
								LogIllegal(context, "stencil", token);
								break;
							}
                            break;
						default:
							LogError(context, "Internal compositor parser error: illegal context");
							break;
						}
                    } // if
                } // if
            } // while
			if (context.section != CompositorScriptSection.None)
				LogError(context, "At end of file, unterminated compositor script!");
        }

		protected void ParseTextureLine(CompositorScriptContext context, string [] args) {
			if (args.Length == 4) {
				CompositionTextureDefinition textureDef = context.technique.CreateTextureDefinition(args[0]);
				textureDef.Width = (args[1] == "target_width" ? 0 : ParseInt(context, args[1]));
				textureDef.Height = (args[2] == "target_height" ? 0 : ParseInt(context, args[2]));
				switch (args[3]) {
				case "PF_A8R8G8B8":
					textureDef.Format = Axiom.Media.PixelFormat.A8R8G8B8;
					break;
                case "PF_R8G8B8A8":
                    textureDef.Format = Axiom.Media.PixelFormat.R8G8B8A8;
                    break;
				case "PF_R8G8B8":
					textureDef.Format = Axiom.Media.PixelFormat.R8G8B8;
					break;
                case "PF_FLOAT16_RGBA":
                    textureDef.Format = Axiom.Media.PixelFormat.FLOAT16_RGBA;
                    break;
                case "PF_FLOAT16_RGB":
                    textureDef.Format = Axiom.Media.PixelFormat.FLOAT16_RGB;
                    break;
                case "PF_FLOAT32_RGBA":
                    textureDef.Format = Axiom.Media.PixelFormat.FLOAT32_RGBA;
                    break;
				case "PF_FLOAT16_R":
					textureDef.Format = Axiom.Media.PixelFormat.FLOAT16_R;
					break;
				case "PF_FLOAT32_R":
					textureDef.Format = Axiom.Media.PixelFormat.FLOAT32_R;
					break;
				default:
					LogError(context, "Unsupported texture pixel format '{0}'", args[3]);
                    break;
				}
			}
		}

		protected CompareFunction ParseCompareFunc(CompositorScriptContext context, string arg) {
			switch (arg.Trim()) {
			case "always_fail":
				return CompareFunction.AlwaysFail;
			case "always_pass":
				return CompareFunction.AlwaysPass;
			case "less_equal":
				return CompareFunction.LessEqual;
			case "less'":
				return CompareFunction.Less;
			case "equal":
				return CompareFunction.Equal;
			case "not_equal":
				return CompareFunction.NotEqual;
			case "greater_equal":
				return CompareFunction.GreaterEqual;
			case "greater":
				return CompareFunction.Greater;
			default:
				LogError(context, "Illegal stencil compare_func '{0}'", arg);
				return CompareFunction.AlwaysPass;
			}
		}
		
		protected StencilOperation ParseStencilOperation(CompositorScriptContext context, string arg) {
			switch (arg.Trim()) {
			case "keep":
				return StencilOperation.Keep;
			case "zero":
				return StencilOperation.Zero;
			case "replace":
				return StencilOperation.Replace;
			case "increment_wrap":
				return StencilOperation.IncrementWrap;
			case "increment":
				return StencilOperation.Increment;
			case "decrement_wrap":
				return StencilOperation.DecrementWrap;
			case "decrement":
				return StencilOperation.Decrement;
			case "invert":
				return StencilOperation.Invert;
			default:
				LogError(context, "Illegal stencil_operation '{0}'", arg);
				return StencilOperation.Keep;
			}
		}

		///<summary>
		///    Get the compositor chain for a Viewport. If there is none yet, a new
		///    compositor chain is registered.
		///    XXX We need a _notifyViewportRemoved to find out when this viewport disappears,
		///    so we can destroy its chain as well.
		///</summary>
		public CompositorChain GetCompositorChain(Viewport vp) {
			CompositorChain chain;
			if (chains.TryGetValue(vp, out chain))
				return chain;
			else {
				chain = new CompositorChain(vp);
				chains[vp] = chain;
				return chain;
			}
		}

		///<summary>
		///    Returns whether exists compositor chain for a viewport.
		///</summary>
		public bool HasCompositorChain(Viewport vp) {
			return chains.ContainsKey(vp);
		}


		///<summary>
		///    Remove the compositor chain from a viewport if exists.
		///</summary>
		public void RemoveCompositorChain(Viewport vp) {
			chains.Remove(vp);
		}

		///<summary>
		///    Overridden from ResourceManager since we have to clean up chains too.
		///</summary>
		public void RemoveAll() {
			FreeChains();
			chains.Clear();
		}


		///<summary>
		///    Clear composition chains for all viewports
		///</summary>
		protected void FreeChains() {
            // Do I need to dispose the CompositorChain objects?
            chains.Clear();
		}
			
		///<summary>
		///    Get a textured fullscreen 2D rectangle, for internal use.
		///</summary>
		public IRenderable GetTexturedRectangle2D() {
			if(rectangle == null)
				/// 2D rectangle, to use for render_quad passes
				rectangle = new Rectangle2D(true);
			RenderSystem rs = Root.Instance.RenderSystem;
			Viewport vp = rs.ActiveViewport;
			float hOffset = rs.HorizontalTexelOffset / (0.5f * vp.ActualWidth);
			float vOffset = rs.VerticalTexelOffset / (0.5f * vp.ActualHeight);
			rectangle.SetCorners(-1f + hOffset, 1f - vOffset, 1f + hOffset, -1f - vOffset);
			return rectangle;
		}

		///<summary>
		///    Add a compositor to a viewport. By default, it is added to end of the chain,
		///    after the other compositors.
		///</summary>
		///<param name="vp">Viewport to modify</param>
		///<param name="compositor">The name of the compositor to apply</param>
		///<param name="addPosition">At which position to add, defaults to the end (-1).</param>
		///<returns>pointer to instance, or null if it failed.</returns>
		public CompositorInstance AddCompositor(Viewport vp, string compositor, int addPosition) {
			Compositor comp = (Compositor)GetByName(compositor);
			if(comp == null)
				return null;
			CompositorChain chain = GetCompositorChain(vp);
			return chain.AddCompositor(comp, addPosition == -1 ? CompositorChain.LastCompositor : addPosition);
		}

		public CompositorInstance AddCompositor(Viewport vp, string compositor) {
			return AddCompositor(vp, compositor, -1);
		}
		
		///<summary>
		///    Remove a compositor from a viewport
		///</summary>
		public void RemoveCompositor(Viewport vp, string compositor) {
			CompositorChain chain = GetCompositorChain(vp);
			for (int i=0; i< chain.Instances.Count; i++) {
				CompositorInstance instance = chain.GetCompositor(i);
				if (instance.Compositor.Name == compositor) {
					chain.RemoveCompositor(i);
					break;
				}
			}
		}

        /// <summary>
        /// another overload to remove a compositor instance from its chain
        /// </summary>
        /// <param name="remInstance"></param>
        public void RemoveCompositor(CompositorInstance remInstance)
        {
            CompositorChain chain = remInstance.Chain;

            for (int i = 0; i < chain.Instances.Count; i++)
            {
                CompositorInstance instance = chain.GetCompositor(i);
                if (instance == remInstance)
                {
                    chain.RemoveCompositor(i);
                    break;
                }
            }
        }

		///<summary>
		///    Set the state of a compositor on a viewport to enabled or disabled.
		///    Disabling a compositor stops it from rendering but does not free any resources.
		///    This can be more efficient than using removeCompositor and addCompositor in cases
		///    the filter is switched on and off a lot.
		///</summary>
		public void SetCompositorEnabled(Viewport vp, string compositor, bool value) {
			CompositorChain chain = GetCompositorChain(vp);
			for (int i=0; i< chain.Instances.Count; i++) {
				CompositorInstance instance = chain.GetCompositor(i);
				if (instance.Compositor.Name == compositor) {
					chain.SetCompositorEnabled(i, value);
					break;
				}
			}
        }

        #endregion Methods

    }

	/// <summary>
	///		Enum to identify compositor sections.
	/// </summary>
	public enum CompositorScriptSection 
	{
		None,
		Compositor,
		Technique,
		Target,
		Pass,
		Clear,
		Stencil
	}

	/// <summary>
	///		Struct for holding the script context while parsing.
	/// </summary>
	public class CompositorScriptContext {
		public CompositorScriptSection section = CompositorScriptSection.None;
		public Compositor compositor = null;
		public CompositionTechnique technique = null;
		public CompositionPass pass = null;
		public CompositionTargetPass target = null;
		public bool seenOpen = false;
		// Error reporting state
		public int lineNo;
		public string line;
		public string filename;
	}

}
