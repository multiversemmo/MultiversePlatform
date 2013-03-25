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
	class AddTreeCommand : ICommand
	{
		WorldEditor app;
		// bool cancelled = false;
		string filename;
        string name;
		float scale;
		float scaleVariance;
		Forest forest;
		Tree tree;
		uint instances;
		protected WorldTreeNode parentNode = null;



		public AddTreeCommand(WorldEditor worldEditor, Forest forest, float scale, float scaleVariance, string filename, string name, uint instances)
		{
			this.app = worldEditor;
			this.forest = forest;
			this.scale = scale;
			this.scaleVariance = scaleVariance;
			this.filename = filename;
            this.name = name;
			this.instances = instances;
		}

		#region ICommand Members

		public bool Undoable()
		{
			return true;
		}

		public void Execute()
		{
            if (tree == null)
            {
                tree = new Tree(name, filename, scale, scaleVariance, instances, forest, app);
            }
			this.forest.Add(tree);

            for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
            {
                app.SelectedObject[i].Node.UnSelect();
            }
            if (tree.Node != null)
            {
                tree.Node.Select();
            }
		}

		public void UnExecute()
		{
			forest.Remove(tree);
		}

		#endregion

	}
}
