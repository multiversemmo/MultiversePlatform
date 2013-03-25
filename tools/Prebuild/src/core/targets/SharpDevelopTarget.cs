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

#region BSD License
/*
Copyright (c) 2004 Matthew Holmes (matthew@wildfiregames.com), Dan Moorehead (dan05a@gmail.com)

Redistribution and use in source and binary forms, with or without modification, are permitted
provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions 
  and the following disclaimer. 
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions 
  and the following disclaimer in the documentation and/or other materials provided with the 
  distribution. 
* The name of the author may not be used to endorse or promote products derived from this software 
  without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

#region CVS Information
/*
 * $Source$
 * $Author: mccollum $
 * $Date: 2006-09-08 17:12:07 -0700 (Fri, 08 Sep 2006) $
 * $Revision: 6434 $
 */
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;

using DNPreBuild.Core.Attributes;
using DNPreBuild.Core.Interfaces;
using DNPreBuild.Core.Nodes;
using DNPreBuild.Core.Util;

namespace DNPreBuild.Core.Targets
{
	[Target("sharpdev")]
	public class SharpDevelopTarget : ITarget
	{
		#region Fields

		private Kernel m_Kernel = null;

		#endregion

		#region Private Methods

		private string PrependPath(string path)
		{
			string tmpPath = Helper.NormalizePath(path, '/');
			Regex regex = new Regex(@"(\w):/(\w+)");
			Match match = regex.Match(tmpPath);
			if(match.Success || tmpPath[0] == '.' || tmpPath[0] == '/')
				tmpPath = Helper.NormalizePath(tmpPath);
			else
				tmpPath = Helper.NormalizePath("./" + tmpPath);

			return tmpPath;
		}

		private string BuildReference(SolutionNode solution, ReferenceNode refr)
		{
			string ret = "\t\t<Reference type=\"";
			if(solution.ProjectsTable.ContainsKey(refr.Name))
			{
				ret += "Project\" refto=\"" + refr.Name;
				ret += "\" localcopy=\"" + refr.LocalCopy.ToString() + "\" />";
			}
			else
			{
				ProjectNode project = (ProjectNode)refr.Parent;
				string fileRef = FindFileReference(refr.Name, project);

				if(refr.Path != null || fileRef != null)
				{
					ret += "Assembly\" refto=\"";

					string finalPath = (refr.Path != null) ? Helper.MakeFilePath(refr.Path, refr.Name, "dll") : fileRef;

					ret += finalPath;
					ret += "\" localcopy=\"" + refr.LocalCopy.ToString() + "\" />";
					return ret;
				}

				ret += "Gac\" refto=\"";
				ret += refr.Name;
				ret += "\" localcopy=\"" + refr.LocalCopy.ToString() + "\" />";
			}

			return ret;
		}

		private string FindFileReference(string refName, ProjectNode project) 
		{
			foreach(ReferencePathNode refPath in project.ReferencePaths) 
			{
				string fullPath = Helper.MakeFilePath(refPath.Path, refName, "dll");

				if(File.Exists(fullPath)) 
				{
					return fullPath;
				}
			}

			return null;
		}

		private void WriteProject(SolutionNode solution, ProjectNode project)
		{
			string csComp = "Csc";
			string netRuntime = "MsNet";
			if(project.Runtime == Runtime.Mono)
			{
				csComp = "Mcs";
				netRuntime = "Mono";
			}

			string projFile = Helper.MakeFilePath(project.FullPath, project.Name, "prjx");
			StreamWriter ss = new StreamWriter(projFile);

			m_Kernel.CWDStack.Push();
			Helper.SetCurrentDir(Path.GetDirectoryName(projFile));

			using(ss)
			{
				ss.WriteLine(
					"<Project name=\"{0}\" description=\"\" standardNamespace=\"{1}\" newfilesearch=\"None\" enableviewstate=\"True\" version=\"1.1\" projecttype=\"C#\">",
					project.Name,
					project.RootNamespace
					);

				ss.WriteLine("\t<Contents>");
				foreach(string file in project.Files)
				{
					string buildAction = "Compile";
					switch(project.Files.GetBuildAction(file))
					{
						case BuildAction.None:
							buildAction = "Nothing";
							break;

						case BuildAction.Content:
							buildAction = "Exclude";
							break;

						case BuildAction.EmbeddedResource:
							buildAction = "EmbedAsResource";
							break;

						default:
							buildAction = "Compile";
							break;
					}

					// Sort of a hack, we try and resolve the path and make it relative, if we can.
					string filePath = PrependPath(file);
					ss.WriteLine("\t\t<File name=\"{0}\" subtype=\"Code\" buildaction=\"{1}\" dependson=\"\" data=\"\" />", filePath, buildAction);
				}
				ss.WriteLine("\t</Contents>");

				ss.WriteLine("\t<References>");
				foreach(ReferenceNode refr in project.References)
					ss.WriteLine("\t\t{0}", BuildReference(solution, refr));
				ss.WriteLine("\t</References>");

				int count = 0;
				foreach(ConfigurationNode conf in solution.Configurations)
				{
					if(count == 0)
						ss.WriteLine("\t<Configurations active=\"{0}\">", conf.Name);

					ss.WriteLine("\t\t<Configuration name=\"{0}\">", conf.Name);
					ss.WriteLine("\t\t\t<CodeGeneration");
					ss.WriteLine("\t\t\t\truntime=\"{0}\"", netRuntime);
					ss.WriteLine("\t\t\t\tcompiler=\"{0}\"", csComp);
					ss.WriteLine("\t\t\t\twarninglevel=\"{0}\"", conf.Options["WarningLevel"]);
					ss.WriteLine("\t\t\t\tincludedebuginformation=\"{0}\"", conf.Options["DebugInformation"]);
					ss.WriteLine("\t\t\t\toptimize=\"{0}\"", conf.Options["OptimizeCode"]);
					ss.WriteLine("\t\t\t\tunsafecodeallowed=\"{0}\"", conf.Options["AllowUnsafe"]);
					ss.WriteLine("\t\t\t\tgenerateoverflowchecks=\"{0}\"", conf.Options["CheckUnderflowOverflow"]);
					ss.WriteLine("\t\t\t\tmainclass=\"{0}\"", project.StartupObject);
					ss.WriteLine("\t\t\t\ttarget=\"{0}\"", project.Type);
					ss.WriteLine("\t\t\t\tdefinesymbols=\"{0}\"", conf.Options["CompilerDefines"]);
					ss.WriteLine("\t\t\t\tgeneratexmldocumentation=\"{0}\"", (((string)conf.Options["XmlDocFile"]).Length > 0));
					ss.WriteLine("\t\t\t/>");

					ss.WriteLine("\t\t\t<Output");
					ss.WriteLine("\t\t\t\tdirectory=\".\\{0}\"", conf.Options["OutputPath"]);
					ss.WriteLine("\t\t\t\tassembly=\"{0}\"", project.AssemblyName);
					ss.WriteLine("\t\t\t\texecuteScript=\"\"");
					ss.WriteLine("\t\t\t\texecuteBeforeBuild=\"\"");
					ss.WriteLine("\t\t\t\texecuteAfterBuild=\"\"");
					ss.WriteLine("\t\t\t/>");
					ss.WriteLine("\t\t</Configuration>");

					count++;
				}                
				ss.WriteLine("\t</Configurations>");
				ss.WriteLine("</Project>");
			}

			m_Kernel.CWDStack.Pop();
		}

		private void WriteCombine(SolutionNode solution)
		{
			m_Kernel.Log.Write("Creating SharpDevelop combine and project files");
			foreach(ProjectNode project in solution.Projects)
			{
				m_Kernel.Log.Write("...Creating project: {0}", project.Name);
				WriteProject(solution, project);
			}

			m_Kernel.Log.Write("");
			string combFile = Helper.MakeFilePath(solution.FullPath, solution.Name, "cmbx");
			StreamWriter ss = new StreamWriter(combFile);

			m_Kernel.CWDStack.Push();
			Helper.SetCurrentDir(Path.GetDirectoryName(combFile));
            
			using(ss)
			{
				ss.WriteLine("<Combine fileversion=\"1.0\" name=\"{0}\" description=\"\">", solution.Name);
                
				int count = 0;
				foreach(ProjectNode project in solution.Projects)
				{                    
					if(count == 0)
						ss.WriteLine("\t<StartMode startupentry=\"{0}\" single=\"True\">", project.Name);

					ss.WriteLine("\t\t<Execute entry=\"{0}\" type=\"None\" />", project.Name);
					count++;
				}
				ss.WriteLine("\t</StartMode>");
                
				ss.WriteLine("\t<Entries>");
				foreach(ProjectNode project in solution.Projects)
				{
					string path = Helper.MakePathRelativeTo(solution.FullPath, project.FullPath);
					ss.WriteLine("\t\t<Entry filename=\"{0}\" />",
						Helper.MakeFilePath(path, project.Name, "prjx"));
				}
				ss.WriteLine("\t</Entries>");

				count = 0;
				foreach(ConfigurationNode conf in solution.Configurations)
				{
					if(count == 0)
						ss.WriteLine("\t<Configurations active=\"{0}\">", conf.Name);

					ss.WriteLine("\t\t<Configuration name=\"{0}\">", conf.Name);
					foreach(ProjectNode project in solution.Projects)
						ss.WriteLine("\t\t\t<Entry name=\"{0}\" configurationname=\"{1}\" build=\"True\"/>", project.Name, conf.Name);
					ss.WriteLine("\t\t</Configuration>");

					count++;
				}
				ss.WriteLine("\t</Configurations>");
				ss.WriteLine("</Combine>");
			}

			m_Kernel.CWDStack.Pop();
		}

		private void CleanProject(ProjectNode project)
		{
			m_Kernel.Log.Write("...Cleaning project: {0}", project.Name);
			string projectFile = Helper.MakeFilePath(project.FullPath, project.Name, "prjx");
			Helper.DeleteIfExists(projectFile);
		}

		private void CleanSolution(SolutionNode solution)
		{
			m_Kernel.Log.Write("Cleaning SharpDevelop combine and project files for", solution.Name);

			string slnFile = Helper.MakeFilePath(solution.FullPath, solution.Name, "cmbx");
			Helper.DeleteIfExists(slnFile);

			foreach(ProjectNode project in solution.Projects)
				CleanProject(project);
            
			m_Kernel.Log.Write("");
		}

		#endregion

		#region ITarget Members

		public void Write(Kernel kern)
		{
			m_Kernel = kern;
			foreach(SolutionNode solution in kern.Solutions)
				WriteCombine(solution);
			m_Kernel = null;
		}

		public virtual void Clean(Kernel kern)
		{
			m_Kernel = kern;
			foreach(SolutionNode sol in kern.Solutions)
				CleanSolution(sol);
			m_Kernel = null;
		}

		public string Name
		{
			get
			{
				return "sharpdev";
			}
		}

		#endregion
	}
}
