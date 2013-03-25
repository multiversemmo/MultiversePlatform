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
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Specialized;

using Axiom.Core;
using Axiom.Graphics;
using Axiom.Scripting;

namespace Axiom.SceneManagers.Bsp
{
	/// <summary>
	///		Class for managing Quake3 custom shaders.
	///	</summary>
	///	<remarks>
	///		Quake3 uses .shader files to define custom shaders which are the rough equivelent of OGRE material scripts.
    ///		When a surface texture is mentioned in a level file, it includes no file extension
	///		meaning that it can either be a standard texture image (+lightmap) if there is only a .jpg or .tga
    ///		file, or it may refer to a custom shader if a shader with that name is included in one of the .shader
    ///		files in the scripts/ folder. Because there are multiple shaders per file you have to parse all the
    ///		.shader files available to know if there is a custom shader available. This class is designed to parse
    ///		all the .shader files available and save their settings for future use. </p>
    ///		I choose not to set up Material instances for shaders found since they may or may not be used by a level,
    ///		so it would be very wasteful to set up Materials since they load texture images for each layer (apart from the
    ///		lightmap). Once the usage of a shader is confirmed, a full Material instance can be set up from it.</p>
    ///		Because this is a subclass of ResourceManager, any files mentioned will be searched for in any path or
    ///		archive added to the resource paths/archives. See <see cref="ResourceManager"/> for details.
    ///	</remarks>
	public class Quake3ShaderManager : ResourceManager
	{
		#region Singleton implementation
		protected static Quake3ShaderManager instance;

		public static Quake3ShaderManager Instance 
		{
			get { return instance; }
		}

		static Quake3ShaderManager() 
		{ 
			instance = new Quake3ShaderManager();
		}

		protected Quake3ShaderManager() 
		{ 
		}
        #endregion

		#region Methods
		public void ParseShaderFile(Stream stream)
		{
			StreamReader file = new StreamReader(stream, Encoding.ASCII);
			string line;
			Quake3Shader shader = null;

			while((line = file.ReadLine()) != null)
			{
				line = line.Trim();

				// Ignore comments & blanks
				if((line != String.Empty) && !line.StartsWith("//"))
				{
					if(shader == null)
					{
						LogManager.Instance.Write("Creating {0}...", line);
						// No current shader
						// So first valid data should be a shader name
						shader = (Quake3Shader) Create(line);

						// Skip to and over next brace
						ParseHelper.SkipToNextOpenBrace(file);
					}
					else
					{
						// Already in a shader
						if(line == "}")
						{
							LogManager.Instance.Write("End of shader.");
							shader = null;
						}
						else if(line == "{")
						{
                            LogManager.Instance.Write("New pass...");
                            ParseNewShaderPass(file, shader);
						}
						else
						{
							LogManager.Instance.Write("New attrib, {0}...", line);
							ParseShaderAttrib(line.ToLower(), shader);
						}
					}
				}
			}
		}

		public void ParseAllSources(string extension)
		{
			Stream chunk;
			StringCollection shaderFiles = ResourceManager.GetAllCommonNamesLike("", extension);

			for(int i = 0; i < shaderFiles.Count; i++)
			{
				if((chunk = ResourceManager.FindCommonResourceData(shaderFiles[i])) == null)
					continue;

				ParseShaderFile(chunk);
			}
		}

		protected void ParseNewShaderPass(StreamReader stream, Quake3Shader shader)
		{
			string line;
			ShaderPass pass = new ShaderPass();

			// Default pass details
			pass.animNumFrames = 0;
			pass.blend = LayerBlendOperation.Replace;
			pass.blendDest = SceneBlendFactor.Zero;
			pass.depthFunc = CompareFunction.LessEqual;
			pass.flags = 0;
			pass.rgbGenFunc = ShaderGen.Identity;
			pass.tcModRotate = 0;
			pass.tcModScale[0] = pass.tcModScale[1] = 1.0f;
			pass.tcModScroll[0] = pass.tcModScroll[1] = 0.0f;
			pass.tcModStretchWave = ShaderWaveType.None;
			pass.tcModTransform[0] = pass.tcModTransform[1] = 0.0f;
			pass.tcModTurbOn = false;
			pass.tcModTurb[0] = pass.tcModTurb[1] = pass.tcModTurb[2] = pass.tcModTurb[3] = 0.0f;
			pass.texGen = ShaderTextureGen.Base;
			pass.addressMode = TextureAddressing.Wrap;
			pass.customBlend = false;
			pass.alphaVal = 0;
			pass.alphaFunc = CompareFunction.AlwaysPass;

			shader.Pass.Add(pass);

			while((line = stream.ReadLine()) != null)
			{
				line = line.Trim();

				// Ignore comments & blanks
				if((line != String.Empty) && !line.StartsWith("//"))
				{
					if(line == "}")
						return;
					else
						ParseShaderPassAttrib(line, shader, pass);
				}
			}
		}

		protected void ParseShaderAttrib(string line, Quake3Shader shader)
		{
			string[] attribParams = line.Replace("(", "").Replace(")", "").Split(' ', '\t');

			if(attribParams[0] == "skyparms")
			{
				if(attribParams[1] != "-")
				{
					shader.Farbox = true;
					shader.FarboxName = attribParams[1];
				}
				if(attribParams[2] != "-")
				{
					shader.SkyDome = true;

					if(attribParams[2] == "full")
						shader.CloudHeight = 512;
					else
						shader.CloudHeight = StringConverter.ParseFloat(attribParams[2]);
				}

				// nearbox not supported
			}
			else if(attribParams[0] == "cull")
			{
				if((attribParams[1] == "diable") || (attribParams[1] == "none"))
					shader.CullingMode = ManualCullingMode.None;
				else if(attribParams[1] == "front")
					shader.CullingMode = ManualCullingMode.Front;
				else if(attribParams[1] == "back")
					shader.CullingMode = ManualCullingMode.Back;
			}
			else if (attribParams[0] == "deformvertexes")
			{
				// TODO
			}
			else if(attribParams[0] == "fogparms")
			{
				string[] fogValues = new string[4];
				Array.Copy(attribParams, 1, fogValues, 0, 4);

				/*shader.Fog = true;
				shader.FogColour = StringConverter.ParseColor(fogValues);
				shader.FogDistance = StringConverter.ParseFloat(attribParams[4]);*/
			}
		}

		protected void ParseShaderPassAttrib(string line, Quake3Shader shader, ShaderPass pass)
		{
			string[] attribParams = line.Split(' ', '\t');
			attribParams[0] = attribParams[0].ToLower();

			LogManager.Instance.Write("Attrib {0}", attribParams[0]);
			if((attribParams[0] != "map") && (attribParams[0] != "clampmap") && (attribParams[0] != "animmap"))
			{
				// lower case all except textures
                for (int i = 1; i < attribParams.Length; i++) {
                    attribParams[i] = attribParams[i].ToLower();
                }
            }

			// MAP
			if(attribParams[0] == "map")
			{
				pass.textureName = attribParams[1];

                if (attribParams[1].ToLower() == "$lightmap") {
                    pass.texGen = ShaderTextureGen.Lightmap;
                }
            }
			// CLAMPMAP
			else if(attribParams[0] == "clampmap")
			{
				pass.textureName = attribParams[1];

				if(attribParams[1].ToLower() == "$lightmap")
					pass.texGen = ShaderTextureGen.Lightmap;

				pass.addressMode = TextureAddressing.Clamp;
			}
			// ANIMMAP
			else if(attribParams[0] == "animmap")
			{
				pass.animFps = StringConverter.ParseFloat(attribParams[1]);
				pass.animNumFrames = attribParams.Length - 2;

				for(uint frame = 0; frame < pass.animNumFrames; frame++)
					pass.frames[frame] = attribParams[frame + 2];
			}
			// BLENDFUNC
			else if(attribParams[0] == "blendfunc")
			{
				if((attribParams[1] == "add") || (attribParams[1] == "gl_add"))
				{
					pass.blend = LayerBlendOperation.Add;
					pass.blendDest = SceneBlendFactor.One;
					pass.blendSrc = SceneBlendFactor.One;
				}
				else if((attribParams[1] == "filter") || (attribParams[1] == "gl_filter"))
				{
					pass.blend = LayerBlendOperation.Modulate;
					pass.blendDest = SceneBlendFactor.Zero;
					pass.blendSrc = SceneBlendFactor.DestColor;
				}
				else if((attribParams[1] == "blend") || (attribParams[1] == "gl_blend"))
				{
					pass.blend = LayerBlendOperation.AlphaBlend;
					pass.blendDest = SceneBlendFactor.OneMinusSourceAlpha;
					pass.blendSrc = SceneBlendFactor.SourceAlpha;
				}
				else
				{
					// Manual blend
					pass.blendSrc = ConvertBlendFunc(attribParams[1]);
					pass.blendDest = ConvertBlendFunc(attribParams[2]);
					
					// Detect common blends
					if((pass.blendSrc == SceneBlendFactor.One) && (pass.blendDest == SceneBlendFactor.Zero))
						pass.blend = LayerBlendOperation.Replace;
					else if((pass.blendSrc == SceneBlendFactor.One) && (pass.blendDest == SceneBlendFactor.One))
						pass.blend = LayerBlendOperation.Add;
					else if(((pass.blendSrc == SceneBlendFactor.Zero) && (pass.blendDest == SceneBlendFactor.SourceColor)) ||
						((pass.blendSrc == SceneBlendFactor.DestColor) && (pass.blendDest == SceneBlendFactor.Zero)))
						pass.blend = LayerBlendOperation.Modulate;
					else if((pass.blendSrc == SceneBlendFactor.SourceAlpha) && (pass.blendDest == SceneBlendFactor.OneMinusSourceAlpha))
						pass.blend = LayerBlendOperation.AlphaBlend;
					else 
						pass.customBlend = true;

					// NB other custom blends might not work due to OGRE trying to use multitexture over multipass
				}
			}
			// RGBGEN
			else if(attribParams[0] == "rgbgen")
			{
				// TODO
			}
			// ALPHAGEN
			else if(attribParams[0] == "alphagen")
			{
				// TODO
			}
			// TCGEN
			else if(attribParams[0] == "tcgen")
			{
				if(attribParams[1] == "base")
					pass.texGen = ShaderTextureGen.Base;
				else if(attribParams[1] == "lightmap")
					pass.texGen = ShaderTextureGen.Lightmap;
				else if(attribParams[1] == "environment")
					pass.texGen = ShaderTextureGen.Environment;
			}
			// TCMOD
			else if(attribParams[0] == "tcmod")
			{
				if(attribParams[1] == "rotate")
				{
					pass.tcModRotate = -StringConverter.ParseFloat(attribParams[2]) / 360; // +ve is clockwise degrees in Q3 shader, anticlockwise complete rotations in Ogre
				}
				else if(attribParams[1] == "scroll")
				{
					pass.tcModScroll[0] = StringConverter.ParseFloat(attribParams[2]);
					pass.tcModScroll[1] = StringConverter.ParseFloat(attribParams[3]);
				}
				else if(attribParams[1] == "scale")
				{
					pass.tcModScale[0] = StringConverter.ParseFloat(attribParams[2]);
					pass.tcModScale[1] = StringConverter.ParseFloat(attribParams[3]);
				}
				else if(attribParams[1] == "stretch")
				{
					if(attribParams[2] == "sin")
						pass.tcModStretchWave = ShaderWaveType.Sin;
					else if(attribParams[2] == "triangle")
						pass.tcModStretchWave = ShaderWaveType.Triangle;
					else if(attribParams[2] == "square")
						pass.tcModStretchWave = ShaderWaveType.Square;
					else if(attribParams[2] == "sawtooth")
						pass.tcModStretchWave = ShaderWaveType.SawTooth;
					else if(attribParams[2] == "inversesawtooth")
						pass.tcModStretchWave = ShaderWaveType.InverseSawtooth;

					pass.tcModStretchParams[0] = StringConverter.ParseFloat(attribParams[3]);
					pass.tcModStretchParams[1] = StringConverter.ParseFloat(attribParams[4]);
					pass.tcModStretchParams[2] = StringConverter.ParseFloat(attribParams[5]);
					pass.tcModStretchParams[3] = StringConverter.ParseFloat(attribParams[6]);
				}
			}
			// TURB
			else if(attribParams[0] == "turb")
			{
				pass.tcModTurbOn = true;
				pass.tcModTurb[0] = StringConverter.ParseFloat(attribParams[2]);
				pass.tcModTurb[1] = StringConverter.ParseFloat(attribParams[3]);
				pass.tcModTurb[2] = StringConverter.ParseFloat(attribParams[4]);
				pass.tcModTurb[3] = StringConverter.ParseFloat(attribParams[5]);
			}
			// DEPTHFUNC
			else if(attribParams[0] == "depthfunc")
			{
				// TODO
			}
			// DEPTHWRITE
			else if(attribParams[0] == "depthwrite")
			{
				// TODO
			}
			// ALPHAFUNC
			else if(attribParams[0] == "alphafunc")
			{
				if(attribParams[1] == "gt0")
				{
					pass.alphaVal = 0;
					pass.alphaFunc = CompareFunction.Greater;
				}
				else if(attribParams[1] == "ge128")
				{
					pass.alphaVal = 128;
					pass.alphaFunc = CompareFunction.GreaterEqual;
				}
				else if(attribParams[1] == "lt128")
				{
					pass.alphaVal = 128;
					pass.alphaFunc = CompareFunction.Less;
				}
			}
		}

		protected SceneBlendFactor ConvertBlendFunc(string q3func)
		{
			if(q3func == "gl_one")
				return SceneBlendFactor.One;
			else if(q3func == "gl_zero")
				return SceneBlendFactor.Zero;
			else if(q3func == "gl_dst_color")
				return SceneBlendFactor.DestColor;
			else if(q3func == "gl_src_color")
				return SceneBlendFactor.SourceColor;
			else if(q3func == "gl_one_minus_dest_color")
				return SceneBlendFactor.OneMinusDestColor;
			else if(q3func == "gl_src_alpha")
				return SceneBlendFactor.SourceAlpha;
			else if(q3func == "gl_one_minus_src_alpha")
				return SceneBlendFactor.OneMinusSourceAlpha;

			// Default if unrecognised
			return SceneBlendFactor.One;
		}
		#endregion

		#region ResourceManager implementation
		public override Resource Create(string name)
		{
			Quake3Shader s = new Quake3Shader(name);
			Load(s, 1);
			
			return s;
		} 
		#endregion
	}
}