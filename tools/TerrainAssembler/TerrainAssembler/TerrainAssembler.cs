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
using System.Diagnostics;
using Multiverse.Controls;
using Multiverse.Lib.WorldMap;
using Multiverse.Lib.Coordinates;

namespace Multiverse.Tools.TerrainAssembler
{
    public partial class TerrainAssembler : Form
    {
        private ImageGrid imageGrid;
        protected WorldMap worldMap;
        protected CoordXZ gridOffset;
        protected TreeNode rootNode;
        protected TreeNode zonesNode;
        protected TreeNode layersNode;
        protected TreeNode bookmarksNode;
        protected CoordXZ minVisibleTile;
        protected CoordXZ maxVisibleTile;
        protected MapLayer currentViewLayer;

        public TerrainAssembler()
        {
            InitializeComponent();

            // Initialize the grid control
            imageGrid = new ImageGrid();
            mainSplitContainer.Panel1.Controls.Add(imageGrid);
            imageGrid.Name = "imageGrid";
            imageGrid.Location = new System.Drawing.Point(0, 0);
            imageGrid.TabIndex = 0;
            imageGrid.Dock = DockStyle.Fill;
            imageGrid.Enabled = false;
            imageGrid.VisibleCellsChange += new VisibleCellsChangeEvent(VisibleCellsChangeHandler);
            imageGrid.UserSelectionChange += new UserSelectionChangeEvent(UserSelectionChangeHandler);

            //imageGrid.WidthCells = 50;
            //imageGrid.HeightCells = 30;

            imageGrid.LabelCells = showLabelsToolStripMenuItem.Checked;
            imageGrid.SelectionBorderColor = Color.Red;

            //for (int i = 0; i < 10; i++)
            //{
            //    ImageGridCell cell = imageGrid.CreateCell(i, 5);
            //    cell.Color = Color.Red;
            //    cell.Label = String.Format("color{0}", i);
            //}

            //for (int y = 0; y < 10; y++)
            //{
            //    for (int x = 0; x < 8; x++)
            //    {
            //        ImageGridCell cell = imageGrid.CreateCell(5 + x, 10 + y);
            //        Bitmap tmpBitmap = new Bitmap(String.Format("minimap\\minimap_x{0}y{1}.png", x, 9 - y));
            //        cell.Image = new Bitmap(tmpBitmap, new Size(64, 64));
            //        cell.Label = String.Format("map({0}, {1})", x, y);
            //        cell.ToolTipText = String.Format("Zone: Island\nGrid Coordinate: {0}, {1}", x, y);
            //    }
            //}
        }

        protected void InitTreeView()
        {
            rootNode = new TreeNode(worldMap.WorldName);
            zonesNode = new TreeNode("Zones");
            bookmarksNode = new TreeNode("Bookmarks");
            layersNode = new TreeNode("Layers");

            rootNode.Nodes.Add(zonesNode);
            rootNode.Nodes.Add(layersNode);
            rootNode.Nodes.Add(bookmarksNode);

            treeView.Nodes.Clear();
            treeView.Nodes.Add(rootNode);

            foreach (string zoneName in worldMap.ZoneNames)
            {
                AddZone(worldMap.GetZone(zoneName));
            }

            foreach (string layerName in worldMap.LayerNames)
            {
                AddLayer(worldMap.GetLayer(layerName));
            }

        }

        protected void AddZone(MapZone zone)
        {

            TreeNode node = new TreeNode(zone.Name);
            node.Tag = zone;
            zonesNode.Nodes.Add(node);

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowCheckMargin = false;
            menu.ShowImageMargin = false;
            menu.SuspendLayout();

            ToolStripButton button = new ToolStripButton("Load Layer...");
            button.Tag = zone;
            button.Click += new EventHandler(loadLayerHandler);
            menu.Items.Add(button);

            menu.ResumeLayout();

            // update menu width
            int w = 0;
            foreach (ToolStripItem item in menu.Items)
            {
                if (item.Width > w)
                {
                    w = item.Width;
                }
            }
            menu.Width = w;

            node.ContextMenuStrip = menu;
        }

        protected void AddLayer(MapLayer layer)
        {

            TreeNode node = new TreeNode(layer.LayerName);
            node.Tag = layer;
            layersNode.Nodes.Add(node);
        }

        protected void loadLayerHandler(object sender, EventArgs args)
        {
            ToolStripButton button = sender as ToolStripButton;

            MapZone zone = button.Tag as MapZone;

            using (LoadLayerDialog dlg = new LoadLayerDialog())
            {
                DialogResult result;

                dlg.LayerNames = worldMap.LayerNames;

                result = dlg.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }

                int tilesWidth = zone.MaxTileCoord.x - zone.MinTileCoord.x + 1;
                int tilesHeight = zone.MaxTileCoord.z - zone.MinTileCoord.z + 1;
                int zoneWidthMeters = tilesWidth * worldMap.TileSize / worldMap.OneMeter;
                int zoneHeightMeters = tilesHeight * worldMap.TileSize / worldMap.OneMeter;

                int metersPerPixel = zoneWidthMeters / dlg.LayerMapImage.Width;
                Debug.Assert(metersPerPixel == (zoneHeightMeters / dlg.LayerMapImage.Height));


                int samplesPerTile = WorldMap.metersPerTile / metersPerPixel;

                MapLayer layer = worldMap.GetLayer(dlg.LayerName);

                for (int z = 0; z < tilesHeight; z++)
                {
                    int dz = z + zone.MinTileCoord.z;
                    for (int x = 0; x < tilesWidth; x++)
                    {
                        int dx = x + zone.MinTileCoord.x;

                        CoordXZ tileCoord = new CoordXZ(dx, dz, WorldMap.tileSize);

                        MapBuffer tileMap;

                        if (layer is ColorMapLayer)
                        {
                            ColorMapLayer tmpLayer = layer as ColorMapLayer;

                            tileMap = tmpLayer.CreateCompatibleMapBuffer(dlg.LayerMapImage, metersPerPixel,
                                x * samplesPerTile, z * samplesPerTile, samplesPerTile);
                        }
                        else
                        {
                            ValueMapLayer tmpLayer = layer as ValueMapLayer;
                            tileMap = null;
                        }

                        MapTile tile = worldMap.GetTile(tileCoord);

                        if (tileMap.MetersPerSample != layer.MetersPerSample)
                        {
                            tileMap = tileMap.Scale(worldMap.MetersPerTile / layer.MetersPerSample);
                        }

                        layer.CopyIn(tileCoord, tileMap);
                    }
                }

            }
        }

        public void SaveMap()
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                string filename = string.Format("{0}.mwm", worldMap.WorldName);

                dlg.Title = "Save Map";
                dlg.DefaultExt = "mwm";
                dlg.FileName = filename;

                dlg.Filter = "Multiverse World Map files (*.mwm)|*.mwm|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    worldMap.Save(System.IO.Path.GetDirectoryName(dlg.FileName));
                }
            }
        }

        public void LoadMap()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Load Map";
                dlg.DefaultExt = "mwm";
                dlg.Filter = "Multiverse World Map files (*.mwm)|*.mwm|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    worldMap = new WorldMap(dlg.FileName);

                    currentViewLayer = worldMap.GetLayer("heightfield");

                    gridOffset = worldMap.MinTile;

                    imageGrid.WidthCells = worldMap.MaxTile.x - worldMap.MinTile.x + 1;
                    imageGrid.HeightCells = worldMap.MaxTile.z - worldMap.MinTile.z + 1;
                    imageGrid.Enabled = true;

                    InitTreeView();
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();   
        }

        private void showLabelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showLabelsToolStripMenuItem.Checked = !showLabelsToolStripMenuItem.Checked;
            imageGrid.LabelCells = showLabelsToolStripMenuItem.Checked;
        }

        private void newMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (NewMapDialog dlg = new NewMapDialog())
            {
                DialogResult result;

                result = dlg.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }

                int tw = (dlg.MapWidth + WorldMap.tileSize - 1) / WorldMap.tileSize;
                int th = (dlg.MapHeight + WorldMap.tileSize - 1) / WorldMap.tileSize;

                imageGrid.WidthCells = tw;
                imageGrid.HeightCells = th;
                imageGrid.Enabled = true;

                int tx, tz;

                if ((tw <= WorldMap.tilesPerSection) && (th <= WorldMap.tilesPerSection))
                {
                    // if the world fits in a single section, then center it in section 0,0
                    tx = (WorldMap.tilesPerSection - tw) / 2;
                    tz = (WorldMap.tilesPerSection - th) / 2;
                }
                else
                {
                    // if the world doesn't fit in a single section, then center on the origin
                    tx = -tw / 2;
                    tz = -th / 2;
                }

                CoordXZ minTile = new CoordXZ(tx, tz, WorldMap.tileSize);
                CoordXZ maxTile = new CoordXZ(tx + tw - 1, tz + th - 1, WorldMap.tileSize);

                worldMap = new WorldMap(dlg.MapName, minTile, maxTile, dlg.MinTerrainHeight, dlg.MaxTerrainHeight, dlg.DefaultTerrainHeight);

                currentViewLayer = worldMap.GetLayer("heightfield");

                gridOffset = minTile;

                InitTreeView();
            }

        }

        private bool NewZonePlacementCallback(bool cancelled, Point location, object arg)
        {
            NewZoneData zoneData = (NewZoneData)arg;

            if (!cancelled)
            {
                // Check to see if the new zone overlaps any existing tiles
                for (int z = 0; z < zoneData.TilesHeight; z++)
                {
                    int dz = z + gridOffset.z + location.Y;
                    for (int x = 0; x < zoneData.TilesWidth; x++)
                    {
                        int dx = x + gridOffset.x + location.X;

                        CoordXZ tileCoord = new CoordXZ(dx, dz, WorldMap.tileSize);
                        MapTile tile = worldMap.GetTile(tileCoord);
                        if (tile != null)
                        {
                            // overlaps existing tiles
                            return false;
                        }
                    }
                }

                MapZone zone = worldMap.CreateZone(zoneData.ZoneName);

                int samplesPerTile = WorldMap.metersPerTile / zoneData.MetersPerSample;

                ValueMapLayer heightFieldLayer = worldMap.HeightFieldLayer as ValueMapLayer;

                for (int z = 0; z < zoneData.TilesHeight; z++)
                {
                    int dz = z + gridOffset.z + location.Y;
                    for (int x = 0; x < zoneData.TilesWidth; x++)
                    {
                        int dx = x + gridOffset.x + location.X;

                        CoordXZ tileCoord = new CoordXZ(dx, dz, WorldMap.tileSize);

                        MapBuffer tileHeightmap = heightFieldLayer.CreateCompatibleMapBuffer(zoneData.Heightmap, zoneData.MetersPerSample,
                            x * samplesPerTile, z * samplesPerTile, samplesPerTile, zoneData.MinHeight, zoneData.MaxHeight);

                        MapTile tile = worldMap.CreateTile(tileCoord);
                        tile.Zone = zone;

                        if (tileHeightmap.MetersPerSample != heightFieldLayer.MetersPerSample)
                        {
                            tileHeightmap = tileHeightmap.Scale(worldMap.MetersPerTile / heightFieldLayer.MetersPerSample);
                        }

                        heightFieldLayer.CopyIn(tileCoord, tileHeightmap);

                        ImageGridCell cell = imageGrid.CreateCell(x + location.X, z + location.Y);

                        cell.Image = currentViewLayer.CreateThumbnail(tileCoord, worldMap.TileSize, imageGrid.CellSize);
                    }
                }

                AddZone(zone);
            }
            return true;
        }

        protected void UpdateVisible()
        {
            if ( worldMap != null ) {
                MapLayer heightFieldLayer = worldMap.HeightFieldLayer;

                for (int z = minVisibleTile.z; z <= maxVisibleTile.z; z++)
                {
                    for (int x = minVisibleTile.x; x < maxVisibleTile.x; x++)
                    {
                        CoordXZ tileCoord = new CoordXZ(x, z, WorldMap.tileSize);
                        MapTile tile = worldMap.GetTile(tileCoord);
                        if (tile != null)
                        {
                            ImageGridCell cell = imageGrid.GetCell(x - gridOffset.x, z - gridOffset.z);
                            if (cell == null)
                            {
                                cell = imageGrid.CreateCell(x - gridOffset.x, z - gridOffset.z);
                            }

                            if (cell.Image == null)
                            {
                                cell.Image = currentViewLayer.CreateThumbnail(tileCoord, worldMap.TileSize, imageGrid.CellSize);
                            }
                        }
                    }
                }
            }
        }

        public void SetViewLayer(string layerName)
        {
            currentViewLayer = worldMap.GetLayer(layerName);

            imageGrid.ClearCells();

            UpdateVisible();
        }

        public void VisibleCellsChangeHandler(object sender, VisibleCellsChangeEventArgs args)
        {
            // update min and max visible tiles
            minVisibleTile.x = args.CellsVisible.X + gridOffset.x;
            minVisibleTile.z = args.CellsVisible.Y + gridOffset.z;
            maxVisibleTile.x = minVisibleTile.x + args.CellsVisible.Width - 1;
            maxVisibleTile.z = minVisibleTile.z + args.CellsVisible.Height - 1;

            UpdateVisible();
        }

        public void UserSelectionChangeHandler(object sender, EventArgs args)
        {
            if (imageGrid.SelectedCells.Count == 1)
            {
                int tx = imageGrid.SelectedCells[0].X - gridOffset.x;
                int tz = imageGrid.SelectedCells[0].Y - gridOffset.z;

                
                MapTile tile = worldMap.GetTile(new CoordXZ(tx, tz, WorldMap.tileSize));

                if (tile != null)
                {
                    propertyGrid.SelectedObject = tile.Properties;
                }
                else
                {
                    propertyGrid.SelectedObject = null;
                }
            }
            else
            {
                propertyGrid.SelectedObject = null;
            }
        }

        private void createANewZoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (NewZoneDialog dlg = new NewZoneDialog())
            {
                DialogResult result;

                result = dlg.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }

                NewZoneData newZoneData = dlg.NewZoneData;

                Multiverse.Lib.WorldMap.Image tmpheightmap = dlg.Heightmap;

                imageGrid.BeginDrag(new Size(newZoneData.TilesWidth, newZoneData.TilesHeight), new ImageGridDragFinished(NewZonePlacementCallback), newZoneData);
            }
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (worldMap != null)
            {
                createANewZoneToolStripMenuItem.Enabled = true;
            }
            else
            {
                createANewZoneToolStripMenuItem.Enabled = false;
            }
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (worldMap != null)
            {
                saveMapToolStripMenuItem.Enabled = true;
                saveAsToolStripMenuItem.Enabled = true;
            }
            else
            {
                saveMapToolStripMenuItem.Enabled = false;
                saveAsToolStripMenuItem.Enabled = false;
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.Tag is MapZone)
            {
                MapZone zone = node.Tag as MapZone;

                propertyGrid.SelectedObject = zone.Properties;

                imageGrid.ClearSelection();

                foreach (CoordXZ tileCoord in zone.Tiles)
                {
                    imageGrid.AddSelectedCell(tileCoord.x + gridOffset.x, tileCoord.z + gridOffset.z);
                }
            }
            else if (node.Tag is MapLayer)
            {
                MapLayer layer = node.Tag as MapLayer;

                propertyGrid.SelectedObject = layer.Properties;

                imageGrid.ClearSelection();
            }
            else
            {
                propertyGrid.SelectedObject = null;

                imageGrid.ClearSelection();
            }
        }

        private void saveMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveMap();
        }

        private void loadMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadMap();
        }

        private void viewToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (worldMap == null)
            {
                viewLayerToolStripMenuItem.Enabled = false;
            }
            else
            {
                viewLayerToolStripMenuItem.Enabled = true;
                viewLayerToolStripMenuItem.DropDownItems.Clear();
                foreach (string layerName in worldMap.LayerNames)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem();

                    item.Name = layerName;
                    item.Text = layerName;
                    item.Size = new Size(200, 22);
                    item.Click += new EventHandler(viewLayerItem_Click);

                    if (currentViewLayer.LayerName == layerName)
                    {
                        item.Checked = true;
                    }

                    viewLayerToolStripMenuItem.DropDownItems.Add(item);
                }
            }
        }

        private void viewLayerItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            SetViewLayer(item.Text);
        }
    }
}
