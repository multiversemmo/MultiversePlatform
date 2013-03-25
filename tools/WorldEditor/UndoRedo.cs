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

    public class UndoRedo
    { 
        protected Stack<ICommand> undoStack;
        protected Stack<ICommand> redoStack;
        protected ICommand autoSaveHead;
        protected ICommand head;

        public UndoRedo()
        {
            undoStack = new Stack<ICommand>();
            redoStack = new Stack<ICommand>();
            head = null;
        }

        public void ClearUndoRedo()
        {
            undoStack.Clear();
            redoStack.Clear();
            head = null;
        }

        public void PushCommand(ICommand cmd)
        {
            redoStack.Clear();
            undoStack.Push(cmd);
        }

        public void ResetDirty()
        {
            if (undoStack.Count != 0)
            {
                head = undoStack.Peek();
                autoSaveHead = undoStack.Peek();
            }
            else
            {
                head = null;
                autoSaveHead = null;
            }
        }

        public void ResetAutoSaveDirty()
        {
            if (undoStack.Count != 0)
            {
                autoSaveHead = undoStack.Peek();
            }
            else 
            {
                autoSaveHead = null;
            }
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                ICommand cmd = undoStack.Pop();
                if (cmd != null)
                {
                    cmd.UnExecute();
                    redoStack.Push(cmd);
                }
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                ICommand cmd = redoStack.Pop();
                if (cmd != null)
                {
                    cmd.Execute();
                    undoStack.Push(cmd);
                    head = undoStack.Peek();
                }
            }
        }

        public bool CanRedo
        {
            get
            {
                return redoStack.Count != 0;
            }
        }

        public bool CanUndo
        {
            get
            {
                return undoStack.Count != 0;
            }
        }

        public bool Dirty
        {
            get
            {
                if ((undoStack.Count == 0 && head == null) || (undoStack.Count > 0 && Object.ReferenceEquals((object)(undoStack.Peek()),(object)head)))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool AutoSaveDirty
        {
            get
            {
                if ((undoStack.Count == 0 && head == null) || Object.ReferenceEquals((object)(undoStack.Peek()),(object)autoSaveHead))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
