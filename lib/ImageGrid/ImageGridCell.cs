using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Multiverse.Controls
{
    public enum ImageGridCellType
    {
        None,
        Color,
        Image
    }

    public class ImageGridCell
    {
        protected int x;
        protected int y;

        protected ImageGridCellType type;

        protected Image image;

        protected Color color;

        protected string label;

        protected string toolTipText;

        public ImageGridCell(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.type = ImageGridCellType.None;
        }

        #region Properties

        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                type = ImageGridCellType.Color;
            }
        }

        public Image Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                if (image != null)
                {
                    type = ImageGridCellType.Image;
                }
            }
        }

        public ImageGridCellType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        public string Label
        {
            get
            {
                return label;
            }
            set
            {
                label = value;
            }
        }

        public string ToolTipText
        {
            get
            {
                return toolTipText;
            }
            set
            {
                toolTipText = value;
            }
        }

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
        }

        public Point Coord
        {
            get
            {
                return new Point(x, y);
            }
        }

        #endregion Properties

    }
}
