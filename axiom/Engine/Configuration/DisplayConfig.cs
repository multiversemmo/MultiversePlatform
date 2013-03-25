using System;
using System.Collections.Generic;

namespace Axiom.Configuration
{
    /// <summary>
    /// Summary description for XConfig.
    /// </summary>
    public class DisplayMode
    {
        protected int width;
        protected int height;
        protected int depth;
        protected bool fullscreen;

        public DisplayMode(int width, int height, int depth, bool fullscreen)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.fullscreen = fullscreen;
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public int Depth
        {
            get
            {
                return depth;
            }
        }

        public bool Fullscreen
        {
            get
            {
                return fullscreen;
            }
        }
    }

    public class DisplayConfig
    {
        protected List<DisplayMode> fullscreenModes = new List<DisplayMode>();

        protected DisplayMode desktopMode;

        protected DisplayMode selectedMode;

        public DisplayConfig()
        {
        }

        public List<DisplayMode> FullscreenModes
        {
            get
            {
                return fullscreenModes;
            }
        }

        public DisplayMode SelectedMode
        {
            get
            {
                return selectedMode;
            }
        }

        protected DisplayMode FindMatchingMode(int width, int height, int depth)
        {
            foreach (DisplayMode mode in FullscreenModes)
            {
                if (mode.Fullscreen && mode.Width == width && mode.Height == height && mode.Depth == depth)
                {
                    return mode;
                }
            }

            return null;
        }

        public void SelectMode(int width, int height, int depth, bool fullscreen)
        {
            if (fullscreen)
            {
                // if selecting a fullscreen mode, must match one supported by the hardware
                selectedMode = FindMatchingMode(width, height, depth);
            }
            else
            {
                // if running in a window, can use any size
                selectedMode = new DisplayMode(width, height, depth, false);
            }
        }
    }
}