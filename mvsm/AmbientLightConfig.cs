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

namespace Axiom.SceneManagers.Multiverse
{
    public class AmbientLightConfig : IAnimableObject
    {
        protected SceneManager scene;

        public AmbientLightConfig(SceneManager scene)
        {
            this.scene = scene;
        }

        public ColorEx Color
        {
            get
            {
                return scene.AmbientLight;
            }
            set
            {
                scene.AmbientLight = value;
            }
        }

        #region IAnimableObject Members

        public static string[] animableAttributes = {
            "Color"
		};

        public AnimableValue CreateAnimableValue(string valueName)
        {
            switch (valueName)
            {
                case "Color":
                    return new ColorValue(this);
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


        protected class ColorValue : AnimableValue
        {
            protected AmbientLightConfig config;
            public ColorValue(AmbientLightConfig config)
                : base(AnimableType.ColorEx)
            {
                this.config = config;
                SetAsBaseValue(ColorEx.Black);
            }

            public override void SetValue(ColorEx val)
            {
                config.Color = val;
            }

            public override void ApplyDeltaValue(ColorEx val)
            {
                SetValue(new ColorEx(config.Color.r + val.r, config.Color.g + val.g, config.Color.b + val.b));
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(config.Color);
            }
        }

        #endregion
    }
}
