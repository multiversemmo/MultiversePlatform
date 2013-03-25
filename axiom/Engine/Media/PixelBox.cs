using System;
using System.IO;
using System.Diagnostics;
using Axiom.Core;

namespace Axiom.Media {


	///<summary>
	///    Structure used to define a box in a 3-D integer space.
	///    Note that the left, top, and front edges are included but the right, 
	///    bottom and top ones are not.
	///</summary>
	public class BasicBox {

        #region Fields

		protected int left;
		protected int top;
		protected int right;
		protected int bottom;
		protected int front;
		protected int back;

		#endregion Fields

		#region Constructors

		///<summary>
		///    Parameterless constructor for setting the members manually
		///</summary>
        public BasicBox() {
		}

		///<summary>
		///    Define a box from left, top, right and bottom coordinates
		///    This box will have depth one (front=0 and back=1).
		///</summary>
		///<param name="left">x value of left edge</param>
		///<param name="top">y value of top edge</param>
		///<param name="right">x value of right edge</param>
		///<param name="bottom">y value of bottom edge</param>
		///<remarks>
		///    Note that the left, top, and front edges are included 
		///    but the right, bottom and top ones are not.
		///</remarks>
		public BasicBox(int left, int top, int right, int bottom) {
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
			this.front = 0;
			this.back = 1;
			Debug.Assert(right >= left && bottom >= top && back >= front);
		}

		///<summary>
		///    Define a box from left, top, front, right, bottom and back
		///    coordinates.
		///</summary>
		///<param name="left">x value of left edge</param>
		///<param name="top">y value of top edge</param>
		///<param name="front">z value of front edge</param>
		///<param name="right">x value of right edge</param>
		///<param name="bottom">y value of bottom edge</param>
		///<param name="back">z value of back edge</param>
		///<remarks>
		///    Note that the left, top, and front edges are included 
		///    but the right, bottom and back ones are not.
		///</remarks>
        public BasicBox(int left, int top, int front, int right, int bottom, int back) {
			this.left = left;
			this.top = top;
            this.front = front;
            this.right = right;
			this.bottom = bottom;
            this.back = back;
			Debug.Assert(right >= left && bottom >= top && back >= front);
		}
            
		#endregion Constructors

		#region Properties

		public int Left {
			get { return left; }
			set { left = value; }
		}
		
		public int Top {
			get { return top; }
			set { top = value; }
		}
		
		public int Right {
			get { return right; }
			set { right = value; }
		}
		
		public int Bottom {
			get { return bottom; }
			set { bottom = value; }
		}
		
		public int Front {
			get { return front; }
			set { front = value; }
		}
		
		public int Back {
			get { return back; }
			set { back = value; }
		}
		

		///<summary>
		///    Get the width of this box
		///</summary>
		public int Width {
			get { return right-left; }
		}

		///<summary>
		///    Get the height of this box
		///</summary>
        public int Height {
			get { return bottom-top; }
		}

		///<summary>
		///    Get the depth of this box
		///</summary>
        public int Depth {
			get { return back-front; }
        }

		#endregion Properties

		#region Methods

		///<summary>
		///    Return true if the other box is a part of this one
		///</summary>
        public bool Contains(BasicBox def) {
			return (def.Left >= left && def.top >= top && def.front >= front &&
					def.right <= right && def.bottom <= bottom && def.back <= back);
		}
            
		public void CopyFromBasicBox(BasicBox src) {
			left = src.left;
			top = src.top;
			front = src.front;
			right = src.right;
			bottom = src.bottom;
			back = src.back;
		}

		#endregion Methods
		
	}
	

	///<summary>
	///    A primitive describing a volume (3D), image (2D) or line (1D) of pixels in memory.
	///    In case of a rectangle, depth must be 1. 
	///    Pixels are stored as a succession of "depth" slices, each containing "height" rows of 
	///    "width" pixels.
	///</summary>
	public class PixelBox : BasicBox {

		#region Fields

        ///<summary>
		///    The data pointer.  We do not own this.
        ///</summary>
		protected IntPtr data;
        ///<summary>
		///    A byte offset into the data
        ///</summary>
		protected int offset;
        ///<summary>
		///    The pixel format 
        ///</summary>
		protected PixelFormat format;
        ///<summary>
		///    Number of elements between the leftmost pixel of one row and the left
		///    pixel of the next. This value must always be equal to getWidth() (consecutive) 
		///    for compressed formats.
        ///</summary>
        protected int rowPitch;
        ///<summary>
		///    Number of elements between the top left pixel of one (depth) slice and 
		///    the top left pixel of the next. This can be a negative value. Must be a multiple of
		///    rowPitch. This value must always be equal to getWidth()*getHeight() (consecutive) 
		///    for compressed formats.
        ///</summary>
        protected int slicePitch;

		#endregion Fields

		#region Constructors

        ///<summary>
		///    Parameter constructor for setting the members manually
        ///</summary>
		public PixelBox() {}

        ///<summary>
		///    Constructor providing extents in the form of a Box object. This constructor
		///    assumes the pixel data is laid out consecutively in memory. (this
		///    means row after row, slice after slice, with no space in between)
        ///</summary>
		///<param name="extents">Extents of the region defined by data</param>
		///<param name="ormat">Format of this buffer</param>
		///<param name="data">Pointer to the actual data</param>
		protected PixelBox(BasicBox extents, PixelFormat format, IntPtr data) {
			CopyFromBasicBox(extents);
			this.format = format;
			this.data = data;
			this.offset = 0;
			SetConsecutive();
		}

		public PixelBox(BasicBox extents, PixelFormat format) {
			CopyFromBasicBox(extents);
			this.format = format;
			this.offset = 0;
			SetConsecutive();
		}

        ///<summary>
		///    Constructor providing width, height and depth. This constructor
		///    assumes the pixel data is laid out consecutively in memory. (this
		///    means row after row, slice after slice, with no space in between)
        ///</summary>
		///<param name="width">Width of the region</param>
		///<param name="height">Height of the region</param>
		///<param name="depth">Depth of the region</param>
		///<param name="format">Format of this buffer</param>
		///<param name="data">Pointer to the actual data</param>
		public PixelBox(int width, int height, int depth, PixelFormat format, IntPtr data) :
			base(0, 0, 0, width, height, depth) {
    		this.format = format;
			this.data = data;
    		this.offset = 0;
			SetConsecutive();
    	}
    	
		public PixelBox(int width, int height, int depth, PixelFormat format) :
			base(0, 0, 0, width, height, depth) {
    		this.format = format;
    		SetConsecutive();
    	}
        
		#endregion Constructors

		#region Properties

        ///<summary>
		///    Get/set the data array
        ///</summary>
        public IntPtr Data {
			get { return data; }
			set { data = value; }
		}
		
        ///<summary>
		///    Get/set the offset into the data array
        ///</summary>
        public int Offset {
			get { return offset; }
			set { offset = value; }
		}
		
        ///<summary>
		///    Get/set the pixel format
        ///</summary>
        public PixelFormat Format {
			get { return format; }
			set { format = value; }
		}
		
        ///<summary>
        ///</summary>
        public int RowPitch {
			get { return rowPitch; }
			set { rowPitch = value; }
		}
		
        ///<summary>
		///    Get the number of elements between one past the rightmost pixel of 
		///    one row and the leftmost pixel of the next row. (IE this is zero if rows
		///    are consecutive).
        ///</summary>
        public int RowSkip {
			get { return rowPitch - Width; }
		}
		
        ///<summary>
        ///</summary>
        public int SlicePitch {
			get { return slicePitch; }
			set { slicePitch = value; }
		}
		
        ///<summary>
		///    Get the number of elements between one past the right bottom pixel of
		///    one slice and the left top pixel of the next slice. (IE this is zero if slices
		///    are consecutive).
        ///</summary>
        public int SliceSkip {
			get { return slicePitch - (Height * rowPitch); }
		}

        ///<summary>
		///    Return whether this buffer is laid out consecutive in memory (ie the pitches
		///    are equal to the dimensions)
        ///</summary>
        public bool Consecutive {
            get { return rowPitch == Width && slicePitch == Width * Height; } 
		}

        ///<summary>
		///    Return the size (in bytes) this image would take if it was
		///    laid out consecutive in memory
        ///</summary>
		public int ConsecutiveSize {
            get { return PixelUtil.GetMemorySize(Width, Height, Depth, format); }
		}
	
		#endregion Properties	

        #region Methods

        ///<summary>
		///    Set the rowPitch and slicePitch so that the buffer is laid out consecutive 
		///    in memory.
        ///</summary>
        public void SetConsecutive() {
            rowPitch = Width;
            slicePitch = Width * Height;
        }

        ///<summary>
		///    I don't know how to figure this out.  For now, just deal with the DXT* formats
        ///</summary>
		public static bool Compressed(PixelFormat format) {
			return (format == PixelFormat.DXT1 ||
				    format == PixelFormat.DXT2 ||
					format == PixelFormat.DXT3 ||
					format == PixelFormat.DXT4 ||
					format == PixelFormat.DXT5);
		}

#if OLD_CODE // moved to PixelConverter to better match Ogre
		public static int GetMemorySize(int width, int height, int depth, PixelFormat format)
		{
			if(Compressed(format)) {
				switch(format) {
					// DXT formats work by dividing the image into 4x4 blocks, then encoding each
					// 4x4 block with a certain number of bytes. DXT can only be used on 2D images.
					case PixelFormat.DXT1:
						Debug.Assert(depth == 1);
						return ((width + 3) / 4) * ((height + 3) / 4) * 8;
					case PixelFormat.DXT2:
					case PixelFormat.DXT3:
					case PixelFormat.DXT4:
					case PixelFormat.DXT5:
						Debug.Assert(depth == 1);
						return ((width+3)/4)*((height+3)/4)*16;
					default:
						throw new Exception("Invalid compressed pixel format, in PixelBox.GetMemorySize");
				}
			} 
			else 
				return width * height * depth * PixelConverter.GetNumElemBytes(format); 
		}
#endif
      	/** Return a subvolume of this PixelBox.
      		@param def	Defines the bounds of the subregion to return
      		@returns	A pixel box describing the region and the data in it
      		@remarks	This function does not copy any data, it just returns
      			a PixelBox object with a data pointer pointing somewhere inside 
      			the data of object.
      		@throws	Exception(ERR_INVALIDPARAMS) if def is not fully contained
      	*/
      	public PixelBox GetSubVolume(BasicBox def) {
			if(Compressed(format)) {
				if(def.Left == left && def.Top == top && def.Front == front &&
				   def.Right == right && def.Bottom == bottom && def.Back == back)
					// Entire buffer is being queried
					return this;
				throw new Exception("Cannot return subvolume of compressed PixelBuffer, in PixelBox.GetSubVolume");
			}
			if(!Contains(def))
				throw new Exception("Bounds out of range, in PixelBox.GetSubVolume");

            int elemSize = PixelUtil.GetNumElemBytes(format);
			// Calculate new data origin
			PixelBox rval = new PixelBox(def, format, data);
			rval.offset =  (((def.Left - left)*elemSize) +
							((def.Top - top)*rowPitch*elemSize) +
							((def.Front - front)*slicePitch*elemSize));
			rval.rowPitch = rowPitch;
			rval.slicePitch = slicePitch;
			rval.format = format;
			return rval;
		}

		#endregion Methods

	}
}
