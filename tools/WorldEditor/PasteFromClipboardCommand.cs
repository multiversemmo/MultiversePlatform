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
    public class PasteFromClipboardCommand : ICommand, IWorldContainer
    {
        protected List<IWorldContainer> parents = new List<IWorldContainer>();

        protected List<IWorldObject> worldObjects = new List<IWorldObject>();
        protected List<IObjectCutCopy> pasteObjects = new List<IObjectCutCopy>();
        protected DragCollection dragObjects = new DragCollection();
        protected List<Vector3> dragOffset = new List<Vector3>();
        protected List<float> terrainOffset = new List<float>();
        protected ClipboardObject clip = WorldEditor.Instance.Clipboard;
        protected bool placing = true;
        protected WorldEditor app;
        protected int numPaste;

        public PasteFromClipboardCommand(WorldEditor worldEditor)
        {
            app = worldEditor;
            numPaste = clip.NumPaste;
            int i = 0;

            foreach (IWorldObject obj in app.Clipboard)
            {
                if(app.SelectedObject.Count == 1 && app.SelectedObject[0] is WorldObjectCollection && !(app.SelectedObject[0] is WorldRoot))
                {
                    parents.Add(app.SelectedObject[0] as IWorldContainer);
                }
                else
                {
                    parents.Add(clip.Parents[i]);
                }
                obj.Clone(this as IWorldContainer);
                pasteObjects.Add(worldObjects[i] as IObjectCutCopy);
                i++;
            }
        }

        #region ICommand Members

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            if (placing)
            {
                numPaste = app.Clipboard.NumPaste;
                app.Clipboard.IncrementNumPaste();
                int pasteNum;
                if (clip.State == ClipboardState.copy || (clip.State == ClipboardState.cut && numPaste > 0))
                {
                    foreach (IObjectCutCopy obj in pasteObjects)
                    {
                        if ((numPaste > 0 && clip.State == ClipboardState.copy) || (numPaste > 1 && clip.State == ClipboardState.cut))
                        {
                            if (clip.State == ClipboardState.cut)
                            {
                                pasteNum = numPaste - 1;
                            }
                            else
                            {
                                pasteNum = numPaste;
                            }
                            (obj as IObjectCutCopy).Name = string.Format("{0} Copy of {1}", pasteNum, (obj as IObjectCutCopy).Name);
                        }
                        else
                        {
                            if((clip.State == ClipboardState.copy && numPaste == 0) || (clip.State == ClipboardState.cut && numPaste == 1))
                            {
                                (obj as IObjectCutCopy).Name = string.Format("Copy of {0}", (obj as IObjectCutCopy).Name);
                            }
                        }
                    }
                }
                Vector3 baseObjPosition = pasteObjects[0].Position;
                foreach (IObjectCutCopy obj in pasteObjects)
                {
                    dragOffset.Add(obj.Position - baseObjPosition);
                    switch (obj.ObjectType)
                    {
                        case "Road":
                            ((RoadObject)obj).Points.Clone(dragObjects as IWorldContainer);
                            terrainOffset.Add(0f);
                            break;
                        case "Region":
                            ((Boundary)obj).Points.Clone(dragObjects as IWorldContainer);
                            terrainOffset.Add(0f);
                            break;
                        case "TerrainDecal":
                            dragObjects.Add(obj as IWorldObject);
                            terrainOffset.Add(0f);
                            break;
                        case "Marker":
                        case "Object":
                        case "PointLight":
                            dragObjects.Add(obj as IWorldObject);
                            if (obj.AllowAdjustHeightOffTerrain)
                            {
                                terrainOffset.Add((obj as IObjectDrag).TerrainOffset);
                            }
                            else
                            {
                                terrainOffset.Add(0f);
                            }
                            break;
                     }
                }
                new DragHelper(app, dragObjects.DragList, dragOffset, terrainOffset, DragCallback, false);
            }
            else
            {
                int i = 0;
                foreach(IObjectCutCopy obj in pasteObjects)
                {
                    obj.Parent = parents[i];
                    obj.Parent.Add(obj);
                    i++;
                }
            }
        }


        private bool DragCallback(bool accept, Vector3 loc)
        {
            placing = false;
            int i = 0;
            foreach (IObjectDrag obj in dragObjects.DragList)
            {
                (obj as IWorldObject).RemoveFromScene();
                pasteObjects[i].Position = loc + dragOffset[i];
                if (pasteObjects[i].AllowAdjustHeightOffTerrain)
                {
                    pasteObjects[i].Position = new Vector3(obj.Position.x, app.GetTerrainHeight(obj.Position.x, obj.Position.z) + 
                        terrainOffset[i], obj.Position.z);
                }
                else
                {
                    if (pasteObjects[i] is Waypoint || pasteObjects[i] is StaticObject || pasteObjects[i] is PointLight)
                    {
                        pasteObjects[i].Position = new Vector3(obj.Position.x, pasteObjects[i].Position.y, obj.Position.z);
                    }
                    else
                    {
                        pasteObjects[i].Position = new Vector3(obj.Position.x, app.GetTerrainHeight(obj.Position.x, obj.Position.z),
                            obj.Position.z);
                    }
                }
                switch (obj.ObjectType)
                {
                    case "Points":
                        (obj as PointCollection).Dispose();
                        break;
                }
                parents[i].Add(pasteObjects[i]);
                pasteObjects[i].Parent = parents[i];
                i++;
            }
            return true;
        }

        public void UnExecute()
        {
            foreach (IObjectCutCopy obj in pasteObjects)
            {
                obj.Parent.Remove(obj);
            }
        }

        #endregion


        #region IWorldContainer Members

        public void Add(IWorldObject item)
        {
            worldObjects.Add(item);
        }

        public bool Remove(IWorldObject item)
        {
            return worldObjects.Remove(item);
        }

        #endregion

        #region ICollection<IWorldObject> Members


        public void Clear()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Contains(IWorldObject item)
        {
            foreach (IWorldObject obj in worldObjects)
            {
                if (ReferenceEquals(item, obj))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(IWorldObject[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count
        {
            get
            {
                return worldObjects.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region IEnumerable<IWorldObject> Members

        public IEnumerator<IWorldObject> GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
