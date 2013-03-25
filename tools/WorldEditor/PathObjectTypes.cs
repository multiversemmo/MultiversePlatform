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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.ComponentModel;
using System.IO;
using Axiom.MathLib;
using Multiverse.CollisionLib;

namespace Multiverse.Tools.WorldEditor
{
    public class PathObjectTypeContainer : IWorldObject
    {
        protected WorldEditor app;
        protected IWorldObject parent;
        protected WorldTreeNode node;
        protected WorldTreeNode parentNode;
        protected List<PathObjectTypeNode> pathObjectTypes = new List<PathObjectTypeNode>();
        protected bool inTree = false;
        protected bool allObjectsDirty = false;
        protected List<ToolStripButton> buttonBar;

        public PathObjectTypeContainer(IWorldObject parent, WorldEditor app)
        {
            this.app = app;
            this.parent = parent;
        }

        public void Add(PathObjectTypeNode pathObjectType) {
            pathObjectTypes.Add(pathObjectType);
            pathObjectType.AddToTree(node);
        }
        
        public void Remove(PathObjectTypeNode pathObjectType) {
            pathObjectTypes.Remove(pathObjectType);
            pathObjectType.RemoveFromTree();
        }
        
        public int Count {
            get { return pathObjectTypes.Count; }
        }
        
        public PathObjectType GetType(int index) {
            return pathObjectTypes[index].PathObjectType;
        }
        
        public PathObjectTypeContainer(WorldEditor app, IWorldObject parent, XmlReader r)
        {
            this.app = app;
            this.parent = parent;
            FromXml(r);
        }

        public bool AllObjectsDirty {
            get { return allObjectsDirty; }
            set { allObjectsDirty = value; }
        }

        protected void FromXml(XmlReader r)
        {
            pathObjectTypes = new List<PathObjectTypeNode>();
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Whitespace)
                    continue;
                else if (r.NodeType == XmlNodeType.EndElement)
					break;
                else if (r.NodeType == XmlNodeType.Element)
                {
                    if (r.Name == "PathObjectType")
                        pathObjectTypes.Add(new PathObjectTypeNode(app, this, r));
                }
            }
        }

        public void Clone(IWorldContainer copyParent)
        {
        }

        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;

            // add the Creature Classes node
            node = app.MakeTreeNode(this, "Path Object Types");
            parentNode.Nodes.Add(node);

            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Add Path Object Type", new AddPathObjectTypeCommandFactory(app, this), app.DefaultCommandClickHandler);
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            node.ContextMenuStrip = menuBuilder.Menu;

            foreach (PathObjectTypeNode pathObjectType in pathObjectTypes)
                pathObjectType.AddToTree(node);

            inTree = true;
            buttonBar = menuBuilder.ButtonBar;
        }

        [BrowsableAttribute(false)]
        public List<ToolStripButton> ButtonBar
        {
            get
            {
                return buttonBar;
            }
        }

        public bool IsGlobal
        {
            get
            {
                return true;
            }
        }

        [BrowsableAttribute(false)]
        public bool WorldViewSelectable
        {
            get
            {
                return false;
            }
            set
            {
                // this property is not applicable to this object
            }
        }

        public bool IsTopLevel
        {
            get
            {
                return false;
            }
        }


        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString +=  String.Format("\tCount={0}\r\n", Count);
                objString +=  "\r\n";
                return objString;
            }
        }

        public void RemoveFromTree()
        {
            if (node.IsSelected)
            {
                node.UnSelect();
            }
            parentNode.Nodes.Remove(node);
            parentNode = null;
            node = null;
            inTree = false;
        }

		public void ToXml(XmlWriter w)
        {
			if (pathObjectTypes.Count > 0) {
                w.WriteStartElement("PathObjectTypes");
                foreach (PathObjectTypeNode pathObjectType in pathObjectTypes)
                    pathObjectType.ToXml(w);
                w.WriteEndElement();
            }
        }

		[BrowsableAttribute(false)]
		public WorldTreeNode Node
		{
			get
			{
				return node;
			}
		}

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "PathObjectTypes";
            }
        }

		public void AddToScene()
		{
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
        }
        
		public void RemoveFromScene()
		{
		}

        public void CheckAssets()
        {
        }

        public void ToManifest(System.IO.StreamWriter w)
        {
        }

		[BrowsableAttribute(false)]
		public Vector3 FocusLocation
		{
			get
			{
				return parent.FocusLocation;
			}
		}

		[BrowsableAttribute(false)]
		public bool Highlight
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

        [BrowsableAttribute(false)]
        public bool AcceptObjectPlacement
        {
            get
            {
                return false;
            }
            set
            {
                //not implemented for this type of object
            }
        }

        #endregion IWorldObject Members

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

    public class AddPathObjectTypeCommandFactory : ICommandFactory
    {
        WorldEditor app;
        PathObjectTypeContainer parent;

        public AddPathObjectTypeCommandFactory(WorldEditor app, PathObjectTypeContainer parent)
        {
            this.app = app;
            this.parent = parent;
        }

        #region ICommandFactory Members

        public ICommand CreateCommand()
        {
            PathObjectTypeDialog dialog = new PathObjectTypeDialog();
            if (dialog.ShowDialog() == DialogResult.OK) {
                ICommand cmd = new AddPathObjectTypeCommand(app, parent, dialog.pathObjectType);
                return cmd;
            }
            else
                return null;
        }

        #endregion
    }

    public class AddPathObjectTypeCommand : ICommand
    {
        #region ICommand Members

        private WorldEditor app;
        private PathObjectTypeContainer parent;
        private PathObjectType pathObjectType;
        private PathObjectTypeNode pathObjectTypeNode;

        public AddPathObjectTypeCommand(WorldEditor worldEditor, PathObjectTypeContainer parent, PathObjectType pathObjectType)
        {
            this.app = worldEditor;
            this.parent = parent;
            this.pathObjectType = pathObjectType;
        }

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            pathObjectTypeNode = new PathObjectTypeNode(app, parent, pathObjectType);
            parent.Add(pathObjectTypeNode);
        }

        public void UnExecute()
        {
			parent.Remove(pathObjectTypeNode);
        }

        #endregion

    }

    ///<summary>
    ///    This class contains the attributes of a "path object type"
    ///    relevant to server-side path and collision detection.
    ///</summary>
    public class PathObjectTypeNode : IWorldObject {
        protected WorldEditor app;
        protected PathObjectTypeContainer parent;
        protected WorldTreeNode node;
        protected WorldTreeNode parentNode;
        protected PathObjectType pathObjectType;
        protected bool inTree = false;
        protected List<ToolStripButton> buttonBar;

		public PathObjectTypeNode(WorldEditor app, PathObjectTypeContainer parent, PathObjectType pathObjectType) {
            this.app = app;
            this.pathObjectType = pathObjectType;
            this.parent = parent;
        }

        public PathObjectTypeNode(WorldEditor app, PathObjectTypeContainer parent, XmlReader r) {
            this.app = app;
            this.parent = parent;
            this.pathObjectType = new PathObjectType(r);
        }

        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;

            // add the Path Object Types node
            node = app.MakeTreeNode(this, "Path Object Type " + pathObjectType.name);
            parentNode.Nodes.Add(node);

            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Edit Path Object Type", new EditPathObjectTypeCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Delete Path Object Type", new DeletePathObjectTypeCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
            node.ContextMenuStrip = menuBuilder.Menu;
            buttonBar = menuBuilder.ButtonBar;
        }


        public void Clone(IWorldContainer copyParent)
        {
        }

        public bool IsGlobal
        {
            get
            {
                return true;
            }
        }

        public bool IsTopLevel
        {
            get
            {
                return false;
            }
        }


        [BrowsableAttribute(false)]
        public bool WorldViewSelectable
        {
            get
            {
                return false;
            }
            set
            {
                // This property is not relevent for this object.
            }
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}:{1}\r\n", ObjectType, pathObjectType.name);
                objString +=  "\r\n";
                return objString;
            }
        }

        [BrowsableAttribute(false)]
        public List<ToolStripButton> ButtonBar
        {
            get
            {
                return buttonBar;
            }
        }

        public void RemoveFromTree()
        {
            if (node.IsSelected)
            {
                node.UnSelect();
            }
            parentNode.Nodes.Remove(node);
            parentNode = null;
            node = null;
            inTree = false;
        }


        [BrowsableAttribute(false)]
		public WorldTreeNode Node
		{
			get
			{
				return node;
			}
		}

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "PathObjectType";
            }
        }

        public PathObjectType PathObjectType
        {
            get
            {
                return pathObjectType;
            }
            set 
            {
                pathObjectType = value;
            }
        }

        public void ToXml(XmlWriter w) {
            pathObjectType.ToXml(w);
        }
        
        public void FromXml(XmlReader r) {
            pathObjectType.FromXml(r);
        }

		public void AddToScene()
		{
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
        }
        
		public void RemoveFromScene()
		{
		}

        public void CheckAssets()
        {
        }

        public void ToManifest(System.IO.StreamWriter w)
        {
        }

		[BrowsableAttribute(false)]
		public Vector3 FocusLocation
		{
			get
			{
				return parent.FocusLocation;
			}
		}

		[BrowsableAttribute(false)]
		public bool Highlight
		{
			get
			{
                return false;
			}
			set
			{
			}
		}

        [BrowsableAttribute(false)]
        public bool AcceptObjectPlacement
        {
            get
            {
                return false;
            }
            set
            {
                //not implemented for this type of object
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

    public class EditPathObjectTypeCommandFactory : ICommandFactory
    {
        WorldEditor app;
        IWorldObject parent;
        PathObjectTypeNode pathObjectTypeNode;

        public EditPathObjectTypeCommandFactory(WorldEditor app, IWorldObject parent, PathObjectTypeNode pathObjectTypeNode)
        {
            this.app = app;
            this.parent = parent;
            this.pathObjectTypeNode = pathObjectTypeNode;
        }

        #region ICommandFactory Members

        public ICommand CreateCommand()
        {
            PathObjectTypeDialog dialog = new PathObjectTypeDialog();
            dialog.pathObjectType = pathObjectTypeNode.PathObjectType;
            PathObjectType beforePathObjectType = new PathObjectType(dialog.pathObjectType);
            if (dialog.ShowDialog() == DialogResult.OK) {
                PathObjectType afterPathObjectType = new PathObjectType(dialog.pathObjectType);
                ICommand cmd = new EditPathObjectTypeCommand(app, parent, pathObjectTypeNode, 
                                                             beforePathObjectType, afterPathObjectType);
                return cmd;
            }
            else 
                return null;
        }

        #endregion
    }

    public class EditPathObjectTypeCommand : ICommand
    {
        #region ICommand Members

        private WorldEditor app;
        private IWorldObject parent;
        private PathObjectTypeNode pathObjectTypeNode;
        private PathObjectType beforePathObjectType;
        private PathObjectType afterPathObjectType;

        public EditPathObjectTypeCommand(WorldEditor worldEditor, IWorldObject parent, PathObjectTypeNode pathObjectTypeNode,
                                         PathObjectType beforePathObjectType, PathObjectType afterPathObjectType)
        {
            this.app = worldEditor;
            this.parent = parent;
            this.pathObjectTypeNode = pathObjectTypeNode;
            this.beforePathObjectType = beforePathObjectType;
            this.afterPathObjectType = afterPathObjectType;
        }

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            pathObjectTypeNode.PathObjectType = afterPathObjectType;
        }

        public void UnExecute()
        {
            pathObjectTypeNode.PathObjectType = beforePathObjectType;
        }

        #endregion

    }

    public class DeletePathObjectTypeCommandFactory : ICommandFactory
    {
        WorldEditor app;
        PathObjectTypeContainer parent;
        PathObjectTypeNode pathObjectType;

        public DeletePathObjectTypeCommandFactory(WorldEditor app, PathObjectTypeContainer parent, PathObjectTypeNode pathObjectType)
        {
            this.app = app;
            this.parent = parent;
            this.pathObjectType = pathObjectType;
        }

        #region ICommandFactory Members

        public ICommand CreateCommand()
        {
            ICommand cmd = new DeletePathObjectTypeCommand(app, parent, pathObjectType);

            return cmd;
        }

        #endregion
    }

    public class DeletePathObjectTypeCommand : ICommand
    {
        #region ICommand Members

        private WorldEditor app;
        private PathObjectTypeContainer parent;
        private PathObjectTypeNode pathObjectTypeNode;

        public DeletePathObjectTypeCommand(WorldEditor worldEditor, PathObjectTypeContainer parent, PathObjectTypeNode pathObjectTypeNode)
        {
            this.app = worldEditor;
            this.parent = parent;
            this.pathObjectTypeNode = pathObjectTypeNode;
        }

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            parent.Remove(pathObjectTypeNode);
        }

        public void UnExecute()
        {
            parent.Add(pathObjectTypeNode);
        }

        #endregion

    }

}
