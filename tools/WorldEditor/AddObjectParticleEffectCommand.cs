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

namespace Multiverse.Tools.WorldEditor
{
    public class AddObjectParticleEffectCommand : ICommand
    {
        protected StaticObject parent;
        protected WorldEditor app;
        protected string particleEffectName;
        protected float positionScale;
        protected float velocityScale;
        protected ParticleEffect particleEffect;
        protected string attachmentPointName;
        protected Quaternion orientation;

        public AddObjectParticleEffectCommand(StaticObject parent, WorldEditor app, string particleEffectName, float positionScale, float velocityScale, string attachmentPointName, Quaternion orientation)
        {
            this.app = app;
            this.parent = parent;
            this.particleEffectName = particleEffectName;
            this.positionScale = positionScale;
            this.velocityScale = velocityScale;
            this.attachmentPointName = attachmentPointName;
            this.orientation = orientation;
        }

        #region ICommand Members

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            if (particleEffect == null)
            {
                particleEffect = new ParticleEffect(parent, app, particleEffectName, positionScale, velocityScale, attachmentPointName, orientation);
            }

            parent.Add(particleEffect);
            for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
            {
                app.SelectedObject[i].Node.UnSelect();
            }
            if (particleEffect.Node != null)
            {
                particleEffect.Node.Select();
            }
        }

        public void UnExecute()
        {
            parent.Remove(particleEffect);
        }

        #endregion
    }
}
