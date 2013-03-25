using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Axiom.Core;
using System.Diagnostics;

namespace Multiverse.ToolBox
{
    public class MultiSelectTreeView : TreeView
    {
        public MultiSelectTreeView()
        {
            base.DrawMode = TreeViewDrawMode.OwnerDrawText;
        }

        private List<MultiSelectTreeNode> selectedNodes = new List<MultiSelectTreeNode>();

        public List<MultiSelectTreeNode> SelectedNodes
        {
            get
            {
                return selectedNodes;
            }
            set
            {
                if (value == null)
                {
                    selectedNodes.Clear();

                }
                else
                {
                    selectedNodes = value;
                }
            }
        }

        public void AddToSelectedNodes(MultiSelectTreeNode node)
        {
            selectedNodes.Add(node);
            TreeViewEventArgs e = new TreeViewEventArgs(node);
            OnAfterSelect(e);
        }

        public void AddSelectedNode(MultiSelectTreeNode node)
        {
            selectedNodes.Add(node);
            node.Select(false);
            TreeViewEventArgs e = new TreeViewEventArgs(node);
            OnAfterSelect(e);
        }

        public void RemoveSelectedNode(MultiSelectTreeNode node)
        {
            selectedNodes.Remove(node);
            node.UnSelect(false);
            TreeViewEventArgs e = new TreeViewEventArgs(node);
            OnAfterSelect(e);
        }

        public void ClearSelectedTreeNodes()
        {
            for (int i = selectedNodes.Count - 1; i >= 0; i--)
            {
                selectedNodes[i].UnSelect();
            }
        }

        public MultiSelectTreeNode FindNextNode(MultiSelectTreeNode node)
        {
            if (node != null)
            {
                TreeNode nodep = node.Parent;
                if (node.IsExpanded && node.Nodes.Count != 0)
                {
                    return node.Nodes[0] as MultiSelectTreeNode;
                }
                if (nodep != null)
                {
                    int i = nodep.Nodes.IndexOf(node as TreeNode);

                    if (nodep.Nodes.Count > i + 1)
                    {
                        return nodep.Nodes[i + 1] as MultiSelectTreeNode;
                    }
                    else
                    {
                        if (nodep.Parent != null)
                        {
                            return FindNextNode(nodep as MultiSelectTreeNode);
                        }
                    }
                }
            }
            return null;
        }

        public MultiSelectTreeNode FindPrevNode(MultiSelectTreeNode node)
        {
            if (node != null)
            {
                TreeNode nodep = node.Parent;
                if (nodep != null)
                {
                    int i = nodep.Nodes.IndexOf(node);
                    if (i > 0)
                    {
                        return nodep.Nodes[i - 1] as MultiSelectTreeNode;
                    }
                    else
                    {
                        return nodep as MultiSelectTreeNode;
                    }
                }
            }
            return null;
        }


        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            MultiSelectTreeNode multiNode = e.Node as MultiSelectTreeNode;
            if (multiNode == null)
            {
                e.DrawDefault = true;
            }
            else
            {
                //LogManager.Instance.Write("Enter MuliNode == null false\n");
                Font nodeFont = multiNode.NodeFont;
                if (nodeFont == null)
                {
                    //LogManager.Instance.Write("nodeFont == null");
                    nodeFont = base.Font;
                }

                Brush backBrush;
                Brush foreBrush;
                if (multiNode.IsSelected)
                {
                    //LogManager.Instance.Write("multiNode.IsSelected = true");
                    foreBrush = SystemBrushes.HighlightText;
                    backBrush = SystemBrushes.Highlight;
                }
                else
                {
                    //LogManager.Instance.Write("multiNode.IsSelected = false");
                    if (multiNode.ForeColor != Color.Empty)
                    {
                        foreBrush = new SolidBrush(multiNode.ForeColor);
                    }
                    else
                    {
                        foreBrush = new SolidBrush(multiNode.TreeView.ForeColor);
                    }
                    if (multiNode.BackColor != Color.Empty)
                    {
                        //LogManager.Instance.Write("multiNode.BackColor = r={0}, g={1}, b={2}", multiNode.BackColor.R, multiNode.BackColor.G, multiNode.BackColor.B);
                        Color backColor = multiNode.BackColor;
                        backBrush = new SolidBrush(multiNode.BackColor);
                    }
                    else
                    {
                        //LogManager.Instance.Write("multiNode.TreeView.BackColor = r={0}, g={1}, b={2}", multiNode.TreeView.BackColor.R.ToString(), multiNode.TreeView.BackColor.G.ToString(), multiNode.TreeView.BackColor.B.ToString());
                        Color backColor = multiNode.TreeView.BackColor;
                        backBrush = new SolidBrush(multiNode.TreeView.BackColor);
                    }
                }
                PointF point = new PointF(e.Bounds.X, e.Bounds.Y);
                //LogManager.Instance.Write(
                //    "e.Bounds.X = {0}, e.Bounds.y = {1} , e.Bounds.Width = {2}, e.Bounds.Height = {3}, e.Node.Text = {4}\n", e.Bounds.X,
                //    e.Bounds.Y, e.Bounds.Width, e.Bounds.Height, e.Node.Text);
                if (!String.Equals(e.Node.Text, "World: k"))
                {
                    if (e.Bounds.X == -1 && e.Bounds.Y == 0)
                    {
                        return;
                    }
                }
                e.Graphics.FillRectangle(backBrush, e.Bounds);
                StringFormat format = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
                e.Graphics.DrawString(e.Node.Text, nodeFont, foreBrush, point, format);

                //Rectangle bounds = e.Bounds;
                //bounds.X += 1;
                //e.Graphics.FillRectangle(backBrush, bounds);
                //e.Graphics.DrawString(e.Node.Text, nodeFont, foreBrush, e.Bounds.X, e.Bounds.Y);

                if ((e.State & TreeNodeStates.Focused) != 0)
                {
                    using (Pen focusPen = new Pen(Color.Black))
                    {
                        focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        Rectangle focusBounds = e.Bounds;
                        focusBounds.Size = new Size(focusBounds.Width - 1, focusBounds.Height - 1);
                        //LogManager.Instance.Write("focusBounds.X = {0}, FocusBounds.Y = {1}, e.Bounds.Height ={2}, e.Bounds.Width = {3}, focusPen.Color = {4}\n", focusBounds.X.ToString(), focusBounds.Y.ToString(), focusBounds.Height.ToString(), focusBounds.Width.ToString(), focusPen.Color.ToString());
                        e.Graphics.DrawRectangle(focusPen, focusBounds);
                        //focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        //Rectangle focusBounds = e.Bounds;
                        //focusBounds.Size = new Size(focusBounds.Width,
                        //  focusBounds.Height - 1);

                        //e.Graphics.DrawRectangle(focusPen, focusBounds);
                    }
                }

                if (!multiNode.IsSelected)
                {
                    backBrush.Dispose();
                    foreBrush.Dispose();
                }
            }
            base.OnDrawNode(e);
        }



        protected override void OnMouseClick(MouseEventArgs e)
        {
            MultiSelectTreeNode nodeHit = base.HitTest(e.X, e.Y).Node as MultiSelectTreeNode;


            if (e.Button == MouseButtons.Left)
            {
                MultiSelectTreeNode multiNode = nodeHit as MultiSelectTreeNode;

                if (multiNode != null && selectedNodes != null)
                {
                    if ((Control.ModifierKeys & Keys.Control) == 0 && selectedNodes.Count != 0)
                    {
                        for (int i = 0; i < selectedNodes.Count && selectedNodes.Count != 0; )
                        {
                            MultiSelectTreeNode node = selectedNodes[i];
                            if (node != null && node != multiNode)
                            {
                                node.UnSelect();
                            }
                            else
                            {
                                i++;
                            }
                        }
                        if (!selectedNodes.Contains(multiNode))
                        {
                            multiNode.Select();
                        }
                        else
                        {
                            multiNode.UnSelect();
                        }
                    }
                    else
                    {
                        if (multiNode.IsSelected)
                        {
                            multiNode.UnSelect();
                        }
                        else
                        {
                            multiNode.Select();
                        }
                    }

                }
            }
            if (e.Button == MouseButtons.Right)
            {
                MultiSelectTreeNode treeNode = nodeHit as MultiSelectTreeNode;
                if ((Control.ModifierKeys & Keys.Control) == 0)
                {
                    //if (SelectedObject.Count = 1)
                    //{
                    //    if (SelectedObject.Contains(treeNode.WorldObject))
                    //    {
                    //        return;
                    //    }
                    //    else
                    //    {
                    //        SelectedObject[0].Node.UnSelect();
                    //        SelectedObject.Add(treeNode.WorldObject);
                    //    }
                    //}
                    if (selectedNodes.Count >= 1)
                    {
                        if (selectedNodes.Contains(treeNode))
                        {
                            return;
                        }
                        for (int i = selectedNodes.Count - 1; i >= 0; i--)
                        {
                            selectedNodes[i].UnSelect();
                        }
                    }
                    if (selectedNodes.Count == 0)
                    {
                        treeNode.Select();
                    }
                }
                else
                {
                    return;
                }
            }
            base.OnMouseClick(e);
        }

    }
}