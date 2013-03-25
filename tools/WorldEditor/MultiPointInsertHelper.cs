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
    public delegate void MultiPointInsertComplete(List<Vector3> points);
    public delegate bool MultiPointInsertValidate(List<Vector3> points, Vector3 location, int index);

    public class MultiPointInsertHelper
    {
        protected List<Vector3> points;
        protected MultiPointInsertComplete completeCallback;
        protected MultiPointInsertValidate validateCallback;
        protected DisplayObject dragObject;
        protected WorldEditor app;
        protected int index;

        protected DragHelper dragHelper;

        /// <summary>
        /// This overload is used when adding points to an existing Collection.
        /// </summary>
        /// <param name="worldEditor"></param>
        /// <param name="displayObject"></param>
        /// <param name="validate"></param>
        /// <param name="Complete"></param>
        /// <param name="points"></param>
        public MultiPointInsertHelper(WorldEditor worldEditor, DisplayObject displayObject, MultiPointInsertValidate validate, MultiPointInsertComplete complete, List<Vector3> points, int index)
        {
            app = worldEditor;
            dragObject = displayObject;
            completeCallback = complete;
            validateCallback = validate;
            this.points = points;
            this.index = index;
            dragHelper = new DragHelper(app, dragObject, new DragComplete(DragCallback));
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
                    valid = validateCallback(points, location, index);
                }
                if (valid)
                {
                    // point is valid, add it to the collection
                    points.Insert(index, location);
                    index++;
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
