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
using Axiom.Animating;
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.SceneManagers.Multiverse
{
    /// <summary>
    /// This class is used for scripting and animating the scene fog in the client.
    /// </summary>
    public class FogConfig: IAnimableObject
    {
        protected SceneManager scene;

        public FogConfig(SceneManager scene)
        {
            this.scene = scene;
        }

        /// <summary>
        ///		Gets the fog mode that was set during the last call to SetFog.
        /// </summary>
        public FogMode FogMode
        {
            get
            {
                return scene.FogMode;
            }
            set
            {
                scene.FogMode = value;
            }
        }

        /// <summary>
        ///		Gets the fog starting point that was set during the last call to SetFog.
        /// </summary>
        public float FogNear
        {
            get
            {
                return scene.FogStart;
            }
            set
            {
                scene.FogStart = value;
            }
        }

        /// <summary>
        ///		Gets the fog ending point that was set during the last call to SetFog.
        /// </summary>
        public float FogFar
        {
            get
            {
                return scene.FogEnd;
            }
            set
            {
                scene.FogEnd = value;
            }
        }

        /// <summary>
        ///		Gets the fog density that was set during the last call to SetFog.
        /// </summary>
        public float FogDensity
        {
            get
            {
                return scene.FogDensity;
            }
            set
            {
                scene.FogDensity = value;
            }
        }

        /// <summary>
        ///		Gets the fog color that was set during the last call to SetFog.
        /// </summary>
        public virtual ColorEx FogColor
        {
            get
            {
                return scene.FogColor;
            }
            set
            {
                scene.FogColor = value;
            }
        }

        #region IAnimableObject Members

        public static string[] animableAttributes = {
            "FogNear",
            "FogFar",
            "FogColor"
		};

        public AnimableValue CreateAnimableValue(string valueName)
        {
            switch (valueName)
            {
                case "FogNear":
                    return new FogNearValue(this);
                case "FogFar":
                    return new FogFarValue(this);
                case "FogColor":
                    return new FogColorValue(this);
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

        protected class FogNearValue : AnimableValue
        {
            protected FogConfig fogConfig;
            public FogNearValue(FogConfig fogConfig)
                : base(AnimableType.Float)
            {
                this.fogConfig = fogConfig;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                fogConfig.FogNear = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(fogConfig.FogNear + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(fogConfig.FogNear);
            }
        }

        protected class FogFarValue : AnimableValue
        {
            protected FogConfig fogConfig;
            public FogFarValue(FogConfig fogConfig)
                : base(AnimableType.Float)
            {
                this.fogConfig = fogConfig;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                fogConfig.FogFar = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(fogConfig.FogFar + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(fogConfig.FogFar);
            }
        }

        protected class FogColorValue : AnimableValue
        {
            protected FogConfig fogConfig;
            public FogColorValue(FogConfig fogConfig)
                : base(AnimableType.ColorEx)
            {
                this.fogConfig = fogConfig;
                SetAsBaseValue(ColorEx.Black);
            }

            public override void SetValue(ColorEx val)
            {
                fogConfig.FogColor = val;
            }

            public override void ApplyDeltaValue(ColorEx val)
            {
                SetValue(new ColorEx(fogConfig.FogColor.r + val.r, fogConfig.FogColor.g + val.g, fogConfig.FogColor.b + val.b));
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(fogConfig.FogColor);
            }
        }

        #endregion
    }
}
