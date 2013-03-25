using System;
using System.Runtime.InteropServices;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Bsp
{
	/// <summary>
	/// Summary description for TextureLight.
	/// </summary>
	public class TextureLight : Light
	{
		protected BspSceneManager creator;
		protected bool isTextureLight;
		protected ColorEx textureColor;
		protected LightIntensity intensity;
		protected int priority;

		public bool IsTextureLight
		{
			get { return isTextureLight; }
			set { isTextureLight = value; }
		}

		public LightIntensity Intensity
		{
			get { return intensity; }
			set { intensity = value; }
		}

		public int Priority
		{
			get { return priority; }
			set { priority = value; }
		}

		// used in BspSceneManager.PopulateLightList method
		internal float TempSquaredDist
		{
			get { return base.tempSquaredDist;	}
			set	{ base.tempSquaredDist = value;	}
		}

		/// <summary>
		///		Default constructor.
		/// </summary>
		public TextureLight(BspSceneManager creator) : this("", creator)
		{
		}

		/// <summary>
		///		Normal constructor. Should not be called directly, but rather the SceneManager.CreateLight method should be used.
		/// </summary>
		/// <param name="name"></param>
		public TextureLight(string name, BspSceneManager creator) : base(name)
		{
			this.creator = creator;
			isTextureLight = true;
			diffuse = ColorEx.White;
            textureColor = ColorEx.White;
			intensity = LightIntensity.Normal;
			priority = 100;
		}

		public bool AffectsFaceGroup(StaticFaceGroup faceGroup, ManualCullingMode cullMode)
		{
			bool affects = false;
			float lightDist = 0, angle;

			if (this.Type == LightType.Directional)
			{
				angle = faceGroup.plane.Normal.Dot(this.DerivedDirection);

				if(cullMode != ManualCullingMode.None)
				{
					if(((angle < 0) && (cullMode == ManualCullingMode.Front)) ||
						((angle > 0) && (cullMode == ManualCullingMode.Back)))
						return false;
				}
			}
			else
			{
				lightDist = faceGroup.plane.GetDistance(this.DerivedPosition);

				if(cullMode != ManualCullingMode.None)
				{
					if(((lightDist < 0) && (cullMode == ManualCullingMode.Back)) ||
						((lightDist > 0) && (cullMode == ManualCullingMode.Front)))
						return false;
				}
			}

			switch (this.Type)
			{
				case LightType.Directional:
					affects = true;
					break;

				case LightType.Point:
					if (MathUtil.Abs(lightDist) < range)
						affects = true;
					break;

				case LightType.Spotlight:
					if (MathUtil.Abs(lightDist) < range)
					{
						angle = faceGroup.plane.Normal.Dot(this.DerivedDirection);
						if (((lightDist < 0 && angle > 0) || (lightDist > 0 && angle < 0)) &&
							MathUtil.Abs(angle) >= MathUtil.Cos(this.spotOuter * 0.5f))
							affects = true;
					}
					break;
			}

			return affects;
		}

		public bool CalculateTexCoordsAndColors(Plane plane, Vector3[] vertices, out Vector2[] texCoors, out ColorEx[] colors)
		{
			switch (this.Type)
			{
				case LightType.Directional:
					return CalculateForDirectionalLight(plane, vertices, out texCoors, out colors);

				case LightType.Point:
					return CalculateForPointLight(plane, vertices, out texCoors, out colors);

				case LightType.Spotlight:
					return CalculateForSpotLight(plane, vertices, out texCoors, out colors);
			}

			texCoors = null;
			colors = null;
			return false;
		}

		protected bool CalculateForPointLight(Plane plane, Vector3[] vertices, out Vector2[] texCoors, out ColorEx[] colors)
		{
			texCoors = new Vector2[vertices.Length];
			colors = new ColorEx[vertices.Length];

			Vector3 lightPos, faceLightPos;

			lightPos = this.DerivedPosition;

			float dist = plane.GetDistance(lightPos);
			if (MathUtil.Abs(dist) < range)
			{
				// light is visible

				//light pos on face
				faceLightPos = lightPos - plane.Normal * dist;

				Vector3 verAxis = plane.Normal.Perpendicular();
				Vector3 horAxis = verAxis.Cross(plane.Normal);
				Plane verPlane = new Plane(verAxis, faceLightPos);
				Plane horPlane = new Plane(horAxis, faceLightPos);

				float lightRadiusSqr = range * range;
				float relRadiusSqr = lightRadiusSqr - dist * dist;
				float relRadius = MathUtil.Sqrt(relRadiusSqr);
				float scale = 0.5f / relRadius;

				float brightness = relRadiusSqr / lightRadiusSqr;
				ColorEx lightCol = new ColorEx(brightness * textureColor.a,
					textureColor.r, textureColor.g, textureColor.b);

				for (int i = 0; i < vertices.Length; i++)
				{
					texCoors[i].x = horPlane.GetDistance(vertices[i]) * scale + 0.5f;
					texCoors[i].y = verPlane.GetDistance(vertices[i]) * scale + 0.5f;
					colors[i] = lightCol;
				}

				return true;
			}

			return false;
		}

		protected bool CalculateForSpotLight(Plane plane, Vector3[] vertices, out Vector2[] texCoors, out ColorEx[] colors)
		{
			texCoors = new Vector2[vertices.Length];
			colors = new ColorEx[vertices.Length];

			ColorEx lightCol = new ColorEx(textureColor.a,
				textureColor.r, textureColor.g, textureColor.b);

			for (int i = 0; i < vertices.Length; i++)
			{
				colors[i] = lightCol;
			}

			return true;
		}
		
		protected bool CalculateForDirectionalLight(Plane plane, Vector3[] vertices, out Vector2[] texCoors, out ColorEx[] colors)
		{
			texCoors = new Vector2[vertices.Length];
			colors = new ColorEx[vertices.Length];

			float angle = MathUtil.Abs(plane.Normal.Dot(this.DerivedDirection));

			ColorEx lightCol = new ColorEx(textureColor.a * angle,
				textureColor.r, textureColor.g, textureColor.b);

			for (int i = 0; i < vertices.Length; i++)
			{
				colors[i] = lightCol;
			}

			return true;
		}

		public static Texture CreateTexture()
		{
			Texture tex = TextureManager.Instance.GetByName("Axiom/LightingTexture");
			if (tex == null)
			{
				byte[] fotbuf = new byte[128 * 128 * 4];
				for (int y=0;y<128;y++)
					for (int x=0;x<128;x++)
					{
						byte alpha = 0;
						float radius = (x-64)*(x-64) + (y-64)*(y-64);
						radius = 4000 - radius;
						if (radius > 0)
						{
							alpha = (byte)((radius / 4000) * 255);
						}
						fotbuf[y*128*4+x*4] = 255;
						fotbuf[y*128*4+x*4+1] = 255;
						fotbuf[y*128*4+x*4+2] = 255;
						fotbuf[y*128*4+x*4+3] = alpha;
					}
            
				System.IO.MemoryStream stream = new System.IO.MemoryStream(fotbuf);
				Axiom.Media.Image img = Axiom.Media.Image.FromRawStream(stream,128,128,Axiom.Media.PixelFormat.A8R8G8B8);
				TextureManager.Instance.LoadImage("Axiom/LightingTexture", img, TextureType.TwoD, 0, 1, 1);

				tex = TextureManager.Instance.GetByName("Axiom/LightingTexture");
			}

			return tex;
		}


		public override ColorEx Diffuse
		{
			get
			{
				return diffuse;
			}
			set
			{
				diffuse = value;
                
				float maxParam = MathUtil.Max(MathUtil.Max(diffuse.r, diffuse.g), diffuse.b);
				if (maxParam > 0f)
				{
					float inv = 1 / maxParam;
					textureColor.r = diffuse.r * inv;
					textureColor.g = diffuse.g * inv;
					textureColor.b = diffuse.b * inv;
				}

				textureColor.a = maxParam;
			}
		}

		public override void Update()
		{
			Vector3 prevPosition = derivedPosition;

			base.Update ();

			// if the position is changed notify BspSceneManager to put it in the bsp tree
			if (derivedPosition != prevPosition)
                creator.NotifyObjectMoved(this, derivedPosition);
		}
	}

	/// <summary>
	///		Parameters for texture lighting.
	/// </summary>
	/// <remarks>
	///		The diffuse color and textureLightMap components are held separately from
	///		the other vertex components (position, normal, etc) so that dynamic vertex
	///		buffers can be used to change their values.
	///	</remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct TextureLightMap
	{
		public int color;
		public Vector2 textureLightMap;
	}

	/// <summary>
	///		Determines how bright the texture light can be
	/// </summary>
	public enum LightIntensity
	{
		// Bright as the original texture
		Normal,
		// Bright as the texture with X2 modulated colors
		ModulateX2,
		// Bright as the texture with X4 modulated colors
		ModulateX4
	}
}
