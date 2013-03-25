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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Multiverse.Tools.WorldEditor
{
    public partial class SearchDialog : Form
    {
        protected TreeNodeCollection rootNodeCollection;
        protected WorldTreeNode rootNode;
        protected WorldTreeNode currentNode;
        protected bool findNext = false;
        protected int nodeCount;
        protected bool found = false;
        protected bool end = false;
        protected WorldEditor app;

        public SearchDialog(WorldTreeNode node, TreeNodeCollection collection, WorldEditor worldEditor)
        {
            this.rootNode = node;
            this.rootNodeCollection = collection;
            this.app = worldEditor;
            InitializeComponent();
        }

        private void findButton_clicked(object obj, EventArgs args)
        {
            if (end)
            {
                end = false;
                findNext = false;
            }
            found = false;
            searchTreeNode(rootNodeCollection[0] as WorldTreeNode, searchTextBox.Text, true);
        }

        private void searchTextBox_textChanged(object obj, EventArgs args)
        {
            currentNode = null;
            findNext = false;
            findButton.Text = "Find";
            nodeCount = 0;
            found = false;
            
        }

        private void searchTreeNode(WorldTreeNode parentNode, string searchQuery, bool Top)
        {
            foreach (WorldTreeNode node in parentNode.Nodes)
            {
                if (found)
                {
                    return;
                }
                if (findNext)
                {
                    if (ReferenceEquals((object)node,(object)app.SelectedNodes[0]))
                    {
                        findNext = false;
                        searchTreeNode(node, searchQuery, false);
                        if (end)
                        {
                            return;
                        }
                    }
                    else
                    {
                        searchTreeNode(node, searchQuery, false);
                    }
                    if (end)
                    {
                        return;
                    }
                    continue;
                }
                if (((node.Text).ToLower()).Contains((searchQuery.ToLower())))
                {
                    if (end)
                    {
                        return;
                    }
                    nodeCount++;
                    foreach (WorldTreeNode selectedNode in app.SelectedNodes)
                    {
                        selectedNode.UnSelect();
                    }
                    node.Select(); 
                    findNext = true;
                    findButton.Text = "Find next";
                    found = true;
                    return;
                }
                else
                {
                    nodeCount++;
                    searchTreeNode(node, searchQuery, false);
                    if (end)
                    {
                        return;
                    }
                }
            }
            int count = rootNode.GetNodeCount(true);
            if (nodeCount >= (rootNode.GetNodeCount(true)) )
            {
                nodeCount = 0;
                currentNode = null;
                findNext = false;
                findButton.Text = "Find";
                end = true;
                if (!found)
                {
                    MessageBox.Show("You reached the end and no more matches were found");
                }
                return;
            }

        }

        private void dismissButton_click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
