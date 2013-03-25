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
using System.Collections;
using System.Runtime.InteropServices;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Controllers;

using Axiom.SceneManagers.Bsp.Collections;

namespace Axiom.SceneManagers.Bsp
{
	/// <summary>
	///		Class for recording Quake3 shaders.
	/// </summary>
	/// <remarks>
	///		This is a temporary holding area since shaders are actually converted into
	///		Material objects for use in the engine proper. However, because we have to read
	///		in shader definitions en masse (because they are stored in shared .shader files)
	///		without knowing which will actually be used, we store their definitions here
	///		temporarily since their instantiations as Materials would use precious resources
	///		because of the automatic loading of textures etc.
	/// </remarks>
	public class Quake3Shader : Resource
	{
		#region Protected members
		protected uint flags;
		protected ShaderPassCollection pass;
		protected bool farBox;            // Skybox
		protected string farBoxName;
		protected bool skyDome;
		protected float cloudHeight;       // Skydome
		protected ShaderDeformFunc deformFunc;
		protected float[] deformParams;
		protected ManualCullingMode cullMode;

		protected bool fog;
		protected ColorEx fogColour;
		protected float fogDistance;
		#endregion

		#region Properties
		public uint Flags
		{
			get { return flags; }
			set { flags = value; }
		}

		public int NumPasses
		{
			get { return pass.Count; }
		}

		public ShaderPassCollection Pass
		{
			get 
			{ 
				return pass;
			}
			set 
			{ 
				pass = value;
			}
		}

		public bool Farbox
		{
			get { return farBox; }
			set { farBox = value; }
		}

		public string FarboxName
		{
			get { return farBoxName; }
			set { farBoxName = value; }
		}

		public bool SkyDome
		{
			get { return skyDome; }
			set { skyDome = value; }
		}

		public float CloudHeight
		{
			get { return cloudHeight; }
			set { cloudHeight = value; }
		}

		public ShaderDeformFunc DeformFunc
		{
			get { return deformFunc; }
			set { deformFunc = value; }
		}

		public float[] DeformParams
		{
			get { return deformParams; }
			set { deformParams = value; }
		}

		public ManualCullingMode CullingMode
		{
			get { return cullMode; }
			set { cullMode = value; }
		}

		public bool Fog
		{
			get { return fog; }
			set { fog = value; }
		}

		public ColorEx FogColour
		{
			get { return fogColour; }
			set { fogColour = value; }
		}

		public float FogDistance
		{
			get { return fogDistance; }
			set { fogDistance = value; }
		}
		#endregion

		#region Constructor
		/// <summary>
		///		Default constructor - used by <see cref="Quake3ShaderManager"/> (do not call directly)
		/// </summary>
		/// <param name="name">Shader name.</param>
		public Quake3Shader(string name)
		{
			this.name = name;
			deformFunc = ShaderDeformFunc.None;
			deformParams = new float[5];
			cullMode = ManualCullingMode.Back;
			pass = new ShaderPassCollection();
		}
		#endregion

		#region Methods
		protected string GetAlternateName(string textureName)
		{
			// Get alternative JPG to TGA and vice versa
			int pos;
			string ext, baseName;

			pos = textureName.LastIndexOf(".");
			ext = textureName.Substring(pos,4).ToLower();
			baseName = textureName.Substring(0,pos);
			if (ext == ".jpg")
			{
				return baseName + ".tga";
			}
			else
			{
				return baseName + ".jpg";
			}
		}

		/// <summary>
		///		Creates this shader as an OGRE material.
		/// </summary>
		/// <remarks>
		///		Creates a new material based on this shaders settings and registers it with the
		///		SceneManager passed in. 
		///		Material name is in the format of: shader#lightmap.
		/// </remarks>
		/// <param name="sm">SceneManager to register the material with.</param>
		/// <param name="lightmapNumber">Lightmap number</param>
		public Material CreateAsMaterial(SceneManager sm, int lightmapNumber)
		{
			string materialName = String.Format("{0}#{1}", name, lightmapNumber);
			Material material = sm.CreateMaterial(materialName);

            LogManager.Instance.Write("Using Q3 shader {0}", name);

            for(int p = 0; p < pass.Count; ++p)
			{
				TextureUnitState t;
				
				// Create basic texture
				t = LoadMaterialTextures(p, lightmapNumber, material);

				// Blending
				if(p == 0)
				{
					// scene blend
					material.SetSceneBlending(pass[p].blendSrc, pass[p].blendDest);

					if(material.IsTransparent && (pass[p].blendSrc != SceneBlendFactor.SourceAlpha))
						material.DepthWrite = false;

					t.SetColorOperation(LayerBlendOperation.Replace);
				}
				else
				{
					if(pass[p].customBlend)
					{
						// Fallback for now
						t.SetColorOperation(LayerBlendOperation.Modulate);
					}
					else
					{
						t.SetColorOperation(pass[p].blend);
					}
				}

				// Tex coords
				if(pass[p].texGen == ShaderTextureGen.Base)
					t.TextureCoordSet = 0;
				else if(pass[p].texGen == ShaderTextureGen.Lightmap)
					t.TextureCoordSet = 1;
				else if(pass[p].texGen == ShaderTextureGen.Environment)
					t.SetEnvironmentMap(true, EnvironmentMap.Planar);

				// Tex mod
				// Scale
				t.SetTextureScaleU(pass[p].tcModScale[0]);
				t.SetTextureScaleV(pass[p].tcModScale[1]);

				CreateProceduralTextureMods(p, t);
				// Address mode
				t.TextureAddressing = pass[p].addressMode;
				// Alpha mode
				t.SetAlphaRejectSettings(pass[p].alphaFunc, pass[p].alphaVal);
			}

			// Do farbox (create new material)

			// Do skydome (use this material)
			if(skyDome)
			{
				float halfAngle = 0.5f * (0.5f * (4.0f * (float) Math.Atan(1.0f)));
				float sin = (float) Math.Sin(halfAngle);

				// Quake3 is always aligned with Z upwards
				Quaternion q = new Quaternion(
					(float) Math.Cos(halfAngle),
					sin * Vector3.UnitX.x,
					sin * Vector3.UnitY.y,
					sin * Vector3.UnitX.z
					);
					
				// Also draw last, and make close to camera (far clip plane is shorter)
				sm.SetSkyDome(true, materialName, 20 - (cloudHeight / 256 * 18), 12, 2000, false, q);
			}

			material.CullingMode = Axiom.Graphics.CullingMode.None;
			material.ManualCullMode = cullMode;
			material.Lighting = false;
			material.Load();

			return material;
		}

		private void CreateProceduralTextureMods(int p, TextureUnitState t)
		{
			// Procedural mods
			// Custom - don't use mod if generating environment
			// Because I do env a different way it look horrible
			if(pass[p].texGen != ShaderTextureGen.Environment)
			{
				if(pass[p].tcModRotate != 0.0f)
					t.SetRotateAnimation(pass[p].tcModRotate);

				if((pass[p].tcModScroll[0] != 0.0f) || (pass[p].tcModScroll[1] != 0.0f))
				{
					if(pass[p].tcModTurbOn)
					{
						// Turbulent scroll
						if(pass[p].tcModScroll[0] != 0.0f)
						{
							t.SetTransformAnimation(TextureTransform.TranslateU, WaveformType.Sine, 
							                        pass[p].tcModTurb[0], pass[p].tcModTurb[3], pass[p].tcModTurb[2], pass[p].tcModTurb[1]);
						}
						if(pass[p].tcModScroll[1] != 0.0f)
						{
							t.SetTransformAnimation(TextureTransform.TranslateV, WaveformType.Sine,
							                        pass[p].tcModTurb[0], pass[p].tcModTurb[3], pass[p].tcModTurb[2], pass[p].tcModTurb[1]);
						}
					}
					else
					{
						// Constant scroll
						t.SetScrollAnimation(pass[p].tcModScroll[0], pass[p].tcModScroll[1]);
					}
				}

				if(pass[p].tcModStretchWave != ShaderWaveType.None)
				{
					WaveformType wft = WaveformType.Sine;
					switch(pass[p].tcModStretchWave)
					{
						case ShaderWaveType.Sin:
							wft = WaveformType.Sine;
							break;
						case ShaderWaveType.Triangle:
							wft = WaveformType.Triangle;
							break;
						case ShaderWaveType.Square:
							wft = WaveformType.Square;
							break;
						case ShaderWaveType.SawTooth:
							wft = WaveformType.Sawtooth;
							break;
						case ShaderWaveType.InverseSawtooth:
							wft = WaveformType.InverseSawtooth;
							break;
					}
		
					// Create wave-based stretcher
					t.SetTransformAnimation(TextureTransform.ScaleU, wft, pass[p].tcModStretchParams[3],
					                        pass[p].tcModStretchParams[0], pass[p].tcModStretchParams[2], pass[p].tcModStretchParams[1]);
					t.SetTransformAnimation(TextureTransform.ScaleV, wft, pass[p].tcModStretchParams[3],
					                        pass[p].tcModStretchParams[0], pass[p].tcModStretchParams[2], pass[p].tcModStretchParams[1]);
				}
			}
		}

		private TextureUnitState LoadMaterialTextures(int p, int lightmapNumber, Material material)
		{
			TextureUnitState t;
			if(pass[p].textureName == "$lightmap")
			{
				string lightmapName = String.Format("@lightmap{0}", lightmapNumber);
				t = material.GetTechnique(0).GetPass(0).CreateTextureUnitState(lightmapName);
			}
				// Animated texture support
			else if(pass[p].animNumFrames > 0)
			{
				float sequenceTime = pass[p].animNumFrames / pass[p].animFps;

				/* Pre-load textures
					We need to know if each one was loaded OK since extensions may change for each
					Quake3 can still include alternate extension filenames e.g. jpg instead of tga
					Pain in the arse - have to check for each frame as letters<n>.tga for example
					is different per frame!
					*/
				for(uint alt = 0; alt < pass[p].animNumFrames; ++alt)
				{
					try 
					{
						TextureManager.Instance.Load(pass[p].frames[alt]);
					}
					catch
					{
						// Try alternate extension
						pass[p].frames[alt] = GetAlternateName(pass[p].frames[alt]);

						try 
						{
							TextureManager.Instance.Load(pass[p].frames[alt]);
						}
						catch
						{ 
							// stuffed - no texture
						}
					}

				}
				
				t = material.GetTechnique(0).GetPass(0).CreateTextureUnitState("");
				t.SetAnimatedTextureName(pass[p].frames, pass[p].animNumFrames, sequenceTime);

				if(t.IsBlank)
				{
					for(int alt = 0; alt < pass[p].animNumFrames; alt++)
						pass[p].frames[alt] = GetAlternateName(pass[p].frames[alt]);

					t.SetAnimatedTextureName(pass[p].frames, pass[p].animNumFrames, sequenceTime);
				}
			}
			else
			{
				// Quake3 can still include alternate extension filenames e.g. jpg instead of tga
				// Pain in the arse - have to check for failure
				try 
				{
					TextureManager.Instance.Load(pass[p].textureName);
				}
				catch
				{
					// Try alternate extension
					pass[p].textureName = GetAlternateName(pass[p].textureName);
					
					try
					{
						TextureManager.Instance.Load(pass[p].textureName);
					}
					catch
					{
						// stuffed - no texture
					}
				}

				t = material.GetTechnique(0).GetPass(0).CreateTextureUnitState(pass[p].textureName);
			}
			return t;
		}

		#endregion

		#region Implementation of Resource
		public override void Load()
		{
			// Do nothing.
		}

		public override void Unload()
		{
			// Do nothing.
		}
		#endregion
	}

	public class ShaderPass 
	{
		public uint flags;
		public string textureName;
		public ShaderTextureGen texGen;

		// Multitexture blend
		public LayerBlendOperation blend;
		// Multipass blends (Quake3 only supports multipass?? Surely not?)
		public SceneBlendFactor blendSrc;
		public SceneBlendFactor blendDest;
		public bool customBlend;

		public CompareFunction depthFunc;
		public TextureAddressing addressMode;

		// TODO - alphaFunc
		public ShaderGen rgbGenFunc;
		public ShaderWaveType rgbGenWave;
		public float[] rgbGenParams = new float[4]; // base, amplitude, phase, frequency
		public float[] tcModScale = new float[2];
		public float tcModRotate;
		public float[] tcModScroll = new float[2];
		public float[] tcModTransform = new float[6];
		public bool tcModTurbOn;
		public float[] tcModTurb = new float[4];
		public ShaderWaveType tcModStretchWave;
		public float[] tcModStretchParams = new float[4];    // base, amplitude, phase, frequency
		public CompareFunction alphaFunc;
		public byte alphaVal;

		public float animFps;
		public int animNumFrames;
		public string[] frames = new string[32];
	};

	[Flags]
	public enum ShaderFlags
	{
		NoCull		= 1 << 0,
		Transparent = 1 << 1,
		DepthWrite	= 1 << 2,
		Sky			= 1 << 3,
		NoMipMaps	= 1 << 4,
		NeedColours = 1 << 5,
		DeformVerts = 1 << 6
	}

	[Flags]
	public enum ShaderPassFlags
	{
		Lightmap	= 1 << 0,
		Blend		= 1 << 1,
		AlphaFunc	= 1 << 2,
		TCMod		= 1 << 3,
		AnimMap		= 1 << 5,
		TCGenEnv	= 1 << 6
	}

	public enum ShaderGen
	{
		Identity = 0,
		Wave,
		Vertex
	}
	
	public enum ShaderTextureGen
	{
		Base = 0,
		Lightmap,
		Environment
	}

	public enum ShaderWaveType
	{
		None = 0,
		Sin,
		Triangle,
		Square,
		SawTooth,
		InverseSawtooth
	}
	
	public enum ShaderDeformFunc
	{
		None = 0,
		Bulge,
		Wave,
		Normal,
		Move,
		AutoSprite,
		AutoSprite2
	}
}