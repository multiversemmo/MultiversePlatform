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
using System.Diagnostics;
using Axiom.Core;
using Axiom.Animating;


namespace Axiom.SceneManagers.Multiverse
{
    public delegate void ConfigChangeHandler(object sender, EventArgs e);

    public class OceanConfig : IAnimableObject
    {
        protected bool showOcean = true;
        protected float seaLevel = 10 * TerrainManager.oneMeter;
        protected float waveHeight = 1 * TerrainManager.oneMeter;
        protected ColorEx deepColor = new ColorEx(1, 0, 0, 0.1f);
        protected ColorEx shallowColor = new ColorEx(1, 0, 0.3f, 0.5f);
        protected float bumpScale = 0.5f;
        protected float textureScaleX = 0.015625f;
        protected float textureScaleZ = 0.0078125f;
        protected float bumpSpeedX = -0.005f;
        protected float bumpSpeedZ = 0;
        protected bool useParams = true;

        public event ConfigChangeHandler ConfigChange;

        public OceanConfig()
        {

        }

        protected void OnConfigChange()
        {
            ConfigChangeHandler handler = ConfigChange;
            if (handler != null)
            {
                handler(null, new EventArgs());
            }
        }

        public bool UseParams
        {
            get
            {
                return useParams;
            }
            set
            {
                if (useParams != value)
                {
                    useParams = value;
                    OnConfigChange();
                }
            }
        }

        public bool ShowOcean
        {
            get
            {
                return showOcean;
            }
            set
            {
                if (showOcean != value)
                {
                    showOcean = value;
                    OnConfigChange();
                }
            }
        }

        public float SeaLevel
        {
            get
            {
                return seaLevel;
            }
            set
            {
                if (seaLevel != value)
                {
                    seaLevel = value;
                    OnConfigChange();
                }
            }
        }

        public float WaveHeight
        {
            get
            {
                return waveHeight;
            }
            set
            {
                if (waveHeight != value)
                {
                    waveHeight = value;
                    OnConfigChange();
                }
            }
        }

        public ColorEx DeepColor
        {
            get
            {
                return deepColor;
            }
            set
            {
                if (deepColor != value)
                {
                    deepColor = value;
                    OnConfigChange();
                }
            }
        }

        public ColorEx ShallowColor
        {
            get
            {
                return shallowColor;
            }
            set
            {
                if (shallowColor != value)
                {
                    shallowColor = value;
                    OnConfigChange();
                }
            }
        }

        public float BumpScale
        {
            get
            {
                return bumpScale;
            }
            set
            {
                if (bumpScale != value)
                {
                    bumpScale = value;
                    OnConfigChange();
                }
            }
        }

        public float BumpSpeedX
        {
            get
            {
                return bumpSpeedX;
            }
            set
            {
                if (bumpSpeedX != value)
                {
                    bumpSpeedX = value;
                    OnConfigChange();
                }
            }
        }

        public float BumpSpeedZ
        {
            get
            {
                return bumpSpeedZ;
            }
            set
            {
                if (bumpSpeedZ != value)
                {
                    bumpSpeedZ = value;
                    OnConfigChange();
                }
            }
        }

        public float TextureScaleX
        {
            get
            {
                return textureScaleX;
            }
            set
            {
                if (textureScaleX != value)
                {
                    textureScaleX = value;
                    OnConfigChange();
                }
            }
        }

        public float TextureScaleZ
        {
            get
            {
                return textureScaleZ;
            }
            set
            {
                if (textureScaleZ != value)
                {
                    textureScaleZ = value;
                    OnConfigChange();
                }
            }
        }

        #region IAnimableObject Members

        public static string[] animableAttributes = {
            "SeaLevel",
            "WaveHeight",
            "DeepColor",
            "ShallowColor",
            "BumpScale",
            "BumpSpeedX",
            "BumpSpeedZ",
            "TextureScaleX",
            "TextureScaleZ"
		};

        public AnimableValue CreateAnimableValue(string valueName)
        {
            switch (valueName)
            {
                case "SeaLevel":
                    return new SeaLevelValue(this);
                case "WaveHeight":
                    return new WaveHeightValue(this);
                case "DeepColor":
                    return new DeepColorValue(this);
                case "ShallowColor":
                    return new ShallowColorValue(this);
                case "BumpScale":
                    return new BumpScaleValue(this);
                case "BumpSpeedX":
                    return new BumpSpeedXValue(this);
                case "BumpSpeedZ":
                    return new BumpSpeedZValue(this);
                case "TextureScaleX":
                    return new TextureScaleXValue(this);
                case "TextureScaleZ":
                    return new TextureScaleZValue(this);
            }
            throw new Exception(string.Format("Could not find animable attribute '{0}'", valueName));
        }

        public string[] AnimableValueNames
        {
            get
            {
                return animableAttributes;
            }
        }


        protected class SeaLevelValue : AnimableValue
        {
            protected OceanConfig ocean;
            public SeaLevelValue(OceanConfig ocean)
                : base(AnimableType.Float)
            {
                this.ocean = ocean;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                ocean.SeaLevel = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(ocean.SeaLevel + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(ocean.SeaLevel);
            }
        }

        protected class WaveHeightValue : AnimableValue
        {
            protected OceanConfig ocean;
            public WaveHeightValue(OceanConfig ocean)
                : base(AnimableType.Float)
            {
                this.ocean = ocean;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                ocean.WaveHeight = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(ocean.WaveHeight + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(ocean.WaveHeight);
            }
        }

        protected class DeepColorValue : AnimableValue
        {
            protected OceanConfig ocean;
            public DeepColorValue(OceanConfig ocean)
                : base(AnimableType.ColorEx)
            {
                this.ocean = ocean;
                SetAsBaseValue(ColorEx.Black);
            }

            public override void SetValue(ColorEx val)
            {
                ocean.DeepColor = val;
            }

            public override void ApplyDeltaValue(ColorEx val)
            {
                SetValue(new ColorEx(ocean.DeepColor.r + val.r, ocean.DeepColor.g + val.g, ocean.DeepColor.b + val.b));
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(ocean.DeepColor);
            }
        }

        protected class ShallowColorValue : AnimableValue
        {
            protected OceanConfig ocean;
            public ShallowColorValue(OceanConfig ocean)
                : base(AnimableType.ColorEx)
            {
                this.ocean = ocean;
                SetAsBaseValue(ColorEx.Black);
            }

            public override void SetValue(ColorEx val)
            {
                ocean.ShallowColor = val;
            }

            public override void ApplyDeltaValue(ColorEx val)
            {
                SetValue(new ColorEx(ocean.ShallowColor.r + val.r, ocean.ShallowColor.g + val.g, ocean.ShallowColor.b + val.b));
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(ocean.ShallowColor);
            }
        }

        protected class BumpScaleValue : AnimableValue
        {
            protected OceanConfig ocean;
            public BumpScaleValue(OceanConfig ocean)
                : base(AnimableType.Float)
            {
                this.ocean = ocean;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                ocean.BumpScale = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(ocean.BumpScale + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(ocean.BumpScale);
            }
        }

        protected class BumpSpeedXValue : AnimableValue
        {
            protected OceanConfig ocean;
            public BumpSpeedXValue(OceanConfig ocean)
                : base(AnimableType.Float)
            {
                this.ocean = ocean;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                ocean.BumpSpeedX = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(ocean.BumpSpeedX + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(ocean.BumpSpeedX);
            }
        }

        protected class BumpSpeedZValue : AnimableValue
        {
            protected OceanConfig ocean;
            public BumpSpeedZValue(OceanConfig ocean)
                : base(AnimableType.Float)
            {
                this.ocean = ocean;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                ocean.BumpSpeedZ = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(ocean.BumpSpeedZ + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(ocean.BumpSpeedZ);
            }
        }

        protected class TextureScaleXValue : AnimableValue
        {
            protected OceanConfig ocean;
            public TextureScaleXValue(OceanConfig ocean)
                : base(AnimableType.Float)
            {
                this.ocean = ocean;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                ocean.TextureScaleX = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(ocean.TextureScaleX + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(ocean.TextureScaleX);
            }
        }


        protected class TextureScaleZValue : AnimableValue
        {
            protected OceanConfig ocean;
            public TextureScaleZValue(OceanConfig ocean)
                : base(AnimableType.Float)
            {
                this.ocean = ocean;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                ocean.TextureScaleZ = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(ocean.TextureScaleZ + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(ocean.TextureScaleZ);
            }
        }
        #endregion
    }
}
