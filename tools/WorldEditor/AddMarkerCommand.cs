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
using System.Windows.Forms;
using Axiom.Core;

namespace Multiverse.Tools.WorldEditor
{
	class AddMarkerCommand : ICommand
	{
		WorldEditor app;
		IWorldContainer parent;
		String name;
		String meshName;
		float rotation = 0f;
		bool placing;
		Vector3 location;
		Waypoint waypoint;
		DisplayObject dragObject;
		bool cancelled = false;
		// SubMeshCollection subMeshes;

		public AddMarkerCommand(WorldEditor worldEditor, IWorldContainer parentObject, String objectName, string meshNameIn)
		{
			this.app = worldEditor;
			this.parent = parentObject;
			this.name = objectName;
			this.meshName = meshNameIn;
			placing = true;

		}

		#region ICommand Members

		public bool Undoable()
		{
			return true;
		}

		public void Execute()
		{
			if (!cancelled)
			{
                float scale = app.Config.MarkerPointScale;
				Vector3 scaleVec = new Vector3(scale, scale, scale);
				Vector3 rotVec = new Vector3(0, rotation, 0);
				if (placing)
				{
					// we need to place the object (which is handled asynchronously) before we can create it
					dragObject = new DisplayObject(name, app,"Drag", app.Scene, meshName, location, scaleVec, rotVec, null);
					dragObject.MaterialName = "directional_marker.orange";
                    dragObject.ScaleWithCameraDistance = true;
					// set up mouse capture and callbacks for placing the object
                    new DragHelper(app, new DragComplete(DragCallback), dragObject);
				}
				else
				{
                    if (waypoint == null)
                    {
                        // object has already been placed, so create it now
                        waypoint = new Waypoint(name, parent, app, location, rotVec);
                    }
                    parent.Add(waypoint);
                    for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
                    {
                        app.SelectedObject[i].Node.UnSelect();
                    }
                    if (waypoint.Node != null)
                    {
                        waypoint.Node.Select();
                    }
				}
			}
		}

		public void UnExecute()
		{
            parent.Remove(waypoint);
		}

		#endregion

		private bool DragCallback(bool accept, Vector3 loc)
		{
			placing = false;

			dragObject.Dispose();

			if (accept)
			{
				location = loc;
				Vector3 rotVec = new Vector3(0, rotation, 0);
				waypoint = new Waypoint(name, parent, app, location, rotVec);
				parent.Add(waypoint);

                for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
                {
                    app.SelectedObject[i].Node.UnSelect();
                }
                if (waypoint.Node != null)
                {
                    waypoint.Node.Select();
                }
			}
			else
			{
				cancelled = true;
			}

			return true;
		}
	}
}
