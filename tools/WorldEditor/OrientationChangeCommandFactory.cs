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

namespace Multiverse.Tools.WorldEditor
{
    public class OrientationChangeCommandFactory : ICommandFactory
    {
        WorldEditor app;
        IObjectOrientation obj;
        float origAzimuth;
        float origZenith;
        float azimuth = 0.0f;
        float zenith = 0.0f;


        public OrientationChangeCommandFactory(WorldEditor worldEditor, IObjectOrientation obj, float azimuth, float zenith)
        {
            app = worldEditor;
            this.obj = obj;
            switch ((obj as IWorldObject).ObjectType)
            {
                case "DirectionalLight":
                    origAzimuth = (obj as DirectionalLight).OrigAzimuth;
                    origZenith = (obj as DirectionalLight).OrigZenith;
                    break;
                case "GlobalDirectionalLight":
                    origAzimuth = (obj as GlobalDirectionalLight).OrigAzimuth;
                    origZenith = (obj as GlobalDirectionalLight).OrigZenith;
                    break;
                default:
                    origAzimuth = obj.Azimuth;
                    origZenith = obj.Zenith;
                    break;
            }
            
        }


        #region ICommandFactory Members

        public ICommand  CreateCommand()
        {
            ICommand cmd = new OrientationChangeCommand(app, obj, azimuth, zenith, origAzimuth, origZenith);
            return cmd;
        }

        #endregion
    }
}
