using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Multiverse.Controls
{

    public delegate bool ImageGridDragFinished(bool cancelled, Point location, object arg);

    public partial class ImageGrid : Panel
    {

        #region Member Variables

        /// <summary>
        /// Number of horizontal cells in the grid
        /// </summary>
        protected int widthCells = 0;

        /// <summary>
        /// Number of vertical cells in the grid
        /// </summary>
        protected int heightCells = 0;

        /// <summary>
        /// The size of each cell in pixels
        /// </summary>
        protected int cellSize = 32;

        /// <summary>
        /// Width of the border in pixels
        /// </summary>
        protected int cellBorderWidth = 3;

        /// <summary>
        /// computed value of cellSize + cellBorderWidth
        /// </summary>
        protected int cellIncr;

        /// <summary>
        /// whether to draw labels on cells.
        /// </summary>
        protected bool labelCells = true;

        /// <summary>
        /// The color of the border drawn around selected cells
        /// </summary>
        protected Color selectionBorderColor = Color.Black;

        /// <summary>
        /// The width of the border drawn around selected cells
        /// </summary>
        protected int selectionBorderWidth = 2;

        /// <summary>
        /// The brush to use when drawing an empty cell
        /// </summary>
        protected Brush emptyCellBrush = Brushes.Blue;

        /// <summary>
        /// This dictionary holds all the currently active cells.
        /// </summary>
        protected Dictionary<Point, ImageGridCell> cells;

        /// <summary>
        /// The cell coordinates of the cells that are currently visible.
        /// </summary>
        protected Rectangle visibleCoords;

        /// <summary>
        /// This dictionary keeps track of the currently selected cells.
        /// Only keys are used.  The values are ignored.
        /// </summary>
        protected Dictionary<Point, int> selectedCells;

        protected Pen selectionPen;

        // drag support
        protected bool dragging = false;
        protected Size dragSize;
        protected Pen dragPen;
        protected Color dragOutlineColor = Color.Black;
        protected ImageGridDragFinished dragFinished;
        protected object dragArg;

        #endregion Member Variables

        #region Events

        /// <summary>
        /// VisibleCellsChange event occurs whenever the visible cells change, which occurs
        /// when the control is enabled, when the control is resized, or when it is scrolled.
        /// </summary>
        public event VisibleCellsChangeEvent VisibleCellsChange;
        public event UserSelectionChangeEvent UserSelectionChange;

        #endregion Events

        public ImageGrid()
        {
            cellIncr = cellSize + cellBorderWidth;

            this.DoubleBuffered = true;
            AutoScroll = true;

            InitializeComponent();

            UpdateSize();

            cells = new Dictionary<Point, ImageGridCell>();

            selectedCells = new Dictionary<Point, int>();
            UpdateSelectionPen();

            // create drag pen
            dragPen = new Pen(dragOutlineColor, selectionBorderWidth);
            dragPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
        }

        public ImageGridCell GetCell(int x, int y)
        {
            Point key = new Point(x, y);

            ImageGridCell cell;

            cells.TryGetValue(key, out cell);

            //if (cell == null)
            //{
            //    cell = CreateCell(x, y);
            //}

            return cell;
        }

        public ImageGridCell CreateCell(int x, int y)
        {
            Point key = new Point(x, y);

            ImageGridCell cell = new ImageGridCell(x, y);
            cells.Add(key, cell);

            return cell;
        }

        public void FreeCell(ImageGridCell cell)
        {
            cells.Remove(new Point(cell.X, cell.Y));
        }

        public void ClearCells()
        {
            cells.Clear();

            Invalidate();
        }

        protected void UpdateSize()
        {
            int w = cellSize * widthCells + cellBorderWidth * (widthCells + 1);
            int h = cellSize * heightCells + cellBorderWidth * (heightCells + 1);
            AutoScrollMinSize = new Size(w, h);
            UpdateVisibleCoords();
            Invalidate();
        }

        protected void UpdateSelectionPen()
        {
            if (selectionPen != null)
            {
                selectionPen.Dispose();
            }

            selectionPen = new Pen(selectionBorderColor, selectionBorderWidth);
        }

        protected void PaintCell(Graphics g, int x, int y)
        {
            ImageGridCell cell;
            Point cellCoord = new Point(x,y);
            bool gotCell = cells.TryGetValue(cellCoord, out cell);

            Rectangle r = new Rectangle(x * cellIncr + cellBorderWidth + AutoScrollPosition.X,
                y * cellIncr + cellBorderWidth + AutoScrollPosition.Y, cellSize, cellSize);

            Brush brush = emptyCellBrush;
            string labelString;
            ImageGridCellType type;

            if ((cell == null) || (cell.Type == ImageGridCellType.None))
            {
                labelString = String.Format("{0}, {1}", x, y);
                type = ImageGridCellType.None;
            }
            else
            {
                type = cell.Type;
                labelString = cell.Label;
                if (type == ImageGridCellType.Color)
                {
                    brush = new SolidBrush(cell.Color);
                }
            }

            if (type == ImageGridCellType.Image)
            {
                g.DrawImageUnscaled(cell.Image, r);
            }
            else
            {
                g.FillRectangle(brush, r);
            }

            if (labelCells)
            {
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                g.DrawString(labelString, this.Font, Brushes.White, r, stringFormat);
            }

            if (selectedCells.ContainsKey(cellCoord))
            {
                g.DrawRectangle(selectionPen, r);
            }

            if (dragging)
            {
                if ((x >= lastMouseCell.X) && (x < (lastMouseCell.X + dragSize.Width)) &&
                    (y >= lastMouseCell.Y) && (y < (lastMouseCell.Y + dragSize.Height)))
                {
                    g.DrawRectangle(dragPen, r);
                }
            }

        }

        protected void PaintGrid(Graphics g, int cellX, int cellY, int w, int h)
        {
            int cellIncr = cellSize + cellBorderWidth;

            int maxX = cellX + w;
            int maxY = cellY + h;

            if (maxX >= widthCells)
            {
                maxX = widthCells;
            }
            if (maxY >= heightCells)
            {
                maxY = heightCells;
            }

            for (int x = cellX; x < maxX; x++)
            {
                for (int y = cellY; y < maxY; y++)
                {
                    PaintCell(g, x, y);
                }
            }
        }

        public void ClearSelection()
        {
            selectedCells.Clear();
            Invalidate();
        }

        public void AddSelectedCell(int x, int y)
        {
            selectedCells.Add(new Point(x, y), 0);
            Invalidate();
        }

        #region Properties

        public int WidthCells
        {
            get
            {
                return widthCells;
            }
            set
            {
                widthCells = value;
                UpdateSize();
            }
        }

        public int HeightCells
        {
            get
            {
                return heightCells;
            }
            set
            {
                heightCells = value;
                UpdateSize();
            }
        }

        public int CellBorderWidth
        {
            get
            {
                return cellBorderWidth;
            }
            set
            {
                cellBorderWidth = value;
                cellIncr = cellSize + cellBorderWidth;
                UpdateSize();
            }
        }

        public int CellSize
        {
            get
            {
                return cellSize;
            }
            set
            {
                cellSize = value;
                cellIncr = cellSize + cellBorderWidth;
                UpdateSize();
            }
        }

        public bool LabelCells
        {
            get
            {
                return labelCells;
            }
            set
            {
                labelCells = value;
                Invalidate();
            }
        }

        public Rectangle VisibleCoords
        {
            get
            {
                return visibleCoords;
            }
        }

        public int SelectionBorderWidth
        {
            get
            {
                return selectionBorderWidth;
            }
            set
            {
                selectionBorderWidth = value;
                UpdateSelectionPen();
            }
        }

        public Color SelectionBorderColor
        {
            get
            {
                return selectionBorderColor;
            }
            set
            {
                selectionBorderColor = value;
                UpdateSelectionPen();
            }
        }

        public List<Point> SelectedCells
        {
            get
            {
                return new List<Point>(selectedCells.Keys);
            }
        }

        #endregion Properties

        public void BeginDrag(Size dragSize, ImageGridDragFinished callback, object arg)
        {
            if (dragging)
            {
                throw new ArgumentException("already dragging");
            }

            dragFinished = callback;
            this.dragSize = dragSize;
            dragging = true;
            dragArg = arg;
            Invalidate();

        }

        protected Rectangle PixelToCellCoords(Rectangle pixCoords)
        {
            int px = pixCoords.X - AutoScrollPosition.X;
            int py = pixCoords.Y - AutoScrollPosition.Y;

            int x = px / cellIncr;
            int y = py / cellIncr;

            int x2 = (px + pixCoords.Width + cellBorderWidth) / cellIncr;
            int y2 = (py + pixCoords.Height + cellBorderWidth) / cellIncr;

            return new Rectangle(x, y, x2 - x + 1, y2 - y + 1);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (Enabled)
            {
                Rectangle gridCoords = PixelToCellCoords(pe.ClipRectangle);

                PaintGrid(pe.Graphics, gridCoords.X, gridCoords.Y, gridCoords.Width, gridCoords.Height);
            }

            // Calling the base class OnPaint
            base.OnPaint(pe);
        }

        protected ImageGridCell lastToolTipCell = null;

        protected Point lastMouseLoc;
        protected Point lastMouseCell;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // compute cell coord of event location
            int gridX = e.Location.X - AutoScrollPosition.X;
            int gridY = e.Location.Y - AutoScrollPosition.Y;

            Point CellCoord = new Point(gridX / cellIncr, gridY / cellIncr);

            if (dragging && (CellCoord != lastMouseCell))
            {
                // Might want to optimize
                Invalidate();
            }

            // save mouse location
            lastMouseLoc = e.Location;
            lastMouseCell = CellCoord;

            // get the cell
            ImageGridCell cell;
            cells.TryGetValue(CellCoord, out cell);

            // hide or show the tooltip
            if ((cell != null) && (cell.ToolTipText != null))
            {
                if (cell != lastToolTipCell)
                {
                    Point pt = e.Location;
                    pt.Y += cellSize / 2;

                    toolTip.Show(cell.ToolTipText, this, pt);
                    lastToolTipCell = cell;
                }
            }
            else
            {
                toolTip.Hide(base.FindForm());
            }

            base.OnMouseMove(e);
        }

        protected Point lastSelectedCell = Point.Empty;

        protected override void OnMouseClick(MouseEventArgs e)
        {
            // get state of modifier keys
            bool shift = (ModifierKeys == Keys.Shift);
            bool ctrl = (ModifierKeys == Keys.Control);

            // compute cell coordiate
            int gridX = e.Location.X - AutoScrollPosition.X;
            int gridY = e.Location.Y - AutoScrollPosition.Y;
            Point cellCoord = new Point(gridX / cellIncr, gridY / cellIncr);

            if (dragging)
            {
                bool cancelled;
                cancelled = (e.Button == MouseButtons.Right);

                bool finish = dragFinished(cancelled, cellCoord, dragArg);

                if (finish)
                {
                    dragging = false;
                    Invalidate();
                }
            }
            else if (shift)
            { // handle shift-click
                if (lastSelectedCell == Point.Empty)
                {
                    lastSelectedCell = cellCoord;
                }

                // clear current selection
                selectedCells.Clear();

                // sort coordinates for selection rectangle
                int x1, x2, y1, y2;
                if (lastSelectedCell.X < cellCoord.X)
                {
                    x1 = lastSelectedCell.X;
                    x2 = cellCoord.X;
                }
                else
                {
                    x1 = cellCoord.X;
                    x2 = lastSelectedCell.X;
                }
                if (lastSelectedCell.Y < cellCoord.Y)
                {
                    y1 = lastSelectedCell.Y;
                    y2 = cellCoord.Y;
                }
                else
                {
                    y1 = cellCoord.Y;
                    y2 = lastSelectedCell.Y;
                }

                // add all cells in selection rectangle to current selection
                for (int y = y1; y <= y2; y++)
                {
                    for (int x = x1; x <= x2; x++)
                    {
                        selectedCells.Add(new Point(x, y), 0);
                    }
                }
                OnUserSelectionChange();
            }
            else if (ctrl)
            { // handle ctrl-click
                // toggle cell
                if (selectedCells.ContainsKey(cellCoord))
                {
                    selectedCells.Remove(cellCoord);
                }
                else
                {
                    selectedCells.Add(cellCoord, 0);
                }

                // save as last selected cell
                lastSelectedCell = cellCoord;

                OnUserSelectionChange();
            }
            else
            { // handle regular click
                // reset selected cell
                selectedCells.Clear();
                selectedCells.Add(cellCoord, 0);

                // save as last selected cell
                lastSelectedCell = cellCoord;

                OnUserSelectionChange();
            }

            Invalidate();

            base.OnMouseClick(e);
        }

        protected virtual void OnUserSelectionChange()
        {
            UserSelectionChangeEvent handler = UserSelectionChange;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected virtual void OnVisibleCellsChange()
        {
            VisibleCellsChangeEvent handler = VisibleCellsChange;
            if (handler != null)
            {
                VisibleCellsChangeEventArgs args = new VisibleCellsChangeEventArgs(this.VisibleCoords);
                handler(this, args);
            }
        }

        protected void UpdateVisibleCoords()
        {
            Rectangle newCoords = PixelToCellCoords(new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height));

            if (visibleCoords != newCoords)
            {
                visibleCoords = newCoords;

                OnVisibleCellsChange();
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            UpdateVisibleCoords();

            base.OnScroll(se);
        }

    }

    #region Event Arg Classes

    public class VisibleCellsChangeEventArgs : EventArgs
    {
        protected Rectangle cellsVisible;

        public VisibleCellsChangeEventArgs(Rectangle visibleRect)
        {
            cellsVisible = visibleRect;
        }

        public Rectangle CellsVisible
        {
            get
            {
                return cellsVisible;
            }
        }
    }

    #endregion Event Arg Classes

    #region Event Delegates

    public delegate void VisibleCellsChangeEvent(object sender, VisibleCellsChangeEventArgs args);
    public delegate void UserSelectionChangeEvent(object sender, EventArgs args);

    #endregion Event Delegates
}
