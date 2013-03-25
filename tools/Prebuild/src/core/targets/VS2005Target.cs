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
Copyright (c) 2004 Matthew Holmes (matthew@wildfiregames.com)

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

using DNPreBuild.Core.Attributes;
using DNPreBuild.Core.Interfaces;
using DNPreBuild.Core.Nodes;
using DNPreBuild.Core.Util;

namespace DNPreBuild.Core.Targets
{
	[Target("vs2005")]
	public class VS2005Target : VS2003Target 
	{

		#region Private Methods

		private void SetVS2005() {
			m_SolutionVersion = "9.00";
			m_ProductVersion = "8.0.50215.44";
			m_SchemaVersion = "2.0";
			m_VersionName = "2005";
			m_Version = VSVersion.VS80;
		}

		#endregion

		protected override void WriteProject(SolutionNode solution, ProjectNode project) {
			if (!m_Tools.ContainsKey(project.Language))
				throw new Exception("Unknown .NET language: " + project.Language);

			ToolInfo toolInfo = (ToolInfo)m_Tools[project.Language];
			string projectFile = Helper.MakeFilePath(project.FullPath, project.Name, toolInfo.FileExtension);
			StreamWriter ps = new StreamWriter(projectFile);

			m_Kernel.CWDStack.Push();
			Helper.SetCurrentDir(Path.GetDirectoryName(projectFile));

			using (ps) {
				ps.WriteLine("<Project DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
				// ps.WriteLine("\t<{0}", toolInfo.XMLTag);
				ps.WriteLine("\t<PropertyGroup>");
				ps.WriteLine("\t\t<ProjectType>Local</ProjectType>");
				ps.WriteLine("\t\t<ProductVersion>{0}</ProductVersion>", m_ProductVersion);
				ps.WriteLine("\t\t<SchemaVersion>{0}</SchemaVersion>", m_SchemaVersion);
				ps.WriteLine("\t\t<ProjectGuid>{{{0}}}</ProjectGuid>", project.Guid.ToString().ToUpper());

				ps.WriteLine("\t\t<Configuration Condition = \" '$(Configuration)' == '' \">Debug</Configuration>");
				ps.WriteLine("\t\t<Platform Condition = \" '$(Platform)' == '' \">AnyCPU</Platform>");
				// ps.WriteLine("\t\t<Build>");

				// ps.WriteLine("\t\t\t<Settings");
				ps.WriteLine("\t\t<ApplicationIcon></ApplicationIcon>");
				ps.WriteLine("\t\t<AssemblyKeyContainerName></AssemblyKeyContainerName>");
				ps.WriteLine("\t\t<AssemblyName>{0}</AssemblyName>", project.AssemblyName);
				ps.WriteLine("\t\t<AssemblyOriginatorKeyFile></AssemblyOriginatorKeyFile>");
				ps.WriteLine("\t\t<DefaultClientScript>JScript</DefaultClientScript>");
				ps.WriteLine("\t\t<DefaultHTMLPageLayout>Grid</DefaultHTMLPageLayout>");
				ps.WriteLine("\t\t<DefaultTargetSchema>IE50</DefaultTargetSchema>");
				ps.WriteLine("\t\t<DelaySign>false</DelaySign>");

				// if(m_Version == VSVersion.VS70)
				//     ps.WriteLine("\t\t\t\tNoStandardLibraries = \"false\"");

				ps.WriteLine("\t\t<OutputType>{0}</OutputType>", project.Type.ToString());
				ps.WriteLine("\t\t<RootNamespace>{0}</RootNamespace>", project.RootNamespace);
				ps.WriteLine("\t\t<StartupObject>{0}</StartupObject>", project.StartupObject);
				// ps.WriteLine("\t\t\t>");
				ps.WriteLine("\t\t<FileUpgradeFlags></FileUpgradeFlags>");

				ps.WriteLine("\t</PropertyGroup>");

				foreach (ConfigurationNode conf in project.Configurations) {
					ps.Write("\t<PropertyGroup ");
					ps.WriteLine("Condition=\" '$(Configuration)|$(Platform)' == '{0}|AnyCPU' \">", conf.Name);
					ps.WriteLine("\t\t<AllowUnsafeBlocks>{0}</AllowUnsafeBlocks>", conf.Options["AllowUnsafe"]);
					ps.WriteLine("\t\t<BaseAddress>{0}</BaseAddress>", conf.Options["BaseAddress"]);
					ps.WriteLine("\t\t<CheckForOverflowUnderflow>{0}</CheckForOverflowUnderflow>", conf.Options["CheckUnderflowOverflow"]);
					ps.WriteLine("\t\t<ConfigurationOverrideFile></ConfigurationOverrideFile>");
					ps.WriteLine("\t\t<DefineConstants>{0}</DefineConstants>", conf.Options["CompilerDefines"]);
					ps.WriteLine("\t\t<DocumentationFile>{0}</DocumentationFile>", conf.Options["XmlDocFile"]);
					ps.WriteLine("\t\t<DebugSymbols>{0}</DebugSymbols>", conf.Options["DebugInformation"]);
					ps.WriteLine("\t\t<FileAlignment>{0}</FileAlignment>", conf.Options["FileAlignment"]);
					// ps.WriteLine("\t\t<IncrementalBuild = \"{0}\"", conf.Options["IncrementalBuild"]);

					// if(m_Version == VSVersion.VS71)
					// {
					//     ps.WriteLine("\t\t\t\t\tNoStdLib = \"{0}\"", conf.Options["NoStdLib"]);
					//     ps.WriteLine("\t\t\t\t\tNoWarn = \"{0}\"", conf.Options["SupressWarnings"]);
					// }

					ps.WriteLine("\t\t<Optimize>{0}</Optimize>", conf.Options["OptimizeCode"]);
					ps.WriteLine("\t\t<OutputPath>{0}</OutputPath>",
						Helper.EndPath(Helper.NormalizePath(conf.Options["OutputPath"].ToString())));
					ps.WriteLine("\t\t<RegisterForComInterop>{0}</RegisterForComInterop>", conf.Options["RegisterCOMInterop"]);
					ps.WriteLine("\t\t<RemoveIntegerChecks>{0}</RemoveIntegerChecks>", conf.Options["RemoveIntegerChecks"]);
					ps.WriteLine("\t\t<TreatWarningsAsErrors>{0}</TreatWarningsAsErrors>", conf.Options["WarningsAsErrors"]);
					ps.WriteLine("\t\t<WarningLevel>{0}</WarningLevel>", conf.Options["WarningLevel"]);
					ps.WriteLine("\t</PropertyGroup>");
				}

				// ps.WriteLine("\t\t\t</Settings>");

				ps.WriteLine("\t<ItemGroup>");
				foreach (ReferenceNode refr in project.References) {
					if (solution.ProjectsTable.ContainsKey(refr.Name)) {
						ProjectNode refProject = (ProjectNode)solution.ProjectsTable[refr.Name];
						ps.Write("\t\t<ProjectReference");
						ps.WriteLine(" Include=\"{0}\">", refProject.Path);
						ps.WriteLine("\t\t\t<Project>{{{0}}}</Project>", refProject.Guid.ToString().ToUpper());
						ps.WriteLine("\t\t\t<Package>{0}</Package>", toolInfo.Guid.ToString().ToUpper());
						ps.WriteLine("\t\t\t<Name>{0}</Name>", refr.Name);
						if (refr.LocalCopy)
							ps.WriteLine("\t\t\t<Private>{0}</Private>", refr.LocalCopy);
						ps.WriteLine("\t\t</ProjectReference>");
					} else {
						ps.Write("\t\t<Reference");
						ps.WriteLine(" Include=\"{0}\">", refr.Name);
						ps.WriteLine("\t\t\t<Name>{0}</Name>", refr.Name);
						if (refr.Path != null)
							ps.WriteLine("\t\t\t<HintPath>{0}</HintPath>", Helper.MakeFilePath(refr.Path, refr.Name, "dll"));
						if (refr.LocalCopy)
							ps.WriteLine("\t\t\t<Private>{0}</Private>", refr.LocalCopy);
						ps.WriteLine("\t\t</Reference>");
					}
					// ps.WriteLine("\t\t\t\t/>");
				}
				ps.WriteLine("\t</ItemGroup>");

				// ps.WriteLine("\t\t</Build>");
				ps.WriteLine("\t<ItemGroup>");

				// ps.WriteLine("\t\t\t<Include>");
				foreach (string file in project.Files) {
					ps.Write("\t\t<{0} ", project.Files.GetBuildAction(file));
					ps.WriteLine(" Include =\"{0}\">", file.Replace(".\\", ""));
					ps.WriteLine("\t\t\t<SubType>Code</SubType>");
					ps.WriteLine("\t\t</{0}>", project.Files.GetBuildAction(file));

					// ps.WriteLine("\t\t\t\t<File");
					// ps.WriteLine("\t\t\t\t\tRelPath = \"{0}\"", file.Replace(".\\", ""));
					// ps.WriteLine("\t\t\t\t\tSubType = \"Code\"");
					// ps.WriteLine("\t\t\t\t\tBuildAction = \"{0}\"", project.Files.GetBuildAction(file));
					// ps.WriteLine("\t\t\t\t/>");
				}
				// ps.WriteLine("\t\t\t</Include>");

				ps.WriteLine("\t</ItemGroup>");
				ps.WriteLine("\t<Import Project=\"$(MSBuildBinPath)\\Microsoft.CSHARP.Targets\" />");
				ps.WriteLine("\t<PropertyGroup>");
				ps.WriteLine("\t\t<PreBuildEvent>");
				ps.WriteLine("\t\t</PreBuildEvent>");
				ps.WriteLine("\t\t<PostBuildEvent>");
				ps.WriteLine("\t\t</PostBuildEvent>");
				ps.WriteLine("\t</PropertyGroup>");
				// ps.WriteLine("\t</{0}>", toolInfo.XMLTag);
				ps.WriteLine("</Project>");
			}

			ps = new StreamWriter(projectFile + ".user");
			using (ps) {
				ps.WriteLine("<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
				// ps.WriteLine( "<VisualStudioProject>" );
				// ps.WriteLine("\t<{0}>", toolInfo.XMLTag);
				// ps.WriteLine("\t\t<Build>");
				ps.WriteLine("\t<PropertyGroup>");
				// ps.WriteLine("\t\t\t<Settings ReferencePath=\"{0}\">", MakeRefPath(project));
				ps.WriteLine("\t\t<Configuration Condition=\" '$(Configuration)' == '' \">Debug</Configuration>");
				ps.WriteLine("\t\t<Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform>");
				ps.WriteLine("\t\t<ReferencePath>{0}</ReferencePath>", MakeRefPath(project));
				ps.WriteLine("\t\t<LastOpenVersion>{0}</LastOpenVersion>", m_ProductVersion);
				ps.WriteLine("\t\t<ProjectView>ProjectFiles</ProjectView>");
				ps.WriteLine("\t\t<ProjectTrust>0</ProjectTrust>");
				ps.WriteLine("\t</PropertyGroup>");
				foreach (ConfigurationNode conf in project.Configurations) {
					ps.Write("\t<PropertyGroup");
					ps.Write(" Condition = \" '$(Configuration)|$(Platform)' == '{0}|AnyCPU' \"", conf.Name);
					ps.WriteLine(" />");
				}
				// ps.WriteLine("\t\t\t</Settings>");

				// ps.WriteLine("\t\t</Build>");
				// ps.WriteLine("\t</{0}>", toolInfo.XMLTag);
				// ps.WriteLine("</VisualStudioProject>");
				ps.WriteLine("</Project>");
			}

			m_Kernel.CWDStack.Pop();
		}

		protected override void WriteSolution(SolutionNode solution)
		{
			m_Kernel.Log.Write("Creating Visual Studio {0} solution and project files", m_VersionName);

			foreach(ProjectNode project in solution.Projects)
			{
				m_Kernel.Log.Write("...Creating project: {0}", project.Name);
				WriteProject(solution, project);
			}

			m_Kernel.Log.Write("");
			string solutionFile = Helper.MakeFilePath(solution.FullPath, solution.Name, "sln");
			StreamWriter ss = new StreamWriter(solutionFile);

			m_Kernel.CWDStack.Push();
			Helper.SetCurrentDir(Path.GetDirectoryName(solutionFile));
            
			using(ss)
			{
				ss.WriteLine("Microsoft Visual Studio Solution File, Format Version {0}", m_SolutionVersion);
				foreach(ProjectNode project in solution.Projects)
				{
					if(!m_Tools.ContainsKey(project.Language))
						throw new Exception("Unknown .NET language: " + project.Language);

					ToolInfo toolInfo = (ToolInfo)m_Tools[project.Language];
                
					string path = Helper.MakePathRelativeTo(solution.FullPath, project.FullPath);
					ss.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{{{3}}}\"",
						toolInfo.Guid, project.Name, Helper.MakeFilePath(path, project.Name,
						toolInfo.FileExtension), project.Guid.ToString().ToUpper());

					// ss.WriteLine("\tProjectSection(ProjectDependencies) = postProject");
					// ss.WriteLine("\tEndProjectSection");

					ss.WriteLine("EndProject");
				}

				ss.WriteLine("Global");

				// VS2005 uses SolutionConfigurationPlatforms instead of SolutionConfiguration
				// This also means that the config name includes the "|Any CPU" part.
				ss.WriteLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
				foreach (ConfigurationNode conf in solution.Configurations)
					ss.WriteLine("\t\t{0}|Any CPU = {0}|Any CPU", conf.Name);
				ss.WriteLine("\tEndGlobalSection");

				// VS2005 doesn't want this line if there are no projects
				if (solution.Projects.Count > 1) {
					ss.WriteLine("\tGlobalSection(ProjectDependencies) = postSolution");
				}
				foreach (ProjectNode project in solution.Projects) {
					for (int i = 0; i < project.References.Count; i++) {
						ReferenceNode refr = (ReferenceNode)project.References[i];
						if (solution.ProjectsTable.ContainsKey(refr.Name)) {
							ProjectNode refProject = (ProjectNode)solution.ProjectsTable[refr.Name];
							ss.WriteLine("\t\t({{{0}}}).{1} = ({{{2}}})",
								project.Guid.ToString().ToUpper()
								, i,
								refProject.Guid.ToString().ToUpper()
							);
						}
					}
				}
				// VS2005 doesn't want this line if there are no projects
				if (solution.Projects.Count > 1) {
					ss.WriteLine("\tEndGlobalSection");
				}

				// Again VS2005 wants to include the platform, so we use
				// ProjectConfigurationPlatforms instead of ProjectConfiguration and
				// we add the "|Any CPU" to the config lines.
				ss.WriteLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
				foreach (ProjectNode project in solution.Projects) {
					foreach (ConfigurationNode conf in solution.Configurations) {
						ss.WriteLine("\t\t{{{0}}}.{1}|Any CPU.ActiveCfg = {1}|Any CPU",
							project.Guid.ToString().ToUpper(),
							conf.Name);

						ss.WriteLine("\t\t{{{0}}}.{1}|Any CPU.Build.0 = {1}|Any CPU",
							project.Guid.ToString().ToUpper(),
							conf.Name);
					}
				}
				ss.WriteLine("\tEndGlobalSection");

				// VS2005 has SolutionProperties
				ss.WriteLine("\tGlobalSection(SolutionProperties) = preSolution");
				ss.WriteLine("\t\tHideSolutionNode = FALSE");
				ss.WriteLine("\tEndGlobalSection");

				// VS2005 does not have SolutionItems
				// if(solution.Files != null)
				// {
				//     ss.WriteLine("\tGlobalSection(SolutionItems) = postSolution");
				//     foreach(string file in solution.Files)
				//         ss.WriteLine("\t\t{0} = {0}", file);
				//     ss.WriteLine("\tEndGlobalSection");
				// }

				ss.WriteLine("EndGlobal");
			}

			m_Kernel.CWDStack.Pop();
}
		#region ITarget Members

				public override void Write(Kernel kern)
		{
			SetVS2005();
			base.Write(kern);
		}

		public override void Clean(Kernel kern)
		{
			SetVS2005();
			base.Clean(kern);
		}

		public override string Name
		{
			get
			{
				return "vs2005";
			}
		}

		#endregion
	}
}
