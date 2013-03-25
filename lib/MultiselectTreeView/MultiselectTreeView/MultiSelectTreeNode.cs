using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Axiom.Core;
using System.Diagnostics;


namespace Multiverse.ToolBox
{
    public class MultiSelectTreeNode : TreeNode
    {
        private bool selected = false;

        public new bool IsSelected
        {
            get { return selected; }
        }

        public MultiSelectTreeNode(string text)
            : base(text)
        {
        }

        public virtual void Select()
        {
            if (!this.IsVisible)
            {
                this.EnsureVisible();
            }
            if (selected != true)
            {
                selected = true;
            }
            if (!(this.TreeView as MultiSelectTreeView).SelectedNodes.Contains(this))
            {
                (this.TreeView as MultiSelectTreeView).AddToSelectedNodes(this);
            }
            RepaintNode();
        }


        public virtual void Select(bool add)
        {
            if (!add)
            {
                if (this.Parent != null && !this.Parent.IsExpanded)
                {
                    MultiSelectTreeNode node = this.Parent as MultiSelectTreeNode;
                    for (; node != null; node = (node.Parent as MultiSelectTreeNode))
                    {
                        node.Expand();
                    }
                }
            }
            if (selected != true)
            {
                selected = true;
            }
            if (!add)
            {
                if (!(this.TreeView as MultiSelectTreeView).SelectedNodes.Contains(this))
                {
                    (this.TreeView as MultiSelectTreeView).AddSelectedNode(this);
                }
            }
            RepaintNode();
        }
        


        public virtual void UnSelect()
        {
            if ((this.TreeView as MultiSelectTreeView).SelectedNodes.Contains(this) != false)
            {
                (this.TreeView as MultiSelectTreeView).RemoveSelectedNode(this);
                selected = false;
                RepaintNode();
            }
        }

        public virtual void UnSelect(bool remove)
        {
            if (remove)
            {
                if ((this.TreeView as MultiSelectTreeView).SelectedNodes.Contains(this) != false)
                {
                    (this.TreeView as MultiSelectTreeView).RemoveSelectedNode(this);
                }
            }
            selected = false;
            RepaintNode();
        }

        private void RepaintNode()
        {
            if (base.IsVisible)
            {
                Rectangle bound = base.Bounds; 
                bound.X = bound.X -3;
                bound.Width = bound.Width + 6;
                (base.TreeView as TreeView).Invalidate(bound);
            }
        }

    }
}
