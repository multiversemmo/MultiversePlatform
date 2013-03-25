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
    public delegate void MultiPointComplete(List<Vector3> points);
    public delegate bool MultiPointValidate(List<Vector3> points,  Vector3 location);

    public class MultiPointPlacementHelper
    {
        protected List<Vector3> points;
        protected MultiPointComplete completeCallback;
        protected MultiPointValidate validateCallback;
        protected DisplayObject dragObject;
        protected WorldEditor app;

        protected DragHelper dragHelper;


        /// <summary>
        /// Use this overload for placing points for roads and boundaries 
        /// </summary>
        /// <param name="worldEditor"></param>
        /// <param name="displayObject"></param>
        /// <param name="validate"></param>
        /// <param name="complete"></param>
        public MultiPointPlacementHelper(WorldEditor worldEditor, DisplayObject displayObject, MultiPointValidate validate, MultiPointComplete complete)
        {
            app = worldEditor;
            dragObject = displayObject;
            completeCallback = complete;
            validateCallback = validate;

            points = new List<Vector3>();

            
            dragHelper = new DragHelper(app, dragObject, new DragComplete(DragCallback));
        }


        /// <summary>
        /// Use this overload to place multiple Static Objects.  It allows the static objects placed to be placed on other objects.
        /// </summary>
        /// <param name="worldEditor"></param>
        /// <param name="displayObject"></param>
        /// <param name="validate"></param>
        /// <param name="complete"></param>
        public MultiPointPlacementHelper(WorldEditor worldEditor, MultiPointValidate validate, DisplayObject displayObject, MultiPointComplete complete)
        {
            app = worldEditor;
            dragObject = displayObject;
            completeCallback = complete;
            validateCallback = validate;

            points = new List<Vector3>();


            dragHelper = new DragHelper(app, new DragComplete(DragCallback), dragObject);
        }

        public bool DragCallback(bool accept, Vector3 location)
        {
            // normally we return false so that the draghelper continues to drag the object around.
            bool ret = false;
            if (accept)
            {
                // user is trying to place a point.  call validation callback.

                bool valid = true;
                if (validateCallback != null)
                {
                    valid = validateCallback(points, location);
                }
                if (valid)
                {
                    // point is valid, add it to the collection
                    points.Add(location);
                }
            }
            else
            {
                // user clicked right button, so they are done adding points
                if (completeCallback != null)
                {
                    completeCallback(points);
                }

                ret = true;
            }

            return ret;
        }
    }
}
