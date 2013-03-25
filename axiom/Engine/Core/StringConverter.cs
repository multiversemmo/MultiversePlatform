using System;
using System.Globalization;
using System.Text;
using Axiom.MathLib;

namespace Axiom.Core {
    /// <summary>
    ///     Helper class for going back and forth between strings and various types.
    /// </summary>
    public sealed class StringConverter {
        #region Fields

        /// <summary>
        ///		Culture info to use for parsing numeric data.
        /// </summary>
        private static CultureInfo englishCulture = new CultureInfo("en-US");

        #endregion Fields

        #region Constructor

        /// <summary>
        ///     Private constructor so no instances can be created.
        /// </summary>
        private StringConverter() {
        }

        #endregion Constructor

        #region Static Methods

        /// <summary>
        ///		Parses a boolean type value 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool ParseBool(string val) {
            switch (val) {
                case "true":
                case "on":
                    return true;
                case "false":
                case "off":
                    return false;
            }

            // make the compiler happy
            return false;
        }

        /// <summary>
        ///		Parses an array of params and returns a color from it.
        /// </summary>
        /// <param name="val"></param>
        public static ColorEx ParseColor(string[] values) {
            ColorEx color = new ColorEx();
            color.r = ParseFloat(values[0]);
            color.g = ParseFloat(values[1]);
            color.b = ParseFloat(values[2]);
            color.a = (values.Length > 3) ? ParseFloat(values[3]) : 1.0f;

            return color;
        }

        /// <summary>
        ///		Parses an array of params and returns a color from it.
        /// </summary>
        /// <param name="val"></param>
        public static ColorEx ParseColor(string val) {
            ColorEx color = new ColorEx();
            string[] vals = val.Split(' ');

            color.r = ParseFloat(vals[0]);
            color.g = ParseFloat(vals[1]);
            color.b = ParseFloat(vals[2]);
            color.a = (vals.Length == 4) ? ParseFloat(vals[3]) : 1.0f;

            return color;
        }

        /// <summary>
        ///		Parses an array of params and returns a color from it.
        /// </summary>
        /// <param name="val"></param>
        public static Vector3 ParseVector3(string[] values) {
            Vector3 vec = new Vector3();
            vec.x = ParseFloat(values[0]);
            vec.y = ParseFloat(values[1]);
            vec.z = ParseFloat(values[2]);

            return vec;
        }

        /// <summary>
        ///		Parses an array of params and returns a color from it.
        /// </summary>
        /// <param name="val"></param>
        public static Vector3 ParseVector3(string val) {
            string[] values = val.Split(' ');

            Vector3 vec = new Vector3();
            vec.x = ParseFloat(values[0]);
            vec.y = ParseFloat(values[1]);
            vec.z = ParseFloat(values[2]);

            return vec;
        }

        /// <summary>
        ///		Parses an array of params and returns a color from it.
        /// </summary>
        /// <param name="val"></param>
        public static Vector4 ParseVector4(string[] values) {
            Vector4 vec = new Vector4();
            vec.x = ParseFloat(values[0]);
            vec.y = ParseFloat(values[1]);
            vec.z = ParseFloat(values[2]);
            vec.w = ParseFloat(values[3]);

            return vec;
        }

        /// <summary>
        ///		Parse a float value from a string.
        /// </summary>
        /// <remarks>
        ///		Since our file formats assume the 'en-US' style format for numbers, we need to
        ///		let the framework know that where numbers are being parsed.
        /// </remarks>
        /// <param name="val">String value holding the float.</param>
        /// <returns>A float representation of the string value.</returns>
        public static float ParseFloat(string val) {
            return float.Parse(val, englishCulture);
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToString(ColorEx color) {
            return string.Format(englishCulture, "{0} {1} {2} {3}", color.r, color.g, color.b, color.a);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static string ToString(Vector4 vec) {
            return string.Format(englishCulture, "{0} {1} {2} {3}", vec.x, vec.y, vec.z, vec.w);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static string ToString(Vector3 vec) {
            return string.Format(englishCulture, "{0} {1} {2}", vec.x, vec.y, vec.z);
        }

        /// <summary>
        ///     Converts a 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ToString(float val) {
            return val.ToString(englishCulture);
        }

        #endregion Static Methods
    }
}
