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

using System;
using System.Collections.Generic;
using System.Text;
using Axiom.MathLib;
using Axiom.Core;
using Axiom.Graphics;
using Multiverse.Config;

namespace Axiom.SceneManagers.Multiverse
{
    public enum ShadowTechnique
    {
        None,
        Simple,
        Depth
    }

    public class ShadowConfig
    {

        protected ShadowTechnique shadowTechnique;
        protected SceneManager scene;
        protected int maxVertexProgramModel = 0;
        protected int maxFragmentProgramModel = 0;

        public event ConfigChangeHandler ShadowTechniqueChange;

        public ShadowConfig(SceneManager scene)
        {
            this.scene = scene;

            GetShaderSupport();

            shadowTechnique = ShadowTechnique.None;

            ParameterRegistry.RegisterSubsystemHandlers("Shadows", setShadowParameterHandler,
                                                        getShadowParameterHandler);
        }

        protected void GetShaderSupport()
        {
            switch (scene.TargetRenderSystem.Caps.MaxVertexProgramVersion)
            {
                case "vs_1_1":
                    maxVertexProgramModel = 1;
                    break;
                case "vs_2_0":
                case "vs_2_x":
                    maxVertexProgramModel = 2;
                    break;
                case "vs_3_0":
                    maxVertexProgramModel = 3;
                    break;
            }

            switch (scene.TargetRenderSystem.Caps.MaxFragmentProgramVersion)
            {
                case "ps_1_1":
                case "ps_1_2":
                case "ps_1_3":
                case "ps_1_4":
                    maxFragmentProgramModel = 1;
                    break;
                case "ps_2_0":
                case "ps_2_x":
                    maxFragmentProgramModel = 2;
                    break;
                case "ps_3_0":
                case "ps_3_x":
                    maxFragmentProgramModel = 3;
                    break;
            }
        }

        protected void OnShadowTechniqueChange()
        {
            ConfigChangeHandler handler = ShadowTechniqueChange;
            if (handler != null)
            {
                handler(null, new EventArgs());
            }
        }

        protected void ValidateShadowTechnique()
        {
            switch (shadowTechnique)
            {
                case ShadowTechnique.None:
                    break;
                case ShadowTechnique.Simple:
                    break;
                case ShadowTechnique.Depth:
                    // fall back to simple if we don't have shader model 2.0
                    if (maxFragmentProgramModel < 2)
                    {
                        shadowTechnique = ShadowTechnique.Simple;
                    }
                    break;
            }
        }

        protected void ChangeShadowTechnique()
        {
            switch (shadowTechnique)
            {
                case ShadowTechnique.None:
                    scene.ShadowTechnique = Axiom.Graphics.ShadowTechnique.None;
                    scene.AutoParamDataSource.MVShadowTechnique = new Vector4(0, 0, 0, 0);
                    break;

                case ShadowTechnique.Simple:
                    scene.EnsureShadowTexturesCreated();
                    scene.ShadowTechnique = Axiom.Graphics.ShadowTechnique.TextureModulative;

                    scene.ShadowTextureCasterMaterial = "";
                    scene.ShadowTextureReceiverMaterial = "";
                    scene.ShadowTextureSelfShadow = false;
                    scene.ShadowTextureFormat = Axiom.Media.PixelFormat.A8R8G8B8;
                    scene.AutoParamDataSource.MVShadowTechnique = new Vector4(1, 0, 0, 0);
                    break;

                case ShadowTechnique.Depth:
                    scene.EnsureShadowTexturesCreated();
                    scene.ShadowTechnique = Axiom.Graphics.ShadowTechnique.TextureModulative;

                    scene.ShadowTextureCasterMaterial = "MVSMShadowCaster";
                    scene.ShadowTextureReceiverMaterial = "MVSMShadowReceiver";
                    scene.ShadowTextureSelfShadow = true;
                    scene.ShadowTextureFormat = Axiom.Media.PixelFormat.FLOAT32_R;
                    scene.AutoParamDataSource.MVShadowTechnique = new Vector4(0, 1, 0, 0);
                    UpdateDepthShadowReceiverMaterial();
                    break;
            }
        }

        public ShadowTechnique ShadowTechnique
        {
            get
            {
                return shadowTechnique;
            }
            set
            {
                shadowTechnique = value;

                ChangeShadowTechnique();

                OnShadowTechniqueChange();
            }
        }

        public Axiom.Core.ColorEx ShadowColor
        {
            get
            {
                return scene.ShadowColor;
            }
            set
            {
                scene.ShadowColor = value;
                if (shadowTechnique == ShadowTechnique.Depth)
                {
                    UpdateDepthShadowReceiverMaterial();
                }
            }
        }

        public int ShadowTextureCount
        {
            get
            {
                return scene.ShadowTextureCount;
            }
            set
            {
                scene.ShadowTextureCount = value;
            }
        }

        public int ShadowTextureSize
        {
            get
            {
                return scene.ShadowTextureSize;
            }
            set
            {
                scene.ShadowTextureSize = value;
                if (shadowTechnique == ShadowTechnique.Depth)
                {
                    UpdateDepthShadowReceiverMaterial();
                }
            }
        }

        public float ShadowFarDistance
        {
            get
            {
                return scene.ShadowFarDistance;
            }
            set
            {
                scene.ShadowFarDistance = value;
            }
        }

        protected void UpdateDepthShadowReceiverMaterial()
        {
            Material material = MaterialManager.Instance.GetByName("MVSMShadowReceiver");

            if (material != null)
            {
                Pass depthReceiverPass = material.GetTechnique(0).GetPass(0);

                depthReceiverPass.FragmentProgramParameters.SetNamedConstant("ShadowColor", ShadowColor);
                depthReceiverPass.FragmentProgramParameters.SetNamedConstant("TexelOffset", new Vector4(1.0f / ShadowTextureSize, 1.0f / ShadowTextureSize, 0, 0));
            }
        }

        private bool setShadowParameterHandler(string parameterName, string parameterValue)
        {
            string s = parameterValue.ToLower();
            bool ret = true;
            switch (parameterName)
            {
                case "ShadowTechnique":
                    switch (s)
                    {
                        case "none":
                            ShadowTechnique = ShadowTechnique.None;
                            break;
                        case "simple":
                            ShadowTechnique = ShadowTechnique.Simple;
                            break;
                        case "depth":
                            ShadowTechnique = ShadowTechnique.Depth;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                    break;
                case "ShadowColor":
                    ShadowColor = ColorEx.Parse_0_255_String(s);
                    break;
                case "ShadowTextureCount":
                    int count;
                    ret = int.TryParse(s, out count);
                    if (ret)
                    {
                        ShadowTextureCount = (ushort)count;
                    }
                    break;
                case "ShadowTextureSize":
                    int size;
                    ret = int.TryParse(s, out size);
                    if (ret)
                    {
                        ShadowTextureSize = (ushort)size;
                    }
                    break;
                case "ShadowFarDistance":
                    float dist;
                    ret = float.TryParse(s, out dist);
                    if (ret)
                    {
                        ShadowFarDistance = dist;
                    }
                    break;
                default:
                    ret = false;
                    break;
            }
            return ret;
        }

        private bool getShadowParameterHandler(string parameterName, out string parameterValue)
        {
            switch (parameterName)
            {
                case "ShadowTechnique":
                    switch (ShadowTechnique)
                    {
                        case ShadowTechnique.None:
                            parameterValue = "None";
                            break;
                        case ShadowTechnique.Simple:
                            parameterValue = "Simple";
                            break;
                        case ShadowTechnique.Depth:
                            parameterValue = "Depth";
                            break;
                        default:
                            parameterValue = "";
                            return false;
                    }
                    break;
                case "ShadowColor":
                    parameterValue = ShadowColor.To_0_255_String();
                    break;
                case "ShadowTextureCount":
                    parameterValue = ShadowTextureCount.ToString();
                    break;
                case "ShadowTextureSize":
                    parameterValue = ShadowTextureSize.ToString();
                    break;
                case "ShadowFarDistance":
                    parameterValue = ShadowFarDistance.ToString();
                    break;
                case "Help":
                    parameterValue = ShadowParameterHelp();
                    break;
                default:
                    parameterValue = "";
                    return false;
            }
            return true;
        }

        private string ShadowParameterHelp()
        {
            return
                "bool DisplayOcean: This property controls whether the client will display the automatically generated ocean; default is true" +
                "\n" +
                "bool UseParams: Set this property to false if you are using your own ocean material that uses different Vertex and Pixel shader parameters than the default Ocean shaders provided by Multiverse.; default is true" +
                "\n" +
                "Color(r,g,b,a) DeepColor: The predominate color when looking directly into the water, used to simulate a deep water effect.; default is (0,0,25,255)" +
                "\n" +
                "Color(r,g,b,a) ShallowColor:  	 The predominate color when looking at the water from an acute angle, used to simulate a shallow water effect.; default is (0,76,127, 255)" +
                "\n" +
                "float SeaLevel: The SeaLevel property allows you to set the average level of the ocean. The value is specified in millimeters.; default is 10000 (10 meters)" +
                "\n" +
                "float WaveHeight: This property lets you set the amplitude of the ocean waves. The value is specified in millimeters.; default is 1000 (1 meter)" +
                "\n" +
                "float BumpScale: This value is used to scale the normal mapping effect used to simulate smaller waves. Increasing BumpScale will make these smaller waves look taller.; default is 0.5" +
                "\n" +
                "float TextureScaleX: This value controls the scaling of the normal map texture used to generate the small wave bump effect, along the X axis.; default is 0.015625" +
                "\n" +
                "float TextureScaleZ: This value controls the scaling of the normal map texture used to generate the small wave bump effect, along the Z axis.; default is 0.0078125" +
                "\n" +
                "float BumpSpeedX: This value controls the speed at which the small wave bump effect moves along the X axis. Increasing this value will make the smaller waves move faster.; default is -0.005" +
                "\n" +
                "float BumpSpeedZ: This value controls the speed at which the small wave bump effect moves along the Z axis. Increasing this value will make the smaller waves move faster.; default is 0" +
                "\n";
        }
    }
}
