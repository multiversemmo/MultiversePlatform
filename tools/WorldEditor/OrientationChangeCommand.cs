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
using Axiom.Graphics;

namespace Multiverse.Tools.WorldEditor
{
    public class OrientationChangeCommand : ICommand
    {
        protected WorldEditor app;
        protected IObjectOrientation objOrient;
        protected float oldAzimuth;
        protected float oldZenith;
        protected float lightAzimuth;
        protected float lightZenith;
            
            
        public OrientationChangeCommand(WorldEditor worldEditor, IObjectOrientation objOri, float lightAzi, float lightZen, float oldAzi, float oldZen)
        {

            while (lightAzi > 180 || lightAzi < -180)
            {
                if (lightAzi > 180)
                {
                    lightAzi -= 360;
                }
                else
                {
                    if (lightAzi < -180)
                    {
                        lightAzi += 360;
                    }
                }
            }
            while (lightZen > 90 || lightZen < -90)
            {
                if (lightZen > 90)
                {
                    lightZen -= 180;
                }
                else
                {
                    if (lightZen < -90)
                    {
                        lightZen += 180;
                    }
                }
            }
            this.app = worldEditor;
            this.objOrient = objOri;
            this.lightAzimuth = lightAzi;
            this.lightZenith = lightZen;
            this.oldAzimuth = oldAzi;
            this.oldZenith = oldZen;
        }

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            objOrient.SetDirection(lightAzimuth, lightZenith);
            app.UpdateOrientationPanel(objOrient);
            switch ((objOrient as IWorldObject).ObjectType)
            {
                case "DirectionalLight":
                    (objOrient as DirectionalLight).OrigAzimuth = lightAzimuth;
                    (objOrient as DirectionalLight).OrigZenith = lightZenith;
                    break;
                case "GlobalDirectionalLight":
                    (objOrient as GlobalDirectionalLight).OrigAzimuth = lightAzimuth;
                    (objOrient as GlobalDirectionalLight).OrigZenith = lightZenith;
                    break;
                default:
                    break;
            }
        }

        public void UnExecute()
        {
            objOrient.SetDirection(oldAzimuth, oldZenith);
            app.UpdateOrientationPanel(objOrient);
            switch ((objOrient as IWorldObject).ObjectType)
            {
                case "DirectionalLight":
                    (objOrient as DirectionalLight).OrigAzimuth = oldAzimuth;
                    (objOrient as DirectionalLight).OrigZenith = oldZenith;
                    break;
                case "GlobalDirectionalLight":
                    (objOrient as GlobalDirectionalLight).OrigAzimuth = oldAzimuth;
                    (objOrient as GlobalDirectionalLight).OrigZenith = oldZenith;
                    break;
                default:
                    break;
            }
        }
    }
}
