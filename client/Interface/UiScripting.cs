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

#region Using directives

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.IO;

using IronPython.Hosting;
using Multiverse.Lib.LogUtil;
using Multiverse.Gui;

#endregion

namespace Multiverse.Interface
{
	public interface ILogger {
		void Write(string message);
	}

	public class UiScripting {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(UiScripting));
		protected static string defaultPath = null;
		public static PythonEngine interpreter = null;
        protected static Dictionary<string, EngineModule> modules;

        /// <summary>
        ///   Initialize the python interpreter, but do not reflect the assemblies yet
        /// </summary>
		public static void SetupInterpreter() {
            modules = new Dictionary<string, EngineModule>();
			// interpreter.Reset();
			interpreter = new IronPython.Hosting.PythonEngine();
        }

        /// <summary>
        ///   Reflect the calling assembly and all referenced assemblies into Python
        /// </summary>
        public static void LoadCallingAssembly() {
			Assembly callingAssembly = Assembly.GetCallingAssembly();
            LoadAssembly(callingAssembly);
        }
        public static void LoadAssembly(Assembly assembly)
        {
            log.InfoFormat("assembly.FullName: {0}", assembly.FullName);
            interpreter.LoadAssembly(assembly);
			foreach (AssemblyName name in assembly.GetReferencedAssemblies()) {
				Assembly refAssembly = Assembly.Load(name);
                log.InfoFormat("refAssembly.FullName: {0}", refAssembly.FullName);
				interpreter.LoadAssembly(refAssembly);
			}
		}

        public static TDelegate SetupDelegate<TDelegate>(string code, IList<string> args) {
            try {
                return interpreter.CreateMethod<TDelegate>(code, args);
            } catch (Exception) {
                log.WarnFormat("Failed to compile delegate code:\n{0}", code);
                throw;
            }
        }

        public static TDelegate SetupDelegate<TDelegate>(string methodName) {
            try {
                return interpreter.EvaluateAs<TDelegate>(methodName);
            } catch (Exception) {
                log.WarnFormat("Failed to evaluate delegate for method:\n{0}", methodName);
                throw;
            }
        }
        /// <summary>
        ///   Run the code in the __main__ module of the interpreter.
        /// </summary>
        /// <param name="code">the code to run</param>
        /// <returns>true if the script was run successfully</returns>
        public static void RunScript(string code) {
            if (code == null)
                return;
            try {
                interpreter.Execute(code);
            } catch (Exception ex) {
                LogUtil.ExceptionLog.InfoFormat("Exception when running script code: \n{0}", code);
                LogUtil.ExceptionLog.WarnFormat("Python Stack Trace: {0}", interpreter.FormatException(ex));
                LogUtil.ExceptionLog.WarnFormat("Full Stack Trace: {0}", ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Run the code in the __main__ module of the interpreter.
        /// </summary>
        /// <param name="code">the code to run</param>
        /// <param name="locals">a dictionary of local variables that will be accessible to the script</param>
        public static void RunScript(string code, IDictionary<string, object> locals)
        {
            if (code == null)
                return;
            try
            {
                interpreter.Execute(code, interpreter.DefaultModule, locals);
            }
            catch (Exception ex)
            {
                LogUtil.ExceptionLog.InfoFormat("Exception when running script code: \n{0}", code);
                LogUtil.ExceptionLog.WarnFormat("Python Stack Trace: {0}", interpreter.FormatException(ex));
                LogUtil.ExceptionLog.WarnFormat("Full Stack Trace: {0}", ex.ToString());
                throw;
            }
        }

        /// <summary>
        ///   Compile the code in the __main__ module of the interpreter.
        /// </summary>
        /// <param name="code">the code to compile</param>
        /// <returns>the object that was compiled</returns>
        public static object CompileScript(string code) {
            object rv;
            if (code == null)
                return null;
            try {
                rv = interpreter.Compile(code);
            } catch (Exception ex) {
                LogUtil.ExceptionLog.InfoFormat("Failed to compile scripting code: \n{0}", code);
                LogUtil.ExceptionLog.Warn(ex.ToString());
                return null;
            }
            return rv;
        }

        /// <summary>
        ///   Run a script file from some non-standard location.
        /// </summary>
        /// <param name="path">the directory where the script file is located</param>
        /// <param name="file">the name of the script file</param>
        /// <param name="module">the name of the module where the script will
        ///                      be run, or null to run in __main__</param>
        /// <returns>true if the script was run successfully</returns>
		public static bool RunFile(string path, string file, string module) {
			return RunFileHelper(ResolvePath(path) + file, module, false);
		}

        /// <summary>
        ///   Run a script file from some non-standard location.
        /// </summary>
        /// <param name="path">the directory where the script file is located</param>
        /// <param name="file">the name of the script file</param>
        /// <param name="module">the name of the module where the script will
        ///                      be run, or null to run in __main__</param>
        /// <param name="publish">whether to publish the module</param>
        /// <returns>true if the script was run successfully</returns>
        public static bool RunFile(string path, string file, string module, bool publish)
        {
            return RunFileHelper(ResolvePath(path) + file, module, publish);
        }

        /// <summary>
        ///    Run a script file from the collection of "Script" assets.
        ///    If the module argument is null, these will be run in the 
        ///    __main__ module.  Otherwise, they will be run in a new module 
        ///    using the name provided.
        /// </summary>
        /// <param name="file">the name of the script file</param>
        /// <param name="module">the name of the module where the script will
        ///                      be run, or null to run in __main__</param>
        /// <returns>true if the script was run successfully</returns>
        public static bool RunFile(string file, string module) {
			string fullFile =
				AssetManager.Instance.ResolveResourceData("Script", file);
            if (fullFile == null) { // didn't find the file
                log.WarnFormat("Unable to resolve script file: {0}", file);
                return false;
            }
			return RunFileHelper(fullFile, module, false);
		}

        /// <summary>
        ///   Run a script file in the __main__ module.
        /// </summary>
        /// <param name="file">the name of the script file</param>
        /// <returns>true if the script was run successfully</returns>
        public static bool RunFile(string file) {
            return RunFile(file, null);
        }

        /// <summary>
        ///   Run the script file in the given module.  If module is null,
        ///   run the script in the default context (__main__).
        /// </summary>
        /// <param name="scriptFile">the name of the script file</param>
        /// <param name="module">the name of the module where the script will
        ///                      be run, or null to run in __main__</param>
        /// <returns>true if the script was run successfully</returns>
		private static bool RunFileHelper(string scriptFile, string module, bool publish) {
			if (!scriptFile.EndsWith(".py"))
				return false;

            try {
                if (module == null || module == "") {
                    log.InfoFormat("Executing script file '{0}'", scriptFile);
                    interpreter.ExecuteFile(scriptFile);
                } else {
                    log.InfoFormat("Running script file '{0}' in module '{1}'", scriptFile, module);
                    EngineModule mod = interpreter.CreateModule(module, publish);
                    // save the module so we can use it later
                    modules[module] = mod;
                    interpreter.ExecuteFile(scriptFile, mod);
                }
                log.InfoFormat("Ran script file '{0}' in module '{1}'", scriptFile, module);
                return true;
            } catch (IronPython.Runtime.Exceptions.PythonSyntaxErrorException ex) {
                LogUtil.ExceptionLog.ErrorFormat("Failed to run script file '{0}' in module '{1}'", scriptFile, module);
                LogUtil.ExceptionLog.ErrorFormat("PythonSyntaxErrorException at {0}:{1}: {2}", ex.FileName, ex.Line, ex.Message);
                // log.Warn(ex.ToString());
                return false;
            } catch (Exception ex) {
               // interpreter.DumpException(ex);
                LogUtil.ExceptionLog.ErrorFormat("Exception when running script file '{0}' in module '{1}'", scriptFile, module);
                LogUtil.ExceptionLog.ErrorFormat("Python Stack Trace: {0}", interpreter.FormatException(ex));
                LogUtil.ExceptionLog.ErrorFormat("Full Stack Trace: {0}", ex.ToString());
                return false;
            // } finally {
                // interpreter.DumpDebugInfo();
            }
		}

        public static bool RunModule(string path, string file, string module, bool publish, Dictionary<string, object> globals)
        {
            string scriptFile = ResolvePath(path) + file;

            if (!scriptFile.EndsWith(".py"))
                return false;

            try
            {
                log.InfoFormat("Running module file '{0}' in module '{1}'", scriptFile, module);
                EngineModule mod = interpreter.CreateModule(module, globals, publish);
                // save the module so we can use it later
                modules[module] = mod;
                interpreter.ExecuteFile(scriptFile, mod);

                log.InfoFormat("Ran script file '{0}' in module '{1}'", scriptFile, module);
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.ExceptionLog.WarnFormat("Exception when running script file '{0}' in module '{1}'", scriptFile, module);
                LogUtil.ExceptionLog.WarnFormat("Python Stack Trace: {0}", interpreter.FormatException(ex));
                LogUtil.ExceptionLog.WarnFormat("Full Stack Trace: {0}", ex.ToString());
                return false;
            }
        }

        /// <summary>
        ///   Sets a variable in the default module (__main__)
        /// </summary>
        /// <param name="name">the name of the variable</param>
        /// <param name="val">the value of the variable</param>
        public static void SetVariable(string name, object val) {
            interpreter.Globals[name] = val;
        }

        public static void SetVariable(string moduleName, string name, object val)
        {
            //if (interpreter.Sys.modules.ContainsKey(moduleName))
            {
                EngineModule module = modules[moduleName];

                module.Globals[name] = val;
            }
        }
        
        /// <summary>
        ///   Gets a variable from the default module (__main__)
        /// </summary>
        /// <param name="name">the name of the variable</param>
        public static object GetVariable(string name) {
            return interpreter.Globals[name];
        }

        /// <summary>
        ///   Convert a path string into a more full path name, resolving 
        ///   portions like "../" and "./" based on the current directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
		private static string ResolvePath(string path) {
			if (path.StartsWith("/"))
				return path;
			else if (path.IndexOf(':') == 1)
				// Something like "C:\blah\blah"
				return path;
			string tmpPath = Environment.CurrentDirectory;
			while (path.StartsWith("../") || path.StartsWith("./")) {
				if (path.StartsWith("../")) {
					int lastSlash = tmpPath.LastIndexOf('\\');
					if (lastSlash != -1)
						tmpPath = tmpPath.Substring(0, lastSlash);
					path = path.Substring(3);
				} else {
					path = path.Substring(2);
				}
			}
			path = tmpPath + "/" + path;
			return path;
		}

        /// <summary>
        ///   Add the given path to the list of places to search for scripts 
        /// </summary>
        /// <param name="path"></param>
		public static void AddPath(string path) {
			interpreter.AddToPath(ResolvePath(path));
		}

        /// <summary>
        /// Format an exception for printing to the log file.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string FormatException(Exception ex)
        {
            return interpreter.FormatException(ex);
        }
	}
}
