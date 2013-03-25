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
using System.Windows.Forms;
using System.ComponentModel;
using Axiom.Core;

namespace Multiverse.Tools.WorldEditor
{
    public class CommandMenuBuilder
    {
        WorldEditor app = WorldEditor.Instance;

        protected ContextMenuStrip menu;

        protected ToolStripDropDown currentMenu;

        protected List<ToolStripButton> currentButtonBar;

        protected List<ToolStripButton> multiSelectButtonBar;

        protected ToolStripButton multiSelectHelpButton;

        protected ToolStripButton multiSelectDeleteButton;

        protected ToolStripButton multiSelectMoveButton;

        protected ToolStripButton multiSelectCopyClipbord;

        protected ToolStripDropDownButton multiSelectChangeCollection;

        public CommandMenuBuilder()
        {
            menu = new ContextMenuStrip();
            menu.ShowCheckMargin = false;
            menu.ShowImageMargin = false;
            menu.SuspendLayout();
            menu.Opening +=new System.ComponentModel.CancelEventHandler(menu_Opening);
            menu.Closed += new ToolStripDropDownClosedEventHandler(menu_Closed);
            currentMenu = menu;
            currentButtonBar = new List<ToolStripButton>();
        }

        public void Add(String text, ICommandFactory commandFactory, EventHandler clickHandler)
        {
            ToolStripButton button = new ToolStripButton(text);
            button.Tag = commandFactory;
            button.Click += clickHandler;
            currentMenu.Items.Add(button);
            ToolStripButton button2 = new ToolStripButton();
            button2.Tag = commandFactory;
            button2.Click += clickHandler;
            button2.Alignment = ToolStripItemAlignment.Right;
            button2.ToolTipText = text;
            addImage(button2);
            //button.Width = 200;
            currentButtonBar.Add(button2);
        }

        public void Add(String text, string argString, EventHandler clickHandler)
        {
            ToolStripButton button = new ToolStripButton(text);
            button.Tag = argString;
            button.Click += clickHandler;
            //button.Width = 200;
            currentMenu.Items.Add(button);
            ToolStripButton button2 = new ToolStripButton();
            button2.Tag = argString;
            button2.Click += clickHandler;
            button2.Alignment = ToolStripItemAlignment.Right;
            button2.ToolTipText = text;
            addImage(button2);
            //button.Width = 200;
            currentButtonBar.Add(button2);
        }

        public void Add(String text, DirectionAndObject dirObj, EventHandler clickHandler)
        {
            ToolStripButton button = new ToolStripButton(text);
            button.Tag = dirObj;
            button.Click += clickHandler;
            //button.Width = 200;
            currentMenu.Items.Add(button);
            ToolStripButton button2 = new ToolStripButton();
            button2.Tag = dirObj;
            button2.Click += clickHandler;
            button2.ToolTipText = text;
            currentButtonBar.Add(button2);
            button2.Alignment = ToolStripItemAlignment.Right;
            addImage(button2);
        }

        public void Add(String text, DirectionAndMarker dirObj, EventHandler clickHandler)
        {
            ToolStripButton button = new ToolStripButton(text);
            button.Tag = dirObj;
            button.Click += clickHandler;
            //button.Width = 200;
            currentMenu.Items.Add(button);
            ToolStripButton button2 = new ToolStripButton();
            button2.Tag = dirObj;
            button2.Click += clickHandler;
            button2.ToolTipText = text;
            currentButtonBar.Add(button2);
            button2.Alignment = ToolStripItemAlignment.Right;
            addImage(button2);
        }


        public void Add(String text, WorldObjectCollection obj, EventHandler clickHandler)
        {
            ToolStripButton button = new ToolStripButton(text);
            button.Tag = obj;
            button.Click += clickHandler;
            currentMenu.Items.Add(button);
            ToolStripButton button2 = new ToolStripButton();
            button2.Tag = obj;
            button2.Click += clickHandler;
            button2.ToolTipText = text;
            currentButtonBar.Add(button2);
            button2.Alignment = ToolStripItemAlignment.Right;
            addImage(button2);
        }

        public void AddDropDown(String text)
        {
            ToolStripDropDownButton button = new ToolStripDropDownButton(text);
            button.ShowDropDownArrow = true;
            //button.Width = 200;
            button.DropDownDirection = ToolStripDropDownDirection.Right;
            ToolStripDropDown dropDown = new ToolStripDropDown();
            button.DropDown = dropDown;
            button.MouseEnter += new System.EventHandler(dropDownMouseEnter);
            currentMenu.Items.Add(button);
            currentMenu = dropDown;
        }

        public void AddDropDown(String text, EventHandler dropdownopeningevent)
        {
            ToolStripDropDownButton button = new ToolStripDropDownButton(text);
            button.ShowDropDownArrow = true;
            //button.Width = 200;
            button.DropDownDirection = ToolStripDropDownDirection.Right;
            ToolStripDropDown dropDown = new ToolStripDropDown();
            button.DropDown = dropDown;
            button.DropDownOpening += new System.EventHandler(dropdownopeningevent);
            button.MouseEnter += new System.EventHandler(this.toolStripButton_Enter);
            button.Tag = (IObjectCollectionParent) app.WorldRoot;
            button.DropDownClosed += new System.EventHandler(this.collectionDropDown_closed);
            currentMenu.Items.Add(button);
        }

        public void menu_Opening(object sender, CancelEventArgs e)
        {
            bool collection = false;
            bool changeCollection = true;
            bool objDelete = true;
            bool dragable = true;
            List<IWorldContainer>  parents = new List<IWorldContainer>();
            if (app.SelectedNodes.Count > 1)
            {
                foreach (WorldTreeNode node in app.SelectedNodes)
                {
                    if (String.Equals(node.WorldObject.ObjectType, "Collection"))
                    {
                        collection = true;
                    }
                    if (!(node.WorldObject is IObjectChangeCollection))
                    {
                        changeCollection = false;
                    }
                    if (!(node.WorldObject is IObjectDelete))
                    {
                        objDelete = false;
                    }

                    if (node.WorldObject is IObjectDelete && !parents.Contains(((IObjectDelete)node.WorldObject).Parent))
                    {
                        parents.Add(((IObjectDelete)node.WorldObject).Parent);
                    }
                    if (!node.WorldObject.IsTopLevel)
                    {
                        dragable = false;
                    }
                }
                foreach (ToolStripItem item in ((ContextMenuStrip)sender).Items)
                {
                    ToolStripItem button = item;
                    button.Visible = false;
                }
                if (dragable)
                {
                    ToolStripButton newButton3 = new ToolStripButton();
                    newButton3.Text = "Drag Selected Objects";
                    newButton3.Tag = new DragObjectsFromMenuCommandFactory(app);
                    newButton3.Click += new EventHandler(app.DefaultCommandClickHandler);
                    (sender as ContextMenuStrip).Items.Add(newButton3 as ToolStripItem);
                }
                if (changeCollection && !collection)
                {
                    List<IObjectChangeCollection> changeCollList = new List<IObjectChangeCollection>();
                    foreach (IWorldObject changeObj in app.SelectedObject)
                    {
                        changeCollList.Add((IObjectChangeCollection)changeObj);
                    }
                    ToolStripDropDownButton newButton = new ToolStripDropDownButton("Move");
                    newButton.Text = "Move";
                    newButton.Tag = WorldRoot.Instance;
                    newButton.DropDownOpening +=new EventHandler(ObjectCollectionDropDown_Opening);
                    newButton.MouseEnter += new System.EventHandler(this.toolStripButton_Enter);
                    ((ContextMenuStrip)sender).Items.Add(newButton);
                    multiSelectChangeCollection = newButton;
                }
                ToolStripButton newButton2 = new ToolStripButton("Copy Description");
                newButton2.Text = "Copy Description";
                newButton2.Tag = "";
                newButton2.Click += new EventHandler(app.copyToClipboardMenuButton_Click);
                ((ContextMenuStrip)sender).Items.Add((ToolStripItem)newButton2);
                multiSelectCopyClipbord = newButton2;
                ToolStripButton newButton1 = new ToolStripButton("Help");
                newButton1.Text = "Help";
                newButton1.Tag = "MultiSelect";
                newButton1.Click += new EventHandler(app.HelpClickHandler);
                ((ContextMenuStrip)sender).Items.Add((ToolStripItem)newButton1);
                multiSelectHelpButton = newButton1;
                if (parents.Count == 1 && objDelete)
                {
                    ToolStripButton newButton = new ToolStripButton("Delete");
                    newButton.Text = "Delete";
                    newButton.Click += new EventHandler(app.DefaultCommandClickHandler);
                    ((ContextMenuStrip)sender).Items.Add((ToolStripItem)newButton);
                    newButton.Tag = new DeleteObjectsCommandFactory(app, app.SelectedObject);
                    multiSelectDeleteButton = newButton;
                    
                }
                ((ToolStripDropDown)sender).Closed += new ToolStripDropDownClosedEventHandler(menu_Closed);
            }
        }

        private void menu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (multiSelectHelpButton != null)
            {
                ((ContextMenuStrip)sender).Items.Remove(multiSelectHelpButton);
                multiSelectHelpButton = null;
            }
            if (multiSelectDeleteButton != null)
            {
                ((ContextMenuStrip)sender).Items.Remove(multiSelectDeleteButton);
                multiSelectDeleteButton = null;
            }
            if (multiSelectCopyClipbord != null)
            {
                ((ContextMenuStrip)sender).Items.Remove(multiSelectCopyClipbord);
                multiSelectCopyClipbord = null;
            }
            if (multiSelectChangeCollection != null)
            {
                ((ContextMenuStrip)sender).Items.Remove(multiSelectChangeCollection);
                multiSelectChangeCollection = null;
            }
            foreach (ToolStripItem item in ((ContextMenuStrip)sender).Items)
            {
                if (item != null && item is ToolStripButton)
                {
                    ToolStripButton button = item as ToolStripButton;
                    button.Visible = true;
                }
                else
                {
                    if (item != null && item is ToolStripDropDownButton)
                    {
                        ToolStripDropDownButton button2 = item as ToolStripDropDownButton;
                        button2.Visible = true;
                    }
                }
            }
            ((ContextMenuStrip)sender).Closed -= new ToolStripDropDownClosedEventHandler(menu_Closed);
        }

        public void addImage(ToolStripButton button)
        {
            switch (button.ToolTipText)
            {
                case "Help":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.help;
                    break;
                case "Delete":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.delete;
                    break;
                case "Add Forest":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_forest;
                    break;
                case "Add Water":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_water;
                    break;
                case "Add Fog":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_fog;
                    break;
                case "Add Sound":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_sound;
                    break;
                case "Add Vegetation":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_grass;
                    break;
                case "Add Spawn Generator":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_spawn_generator;
                    break;
                case "Add Ambient Light":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_ambient_light;
                    break;
                case "Add Directional Light":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_directional_light;
                    break;
                case "Add Tree":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_tree;
                    break;
                case "Add Plant Type":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_plant_type;
                    break;
                case "Insert new points":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.insert_new_points;
                    break;
                case "Edit Path Object Type":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.edit_object_path_type;
                    break;
                case "Above":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.above;
                    break;
                case "North":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.north;
                    break;
                case "South":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.south;
                    break;
                case "West":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.west;
                    break;
                case "East":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.east;
                    break;
                case "Attach Particle Effect":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.attach_particle;
                    break;
                case "Add Object":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_object;
                    break;
                case "Add Road":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_road;
                    break;
                case "Add Marker":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_marker;
                    break;
                case "Add Region":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_region;
                    break;
                case "Add Point Light":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_point_light;
                    break;
                case "Create Object Collection":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.create_object_collection;
                    break;
                case "Load Terrain":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.load_terrain;
                    break;
                case "Add Terrain Decal":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.attach_decal;
                    break;
                case "Copy Description":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.copy_description;
                    break;
                case "Load Collection":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.load_collection_03;
                    break;
                case "Unload Collection":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.unload_collection;
                    break;
                case "Add To Scene":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.add_collection;
                    break;
                case "Remove From Scene":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.remove_collection_04;
                    break;
                case "Drag Road":
                case "Drag Point Light":
                case "Drag Marker":
                case "Drag Region":
                case "Drag Object":
                case "Drag Selected Objects":
                case "Drag Decal":
                case "Drag Point":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.drag_object;
                    break;
                case "Add Marker at Camera":
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.cam_position_marker;
                    break;
                default:
                    button.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.help;
                    break;
            }
        }

        public void FinishDropDown()
        {
            
            currentMenu = menu;
        }

        public void AddSeparator()
        {
            ToolStripSeparator separator = new ToolStripSeparator();
            currentMenu.Items.Add(separator);
        }

        protected void UpdateWidth()
        {
            int w = 0;
            foreach (ToolStripItem item in menu.Items)
            {
                if (item.Width > w)
                {
                    w = item.Width;
                }
            }
            menu.Width = w;
        }

        public ContextMenuStrip Menu
        {
            get
            {
                menu.ResumeLayout();
                UpdateWidth();
                return menu;
            }
        }

        public List<ToolStripButton> ButtonBar
        {
            get
            {
                return currentButtonBar;
            }
        }

        public void toolStripButton_Enter(object sender, EventArgs e)
        {
            ToolStripItem but = (ToolStripItem)sender;
            if (but.Tag != null)
            {
                if (((IObjectCollectionParent)(but.Tag)).CollectionList.Count != 0)
                {
                    ToolStripDropDownButton button = (ToolStripDropDownButton)sender;
                    button.ShowDropDown();
                }
            }
        }

        public void dropDownMouseEnter(object sender, EventArgs e)
        {
            ToolStripItem but = (ToolStripItem)sender;
            if (but is ToolStripDropDownButton)
            {
                (but as ToolStripDropDownButton).ShowDropDown();
            }
        }

        public void onDropDownMouseLeave(object sender, EventArgs e)
        {
            ToolStripItem but = (ToolStripItem)sender;
            if (but is ToolStripDropDownButton)
            {
                (but as ToolStripDropDownButton).HideDropDown();
            }
        }

        public void toolStripButton_Leave(object sender, EventArgs e)
        {
            ToolStripItem but = (ToolStripItem)sender;
            if (but.Tag != null)
            {
                if (((IObjectCollectionParent)(but.Tag)).CollectionList.Count != 0)
                {
                    ToolStripDropDownButton button = (ToolStripDropDownButton)sender;
                    button.HideDropDown();
                }
            }
        }

        public void toolStripCollectionTree_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripItem menuItem = (ToolStripItem)sender;
            ToolStripDropDownButton parent = (ToolStripDropDownButton)sender;
            WorldObjectCollection collection = (WorldObjectCollection)menuItem.Tag;


            if (app.SelectedObject.Count >= 1 && app.SelectedObject.Count != 0)
            {
                foreach (WorldObjectCollection coll in collection.CollectionList)
                {
                    if (app.SelectedObject[0] is WorldObjectCollection && ReferenceEquals(((IObjectChangeCollection)(app.SelectedObject[0])).Parent, coll) ||
                        ReferenceEquals(app.SelectedObject[0], coll))
                    {
                        continue;
                    }
                    if (coll.CollectionList.Count != 0)
                    {
                        ToolStripDropDownButton but = new ToolStripDropDownButton();

                        but.Tag = coll;
                        but.Text = coll.Name;
                        but.DropDownOpening += new System.EventHandler(this.toolStripCollectionTree_DropDownOpening);
                        but.MouseEnter += new System.EventHandler(this.toolStripButton_Enter);
                        but.Click += new System.EventHandler(app.CollectionButton_clicked);
                        but.DropDownClosed += new System.EventHandler(this.collectionDropDown_closed); ;
                        parent.DropDownItems.Add(but as ToolStripItem);
                    }
                    else
                    {
                        ToolStripButton but = new ToolStripButton(coll.Name);
                        but.Tag = coll;
                        but.Click += new System.EventHandler(app.CollectionButton_clicked);
                        parent.DropDownItems.Add(but);
                    }
                }
            }
            else return;
        }

        public void collectionDropDown_closed(object sender, EventArgs e)
        {
            ToolStripDropDownButton button =  (ToolStripDropDownButton)sender;
            button.DropDownItems.Clear();
        }


        public void changeCollectionDropDown_Opening(object sender, EventArgs e)
        {
            ToolStripDropDownButton parent = (ToolStripDropDownButton)sender;
            List<IWorldContainer> parents = new List<IWorldContainer>();
            foreach(IWorldObject obj in app.SelectedObject)
            {
                if (obj is IObjectChangeCollection)
                {
                    parents.Add(((IObjectChangeCollection)obj).Parent);
                }
            }
            foreach (WorldObjectCollection collection in WorldRoot.Instance.CollectionList)
            {
                if (collection.CollectionList.Count != 0)
                {
                    ToolStripDropDownButton button = new ToolStripDropDownButton();
                    button.Tag = collection;
                    button.Text = collection.Name;
                    button.DropDownOpening += new System.EventHandler(this.toolStripCollectionTree_DropDownOpening);
                    button.MouseEnter += new System.EventHandler(this.toolStripButton_Enter);
                    button.Click += new System.EventHandler(app.CollectionButton_clicked);
                    button.DropDownClosed += new System.EventHandler(this.collectionDropDown_closed);
                    parent.DropDownItems.Add(button);
                }
                else
                {
                    ToolStripButton but = new ToolStripButton(collection.Name);
                    but.Tag = collection;
                    but.Text = collection.Name;
                    but.Click += new System.EventHandler(app.CollectionButton_clicked);
                    parent.DropDownItems.Add(but);
                }
            }
        }

        private bool checkChildren(WorldObjectCollection obj)
        {
            bool ret = true;
            foreach(WorldObjectCollection coll in obj.CollectionList)
            {
                if (ReferenceEquals(coll.Parent, app.SelectedObject[0]))
                {
                    ret = false;
                }
                else
                {
                    if (!(coll.Parent is WorldRoot))
                    {
                        ret = checkChildren(coll.Parent as WorldObjectCollection);
                    }
                }
                if (!ret)
                {
                    return ret;
                }
            }
            return ret;
        }


        public void ObjectCollectionDropDown_Opening(object sender, EventArgs e)
        {
            // used as the right click handler for objects, also used by the object collection right click handler after the first level.
            ToolStripDropDownButton parent = (ToolStripDropDownButton)sender;
            List<IWorldContainer> parents = new List<IWorldContainer>();

            if (app.SelectedObject.Count > 1)
            {
                // Object collections can not be moved if more than one object is selected
                foreach (IWorldObject obj in app.SelectedObject)
                {
                    if (!(obj is WorldObjectCollection))
                    {
                        parents.Add(((IObjectChangeCollection)obj).Parent);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            foreach (WorldObjectCollection collection in WorldRoot.Instance.CollectionList)
            {
                // If an object is a WorldObjectCollection, exclude the children from being in the change collection drop downs
                if (app.SelectedObject.Count == 1 && app.SelectedObject[0] is WorldObjectCollection)
                {
                    if(ReferenceEquals(app.SelectedObject[0],collection))
                    {
                        continue;
                    }
                    if ((app.SelectedObject[0] as WorldObjectCollection).Contains(collection))
                    {
                        if (!(checkChildren(collection)))
                        {
                            continue;
                        }
                    }
                }

                if (collection.CollectionList.Count != 0)
                {
                    ToolStripDropDownButton button = new ToolStripDropDownButton();
                    button.Tag = collection;
                    button.Text = collection.Name;
                    button.DropDownOpening += new System.EventHandler(this.toolStripCollectionTree_DropDownOpening);
                    button.MouseEnter += new System.EventHandler(this.toolStripButton_Enter);
                    button.Click += new System.EventHandler(app.CollectionButton_clicked);
                    button.DropDownClosed += new System.EventHandler(this.collectionDropDown_closed);
                    parent.DropDownItems.Add(button);
                }
                else
                {
                    ToolStripButton but = new ToolStripButton(collection.Name);
                    but.Tag = collection;
                    but.Text = collection.Name;
                    but.Click += new System.EventHandler(app.CollectionButton_clicked);
                    parent.DropDownItems.Add(but);
                }
            }
        }

        public void ObjectCollectionMoveDropDown_Opening(object sender, EventArgs e)
        {
            // Use this as a handler for the right click menu of an object collection
            if (ReferenceEquals(((IObjectChangeCollection)(app.SelectedObject[0])).Parent, WorldRoot.Instance) && app.SelectedObject[0] is WorldObjectCollection)
            {
                this.ObjectCollectionDropDown_Opening(sender, e);
                return;
            }
            ToolStripDropDownButton parent = (ToolStripDropDownButton)sender;
            ToolStripDropDownButton button = new ToolStripDropDownButton();
            button.Tag = (IObjectCollectionParent)app.WorldRoot;
            button.Text = WorldRoot.Instance.Name;
            button.DropDownOpening += new System.EventHandler(this.ObjectCollectionDropDown_Opening);
            button.MouseEnter += new System.EventHandler(this.toolStripButton_Enter);
            button.Click += new System.EventHandler(app.CollectionButton_clicked);
            button.DropDownClosed += new System.EventHandler(this.collectionDropDown_closed);
            parent.DropDownItems.Add(button);
         }

        public List<ToolStripButton> MultiSelectButtonBar()
        {
            bool showDelete = true;
            bool showDrag = true;
            List<IWorldContainer> parents = new List<IWorldContainer>();
            List<ToolStripButton> list = new List<ToolStripButton>();
            foreach (IWorldObject obj in app.SelectedObject)
            {
                if (!(obj is IObjectDelete))
                {
                    showDelete = false;
                }
                if (showDelete && !parents.Contains((obj as IObjectDelete).Parent))
                {
                    parents.Add((obj as IObjectDelete).Parent);
                }
                if (!(obj is IObjectDrag) || (obj is MPPoint))
                {
                    showDrag = false;
                }

            }
            if (showDelete && parents.Count == 1)
            {
                ToolStripButton button0 = new ToolStripButton();
                button0.ToolTipText = "Delete";
                button0.Tag = new DeleteObjectsCommandFactory(app, app.SelectedObject);
                button0.Click += new EventHandler(app.DefaultCommandClickHandler);
                button0.Alignment = ToolStripItemAlignment.Right;
                addImage(button0);
                list.Add(button0);
            }
            ToolStripButton button1 = new ToolStripButton();
            button1.Tag = "MultiSelect";
            button1.Click += new EventHandler(app.HelpClickHandler);
            button1.ToolTipText = "Help";
            button1.Alignment = ToolStripItemAlignment.Right;
            addImage(button1);
            list.Add(button1);
            ToolStripButton button2 = new ToolStripButton();
            if (app.SelectedObject.Count > 0)
            {
                button2.ToolTipText = "Copy Description";
                button2.Click += new EventHandler(app.copyToClipboardMenuButton_Click);
                button2.Alignment = ToolStripItemAlignment.Right;
                addImage(button2);
                list.Add(button2);
            }
            ToolStripButton button3 = new ToolStripButton();
            if (showDrag && app.SelectedObject.Count > 1)
            {
                button3.ToolTipText = "Drag Selected Objects";
                button3.Tag = new DragObjectsFromMenuCommandFactory(app);
                button3.Click += new EventHandler(app.DefaultCommandClickHandler);
                button3.Alignment = ToolStripItemAlignment.Right;
                addImage(button3);
                list.Add(button3);
            }
            return list;

        }
    }
}
