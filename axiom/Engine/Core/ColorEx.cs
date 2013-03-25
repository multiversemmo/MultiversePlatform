#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion
using System;

namespace Axiom.Core {
    /// <summary>
    ///		This class is necessary so we can store the color components as floating 
    ///		point values in the range [0,1].  It serves as an intermediary to System.Drawing.Color, which
    ///		stores them as byte values.  This doesn't allow for slow color component
    ///		interpolation, because with the values always being cast back to a byte would lose
    ///		any small interpolated values (i.e. 223 - .25 as a byte is 223).
    /// </summary>
    public class ColorEx : IComparable {
        #region Member variables

        /// <summary>
        ///		Alpha value [0,1].
        /// </summary>
        public float a;
        /// <summary>
        ///		Red color component [0,1].
        /// </summary>
        public float r;
        /// <summary>
        ///		Green color component [0,1].
        /// </summary>
        public float g;
        /// <summary>
        ///		Blue color component [0,1].
        /// </summary>
        public float b;

        #endregion

        #region Constructors

        /// <summary>
        ///		Default constructor.
        /// </summary>
        public ColorEx() {
            // set the color components to a default of 1;
            a = 1.0f;
            r = 1.0f;
            g = 1.0f;
            b = 1.0f;
        }

        /// <summary>
        ///		Default constructor.
        /// </summary>
        public ColorEx(float r, float g, float b) : this(1.0f, r, g, b) { }

        /// <summary>
        ///		Constructor taking all component values.
        /// </summary>
        /// <param name="a">Alpha value.</param>
        /// <param name="r">Red color component.</param>
        /// <param name="g">Green color component.</param>
        /// <param name="b">Blue color component.</param>
        public ColorEx(float a, float r, float g, float b) {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }

        /// <summary>
        ///		Copy constructor.
        /// </summary>
        public ColorEx(ColorEx other)
            : this() {
            if (other != null) {
                this.a = other.a;
                this.r = other.r;
                this.g = other.g;
                this.b = other.b;
            }
        }
        
        #endregion Constructors

        #region Methods

		/// <summary>
		///		Returns a copy of this ColorEx instance.
		/// </summary>
		/// <returns></returns>
		public ColorEx Clone() {
			return new ColorEx(a, r, g, b);
		}

        /// <summary>
        ///		Converts this instance to a <see cref="System.Drawing.Color"/> structure.
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Color ToColor() {
            return System.Drawing.Color.FromArgb((int)(a * 255.0f), (int)(r * 255.0f), (int)(g * 255.0f), (int)(b * 255.0f));
        }

        /// <summary>
        ///		Converts this color value to packed ABGR format.
        /// </summary>
        /// <returns></returns>
        public uint ToABGR() {
            uint result = 0;

            result += ((uint)(a * 255.0f)) << 24;
            result += ((uint)(b * 255.0f)) << 16;
            result += ((uint)(g * 255.0f)) << 8;
            result += ((uint)(r * 255.0f));

            return result;
        }

        /// <summary>
        ///		Converts this color value to packed ARBG format.
        /// </summary>
        /// <returns></returns>
        public uint ToARGB() {
            uint result = 0;

            result += ((uint)(a * 255.0f)) << 24;
            result += ((uint)(r * 255.0f)) << 16;
            result += ((uint)(g * 255.0f)) << 8;
            result += ((uint)(b * 255.0f));

            return result;
        }

        /// <summary>
        ///		Populates the color components in a 4 elements array in RGBA order.
        /// </summary>
        /// <remarks>
        ///		Primarily used to help in OpenGL.
        /// </remarks>
        /// <returns></returns>
        public void ToArrayRGBA(float[] vals) {
            vals[0] = r; vals[1] = g; vals[2] = b; vals[3] = a;
        }

        /// <summary>
        ///		Keep all color components in the allowed range of 0.0f .. 1.0f
        /// </summary>
        public void Saturate() {
            a = Math.Min(1.0f, Math.Max(0.0f, a));
            r = Math.Min(1.0f, Math.Max(0.0f, r));
            g = Math.Min(1.0f, Math.Max(0.0f, g));
            b = Math.Min(1.0f, Math.Max(0.0f, b));
        }

        /// <summary>
        ///		Static method used to create a new <code>ColorEx</code> instance based
        ///		on an existing <see cref="System.Drawing.Color"/> structure.
        /// </summary>
        /// <param name="color">.Net color structure to use as a basis.</param>
        /// <returns>A new <code>ColorEx instance.</code></returns>
        public static ColorEx FromColor(System.Drawing.Color color) {
            return new ColorEx((float)color.A / 255.0f, (float)color.R / 255.0f, (float)color.G / 255.0f, (float)color.B / 255.0f);
        }

		// arithmetic operations
        public static ColorEx operator + (ColorEx c1, ColorEx c2)
        {
            ColorEx result = new ColorEx();

            result.r = c1.r + c2.r;
            result.g = c1.g + c2.g;
            result.b = c1.b + c2.b;
            result.a = c1.a + c2.a;

            return result;
        }

        public static ColorEx operator - (ColorEx c1, ColorEx c2)
        {
            ColorEx result = new ColorEx();

            result.r = c1.r - c2.r;
            result.g = c1.g - c2.g;
            result.b = c1.b - c2.b;
            result.a = c1.a - c2.a;

            return result;
        }

        public static ColorEx operator * (ColorEx c1, ColorEx c2)
        {
            ColorEx result = new ColorEx();

            result.r = c1.r * c2.r;
            result.g = c1.g * c2.g;
            result.b = c1.b * c2.b;
            result.a = c1.a * c2.a;

            return result;
        }

        public static ColorEx operator * (ColorEx c, float scale)
        {
            ColorEx result = new ColorEx();

            result.r = c.r * scale;
            result.g = c.g * scale;
            result.b = c.b * scale;
            result.a = c.a * scale;

            return result;
        }

        #endregion

        #region Static color properties

        /// <summary>
        ///		The color Transparent.
        /// </summary>
        public static ColorEx Transparent {
            get {
                return new ColorEx(0f, 1f, 1f, 1f);
            }
        }

        /// <summary>
        ///		The color AliceBlue.
        /// </summary>
        public static ColorEx AliceBlue {
            get {
                return new ColorEx(1f, 0.9411765f, 0.972549f, 1f);
            }
        }

        /// <summary>
        ///		The color AntiqueWhite.
        /// </summary>
        public static ColorEx AntiqueWhite {
            get {
                return new ColorEx(1f, 0.9803922f, 0.9215686f, 0.8431373f);
            }
        }

        /// <summary>
        ///		The color Aqua.
        /// </summary>
        public static ColorEx Aqua {
            get {
                return new ColorEx(1f, 0f, 1f, 1f);
            }
        }

        /// <summary>
        ///		The color Aquamarine.
        /// </summary>
        public static ColorEx Aquamarine {
            get {
                return new ColorEx(1f, 0.4980392f, 1f, 0.8313726f);
            }
        }

        /// <summary>
        ///		The color Azure.
        /// </summary>
        public static ColorEx Azure {
            get {
                return new ColorEx(1f, 0.9411765f, 1f, 1f);
            }
        }

        /// <summary>
        ///		The color Beige.
        /// </summary>
        public static ColorEx Beige {
            get {
                return new ColorEx(1f, 0.9607843f, 0.9607843f, 0.8627451f);
            }
        }

        /// <summary>
        ///		The color Bisque.
        /// </summary>
        public static ColorEx Bisque {
            get {
                return new ColorEx(1f, 1f, 0.8941177f, 0.7686275f);
            }
        }

        /// <summary>
        ///		The color Black.
        /// </summary>
        public static ColorEx Black {
            get {
                return new ColorEx(1f, 0f, 0f, 0f);
            }
        }

        /// <summary>
        ///		The color BlanchedAlmond.
        /// </summary>
        public static ColorEx BlanchedAlmond {
            get {
                return new ColorEx(1f, 1f, 0.9215686f, 0.8039216f);
            }
        }

        /// <summary>
        ///		The color Blue.
        /// </summary>
        public static ColorEx Blue {
            get {
                return new ColorEx(1f, 0f, 0f, 1f);
            }
        }

        /// <summary>
        ///		The color BlueViolet.
        /// </summary>
        public static ColorEx BlueViolet {
            get {
                return new ColorEx(1f, 0.5411765f, 0.1686275f, 0.8862745f);
            }
        }

        /// <summary>
        ///		The color Brown.
        /// </summary>
        public static ColorEx Brown {
            get {
                return new ColorEx(1f, 0.6470588f, 0.1647059f, 0.1647059f);
            }
        }

        /// <summary>
        ///		The color BurlyWood.
        /// </summary>
        public static ColorEx BurlyWood {
            get {
                return new ColorEx(1f, 0.8705882f, 0.7215686f, 0.5294118f);
            }
        }

        /// <summary>
        ///		The color CadetBlue.
        /// </summary>
        public static ColorEx CadetBlue {
            get {
                return new ColorEx(1f, 0.372549f, 0.6196079f, 0.627451f);
            }
        }

        /// <summary>
        ///		The color Chartreuse.
        /// </summary>
        public static ColorEx Chartreuse {
            get {
                return new ColorEx(1f, 0.4980392f, 1f, 0f);
            }
        }

        /// <summary>
        ///		The color Chocolate.
        /// </summary>
        public static ColorEx Chocolate {
            get {
                return new ColorEx(1f, 0.8235294f, 0.4117647f, 0.1176471f);
            }
        }

        /// <summary>
        ///		The color Coral.
        /// </summary>
        public static ColorEx Coral {
            get {
                return new ColorEx(1f, 1f, 0.4980392f, 0.3137255f);
            }
        }

        /// <summary>
        ///		The color CornflowerBlue.
        /// </summary>
        public static ColorEx CornflowerBlue {
            get {
                return new ColorEx(1f, 0.3921569f, 0.5843138f, 0.9294118f);
            }
        }

        /// <summary>
        ///		The color Cornsilk.
        /// </summary>
        public static ColorEx Cornsilk {
            get {
                return new ColorEx(1f, 1f, 0.972549f, 0.8627451f);
            }
        }

        /// <summary>
        ///		The color Crimson.
        /// </summary>
        public static ColorEx Crimson {
            get {
                return new ColorEx(1f, 0.8627451f, 0.07843138f, 0.2352941f);
            }
        }

        /// <summary>
        ///		The color Cyan.
        /// </summary>
        public static ColorEx Cyan {
            get {
                return new ColorEx(1f, 0f, 1f, 1f);
            }
        }

        /// <summary>
        ///		The color DarkBlue.
        /// </summary>
        public static ColorEx DarkBlue {
            get {
                return new ColorEx(1f, 0f, 0f, 0.5450981f);
            }
        }

        /// <summary>
        ///		The color DarkCyan.
        /// </summary>
        public static ColorEx DarkCyan {
            get {
                return new ColorEx(1f, 0f, 0.5450981f, 0.5450981f);
            }
        }

        /// <summary>
        ///		The color DarkGoldenrod.
        /// </summary>
        public static ColorEx DarkGoldenrod {
            get {
                return new ColorEx(1f, 0.7215686f, 0.5254902f, 0.04313726f);
            }
        }

        /// <summary>
        ///		The color DarkGray.
        /// </summary>
        public static ColorEx DarkGray {
            get {
                return new ColorEx(1f, 0.6627451f, 0.6627451f, 0.6627451f);
            }
        }

        /// <summary>
        ///		The color DarkGreen.
        /// </summary>
        public static ColorEx DarkGreen {
            get {
                return new ColorEx(1f, 0f, 0.3921569f, 0f);
            }
        }

        /// <summary>
        ///		The color DarkKhaki.
        /// </summary>
        public static ColorEx DarkKhaki {
            get {
                return new ColorEx(1f, 0.7411765f, 0.7176471f, 0.4196078f);
            }
        }

        /// <summary>
        ///		The color DarkMagenta.
        /// </summary>
        public static ColorEx DarkMagenta {
            get {
                return new ColorEx(1f, 0.5450981f, 0f, 0.5450981f);
            }
        }

        /// <summary>
        ///		The color DarkOliveGreen.
        /// </summary>
        public static ColorEx DarkOliveGreen {
            get {
                return new ColorEx(1f, 0.3333333f, 0.4196078f, 0.1843137f);
            }
        }

        /// <summary>
        ///		The color DarkOrange.
        /// </summary>
        public static ColorEx DarkOrange {
            get {
                return new ColorEx(1f, 1f, 0.5490196f, 0f);
            }
        }

        /// <summary>
        ///		The color DarkOrchid.
        /// </summary>
        public static ColorEx DarkOrchid {
            get {
                return new ColorEx(1f, 0.6f, 0.1960784f, 0.8f);
            }
        }

        /// <summary>
        ///		The color DarkRed.
        /// </summary>
        public static ColorEx DarkRed {
            get {
                return new ColorEx(1f, 0.5450981f, 0f, 0f);
            }
        }

        /// <summary>
        ///		The color DarkSalmon.
        /// </summary>
        public static ColorEx DarkSalmon {
            get {
                return new ColorEx(1f, 0.9137255f, 0.5882353f, 0.4784314f);
            }
        }

        /// <summary>
        ///		The color DarkSeaGreen.
        /// </summary>
        public static ColorEx DarkSeaGreen {
            get {
                return new ColorEx(1f, 0.5607843f, 0.7372549f, 0.5450981f);
            }
        }

        /// <summary>
        ///		The color DarkSlateBlue.
        /// </summary>
        public static ColorEx DarkSlateBlue {
            get {
                return new ColorEx(1f, 0.282353f, 0.2392157f, 0.5450981f);
            }
        }

        /// <summary>
        ///		The color DarkSlateGray.
        /// </summary>
        public static ColorEx DarkSlateGray {
            get {
                return new ColorEx(1f, 0.1843137f, 0.3098039f, 0.3098039f);
            }
        }

        /// <summary>
        ///		The color DarkTurquoise.
        /// </summary>
        public static ColorEx DarkTurquoise {
            get {
                return new ColorEx(1f, 0f, 0.8078431f, 0.8196079f);
            }
        }

        /// <summary>
        ///		The color DarkViolet.
        /// </summary>
        public static ColorEx DarkViolet {
            get {
                return new ColorEx(1f, 0.5803922f, 0f, 0.827451f);
            }
        }

        /// <summary>
        ///		The color DeepPink.
        /// </summary>
        public static ColorEx DeepPink {
            get {
                return new ColorEx(1f, 1f, 0.07843138f, 0.5764706f);
            }
        }

        /// <summary>
        ///		The color DeepSkyBlue.
        /// </summary>
        public static ColorEx DeepSkyBlue {
            get {
                return new ColorEx(1f, 0f, 0.7490196f, 1f);
            }
        }

        /// <summary>
        ///		The color DimGray.
        /// </summary>
        public static ColorEx DimGray {
            get {
                return new ColorEx(1f, 0.4117647f, 0.4117647f, 0.4117647f);
            }
        }

        /// <summary>
        ///		The color DodgerBlue.
        /// </summary>
        public static ColorEx DodgerBlue {
            get {
                return new ColorEx(1f, 0.1176471f, 0.5647059f, 1f);
            }
        }

        /// <summary>
        ///		The color Firebrick.
        /// </summary>
        public static ColorEx Firebrick {
            get {
                return new ColorEx(1f, 0.6980392f, 0.1333333f, 0.1333333f);
            }
        }

        /// <summary>
        ///		The color FloralWhite.
        /// </summary>
        public static ColorEx FloralWhite {
            get {
                return new ColorEx(1f, 1f, 0.9803922f, 0.9411765f);
            }
        }

        /// <summary>
        ///		The color ForestGreen.
        /// </summary>
        public static ColorEx ForestGreen {
            get {
                return new ColorEx(1f, 0.1333333f, 0.5450981f, 0.1333333f);
            }
        }

        /// <summary>
        ///		The color Fuchsia.
        /// </summary>
        public static ColorEx Fuchsia {
            get {
                return new ColorEx(1f, 1f, 0f, 1f);
            }
        }

        /// <summary>
        ///		The color Gainsboro.
        /// </summary>
        public static ColorEx Gainsboro {
            get {
                return new ColorEx(1f, 0.8627451f, 0.8627451f, 0.8627451f);
            }
        }

        /// <summary>
        ///		The color GhostWhite.
        /// </summary>
        public static ColorEx GhostWhite {
            get {
                return new ColorEx(1f, 0.972549f, 0.972549f, 1f);
            }
        }

        /// <summary>
        ///		The color Gold.
        /// </summary>
        public static ColorEx Gold {
            get {
                return new ColorEx(1f, 1f, 0.8431373f, 0f);
            }
        }

        /// <summary>
        ///		The color Goldenrod.
        /// </summary>
        public static ColorEx Goldenrod {
            get {
                return new ColorEx(1f, 0.854902f, 0.6470588f, 0.1254902f);
            }
        }

        /// <summary>
        ///		The color Gray.
        /// </summary>
        public static ColorEx Gray {
            get {
                return new ColorEx(1f, 0.5019608f, 0.5019608f, 0.5019608f);
            }
        }

        /// <summary>
        ///		The color Green.
        /// </summary>
        public static ColorEx Green {
            get {
                return new ColorEx(1f, 0f, 0.5019608f, 0f);
            }
        }

        /// <summary>
        ///		The color GreenYellow.
        /// </summary>
        public static ColorEx GreenYellow {
            get {
                return new ColorEx(1f, 0.6784314f, 1f, 0.1843137f);
            }
        }

        /// <summary>
        ///		The color Honeydew.
        /// </summary>
        public static ColorEx Honeydew {
            get {
                return new ColorEx(1f, 0.9411765f, 1f, 0.9411765f);
            }
        }

        /// <summary>
        ///		The color HotPink.
        /// </summary>
        public static ColorEx HotPink {
            get {
                return new ColorEx(1f, 1f, 0.4117647f, 0.7058824f);
            }
        }

        /// <summary>
        ///		The color IndianRed.
        /// </summary>
        public static ColorEx IndianRed {
            get {
                return new ColorEx(1f, 0.8039216f, 0.3607843f, 0.3607843f);
            }
        }

        /// <summary>
        ///		The color Indigo.
        /// </summary>
        public static ColorEx Indigo {
            get {
                return new ColorEx(1f, 0.2941177f, 0f, 0.509804f);
            }
        }

        /// <summary>
        ///		The color Ivory.
        /// </summary>
        public static ColorEx Ivory {
            get {
                return new ColorEx(1f, 1f, 1f, 0.9411765f);
            }
        }

        /// <summary>
        ///		The color Khaki.
        /// </summary>
        public static ColorEx Khaki {
            get {
                return new ColorEx(1f, 0.9411765f, 0.9019608f, 0.5490196f);
            }
        }

        /// <summary>
        ///		The color Lavender.
        /// </summary>
        public static ColorEx Lavender {
            get {
                return new ColorEx(1f, 0.9019608f, 0.9019608f, 0.9803922f);
            }
        }

        /// <summary>
        ///		The color LavenderBlush.
        /// </summary>
        public static ColorEx LavenderBlush {
            get {
                return new ColorEx(1f, 1f, 0.9411765f, 0.9607843f);
            }
        }

        /// <summary>
        ///		The color LawnGreen.
        /// </summary>
        public static ColorEx LawnGreen {
            get {
                return new ColorEx(1f, 0.4862745f, 0.9882353f, 0f);
            }
        }

        /// <summary>
        ///		The color LemonChiffon.
        /// </summary>
        public static ColorEx LemonChiffon {
            get {
                return new ColorEx(1f, 1f, 0.9803922f, 0.8039216f);
            }
        }

        /// <summary>
        ///		The color LightBlue.
        /// </summary>
        public static ColorEx LightBlue {
            get {
                return new ColorEx(1f, 0.6784314f, 0.8470588f, 0.9019608f);
            }
        }

        /// <summary>
        ///		The color LightCoral.
        /// </summary>
        public static ColorEx LightCoral {
            get {
                return new ColorEx(1f, 0.9411765f, 0.5019608f, 0.5019608f);
            }
        }

        /// <summary>
        ///		The color LightCyan.
        /// </summary>
        public static ColorEx LightCyan {
            get {
                return new ColorEx(1f, 0.8784314f, 1f, 1f);
            }
        }

        /// <summary>
        ///		The color LightGoldenrodYellow.
        /// </summary>
        public static ColorEx LightGoldenrodYellow {
            get {
                return new ColorEx(1f, 0.9803922f, 0.9803922f, 0.8235294f);
            }
        }

        /// <summary>
        ///		The color LightGreen.
        /// </summary>
        public static ColorEx LightGreen {
            get {
                return new ColorEx(1f, 0.5647059f, 0.9333333f, 0.5647059f);
            }
        }

        /// <summary>
        ///		The color LightGray.
        /// </summary>
        public static ColorEx LightGray {
            get {
                return new ColorEx(1f, 0.827451f, 0.827451f, 0.827451f);
            }
        }

        /// <summary>
        ///		The color LightPink.
        /// </summary>
        public static ColorEx LightPink {
            get {
                return new ColorEx(1f, 1f, 0.7137255f, 0.7568628f);
            }
        }

        /// <summary>
        ///		The color LightSalmon.
        /// </summary>
        public static ColorEx LightSalmon {
            get {
                return new ColorEx(1f, 1f, 0.627451f, 0.4784314f);
            }
        }

        /// <summary>
        ///		The color LightSeaGreen.
        /// </summary>
        public static ColorEx LightSeaGreen {
            get {
                return new ColorEx(1f, 0.1254902f, 0.6980392f, 0.6666667f);
            }
        }

        /// <summary>
        ///		The color LightSkyBlue.
        /// </summary>
        public static ColorEx LightSkyBlue {
            get {
                return new ColorEx(1f, 0.5294118f, 0.8078431f, 0.9803922f);
            }
        }

        /// <summary>
        ///		The color LightSlateGray.
        /// </summary>
        public static ColorEx LightSlateGray {
            get {
                return new ColorEx(1f, 0.4666667f, 0.5333334f, 0.6f);
            }
        }

        /// <summary>
        ///		The color LightSteelBlue.
        /// </summary>
        public static ColorEx LightSteelBlue {
            get {
                return new ColorEx(1f, 0.6901961f, 0.7686275f, 0.8705882f);
            }
        }

        /// <summary>
        ///		The color LightYellow.
        /// </summary>
        public static ColorEx LightYellow {
            get {
                return new ColorEx(1f, 1f, 1f, 0.8784314f);
            }
        }

        /// <summary>
        ///		The color Lime.
        /// </summary>
        public static ColorEx Lime {
            get {
                return new ColorEx(1f, 0f, 1f, 0f);
            }
        }

        /// <summary>
        ///		The color LimeGreen.
        /// </summary>
        public static ColorEx LimeGreen {
            get {
                return new ColorEx(1f, 0.1960784f, 0.8039216f, 0.1960784f);
            }
        }

        /// <summary>
        ///		The color Linen.
        /// </summary>
        public static ColorEx Linen {
            get {
                return new ColorEx(1f, 0.9803922f, 0.9411765f, 0.9019608f);
            }
        }

        /// <summary>
        ///		The color Magenta.
        /// </summary>
        public static ColorEx Magenta {
            get {
                return new ColorEx(1f, 1f, 0f, 1f);
            }
        }

        /// <summary>
        ///		The color Maroon.
        /// </summary>
        public static ColorEx Maroon {
            get {
                return new ColorEx(1f, 0.5019608f, 0f, 0f);
            }
        }

        /// <summary>
        ///		The color MediumAquamarine.
        /// </summary>
        public static ColorEx MediumAquamarine {
            get {
                return new ColorEx(1f, 0.4f, 0.8039216f, 0.6666667f);
            }
        }

        /// <summary>
        ///		The color MediumBlue.
        /// </summary>
        public static ColorEx MediumBlue {
            get {
                return new ColorEx(1f, 0f, 0f, 0.8039216f);
            }
        }

        /// <summary>
        ///		The color MediumOrchid.
        /// </summary>
        public static ColorEx MediumOrchid {
            get {
                return new ColorEx(1f, 0.7294118f, 0.3333333f, 0.827451f);
            }
        }

        /// <summary>
        ///		The color MediumPurple.
        /// </summary>
        public static ColorEx MediumPurple {
            get {
                return new ColorEx(1f, 0.5764706f, 0.4392157f, 0.8588235f);
            }
        }

        /// <summary>
        ///		The color MediumSeaGreen.
        /// </summary>
        public static ColorEx MediumSeaGreen {
            get {
                return new ColorEx(1f, 0.2352941f, 0.7019608f, 0.4431373f);
            }
        }

        /// <summary>
        ///		The color MediumSlateBlue.
        /// </summary>
        public static ColorEx MediumSlateBlue {
            get {
                return new ColorEx(1f, 0.4823529f, 0.4078431f, 0.9333333f);
            }
        }

        /// <summary>
        ///		The color MediumSpringGreen.
        /// </summary>
        public static ColorEx MediumSpringGreen {
            get {
                return new ColorEx(1f, 0f, 0.9803922f, 0.6039216f);
            }
        }

        /// <summary>
        ///		The color MediumTurquoise.
        /// </summary>
        public static ColorEx MediumTurquoise {
            get {
                return new ColorEx(1f, 0.282353f, 0.8196079f, 0.8f);
            }
        }

        /// <summary>
        ///		The color MediumVioletRed.
        /// </summary>
        public static ColorEx MediumVioletRed {
            get {
                return new ColorEx(1f, 0.7803922f, 0.08235294f, 0.5215687f);
            }
        }

        /// <summary>
        ///		The color MidnightBlue.
        /// </summary>
        public static ColorEx MidnightBlue {
            get {
                return new ColorEx(1f, 0.09803922f, 0.09803922f, 0.4392157f);
            }
        }

        /// <summary>
        ///		The color MintCream.
        /// </summary>
        public static ColorEx MintCream {
            get {
                return new ColorEx(1f, 0.9607843f, 1f, 0.9803922f);
            }
        }

        /// <summary>
        ///		The color MistyRose.
        /// </summary>
        public static ColorEx MistyRose {
            get {
                return new ColorEx(1f, 1f, 0.8941177f, 0.8823529f);
            }
        }

        /// <summary>
        ///		The color Moccasin.
        /// </summary>
        public static ColorEx Moccasin {
            get {
                return new ColorEx(1f, 1f, 0.8941177f, 0.7098039f);
            }
        }

        /// <summary>
        ///		The color NavajoWhite.
        /// </summary>
        public static ColorEx NavajoWhite {
            get {
                return new ColorEx(1f, 1f, 0.8705882f, 0.6784314f);
            }
        }

        /// <summary>
        ///		The color Navy.
        /// </summary>
        public static ColorEx Navy {
            get {
                return new ColorEx(1f, 0f, 0f, 0.5019608f);
            }
        }

        /// <summary>
        ///		The color OldLace.
        /// </summary>
        public static ColorEx OldLace {
            get {
                return new ColorEx(1f, 0.9921569f, 0.9607843f, 0.9019608f);
            }
        }

        /// <summary>
        ///		The color Olive.
        /// </summary>
        public static ColorEx Olive {
            get {
                return new ColorEx(1f, 0.5019608f, 0.5019608f, 0f);
            }
        }

        /// <summary>
        ///		The color OliveDrab.
        /// </summary>
        public static ColorEx OliveDrab {
            get {
                return new ColorEx(1f, 0.4196078f, 0.5568628f, 0.1372549f);
            }
        }

        /// <summary>
        ///		The color Orange.
        /// </summary>
        public static ColorEx Orange {
            get {
                return new ColorEx(1f, 1f, 0.6470588f, 0f);
            }
        }

        /// <summary>
        ///		The color OrangeRed.
        /// </summary>
        public static ColorEx OrangeRed {
            get {
                return new ColorEx(1f, 1f, 0.2705882f, 0f);
            }
        }

        /// <summary>
        ///		The color Orchid.
        /// </summary>
        public static ColorEx Orchid {
            get {
                return new ColorEx(1f, 0.854902f, 0.4392157f, 0.8392157f);
            }
        }

        /// <summary>
        ///		The color PaleGoldenrod.
        /// </summary>
        public static ColorEx PaleGoldenrod {
            get {
                return new ColorEx(1f, 0.9333333f, 0.9098039f, 0.6666667f);
            }
        }

        /// <summary>
        ///		The color PaleGreen.
        /// </summary>
        public static ColorEx PaleGreen {
            get {
                return new ColorEx(1f, 0.5960785f, 0.9843137f, 0.5960785f);
            }
        }

        /// <summary>
        ///		The color PaleTurquoise.
        /// </summary>
        public static ColorEx PaleTurquoise {
            get {
                return new ColorEx(1f, 0.6862745f, 0.9333333f, 0.9333333f);
            }
        }

        /// <summary>
        ///		The color PaleVioletRed.
        /// </summary>
        public static ColorEx PaleVioletRed {
            get {
                return new ColorEx(1f, 0.8588235f, 0.4392157f, 0.5764706f);
            }
        }

        /// <summary>
        ///		The color PapayaWhip.
        /// </summary>
        public static ColorEx PapayaWhip {
            get {
                return new ColorEx(1f, 1f, 0.9372549f, 0.8352941f);
            }
        }

        /// <summary>
        ///		The color PeachPuff.
        /// </summary>
        public static ColorEx PeachPuff {
            get {
                return new ColorEx(1f, 1f, 0.854902f, 0.7254902f);
            }
        }

        /// <summary>
        ///		The color Peru.
        /// </summary>
        public static ColorEx Peru {
            get {
                return new ColorEx(1f, 0.8039216f, 0.5215687f, 0.2470588f);
            }
        }

        /// <summary>
        ///		The color Pink.
        /// </summary>
        public static ColorEx Pink {
            get {
                return new ColorEx(1f, 1f, 0.7529412f, 0.7960784f);
            }
        }

        /// <summary>
        ///		The color Plum.
        /// </summary>
        public static ColorEx Plum {
            get {
                return new ColorEx(1f, 0.8666667f, 0.627451f, 0.8666667f);
            }
        }

        /// <summary>
        ///		The color PowderBlue.
        /// </summary>
        public static ColorEx PowderBlue {
            get {
                return new ColorEx(1f, 0.6901961f, 0.8784314f, 0.9019608f);
            }
        }

        /// <summary>
        ///		The color Purple.
        /// </summary>
        public static ColorEx Purple {
            get {
                return new ColorEx(1f, 0.5019608f, 0f, 0.5019608f);
            }
        }

        /// <summary>
        ///		The color Red.
        /// </summary>
        public static ColorEx Red {
            get {
                return new ColorEx(1f, 1f, 0f, 0f);
            }
        }

        /// <summary>
        ///		The color RosyBrown.
        /// </summary>
        public static ColorEx RosyBrown {
            get {
                return new ColorEx(1f, 0.7372549f, 0.5607843f, 0.5607843f);
            }
        }

        /// <summary>
        ///		The color RoyalBlue.
        /// </summary>
        public static ColorEx RoyalBlue {
            get {
                return new ColorEx(1f, 0.254902f, 0.4117647f, 0.8823529f);
            }
        }

        /// <summary>
        ///		The color SaddleBrown.
        /// </summary>
        public static ColorEx SaddleBrown {
            get {
                return new ColorEx(1f, 0.5450981f, 0.2705882f, 0.07450981f);
            }
        }

        /// <summary>
        ///		The color Salmon.
        /// </summary>
        public static ColorEx Salmon {
            get {
                return new ColorEx(1f, 0.9803922f, 0.5019608f, 0.4470588f);
            }
        }

        /// <summary>
        ///		The color SandyBrown.
        /// </summary>
        public static ColorEx SandyBrown {
            get {
                return new ColorEx(1f, 0.9568627f, 0.6431373f, 0.3764706f);
            }
        }

        /// <summary>
        ///		The color SeaGreen.
        /// </summary>
        public static ColorEx SeaGreen {
            get {
                return new ColorEx(1f, 0.1803922f, 0.5450981f, 0.3411765f);
            }
        }

        /// <summary>
        ///		The color SeaShell.
        /// </summary>
        public static ColorEx SeaShell {
            get {
                return new ColorEx(1f, 1f, 0.9607843f, 0.9333333f);
            }
        }

        /// <summary>
        ///		The color Sienna.
        /// </summary>
        public static ColorEx Sienna {
            get {
                return new ColorEx(1f, 0.627451f, 0.3215686f, 0.1764706f);
            }
        }

        /// <summary>
        ///		The color Silver.
        /// </summary>
        public static ColorEx Silver {
            get {
                return new ColorEx(1f, 0.7529412f, 0.7529412f, 0.7529412f);
            }
        }

        /// <summary>
        ///		The color SkyBlue.
        /// </summary>
        public static ColorEx SkyBlue {
            get {
                return new ColorEx(1f, 0.5294118f, 0.8078431f, 0.9215686f);
            }
        }

        /// <summary>
        ///		The color SlateBlue.
        /// </summary>
        public static ColorEx SlateBlue {
            get {
                return new ColorEx(1f, 0.4156863f, 0.3529412f, 0.8039216f);
            }
        }

        /// <summary>
        ///		The color SlateGray.
        /// </summary>
        public static ColorEx SlateGray {
            get {
                return new ColorEx(1f, 0.4392157f, 0.5019608f, 0.5647059f);
            }
        }

        /// <summary>
        ///		The color Snow.
        /// </summary>
        public static ColorEx Snow {
            get {
                return new ColorEx(1f, 1f, 0.9803922f, 0.9803922f);
            }
        }

        /// <summary>
        ///		The color SpringGreen.
        /// </summary>
        public static ColorEx SpringGreen {
            get {
                return new ColorEx(1f, 0f, 1f, 0.4980392f);
            }
        }

        /// <summary>
        ///		The color SteelBlue.
        /// </summary>
        public static ColorEx SteelBlue {
            get {
                return new ColorEx(1f, 0.2745098f, 0.509804f, 0.7058824f);
            }
        }

        /// <summary>
        ///		The color Tan.
        /// </summary>
        public static ColorEx Tan {
            get {
                return new ColorEx(1f, 0.8235294f, 0.7058824f, 0.5490196f);
            }
        }

        /// <summary>
        ///		The color Teal.
        /// </summary>
        public static ColorEx Teal {
            get {
                return new ColorEx(1f, 0f, 0.5019608f, 0.5019608f);
            }
        }

        /// <summary>
        ///		The color Thistle.
        /// </summary>
        public static ColorEx Thistle {
            get {
                return new ColorEx(1f, 0.8470588f, 0.7490196f, 0.8470588f);
            }
        }

        /// <summary>
        ///		The color Tomato.
        /// </summary>
        public static ColorEx Tomato {
            get {
                return new ColorEx(1f, 1f, 0.3882353f, 0.2784314f);
            }
        }

        /// <summary>
        ///		The color Turquoise.
        /// </summary>
        public static ColorEx Turquoise {
            get {
                return new ColorEx(1f, 0.2509804f, 0.8784314f, 0.8156863f);
            }
        }

        /// <summary>
        ///		The color Violet.
        /// </summary>
        public static ColorEx Violet {
            get {
                return new ColorEx(1f, 0.9333333f, 0.509804f, 0.9333333f);
            }
        }

        /// <summary>
        ///		The color Wheat.
        /// </summary>
        public static ColorEx Wheat {
            get {
                return new ColorEx(1f, 0.9607843f, 0.8705882f, 0.7019608f);
            }
        }

        /// <summary>
        ///		The color White.
        /// </summary>
        public static ColorEx White {
            get {
                return new ColorEx(1f, 1f, 1f, 1f);
            }
        }

        /// <summary>
        ///		The color WhiteSmoke.
        /// </summary>
        public static ColorEx WhiteSmoke {
            get {
                return new ColorEx(1f, 0.9607843f, 0.9607843f, 0.9607843f);
            }
        }

        /// <summary>
        ///		The color Yellow.
        /// </summary>
        public static ColorEx Yellow {
            get {
                return new ColorEx(1f, 1f, 1f, 0f);
            }
        }

        /// <summary>
        ///		The color YellowGreen.
        /// </summary>
        public static ColorEx YellowGreen {
            get {
                return new ColorEx(1f, 0.6039216f, 0.8039216f, 0.1960784f);
            }
        }

        /// <summary>
        ///		The color with all elements zero.
        /// </summary>
        public static ColorEx Zero {
            get {
                return new ColorEx(0f, 0f, 0f, 0f);
            }
        }

        public static ColorEx Parse_0_255_String(string parsableText)
		{
			if(parsableText == null)
				throw new ArgumentException("The parsableText parameter cannot be null.");
			string[] vals = parsableText.TrimStart('(','[','<').TrimEnd(')',']','>').Split(',');
			if(vals.Length < 3)
				throw new FormatException(string.Format("Cannot parse the text '{0}' because it must of the form (r,g,b) or (r,g,b,a)",
														parsableText));
			float r, g, b, a;
			try
			{
				r = int.Parse(vals[0].Trim()) / 255f;
				g =	int.Parse(vals[1].Trim()) / 255f;
				b =	int.Parse(vals[2].Trim()) / 255f;
				if (vals.Length == 4)
					a =	int.Parse(vals[3].Trim()) / 255f;
				else
					a = 1.0f;
			}
			catch(Exception) 
			{
				throw new FormatException("The parts of the ColorEx in Parse_0_255 must be integers");
			}
			return new ColorEx(a, r, g, b);
		}

		public string To_0_255_String()
		{
			return string.Format("({0},{1},{2},{3})",
								 (int)(r * 255f),
								 (int)(g * 255f),
								 (int)(b * 255f),
								 (int)(a * 255f));
		}

        public override string ToString()
        {
            return string.Format("({0},{1},{2},{3})",r, g, b, a);
        }
		
		#endregion Static color properties

        #region Object overloads

        /// <summary>
        ///    Override GetHashCode.
        /// </summary>
        /// <remarks>
        ///    Done mainly to quash warnings, no real need for it.
        /// </remarks>
        /// <returns></returns>
        public override int GetHashCode() {
            return (int)this.ToARGB();
        }

        #endregion Object overloads

        #region IComparable Members

        /// <summary>
        ///    Used to compare 2 ColorEx objects for equality.
        /// </summary>
        /// <param name="obj">An instance of a ColorEx object to compare to this instance.</param>
        /// <returns>0 if they are equal, 1 if they are not.</returns>
        public int CompareTo(object obj) {
            ColorEx other = obj as ColorEx;

            if(this.a == other.a &&
                this.r == other.r &&
                this.g == other.g &&
                this.b == other.b) {

                return 0;
            }

            return 1;
        }

        #endregion
    }
}
