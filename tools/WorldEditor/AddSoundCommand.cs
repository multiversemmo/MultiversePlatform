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
	class AddSoundCommand : ICommand
	{
		protected WorldEditor app;
		protected IWorldContainer parent;
		protected string filename;
		protected Sound sound;

		// bool cancelled = false;
		



		public AddSoundCommand(WorldEditor app, IWorldContainer parent, string filename)
		{
			this.app = app;
			this.parent = parent;
			this.filename = filename;
		}

		#region ICommand Members

		public bool Undoable()
		{
			return true;
		}

		public void Execute()
		{
			if (sound == null)
			{
				sound = new Sound(filename, parent, app);
			}
			parent.Add(sound);
            for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
            {
                app.SelectedObject[i].Node.UnSelect();
            }
            if (sound.Node != null)
            {
                sound.Node.Select();
            }
		}

		public void UnExecute()
		{
			parent.Remove(sound);
		}

		#endregion

	}
}
